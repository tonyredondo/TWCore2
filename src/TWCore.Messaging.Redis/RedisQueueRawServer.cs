﻿/*
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

using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.RawServer;
using TWCore.Threading;

// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.Redis
{
    /// <inheritdoc />
    /// <summary>
    /// Redis raw Server Implementation
    /// </summary>
    public class RedisQueueRawServer : MQueueRawServerBase
    {
        private readonly ConcurrentDictionary<string, RedisMQConnection> _rQueue = new ConcurrentDictionary<string, RedisMQConnection>();

        /// <inheritdoc />
        /// <summary>
        /// On Create all server listeners
        /// </summary>
        /// <param name="connection">Queue server listener</param>
        /// <param name="responseServer">true if the server is going to act as a response server</param>
        /// <returns>IMQueueServerListener</returns>
        protected override IMQueueRawServerListener OnCreateQueueServerListener(MQConnection connection, bool responseServer = false)
            => new RedisQueueRawServerListener(connection, this, responseServer);

        /// <inheritdoc />
        /// <summary>
        /// On Send message data
        /// </summary>
        /// <param name="message">Response message instance</param>
        /// <param name="e">Event Args</param>
        protected override async Task<int> OnSendAsync(MultiArray<byte> message, RawRequestReceivedEventArgs e)
        {
            var queues = e.ResponseQueues;
            var replyTo = e.Metadata["ReplyTo"] ?? string.Empty;

            queues.Add(new MQConnection
            {
                Route = e.Sender.Route,
                Parameters = e.Sender.Parameters
            });

            var senderOptions = Config.ResponseOptions.ServerSenderOptions;
            if (senderOptions is null)
                throw new NullReferenceException("ServerSenderOptions is null.");

            var body = RedisQueueRawClient.CreateRawMessageBody(message, e.CorrelationId, replyTo);

            var response = true;
            foreach (var queue in e.ResponseQueues)
            {
                try
                {
                    var producer = _rQueue.GetOrAdd(queue.Route, _ => new RedisMQConnection(queue));
                    var subscriber = await producer.GetSubscriberAsync().ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(replyTo))
                    {
                        if (string.IsNullOrEmpty(queue.Name))
                        {
                            Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}/{2}' with CorrelationId={3}", body.Length, queue.Route, replyTo, e.CorrelationId);
                            await subscriber.PublishAsync(RedisChannel.Literal(replyTo), body).ConfigureAwait(false);

                        }
                        else if (queue.Name.StartsWith(replyTo, StringComparison.Ordinal))
                        {
                            var nName = queue.Name + "_" + replyTo;
                            Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}/{2}' with CorrelationId={3}", body.Length, queue.Route, nName, e.CorrelationId);
                            await subscriber.PublishAsync(RedisChannel.Literal(nName), body).ConfigureAwait(false);

                        }
                    }
                    else
                    {
                        Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}/{2}' with CorrelationId={3}", body.Length, queue.Route, queue.Name, e.CorrelationId);
                        await subscriber.PublishAsync(RedisChannel.Literal(queue.Name), body).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    response = false;
                    Core.Log.Write(ex);
                }
            }
            return response ? message.Count : -1;
        }

        /// <inheritdoc />
        /// <summary>
        /// On Dispose
        /// </summary>
        protected override void OnDispose()
        {
            _rQueue.Clear();
        }
    }
}
