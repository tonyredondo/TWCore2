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

using NATS.Client;
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

namespace TWCore.Messaging.NATS
{
    /// <inheritdoc />
    /// <summary>
    /// NATS Queue Client
    /// </summary>
    public class NATSQueueClient : MQueueClientBase
    {
        private static readonly ConcurrentDictionary<Guid, NATSQueueMessage> ReceivedMessages = new ConcurrentDictionary<Guid, NATSQueueMessage>();

        #region Fields
        private ConnectionFactory _factory;
        private List<(MQConnection, IConnection)> _senders;
        private IConnection _receiverNASTConnection;
        private IAsyncSubscription _receiver;
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
        private class NATSQueueMessage
        {
            public MultiArray<byte> Body;
            public readonly AsyncManualResetEvent WaitHandler = new AsyncManualResetEvent(false);
        }
        private static void MessageHandler(object sender, MsgHandlerEventArgs e)
        {
            (var body, var correlationId) = GetFromMessageBody(e.Message.Data);
            var rMsg = ReceivedMessages.GetOrAdd(correlationId, cId => new NATSQueueMessage());
            rMsg.Body = body;
            rMsg.WaitHandler.Set();
        }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// NATS Queue Client
        /// </summary>
        public NATSQueueClient()
        {
            _factory = new ConnectionFactory();
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
            _senders = new List<(MQConnection, IConnection)>();
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
                UseSingleResponseQueue = _receiverOptions?.Parameters?[ParameterKeys.SingleResponseQueue].ParseTo(true) ?? true;

                if (_clientQueues?.SendQueues?.Any() == true)
                {
                    foreach (var queue in _clientQueues.SendQueues)
                    {
                        Core.Log.LibVerbose("New Producer from QueueClient");
                        IConnection connection = null;
                        if (string.IsNullOrEmpty(queue.Route))
                            throw new UriFormatException($"The route for the connection to {queue.Name} is null.");
                        connection = Extensions.InvokeWithRetry(() => _factory.CreateConnection(queue.Route), 5000, int.MaxValue).WaitAsync();
                        _senders.Add((queue, connection));
                    }
                }
                if (_clientQueues?.RecvQueue != null && !SendOnly)
                {
                    _receiverConnection = _clientQueues.RecvQueue;
                    if (string.IsNullOrEmpty(_receiverConnection.Route))
                        throw new UriFormatException($"The route for the connection to {_receiverConnection.Name} is null.");
                    _receiverNASTConnection = Extensions.InvokeWithRetry(() => _factory.CreateConnection(_receiverConnection.Route), 5000, int.MaxValue).WaitAsync();
                    if (!UseSingleResponseQueue)
                    {
                        var rcvName = _receiverConnection.Name + "-" + Core.InstanceIdString;
                        Core.Log.InfoBasic("Using custom response queue: {0}", rcvName);
                        _receiver = _receiverNASTConnection.SubscribeAsync(rcvName, MessageHandler);
                    }
                    else
                    {
                        _receiver = _receiverNASTConnection.SubscribeAsync(_receiverConnection.Name, MessageHandler);
                    }
                }
            }

            Core.Status.Attach(collection =>
            {
                if (_senders != null)
                {
                    for (var i = 0; i < _senders.Count; i++)
                    {
                        collection.Add("Sender Path: {0}".ApplyFormat(i), _senders[i].Item1.Route);
                    }
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
                foreach (var sender in _senders)
                {
                    var conn = sender.Item2;
                    conn.Dispose();
                }
                _senders.Clear();
                _senders = null;
            }
            if (_receiver is null) return;
            _receiver.Unsubscribe();
            _receiverNASTConnection.Dispose();
            _receiver = null;
            _factory = null;
        }
        #endregion

        #region Send Method
        /// <inheritdoc />
        /// <summary>
        /// On Send message data
        /// </summary>
        /// <param name="message">Request message instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Task<bool> OnSendAsync(RequestMessage message)
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
                        message.Header.ResponseQueue.Name += "-" + Core.InstanceIdString;
                }
                else
                {
                    message.Header.ResponseExpected = false;
                    message.Header.ResponseTimeoutInSeconds = -1;
                }
            }
            message.Header.ContextGroupName = Core.ContextGroupName;
            var data = SenderSerializer.Serialize(message);
            var body = CreateMessageBody(data, message.CorrelationId);

            foreach ((var queue, var producer) in _senders)
            {
                Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}/{2}' with CorrelationId={3}", body.Length, queue.Route, queue.Name, message.Header.CorrelationId);
                producer.Publish(queue.Name, body);
            }
            Core.Log.LibVerbose("Message with CorrelationId={0} sent", message.Header.CorrelationId);
            Counters.IncrementTotalBytesSent(data.Count);
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
        protected override async Task<ResponseMessage> OnReceiveAsync(Guid correlationId, CancellationToken cancellationToken)
        {
            if (_receiver is null && UseSingleResponseQueue)
                throw new NullReferenceException("There is not receiver queue.");

            var sw = Stopwatch.StartNew();
            var message = ReceivedMessages.GetOrAdd(correlationId, cId => new NATSQueueMessage());
            try
            {
                if (!await message.WaitHandler.WaitAsync(_receiverOptionsTimeout, cancellationToken).ConfigureAwait(false))
                    throw new MessageQueueTimeoutException(_receiverOptionsTimeout, correlationId.ToString());

                if (message.Body == MultiArray<byte>.Empty)
                    throw new MessageQueueBodyNullException("The Message can't be retrieved, null body on CorrelationId = " + correlationId);

                Counters.IncrementTotalBytesReceived(message.Body.Count);
                Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}' with CorrelationId={2} at {3}ms", message.Body.Count, _clientQueues.RecvQueue.Name, correlationId, sw.Elapsed.TotalMilliseconds);
                var rs = ReceiverSerializer.Deserialize<ResponseMessage>(message.Body);
                return rs;
            }
            finally
            {
                ReceivedMessages.TryRemove(correlationId, out _);
            }
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
