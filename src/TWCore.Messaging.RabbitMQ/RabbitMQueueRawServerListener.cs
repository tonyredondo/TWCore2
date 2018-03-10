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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.RawServer;
// ReSharper disable MethodSupportsCancellation

namespace TWCore.Messaging.RabbitMQ
{
	/// <inheritdoc />
	/// <summary>
	/// RabbitMQ server listener implementation
	/// </summary>
	public class RabbitMQueueRawServerListener : MQueueRawServerListenerBase
    {
        #region Fields
        private readonly object _lock = new object();
        private readonly string _name;
        private RabbitMQueue _receiver;
        private EventingBasicConsumer _receiverConsumer;
        private string _receiverConsumerTag;
        private CancellationToken _token;
        private Task _monitorTask;
        private bool _exceptionSleep;
        #endregion

        #region Nested Type
        private struct RabbitMessage
        {
            public Guid CorrelationId;
            public IBasicProperties Properties;
            public byte[] Body;
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
        public RabbitMQueueRawServerListener(MQConnection connection, IMQueueRawServer server, bool responseServer) : base(connection, server, responseServer)
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
            _token = token;
            _receiver = new RabbitMQueue(Connection);
            _receiver.EnsureConnection();
            _receiver.EnsureQueue();
            _receiverConsumer = new EventingBasicConsumer(_receiver.Channel);
            _receiverConsumer.Received += (ch, ea) =>
            {
                var message = new RabbitMessage
                {
                    CorrelationId = Guid.Parse(ea.BasicProperties.CorrelationId),
                    Properties = ea.BasicProperties,
                    Body = ea.Body
                };
                Task.Run(() => EnqueueMessageToProcessAsync(ProcessingTaskAsync, message));
                _receiver.Channel.BasicAck(ea.DeliveryTag, false);
            };
            _receiverConsumerTag = _receiver.Channel.BasicConsume(_receiver.Name, false, _receiverConsumer);
            _monitorTask = Task.Run(MonitorProcess, _token);

            await token.WhenCanceledAsync().ConfigureAwait(false);
            if (_receiverConsumerTag != null)
                _receiver.Channel.BasicCancel(_receiverConsumerTag);
            _receiver.Close();

            WorkerEvent.Wait(TimeSpan.FromSeconds(Config.RequestOptions.ServerReceiverOptions.ProcessingWaitOnFinalizeInSec));
            await _monitorTask.ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// On Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnDispose()
        {
            if (_receiver == null) return;
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
                    bool exSleep;
                    lock (_lock)
                        exSleep = _exceptionSleep;
                    if (exSleep)
                    {
                        if (_receiverConsumerTag != null)
                            _receiver.Channel.BasicCancel(_receiverConsumerTag);
                        Core.Log.Warning("An exception has been thrown, the listener has been stoped for {0} seconds.", Config.RequestOptions.ServerReceiverOptions.SleepOnExceptionInSec);
                        await Task.Delay(Config.RequestOptions.ServerReceiverOptions.SleepOnExceptionInSec * 1000, _token).ConfigureAwait(false);
                        lock (_lock)
                            _exceptionSleep = false;
                        _receiverConsumerTag = _receiver.Channel.BasicConsume(_receiver.Name, false, _receiverConsumer);
                    }

                    if (Counters.CurrentMessages >= Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue)
                    {
                        if (_receiverConsumerTag != null)
                            _receiver.Channel.BasicCancel(_receiverConsumerTag);
                        Core.Log.Warning("Maximum simultaneous messages per queue has been reached, the message needs to wait to be processed, consider increase the MaxSimultaneousMessagePerQueue value, CurrentValue={0}.", Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue);

                        while (!_token.IsCancellationRequested && Counters.CurrentMessages >= Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue)
                            await Task.Delay(500, _token).ConfigureAwait(false);

                        _receiverConsumerTag = _receiver.Channel.BasicConsume(_receiver.Name, false, _receiverConsumer);
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
            if (message.Body == null) return;
            try
            {
                Counters.IncrementProcessingThreads();
                Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}/{2}'", message.Body.Length, _receiver.Route, _receiver.Name);
                Counters.IncrementTotalReceivingBytes(message.Body.Length);
                if (ResponseServer)
                {
                    var evArgs =
                        new RawResponseReceivedEventArgs(_name, message.Body, message.CorrelationId, message.Body.Length)
                        {
                            Metadata =
                            {
                                ["AppId"] = message.Properties.AppId,
                                ["ContentEncoding"] = message.Properties.ContentEncoding,
                                ["ContentType"] = message.Properties.ContentType,
                                ["DeliveryMode"] = message.Properties.DeliveryMode.ToString(),
                                ["Expiration"] = message.Properties.Expiration,
                                ["Priority"] = message.Properties.Priority.ToString(),
                                ["Timestamp"] = message.Properties.Timestamp.ToString(),
                                ["Type"] = message.Properties.Type,
                                ["UserId"] = message.Properties.UserId,
                                ["ReplyTo"] = message.Properties.ReplyTo,
                                ["MessageId"] = message.Properties.MessageId
                            }
                        };
                    await OnResponseReceivedAsync(evArgs).ConfigureAwait(false);
                }
                else
                {
                    var evArgs =
                        new RawRequestReceivedEventArgs(_name, _receiver, message.Body, message.CorrelationId, message.Body.Length)
                        {
                            Metadata =
                            {
                                ["AppId"] = message.Properties.AppId,
                                ["ContentEncoding"] = message.Properties.ContentEncoding,
                                ["ContentType"] = message.Properties.ContentType,
                                ["DeliveryMode"] = message.Properties.DeliveryMode.ToString(),
                                ["Expiration"] = message.Properties.Expiration,
                                ["Priority"] = message.Properties.Priority.ToString(),
                                ["Timestamp"] = message.Properties.Timestamp.ToString(),
                                ["Type"] = message.Properties.Type,
                                ["UserId"] = message.Properties.UserId,
                                ["ReplyTo"] = message.Properties.ReplyTo,
                                ["MessageId"] = message.Properties.MessageId
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
                lock (_lock)
                    _exceptionSleep = true;
            }
            finally
            {
                Counters.DecrementProcessingThreads();
            }
        }
        #endregion
    }
}