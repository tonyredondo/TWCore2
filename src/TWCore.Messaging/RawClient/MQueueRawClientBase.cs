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

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Compression;
using TWCore.Diagnostics.Status;
using TWCore.Messaging.Configuration;
using TWCore.Serialization;
using TWCore.Threading;

// ReSharper disable UnusedParameter.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace TWCore.Messaging.RawClient
{
    /// <inheritdoc />
    /// <summary>
    /// Message Queue raw client base
    /// </summary>
    [StatusName("Raw Queue Client")]
    public abstract class MQueueRawClientBase : IMQueueRawClient
    {
        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Gets or Sets the client name
        /// </summary>
        [StatusProperty]
        public string Name { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the sender serializer
        /// </summary>
        [StatusProperty]
        public ISerializer SenderSerializer { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the receiver serializer
        /// </summary>
        [StatusProperty]
        public ISerializer ReceiverSerializer { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Gets the current configuration
        /// </summary>
        public MQPairConfig Config { get; private set; }
        /// <inheritdoc />
        /// <summary>
        /// Gets the client counters
        /// </summary>
        [StatusReference]
        public MQRawClientCounters Counters { get; }
        #endregion

        #region Events
        /// <inheritdoc />
        /// <summary>
        /// Events that fires when a request message is sent
        /// </summary>
        //public event AsyncEventHandler<RawMessageEventArgs> OnRequestSent;
        public AsyncEvent<RawMessageEventArgs> OnRequestSent { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Events that fires when a request message is about to be sent
        /// </summary>
        //public event AsyncEventHandler<RawMessageEventArgs> OnBeforeSendRequest;
        public AsyncEvent<RawMessageEventArgs> OnBeforeSendRequest { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        //public event AsyncEventHandler<RawMessageEventArgs> OnResponseReceived;
        public AsyncEvent<RawMessageEventArgs> OnResponseReceived { get; set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Message Queue client base
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected MQueueRawClientBase()
        {
            Counters = new MQRawClientCounters();
            Core.Status.Attach(collection =>
            {
                collection.Add("Type", GetType().FullName);
            });
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Initialize client with the configuration
        /// </summary>
        /// <param name="config">Message queue client configuration</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(MQPairConfig config)
        {
            if (config == null) return;
            Config = config;

            Name = Config.Name;
            SenderSerializer = SerializerManager.GetByMimeType(Config.RequestOptions?.SerializerMimeType);
            if (SenderSerializer != null && Config.RequestOptions?.CompressorEncodingType.IsNotNullOrEmpty() == true)
                SenderSerializer.Compressor = CompressorManager.GetByEncodingType(Config.RequestOptions?.CompressorEncodingType);
            ReceiverSerializer = SerializerManager.GetByMimeType(Config.ResponseOptions?.SerializerMimeType);
            if (ReceiverSerializer != null && Config.ResponseOptions?.CompressorEncodingType.IsNotNullOrEmpty() == true)
                ReceiverSerializer.Compressor = CompressorManager.GetByEncodingType(Config.ResponseOptions?.CompressorEncodingType);

            OnInit();
        }

        /// <inheritdoc />
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Message correlation Id</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Guid> SendAsync(object obj)
            => SendAsync(obj, Guid.NewGuid());
        /// <inheritdoc />
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Message correlation Id</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Guid> SendAsync(object obj, Guid correlationId)
        {
            if (obj is byte[] bytes)
                return SendBytesAsync(bytes, correlationId);
            return SendBytesAsync(SenderSerializer.Serialize(obj).AsArray(), correlationId);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Message correlation Id</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Guid> SendBytesAsync(byte[] obj)
            => SendBytesAsync(obj, Guid.NewGuid());
        /// <inheritdoc />
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Message correlation Id</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Guid> SendBytesAsync(byte[] obj, Guid correlationId)
        {
            RawMessageEventArgs rmea = null;
            if (OnBeforeSendRequest != null || MQueueRawClientEvents.OnBeforeSendRequest != null ||
                OnRequestSent != null || MQueueRawClientEvents.OnRequestSent != null)
            {
                rmea = new RawMessageEventArgs(Name, obj);
                if (OnBeforeSendRequest != null)
                    await OnBeforeSendRequest.InvokeAsync(this, rmea).ConfigureAwait(false);
                if (MQueueRawClientEvents.OnBeforeSendRequest != null)
                    await MQueueRawClientEvents.OnBeforeSendRequest.InvokeAsync(this, rmea).ConfigureAwait(false);
                obj = rmea.Message;
            }
            if (!await OnSendAsync(obj, correlationId).ConfigureAwait(false)) return Guid.Empty;
            Counters.IncrementMessagesSent();
            Counters.IncrementTotalBytesSent(obj.Length);
            if (rmea != null)
            {
                if (OnRequestSent != null)
                    await OnRequestSent.InvokeAsync(this, rmea).ConfigureAwait(false);
                if (MQueueRawClientEvents.OnRequestSent != null)
                    await MQueueRawClientEvents.OnRequestSent.InvokeAsync(this, rmea).ConfigureAwait(false);
            }
            return correlationId;
        }
        /// <inheritdoc />
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <typeparam name="T">Type of the object to be received</typeparam>
        /// <param name="correlationId">Correlation id</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> ReceiveAsync<T>(Guid correlationId)
            => ReceiveAsync<T>(correlationId, CancellationToken.None);
        /// <inheritdoc />
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <typeparam name="T">Type of the object to be received</typeparam>
        /// <param name="correlationId">Correlation id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> ReceiveAsync<T>(Guid correlationId, CancellationToken cancellationToken)
        {
            var msg = await ReceiveBytesAsync(correlationId, cancellationToken).ConfigureAwait(false);
            if (msg == null) return default(T);
            if (typeof(T) == typeof(byte[]))
                return (T)(object)msg;
            return ReceiverSerializer.Deserialize<T>(msg);
        }
        /// <inheritdoc />
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <param name="correlationId">Correlation id</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<byte[]> ReceiveBytesAsync(Guid correlationId)
            => ReceiveBytesAsync(correlationId, CancellationToken.None);
        /// <inheritdoc />
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <param name="correlationId">Correlation id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<byte[]> ReceiveBytesAsync(Guid correlationId, CancellationToken cancellationToken)
        {
            var bytes = await OnReceiveAsync(correlationId, cancellationToken).ConfigureAwait(false);
            if (bytes == null) return null;

            Counters.IncrementMessagesReceived();
            Counters.IncrementTotalBytesReceived(bytes.Length);

            if (OnResponseReceived != null || MQueueRawClientEvents.OnResponseReceived != null)
            {
                var rrea = new RawMessageEventArgs(Name, bytes);
                if (OnResponseReceived != null)
                    await OnResponseReceived.InvokeAsync(this, rrea).ConfigureAwait(false);
                if (MQueueRawClientEvents.OnResponseReceived != null)
                    await MQueueRawClientEvents.OnResponseReceived.InvokeAsync(this, rrea).ConfigureAwait(false);
            }

            return bytes;
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<byte[]> SendAndReceiveAsync(byte[] obj)
        {
            var correlationId = await SendBytesAsync(obj).ConfigureAwait(false);
            return await ReceiveBytesAsync(correlationId).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TR> SendAndReceiveAsync<TR, T>(T obj)
        {
            var correlationId = await SendAsync(obj).ConfigureAwait(false);
            return await ReceiveAsync<TR>(correlationId).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TR> SendAndReceiveAsync<TR, T>(T obj, CancellationToken cancellationToken)
        {
            var correlationId = await SendAsync(obj).ConfigureAwait(false);
            return await ReceiveAsync<TR>(correlationId, cancellationToken).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TR> SendAndReceiveAsync<TR, T>(T obj, Guid correlationId)
        {
            correlationId = await SendAsync(obj, correlationId).ConfigureAwait(false);
            return await ReceiveAsync<TR>(correlationId).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TR> SendAndReceiveAsync<TR, T>(T obj, Guid correlationId, CancellationToken cancellationToken)
        {
            correlationId = await SendAsync(obj, correlationId).ConfigureAwait(false);
            return await ReceiveAsync<TR>(correlationId, cancellationToken).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TR> SendAndReceiveAsync<TR>(object obj)
        {
            var correlationId = await SendAsync(obj).ConfigureAwait(false);
            return await ReceiveAsync<TR>(correlationId).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TR> SendAndReceiveAsync<TR>(object obj, CancellationToken cancellationToken)
        {
            var correlationId = await SendAsync(obj).ConfigureAwait(false);
            return await ReceiveAsync<TR>(correlationId, cancellationToken).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TR> SendAndReceiveAsync<TR>(object obj, Guid correlationId)
        {
            correlationId = await SendAsync(obj, correlationId).ConfigureAwait(false);
            return await ReceiveAsync<TR>(correlationId).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TR> SendAndReceiveAsync<TR>(object obj, Guid correlationId, CancellationToken cancellationToken)
        {
            correlationId = await SendAsync(obj, correlationId).ConfigureAwait(false);
            return await ReceiveAsync<TR>(correlationId, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            OnDispose();
            Core.Status.DeAttachObject(this);
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On client initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void OnInit();
        /// <summary>
        /// On Send message data
        /// </summary>
        /// <param name="message">Request message instance</param>
        /// <param name="correlationId">Correlation Id</param>
        /// <returns>true if message has been sent; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<bool> OnSendAsync(byte[] message, Guid correlationId);
        /// <summary>
        /// On Receive message data
        /// </summary>
        /// <param name="correlationId">Correlation Id</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Response message instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<byte[]> OnReceiveAsync(Guid correlationId, CancellationToken cancellationToken);
        /// <summary>
        /// On Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void OnDispose();
        #endregion
    }
}
