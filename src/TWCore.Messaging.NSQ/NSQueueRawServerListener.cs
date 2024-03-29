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

using NsqSharp;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.RawServer;
// ReSharper disable InconsistentNaming
// ReSharper disable MethodSupportsCancellation
#pragma warning disable CS4014 // Because a call is not awaited

namespace TWCore.Messaging.NSQ
{
    /// <inheritdoc />
    /// <summary>
    /// NSQ raw server listener implementation
    /// </summary>
    public class NSQueueRawServerListener : MQueueRawServerListenerBase
    {
        #region Fields
        private readonly string _name;
        private Consumer _receiver;
        private CancellationToken _token;
        private Task _monitorTask;
        private int _exceptionSleep;
        #endregion

        #region Nested Type
        private class NSQMessageHandler : IHandler
        {
            private readonly NSQueueRawServerListener _listener;
            public NSQMessageHandler(NSQueueRawServerListener listener)
            {
                _listener = listener;
            }
            public void HandleMessage(NsqSharp.IMessage message)
            {
                Core.Log.LibVerbose("Message received");
                try
                {
#if COMPATIBILITY
                    Task.Run(() => _listener.EnqueueMessageToProcessAsync(_listener.ProcessingTaskAsync, message.Body));
#else
                    ThreadPool.QueueUserWorkItem( item =>
                    {
                        _listener.EnqueueMessageToProcessAsync(_listener.ProcessingTaskAsync, item);
                    }, message.Body, false);
#endif
                    message.Finish();
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
            public void LogFailedMessage(NsqSharp.IMessage message)
            {
            }
        }

        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// NSQ server listener implementation
        /// </summary>
        /// <param name="connection">Queue server listener</param>
        /// <param name="server">Message queue server instance</param>
        /// <param name="responseServer">true if the server is going to act as a response server</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NSQueueRawServerListener(MQConnection connection, IMQueueRawServer server, bool responseServer) : base(connection, server, responseServer)
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
            _receiver = new Consumer(Connection.Name, Connection.Name);
            _receiver.AddHandler(new NSQMessageHandler(this));
            if (string.IsNullOrEmpty(Connection.Route))
                throw new UriFormatException($"The route for the connection to {Connection.Name} is null.");
            await Extensions.InvokeWithRetry(() =>
            {
                _receiver.ConnectToNsqd(Connection.Route);
            }, 5000, int.MaxValue).ConfigureAwait(false);
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
            if (_receiver is null) return;
            try
            {
                _receiver.Stop();
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
                    if (Interlocked.CompareExchange(ref _exceptionSleep, 0, 1) == 1)
                    {
                        OnDispose();
                        Core.Log.Warning("An exception has been thrown, the listener has been stopped for {0} seconds.", Config.RequestOptions.ServerReceiverOptions.SleepOnExceptionInSec);
                        await Task.Delay(Config.RequestOptions.ServerReceiverOptions.SleepOnExceptionInSec * 1000, _token).ConfigureAwait(false);
                        _receiver = new Consumer(Connection.Name, Connection.Name);
                        _receiver.AddHandler(new NSQMessageHandler(this));
                        _receiver.ConnectToNsqd(Connection.Route);
                        Core.Log.Warning("The listener has been resumed.");
                    }

                    if (Counters.CurrentMessages >= Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue)
                    {
                        OnDispose();
                        Core.Log.Warning("Maximum simultaneous messages per queue has been reached, the message needs to wait to be processed, consider increase the MaxSimultaneousMessagePerQueue value, CurrentValue={0}.", Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue);

                        while (!_token.IsCancellationRequested && Counters.CurrentMessages >= Config.RequestOptions.ServerReceiverOptions.MaxSimultaneousMessagesPerQueue)
                            await Task.Delay(500, _token).ConfigureAwait(false);

                        _receiver = new Consumer(Connection.Name, Connection.Name);
                        _receiver.AddHandler(new NSQMessageHandler(this));
                        _receiver.ConnectToNsqd(Connection.Route);
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
        /// <param name="data">Message data</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task ProcessingTaskAsync(byte[] data)
        {
            try
            {
                (var body, var correlationId, var name) = NSQueueRawClient.GetFromRawMessageBody(data);
                Core.Log.LibVerbose("Received {0} bytes from the Queue '{1}/{2}'", body.Count, Connection.Route, Connection.Name);
                Counters.IncrementTotalReceivingBytes(body.Count);

                if (ResponseServer)
                {
                    var evArgs =
                        new RawResponseReceivedEventArgs(_name, body, correlationId, body.Count)
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
                        new RawRequestReceivedEventArgs(_name, Connection, body, correlationId, body.Count)
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
                Interlocked.Exchange(ref _exceptionSleep, 1);
            }
        }
        #endregion
    }
}