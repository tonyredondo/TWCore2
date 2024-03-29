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

using StackExchange.Redis;
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

namespace TWCore.Messaging.Redis
{
    /// <inheritdoc />
    /// <summary>
    /// Redis Queue Client
    /// </summary>
    public class RedisQueueClient : MQueueClientBase
    {
        private static readonly ConcurrentDictionary<Guid, Message> ReceivedMessages = new ConcurrentDictionary<Guid, Message>();

        #region Fields
        private List<RedisMQConnection> _senders;
        private RedisMQConnection _receiverConnection;
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
        private class Message
        {
            private static readonly ObjectPool<Message> Pool = new ObjectPool<Message>(_ => new Message());

            public MultiArray<byte> Body;
            public readonly AsyncManualResetEvent WaitHandler = new AsyncManualResetEvent(false);


            private Message() { }

            //
            public static Message Rent()
            {
                return Pool.New();
            }
            public static void Free(Message item)
            {
                item.Body = MultiArray<byte>.Empty;
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
            _senders = new List<RedisMQConnection>();

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
                        var redisConnection = new RedisMQConnection(queue);
                        _senders.Add(redisConnection);
                    }
                }
                if (_clientQueues?.RecvQueue != null && !SendOnly)
                {
                    _receiverConnection = new RedisMQConnection(_clientQueues.RecvQueue);
                    if (!UseSingleResponseQueue)
                    {
                        _receiverConnection.Name += "-" + Core.InstanceIdString;
                        Core.Log.InfoBasic("Using custom response queue: {0}", _receiverConnection.Name);
                    }
                    _receiverConnection.SubscribeAsync(new Action<RedisChannel, RedisValue>(ProcessReceivedMessage)).WaitAsync();
                }
            }

            Core.Status.Attach(collection =>
            {
                if (_senders != null)
                {
                    for (var i = 0; i < _senders.Count; i++)
                    {
                        collection.Add("Sender Path: {0}".ApplyFormat(i), _senders[i].Route);
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
                _senders.Clear();
                _senders = null;
            }
            if (_receiverConnection is null) return;
            _receiverConnection.UnsubscribeAsync().WaitAsync();
            _receiverConnection = null;
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

            foreach (var queue in _senders)
            {
                var subscriber = await queue.GetSubscriberAsync().ConfigureAwait(false);
                Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}/{2}' with CorrelationId={3}", body.Length, queue.Route, queue.Name, message.Header.CorrelationId);
                await subscriber.PublishAsync(RedisChannel.Literal(queue.Name), body).ConfigureAwait(false);
            }
            Core.Log.LibVerbose("Message with CorrelationId={0} sent", message.Header.CorrelationId);
            Counters.IncrementTotalBytesSent(data.Count);
            data.ReturnContentToPoolAndDispose();
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
            if (_receiverConnection is null && UseSingleResponseQueue)
                throw new NullReferenceException("There is not receiver queue.");

            var sw = Stopwatch.StartNew();
            var message = ReceivedMessages.GetOrAdd(correlationId, cId => Message.Rent());
            if (!await message.WaitHandler.WaitAsync(_receiverOptionsTimeout, cancellationToken).ConfigureAwait(false))
                throw new MessageQueueTimeoutException(_receiverOptionsTimeout, correlationId.ToString());

            if (message.Body == MultiArray<byte>.Empty)
                throw new MessageQueueBodyNullException("The Message can't be retrieved, null body on CorrelationId = " + correlationId);

            Counters.IncrementTotalBytesReceived(message.Body.Count);
            Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}' with CorrelationId={2} at {3}ms", message.Body.Count, _clientQueues.RecvQueue.Name, correlationId, sw.Elapsed.TotalMilliseconds);
            var rs = ReceiverSerializer.Deserialize<ResponseMessage>(message.Body);
            ReceivedMessages.TryRemove(correlationId, out _);
            Message.Free(message);
            return rs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ProcessReceivedMessage(RedisChannel channel, RedisValue value)
        {
            try
            {
                (var body, var correlationId) = GetFromMessageBody(value);
                var rMsg = ReceivedMessages.GetOrAdd(correlationId, cId => Message.Rent());
                rMsg.Body = body;
                rMsg.WaitHandler.Set();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
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
