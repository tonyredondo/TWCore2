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
using System.Threading;
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Diagnostics.Status;
using TWCore.Messaging.Configuration;
using TWCore.Serialization;

namespace TWCore.Messaging.Client
{
    /// <summary>
    /// Message Queue client base
    /// </summary>
    public abstract class MQueueClientBase : IMQueueClient
    {
        readonly WeakDictionary<object, object> receivedMessagesCache = new WeakDictionary<object, object>();

        #region Properties
        /// <summary>
        /// Gets or Sets the client name
        /// </summary>
        [StatusProperty]
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the sender serializer
        /// </summary>
        [StatusProperty]
        public ISerializer SenderSerializer { get; set; }
        /// <summary>
        /// Gets or sets the receiver serializer
        /// </summary>
        [StatusProperty]
        public ISerializer ReceiverSerializer { get; set; }
        /// <summary>
        /// Gets the current configuration
        /// </summary>
        public MQPairConfig Config { get; private set; }
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
        public MQueueClientBase()
        {
            Counters = new MQClientCounters();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize client with the configuration
        /// </summary>
        /// <param name="config">Message queue client configuration</param>
        public void Init(MQPairConfig config)
        {
            if (config != null)
            {
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
        }
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Message correlation Id</returns>
        public Guid Send<T>(T obj)
            => Send(obj, Guid.NewGuid());
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Message correlation Id</returns>
        public Guid Send<T>(T obj, Guid correlationId)
        {
            RequestMessage rqMsg;
            if (obj is RequestMessage)
            {
                rqMsg = obj as RequestMessage;
            }
            else
            {
                rqMsg = new RequestMessage(obj);
                rqMsg.Header.CorrelationId = correlationId;
                rqMsg.Header.ApplicationSentDate = Core.Now;
                rqMsg.Header.MachineName = Core.MachineName;
                rqMsg.Header.Label = Config?.RequestOptions?.ClientSenderOptions?.Label ?? obj?.GetType().FullName;
                rqMsg.Header.ClientName = Config?.Name;
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
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <typeparam name="T">Type of the object to be received</typeparam>
        /// <param name="correlationId">Correlation id</param>
        /// <returns>Object instance received from the queue</returns>
        public T Receive<T>(Guid correlationId)
        {
            var tSource = new CancellationTokenSource();
            return Receive<T>(correlationId, tSource.Token);
        }
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <typeparam name="T">Type of the object to be received</typeparam>
        /// <param name="correlationId">Correlation id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        public T Receive<T>(Guid correlationId, CancellationToken cancellationToken)
        {
            var rsMsg = OnReceive(correlationId, cancellationToken);
            if (rsMsg != null)
            {
                rsMsg.Header.Response.ApplicationReceivedTime = Core.Now;
                OnAfterReceive(rsMsg);
                Counters.IncrementMessagesReceived();
                Counters.IncrementReceivingTime(rsMsg.Header.Response.TotalTime);
                if (rsMsg.Header.Response.NetworkTime.HasValue)
                    Counters.IncrementTotalNetworkTime(rsMsg.Header.Response.NetworkTime.Value);
                var rrea = new ResponseReceivedEventArgs(Name, rsMsg);
                OnResponseReceived?.Invoke(this, rrea);
                MQueueClientEvents.FireOnResponseReceived(this, rrea);
                if (rsMsg.Body != null)
                {
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
                        receivedMessagesCache.TryAdd(res, rsMsg);
                    //receivedMessagesCache.TryAdd(res, rsMsg, TimeSpan.FromSeconds(30));
                    return res;
                }
            }
            return default(T);
        }
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Object instance received from the queue</returns>
        public R SendAndReceive<R, T>(T obj)
        {
            var correlationId = Send(obj);
            return Receive<R>(correlationId);
        }
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        public R SendAndReceive<R, T>(T obj, CancellationToken cancellationToken)
        {
            var correlationId = Send(obj);
            return Receive<R>(correlationId, cancellationToken);
        }
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Object instance received from the queue</returns>
        public R SendAndReceive<R, T>(T obj, Guid correlationId)
        {
            correlationId = Send(obj, correlationId);
            return Receive<R>(correlationId);
        }
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        public R SendAndReceive<R, T>(T obj, Guid correlationId, CancellationToken cancellationToken)
        {
            correlationId = Send(obj, correlationId);
            return Receive<R>(correlationId, cancellationToken);
        }
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Object instance received from the queue</returns>
        public R SendAndReceive<R>(object obj)
        {
            var correlationId = Send(obj);
            return Receive<R>(correlationId);
        }
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        public R SendAndReceive<R>(object obj, CancellationToken cancellationToken)
        {
            var correlationId = Send(obj);
            return Receive<R>(correlationId, cancellationToken);
        }
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Object instance received from the queue</returns>
        public R SendAndReceive<R>(object obj, Guid correlationId)
        {
            correlationId = Send(obj, correlationId);
            return Receive<R>(correlationId);
        }
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        public R SendAndReceive<R>(object obj, Guid correlationId, CancellationToken cancellationToken)
        {
            correlationId = Send(obj, correlationId);
            return Receive<R>(correlationId, cancellationToken);
        }
        /// <summary>
        /// Gets the complete response message with headers from a body
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <returns>Complete Response message instance</returns>
        public ResponseMessage GetCompleteMessage(object messageBody)
        {
            if (messageBody == null)
                return null;
            if (receivedMessagesCache.TryGetValue(messageBody, out object _out))
                return (ResponseMessage)_out;
            return null;
        }
        /// <summary>
        /// Dispose all resources
        /// </summary>
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
        protected abstract void OnInit();
        /// <summary>
        /// Before send the request message
        /// </summary>
        /// <param name="message">Request message instance</param>
        protected virtual void OnBeforeSend(RequestMessage message) { }
        /// <summary>
        /// On Send message data
        /// </summary>
        /// <param name="message">Request message instance</param>
        /// <returns>true if message has been sent; otherwise, false.</returns>
        protected abstract bool OnSend(RequestMessage message);
        /// <summary>
        /// On Receive message data
        /// </summary>
        /// <param name="correlationId">Correlation Id</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Response message instance</returns>
        protected abstract ResponseMessage OnReceive(Guid correlationId, CancellationToken cancellationToken);
        /// <summary>
        /// After a response message has been received
        /// </summary>
        /// <param name="message">Response message instance</param>
        protected virtual void OnAfterReceive(ResponseMessage message) { }
        /// <summary>
        /// On Dispose
        /// </summary>
        protected abstract void OnDispose();
        #endregion
    }
}
