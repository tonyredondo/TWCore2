/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using TWCore.Messaging;
using TWCore.Messaging.Server;
using TWCore.Services.Messaging;
// ReSharper disable CheckNamespace

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Messaging Service base class async version
    /// </summary>
    public abstract class MessagingServiceAsync : IMessagingServiceAsync
    {
        private static readonly ConcurrentDictionary<object, object> ReceivedMessagesCache = new ConcurrentDictionary<object, object>();
        private CancellationTokenSource _cTokenSource;

        #region Events
        /// <summary>
        /// Events that fires when a message has been received
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;
        /// <summary>
        /// Events that fires before sending a message
        /// </summary>
        public event EventHandler<MessageEventArgs> BeforeSendMessage;
        /// <summary>
        /// Events that fires when a message has been sent
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageSent;
        #endregion

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Message processor async version
        /// </summary>
        public IMessageProcessorAsync Processor { get; private set; }
        /// <inheritdoc />
        /// <summary>
        /// Messaging queue server
        /// </summary>
        public IMQueueServer QueueServer { get; private set; }
        /// <inheritdoc />
        /// <summary>
        /// Get if the service support pause and continue
        /// </summary>
        public bool CanPauseAndContinue => true;
        /// <summary>
        /// Messaging service counters
        /// </summary>
        public MessagingServiceCounters Counters { get; private set; }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// On Service Start method
        /// </summary>
        /// <param name="args">Start arguments</param>
        public void OnStart(string[] args)
        {
            try
            {
                Core.Log.InfoBasic("Starting messaging service");
                _cTokenSource = new CancellationTokenSource();
                OnInit(args);
                Counters = new MessagingServiceCounters();
                QueueServer = GetQueueServer();
                if (QueueServer == null)
                    throw new Exception("The queue server is null, please check the configuration file and ensure the Types assemblies are on the assembly folder.");
                Processor = GetMessageProcessorAsync(QueueServer);
                if (Processor == null)
                    throw new Exception("The message processor is null, please check your GetMessageProcessor method implementation.");
                if (QueueServer.ResponseServer)
                {
                    QueueServer.ResponseReceived += async (s, e) =>
                    {
                        MessageReceived?.Invoke(this, new MessageEventArgs(e.Message));
                        if (e.Message?.Body == null) return;

                        ReceivedMessagesCache.TryAdd(e.Message.Body, e.Message);
                        Counters.IncrementCurrentMessagesBeingProcessed();
                        var sw = Stopwatch.StartNew();
                        try
                        {
                            await Processor.ProcessAsync(e.Message.Body, _cTokenSource.Token);
                        }
                        catch(Exception ex)
                        {
                            Core.Log.Write(ex);
                            Counters.IncrementTotalExceptions();
                        }
                        sw.Stop();
                        Counters.ReportProcessingTime(sw.Elapsed.TotalMilliseconds);
                        Counters.DecrementCurrentMessagesBeingProcessed();
                        Counters.IncrementTotalMessagesProccesed();
                        ReceivedMessagesCache.TryRemove(e.Message.Body, out object _);
                    };
                }
                else
                {
                    QueueServer.RequestReceived += async (s, e) =>
                    {
                        MessageReceived?.Invoke(this, new MessageEventArgs(e.Request));
                        if (e.Request?.Body == null) return;

                        ReceivedMessagesCache.TryAdd(e.Request.Body, e.Request);
                        object result = null;
                        Counters.IncrementCurrentMessagesBeingProcessed();
                        var sw = Stopwatch.StartNew();
                        try
                        {
                            result = await Processor.ProcessAsync(e.Request.Body, e.ProcessResponseTimeoutCancellationToken).ConfigureAwait(false);
                        }
                        catch(Exception ex)
                        {
                            Core.Log.Write(ex);
                            Counters.IncrementTotalExceptions();
                        }
                        sw.Stop();
                        Counters.ReportProcessingTime(sw.Elapsed.TotalMilliseconds);
                        Counters.DecrementCurrentMessagesBeingProcessed();
                        Counters.IncrementTotalMessagesProccesed();
                        e.Response.Body = result;
                        ReceivedMessagesCache.TryRemove(e.Request.Body, out object _);
                    };
                    QueueServer.BeforeSendResponse += (s, e) =>
                    {
                        BeforeSendMessage?.Invoke(this, new MessageEventArgs(e.Message));
                    };
                    QueueServer.ResponseSent += (s, e) =>
                    {
                        MessageSent?.Invoke(this, new MessageEventArgs(e.Message));
                    };
                }

                Processor.Init();
                QueueServer.StartListeners();
                Core.Log.InfoBasic("Messaging service started.");

                Core.Status.Attach(collection =>
                {
                    Core.Status.AttachChild(Processor, this);
                    Core.Status.AttachChild(QueueServer, this);
                    Core.Status.AttachChild(Counters, this);
                });
            }
            catch(Exception ex)
            {
                Core.Log.Write(ex);
                throw;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On Service Stops method
        /// </summary>
        public void OnStop()
        {
            try
            {
                _cTokenSource.Cancel();
                OnFinalizing();
                Core.Log.InfoBasic("Stopping messaging service");
                QueueServer.StopListeners();
                QueueServer.Dispose();
                Processor.Dispose();
                OnDispose();
                Core.Log.InfoBasic("Messaging service stopped.");
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                //throw;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On Continue from pause method
        /// </summary>
        public void OnContinue()
        {
            try
            {
                Core.Log.InfoBasic("Continuing messaging service");
                _cTokenSource = new CancellationTokenSource();
                QueueServer.StartListeners();
                Core.Log.InfoBasic("Messaging service is running");
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                //throw;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On Pause method
        /// </summary>
        public void OnPause()
        {
            try
            {
                Core.Log.InfoBasic("Pausing messaging service");
                _cTokenSource.Cancel();
                QueueServer.StopListeners();
                Core.Log.InfoBasic("Messaging service is paused");
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                //throw;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// On shutdown requested method
        /// </summary>
        public void OnShutdown()
        {
            Core.Log.InfoBasic("Shutting down messaging service");
            OnStop();
            Core.Log.InfoBasic("Messaging service has been shutted down.");
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Service Init
        /// </summary>
        /// <param name="args">Service arguments</param>
        protected virtual void OnInit(string[] args) { }
        /// <summary>
        /// On Service Stop
        /// </summary>
        protected virtual void OnFinalizing() { }
        /// <summary>
        /// On Service Dispose
        /// </summary>
        protected virtual void OnDispose() { }
        /// <summary>
        /// Gets the queue server object
        /// </summary>
        /// <returns>IMQueueServer object instance</returns>
        protected abstract IMQueueServer GetQueueServer();
        /// <summary>
        /// Gets the message processor
        /// </summary>
        /// <param name="server">Queue server object instance</param>
        /// <returns>Message processor instance</returns>
        protected abstract IMessageProcessorAsync GetMessageProcessorAsync(IMQueueServer server);
        #endregion

        #region Static Methods
        /// <summary>
        /// Gets the complete request message with headers from a body
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <returns>Complete Request message instance</returns>
        public static RequestMessage GetCompleteRequestMessage(object messageBody)
        {
            if (ReceivedMessagesCache.TryGetValue(messageBody, out var _out))
                return (RequestMessage)_out;
            return null;
        }
        /// <summary>
        /// Gets the complete response message with headers from a body
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <returns>Complete Response message instance</returns>
        public static ResponseMessage GetCompleteResponseMessage(object messageBody)
        {
            if (ReceivedMessagesCache.TryGetValue(messageBody, out var _out))
                return (ResponseMessage)_out;
            return null;
        }
        #endregion
    }
}
