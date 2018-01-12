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
using System.Collections.Generic;
using System.Linq;
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

namespace TWCore.Messaging.RawServer
{
	/// <inheritdoc />
	/// <summary>
	/// Message Queue server base
	/// </summary>
	public abstract class MQueueRawServerBase : IMQueueRawServer
	{
		private readonly List<Task> _listenerTasks = new List<Task>();
		private CancellationTokenSource _tokenSource;
		private MQClientQueues _clientQueues;
		private MQServerQueues _serverQueues;

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
		/// Gets the list of message queue server listeners
		/// </summary>
		public List<IMQueueRawServerListener> QueueServerListeners { get; } = new List<IMQueueRawServerListener>();
		/// <inheritdoc />
		/// <summary>
		/// Gets if the server is configured as response server
		/// </summary>
		[StatusProperty]
		public bool ResponseServer { get; set; } = false;
        #endregion

        #region Events
        /// <inheritdoc />
        /// <summary>
        /// Events that fires when a request message is received
        /// </summary>
        //public event AsyncEventHandler<RawRequestReceivedEventArgs> RequestReceived;
        public AsyncEvent<RawRequestReceivedEventArgs> RequestReceived { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        //public event AsyncEventHandler<RawResponseReceivedEventArgs> ResponseReceived;
        public AsyncEvent<RawResponseReceivedEventArgs> ResponseReceived { get; set; }
        /// <inheritdoc />
		/// <summary>
		/// Events that fires when a response message is sent
		/// </summary>
		//public event AsyncEventHandler<RawResponseSentEventArgs> ResponseSent;
        public AsyncEvent<RawResponseSentEventArgs> ResponseSent { get; set; }
        /// <inheritdoc />
		/// <summary>
		/// Events that fires when a response message is about to be sent
		/// </summary>
		//public event AsyncEventHandler<RawResponseSentEventArgs> BeforeSendResponse;
        public AsyncEvent<RawResponseSentEventArgs> BeforeSendResponse { get; set; }
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
			StopListeners();
			QueueServerListeners.Clear();

			Config = config;
			Name = Config.Name;

			OnInit();

			Core.Status.Attach(collection =>
			{
				if (QueueServerListeners?.Any() != true) return;
				foreach (var listener in QueueServerListeners)
					Core.Status.AttachChild(listener, this);
			});
		}
		/// <inheritdoc />
		/// <summary>
		/// Start the queue listener for request messages
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void StartListeners()
		{
			if (_tokenSource != null)
				StopListeners();

			Core.Log.InfoBasic("Configuring server mode for {0} with ResponseServer = {1}", Name, ResponseServer);
			if (ResponseServer)
			{
				ReceiverSerializer = SerializerManager.GetByMimeType(Config.ResponseOptions?.SerializerMimeType);
				if (ReceiverSerializer != null && Config.ResponseOptions?.CompressorEncodingType.IsNotNullOrEmpty() == true)
					ReceiverSerializer.Compressor = CompressorManager.GetByEncodingType(Config.ResponseOptions?.CompressorEncodingType);

				SenderSerializer = null;

				Core.Log.InfoBasic("Adding queue client listener for {0}, Environment: {1}", Name, Core.EnvironmentName);
				_clientQueues = Config.ClientQueues?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true)
					?? Config.ClientQueues?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true)
					?? Config.ClientQueues?.FirstOrDefault(c => c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true)
					?? Config.ClientQueues?.FirstOrDefault(c => c.EnvironmentName.IsNullOrWhitespace());
				if (_clientQueues?.RecvQueue != null)
				{
					var queueListener = OnCreateQueueServerListener(_clientQueues.RecvQueue, ResponseServer);
					queueListener.ResponseReceived += QueueListener_ResponseReceived;
					QueueServerListeners.Add(queueListener);
				}
				else
					Core.Log.Warning("There aren't any Receiver Queue for the Client Listener. Check the ClientQueues element on the queue config file.");
			}
			else
			{
				ReceiverSerializer = SerializerManager.GetByMimeType(Config.RequestOptions?.SerializerMimeType);
				if (ReceiverSerializer != null && Config.RequestOptions?.CompressorEncodingType.IsNotNullOrEmpty() == true)
					ReceiverSerializer.Compressor = CompressorManager.GetByEncodingType(Config.RequestOptions?.CompressorEncodingType);

				SenderSerializer = SerializerManager.GetByMimeType(Config.ResponseOptions?.SerializerMimeType);
				if (SenderSerializer != null && Config.ResponseOptions?.CompressorEncodingType.IsNotNullOrEmpty() == true)
					SenderSerializer.Compressor = CompressorManager.GetByEncodingType(Config.ResponseOptions?.CompressorEncodingType);


				Core.Log.InfoBasic("Adding queue server listeners for {0}, Environment: {1}", Name, Core.EnvironmentName);
				_serverQueues = Config.ServerQueues?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true)
					?? Config.ServerQueues?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true)
					?? Config.ServerQueues?.FirstOrDefault(c => c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true)
					?? Config.ServerQueues?.FirstOrDefault(c => c.EnvironmentName.IsNullOrWhitespace());
				if (_serverQueues?.RecvQueues?.Any() == true)
				{
					_serverQueues.RecvQueues.Each(queue =>
					{
						var queueListener = OnCreateQueueServerListener(queue, ResponseServer);
						queueListener.RequestReceived += QueueListener_RequestReceived;
						QueueServerListeners.Add(queueListener);
					});
				}
				else
					Core.Log.Warning("There aren't any Receiver Queue for the Server Listeners. Check the ServerQueues element on the queue config file.");
			}

			if (QueueServerListeners.Count > 0)
			{
				Core.Log.InfoBasic("Starting queue server listeners for {0}", Name);
				_tokenSource = new CancellationTokenSource();
				_listenerTasks.Clear();

				foreach (var listener in QueueServerListeners)
				{
					var tsk = Task.Run(async () =>
					{
						Core.Log.InfoBasic("Starting queue server listener. Route: {0}, Name: {1}", listener.Connection.Route, listener.Connection.Name);
						await listener.TaskStartAsync(_tokenSource.Token).ConfigureAwait(false);
						Core.Log.InfoBasic("Queue server listener stopped. Route: {0}, Name: {1}", listener.Connection.Route, listener.Connection.Name);
					}, _tokenSource.Token);
					_listenerTasks.Add(tsk);
				}
			}
			else
				Core.Log.Warning("There are not server listener to start.");
		}
		/// <inheritdoc />
		/// <summary>
		/// Stop the queue listener
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void StopListeners()
		{
			if (_tokenSource == null) return;
			Core.Log.InfoBasic("Stopping queue server listeners for {0}", Name);
			_tokenSource.Cancel();
			Task.WaitAll(_listenerTasks.ToArray(), Config.RequestOptions.ServerReceiverOptions.ProcessingWaitOnFinalizeInSec);
			_listenerTasks.Clear();
			QueueServerListeners.Each(l => l.Dispose());
			QueueServerListeners.Clear();
			_tokenSource = null;
			Core.Log.InfoBasic("Queue server listeners for {0} stopped", Name);
		}
		/// <inheritdoc />
		/// <summary>
		/// Dispose all resources
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			StopListeners();
			OnDispose();
			Core.Status.DeAttachObject(this);
		}
		#endregion

		#region Private Methods
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private async Task QueueListener_RequestReceived(object sender, RawRequestReceivedEventArgs e)
		{
			if (_serverQueues?.AdditionalSendQueues?.Any() == true)
				e.ResponseQueues.AddRange(_serverQueues.AdditionalSendQueues);

            if (RequestReceived != null)
			    await RequestReceived.InvokeAsync(sender, e).ConfigureAwait(false);
			await MQueueRawServerEvents.FireRequestReceivedAsync(sender, e).ConfigureAwait(false);

			if (e.SendResponse && e.Response != null)
			{
				var response = e.Response;
				var rsea = new RawResponseSentEventArgs(Name, response, e.CorrelationId);
                if (BeforeSendResponse != null)
			        await BeforeSendResponse.InvokeAsync(this, rsea).ConfigureAwait(false);
				await MQueueRawServerEvents.FireBeforeSendResponseAsync(this, rsea).ConfigureAwait(false);
				response = rsea.Message;
				var sentBytes = await OnSendAsync(response, e).ConfigureAwait(false);
				if (sentBytes > -1)
				{
					rsea.MessageLength = sentBytes;
                    if (ResponseSent != null)
					    await ResponseSent.InvokeAsync(this, rsea).ConfigureAwait(false);
					await MQueueRawServerEvents.FireResponseSentAsync(this, rsea).ConfigureAwait(false);
				}
				else
					Core.Log.Warning("The message couldn't be sent.");
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private async Task QueueListener_ResponseReceived(object sender, RawResponseReceivedEventArgs e)
		{
            if (ResponseReceived != null)
			    await ResponseReceived.InvokeAsync(sender, e).ConfigureAwait(false);
			await MQueueRawServerEvents.FireResponseReceivedAsync(sender, e).ConfigureAwait(false);
		}
		#endregion

		#region Abstract Methods
		/// <summary>
		/// On Create all server listeners
		/// </summary>
		/// <param name="connection">Queue server listener</param>
		/// <param name="responseServer">true if the server is going to act as a response server</param>
		/// <returns>IMQueueServerListener</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract IMQueueRawServerListener OnCreateQueueServerListener(MQConnection connection, bool responseServer = false);
		/// <summary>
		/// On client initialization
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void OnInit() { }
		/// <summary>
		/// On Send message data
		/// </summary>
		/// <param name="message">Response message instance</param>
		/// <param name="e">Request event args</param>
		/// <returns>Number of bytes sent to the queue, -1 if no message was sent.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract Task<int> OnSendAsync(SubArray<byte> message, RawRequestReceivedEventArgs e);
		/// <summary>
		/// On Dispose
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void OnDispose() { }
		#endregion
	}
}
