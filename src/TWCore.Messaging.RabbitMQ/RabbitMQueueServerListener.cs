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
using RabbitMQ.Client.Events;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Server;
// ReSharper disable MethodSupportsCancellation
#pragma warning disable 414

namespace TWCore.Messaging.RabbitMQ
{
    /// <inheritdoc />
    /// <summary>
    /// RabbitMQ server listener implementation
    /// </summary>
    public class RabbitMQueueServerListener : MQueueServerListenerBase
    {
        #region Fields
        private readonly Type _messageType;
        private readonly string _name;
        private RabbitMQueue _receiver;
        private EventingBasicConsumer _receiverConsumer;
        private string _receiverConsumerTag;
        private CancellationToken _token;
        private Task _monitorTask;
        private int _exceptionSleep;
        #endregion

        #region Nested Type
        private readonly struct RabbitMessage
        {
            public readonly Guid CorrelationId;
            public readonly IBasicProperties Properties;
            public readonly byte[] Body;

            public RabbitMessage(Guid correlationId, IBasicProperties properties, byte[] body)
            {
                CorrelationId = correlationId;
                Properties = properties;
                Body = body;
            }
        }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// RabbitMQ server listener implementation
        /// </summary>
        /// <param name="connection">Queue server listener</param>
        /// <param name="server">Message queue server instance</param>
        /// <param name="responseServer">true if the server is going to act as a response server</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RabbitMQueueServerListener(MQConnection connection, IMQueueServer server, bool responseServer) : base(connection, server, responseServer)
        {
            _messageType = responseServer ? typeof(ResponseMessage) : typeof(RequestMessage);
            _name = server.Name;
            Core.Status.Attach(collection =>
            {
                collection.Add("Message Type", _messageType);
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
            _token = token;
            _receiver = new RabbitMQueue(Connection);
            await _receiver.EnsureConnectionAsync(5000, int.MaxValue).ConfigureAwait(false);
            _receiver.EnsureQueue();
            _receiverConsumer = new EventingBasicConsumer(_receiver.Channel);
            _receiverConsumer.Received += async (ch, ea) =>
            {
                _receiver.Channel.BasicAck(ea.DeliveryTag, false);
                var msg = new RabbitMessage(Guid.Parse(ea.BasicProperties.CorrelationId), ea.BasicProperties, ea.Body);
                await EnqueueMessageToProcessAsync(ProcessingTaskAsync, msg).ConfigureAwait(false);
            };
            _receiverConsumerTag = _receiver.Channel.BasicConsume(_receiver.Name, true, _receiverConsumer);
            _monitorTask = Task.Run(MonitorProcess, _token);

            await token.WhenCanceledAsync().ConfigureAwait(false);
            if (_receiverConsumerTag != null)
                _receiver.Channel.BasicCancel(_receiverConsumerTag);
            _receiver.Close();

            await _monitorTask.ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// On Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnDispose()
        {
            if (_receiver is null) return;
            if (!string.IsNullOrEmpty(_receiverConsumerTag))
                _receiver.Channel?.BasicCancel(_receiverConsumerTag);
            _receiver.Close();
            _receiver = null;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Monitors the maximum concurrent message allowed for the listener
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task MonitorProcess()
        {
            while (!_token.IsCancellationRequested)
            {
                try
                {
                    if (Interlocked.CompareExchange(ref _exceptionSleep, 0, 1) == 1)
                    {
                        if (_receiverConsumerTag != null)
                            _receiver.Channel.BasicCancel(_receiverConsumerTag);
                        Core.Log.Warning("An exception has been thrown, the listener has been stopped for {0} seconds.", Config.RequestOptions.ServerReceiverOptions.SleepOnExceptionInSec);
                        await Task.Delay(Config.RequestOptions.ServerReceiverOptions.SleepOnExceptionInSec * 1000, _token).ConfigureAwait(false);
                        _receiverConsumerTag = _receiver.Channel.BasicConsume(_receiver.Name, false, _receiverConsumer);
                        Core.Log.Warning("The listener has been resumed.");
                    }

                    if (Counters.CurrentMessages >= Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue)
                    {
                        if (_receiverConsumerTag != null)
                            _receiver.Channel.BasicCancel(_receiverConsumerTag);
                        Core.Log.Warning("Maximum simultaneous messages per queue has been reached, the message needs to wait to be processed, consider increase the MaxSimultaneousMessagePerQueue value, CurrentValue={0}.", Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue);

                        while (!_token.IsCancellationRequested && Counters.CurrentMessages >= Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue)
                            await Task.Delay(500, _token).ConfigureAwait(false);

                        _receiverConsumerTag = _receiver.Channel.BasicConsume(_receiver.Name, false, _receiverConsumer);
                        Core.Log.Warning("The listener has been resumed.");
                    }

                    await Task.Delay(1000, _token).ConfigureAwait(false);
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                    if (!_token.IsCancellationRequested)
                        await Task.Delay(2000, _token).ConfigureAwait(false);
                }
            }
        }
        /// <summary>
        /// Process a received message from the queue
        /// </summary>
        /// <param name="message">Message instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task ProcessingTaskAsync(RabbitMessage message)
        {
            if (message.Body is null) return;
            try
            {
                Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}/{2}'", message.Body.Length, _receiver.Route, _receiver.Name);
                var messageBody = ReceiverSerializer.Deserialize(message.Body, _messageType);
                switch (messageBody)
                {
                    case RequestMessage request when request.Header != null:
                        request.Header.ApplicationReceivedTime = Core.Now;
                        Counters.IncrementReceivingTime(request.Header.TotalTime);
                        if (request.Header.ClientName != Config.Name)
                            Core.Log.Warning("The Message Client Name '{0}' is different from the Server Name '{1}'", request.Header.ClientName, Config.Name);
                        var evArgs =
                            new RequestReceivedEventArgs(_name, _receiver, request, message.Body.Length, SenderSerializer)
                            {
                                Metadata =
                                {
                                    ["ReplyTo"] = message.Properties.ReplyTo,
                                    ["MessageId"] = message.Properties.MessageId
                                }
                            };
                        if (request.Header.ResponseQueue != null)
                            evArgs.ResponseQueues.Add(request.Header.ResponseQueue);
                        await OnRequestReceivedAsync(evArgs).ConfigureAwait(false);
                        break;
                    case ResponseMessage response when response.Header != null:
                        response.Header.Response.ApplicationReceivedTime = Core.Now;
                        Counters.IncrementReceivingTime(response.Header.Response.TotalTime);
                        var evArgs2 =
                            new ResponseReceivedEventArgs(_name, response, message.Body.Length)
                            {
                                Metadata =
                                {
                                    ["ReplyTo"] = message.Properties.ReplyTo,
                                    ["MessageId"] = message.Properties.MessageId
                                }
                            };
                        await OnResponseReceivedAsync(evArgs2).ConfigureAwait(false);
                        break;
                }
                Counters.IncrementTotalMessagesProccesed();
            }
            catch (Exception ex)
            {
                Counters.IncrementTotalExceptions();
                Core.Log.Write(ex);
                Interlocked.Exchange(ref _exceptionSleep, 1);
            }
        }
        #endregion
    }
}