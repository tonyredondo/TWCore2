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

namespace TWCore.Messaging.Redis
{
    /// <inheritdoc />
    /// <summary>
    /// Redis Queue Raw Client
    /// </summary>
    public class RedisQueueRawClient : MQueueRawClientBase
    {
        private static readonly ConcurrentDictionary<Guid, Message> ReceivedMessages = new ConcurrentDictionary<Guid, Message>();
        private static readonly InstanceLocker<Guid> CreationLock = new InstanceLocker<Guid>();
        private static readonly UTF8Encoding Encoding = new UTF8Encoding(false);

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
        /// <param name="correlationId">Message CorrelationId</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override async Task<bool> OnSendAsync(byte[] message, Guid correlationId)
        {
            if (_senders?.Any() != true)
                throw new NullReferenceException("There aren't any senders queues.");
            if (_senderOptions is null)
                throw new NullReferenceException("SenderOptions is null.");

            var recvQueue = _clientQueues.RecvQueue;
            var name = recvQueue?.Name;
            if (!UseSingleResponseQueue && name != null)
                name += "-" + Core.InstanceIdString;
            var body = CreateRawMessageBody(message, correlationId, name ?? string.Empty);

            foreach (var queue in _senders)
            {
                var subscriber = await queue.GetSubscriberAsync().ConfigureAwait(false);
                Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}/{2}' with CorrelationId={3}", body.Length, queue.Route, queue.Name, correlationId);
                await subscriber.PublishAsync(RedisChannel.Literal(queue.Name), body).ConfigureAwait(false);
            }

            Core.Log.LibVerbose("Message with CorrelationId={0} sent", correlationId);
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
        protected override async Task<byte[]> OnReceiveAsync(Guid correlationId, CancellationToken cancellationToken)
        {
            if (_receiverConnection is null && UseSingleResponseQueue)
                throw new NullReferenceException("There is not receiver queue.");

            var sw = Stopwatch.StartNew();
            var message = ReceivedMessages.GetOrAdd(correlationId, _ => Message.Rent());
            if (!await message.WaitHandler.WaitAsync(_receiverOptionsTimeout, cancellationToken).ConfigureAwait(false))
                throw new MessageQueueTimeoutException(_receiverOptionsTimeout, correlationId.ToString());

            Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}' with CorrelationId={2} at {3}ms", message.Body.Count, _clientQueues.RecvQueue.Name, correlationId, sw.Elapsed.TotalMilliseconds);
            var body = message.Body;
            ReceivedMessages.TryRemove(correlationId, out _);
            Message.Free(message);
            return body.AsArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ProcessReceivedMessage(RedisChannel channel, RedisValue value)
        {
            try
            {
                (var body, var correlationId, var _) = GetFromRawMessageBody(value, false);
                var rMsg = ReceivedMessages.GetOrAdd(correlationId, _ => Message.Rent());
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
