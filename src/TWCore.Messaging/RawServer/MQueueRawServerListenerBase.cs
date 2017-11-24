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

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Messaging.Configuration;
using TWCore.Serialization;
// ReSharper disable SuggestBaseTypeForParameter

namespace TWCore.Messaging.RawServer
{
    /// <inheritdoc />
    /// <summary>
    /// Message queue server listener base
    /// </summary>
    public abstract class MQueueRawServerListenerBase : IMQueueRawServerListener
    {
        protected readonly ReferencePool<ActionWorker> WorkerPool;
        protected readonly ManualResetEventSlim WorkerEvent = new ManualResetEventSlim();
        protected int ActiveWorkers;

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Message queue connection
        /// </summary>
        public MQConnection Connection { get; protected set; }
        /// <inheritdoc />
        /// <summary>
        /// Gets the current configuration
        /// </summary>
        public MQPairConfig Config { get; protected set; }
        /// <inheritdoc />
        /// <summary>
        /// Message queue listener server counters
        /// </summary>
        public MQRawServerCounters Counters { get; }
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the receiver serializer
        /// </summary>
        public ISerializer ReceiverSerializer { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Gets if the server is configured as response server
        /// </summary>
        public bool ResponseServer { get; }
        #endregion

        #region Events
        /// <summary>
        /// Events that fires when a request message is received
        /// </summary>
        public event EventHandler<RawRequestReceivedEventArgs> RequestReceived;
        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        public event EventHandler<RawResponseReceivedEventArgs> ResponseReceived;
		#endregion

		#region .ctor
		/// <summary>
		/// Message queue server listener base
		/// </summary>
		/// <param name="connection">Queue server listener</param>
		/// <param name="server">Message queue server instance</param>
		/// <param name="responseServer">true if the server is going to act as a response server</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected MQueueRawServerListenerBase(MQConnection connection, IMQueueRawServer server, bool responseServer)
        {
            Connection = connection;
            Config = server.Config;
            Counters = new MQRawServerCounters();
            ReceiverSerializer = server.ReceiverSerializer;
            ResponseServer = responseServer;
            WorkerPool = new ReferencePool<ActionWorker>(Environment.ProcessorCount);
            Core.Status.Attach(collection =>
            {
                collection.Add("Connection Route:", Connection?.Route);
                collection.Add("Connection Name:", Connection?.Name);
                collection.Add(nameof(ResponseServer), ResponseServer);
                Core.Status.AttachChild(Counters, this);
            });
        }
		#endregion

		#region Public Methods
		/// <inheritdoc />
		/// <summary>
		/// Start the queue listener for request messages
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <returns>Task of the method execution</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task TaskStartAsync(CancellationToken token)
        {
            return OnListenerTaskStartAsync(token);
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
		/// Start the queue listener for request messages
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <returns>Task of the method execution</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract Task OnListenerTaskStartAsync(CancellationToken token);
		/// <summary>
		/// On Dispose
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract void OnDispose();
		#endregion

		#region Protected Methods
		/// <summary>
		/// Fires the RequestReceived event
		/// </summary>
		/// <param name="requestReceived">Request received event args</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void OnRequestReceived(RawRequestReceivedEventArgs requestReceived)
            => RequestReceived?.Invoke(this, requestReceived);
		/// <summary>
		/// Fires the ResponseReceived event
		/// </summary>
		/// <param name="responseReceived">Response received event args</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void OnResponseReceived(RawResponseReceivedEventArgs responseReceived)
            => ResponseReceived?.Invoke(this, responseReceived);
        /// <summary>
        /// Enqueue Message To Process
        /// </summary>
        /// <param name="processingAction">Processing Action</param>
        /// <param name="message">Message</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnqueueMessageToProcess<T>(Action<T> processingAction, T message)
        {
            Interlocked.Increment(ref ActiveWorkers);
            var worker = WorkerPool.New();
            WorkerEvent.Reset();
            worker.Enqueue(arg =>
            {
                processingAction(arg.Message);
                Counters.DecrementMessages();
                WorkerPool.Store(arg.Worker);
                if (Interlocked.Decrement(ref ActiveWorkers) == 0)
                    WorkerEvent.Set();
            }, (Worker: worker, Message: message));
            Counters.IncrementMessages();
        }
        #endregion
    }
}
