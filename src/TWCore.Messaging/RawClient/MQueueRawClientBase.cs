﻿/*
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

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using TWCore.Compression;
using TWCore.Diagnostics.Status;
using TWCore.Messaging.Configuration;
using TWCore.Serialization;

namespace TWCore.Messaging.RawClient
{
    /// <inheritdoc />
    /// <summary>
    /// Message Queue raw client base
    /// </summary>
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
        /// <summary>
        /// Events that fires when a request message is sent
        /// </summary>
        public event EventHandler<RawMessageEventArgs> OnRequestSent;
        /// <summary>
        /// Events that fires when a request message is about to be sent
        /// </summary>
        public event EventHandler<RawMessageEventArgs> OnBeforeSendRequest;
        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        public event EventHandler<RawMessageEventArgs> OnResponseReceived;
        #endregion

        #region .ctor
        /// <summary>
        /// Message Queue client base
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected MQueueRawClientBase()
        {
            Counters = new MQRawClientCounters();
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
        public Guid Send(object obj)
            => Send(obj, Guid.NewGuid());
        /// <inheritdoc />
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Message correlation Id</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid Send(object obj, Guid correlationId)
        {
            if (obj is byte[] bytes)
                return SendBytes(bytes, correlationId);
            return SendBytes((byte[])SenderSerializer.Serialize(obj), correlationId);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Message correlation Id</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid SendBytes(byte[] obj)
            => SendBytes(obj, Guid.NewGuid());
        /// <inheritdoc />
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Message correlation Id</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid SendBytes(byte[] obj, Guid correlationId)
        {
            OnBeforeSend(ref obj);
            var rmea = new RawMessageEventArgs(Name, obj);
            OnBeforeSendRequest?.Invoke(this, rmea);
            MQueueRawClientEvents.FireOnBeforeSendRequest(this, rmea);
            obj = rmea.Message;
            if (OnSend(obj, correlationId))
            {
                Counters.IncrementMessagesSent();
                Counters.IncrementTotalBytesSent(obj.Length);
                rmea = new RawMessageEventArgs(Name, obj);
                OnRequestSent?.Invoke(this, rmea);
                MQueueRawClientEvents.FireOnRequestSent(this, rmea);
                return correlationId;
            }
            return Guid.Empty;
        }
        /// <inheritdoc />
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <typeparam name="T">Type of the object to be received</typeparam>
        /// <param name="correlationId">Correlation id</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Receive<T>(Guid correlationId)
            => Receive<T>(correlationId, CancellationToken.None);
        /// <inheritdoc />
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <typeparam name="T">Type of the object to be received</typeparam>
        /// <param name="correlationId">Correlation id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Receive<T>(Guid correlationId, CancellationToken cancellationToken)
        {
            var msg = ReceiveBytes(correlationId, cancellationToken);
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
        public byte[] ReceiveBytes(Guid correlationId)
            => ReceiveBytes(correlationId, CancellationToken.None);
        /// <inheritdoc />
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <param name="correlationId">Correlation id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReceiveBytes(Guid correlationId, CancellationToken cancellationToken)
        {
            var bytes = OnReceive(correlationId, cancellationToken);
            if (bytes == null) return null;

            OnAfterReceive(ref bytes);
            Counters.IncrementMessagesReceived();
            Counters.IncrementTotalBytesReceived(bytes.Length);
            var rrea = new RawMessageEventArgs(Name, bytes);
            OnResponseReceived?.Invoke(this, rrea);
            MQueueRawClientEvents.FireOnResponseReceived(this, rrea);
            return bytes;
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] SendAndReceive(byte[] obj)
        {
            var correlationId = SendBytes(obj);
            return ReceiveBytes(correlationId);
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
        public TR SendAndReceive<TR, T>(T obj)
        {
            var correlationId = Send(obj);
            return Receive<TR>(correlationId);
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
        public TR SendAndReceive<TR, T>(T obj, CancellationToken cancellationToken)
        {
            var correlationId = Send(obj);
            return Receive<TR>(correlationId, cancellationToken);
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
        public TR SendAndReceive<TR, T>(T obj, Guid correlationId)
        {
            correlationId = Send(obj, correlationId);
            return Receive<TR>(correlationId);
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
        public TR SendAndReceive<TR, T>(T obj, Guid correlationId, CancellationToken cancellationToken)
        {
            correlationId = Send(obj, correlationId);
            return Receive<TR>(correlationId, cancellationToken);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Object instance received from the queue</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TR SendAndReceive<TR>(object obj)
        {
            var correlationId = Send(obj);
            return Receive<TR>(correlationId);
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
        public TR SendAndReceive<TR>(object obj, CancellationToken cancellationToken)
        {
            var correlationId = Send(obj);
            return Receive<TR>(correlationId, cancellationToken);
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
        public TR SendAndReceive<TR>(object obj, Guid correlationId)
        {
            correlationId = Send(obj, correlationId);
            return Receive<TR>(correlationId);
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
        public TR SendAndReceive<TR>(object obj, Guid correlationId, CancellationToken cancellationToken)
        {
            correlationId = Send(obj, correlationId);
            return Receive<TR>(correlationId, cancellationToken);
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
		/// Before send the request message
		/// </summary>
		/// <param name="message">Request message instance</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void OnBeforeSend(ref byte[] message) { }
		/// <summary>
		/// On Send message data
		/// </summary>
		/// <param name="message">Request message instance</param>
		/// <param name="correlationId">Correlation Id</param>
		/// <returns>true if message has been sent; otherwise, false.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract bool OnSend(byte[] message, Guid correlationId);
		/// <summary>
		/// On Receive message data
		/// </summary>
		/// <param name="correlationId">Correlation Id</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <returns>Response message instance</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract byte[] OnReceive(Guid correlationId, CancellationToken cancellationToken);
		/// <summary>
		/// After a response message has been received
		/// </summary>
		/// <param name="message">Response message instance</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void OnAfterReceive(ref byte[] message) { }
		/// <summary>
		/// On Dispose
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract void OnDispose();
        #endregion
    }
}
