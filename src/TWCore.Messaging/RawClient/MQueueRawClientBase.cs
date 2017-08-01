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
using TWCore.Compression;
using TWCore.Diagnostics.Status;
using TWCore.Messaging.Configuration;
using TWCore.Serialization;

namespace TWCore.Messaging.RawClient
{
    /// <summary>
    /// Message Queue raw client base
    /// </summary>
    public abstract class MQueueRawClientBase : IMQueueRawClient
    {
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
        public MQueueRawClientBase()
        {
            Counters = new MQRawClientCounters();
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
        /// <param name="obj">Object to be sent</param>
        /// <returns>Message correlation Id</returns>
        public Guid Send(object obj)
            => Send(obj, Guid.NewGuid());
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Message correlation Id</returns>
        public Guid Send(object obj, Guid correlationId)
        {
            var bytes = SenderSerializer.Serialize(obj);
            return SendBytes((byte[])bytes, correlationId);
        }
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Message correlation Id</returns>
        public Guid SendBytes(byte[] obj)
            => SendBytes(obj, Guid.NewGuid());
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Message correlation Id</returns>
        public Guid SendBytes(byte[] obj, Guid correlationId)
        {
            RawMessageEventArgs rmea;
            OnBeforeSend(ref obj);
            rmea = new RawMessageEventArgs(Name, obj);
            OnBeforeSendRequest?.Invoke(this, rmea);
            MQueueRawClientEvents.FireOnBeforeSendRequest(this, rmea);
            obj = rmea.Message;
            if (OnSend(ref obj, correlationId))
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
            var msg = ReceiveBytes(correlationId, cancellationToken);
            if (msg != null)
                return ReceiverSerializer.Deserialize<T>(msg);
            else
                return default(T);
        }
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <param name="correlationId">Correlation id</param>
        /// <returns>Object instance received from the queue</returns>
        public byte[] ReceiveBytes(Guid correlationId)
        {
            var tSource = new CancellationTokenSource();
            return ReceiveBytes(correlationId, tSource.Token);
        }
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <param name="correlationId">Correlation id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        public byte[] ReceiveBytes(Guid correlationId, CancellationToken cancellationToken)
        {
            var bytes = OnReceive(correlationId, cancellationToken);
            if (bytes != null)
            {
                OnAfterReceive(ref bytes);
                Counters.IncrementMessagesReceived();
                Counters.IncrementTotalBytesReceived(bytes.Length);
                var rrea = new RawMessageEventArgs(Name, bytes);
                OnResponseReceived?.Invoke(this, rrea);
                MQueueRawClientEvents.FireOnResponseReceived(this, rrea);
            }
            return bytes;
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
        protected virtual void OnBeforeSend(ref byte[] message) { }
        /// <summary>
        /// On Send message data
        /// </summary>
        /// <param name="message">Request message instance</param>
        /// <param name="correlationId">Correlation Id</param>
        /// <returns>true if message has been sent; otherwise, false.</returns>
        protected abstract bool OnSend(ref byte[] message, Guid correlationId);
        /// <summary>
        /// On Receive message data
        /// </summary>
        /// <param name="correlationId">Correlation Id</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Response message instance</returns>
        protected abstract byte[] OnReceive(Guid correlationId, CancellationToken cancellationToken);
        /// <summary>
        /// After a response message has been received
        /// </summary>
        /// <param name="message">Response message instance</param>
        protected virtual void OnAfterReceive(ref byte[] message) { }
        /// <summary>
        /// On Dispose
        /// </summary>
        protected abstract void OnDispose();
        #endregion
    }
}
