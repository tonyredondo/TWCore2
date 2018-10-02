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

using NsqSharp;
using NsqSharp.Api;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Messaging.Client;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Exceptions;
using TWCore.Threading;

// ReSharper disable NotAccessedField.Local
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.NSQ
{
    /// <inheritdoc />
    /// <summary>
    /// NSQ Queue Client
    /// </summary>
    public class NSQueueClient : MQueueClientBase
    {
        private static readonly ConcurrentDictionary<Guid, NSQueueMessage> ReceivedMessages = new ConcurrentDictionary<Guid, NSQueueMessage>();
        private static readonly NSQMessageHandler MessageHandler = new NSQMessageHandler();

        #region Fields
        private List<(MQConnection, Producer)> _senders;
        private Consumer _receiver;
        private MQConnection _receiverConnection;
        private MQClientQueues _clientQueues;
        private MQClientSenderOptions _senderOptions;
        private MQClientReceiverOptions _receiverOptions;
        private TimeSpan _receiverOptionsTimeout;
        #endregion

        #region Properties
        /// <summary>
        /// Use Single Response Queue
        /// </summary>
        [StatusProperty]
        public bool UseSingleResponseQueue { get; private set; }
        #endregion

        #region Nested Type
        private class NSQueueMessage
        {
            public Guid CorrelationId;
            public MultiArray<byte> Body;
            public readonly AsyncManualResetEvent WaitHandler = new AsyncManualResetEvent(false);
            public Consumer Consumer;
            public string Route;
            public string Name;
        }
        private class NSQMessageHandler : IHandler
        {
            public void HandleMessage(NsqSharp.IMessage message)
            {
                (var body, var correlationId) = GetFromMessageBody(message.Body);
                Try.Do(message.Finish, false);
                var rMsg = ReceivedMessages.GetOrAdd(correlationId, cId => new NSQueueMessage());
                rMsg.CorrelationId = correlationId;
                rMsg.Body = body;
                rMsg.WaitHandler.Set();
            }
            public void LogFailedMessage(NsqSharp.IMessage message)
            {
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
            _senders = new List<(MQConnection, Producer)>();
            _receiver = null;


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
                UseSingleResponseQueue = _receiverOptions?.Parameters?[ParameterKeys.SingleResponseQueue].ParseTo(false) ?? false;

                if (_clientQueues?.SendQueues?.Any() == true)
                {
                    foreach (var queue in _clientQueues.SendQueues)
                    {
                        Core.Log.LibVerbose("New Producer from QueueClient");
                        _senders.Add((queue, new Producer(queue.Route)));
                    }
                }
                if (_clientQueues?.RecvQueue != null)
                {
                    _receiverConnection = _clientQueues.RecvQueue;
                    _receiver = new Consumer(_receiverConnection.Name, _receiverConnection.Name);
                    if (UseSingleResponseQueue)
                    {
                        _receiver.AddHandler(MessageHandler);
                        if (string.IsNullOrEmpty(_receiverConnection.Route))
                            throw new UriFormatException($"The route for the connection to {_receiverConnection.Name} is null.");
                        Extensions.InvokeWithRetry(() =>
                        {
                            _receiver.ConnectToNsqd(_receiverConnection.Route);
                        }, 5000, int.MaxValue).WaitAsync();
                    }
                }
            }

            Core.Status.Attach(collection =>
            {
                if (_senders != null)
                    for (var i = 0; i < _senders.Count; i++)
                    {
                        collection.Add("Sender Path: {0}".ApplyFormat(i), _senders[i].Item1.Route);
                    }
                if (_clientQueues?.RecvQueue != null)
                    collection.Add("Receiver Path", _clientQueues.RecvQueue.Route);
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
                var producers = _senders.Select(i => i.Item2).ToArray();
                Parallel.ForEach(producers, p => p.Stop());
                _senders.Clear();
                _senders = null;
            }
            if (_receiver is null) return;
            if (UseSingleResponseQueue)
                _receiver.Stop();
            _receiver = null;
        }
        #endregion

        #region Send Method
        /// <inheritdoc />
        /// <summary>
        /// On Send message data
        /// </summary>
        /// <param name="message">Request message instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override async Task<bool> OnSendAsync(RequestMessage message)
        {
            if (_senders?.Any() != true)
                throw new NullReferenceException("There aren't any senders queues.");
            if (_senderOptions is null)
                throw new NullReferenceException("SenderOptions is null.");

            if (message.Header.ResponseQueue is null)
            {
                var recvQueue = _clientQueues.RecvQueue;
                if (recvQueue != null)
                {
                    message.Header.ResponseQueue = new MQConnection(recvQueue.Route, recvQueue.Name) { Parameters = recvQueue.Parameters };
                    message.Header.ResponseExpected = true;
                    message.Header.ResponseTimeoutInSeconds = _receiverOptions?.TimeoutInSec ?? -1;
                    if (!UseSingleResponseQueue)
                    {
                        message.Header.ResponseQueue.Name += "_" + message.CorrelationId;
                    }
                }
                else
                {
                    message.Header.ResponseExpected = false;
                    message.Header.ResponseTimeoutInSeconds = -1;
                }
            }
            var data = SenderSerializer.Serialize(message);
            var body = CreateMessageBody(data, message.CorrelationId);

            foreach ((var queue, var nsqProducer) in _senders)
            {
                Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", body.Length, queue.Route + "/" + queue.Name, message.Header.CorrelationId);
                await nsqProducer.PublishAsync(queue.Name, body).ConfigureAwait(false);
            }
            Core.Log.LibVerbose("Message with CorrelationId={0} sent", message.Header.CorrelationId);
            return true;
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
        protected override async Task<ResponseMessage> OnReceiveAsync(Guid correlationId, CancellationToken cancellationToken)
        {
            if (_receiver is null)
                throw new NullReferenceException("There is not receiver queue.");

            var sw = Stopwatch.StartNew();
            var message = ReceivedMessages.GetOrAdd(correlationId, cId => new NSQueueMessage());

            if (!UseSingleResponseQueue)
            {
                message.Name = _receiverConnection.Name + "_" + correlationId;
                message.Route = _receiverConnection.Route;
                message.Consumer = new Consumer(message.Name, message.Name);
                message.Consumer.AddHandler(MessageHandler);
                message.Consumer.ConnectToNsqd(message.Route);

                var waitResult = await message.WaitHandler.WaitAsync(_receiverOptionsTimeout, cancellationToken).ConfigureAwait(false);

                message.Consumer.Stop();
                message.Consumer.DisconnectFromNsqd(message.Route);
                message.Consumer = null;
                var pro = new NsqdHttpClient(message.Route.Replace(":4150", ":4151"), TimeSpan.FromSeconds(60));
                pro.DeleteChannel(message.Name, message.Name);
                pro.DeleteTopic(message.Name);

                if (!waitResult) throw new MessageQueueTimeoutException(_receiverOptionsTimeout, correlationId.ToString());

                if (message.Body == MultiArray<byte>.Empty)
                    throw new MessageQueueNotFoundException("The Message can't be retrieved, null body on CorrelationId = " + correlationId);

                Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}' with CorrelationId={2}", message.Body.Count, _clientQueues.RecvQueue.Name, correlationId);
                var response = ReceiverSerializer.Deserialize<ResponseMessage>(message.Body);
                Core.Log.LibVerbose("Correlation Message ({0}) received at: {1}ms", correlationId, sw.Elapsed.TotalMilliseconds);
                sw.Stop();
                return response;
            }

            if (!await message.WaitHandler.WaitAsync(_receiverOptionsTimeout, cancellationToken).ConfigureAwait(false))
                throw new MessageQueueTimeoutException(_receiverOptionsTimeout, correlationId.ToString());

            if (message.Body == MultiArray<byte>.Empty)
                throw new MessageQueueBodyNullException("The Message can't be retrieved, null body on CorrelationId = " + correlationId);

            Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}' with CorrelationId={2}", message.Body.Count, _clientQueues.RecvQueue.Name, correlationId);
            var rs = ReceiverSerializer.Deserialize<ResponseMessage>(message.Body);
            ReceivedMessages.TryRemove(correlationId, out _);
            Core.Log.LibVerbose("Correlation Message ({0}) received at: {1}ms", correlationId, sw.Elapsed.TotalMilliseconds);
            sw.Stop();
            return rs;
        }
        #endregion

        #region Static Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte[] CreateMessageBody(MultiArray<byte> message, Guid correlationId)
        {
            var body = new byte[16 + message.Count];
#if COMPATIBILITY
            correlationId.ToByteArray().CopyTo(body, 0);
#else
            correlationId.TryWriteBytes(body.AsSpan(0, 16));
#endif
            message.CopyTo(body, 16);
            return body;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (MultiArray<byte>, Guid) GetFromMessageBody(byte[] message)
        {
            var body = new MultiArray<byte>(message);
#if COMPATIBILITY
            var correlationId = new Guid(body.Slice(0, 16).ToArray());
#else
            var correlationId = new Guid(body.Slice(0, 16).AsSpan());
#endif
            var messageBody = body.Slice(16);
            return (messageBody, correlationId);
        }
        #endregion
    }
}
