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

using NsqSharp;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.RawServer;
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.NSQ
{
    /// <inheritdoc />
    /// <summary>
    /// NSQ Raw Server Implementation
    /// </summary>
    public class NSQueueRawServer : MQueueRawServerBase
    {
        private readonly ConcurrentDictionary<string, Producer> _rQueue = new ConcurrentDictionary<string, Producer>();

        /// <inheritdoc />
        /// <summary>
        /// On Create all server listeners
        /// </summary>
        /// <param name="connection">Queue server listener</param>
        /// <param name="responseServer">true if the server is going to act as a response server</param>
        /// <returns>IMQueueServerListener</returns>
        protected override IMQueueRawServerListener OnCreateQueueServerListener(MQConnection connection, bool responseServer = false)
            => new NSQueueRawServerListener(connection, this, responseServer);

        /// <inheritdoc />
        /// <summary>
        /// On Send message data
        /// </summary>
        /// <param name="message">Response message instance</param>
        /// <param name="e">Event Args</param>
        protected override async Task<int> OnSendAsync(MultiArray<byte> message, RawRequestReceivedEventArgs e)
        {
            var queues = e.ResponseQueues;
            queues.Add(new MQConnection
            {
                Route = e.Sender.Route,
                Parameters = e.Sender.Parameters
            });

            var senderOptions = Config.ResponseOptions.ServerSenderOptions;
            if (senderOptions is null)
                throw new NullReferenceException("ServerSenderOptions is null.");

            var body = NSQueueRawClient.CreateRawMessageBody(message, e.CorrelationId, e.Metadata["ReplyTo"]);
            var replyTo = e.Metadata["ReplyTo"];

            var response = true;
            foreach (var queue in e.ResponseQueues)
            {
                try
                {
                    var nsqProducer = _rQueue.GetOrAdd(queue.Route, qRoute =>
                    {
                        Core.Log.LibVerbose("New Producer from RawQueueServer");
                        return new Producer(qRoute);
                    });

                    if (!string.IsNullOrEmpty(replyTo))
                    {
                        if (string.IsNullOrEmpty(queue.Name))
                        {
                            Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", body.Length, queue.Route + "/" + replyTo, e.CorrelationId);
                            await nsqProducer.PublishAsync(replyTo, body).ConfigureAwait(false);
                        }
                        else if (queue.Name.StartsWith(replyTo, StringComparison.Ordinal))
                        {
                            Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", body.Length, queue.Route + "/" + queue.Name + "_" + replyTo, e.CorrelationId);
                            await nsqProducer.PublishAsync(queue.Name + "_" + replyTo, body).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", body.Length, queue.Route + "/" + queue.Name, e.CorrelationId);
                        await nsqProducer.PublishAsync(queue.Name, body).ConfigureAwait(false);
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
            var producers = _rQueue.Select(i => i.Value).ToArray();
            Parallel.ForEach(producers, p => p.Stop());
            _rQueue.Clear();
        }
    }
}