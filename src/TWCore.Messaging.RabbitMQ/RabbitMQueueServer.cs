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
using System.Threading.Tasks;
using RabbitMQ.Client;
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
        private readonly NonBlocking.ConcurrentDictionary<string, RabbitMQueue> _rQueue = new NonBlocking.ConcurrentDictionary<string, RabbitMQueue>();
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
			if (senderOptions == null)
				throw new ArgumentNullException("ServerSenderOptions");
			_priority = (byte)(senderOptions.MessagePriority == MQMessagePriority.High ? 9 :
							senderOptions.MessagePriority == MQMessagePriority.Low ? 1 : 5);
			_expiration = (senderOptions.MessageExpirationInSec * 1000).ToString();
			_deliveryMode = (byte)(senderOptions.Recoverable ? 2 : 1);
			_label = senderOptions.Label;
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
        protected override Task<int> OnSendAsync(ResponseMessage message, RequestReceivedEventArgs e)
        {
            if (e.ResponseQueues?.Any() != true)
                return TaskHelper.CompleteValueMinus1;

            var correlationId = message.CorrelationId.ToString();
            var data = SenderSerializer.Serialize(message);
            var response = true;

            foreach (var queue in e.ResponseQueues)
            {
                try
                {
                    var rabbitQueue = _rQueue.GetOrAdd(queue.Route, q =>
                    {
                        var rq = new RabbitMQueue(queue);
                        rq.EnsureConnection();
                        return rq;
                    });
                    if (!rabbitQueue.EnsureConnection()) continue;
                    rabbitQueue.EnsureExchange();
                    var props = rabbitQueue.Channel.CreateBasicProperties();
                    props.CorrelationId = correlationId;
                    props.Priority = _priority;
                    props.Expiration = _expiration;
                    props.AppId = Core.ApplicationName;
                    props.ContentType = SenderSerializer.MimeTypes[0];
                    props.DeliveryMode = _deliveryMode;
                    props.Type = _label;
                    Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", data.Count, rabbitQueue.Route + "/" + queue.Name, correlationId);
                    rabbitQueue.Channel.BasicPublish(rabbitQueue.ExchangeName ?? string.Empty, queue.Name, props, (byte[])data);
                }
                catch (Exception ex)
                {
                    response = false;
                    Core.Log.Write(ex);
                }
            }
            return response ? Task.FromResult(data.Count) : TaskHelper.CompleteValueMinus1;
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