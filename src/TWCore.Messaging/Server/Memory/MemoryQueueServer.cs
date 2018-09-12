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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Server;
using TWCore.Threading;
// ReSharper disable CheckNamespace
// ReSharper disable MethodSupportsCancellation

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
				return TaskHelper.CompleteValueMinus1;
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
		    return response ? TaskHelper.CompleteValuePlus1 : TaskHelper.CompleteValueMinus1;
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
		    private readonly string _name;
			private MemoryQueue _receiver;
			private CancellationToken _token;

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
			    var messageType = responseServer ? typeof(ResponseMessage) : typeof(RequestMessage);
				_name = server.Name;

                Core.Status.Attach(collection =>
				{
					collection.Add("Message Type", messageType);
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
				while (!_token.IsCancellationRequested)
				{
					var rcvValue = await _receiver.DequeueAsync(_token).ConfigureAwait(false);
                    if (_token.IsCancellationRequested) break;
                    #pragma warning disable 4014
                    Task.Run(() => EnqueueMessageToProcessAsync(ProcessingTaskAsync, rcvValue));
					#pragma warning restore 4014
				}
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
                if (message is null) return;
				try
				{
					Core.Log.LibVerbose("Received message from the memory queue '{0}/{1}'", Connection.Route, Connection.Name);
					var messageBody = message.Value;
					switch (messageBody)
					{
						case RequestMessage request when request.Header != null:
							request.Header.ApplicationReceivedTime = Core.Now;
							Counters.IncrementReceivingTime(request.Header.TotalTime);
							if (request.Header.ClientName != Config.Name)
								Core.Log.Warning("The Message Client Name '{0}' is different from the Server Name '{1}'", request.Header.ClientName, Config.Name);
							var evArgs = new RequestReceivedEventArgs(_name, Connection, request, 1, SenderSerializer);
							if (request.Header.ResponseQueue != null)
								evArgs.ResponseQueues.Add(request.Header.ResponseQueue);
							await OnRequestReceivedAsync(evArgs).ConfigureAwait(false);
							break;
						case ResponseMessage response when response.Header != null:
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
			}

		}
		#endregion
	}
}