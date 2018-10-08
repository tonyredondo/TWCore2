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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Exceptions;
using TWCore.Messaging.RawClient;
using TWCore.Threading;

// ReSharper disable NotAccessedField.Local
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.NATS
{
    /// <inheritdoc />
    /// <summary>
    /// NATS Queue Raw Client
    /// </summary>
    public class NATSQueueRawClient : MQueueRawClientBase
    {
        private static readonly ConcurrentDictionary<Guid, NATSQueueMessage> ReceivedMessages = new ConcurrentDictionary<Guid, NATSQueueMessage>();
        private static readonly UTF8Encoding Encoding = new UTF8Encoding(false);

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
            (var body, var correlationId, var _) = GetFromRawMessageBody(e.Message.Data, false);
            var rMsg = ReceivedMessages.GetOrAdd(correlationId, cId => new NATSQueueMessage());
            rMsg.Body = body;
            rMsg.WaitHandler.Set();
        }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// NATS Queue Raw Client
        /// </summary>
        public NATSQueueRawClient()
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
                if (_clientQueues?.RecvQueue != null)
                {
                    _receiverConnection = _clientQueues.RecvQueue;
                    if (string.IsNullOrEmpty(_receiverConnection.Route))
                        throw new UriFormatException($"The route for the connection to {_receiverConnection.Name} is null.");
                    _receiverNASTConnection = Extensions.InvokeWithRetry(() => _factory.CreateConnection(_receiverConnection.Route), 5000, int.MaxValue).WaitAsync();
                    if (!UseSingleResponseQueue)
                    {
                        var rcvName = _receiverConnection.Name + "-" + Core.InstanceId;
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
                    for (var i = 0; i < _senders.Count; i++)
                        collection.Add("Sender Path: {0}".ApplyFormat(i), _senders[i].Item1.Route);
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
        /// <param name="correlationId">Message CorrelationId</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Task<bool> OnSendAsync(byte[] message, Guid correlationId)
        {
            if (_senders?.Any() != true)
                throw new NullReferenceException("There aren't any senders queues.");
            if (_senderOptions is null)
                throw new NullReferenceException("SenderOptions is null.");

            var recvQueue = _clientQueues.RecvQueue;
            var name = recvQueue.Name;
            if (!UseSingleResponseQueue)
                name += "-" + Core.InstanceId;
            var body = CreateRawMessageBody(message, correlationId, name);

            foreach ((var queue, var producer) in _senders)
            {
                Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}/{2}' with CorrelationId={3}", body.Length, queue.Route, queue.Name, correlationId);
                producer.Publish(queue.Name, body);
            }
            Core.Log.LibVerbose("Message with CorrelationId={0} sent", correlationId);
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
            if (_receiver is null && UseSingleResponseQueue)
                throw new NullReferenceException("There is not receiver queue.");

            var sw = Stopwatch.StartNew();
            var message = ReceivedMessages.GetOrAdd(correlationId, cId => new NATSQueueMessage());
            try
            {
                if (!await message.WaitHandler.WaitAsync(_receiverOptionsTimeout, cancellationToken).ConfigureAwait(false))
                    throw new MessageQueueTimeoutException(_receiverOptionsTimeout, correlationId.ToString());

                Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}' with CorrelationId={2}", message.Body.Count, _clientQueues.RecvQueue.Name, correlationId);
                Core.Log.LibVerbose("Correlation Message ({0}) received at: {1}ms", correlationId, sw.Elapsed.TotalMilliseconds);
                sw.Stop();
                return message.Body.AsArray();
            }
            finally
            {
                ReceivedMessages.TryRemove(correlationId, out _);
            }
        }
        #endregion

        #region Static Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte[] CreateRawMessageBody(MultiArray<byte> message, Guid correlationId, string name)
        {
            var nameLength = Encoding.GetByteCount(name);
            var body = new byte[16 + 4 + nameLength + message.Count];
            var bodySpan = body.AsSpan();
#if COMPATIBILITY
            correlationId.ToByteArray().CopyTo(body, 0);
            MemoryMarshal.Write(bodySpan.Slice(16, 4), ref nameLength);
            Encoding.GetBytes(name).CopyTo(bodySpan.Slice(20, nameLength));
#else
            correlationId.TryWriteBytes(bodySpan.Slice(0, 16));
            BitConverter.TryWriteBytes(bodySpan.Slice(16, 4), nameLength);
            Encoding.GetBytes(name, bodySpan.Slice(20, nameLength));
#endif
            message.CopyTo(body, 20 + nameLength);
            return body;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (MultiArray<byte>, Guid, string) GetFromRawMessageBody(byte[] message, bool returnName = true)
        {
            var body = new MultiArray<byte>(message);
#if COMPATIBILITY
            var correlationId = new Guid(body.Slice(0, 16).ToArray());
#else
            var correlationId = new Guid(body.Slice(0, 16).AsSpan());
#endif
            var nameLength = BitConverter.ToInt32(message, 16);
            var messageBody = body.Slice(20 + nameLength);
            if (!returnName)
                return (messageBody, correlationId, null);

            var name = Encoding.GetString(message, 20, nameLength);
            return (messageBody, correlationId, name);
        }
        #endregion
    }
}
