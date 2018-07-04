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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TWCore.Diagnostics.Api.Models;
using TWCore.Diagnostics.Api.Models.Log;
using TWCore.Diagnostics.Api.Models.Status;
using TWCore.Diagnostics.Api.Models.Trace;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace.Storages;
using TWCore.IO;
using TWCore.Messaging;
using TWCore.Serialization;

// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api.MessageHandlers.RavenDb
{
    public class RavenDbMessagesHandler : IDiagnosticMessagesHandler
    {
        private static readonly JsonTextSerializer JsonSerializer = new JsonTextSerializer
        {
            Indent = true,
            EnumsAsStrings = true,
            UseCamelCase = true
        };

        public async Task ProcessLogItemsMessageAsync(List<LogItem> message)
        {
            using (Watch.Create("Processing LogItems List Message", LogLevel.InfoBasic))
            {
                await RavenHelper.BulkInsertAsync(async bulkOp =>
                {
                    try
                    {
                        foreach (var logItem in message)
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
                            await bulkOp.StoreAsync(logInfo).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        if (bulkOp != null)
                        {
                            await bulkOp.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }).ConfigureAwait(false);
            }
        }

        public async Task ProcessTraceItemsMessageAsync(List<MessagingTraceItem> message)
        {
            using (Watch.Create("Processing TraceItems List Message", LogLevel.InfoBasic))
            {
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
                            Tags = traceItem.Tags?.Select(i => i.ToString()).Join(", "),
                            Group = traceItem.GroupName,
                            Name = traceItem.TraceName,
                            Timestamp = traceItem.Timestamp
                        };
                        await session.StoreAsync(traceInfo).ConfigureAwait(false);

                        if (traceItem.TraceObject != null)
                        {
                            using (var msNBinary = new RecycleMemoryStream())
                            using (var msXml = new RecycleMemoryStream())
                            using (var msJson = new RecycleMemoryStream())
                            {
                                try
                                {
                                    traceItem.TraceObject.SerializeToNBinary(msNBinary);
                                    msNBinary.Position = 0;
                                    session.Advanced.Attachments.Store(traceInfo.Id, "Trace", msNBinary, traceItem.TraceObject?.GetType().FullName);
                                }
                                catch (Exception)
                                {
                                    //
                                }

                                try
                                {
                                    if (traceItem.TraceObject is SerializedObject serObj)
                                    {
                                        var value = serObj.GetValue();
                                        switch (value)
                                        {
                                            case string valStr:
                                                await msXml.WriteTextAsync(valStr).ConfigureAwait(false);
                                                break;
                                            case ResponseMessage rsMessage when rsMessage?.Body != null:
                                                rsMessage.Body?.SerializeToXml(msXml);
                                                break;
                                            case RequestMessage rqMessage when rqMessage?.Body != null:
                                                rqMessage.Body?.SerializeToXml(msXml);
                                                break;
                                            default:
                                                if (value != null)
                                                {
                                                    serObj.GetValue()?.SerializeToXml(msXml);
                                                }

                                                break;
                                        }
                                    }
                                    else
                                        traceItem.TraceObject.SerializeToXml(msXml);

                                    msXml.Position = 0;
                                    session.Advanced.Attachments.Store(traceInfo.Id, "TraceXml", msXml, traceItem.TraceObject?.GetType().FullName);
                                }
                                catch (Exception)
                                {
                                    //
                                }

                                try
                                {
                                    if (traceItem.TraceObject is SerializedObject serObj)
                                    {
                                        var value = serObj.GetValue();
                                        switch (value)
                                        {
                                            case string valStr:
                                                await msJson.WriteTextAsync(valStr).ConfigureAwait(false);
                                                break;
                                            case ResponseMessage rsMessage when rsMessage?.Body != null:
                                                JsonSerializer.Serialize(rsMessage.Body, msJson);
                                                break;
                                            case RequestMessage rqMessage when rqMessage?.Body != null:
                                                JsonSerializer.Serialize(rqMessage.Body, msJson);
                                                break;
                                            default:
                                                if (value != null)
                                                {
                                                    JsonSerializer.Serialize(value, msJson);
                                                }

                                                break;
                                        }
                                    }
                                    else
                                        JsonSerializer.Serialize(traceItem.TraceObject, msJson);

                                    msJson.Position = 0;
                                    session.Advanced.Attachments.Store(traceInfo.Id, "TraceJson", msJson, traceItem.TraceObject?.GetType().FullName);
                                }
                                catch (Exception)
                                {
                                    //
                                }

                                await session.SaveChangesAsync().ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await session.SaveChangesAsync().ConfigureAwait(false);
                        }
                    }).ConfigureAwait(false);
                }
            }
        }

        public async Task ProcessStatusMessageAsync(StatusItemCollection message)
        {
            using (Watch.Create("Processing StatusItemCollection Message", LogLevel.InfoBasic))
            {
                await RavenHelper.ExecuteAsync(async session =>
                {
                    var instanceIds = await session.Advanced.AsyncRawQuery<StatusId>(@"
                        from NodeStatusItems 
                        where InstanceId = $instanceId
                        select ID() as 'Id'
                    ")
                        .AddParameter("instanceId", message.InstanceId)
                        .ToListAsync().ConfigureAwait(false);
                    
                    foreach (var id in instanceIds)
                        session.Delete(id.Id);

                    var newStatus = NodeStatusItem.Create(message);
                    await session.StoreAsync(newStatus).ConfigureAwait(false);
                    await session.SaveChangesAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        private class StatusId
        {
            public string Id { get; set; }
        } 
    }
}