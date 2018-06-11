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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Diagnostics.Api.Models;
using TWCore.Diagnostics.Api.Models.Log;
using TWCore.Diagnostics.Api.Models.Status;
using TWCore.Diagnostics.Api.Models.Trace;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace.Storages;
using TWCore.Serialization;
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api.MessageHandlers.RavenDb
{
    public class RavenDbMessagesHandler : IDiagnosticMessagesHandler
    {
        public async Task ProcessLogItemsMessageAsync(List<LogItem> message)
        {
            Core.Log.InfoBasic("Storing LogItem messages...");

            foreach (var logItem in message)
            {
                await RavenHelper.ExecuteAsync(async session =>
                {
                    var logInfo = new NodeLogItem
                    {
                        Environment = logItem.EnvironmentName,
                        Machine = logItem.MachineName,
                        Application = logItem.ApplicationName,
                        InstanceId = logItem.InstanceId,
                        LogId = logItem.Id,
                        Assembly = logItem.AssemblyName,
                        Code = logItem.Code,
                        Group = logItem.GroupName,
                        Level = logItem.Level,
                        Message = logItem.Message,
                        Type = logItem.TypeName,
                        Exception = logItem.Exception,
                        Timestamp = logItem.Timestamp
                    };
                    await session.StoreAsync(logInfo).ConfigureAwait(false);
                    await session.SaveChangesAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        public async Task ProcessTraceItemsMessageAsync(List<MessagingTraceItem> message)
        {
            Core.Log.InfoBasic("Storing TraceItem messages...");
            foreach (var traceItem in message)
            {
                await RavenHelper.ExecuteAsync(async session =>
                {
                    var traceInfo = new NodeTraceItem
                    {
                        Environment = traceItem.EnvironmentName,
                        Machine = traceItem.MachineName,
                        Application = traceItem.ApplicationName,
                        InstanceId = traceItem.InstanceId,
                        TraceId = traceItem.Id,
                        Group = traceItem.GroupName,
                        Name = traceItem.TraceName,
                        Timestamp = traceItem.Timestamp
                    };
                    await session.StoreAsync(traceInfo).ConfigureAwait(false);

                    using (var ms = new MemoryStream())
                    {
                        if (traceItem.TraceObject != null)
                        {
                            traceItem.TraceObject.SerializeToNBinary(ms);
                            ms.Position = 0;
                        }

                        session.Advanced.Attachments.Store(traceInfo.Id, "Trace", ms, traceItem.TraceObject?.GetType().FullName);

                        await session.SaveChangesAsync().ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
            }
        }

        public async Task ProcessStatusMessageAsync(StatusItemCollection message)
        {
            Core.Log.InfoBasic("Storing StatusCollection message...");
            await RavenHelper.ExecuteAsync(async session =>
            {
                var nodeStatus = await session.Advanced.AsyncDocumentQuery<NodeStatusItem>()
                    .WhereEquals(node => node.InstanceId, message.InstanceId)
                    .FirstOrDefaultAsync().ConfigureAwait(false);

                if (nodeStatus == null)
                {
                    var newStatus = NodeStatusItem.Create(message);
                    await session.StoreAsync(newStatus).ConfigureAwait(false);
                }
                else
                {
                    nodeStatus.Timestamp = message.Timestamp;
                    nodeStatus.FillValues(message);
                }
                await session.SaveChangesAsync().ConfigureAwait(false);

            }).ConfigureAwait(false);
        }
    }
}