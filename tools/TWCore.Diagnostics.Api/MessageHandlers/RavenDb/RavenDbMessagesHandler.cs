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
using TWCore.Threading;
using TWCore.Serialization;
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api.MessageHandlers.RavenDb
{
    public class RavenDbMessagesHandler : IDiagnosticMessagesHandler
    {
        private readonly AsyncLock _lock = new AsyncLock();

        public async Task ProcessLogItemsMessage(List<LogItem> message)
        {
            Core.Log.InfoBasic("Storing Log Info...");
            foreach (var logItem in message)
            {
                await RavenHelper.ExecuteAsync(async session => {

                    NodeInfo nodeInfo;
                    using (await _lock.LockAsync().ConfigureAwait(false))
                    {
                        nodeInfo = (from node in session.Query<NodeInfo>()
                                    where node.Date == logItem.Timestamp.Date &&
                                          node.Machine == logItem.MachineName &&
                                          node.Environment == logItem.EnvironmentName &&
                                          node.Application == logItem.ApplicationName
                                    select node).FirstOrDefault();

                        if (nodeInfo == null)
                        {
                            nodeInfo = new NodeInfo
                            {
                                Application = logItem.ApplicationName,
                                Environment = logItem.EnvironmentName,
                                Machine = logItem.MachineName,
                                Date = logItem.Timestamp.Date
                            };
                            await session.StoreAsync(nodeInfo).ConfigureAwait(false);
                            await session.SaveChangesAsync().ConfigureAwait(false);
                        }
                    }

                    var logInfo = new NodeLogItem
                    {
                        NodeInfoId = nodeInfo.Id,
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

        public async Task ProcessTraceItemsMessage(List<MessagingTraceItem> message)
        {
            Core.Log.InfoBasic("Storing Trace Info...");
            foreach (var traceItem in message)
            {
                await RavenHelper.ExecuteAsync(async session =>
                {
                    NodeInfo nodeInfo;
                    using (await _lock.LockAsync().ConfigureAwait(false))
                    {
                        nodeInfo = (from node in session.Query<NodeInfo>()
                            where node.Date == traceItem.Timestamp.Date &&
                                  node.Machine == traceItem.MachineName &&
                                  node.Environment == traceItem.EnvironmentName &&
                                  node.Application == traceItem.ApplicationName
                            select node).FirstOrDefault();

                        if (nodeInfo == null)
                        {
                            nodeInfo = new NodeInfo
                            {
                                Application = traceItem.ApplicationName,
                                Environment = traceItem.EnvironmentName,
                                Machine = traceItem.MachineName,
                                Date = traceItem.Timestamp.Date
                            };
                            await session.StoreAsync(nodeInfo).ConfigureAwait(false);
                            await session.SaveChangesAsync().ConfigureAwait(false);
                        }
                    }

                    var traceInfo = new NodeTraceItem
                    {
                        TraceId = traceItem.Id,
                        NodeInfoId = nodeInfo.Id,
                        Group = traceItem.GroupName,
                        Name = traceItem.TraceName,
                        //TraceObject = traceItem.TraceObject,
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

        public async Task ProcessStatusMessage(StatusItemCollection message)
        {
            Core.Log.InfoBasic("Storing Status Info...");
            await RavenHelper.ExecuteAsync(async session =>
            {
                NodeInfo nodeInfo;
                using (await _lock.LockAsync().ConfigureAwait(false))
                {
                    nodeInfo = (from node in session.Query<NodeInfo>()
                                where node.Date == message.Timestamp.Date &&
                                      node.Machine == message.MachineName &&
                                      node.Environment == message.EnvironmentName &&
                                      node.Application == message.ApplicationName
                                select node).FirstOrDefault();

                    if (nodeInfo == null)
                    {
                        nodeInfo = new NodeInfo
                        {
                            Application = message.ApplicationName,
                            Environment = message.EnvironmentName,
                            Machine = message.MachineName,
                            Date = message.Timestamp.Date
                        };
                        await session.StoreAsync(nodeInfo).ConfigureAwait(false);
                        await session.SaveChangesAsync().ConfigureAwait(false);
                    }
                }

                var nodeStatus = (from node in session.Query<NodeStatusItem>()
                                  where node.NodeInfoId == nodeInfo.Id &&
                                        node.Date == message.Timestamp.Date &&
                                        node.StartTime == message.StartTime
                                  select node).FirstOrDefault();

                if (nodeStatus == null)
                {
                    var newStatus = new NodeStatusItem
                    {
                        NodeInfoId = nodeInfo.Id,
                        Date = message.Timestamp.Date,
                        StartTime = message.StartTime,
                        Timestamp = message.Timestamp,
                        Children = message.Items?.Select(GetNodeStatusChild).ToList()
                    };
                    await session.StoreAsync(newStatus).ConfigureAwait(false);
                }
                else
                {
                    nodeStatus.Timestamp = message.Timestamp;
                    nodeStatus.Children = message.Items?.Select(GetNodeStatusChild).ToList();
                }
                await session.SaveChangesAsync().ConfigureAwait(false);

            }).ConfigureAwait(false);
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NodeStatusChildItem GetNodeStatusChild(StatusItem item)
        {
            return new NodeStatusChildItem
            {
                Id = item.Id,
                Name = item.Name,
                Values = item.Values?.Select(GetNodeStatusValue).ToList(),
                Children = item.Children?.Select(GetNodeStatusChild).ToList()
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NodeStatusChildValue GetNodeStatusValue(StatusItemValue value)
        {
            return new NodeStatusChildValue
            {
                Id = value.Id,
                Key = value.Key,
                Value = value.Value,
                Type = value.Type,
                Status = value.Status,
                Values = value.Values?.Select(i => new NodeStatusChildValue
                {
                    Id = i.Id,
                    Key = i.Name,
                    Value = i.Value,
                    Type = i.Type,
                    Status = i.Status
                }).ToList()
            };
        }
    }
}