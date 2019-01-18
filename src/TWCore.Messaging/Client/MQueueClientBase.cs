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

#pragma warning disable 1711
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Diagnostics.Status;
using TWCore.Messaging.Configuration;
using TWCore.Serialization;
using TWCore.Threading;

// ReSharper disable UnusedParameter.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace TWCore.Messaging.Client
{
    /// <inheritdoc />
    /// <summary>
    /// Message Queue client base
    /// </summary>
    [StatusName("Queue Client")]
    public abstract class MQueueClientBase : IMQueueClient
    {
        private readonly WeakDictionary<object, object> _receivedMessagesCache = new WeakDictionary<object, object>();
        /// <summary>
        /// Counters
        /// </summary>
        protected MQClientCounters Counters;

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
        /// Enables the Complete Message cache
        /// </summary>
        [StatusProperty]
        public bool EnableCompleteMessageCache { get; set; }
        #endregion

        #region Events
        /// <inheritdoc />
        /// <summary>
        /// Events that fires when a request message is sent
        /// </summary>
        public AsyncEvent<RequestSentEventArgs> OnRequestSent { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Events that fires when a request message is about to be sent
        /// </summary>
        public AsyncEvent<RequestSentEventArgs> OnBeforeSendRequest { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        public AsyncEvent<ResponseReceivedEventArgs> OnResponseReceived { get; set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Message Queue client base
        /// </summary>
        protected MQueueClientBase()
        {
	        Core.Status.Attach(collection =>
	        {
		        collection.Add("Type", GetType().FullName);
	        });
        }
        /// <summary>
        /// Message Queue finalizer
        /// </summary>
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
            if (config is null) return;

            Config = config;

            Name = Config.Name;
            Counters = new MQClientCounters(Name, Config.IgnoreClientCounters);

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
        public Task<Guid> SendAsync<T>(T obj)
            => SendAsync(obj, Guid.NewGuid());
        /// <inheritdoc />
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Message correlation Id</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Guid> SendAsync<T>(T obj, Guid correlationId)
        {
            bool disposeRequest = false;
            RequestMessage rqMsg = null;
            if (obj is RequestMessage rmsg)
                rqMsg = rmsg;
            else
            {
                rqMsg = new RequestMessage(SenderSerializer.GetSerializedObject(obj))
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
                disposeRequest = true;
            }
			RequestSentEventArgs rsea = null;
			if (OnBeforeSendRequest != null || MQueueClientEvents.OnBeforeSendRequest != null || 
			    OnRequestSent != null || MQueueClientEvents.OnRequestSent != null)
            {
				rsea = new RequestSentEventArgs(Name, rqMsg);
                if (OnBeforeSendRequest != null)
                    await OnBeforeSendRequest.InvokeAsync(this, rsea).ConfigureAwait(false);
                if (MQueueClientEvents.OnBeforeSendRequest != null)
                    await MQueueClientEvents.OnBeforeSendRequest.InvokeAsync(this, rsea).ConfigureAwait(false);
            }

            if (!await OnSendAsync(rqMsg).ConfigureAwait(false))
            {
                if (disposeRequest)
                {
                    rqMsg.Body?.Dispose();
                    rqMsg.Body = null;
                }
                return Guid.Empty;
            }
            Counters.IncrementMessagesSent();

            if (rsea != null)
            {
                if (OnRequestSent != null)
                {
                    disposeRequest = false;
                    await OnRequestSent.InvokeAsync(this, rsea).ConfigureAwait(false);
                }
                if (MQueueClientEvents.OnRequestSent != null)
                {
                    disposeRequest = false;
                    await MQueueClientEvents.OnRequestSent.InvokeAsync(this, rsea).ConfigureAwait(false);
                }
            }

            if (disposeRequest)
            {
                rqMsg.Body?.Dispose();
                rqMsg.Body = null;
            }
            return rqMsg.CorrelationId;
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
            var disposeResponse = true;
            var rsMsg = await OnReceiveAsync(correlationId, cancellationToken).ConfigureAwait(false);
            if (rsMsg is null) return default;

            rsMsg.Header.Response.ApplicationReceivedTime = Core.Now;
            Counters.IncrementMessagesReceived();
            Counters.IncrementReceptionTime(rsMsg.Header.Response.TotalTime);

            if (OnResponseReceived != null || MQueueClientEvents.OnResponseReceived != null)
            {
                disposeResponse = false;
                var rrea = new ResponseReceivedEventArgs(Name, rsMsg);
                if (OnResponseReceived != null)
                    await OnResponseReceived.InvokeAsync(this, rrea).ConfigureAwait(false);
                if (MQueueClientEvents.OnResponseReceived != null)
                    await MQueueClientEvents.OnResponseReceived.InvokeAsync(this, rrea).ConfigureAwait(false);
            }

            if (rsMsg.Body is null) return default;

            var res = default(T);
            try
            {
                res = (T)rsMsg.Body.GetValue();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            if (EnableCompleteMessageCache && res != null)
                _receivedMessagesCache.TryAdd(res, rsMsg);
            else if (disposeResponse)
                rsMsg.Body.Dispose();
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
		/// <typeparam name="T">Type of the object to be sent</typeparam>
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
		/// <typeparam name="T">Type of the object to be sent</typeparam>
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
		/// <typeparam name="T">Type of the object to be sent</typeparam>
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
		/// <typeparam name="T">Type of the object to be sent</typeparam>
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
        /// Gets the complete response message with headers from a body
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <returns>Complete Response message instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ResponseMessage GetCompleteMessage(object messageBody)
        {
            if (messageBody is null)
                return null;
            if (EnableCompleteMessageCache && _receivedMessagesCache.TryGetValue(messageBody, out var _out))
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
		/// On Send message data
		/// </summary>
		/// <param name="message">Request message instance</param>
		/// <returns>true if message has been sent; otherwise, false.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract Task<bool> OnSendAsync(RequestMessage message);
		/// <summary>
		/// On Receive message data
		/// </summary>
		/// <param name="correlationId">Correlation Id</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <returns>Response message instance</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract Task<ResponseMessage> OnReceiveAsync(Guid correlationId, CancellationToken cancellationToken);
		/// <summary>
		/// On Dispose
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract void OnDispose();
        #endregion
    }
}
