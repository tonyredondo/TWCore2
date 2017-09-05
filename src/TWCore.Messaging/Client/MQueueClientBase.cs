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

#pragma warning disable 1711
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Diagnostics.Status;
using TWCore.Messaging.Configuration;
using TWCore.Serialization;

namespace TWCore.Messaging.Client
{
    /// <inheritdoc />
    /// <summary>
    /// Message Queue client base
    /// </summary>
    public abstract class MQueueClientBase : IMQueueClient
    {
        private readonly WeakDictionary<object, object> _receivedMessagesCache = new WeakDictionary<object, object>();

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
        public MQClientCounters Counters { get; }
        #endregion

        #region Events
        /// <summary>
        /// Events that fires when a request message is sent
        /// </summary>
        public event EventHandler<RequestSentEventArgs> OnRequestSent;
        /// <summary>
        /// Events that fires when a request message is about to be sent
        /// </summary>
        public event EventHandler<RequestSentEventArgs> OnBeforeSendRequest;
        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        public event EventHandler<ResponseReceivedEventArgs> OnResponseReceived;
        #endregion

        #region .ctor
        /// <summary>
        /// Message Queue client base
        /// </summary>
        protected MQueueClientBase()
        {
            Counters = new MQClientCounters();
        }
        ~MQueueClientBase()
        {
            Dispose();
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
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Message correlation Id</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid Send<T>(T obj)
            => Send(obj, Guid.NewGuid());
        /// <inheritdoc />
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Message correlation Id</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid Send<T>(T obj, Guid correlationId)
        {
            RequestMessage rqMsg;
            if (obj is RequestMessage)
            {
                rqMsg = obj as RequestMessage;
            }
            else
            {
                rqMsg = new RequestMessage(obj)
                {
                    Header =
                    {
                        CorrelationId = correlationId,
                        ApplicationSentDate = Core.Now,
                        MachineName = Core.MachineName,
                        Label = Config?.RequestOptions?.ClientSenderOptions?.Label ?? obj?.GetType().FullName,
                        ClientName = Config?.Name
                    }
                };
            }
            var rsea = new RequestSentEventArgs(Name, rqMsg);
            OnBeforeSend(rqMsg);
            OnBeforeSendRequest?.Invoke(this, rsea);
            MQueueClientEvents.FireOnBeforeSendRequest(this, rsea);
            if (OnSend(rqMsg))
            {
                Counters.IncrementMessagesSent();
                OnRequestSent?.Invoke(this, rsea);
                MQueueClientEvents.FireOnRequestSent(this, rsea);
                return rqMsg.CorrelationId;
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
        {
            var tSource = new CancellationTokenSource();
            return Receive<T>(correlationId, tSource.Token);
        }
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
            var rsMsg = OnReceive(correlationId, cancellationToken);
            if (rsMsg == null) return default(T);

            rsMsg.Header.Response.ApplicationReceivedTime = Core.Now;
            OnAfterReceive(rsMsg);
            Counters.IncrementMessagesReceived();
            Counters.IncrementReceivingTime(rsMsg.Header.Response.TotalTime);
            var rrea = new ResponseReceivedEventArgs(Name, rsMsg);
            OnResponseReceived?.Invoke(this, rrea);
            MQueueClientEvents.FireOnResponseReceived(this, rrea);
            if (rsMsg.Body == null) return default(T);

            var res = default(T);
            try
            {
                res = (T)rsMsg.Body;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            if (res != null)
                _receivedMessagesCache.TryAdd(res, rsMsg);
            //receivedMessagesCache.TryAdd(res, rsMsg, TimeSpan.FromSeconds(30));
            return res;
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
        /// <typeparam name="T">Type of the object to be sent</typeparam>
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
        /// Gets the complete response message with headers from a body
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <returns>Complete Response message instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ResponseMessage GetCompleteMessage(object messageBody)
        {
            if (messageBody == null)
                return null;
            if (_receivedMessagesCache.TryGetValue(messageBody, out object _out))
                return (ResponseMessage)_out;
            return null;
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
		protected virtual void OnBeforeSend(RequestMessage message) { }
		/// <summary>
		/// On Send message data
		/// </summary>
		/// <param name="message">Request message instance</param>
		/// <returns>true if message has been sent; otherwise, false.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract bool OnSend(RequestMessage message);
		/// <summary>
		/// On Receive message data
		/// </summary>
		/// <param name="correlationId">Correlation Id</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <returns>Response message instance</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract ResponseMessage OnReceive(Guid correlationId, CancellationToken cancellationToken);
		/// <summary>
		/// After a response message has been received
		/// </summary>
		/// <param name="message">Response message instance</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void OnAfterReceive(ResponseMessage message) { }
		/// <summary>
		/// On Dispose
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract void OnDispose();
        #endregion
    }
}
