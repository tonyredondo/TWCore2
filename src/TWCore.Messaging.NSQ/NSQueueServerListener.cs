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
using NsqSharp;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Server;
// ReSharper disable InconsistentNaming
// ReSharper disable MethodSupportsCancellation
#pragma warning disable 414
#pragma warning disable CS4014 // Because a call is not awaited

namespace TWCore.Messaging.NSQ
{
	/// <inheritdoc />
	/// <summary>
	/// NSQ server listener implementation
	/// </summary>
	public class NSQueueServerListener : MQueueServerListenerBase
	{
		#region Fields
		private readonly object _lock = new object();
		private readonly Type _messageType;
		private readonly string _name;
		private Consumer _receiver;
		private CancellationToken _token;
		private Task _monitorTask;
		private bool _exceptionSleep;
		#endregion

		#region Nested Type
		private struct NSQMessage
		{
			public Guid CorrelationId;
			public SubArray<byte> Body;
		}
		private class NSQMessageHandler : IHandler
        {
	        private readonly NSQueueServerListener _listener;
            public NSQMessageHandler(NSQueueServerListener listener)
            {
                _listener = listener;
            }
            public void HandleMessage(NsqSharp.IMessage message)
            {
                Core.Log.LibVerbose("Message received");
                try
                {
                    (var body, var correlationId) = NSQueueClient.GetFromMessageBody(message.Body);
                    var rMsg = new NSQMessage
                    {
                        CorrelationId = correlationId,
                        Body = body
                    };
                    Task.Run(() => _listener.EnqueueMessageToProcessAsync(_listener.ProcessingTaskAsync, rMsg));
                    Try.Do(message.Finish, false);
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
            public void LogFailedMessage(NsqSharp.IMessage message)
            {
            }
        }

        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// NSQ server listener implementation
        /// </summary>
        /// <param name="connection">Queue server listener</param>
        /// <param name="server">Message queue server instance</param>
        /// <param name="responseServer">true if the server is going to act as a response server</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public NSQueueServerListener(MQConnection connection, IMQueueServer server, bool responseServer) : base(connection, server, responseServer)
		{
			_messageType = responseServer ? typeof(ResponseMessage) : typeof(RequestMessage);
			_name = server.Name;
			Core.Status.Attach(collection =>
			{
				collection.Add(nameof(_messageType), _messageType);
			});
		}
		#endregion

		#region Override Methods
		/// <inheritdoc />
		/// <summary>
		/// Start the queue listener for request messages
		/// </summary>
		/// <param name="token">Cancellation token</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override async Task OnListenerTaskStartAsync(CancellationToken token)
		{
			_token = token;
            _receiver = new Consumer(Connection.Name, Connection.Name);
            _receiver.AddHandler(new NSQMessageHandler(this));
            await Extensions.InvokeWithRetry(() => 
            {
                _receiver.ConnectToNsqd(Connection.Route);
            }, 5000, int.MaxValue).ConfigureAwait(false);
            _monitorTask = Task.Run(MonitorProcess, _token);
			await token.WhenCanceledAsync().ConfigureAwait(false);
			OnDispose();
		    await _monitorTask.ConfigureAwait(false);
		}
		/// <inheritdoc />
		/// <summary>
		/// On Dispose
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override void OnDispose()
		{
			if (_receiver == null) return;
			try
			{
				_receiver.Stop();
			}
			catch
			{
				// ignored
			}
			_receiver = null;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Monitors the maximum concurrent message allowed for the listener
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private async Task MonitorProcess()
		{
			while (!_token.IsCancellationRequested)
			{
				try
				{
					bool exSleep;
					lock (_lock)
						exSleep = _exceptionSleep;
					if (exSleep)
					{
						OnDispose();
						Core.Log.Warning("An exception has been thrown, the listener has been stoped for {0} seconds.", Config.RequestOptions.ServerReceiverOptions.SleepOnExceptionInSec);
						await Task.Delay(Config.RequestOptions.ServerReceiverOptions.SleepOnExceptionInSec * 1000, _token).ConfigureAwait(false);
						lock (_lock)
							_exceptionSleep = false;
                        _receiver = new Consumer(Connection.Name, Connection.Name);
                        _receiver.AddHandler(new NSQMessageHandler(this));
                        _receiver.ConnectToNsqd(Connection.Route);
					    Core.Log.Warning("The listener has been resumed.");
                    }

                    if (Counters.CurrentMessages >= Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue)
					{
						OnDispose();
						Core.Log.Warning("Maximum simultaneous messages per queue has been reached, the message needs to wait to be processed, consider increase the MaxSimultaneousMessagePerQueue value, CurrentValue={0}.", Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue);

						while (!_token.IsCancellationRequested && Counters.CurrentMessages >= Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue)
							await Task.Delay(500, _token).ConfigureAwait(false);

                        _receiver = new Consumer(Connection.Name, Connection.Name);
                        _receiver.AddHandler(new NSQMessageHandler(this));
                        _receiver.ConnectToNsqd(Connection.Route);
					    Core.Log.Warning("The listener has been resumed.");
                    }

					await Task.Delay(100, _token).ConfigureAwait(false);
				}
				catch (TaskCanceledException) { }
				catch (Exception ex)
				{
					Core.Log.Write(ex);
					if (!_token.IsCancellationRequested)
						await Task.Delay(2000, _token).ConfigureAwait(false);
				}
			}
		}
        /// <summary>
        /// Process a received message from the queue
        /// </summary>
        /// <param name="message">Message instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		private async Task ProcessingTaskAsync(NSQMessage message)
		{
            if (message.Body == null) return;
			try
			{
				Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}/{2}'", message.Body.Count, Connection.Route, Connection.Name);
				var messageBody = ReceiverSerializer.Deserialize(message.Body, _messageType);
				switch (messageBody)
				{
					case RequestMessage request when request.Header != null:
						request.Header.ApplicationReceivedTime = Core.Now;
						Counters.IncrementReceivingTime(request.Header.TotalTime);
						if (request.Header.ClientName != Config.Name)
							Core.Log.Warning("The Message Client Name '{0}' is different from the Server Name '{1}'", request.Header.ClientName, Config.Name);
						var evArgs = new RequestReceivedEventArgs(_name, Connection, request, message.Body.Count, SenderSerializer);
						if (request.Header.ResponseQueue != null)
							evArgs.ResponseQueues.Add(request.Header.ResponseQueue);
						await OnRequestReceivedAsync(evArgs).ConfigureAwait(false);
						break;
					case ResponseMessage response when response.Header != null:
						response.Header.Response.ApplicationReceivedTime = Core.Now;
						Counters.IncrementReceivingTime(response.Header.Response.TotalTime);
						var evArgs2 = new ResponseReceivedEventArgs(_name, response, message.Body.Count);
						await OnResponseReceivedAsync(evArgs2).ConfigureAwait(false);
						break;
				}
				Counters.IncrementTotalMessagesProccesed();
			}
			catch (Exception ex)
			{
				Counters.IncrementTotalExceptions();
				Core.Log.Write(ex);
				lock (_lock)
					_exceptionSleep = true;
			}
		}
		#endregion
	}
}