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
using NsqSharp;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Server;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.NSQ
{
	/// <inheritdoc />
	/// <summary>
	/// NSQ Server Implementation
	/// </summary>
	public class NSQueueServer : MQueueServerBase
	{
		private readonly NonBlocking.ConcurrentDictionary<string, ObjectPool<Producer>> _rQueue = new NonBlocking.ConcurrentDictionary<string, ObjectPool<Producer>>();

		#region .ctor
		/// <summary>
		/// NSQ Server Implementation
		/// </summary>
		public NSQueueServer()
		{
			System.Net.ServicePointManager.DefaultConnectionLimit = 500;
		}
		#endregion

		/// <inheritdoc />
		/// <summary>
		/// On Create all server listeners
		/// </summary>
		/// <param name="connection">Queue server listener</param>
		/// <param name="responseServer">true if the server is going to act as a response server</param>
		/// <returns>IMQueueServerListener</returns>
		protected override IMQueueServerListener OnCreateQueueServerListener(MQConnection connection, bool responseServer = false)
			=> new NSQueueServerListener(connection, this, responseServer);

		/// <inheritdoc />
		/// <summary>
		/// On Send message data
		/// </summary>
		/// <param name="message">Response message instance</param>
		/// <param name="e">Event Args</param>
		protected override async Task<int> OnSendAsync(ResponseMessage message, RequestReceivedEventArgs e)
		{
			if (e.ResponseQueues?.Any() != true)
				return -1;

			var senderOptions = Config.ResponseOptions.ServerSenderOptions;
			if (senderOptions == null)
				throw new ArgumentNullException("ServerSenderOptions");

			var data = SenderSerializer.Serialize(message);
            var body = NSQueueClient.CreateMessageBody(data, message.CorrelationId);

			var response = true;
			foreach (var queue in e.ResponseQueues)
			{
				try
				{
                    var nsqProducerPool = _rQueue.GetOrAdd(queue.Route, q => new ObjectPool<Producer>(pool =>
                    {
                        Core.Log.LibVerbose("New Producer from QueueServer");
                        return new Producer(q);
                    }, null, 1));
					Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", data.Count, queue.Route + "/" + queue.Name, message.CorrelationId);
                    var nsqProducer = nsqProducerPool.New();
				    await nsqProducer.PublishAsync(queue.Name, body).ConfigureAwait(false);
                    nsqProducerPool.Store(nsqProducer);
				}
				catch (Exception ex)
				{
					response = false;
					Core.Log.Write(ex);
				}
			}
			return response ? data.Count : -1;
		}

		/// <inheritdoc />
		/// <summary>
		/// On Dispose
		/// </summary>
		protected override void OnDispose()
		{
            var producers = _rQueue.SelectMany(i => i.Value.GetCurrentObjects()).ToArray();
            Parallel.ForEach(producers, p => p.Stop());
            foreach (var sender in _rQueue)
                sender.Value.Clear();
            _rQueue.Clear();
		}
	}
}