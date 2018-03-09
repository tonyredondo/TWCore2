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
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Server;
using TWCore.Threading;

namespace TWCore.Messaging
{

	/// <inheritdoc />
	/// <summary>
	/// Memory Queue Server Implementation
	/// </summary>
	public class MemoryQueueServer : MQueueServerBase
	{
		/// <inheritdoc />
		/// <summary>
		/// On Create all server listeners
		/// </summary>
		/// <param name="connection">Queue server listener</param>
		/// <param name="responseServer">true if the server is going to act as a response server</param>
		/// <returns>IMQueueServerListener</returns>
		protected override IMQueueServerListener OnCreateQueueServerListener(MQConnection connection, bool responseServer = false)
			=> new MemoryQueueServerListener(connection, this, responseServer);

		/// <inheritdoc />
		/// <summary>
		/// On Send message data
		/// </summary>
		/// <param name="message">Response message instance</param>
		/// <param name="e">Event Args</param>
		protected override Task<int> OnSendAsync(ResponseMessage message, RequestReceivedEventArgs e)
		{
			if (e.ResponseQueues?.Any() != true)
				return Task.FromResult(-1);
			var response = true;
			foreach (var queue in e.ResponseQueues)
			{
				try
				{
					var memQueue = MemoryQueueManager.GetQueue(queue.Route, queue.Name);
					Core.Log.LibVerbose("Enqueue message to the memory '{0}' with CorrelationId={1}", queue.Route + "/" + queue.Name, message.CorrelationId);
					memQueue.Enqueue(message.CorrelationId, message);
				}
				catch (Exception ex)
				{
					response = false;
					Core.Log.Write(ex);
				}
			}
			return Task.FromResult(response ? 1 : -1);
		}

		/// <inheritdoc />
		/// <summary>
		/// On Dispose
		/// </summary>
		protected override void OnDispose()
		{
		}


		#region Nested Classes
		private class MemoryQueueServerListener : MQueueServerListenerBase
		{
			private readonly Type _messageType;
			private readonly string _name;
			private MemoryQueue _receiver;
			private CancellationToken _token;
			private bool _cloneObject;

			#region .ctor

			/// <inheritdoc />
			/// <summary>
			/// Memory server listener implementation
			/// </summary>
			/// <param name="connection">Queue server listener</param>
			/// <param name="server">Message queue server instance</param>
			/// <param name="responseServer">true if the server is going to act as a response server</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal MemoryQueueServerListener(MQConnection connection, IMQueueServer server, bool responseServer) : base(
				connection, server, responseServer)
			{
				_messageType = responseServer ? typeof(ResponseMessage) : typeof(RequestMessage);
				_name = server.Name;
				_cloneObject = false;

                if (server.Config?.RequestOptions?.ServerReceiverOptions?.Parameters?.Contains("Clone") == true)
                    _cloneObject = server.Config.RequestOptions.ServerReceiverOptions.Parameters["Clone"].ParseTo(false);

                Core.Status.Attach(collection =>
				{
					collection.Add("Message Type", _messageType);
				});
			}

			#endregion

			/// <inheritdoc />
			/// <summary>
			/// Start the queue listener for request messages
			/// </summary>
			/// <param name="token">Cancellation token</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected override async Task OnListenerTaskStartAsync(CancellationToken token)
			{
				_token = token;
				_receiver = MemoryQueueManager.GetQueue(Connection.Route, Connection.Name);
				var tokenTask = token.WhenCanceledAsync();
				while (!_token.IsCancellationRequested)
				{
					var rcvTask = _receiver.DequeueAsync(_token);
					var rTask = await Task.WhenAny(rcvTask, tokenTask).ConfigureAwait(false);
					if (rTask == tokenTask) break;
                    #pragma warning disable 4014
                    EnqueueMessageToProcessAsync(ProcessingTaskAsync, rcvTask.Result);
					#pragma warning restore 4014
				}
                Core.Log.InfoDetail("Listener stopped, waiting to finalize al processing messages.");
				await WorkerEvent.WaitAsync(TimeSpan.FromSeconds(Config.RequestOptions.ServerReceiverOptions.ProcessingWaitOnFinalizeInSec)).ConfigureAwait(false);
            }

            /// <inheritdoc />
            /// <summary>
            /// On Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected override void OnDispose()
			{
			}


			private async Task ProcessingTaskAsync(MemoryQueue.Message message)
			{
                if (message == null) return;
				try
				{
					Counters.IncrementProcessingThreads();
					Core.Log.LibVerbose("Received message from the memory queue '{0}/{1}'", Connection.Route, Connection.Name);
					var messageBody = message.Value;
					switch (messageBody)
					{
						case RequestMessage request when request.Header != null:
							if (_cloneObject)
								request.Body = request.Body.DeepClone();
							request.Header.ApplicationReceivedTime = Core.Now;
							Counters.IncrementReceivingTime(request.Header.TotalTime);
							if (request.Header.ClientName != Config.Name)
								Core.Log.Warning("The Message Client Name '{0}' is different from the Server Name '{1}'", request.Header.ClientName, Config.Name);
							var evArgs = new RequestReceivedEventArgs(_name, Connection, request, 1);
							if (request.Header.ResponseQueue != null)
								evArgs.ResponseQueues.Add(request.Header.ResponseQueue);
							await OnRequestReceivedAsync(evArgs).ConfigureAwait(false);
							break;
						case ResponseMessage response when response.Header != null:
							if (_cloneObject)
								response.Body = response.Body.DeepClone();
							response.Header.Response.ApplicationReceivedTime = Core.Now;
							Counters.IncrementReceivingTime(response.Header.Response.TotalTime);
							await OnResponseReceivedAsync(new ResponseReceivedEventArgs(_name, response, 1)).ConfigureAwait(false);
							break;
					}

					Counters.IncrementTotalMessagesProccesed();
				}
				catch (Exception ex)
				{
					Counters.IncrementTotalExceptions();
					Core.Log.Write(ex);
				}
				finally
				{
					Counters.DecrementProcessingThreads();
				}
			}

		}
		#endregion
	}
}