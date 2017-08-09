/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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
using System.Threading;
using TWCore.Messaging.Client;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Exceptions;

namespace TWCore.Messaging.RabbitMQ
{
    /// <summary>
    /// RabbitMQ Queue Client
    /// </summary>
    public class RabbitMQueueClient : MQueueClientBase
    {
        static ConcurrentDictionary<Guid, RabbitResponseMessage> ReceivedMessages = new ConcurrentDictionary<Guid, RabbitResponseMessage>();

        #region Fields
        List<RabbitQueue> _senders;
        RabbitQueue _receiver;
        EventingBasicConsumer _receiverConsumer;
        string _receiverConsumerTag;
        MQClientQueues _clientQueues;
        MQClientSenderOptions _senderOptions;
        MQClientReceiverOptions _receiverOptions;
        #endregion

        #region Properties
        /// <summary>
        /// Use Single Response Queue
        /// </summary>
        public bool UseSingleResponseQueue { get; private set; }
        #endregion

        #region Nested Type
        class RabbitResponseMessage
        {
            public Guid CorrelationId;
            public IBasicProperties Properties;
            public byte[] Body;
            public readonly ManualResetEventSlim WaitHandler = new ManualResetEventSlim(false);
        }
        #endregion

        #region Init and Dispose Methods
        /// <summary>
        /// On client initialization
        /// </summary>
        protected override void OnInit()
        {
            OnDispose();
            _senders = new List<RabbitQueue>();
            _receiver = null;

            if (Config != null)
            {
                if (Config.ClientQueues != null)
                {
                    _clientQueues = Config.ClientQueues.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true)
                                    ?? Config.ClientQueues.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true)
                                    ?? Config.ClientQueues.FirstOrDefault(c => c.EnvironmentName.IsNullOrWhitespace());
                }
                _senderOptions = Config.RequestOptions?.ClientSenderOptions;
                _receiverOptions = Config.ResponseOptions?.ClientReceiverOptions;
                UseSingleResponseQueue = _receiverOptions.Parameters[ParameterKeys.SingleResponseQueue].ParseTo(false);

                if (_clientQueues?.SendQueues?.Any() == true)
                {
                    foreach (var queue in _clientQueues.SendQueues)
                    {
                        var rabbitQueue = new RabbitQueue(queue);
                        rabbitQueue.EnsureConnection(true);
                        _senders.Add(rabbitQueue);
                    }
                }
                if (_clientQueues?.RecvQueue != null)
                {
                    var rabbitQueue = new RabbitQueue(_clientQueues.RecvQueue);
                    _receiver = rabbitQueue;
                    if (UseSingleResponseQueue)
                    {
                        _receiver.EnsureConnection(true);
                        _receiverConsumer = new EventingBasicConsumer(_receiver.Channel);
                        _receiverConsumer.Received += (ch, ea) =>
                        {
                            var correlationId = Guid.Parse(ea.BasicProperties.CorrelationId);
                            var message = ReceivedMessages.GetOrAdd(correlationId, cId => new RabbitResponseMessage());
                            message.CorrelationId = correlationId;
                            message.Body = ea.Body;
                            message.Properties = ea.BasicProperties;
                            message.WaitHandler.Set();
                            _receiver.Channel.BasicAck(ea.DeliveryTag, false);
                        };
                        _receiverConsumerTag = _receiver.Channel.BasicConsume(_receiver.Name, false, _receiverConsumer);
                    }
                }
            }

