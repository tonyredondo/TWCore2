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

using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Server;
using TWCore.Threading;

namespace TWCore.Messaging.RabbitMQ
{
    /// <inheritdoc />
    /// <summary>
    /// RabbitMQ Server Implementation
    /// </summary>
    public class RabbitMQueueServer : MQueueServerBase
    {
        private readonly ConcurrentDictionary<(string, string), RabbitMQueue> _rQueue = new ConcurrentDictionary<(string, string), RabbitMQueue>();
		private byte _priority;
		private byte _deliveryMode;
		private string _expiration;
		private string _label;

		/// <inheritdoc />
		/// <summary>
		/// On client initialization
		/// </summary>
		protected override void OnInit()
		{
			base.OnInit();
			var senderOptions = Config.ResponseOptions.ServerSenderOptions;
			if (senderOptions is null)
				throw new NullReferenceException("ServerSenderOptions is null.");
			_priority = (byte)(senderOptions.MessagePriority == MQMessagePriority.High ? 9 :
							senderOptions.MessagePriority == MQMessagePriority.Low ? 1 : 5);
			_expiration = (senderOptions.MessageExpirationInSec * 1000).ToString();
			_deliveryMode = (byte)(senderOptions.Recoverable ? 2 : 1);
			_label = senderOptions.Label ?? string.Empty;
		}

        /// <inheritdoc />
        /// <summary>
        /// On Create all server listeners
        /// </summary>
        /// <param name="connection">Queue server listener</param>
        /// <param name="responseServer">true if the server is going to act as a response server</param>
        /// <returns>IMQueueServerListener</returns>
        protected override IMQueueServerListener OnCreateQueueServerListener(MQConnection connection, bool responseServer = false)
            => new RabbitMQueueServerListener(connection, this, responseServer);

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

            var correlationId = message.CorrelationId.ToString();
            var data = SenderSerializer.Serialize(message);

            if (e.ResponseQueues.Count == 1)
                await SendTaskAsync(e.ResponseQueues[0]).ConfigureAwait(false);
            else
            {
                var tsk = new Task[e.ResponseQueues.Count];
                for (var i = 0; i < tsk.Length; i++)
                    tsk[i] = SendTaskAsync(e.ResponseQueues[i]);
                await Task.WhenAny(tsk).ConfigureAwait(false);
            }

            var count = data.Count;
            data.ReturnContentToPoolAndDispose();
            return count;

            async Task SendTaskAsync(MQConnection queue)
            {
                try
                {
                    var rabbitQueue = _rQueue.GetOrAdd((queue.Route, queue.Name), q => new RabbitMQueue(queue));
                    if (await rabbitQueue.EnsureConnectionAsync(1000, 2).ConfigureAwait(false))
                    {
                        rabbitQueue.EnsureExchange();
                        var props = rabbitQueue.Channel.CreateBasicProperties();
                        props.CorrelationId = correlationId;
                        props.Priority = _priority;
                        props.Expiration = _expiration;
                        props.AppId = Core.ApplicationName;
                        props.ContentType = SenderSerializer.MimeTypes[0];
                        props.DeliveryMode = _deliveryMode;
                        props.Type = _label;
                        Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}/{2}' with CorrelationId={3}", data.Count, rabbitQueue.Route, queue.Name, correlationId);
                        rabbitQueue.Channel.BasicPublish(rabbitQueue.ExchangeName ?? string.Empty, queue.Name, props, data.AsArray());
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// On Dispose
        /// </summary>
        protected override void OnDispose()
        {
			foreach (var queue in _rQueue.Values)
				queue.Close();
			_rQueue.Clear();
		}
    }
}