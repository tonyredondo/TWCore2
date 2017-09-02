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
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NsqSharp;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Server;

namespace TWCore.Messaging.NSQ
{
	/// <summary>
	/// NSQ server listener implementation
	/// </summary>
	public class NSQueueServerListener : MQueueServerListenerBase
	{
		#region Fields
		readonly ConcurrentDictionary<Task, object> _processingTasks = new ConcurrentDictionary<Task, object>();
		readonly object _lock = new object();
		Type _messageType;
		string _name;
		Consumer _receiver;
		CancellationToken _token;
		Task _monitorTask;
		bool _exceptionSleep = false;
		#endregion

		#region Nested Type
		class NSQMessage
		{
			public Guid CorrelationId;
			public SubArray<byte> Body;
		}
        class NSQMessageHandler : IHandler
        {
            NSQueueServerListener _listener;
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
                    var rMsg = new NSQMessage()
                    {
                        CorrelationId = correlationId,
                        Body = body
                    };
                    Try.Do(message.Finish, false);


                    _listener.Counters.IncrementMessages();
                    var tsk = Task.Factory.StartNew(_listener.ProcessingTask, rMsg, _listener._token);
                    _listener._processingTasks.TryAdd(tsk, null);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    tsk.ContinueWith(_tsk =>
                    {
                        _listener._processingTasks.TryRemove(tsk, out var ts);
                        _listener.Counters.DecrementMessages();
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
            _receiver.ConnectToNsqd(Connection.Route);
            _monitorTask = Task.Run(MonitorProcess, _token);
			await token.WhenCanceledAsync().ConfigureAwait(false);
			OnDispose();
			Task[] tasksToWait;
			lock (_lock)
				tasksToWait = _processingTasks.Keys.Concat(_monitorTask).ToArray();
			if (tasksToWait.Length > 0)
				Task.WaitAll(tasksToWait, TimeSpan.FromSeconds(Config.RequestOptions.ServerReceiverOptions.ProcessingWaitOnFinalizeInSec));
		}
		/// <summary>
		/// On Dispose
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override void OnDispose()
		{
			if (_receiver != null)
			{
				try
				{
					_receiver.Stop();
				}
				catch { }
				_receiver = null;
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Monitors the maximum concurrent message allowed for the listener
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		async Task MonitorProcess()
		{
			while (!_token.IsCancellationRequested)
			{
				try
				{
					bool exSleep = false;
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
		/// <param name="obj">Object message instance</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void ProcessingTask(object obj)
		{
			try
			{
				Counters.IncrementProcessingThreads();
				if (obj is NSQMessage message)
				{
					Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}'", message.Body.Count, Connection.Route + "/" + Connection.Name);
					var messageBody = ReceiverSerializer.Deserialize(message.Body, _messageType);
					if (messageBody is RequestMessage request && request.Header != null)
					{
						request.Header.ApplicationReceivedTime = Core.Now;
						Counters.IncrementReceivingTime(request.Header.TotalTime);
						if (request.Header.ClientName != Config.Name)
							Core.Log.Warning("The Message Client Name '{0}' is different from the Server Name '{1}'", request.Header.ClientName, Config.Name);
						var evArgs = new RequestReceivedEventArgs(_name, Connection, request);
						if (request.Header.ResponseQueue != null)
							evArgs.ResponseQueues.Add(request.Header.ResponseQueue);
						OnRequestReceived(evArgs);
					}
					else if (messageBody is ResponseMessage response && response.Header != null)
					{
						response.Header.Response.ApplicationReceivedTime = Core.Now;
						Counters.IncrementReceivingTime(response.Header.Response.TotalTime);
						var evArgs = new ResponseReceivedEventArgs(_name, response);
						OnResponseReceived(evArgs);
					}
					Counters.IncrementTotalMessagesProccesed();
				}
			}
			catch (Exception ex)
			{
				Counters.IncrementTotalExceptions();
				Core.Log.Write(ex);
				lock (_lock)
					_exceptionSleep = true;
			}
			finally
			{
				Counters.DecrementProcessingThreads();
			}
		}
		#endregion
	}
}