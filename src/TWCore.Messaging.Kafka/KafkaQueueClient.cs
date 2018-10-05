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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Messaging.Client;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Exceptions;
using TWCore.Threading;

// ReSharper disable NotAccessedField.Local
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.Kafka
{
    /// <inheritdoc />
    /// <summary>
    /// Kafka Queue Client
    /// </summary>
    public class KafkaQueueClient : MQueueClientBase
    {
        private static readonly ConcurrentDictionary<Guid, KafkaQueueMessage> ReceivedMessages = new ConcurrentDictionary<Guid, KafkaQueueMessage>();

        #region Fields
        private List<(MQConnection, Producer)> _senders;
        private CancellationTokenSource _tokenSource;
        private MQConnection _receiverConnection;
        private MQClientQueues _clientQueues;
        private MQClientSenderOptions _senderOptions;
        private MQClientReceiverOptions _receiverOptions;
        private TimeSpan _receiverOptionsTimeout;
        #endregion

        #region Properties
        /// <summary>
        /// Use Single Response Queue
        /// </summary>
        [StatusProperty]
        public bool UseSingleResponseQueue { get; private set; }
        #endregion

        #region Nested Type
        private class KafkaQueueMessage
        {
            public MultiArray<byte> Body;
            public readonly AsyncManualResetEvent WaitHandler = new AsyncManualResetEvent(false);
            public string Route;
            public string Name;
        }
        #endregion

        #region Init and Dispose Methods
        /// <inheritdoc />
        /// <summary>
        /// On client initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnInit()
        {
            OnDispose();
            _senders = new List<(MQConnection, Producer)>();
            _tokenSource = new CancellationTokenSource();

            if (Config != null)
            {
                if (Config.ClientQueues != null)
                {
                    _clientQueues = Config.ClientQueues.FirstOf(
                        c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true,
                        c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true,
                        c => c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true,
                        c => c.EnvironmentName.IsNullOrWhitespace());
                }
                _senderOptions = Config.RequestOptions?.ClientSenderOptions;
                _receiverOptions = Config.ResponseOptions?.ClientReceiverOptions;
                _receiverOptionsTimeout = TimeSpan.FromSeconds(_receiverOptions?.TimeoutInSec ?? 20);
                UseSingleResponseQueue = _receiverOptions?.Parameters?[ParameterKeys.SingleResponseQueue].ParseTo(false) ?? false;

                if (_clientQueues?.SendQueues?.Any() == true)
                {
                    foreach (var queue in _clientQueues.SendQueues)
                    {
                        Core.Log.LibVerbose("New Producer from QueueClient");
                        Producer connection = null;
                        if (string.IsNullOrEmpty(queue.Route))
                            throw new UriFormatException($"The route for the connection to {queue.Name} is null.");
                        var options = new KafkaOptions(new Uri(queue.Route));
                        var router = new BrokerRouter(options);
                        Extensions.InvokeWithRetry(() =>
                        {
                            connection = new Producer(router);
                        }, 5000, int.MaxValue).WaitAsync();
                        _senders.Add((queue, connection));
                    }
                }
                if (_clientQueues?.RecvQueue != null)
                {
                    _receiverConnection = _clientQueues.RecvQueue;
                    if (UseSingleResponseQueue)
                    {
                        if (string.IsNullOrEmpty(_receiverConnection.Route))
                            throw new UriFormatException($"The route for the connection to {_receiverConnection.Name} is null.");

                        var cancellationToken = _tokenSource.Token;
                        var consumerTask = Task.Factory.StartNew(() =>
                        {
                            var options = new KafkaOptions(new Uri(_receiverConnection.Route));
                            var router = new BrokerRouter(options);
                            Consumer consumer = null;
                            Extensions.InvokeWithRetry(() =>
                            {
                                consumer = new Consumer(new ConsumerOptions(_receiverConnection.Name, router));
                            }, 5000, int.MaxValue).WaitAsync();
                            using (consumer)
                            {
                                foreach(var cRes in consumer.Consume(cancellationToken))
                                {
                                    if (cancellationToken.IsCancellationRequested) break;
                                    var correlationId = new Guid(cRes.Key);
                                    var message = ReceivedMessages.GetOrAdd(correlationId, cId => new KafkaQueueMessage());
                                    message.Body = cRes.Value;
                                    message.WaitHandler.Set();
                                }
                            }
                        }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);

                    }
                }
            }

            Core.Status.Attach(collection =>
            {
                if (_senders != null)
                {
                    for (var i = 0; i < _senders.Count; i++)
                    {
                        collection.Add("Sender Path: {0}".ApplyFormat(i), _senders[i].Item1.Route);
                    }
                }
                if (_clientQueues?.RecvQueue != null)
                    collection.Add("Receiver Path", _clientQueues.RecvQueue.Route);
            });
        }
        /// <inheritdoc />
        /// <summary>
        /// On Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnDispose()
        {
            if (_senders != null)
            {
                foreach (var sender in _senders)
                {
                    var conn = sender.Item2;
                    conn.Dispose();
                }
                _senders.Clear();
                _senders = null;
            }
            if (_tokenSource is null) return;
            if (UseSingleResponseQueue)
            {
                _tokenSource.Cancel();
            }
            _tokenSource = null;
        }
        #endregion

        #region Send Method
        /// <inheritdoc />
        /// <summary>
        /// On Send message data
        /// </summary>
        /// <param name="message">Request message instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override async Task<bool> OnSendAsync(RequestMessage message)
        {
            if (_senders?.Any() != true)
                throw new NullReferenceException("There aren't any senders queues.");
            if (_senderOptions is null)
                throw new NullReferenceException("SenderOptions is null.");

            if (message.Header.ResponseQueue is null)
            {
                var recvQueue = _clientQueues.RecvQueue;
                if (recvQueue != null)
                {
                    message.Header.ResponseQueue = new MQConnection(recvQueue.Route, recvQueue.Name) { Parameters = recvQueue.Parameters };
                    message.Header.ResponseExpected = true;
                    message.Header.ResponseTimeoutInSeconds = _receiverOptions?.TimeoutInSec ?? -1;
                    if (!UseSingleResponseQueue)
                    {
                        message.Header.ResponseQueue.Name += "_" + message.CorrelationId;
                    }
                }
                else
                {
                    message.Header.ResponseExpected = false;
                    message.Header.ResponseTimeoutInSeconds = -1;
                }
            }
            var data = SenderSerializer.Serialize(message);

            foreach ((var queue, var producer) in _senders)
            {
                Core.Log.LibVerbose("Sending {0} bytes to the Queue '{1}/{2}' with CorrelationId={3}", data.Count, queue.Route, queue.Name, message.Header.CorrelationId);
                await producer.SendMessageAsync(queue.Name, new[] { new Message { Key = message.CorrelationId.ToByteArray(), Value = data.AsArray() } }).ConfigureAwait(false);
            }
            Core.Log.LibVerbose("Message with CorrelationId={0} sent", message.Header.CorrelationId);
            return true;
        }
        #endregion

        #region Receive Method
        /// <inheritdoc />
        /// <summary>
        /// On Receive message data
        /// </summary>
        /// <param name="correlationId">Correlation Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response message instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override async Task<ResponseMessage> OnReceiveAsync(Guid correlationId, CancellationToken cancellationToken)
        {
            if (_tokenSource is null && UseSingleResponseQueue)
                throw new NullReferenceException("There is not receiver queue.");

            var sw = Stopwatch.StartNew();
            var message = ReceivedMessages.GetOrAdd(correlationId, cId => new KafkaQueueMessage());

            if (!UseSingleResponseQueue)
            {
                message.Name = _receiverConnection.Name + "_" + correlationId;
                message.Route = _receiverConnection.Route;

                var consumerTask = Task.Factory.StartNew(() =>
                {
                    var options = new KafkaOptions(new Uri(message.Route));
                    var router = new BrokerRouter(options);
                    using (var consumer = new Consumer(new ConsumerOptions(message.Name, router)))
                    {
                        foreach (var cRes in consumer.Consume(cancellationToken))
                        {
                            message.Body = cRes.Value;
                            message.WaitHandler.Set();
                            break;
                        }
                    }
                }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);

                var waitResult = await message.WaitHandler.WaitAsync(_receiverOptionsTimeout, cancellationToken).ConfigureAwait(false);
                if (!waitResult) throw new MessageQueueTimeoutException(_receiverOptionsTimeout, correlationId.ToString());

                if (message.Body == MultiArray<byte>.Empty)
                    throw new MessageQueueNotFoundException("The Message can't be retrieved, null body on CorrelationId = " + correlationId);

                Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}' with CorrelationId={2}", message.Body.Count, _clientQueues.RecvQueue.Name, correlationId);
                var response = ReceiverSerializer.Deserialize<ResponseMessage>(message.Body);
                ReceivedMessages.TryRemove(correlationId, out _);
                Core.Log.LibVerbose("Correlation Message ({0}) received at: {1}ms", correlationId, sw.Elapsed.TotalMilliseconds);
                sw.Stop();
                return response;
            }

            if (!await message.WaitHandler.WaitAsync(_receiverOptionsTimeout, cancellationToken).ConfigureAwait(false))
                throw new MessageQueueTimeoutException(_receiverOptionsTimeout, correlationId.ToString());

            if (message.Body == MultiArray<byte>.Empty)
                throw new MessageQueueBodyNullException("The Message can't be retrieved, null body on CorrelationId = " + correlationId);

            Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}' with CorrelationId={2}", message.Body.Count, _clientQueues.RecvQueue.Name, correlationId);
            var rs = ReceiverSerializer.Deserialize<ResponseMessage>(message.Body);
            ReceivedMessages.TryRemove(correlationId, out _);
            Core.Log.LibVerbose("Correlation Message ({0}) received at: {1}ms", correlationId, sw.Elapsed.TotalMilliseconds);
            sw.Stop();
            return rs;
        }
        #endregion
    }
}
