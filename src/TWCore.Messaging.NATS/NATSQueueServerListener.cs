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

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NATS.Client;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Server;
// ReSharper disable InconsistentNaming
// ReSharper disable MethodSupportsCancellation
#pragma warning disable 414
#pragma warning disable CS4014 // Because a call is not awaited

namespace TWCore.Messaging.NATS
{
    /// <inheritdoc />
    /// <summary>
    /// NATS server listener implementation
    /// </summary>
    public class NATSQueueServerListener : MQueueServerListenerBase
    {
        #region Fields
        private readonly ConnectionFactory _factory;
        private readonly object _lock = new object();
        private readonly Type _messageType;
        private readonly string _name;
        private IConnection _connection;
        private IAsyncSubscription _receiver;
        private CancellationToken _token;
        private Task _monitorTask;
        private bool _exceptionSleep;
        #endregion

        #region Nested Type
        private struct NATSQMessage
        {
            public Guid CorrelationId;
            public MultiArray<byte> Body;
        }
        private void MessageHandler(object sender, MsgHandlerEventArgs e)
        {
            Core.Log.LibVerbose("Message received");

            try
            {
                (var body, var correlationId) = NATSQueueClient.GetFromMessageBody(e.Message.Data);
                var rMsg = new NATSQMessage
                {
                    CorrelationId = correlationId,
                    Body = body
                };
                Task.Run(() => EnqueueMessageToProcessAsync(ProcessingTaskAsync, rMsg));
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// NATS server listener implementation
        /// </summary>
        /// <param name="connection">Queue server listener</param>
        /// <param name="server">Message queue server instance</param>
        /// <param name="responseServer">true if the server is going to act as a response server</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NATSQueueServerListener(MQConnection connection, IMQueueServer server, bool responseServer) : base(connection, server, responseServer)
        {
            _factory = new ConnectionFactory();
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
            _token = token;
            await Extensions.InvokeWithRetry(() =>
            {
                _connection = _factory.CreateConnection(Connection.Route);
            }, 5000, int.MaxValue).ConfigureAwait(false);
            _receiver = _connection.SubscribeAsync(Connection.Name, MessageHandler);
            _monitorTask = Task.Run(MonitorProcess, _token);
            await token.WhenCanceledAsync().ConfigureAwait(false);
            OnDispose();
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
            try
            {
                _receiver.Unsubscribe();
                _connection.Close();
            }
            catch
            {
                // ignored
            }
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
                        OnDispose();
                        Core.Log.Warning("An exception has been thrown, the listener has been stoped for {0} seconds.", Config.RequestOptions.ServerReceiverOptions.SleepOnExceptionInSec);
                        await Task.Delay(Config.RequestOptions.ServerReceiverOptions.SleepOnExceptionInSec * 1000, _token).ConfigureAwait(false);
                        lock (_lock)
                            _exceptionSleep = false;
                        _connection.Close();
                        _receiver.Unsubscribe();
                        _connection = _factory.CreateConnection(Connection.Route);
                        _receiver = _connection.SubscribeAsync(Connection.Name, MessageHandler);
                        Core.Log.Warning("The listener has been resumed.");
                    }

                    if (Counters.CurrentMessages >= Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue)
                    {
                        OnDispose();
                        Core.Log.Warning("Maximum simultaneous messages per queue has been reached, the message needs to wait to be processed, consider increase the MaxSimultaneousMessagePerQueue value, CurrentValue={0}.", Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue);

                        while (!_token.IsCancellationRequested && Counters.CurrentMessages >= Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue)
                            await Task.Delay(500, _token).ConfigureAwait(false);

                        _connection.Close();
                        _receiver.Unsubscribe();
                        _connection = _factory.CreateConnection(Connection.Route);
                        _receiver = _connection.SubscribeAsync(Connection.Name, MessageHandler);
                        Core.Log.Warning("The listener has been resumed.");
                    }

                    await Task.Delay(100, _token).ConfigureAwait(false);
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
        private async Task ProcessingTaskAsync(NATSQMessage message)
        {
            try
            {
                Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}/{2}'", message.Body.Count, Connection.Route, Connection.Name);
                var messageBody = ReceiverSerializer.Deserialize(message.Body, _messageType);
                switch (messageBody)
                {
                    case RequestMessage request when request.Header != null:
                        request.Header.ApplicationReceivedTime = Core.Now;
                        Counters.IncrementReceivingTime(request.Header.TotalTime);
                        if (request.Header.ClientName != Config.Name)
                            Core.Log.Warning("The Message Client Name '{0}' is different from the Server Name '{1}'", request.Header.ClientName, Config.Name);
                        var evArgs = new RequestReceivedEventArgs(_name, Connection, request, message.Body.Count, SenderSerializer);
                        if (request.Header.ResponseQueue != null)
                            evArgs.ResponseQueues.Add(request.Header.ResponseQueue);
                        await OnRequestReceivedAsync(evArgs).ConfigureAwait(false);
                        break;
                    case ResponseMessage response when response.Header != null:
                        response.Header.Response.ApplicationReceivedTime = Core.Now;
                        Counters.IncrementReceivingTime(response.Header.Response.TotalTime);
                        var evArgs2 = new ResponseReceivedEventArgs(_name, response, message.Body.Count);
                        await OnResponseReceivedAsync(evArgs2).ConfigureAwait(false);
                        break;
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
        }
        #endregion
    }
}