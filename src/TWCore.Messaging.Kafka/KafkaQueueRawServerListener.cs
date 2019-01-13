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

using KafkaNet;
using KafkaNet.Model;
using KafkaNet.Protocol;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.RawServer;
// ReSharper disable InconsistentNaming
// ReSharper disable MethodSupportsCancellation
#pragma warning disable CS4014 // Because a call is not awaited

namespace TWCore.Messaging.Kafka
{
    /// <inheritdoc />
    /// <summary>
    /// Kafka raw server listener implementation
    /// </summary>
    public class KafkaQueueRawServerListener : MQueueRawServerListenerBase
    {
        #region Fields
        private readonly string _name;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Kafka raw server listener implementation
        /// </summary>
        /// <param name="connection">Queue server listener</param>
        /// <param name="server">Message queue server instance</param>
        /// <param name="responseServer">true if the server is going to act as a response server</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KafkaQueueRawServerListener(MQConnection connection, IMQueueRawServer server, bool responseServer) : base(connection, server, responseServer)
        {
            _name = server.Name;
        }
        #endregion

        #region Override Methods
        /// <inheritdoc />
        /// <summary>
        /// Start the queue listener for request messages
        /// </summary>
        /// <param name="token">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override async Task OnListenerTaskStartAsync(CancellationToken token)
        {
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            _token = _tokenSource.Token;
            if (string.IsNullOrEmpty(Connection.Route))
                throw new UriFormatException($"The route for the connection to {Connection.Name} is null.");

            await Task.Factory.StartNew(() =>
            {
                var options = new KafkaOptions(new Uri(Connection.Route));
                var router = new BrokerRouter(options);
                var consumer = Extensions.InvokeWithRetry(() => new Consumer(new ConsumerOptions(Connection.Name, router)), 5000, int.MaxValue).WaitAsync();
                using (consumer)
                {
                    foreach (var cRes in consumer.Consume(_token))
                    {
                        if (_token.IsCancellationRequested) break;
#if COMPATIBILITY
                        Task.Run(() => EnqueueMessageToProcessAsync(ProcessingTaskAsync, cRes));
#else
                ThreadPool.QueueUserWorkItem(async item =>
                {
                    await EnqueueMessageToProcessAsync(ProcessingTaskAsync, item);
                }, cRes, false);
#endif
                    }
                }
            }, _token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
            OnDispose();
        }
        /// <inheritdoc />
        /// <summary>
        /// On Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnDispose()
        {
            if (_tokenSource is null) return;
            _tokenSource.Cancel();
            _tokenSource = null;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Process a received message from the queue
        /// </summary>
        /// <param name="message">Message instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task ProcessingTaskAsync(Message message)
        {
            try
            {
                var (correlationId, name) = KafkaQueueRawClient.GetFromRawMessageHeader(message.Key);
                var body = message.Value;

                Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}/{2}'", body.Length, Connection.Route, Connection.Name);
                Counters.IncrementTotalReceivingBytes(body.Length);

                if (ResponseServer)
                {
                    var evArgs =
                        new RawResponseReceivedEventArgs(_name, body, correlationId, body.Length)
                        {
                            Metadata =
                            {
                                ["ReplyTo"] = name
                            }
                        };
                    await OnResponseReceivedAsync(evArgs).ConfigureAwait(false);
                }
                else
                {
                    var evArgs =
                        new RawRequestReceivedEventArgs(_name, Connection, body, correlationId, body.Length)
                        {
                            Metadata =
                            {
                                ["ReplyTo"] = name
                            }
                        };
                    await OnRequestReceivedAsync(evArgs).ConfigureAwait(false);
                }
                Counters.IncrementTotalMessagesProccesed();
            }
            catch (Exception ex)
            {
                Counters.IncrementTotalExceptions();
                Core.Log.Write(ex);
            }
        }
        #endregion
    }
}
