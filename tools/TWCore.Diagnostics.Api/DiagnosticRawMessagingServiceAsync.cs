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

using System.Collections.Generic;
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
        protected override void OnInit(string[] args)
        {
            Core.GlobalSettings.LargeObjectHeapCompactTimeoutInMinutes = 1;
            Core.GlobalSettings.ReloadSettings();
            SerializerManager.SupressFileExtensionWarning = true;
            base.OnInit(args);
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
            var processor = new ActionMessageProcessorAsync();
            processor.RegisterAction<RawRequestReceivedEventArgs>(async (message, cancellationToken) =>
            {
                var rcvMessage = server.ReceiverSerializer.Deserialize<object>(message.Request);
                if (rcvMessage is List<LogItem> msgLogs)
                {
                    if (msgLogs is null || msgLogs.Count == 0) return;
                    await DbHandlers.Instance.Messages.ProcessLogItemsMessageAsync(msgLogs).ConfigureAwait(false);
                }
                else if (rcvMessage is List<GroupMetadata> msgGroups)
                {
                    if (msgGroups is null || msgGroups.Count == 0) return;
                    await DbHandlers.Instance.Messages.ProcessGroupMetadataMessageAsync(msgGroups).ConfigureAwait(false);
                }
                else if (rcvMessage is List<MessagingTraceItem> msgTraces)
                {
                    if (msgTraces is null || msgTraces.Count == 0) return;
                    await DbHandlers.Instance.Messages.ProcessTraceItemsMessageAsync(msgTraces).ConfigureAwait(false);
                }
                else if (rcvMessage is StatusItemCollection msgStatus)
                {
                    if (msgStatus is null) return;
                    await DbHandlers.Instance.Messages.ProcessStatusMessageAsync(msgStatus).ConfigureAwait(false);
                }
                else if (rcvMessage is List<ICounterItem> msgCounters)
                {
                    if (msgCounters is null || msgCounters.Count == 0) return;
                    await DbHandlers.Instance.Messages.ProcessCountersMessageAsync(msgCounters).ConfigureAwait(false);
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