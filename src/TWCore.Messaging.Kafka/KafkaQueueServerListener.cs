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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Server;
// ReSharper disable InconsistentNaming
// ReSharper disable MethodSupportsCancellation
#pragma warning disable 414
#pragma warning disable CS4014 // Because a call is not awaited

namespace TWCore.Messaging.Kafka
{
    /// <inheritdoc />
    /// <summary>
    /// Kafka server listener implementation
    /// </summary>
    public class KafkaQueueServerListener : MQueueServerListenerBase
    {
        #region Fields
        private readonly Type _messageType;
        private readonly string _name;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Kafka server listener implementation
        /// </summary>
        /// <param name="connection">Queue server listener</param>
        /// <param name="server">Message queue server instance</param>
        /// <param name="responseServer">true if the server is going to act as a response server</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KafkaQueueServerListener(MQConnection connection, IMQueueServer server, bool responseServer) : base(connection, server, responseServer)
        {
            _messageType = responseServer ? typeof(ResponseMessage) : typeof(RequestMessage);
            _name = server.Name;
            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(_messageType), _messageType);
            });
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
                var config = new ConsumerConfig
                {
                    BootstrapServers = Connection.Route,
                    GroupId = Connection.Name + "Consumer",
                    EnableAutoCommit = true,
                    StatisticsIntervalMs = 0,
                    SessionTimeoutMs = 6000,
                    AutoOffsetReset = AutoOffsetResetType.Latest
                };

                Consumer<byte[], byte[]> consumer = null;
                Extensions.InvokeWithRetry(() =>
                {
                    consumer = new Consumer<byte[], byte[]>(config);
                    consumer.Subscribe(Connection.Name);
                }, 5000, int.MaxValue).WaitAsync();

                using (consumer)
                {
                    while (!_token.IsCancellationRequested)
                    {
                        var cRes = consumer.Consume(_token);
                        if (_token.IsCancellationRequested) return;
                        Task.Run(() => EnqueueMessageToProcessAsync(ProcessingTaskAsync, cRes));
                        consumer.Commit(cRes);
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
        /// <param name="result">Message instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task ProcessingTaskAsync(ConsumeResult<byte[], byte[]> result)
        {
            try
            {
                var body = result.Value;

                Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}/{2}'", body.Length, Connection.Route, Connection.Name);
                var messageBody = ReceiverSerializer.Deserialize(body, _messageType);
                switch (messageBody)
                {
                    case RequestMessage request when request.Header != null:
                        request.Header.ApplicationReceivedTime = Core.Now;
                        Counters.IncrementReceivingTime(request.Header.TotalTime);
                        if (request.Header.ClientName != Config.Name)
                            Core.Log.Warning("The Message Client Name '{0}' is different from the Server Name '{1}'", request.Header.ClientName, Config.Name);
                        var evArgs = new RequestReceivedEventArgs(_name, Connection, request, body.Length, SenderSerializer);
                        if (request.Header.ResponseQueue != null)
                            evArgs.ResponseQueues.Add(request.Header.ResponseQueue);
                        await OnRequestReceivedAsync(evArgs).ConfigureAwait(false);
                        break;
                    case ResponseMessage response when response.Header != null:
                        response.Header.Response.ApplicationReceivedTime = Core.Now;
                        Counters.IncrementReceivingTime(response.Header.Response.TotalTime);
                        var evArgs2 = new ResponseReceivedEventArgs(_name, response, body.Length);
                        await OnResponseReceivedAsync(evArgs2).ConfigureAwait(false);
                        break;
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
