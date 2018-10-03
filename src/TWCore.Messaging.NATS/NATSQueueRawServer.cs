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

using NATS.Client;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.RawServer;
using TWCore.Threading;

// ReSharper disable InconsistentNaming


namespace TWCore.Messaging.NATS
{
    /// <inheritdoc />
    /// <summary>
    /// NATS Raw Server Implementation
    /// </summary>
    public class NATSQueueRawServer : MQueueRawServerBase
    {
        private readonly ConcurrentDictionary<string, IConnection> _rQueue = new ConcurrentDictionary<string, IConnection>();
        private readonly ConnectionFactory _factory;

        #region .ctor
        /// <summary>
        /// NATS Raw Server Implementation
        /// </summary>
        public NATSQueueRawServer()
        {
            _factory = new ConnectionFactory();
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// On Create all server listeners
        /// </summary>
        /// <param name="connection">Queue server listener</param>
        /// <param name="responseServer">true if the server is going to act as a response server</param>
        /// <returns>IMQueueServerListener</returns>
        protected override IMQueueRawServerListener OnCreateQueueServerListener(MQConnection connection, bool responseServer = false)
            => new NATSQueueRawServerListener(connection, this, responseServer);

        /// <inheritdoc />
        /// <summary>
        /// On Send message data
        /// </summary>
        /// <param name="message">Response message instance</param>
        /// <param name="e">Event Args</param>
        protected override Task<int> OnSendAsync(MultiArray<byte> message, RawRequestReceivedEventArgs e)
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

            var body = NATSQueueRawClient.CreateRawMessageBody(message, e.CorrelationId, e.Metadata["ReplyTo"]);
            var replyTo = e.Metadata["ReplyTo"];

            var response = true;
            foreach (var queue in e.ResponseQueues)
            {
                try
                {
                    var producer = _rQueue.GetOrAdd(queue.Route, qRoute => 
                    {
                        Core.Log.LibVerbose("New Producer from QueueServer");
                        IConnection connection = null;
                        Extensions.InvokeWithRetry(() =>
                        {
                            connection = _factory.CreateConnection(qRoute);
                        }, 5000, int.MaxValue).WaitAsync();
                        return connection;
                    });

                    if (!string.IsNullOrEmpty(replyTo))
                    {
                        if (string.IsNullOrEmpty(queue.Name))
                        {
                            Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", body.Length, queue.Route + "/" + replyTo, e.CorrelationId);
                            producer.Publish(replyTo, body);
                        }
                        else if (queue.Name.StartsWith(replyTo, StringComparison.Ordinal))
                        {
                            Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", body.Length, queue.Route + "/" + queue.Name + "_" + replyTo, e.CorrelationId);
                            producer.Publish(queue.Name + "_" + replyTo, body);
                        }
                    }
                    else
                    {
                        Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}' with CorrelationId={2}", body.Length, queue.Route + "/" + queue.Name, e.CorrelationId);
                        producer.Publish(queue.Name, body);
                    }
                }
                catch (Exception ex)
                {
                    response = false;
                    Core.Log.Write(ex);
                }
            }
            return response ? Task.FromResult(message.Count) : TaskHelper.CompleteValueMinus1;
        }

        /// <inheritdoc />
        /// <summary>
        /// On Dispose
        /// </summary>
        protected override void OnDispose()
        {
            var producers = _rQueue.Select(i => i.Value).ToArray();
            Parallel.ForEach(producers, p => p.Close());
            _rQueue.Clear();
        }
    }
}
