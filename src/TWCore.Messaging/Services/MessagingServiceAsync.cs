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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Messaging;
using TWCore.Messaging.Server;
using TWCore.Services.Messaging;
using TWCore.Threading;
// ReSharper disable CheckNamespace
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Messaging Service base class async version
    /// </summary>
    public abstract class MessagingServiceAsync : IMessagingServiceAsync
    {
        private static readonly ConditionalWeakTable<object, ResponseMessage> ResponseMessageCache = new ConditionalWeakTable<object, ResponseMessage>();
        private static readonly ConditionalWeakTable<object, RequestMessage> RequestMessageCache = new ConditionalWeakTable<object, RequestMessage>();
        private CancellationTokenSource _cTokenSource;

        #region Events
        /// <summary>
        /// Events that fires when a message has been received
        /// </summary>
        public AsyncEvent<MessageEventArgs> MessageReceived { get; set; }
        /// <summary>
        /// Events that fires before sending a message
        /// </summary>
        public AsyncEvent<MessageEventArgs> BeforeSendMessage { get; set; }
        /// <summary>
        /// Events that fires when a message has been sent
        /// </summary>
        public AsyncEvent<MessageEventArgs> MessageSent { get; set; }
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
        public MessagingServiceStatus Status { get; private set; }
        /// <summary>
        /// Gets a value indicating enable messages trace.
        /// </summary>
        /// <value><c>true</c> if enable messages trace; otherwise, <c>false</c>.</value>
        public bool EnableMessagesTrace { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Enables the Complete Message cache
        /// </summary>
        public bool EnableCompleteMessageCache { get; set; }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// On Service Start method
        /// </summary>
        /// <param name="args">Start arguments</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnStart(string[] args)
        {
            try
            {
                Core.Log.InfoBasic("Starting messaging service");
                _cTokenSource = new CancellationTokenSource();
                OnInit(args);
                Status = new MessagingServiceStatus(this);
                QueueServer = GetQueueServer();
                if (QueueServer is null)
                    throw new Exception("The queue server is null, please check the configuration file and ensure the Types assemblies are on the assembly folder.");
                Processor = GetMessageProcessorAsync(QueueServer);
                if (Processor is null)
                    throw new Exception("The message processor is null, please check your GetMessageProcessor method implementation.");
                if (QueueServer.ResponseServer)
                {
                    QueueServer.ResponseReceived += ResponseReceivedHandler;
                }
                else
                {
                    QueueServer.RequestReceived += RequestReceivedHandler;
                    QueueServer.BeforeSendResponse += BeforeSendResponseHandler;
                    QueueServer.ResponseSent += ResponseSentHandler;
                }
                Processor.Init();
                QueueServer.StartListeners();
                Core.Log.InfoBasic("Messaging service started.");
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task RequestReceivedHandler(object sender, RequestReceivedEventArgs e)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                if (MessageReceived != null)
                    await MessageReceived.InvokeAsync(this, new MessageEventArgs(e.Request)).ConfigureAwait(false);
                if (e.Request?.Body is null) return;
                var body = e.Request.Body.GetValue();
                if (EnableCompleteMessageCache)
                    RequestMessageCache.Add(body, e.Request);
                object result;
                Status.IncrementCurrentMessagesBeingProcessed();
                try
                {
                    result = await Processor.ProcessAsync(body, e.ProcessResponseTimeoutCancellationToken).ConfigureAwait(false);
                    e.SetResponseBody(result);
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                    Status.IncrementTotalExceptions();
                }
                sw.Stop();
                Status.ReportProcessingTime(sw.Elapsed.TotalMilliseconds);
                Status.DecrementCurrentMessagesBeingProcessed();
                Status.IncrementTotalMessagesProccesed();
                if (EnableCompleteMessageCache)
                    RequestMessageCache.Remove(body);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                Status.IncrementTotalExceptions();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task ResponseReceivedHandler(object sender, ResponseReceivedEventArgs e)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                if (MessageReceived != null)
                    await MessageReceived.InvokeAsync(this, new MessageEventArgs(e.Message)).ConfigureAwait(false);
                if (e.Message?.Body is null) return;
                var body = e.Message.Body.GetValue();
                if (EnableCompleteMessageCache)
                    ResponseMessageCache.Add(body, e.Message);
                Status.IncrementCurrentMessagesBeingProcessed();
                try
                {
                    await Processor.ProcessAsync(body, _cTokenSource.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                    Status.IncrementTotalExceptions();
                }
                sw.Stop();
                Status.ReportProcessingTime(sw.Elapsed.TotalMilliseconds);
                Status.DecrementCurrentMessagesBeingProcessed();
                Status.IncrementTotalMessagesProccesed();
                if (EnableCompleteMessageCache)
                    ResponseMessageCache.Remove(body);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                Status.IncrementTotalExceptions();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task BeforeSendResponseHandler(object sender, ResponseSentEventArgs e)
        {
            if (BeforeSendMessage != null)
                await BeforeSendMessage.InvokeAsync(this, new MessageEventArgs(e.Message)).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task ResponseSentHandler(object sender, ResponseSentEventArgs e)
        {
            if (MessageSent != null)
                await MessageSent.InvokeAsync(this, new MessageEventArgs(e.Message)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        /// <summary>
        /// On Service Stops method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnInit(string[] args)
        {
            MessageReceived += MessageReceivedHandler;
            MessageSent += MessageSentHandler;

            Task MessageReceivedHandler(object sender, MessageEventArgs e)
            {
                if (!EnableMessagesTrace) return Task.CompletedTask;
                var msg = e.Message;
                Core.Trace.Write(GetType().FullName, OnGetReceivedMessageTraceName(msg), msg, msg.CorrelationId.ToString());
                return Task.CompletedTask;
            }
            Task MessageSentHandler(object sender, MessageEventArgs e)
            {
                if (!EnableMessagesTrace) return Task.CompletedTask;
                var msg = e.Message;
                Core.Trace.Write(GetType().FullName, OnGetSentMessageTraceName(msg), msg, msg.CorrelationId.ToString());
                return Task.CompletedTask;
            }
        }
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

        /// <summary>
        /// Get Received Message Trace Name
        /// </summary>
        /// <param name="message">Message object</param>
        /// <returns>Trace name</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual string OnGetReceivedMessageTraceName(IMessage message)
            => "QueueReceivedMessage - " + message.CorrelationId;
        /// <summary>
        /// Get Sent Message Trace Name
        /// </summary>
        /// <param name="message">Message object</param>
        /// <returns>Trace name</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual string OnGetSentMessageTraceName(IMessage message)
            => "QueueSentMessage - " + message.CorrelationId;
        #endregion

        #region Static Methods
        /// <summary>
        /// Gets the complete request message with headers from a body
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <returns>Complete Request message instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RequestMessage GetCompleteRequestMessage(object messageBody)
        {
            if (RequestMessageCache.TryGetValue(messageBody, out var msgout))
                return msgout;
            return null;
        }
        /// <summary>
        /// Gets the complete response message with headers from a body
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <returns>Complete Response message instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResponseMessage GetCompleteResponseMessage(object messageBody)
        {
            if (ResponseMessageCache.TryGetValue(messageBody, out var msgout))
                return msgout;
            return null;
        }
        #endregion
    }
}
