/*
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
            private static readonly ObjectPool<RabbitResponseMessage> Pool = new ObjectPool<RabbitResponseMessage>(_ => new RabbitResponseMessage());
            public IBasicProperties Properties;
            public byte[] Body;
            public readonly AsyncManualResetEvent WaitHandler = new AsyncManualResetEvent(false);

            private RabbitResponseMessage() { }

            //
            public static RabbitResponseMessage Rent()
            {
                return Pool.New();
            }
            public static void Free(RabbitResponseMessage item)
            {
                item.Properties = null;
                item.Body = null;
                item.WaitHandler.Reset();
                Pool.Store(item);
            }
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
                            rabbitQueue.EnsureConnectionAsync(2000, int.MaxValue).WaitAndResults();
                            rabbitQueue.EnsureExchange();
                            _senders.Add(rabbitQueue);
                        }
                    }
                    if (_clientQueues.RecvQueue != null && !SendOnly)
                        _receiver = new RabbitMQueue(_clientQueues.RecvQueue);
                }

                if (_senderOptions is null) throw new Exception("Client Sender Options is Null.");

                _priority = (byte)(_senderOptions.MessagePriority == MQMessagePriority.High ? 9 :
                                   _senderOptions.MessagePriority == MQMessagePriority.Low ? 1 : 5);
                _expiration = (_senderOptions.MessageExpirationInSec * 1000).ToString();
                _deliveryMode = (byte)(_senderOptions.Recoverable ? 2 : 1);

                CreateReceiverConsumerAsync().WaitAsync();
            }

            Core.Status.Attach(collection =>
            {
                if (_senders != null)
                    for (var i = 0; i < _senders.Count; i++)
                    {
                        collection.Add("Sender: {0} ".ApplyFormat(i), _senders[i].Route + " - " + _senders[i].Name);
                    }
                if (_receiver != null)
                    collection.Add("Receiver: {0}", _receiver.Route + " - " + _receiver.Name);
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
        protected override async Task<bool> OnSendAsync(byte[] message, Guid correlationId)
        {
            if (_senders?.Any() != true)
                throw new NullReferenceException("There aren't any senders queues.");
            if (_senderOptions is null)
                throw new NullReferenceException("SenderOptions is null.");

            var recvQueue = _clientQueues.RecvQueue;
            var corrId = correlationId.ToString();
            var replyTo = recvQueue?.Name;
            if (replyTo != null && !UseSingleResponseQueue)
                replyTo += "-" + Core.InstanceIdString;

            if (_senders.Count == 1)
                return await SendTaskAsync(_senders[0]).ConfigureAwait(false);

            var tsk = new Task[_senders.Count];
            for (var i = 0; i < tsk.Length; i++)
                tsk[i] = SendTaskAsync(_senders[i]);
            await Task.WhenAny(tsk).ConfigureAwait(false);
            return true;

            async Task<bool> SendTaskAsync(RabbitMQueue sender)
            {
                try
                {
                    if (await sender.EnsureConnectionAsync(1000, 2).ConfigureAwait(false))
                    {
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
                        props.Type = _senderOptions.Label ?? string.Empty;
                        Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}/{2}' with CorrelationId={3}", message.Length, sender.Route, sender.Name, correlationId);
                        sender.Channel.BasicPublish(sender.ExchangeName ?? string.Empty, sender.Name, props, message);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
                return false;
            }
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
            var message = ReceivedMessages.GetOrAdd(correlationId, _ => RabbitResponseMessage.Rent());
            Interlocked.Increment(ref _receiverThreads);
            await CreateReceiverConsumerAsync().ConfigureAwait(false);

            if (!await message.WaitHandler.WaitAsync(_receiverOptionsTimeout, cancellationToken).ConfigureAwait(false))
                throw new MessageQueueTimeoutException(_receiverOptionsTimeout, correlationId.ToString());
            if (message.Body is null)
                throw new MessageQueueNotFoundException("The Message can't be retrieved, null body on CorrelationId = " + correlationId);

            Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}' with CorrelationId={2} at {3}ms", message.Body.Length, _clientQueues.RecvQueue.Name, correlationId, sw.Elapsed.TotalMilliseconds);

            if (Interlocked.Decrement(ref _receiverThreads) <= 0)
                _receiverStopBuffered();

            var body = message.Body;
            RabbitResponseMessage.Free(message);
            return body;
        }
        #endregion

        #region Private Methods
        private AsyncLock _locker = new AsyncLock();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask CreateReceiverConsumerAsync()
        {
            if (_receiverConsumer != null) return;
            if (_receiver is null) return;
            using (await _locker.LockAsync().ConfigureAwait(false))
            {
                if (_receiverConsumer != null) return;
                if (_receiver is null) return;
                await _receiver.EnsureConnectionAsync(5000, int.MaxValue).ConfigureAwait(false);
                _receiver.EnsureQueue();
                _receiverConsumer = new EventingBasicConsumer(_receiver.Channel);
                _receiverConsumer.Received += MessageReceivedHandler;
                var rcvName = _receiver.Name;
                if (!UseSingleResponseQueue)
                {
                    rcvName += "-" + Core.InstanceIdString;
                    _receiver.Channel.QueueDeclare(rcvName, false, false, true, null);
                }
                _receiverConsumerTag = _receiver.Channel.BasicConsume(rcvName, false, _receiverConsumer);
                Core.Log.LibVerbose("The Receiver for the queue \"{0}\" has been created.", rcvName);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MessageReceivedHandler(object sender, BasicDeliverEventArgs ea)
        {
            var correlationId = Guid.Parse(ea.BasicProperties.CorrelationId);
            if (!ReceivedMessages.TryRemove(correlationId, out var message))
            {
                _receiver.Channel.BasicNack(ea.DeliveryTag, false, true);
                return;
            }
#if NETCOREAPP3_1_OR_GREATER
            message.Body = ea.Body.ToArray();
#else
            message.Body = ea.Body;
#endif
            message.Properties = ea.BasicProperties;
            message.WaitHandler.Set();
            _receiver.Channel.BasicAck(ea.DeliveryTag, false);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            var rcvName = _receiver.Name;
            if (!UseSingleResponseQueue)
                rcvName += "-" + Core.InstanceIdString;
            Core.Log.LibVerbose("The Receiver for the queue \"{0}\" has been disposed.", rcvName);
        }
        #endregion
    }
}