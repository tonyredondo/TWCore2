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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Bot;
using TWCore.Diagnostics.Counters;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace.Storages;
using TWCore.Messaging.RawServer;
using TWCore.Serialization;
using TWCore.Services;
using TWCore.Services.Messaging;
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api
{
    public class DiagnosticRawMessagingServiceAsync : RawMessagingServiceAsync
    {
        private static readonly DiagnosticsSettings Settings = Core.GetSettings<DiagnosticsSettings>();
        private readonly BlockingCollection<LogItem> _logItems = new BlockingCollection<LogItem>();
        private readonly BlockingCollection<GroupMetadata> _groupsMetadata = new BlockingCollection<GroupMetadata>();
        private readonly BlockingCollection<MessagingTraceItem> _messagingTraceItems = new BlockingCollection<MessagingTraceItem>();
        private readonly BlockingCollection<ICounterItem> _counterItems = new BlockingCollection<ICounterItem>();
        private Timer _processTimer;
        private int _inProcess = 0;
        
        protected override void OnInit(string[] args)
        {
            Core.GlobalSettings.LargeObjectHeapCompactTimeoutInMinutes = 1;
            Core.GlobalSettings.ReloadSettings();
            SerializerManager.SupressFileExtensionWarning = true;
            base.OnInit(args);

            DbHandlers.Instance.Messages.Init();

            var processTimerTimeSpan = TimeSpan.FromSeconds(Settings.ProcessTimerInSeconds);
            _processTimer = new Timer(ProcessItems, null, processTimerTimeSpan, processTimerTimeSpan);
        }

        private void ProcessItems(object state)
        {
            if (Interlocked.CompareExchange(ref _inProcess, 1, 0) == 1) return;
            try
            {
                var lstLogs = new List<LogItem>();
                while (_logItems.TryTake(out var item, 100))
                    lstLogs.Add(item);
                var logTask = lstLogs.Count > 0 ? DbHandlers.Instance.Messages.ProcessLogItemsMessageAsync(lstLogs) : Task.CompletedTask;

                var lstGroupsMeta = new List<GroupMetadata>();
                while (_groupsMetadata.TryTake(out var item, 100))
                    lstGroupsMeta.Add(item);
                var groupTask = lstGroupsMeta.Count > 0 ? DbHandlers.Instance.Messages.ProcessGroupMetadataMessageAsync(lstGroupsMeta) : Task.CompletedTask;

                var lstTraces = new List<MessagingTraceItem>();
                while (_messagingTraceItems.TryTake(out var item, 100))
                    lstTraces.Add(item);
                var traceTask = lstTraces.Count > 0 ? DbHandlers.Instance.Messages.ProcessTraceItemsMessageAsync(lstTraces) : Task.CompletedTask;

                var lstCounters = new List<ICounterItem>();
                while (_counterItems.TryTake(out var item, 100))
                    lstCounters.Add(item);
                var counterTask = lstCounters.Count > 0 ? DbHandlers.Instance.Messages.ProcessCountersMessageAsync(lstCounters) : Task.CompletedTask;


                Task.WaitAll(logTask, groupTask, traceTask, counterTask);
            }
            finally
            {
                Interlocked.Exchange(ref _inProcess, 0);
            }
        }
        
        #region Overrides
        /// <inheritdoc />
        /// <summary>
        /// Gets the message processor
        /// </summary>
        /// <param name="server">Queue server object instance</param>
        /// <returns>Message processor instance</returns>
        protected override IMessageProcessorAsync GetMessageProcessorAsync(IMQueueRawServer server)
        {
            var processor = new ActionMessageProcessorAsync(Settings.ParallelMessagingProcess);
            processor.RegisterAction<RawRequestReceivedEventArgs>(async (message, cancellationToken) =>
            {
                var rcvMessage = server.ReceiverSerializer.Deserialize<object>(message.Request);
                if (rcvMessage is List<LogItem> msgLogs)
                {
                    if (msgLogs.Count == 0) return;
                    foreach(var item in msgLogs)
                        _logItems.Add(item);
                }
                else if (rcvMessage is List<GroupMetadata> msgGroups)
                {
                    if (msgGroups.Count == 0) return;
                    foreach (var item in msgGroups)
                        _groupsMetadata.Add(item);
                }
                else if (rcvMessage is List<MessagingTraceItem> msgTraces)
                {
                    if (msgTraces.Count == 0) return;
                    foreach (var item in msgTraces)
                        _messagingTraceItems.Add(item);
                }
                else if (rcvMessage is StatusItemCollection msgStatus)
                {
                    await DbHandlers.Instance.Messages.ProcessStatusMessageAsync(msgStatus).ConfigureAwait(false);
                }
                else if (rcvMessage is List<ICounterItem> msgCounters)
                {
                    if (msgCounters.Count == 0) return;
                    foreach (var item in msgCounters)
                        _counterItems.Add(item);
                }
            });
            return processor;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the queue server object
        /// </summary>
        /// <returns>IMQueueServer object instance</returns>
        protected override IMQueueRawServer GetQueueServer()
            => Core.Services.GetQueueRawServer();
        #endregion
    }
}