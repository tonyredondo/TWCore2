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

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly string _name;
        private RabbitMQueue _receiver;
        private EventingBasicConsumer _receiverConsumer;
        private string _receiverConsumerTag;
        private CancellationToken _token;
        private Task _monitorTask;
        private int _exceptionSleep;
        private readonly Func<RabbitMessage, Task> _processingTaskAsyncDelegate;
        #endregion

        #region Nested Type
        private class RabbitMessage
        {
            private static readonly ObjectPool<RabbitMessage> ObjectPool = new ObjectPool<RabbitMessage>(_ => new RabbitMessage());
            public Guid CorrelationId;
            public IBasicProperties Properties;
            public byte[] Body;

            private RabbitMessage() { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static RabbitMessage Rent(Guid correlationId, IBasicProperties properties, byte[] body)
            {
                var item = ObjectPool.New();
                item.CorrelationId = correlationId;
                item.Properties = properties;
                item.Body = body;
                return item;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Return(RabbitMessage value)
            {
                if (value == null) return;
                value.CorrelationId = Guid.Empty;
                value.Properties = null;
                value.Body = null;
                ObjectPool.Store(value);
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
        public RabbitMQueueRawServerListener(MQConnection connection, IMQueueRawServer server, bool responseServer) : base(connection, server, responseServer)
        {
            _name = server.Name;
            _processingTaskAsyncDelegate = ProcessingTaskAsync;
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
            _receiverConsumer.Received += MessageReceivedHandler;
            _receiverConsumerTag = _receiver.Channel.BasicConsume(_receiver.Name, false, _receiverConsumer);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MessageReceivedHandler(object sender, BasicDeliverEventArgs ea)
        {
#if NET6_0_OR_GREATER
            var msg = RabbitMessage.Rent(Guid.Parse(ea.BasicProperties.CorrelationId), ea.BasicProperties, ea.Body.ToArray());
#else
            var msg = RabbitMessage.Rent(Guid.Parse(ea.BasicProperties.CorrelationId), ea.BasicProperties, ea.Body);
#endif
#if COMPATIBILITY
            Task.Run(() => EnqueueMessageToProcessAsync(_processingTaskAsyncDelegate, msg));
#else
            ThreadPool.QueueUserWorkItem(item =>
            {
                _ = EnqueueMessageToProcessAsync(_processingTaskAsyncDelegate, item);
            }, msg, true);
#endif
            _receiver.Channel.BasicAck(ea.DeliveryTag, false);
        }
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
						_receiver.Close();
						Core.Log.Warning("An exception has been thrown, the listener has been stopped for {0} seconds.", Config.RequestOptions.ServerReceiverOptions.SleepOnExceptionInSec);
                        await Task.Delay(Config.RequestOptions.ServerReceiverOptions.SleepOnExceptionInSec * 1000, _token).ConfigureAwait(false);
						await _receiver.EnsureConnectionAsync(5000, int.MaxValue).ConfigureAwait(false);
						_receiverConsumerTag = _receiver.Channel.BasicConsume(_receiver.Name, false, _receiverConsumer);
                        Core.Log.Warning("The listener has been resumed.");
                    }

                    if (Counters.CurrentMessages >= Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue && !_token.IsCancellationRequested)
                    {
                        if (_receiverConsumerTag != null)
                            _receiver.Channel.BasicCancel(_receiverConsumerTag);
                        Core.Log.Warning("Maximum simultaneous messages per queue has been reached, the message needs to wait to be processed, consider increase the MaxSimultaneousMessagePerQueue value, CurrentValue={0}.", Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue);

                        while (!_token.IsCancellationRequested && Counters.CurrentMessages >= Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue)
                            await Task.Delay(500, _token).ConfigureAwait(false);

                        _receiverConsumerTag = _receiver.Channel.BasicConsume(_receiver.Name, false, _receiverConsumer);
                        Core.Log.Warning("The listener has been resumed.");
                    }

					if (!_receiver.Channel.IsOpen && !_token.IsCancellationRequested)
					{
						Core.Log.Warning("The Receiver channel is closed and should be open, reconnecting.");
						_receiver.Close();
						await _receiver.EnsureConnectionAsync(5000, int.MaxValue).ConfigureAwait(false);
						if (_receiverConsumerTag != null)
							_receiver.Channel.BasicCancel(_receiverConsumerTag);
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
            if (message.Body is null)
            {
                RabbitMessage.Return(message);
                return;
            }
            try
            {
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
                Interlocked.Exchange(ref _exceptionSleep, 1);
            }
            finally
            {
                RabbitMessage.Return(message);
            }
        }
        #endregion
    }
}