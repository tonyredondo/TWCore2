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
using NsqSharp;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Server;

namespace TWCore.Messaging.NSQ
{
	/// <summary>
	/// NSQ Server Implementation
	/// </summary>
	public class NSQueueServer : MQueueServerBase
	{
        readonly ConcurrentDictionary<string, ObjectPool<Producer>> rQueue = new ConcurrentDictionary<string, ObjectPool<Producer>>();

		#region .ctor
		/// <summary>
		/// NSQ Server Implementation
		/// </summary>
		public NSQueueServer()
		{
			System.Net.ServicePointManager.DefaultConnectionLimit = 200;
		}
		#endregion

		/// <summary>
		/// On Create all server listeners
		/// </summary>
		/// <param name="connection">Queue server listener</param>
		/// <param name="responseServer">true if the server is going to act as a response server</param>
		/// <returns>IMQueueServerListener</returns>
		protected override IMQueueServerListener OnCreateQueueServerListener(MQConnection connection, bool responseServer = false)
			=> new NSQueueServerListener(connection, this, responseServer);

		/// <summary>
		/// On Send message data
		/// </summary>
		/// <param name="message">Response message instance</param>
		/// <param name="e">Event Args</param>
		protected override bool OnSend(ResponseMessage message, RequestReceivedEventArgs e)
		{
			if (e.ResponseQueues?.Any() != true)
				return false;

			var SenderOptions = Config.ResponseOptions.ServerSenderOptions;
			if (SenderOptions == null)
				throw new ArgumentNullException("ServerSenderOptions");

			var data = SenderSerializer.Serialize(message);
			var body = new byte[data.Count + 16];
			Buffer.BlockCopy(message.CorrelationId.ToByteArray(), 0, body, 0, 16);
			data.CopyTo(body, 16);

			bool response = true;
			foreach (var queue in e.ResponseQueues)
			{
				try
				{
                    var nsqProducerPool = rQueue.GetOrAdd(queue.Route, q => new ObjectPool<Producer>(() =>
                    {
                        Core.Log.LibVerbose("New Producer from QueueServer");
                        return new Producer(q);
                    }, null, 1));
					Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", data.Count, queue.Route + "/" + queue.Name, message.CorrelationId);
                    var nsqProducer = nsqProducerPool.New();
                    nsqProducer.PublishAsync(queue.Name, body).Wait();
                    nsqProducerPool.Store(nsqProducer);
				}
				catch (Exception ex)
				{
					response = false;
					Core.Log.Write(ex);
				}
			}
			return response;
		}

		/// <summary>
		/// On Dispose
		/// </summary>
		protected override void OnDispose()
		{
            foreach (var q in rQueue)
            {
                foreach (var i in q.Value.GetCurrentObjects())
                    i.Stop();
                q.Value.Clear();
            }
            rQueue.Clear();
		}
	}
}