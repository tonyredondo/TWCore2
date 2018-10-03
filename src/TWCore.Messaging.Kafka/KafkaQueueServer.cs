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

using Confluent.Kafka;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Server;
using TWCore.Threading;

// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.Kafka
{
    /// <inheritdoc />
    /// <summary>
    /// Kafka Server Implementation
    /// </summary>
    public class KafkaQueueServer : MQueueServerBase
    {
        private readonly ConcurrentDictionary<string, Producer<byte[], byte[]>> _rQueue = new ConcurrentDictionary<string, Producer<byte[], byte[]>>();

        /// <inheritdoc />
        /// <summary>
        /// On Create all server listeners
        /// </summary>
        /// <param name="connection">Queue server listener</param>
        /// <param name="responseServer">true if the server is going to act as a response server</param>
        /// <returns>IMQueueServerListener</returns>
        protected override IMQueueServerListener OnCreateQueueServerListener(MQConnection connection, bool responseServer = false)
            => new KafkaQueueServerListener(connection, this, responseServer);

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
            if (senderOptions is null)
                throw new NullReferenceException("ServerSenderOptions is null.");

            var data = SenderSerializer.Serialize(message);

            var response = true;
            foreach (var queue in e.ResponseQueues)
            {
                try
                {
                    var producer = _rQueue.GetOrAdd(queue.Route, qRoute =>
                    {
                        Core.Log.LibVerbose("New Producer from QueueServer");
                        Producer<byte[], byte[]> connection = null;
                        if (string.IsNullOrEmpty(queue.Route))
                            throw new UriFormatException($"The route for the connection to {queue.Name} is null.");
                        var config = new ProducerConfig { BootstrapServers = qRoute };
                        Extensions.InvokeWithRetry(() =>
                        {
                            connection = new Producer<byte[], byte[]>(config);
                        }, 5000, int.MaxValue).WaitAsync();
                        return connection;
                    });
                    Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", data.Count, queue.Route + "/" + queue.Name, message.CorrelationId);
                    await producer.ProduceAsync(queue.Name, new Message<byte[], byte[]> { Key = message.CorrelationId.ToByteArray(), Value = data.AsArray() }).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    response = false;
                    Core.Log.Write(ex);
                }
            }
            return response ? data.Count : -1;
        }
    }
}
