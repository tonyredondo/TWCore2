﻿/*
Copyright 2015-2018 Daniel Adrian Redondo Suarez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Exceptions;
using TWCore.Messaging.RawClient;
using TWCore.Threading;

// ReSharper disable NotAccessedField.Local
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Messaging.RabbitMQ
{
    /// <inheritdoc />
    /// <summary>
    /// RabbitMQ Queue Raw Client
    /// </summary>
    public class RabbitMQueueRawClient : MQueueRawClientBase
    {
        private static readonly ConcurrentDictionary<Guid, RabbitResponseMessage> ReceivedMessages = new ConcurrentDictionary<Guid, RabbitResponseMessage>();
        private readonly ConcurrentDictionary<string, ObjectPool<RabbitMQueue>> _routeConnection = new ConcurrentDictionary<string, ObjectPool<RabbitMQueue>>();
        private readonly ConcurrentDictionary<Guid, string> _correlationIdConsumers = new ConcurrentDictionary<Guid, string>();

        #region Fields
        private List<RabbitMQueue> _senders;
        private RabbitMQueue _receiver;
        private EventingBasicConsumer _receiverConsumer;
        private string _receiverConsumerTag;
        private MQClientQueues _clientQueues;
        private MQClientSenderOptions _senderOptions;
        private MQClientReceiverOptions _receiverOptions;
        private long _receiverThreads;
        private Action _receiverStopBuffered;
        private TimeSpan _receiverOptionsTimeout;
		private byte _priority;
		private byte _deliveryMode;
		private string _expiration;
        #endregion

        #region Properties
        /// <summary>
        /// Use Single Response Queue
        /// </summary>
        [StatusProperty]
        public bool UseSingleResponseQueue { get; private set; }
        #endregion

        #region Nested Type
        private class RabbitResponseMessage
        {
            public IBasicProperties Properties;
            public byte[] Body;
            public readonly AsyncManualResetEvent WaitHandler = new AsyncManualResetEvent(false);
        }
        #endregion

        #region Init and Dispose Methods
        /// <inheritdoc />
        /// <summary>
        /// On client initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnInit()
        {
            OnDispose();
            _senders = new List<RabbitMQueue>();
            _receiver = null;
            _receiverStopBuffered = ActionDelegate.Create(RemoveReceiverConsumer).CreateBufferedAction(2000);

            if (Config != null)
            {
                if (Config.ClientQueues != null)
                {
                    _clientQueues = Config.ClientQueues.FirstOf(
                        c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true,
                        c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true,
                        c => c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true,
                        c => c.EnvironmentName.IsNullOrWhitespace());
                }
                _senderOptions = Config.RequestOptions?.ClientSenderOptions;
                _receiverOptions = Config.ResponseOptions?.ClientReceiverOptions;
                _receiverOptionsTimeout = TimeSpan.FromSeconds(_receiverOptions?.TimeoutInSec ?? 20);
                UseSingleResponseQueue = _receiverOptions?.Parameters?[ParameterKeys.SingleResponseQueue].ParseTo(true) ?? true;

                if (_clientQueues != null)
                {
                    if (_clientQueues.SendQueues?.Any() == true)
                    {
                        foreach (var queue in _clientQueues.SendQueues)
                        {
                            var rabbitQueue = new RabbitMQueue(queue);
                            rabbitQueue.EnsureConnection();
                            rabbitQueue.EnsureExchange();
                            _senders.Add(rabbitQueue);
                        }
                    }
                    if (_clientQueues.RecvQueue != null)
                        _receiver = new RabbitMQueue(_clientQueues.RecvQueue);
                }

                if (_senderOptions is null) throw new Exception("Client Sender Options is Null.");

				_priority = (byte)(_senderOptions.MessagePriority == MQMessagePriority.High ? 9 :
								   _senderOptions.MessagePriority == MQMessagePriority.Low ? 1 : 5);
				_expiration = (_senderOptions.MessageExpirationInSec * 1000).ToString();
				_deliveryMode = (byte)(_senderOptions.Recoverable ? 2 : 1);
            }

            Core.Status.Attach(collection =>
            {
                if (_senders != null)
                    for (var i = 0; i < _senders.Count; i++)
                    {
                        if (_senders[i]?.Factory is null) continue;
                        collection.Add("Sender Path: {0}".ApplyFormat(i), _senders[i].Factory.HostName);
                    }
                if (_receiver?.Factory != null)
                    collection.Add("Receiver Path", _receiver.Factory.HostName);
            });
        }
        /// <inheritdoc />
        /// <summary>
        /// On Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnDispose()
        {
            if (_senders != null)
            {
                foreach (var sender in _senders)
                {
                    sender.Close();
                }
                _senders.Clear();
                _senders = null;
            }
            if (_receiver != null)
            {
                RemoveReceiverConsumer();
                _receiver = null;
            }
            foreach (var pools in _routeConnection)
            {
                var pool = pools.Value;
                var connections = pool.GetCurrentObjects();
                foreach (var connection in connections)
                    connection.Close();
                pool.Clear();
            }
            _routeConnection.Clear();
        }
        #endregion

        #region Send Method
        /// <inheritdoc />
        /// <summary>
        /// On Send message data
        /// </summary>
        /// <param name="message">Request message instance</param>
        /// <param name="correlationId">Message CorrelationId</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Task<bool> OnSendAsync(byte[] message, Guid correlationId)
        {
            if (_senders?.Any() != true)
                throw new NullReferenceException("There aren't any senders queues.");
            if (_senderOptions is null)
                throw new NullReferenceException("SenderOptions is null.");

            var recvQueue = _clientQueues.RecvQueue;
            var corrId = correlationId.ToString();
            var replyTo = recvQueue.Name;

            if (!UseSingleResponseQueue)
            {
                replyTo = recvQueue.Name + "_" + correlationId;
                var pool = _routeConnection.GetOrAdd(_receiver.Route, r => new ObjectPool<RabbitMQueue>(p => new RabbitMQueue(_receiver)));
                var cReceiver = pool.New();
                cReceiver.EnsureConnection();
                cReceiver.Channel.QueueDeclare(replyTo, false, false, true, null);
                pool.Store(cReceiver);
            }

            foreach (var sender in _senders)
            {
                if (!sender.EnsureConnection()) continue;
                sender.EnsureExchange();
                var props = sender.Channel.CreateBasicProperties();
                props.CorrelationId = corrId;
                if (replyTo != null)
                    props.ReplyTo = replyTo;
                props.Priority = _priority;
                props.Expiration = _expiration;
                props.AppId = Core.ApplicationName;
                props.ContentType = SenderSerializer.MimeTypes[0];
                props.DeliveryMode = _deliveryMode;
                props.Type = _senderOptions.Label;
                Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", message.Length, sender.Route + "/" + sender.Name, correlationId);
                sender.Channel.BasicPublish(sender.ExchangeName ?? string.Empty, sender.Name, props, message);
            }
            return TaskHelper.CompleteTrue;
        }
        #endregion

        #region Receive Method
        /// <inheritdoc />
        /// <summary>
        /// On Receive message data
        /// </summary>
        /// <param name="correlationId">Correlation Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response message instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override async Task<byte[]> OnReceiveAsync(Guid correlationId, CancellationToken cancellationToken)
        {
            if (_receiver is null)
                throw new NullReferenceException("There is not receiver queue.");

            var sw = Stopwatch.StartNew();
            var message = ReceivedMessages.GetOrAdd(correlationId, _ => new RabbitResponseMessage());
            Interlocked.Increment(ref _receiverThreads);

            if (UseSingleResponseQueue)
            {
                CreateReceiverConsumer();
            }
            else
            {
                var recName = _receiver.Name + "_" + correlationId;
                var pool = _routeConnection.GetOrAdd(_receiver.Route, r => new ObjectPool<RabbitMQueue>(p => new RabbitMQueue(_receiver)));
                var cReceiver = pool.New();
                cReceiver.EnsureConnection();
                cReceiver.Channel.QueueDeclare(recName, false, false, true, null);
                var tmpConsumer = new EventingBasicConsumer(cReceiver.Channel);
                tmpConsumer.Received += (ch, ea) =>
                {
                    var crId = Guid.Parse(ea.BasicProperties.CorrelationId);

                    if (!ReceivedMessages.TryRemove(crId, out var rMessage))
                    {
                        _receiver.Channel.BasicNack(ea.DeliveryTag, false, true);
                        return;
                    }

                    rMessage.Body = ea.Body;
                    rMessage.Properties = ea.BasicProperties;
                    rMessage.WaitHandler.Set();
                    cReceiver.Channel.BasicAck(ea.DeliveryTag, false);
                    if (_correlationIdConsumers.TryGetValue(crId, out var consumerTag))
                        cReceiver.Channel.BasicCancel(consumerTag);
                    cReceiver.Channel.QueueDeleteNoWait(recName);
                    cReceiver.AutoClose();
                    pool.Store(cReceiver);
                };
                _correlationIdConsumers.TryAdd(correlationId, cReceiver.Channel.BasicConsume(recName, false, tmpConsumer));
            }

            if (!await message.WaitHandler.WaitAsync(_receiverOptionsTimeout, cancellationToken).ConfigureAwait(false))
                throw new MessageQueueTimeoutException(_receiverOptionsTimeout, correlationId.ToString());
            if (message.Body is null)
                throw new MessageQueueNotFoundException("The Message can't be retrieved, null body on CorrelationId = " + correlationId);

            Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}' with CorrelationId={2}", message.Body.Length, _clientQueues.RecvQueue.Name, correlationId);
            Core.Log.LibVerbose("Correlation Message ({0}) received at: {1}ms", correlationId, sw.Elapsed.TotalMilliseconds);
            sw.Stop();

            if (Interlocked.Decrement(ref _receiverThreads) <= 0)
                _receiverStopBuffered();

            return message.Body;
        }
        #endregion

        #region Private Methods
        private void CreateReceiverConsumer()
        {
            if (_receiverConsumer != null) return;
            if (_receiver is null) return;
            lock (_receiver)
            {
                if (_receiverConsumer != null) return;
                if (_receiver is null) return;
                _receiver.EnsureConnection();
                _receiver.EnsureQueue();
                _receiverConsumer = new EventingBasicConsumer(_receiver.Channel);
                _receiverConsumer.Received += (ch, ea) =>
                {
                    var correlationId = Guid.Parse(ea.BasicProperties.CorrelationId);

                    if (!ReceivedMessages.TryRemove(correlationId, out var message))
                    {
                        _receiver.Channel.BasicNack(ea.DeliveryTag, false, true);
                        return;
                    }

                    message.Body = ea.Body;
                    message.Properties = ea.BasicProperties;
                    message.WaitHandler.Set();
                    _receiver.Channel.BasicAck(ea.DeliveryTag, false);
                };
                _receiverConsumerTag = _receiver.Channel.BasicConsume(_receiver.Name, false, _receiverConsumer);
                Core.Log.LibVerbose("The Receiver for the queue \"{0}\" has been created.", _receiver.Name);
            }
        }
        private void RemoveReceiverConsumer()
        {
            if (Interlocked.Read(ref _receiverThreads) > 0) return;
            if (_receiver is null) return;
            if (!string.IsNullOrEmpty(_receiverConsumerTag))
            {
                _receiver.Channel.BasicCancel(_receiverConsumerTag);
                _receiverConsumerTag = null;
            }
            _receiverConsumer = null;
            _receiver.Close();
            Core.Log.LibVerbose("The Receiver for the queue \"{0}\" has been disposed.", _receiver.Name);
        }
        #endregion
    }
}