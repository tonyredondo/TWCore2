﻿/*
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
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Server;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace TWCore.Messaging.RabbitMQ
{
    /// <summary>
    /// RabbitMQ Server Implementation
    /// </summary>
    public class RabbitMQueueServer : MQueueServerBase
    {
        ConcurrentDictionary<string, RabbitMQueue> rQueue = new ConcurrentDictionary<string, RabbitMQueue>();

        /// <summary>
        /// On Create all server listeners
        /// </summary>
        /// <param name="connection">Queue server listener</param>
        /// <param name="responseServer">true if the server is going to act as a response server</param>
        /// <returns>IMQueueServerListener</returns>
        protected override IMQueueServerListener OnCreateQueueServerListener(MQConnection connection, bool responseServer = false)
            => new RabbitMQueueServerListener(connection, this, responseServer);

        /// <summary>
        /// On Send message data
        /// </summary>
        /// <param name="message">Response message instance</param>
        /// <param name="queues">Response queues</param>
        protected override bool OnSend(ResponseMessage message, List<MQConnection> queues)
        {
            if (queues?.Any() == false)
                return false;

            var SenderOptions = Config.ResponseOptions.ServerSenderOptions;
            if (SenderOptions == null)
                throw new ArgumentNullException("ServerSenderOptions");

            var correlationId = message.CorrelationId.ToString();
            var data = SenderSerializer.Serialize(message);
            var priority = (byte)(SenderOptions.MessagePriority == MQMessagePriority.High ? 9 :
                            SenderOptions.MessagePriority == MQMessagePriority.Low ? 1 : 5);
            var expiration = (SenderOptions.MessageExpirationInSec * 1000).ToString();
            var deliveryMode = (byte)(SenderOptions.Recoverable ? 2 : 1);
            bool response = true;
            foreach (var queue in queues)
            {
                try
                {
                    var rabbitQueue = rQueue.GetOrAdd(queue.Route, q =>
                    {
                        var rq = new RabbitMQueue(queue);
                        rq.EnsureConnection();
                        return rq;
                    });
                    if (!rabbitQueue.EnsureConnection()) continue;
                    else rabbitQueue.EnsureExchange();
                    var props = rabbitQueue.Channel.CreateBasicProperties();
                    props.CorrelationId = correlationId;
                    props.Priority = priority;
                    props.Expiration = expiration;
                    props.AppId = Core.ApplicationName;
                    props.ContentType = SenderSerializer.MimeTypes[0];
                    props.DeliveryMode = deliveryMode;
                    props.Type = SenderOptions.Label;
                    Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", data.Count, rabbitQueue.Route + "/" + queue.Name, correlationId);
                    rabbitQueue.Channel.BasicPublish(rabbitQueue.ExchangeName ?? string.Empty, queue.Name, props, (byte[])data);
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
            if (rQueue != null)
            {
                foreach (var queue in rQueue.Values)
                    queue.Close();
                rQueue.Clear();
                rQueue = null;
            }
        }
    }
}