            Core.Status.Attach(collection =>
            {
                if (_senders != null)
                    for (var i = 0; i < _senders.Count; i++)
                        collection.Add(nameof(_senders) + " {0} Path".ApplyFormat(i), _senders[i].Factory.HostName);
                if (_receiver != null)
                    collection.Add(nameof(_receiver) + " Path", _receiver.Factory.HostName);
            });
        }
        /// <summary>
        /// On Dispose
        /// </summary>
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
                _receiver.Channel.BasicCancel(_receiverConsumerTag);
                _receiver.Close();
                _receiver = null;
            }
        }
        #endregion

        #region Send Method
        /// <summary>
        /// On Send message data
        /// </summary>
        /// <param name="message">Request message instance</param>
        protected override bool OnSend(RequestMessage message)
        {
            if (_senders?.Any() != true)
                throw new NullReferenceException("There aren't any senders queues.");
            if (_senderOptions == null)
                throw new ArgumentNullException("SenderOptions");

            var recvQueue = _clientQueues.RecvQueue;
            if (message.Header.ResponseQueue == null)
            {
                if (recvQueue != null)
                {
                    message.Header.ResponseQueue = recvQueue;
                    message.Header.ResponseExpected = true;
                    message.Header.ResponseTimeoutInSeconds = _receiverOptions?.TimeoutInSec ?? -1;
                    if (!UseSingleResponseQueue)
                        message.Header.ResponseQueue.Name += "_" + message.CorrelationId;
                }
                else
                {
                    message.Header.ResponseExpected = false;
                    message.Header.ResponseTimeoutInSeconds = -1;
                }
            }

            var correlationId = message.CorrelationId.ToString();
            var data = SenderSerializer.Serialize(message);
            var replyTo = message.Header.ResponseQueue?.Name;
            var priority = (byte)(_senderOptions.MessagePriority == MQMessagePriority.High ? 9 :
                _senderOptions.MessagePriority == MQMessagePriority.Low ? 1 : 5);
            var expiration = (_senderOptions.MessageExpirationInSec * 1000).ToString();
            var deliveryMode = (byte)(_senderOptions.Recoverable ? 2 : 1);

            foreach (var sender in _senders)
            {
                if (!sender.EnsureConnection(true)) continue;
                var props = sender.Channel.CreateBasicProperties();
                props.CorrelationId = correlationId;
                props.ReplyTo = replyTo;
                props.Priority = priority;
                props.Expiration = expiration;
                props.AppId = Core.ApplicationName;
                props.ContentType = SenderSerializer.MimeTypes[0];
                props.DeliveryMode = deliveryMode;
                props.Type = _senderOptions.Label;
                Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", data.Count, sender.Route + "/" + sender.Name, message.Header.CorrelationId);
                sender.Channel.BasicPublish(null, sender.Name, props, (byte[])data);
            }
            return true;
        }
        #endregion

        #region Receive Method
        /// <summary>
        /// On Receive message data
        /// </summary>
        /// <param name="correlationId">Correlation Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response message instance</returns>
        protected override ResponseMessage OnReceive(Guid correlationId, CancellationToken cancellationToken)
        {
            if (_receiver == null)
                throw new NullReferenceException("There is not receiver queue.");
            if (_receiverOptions == null)
                throw new ArgumentNullException("SenderOptions");

            var timeout = TimeSpan.FromSeconds(_receiverOptions.TimeoutInSec);
            var sw = Stopwatch.StartNew();
            var message = ReceivedMessages.GetOrAdd(correlationId, cId => new RabbitResponseMessage());

            string temporalConsumerTag = null;
            if (!UseSingleResponseQueue)
            {
                _receiver.EnsureConnection(false);
                _receiver.Channel.QueueDeclare(_receiver.Name + "_" + correlationId, false, false, true, null);
                var tmpConsumer = new EventingBasicConsumer(_receiver.Channel);
                tmpConsumer.Received += (ch, ea) =>
                {
                    var crId = Guid.Parse(ea.BasicProperties.CorrelationId);
                    var rMessage = ReceivedMessages.GetOrAdd(crId, cId => new RabbitResponseMessage());
                    rMessage.CorrelationId = crId;
                    rMessage.Body = ea.Body;
                    rMessage.Properties = ea.BasicProperties;
                    rMessage.WaitHandler.Set();
                    _receiver.Channel.BasicAck(ea.DeliveryTag, false);
                };
                temporalConsumerTag = _receiver.Channel.BasicConsume(_receiver.Name + "_" + correlationId, false, tmpConsumer);
            }
            var waitResult = message.WaitHandler.Wait(timeout, cancellationToken);
            if (!UseSingleResponseQueue)
            {
                _receiver.Channel.BasicCancel(temporalConsumerTag);
                //_receiver.Close();
            }
            if (waitResult)
            {
                if (message.Body == null)
                    throw new MessageQueueNotFoundException("The Message can't be retrieved, null body on CorrelationId = " + correlationId.ToString());

                Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}' with CorrelationId={2}", message.Body.Length, _clientQueues.RecvQueue.Name, correlationId);
                var response = ReceiverSerializer.Deserialize<ResponseMessage>(message.Body);
                sw.Stop();
                sw = null;
                Core.Log.LibVerbose("Correlation Message ({0}) received at: {1}ms", correlationId, sw.Elapsed.TotalMilliseconds);
                return response;

            }
            else
                throw new MessageQueueTimeoutException(timeout, correlationId.ToString());
        }
        #endregion
    }
}