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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TWCore.Compression;
using TWCore.Diagnostics.Api.MessageHandlers.RavenDb.Indexes;
using TWCore.Diagnostics.Api.Models;
using TWCore.Diagnostics.Api.Models.Counters;
using TWCore.Diagnostics.Api.Models.Log;
using TWCore.Diagnostics.Api.Models.Status;
using TWCore.Diagnostics.Api.Models.Trace;
using TWCore.Diagnostics.Counters;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace.Storages;
using TWCore.IO;
using TWCore.Messaging;
using TWCore.Serialization;
using TWCore.Serialization.NSerializer;

// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api.MessageHandlers.RavenDb
{
    public class RavenDbMessagesHandler : IDiagnosticMessagesHandler
    {
        private static readonly DiagnosticsSettings Settings = Core.GetSettings<DiagnosticsSettings>();
        private static readonly ICompressor Compressor = new GZipCompressor();
        private static readonly NBinarySerializer NBinarySerializer = new NBinarySerializer
        {
            Compressor = Compressor
        };
        private static readonly XmlTextSerializer XmlSerializer = new XmlTextSerializer
        {
            Compressor = Compressor
        };
        private static readonly JsonTextSerializer JsonSerializer = new JsonTextSerializer
        {
            Compressor = Compressor,
            Indent = true,
            EnumsAsStrings = true,
            UseCamelCase = true
        };

        /// <summary>
        /// Initialize handler
        /// </summary>
        public void Init()
        {
            RavenHelper.Init();
        }

        public async Task ProcessLogItemsMessageAsync(List<LogItem> message)
        {
            if (message is null) return;
            using (Watch.Create($"Processing LogItems List Message [{message.Count} items]", LogLevel.InfoBasic))
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
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                }).ConfigureAwait(false);
            }
        }

        public async Task ProcessGroupMetadataMessageAsync(List<GroupMetadata> message)
        {
            if (message is null) return;
            using (Watch.Create($"Processing GroupMetadata List Message [{message.Count} items]", LogLevel.InfoBasic))
            {
                await RavenHelper.BulkInsertAsync(async bulkOp =>
                {
                    try
                    {
                        foreach (var groupMetaItem in message)
                            await bulkOp.StoreAsync(groupMetaItem).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                }).ConfigureAwait(false);
            }
        }

        public async Task ProcessTraceItemsMessageAsync(List<MessagingTraceItem> message)
        {
            if (message is null) return;
            using (Watch.Create($"Processing TraceItems List Message [{message.Count} items]", LogLevel.InfoBasic))
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
                            var lstExtensions = new List<string>();
                            using (var msNBinary = new RecycleMemoryStream())
                            using (var msXml = new RecycleMemoryStream())
                            using (var msJson = new RecycleMemoryStream())
                            using (var msTxt = new RecycleMemoryStream())
                            {

                                var writeBinary = Settings.WriteInBinary || Settings.ForceBinaryOnApp.Contains(traceItem.ApplicationName, StringComparer.OrdinalIgnoreCase);
                                if (writeBinary)
                                {
                                    #region NBinary
                                    try
                                    {
                                        NBinarySerializer.Serialize(traceItem.TraceObject, msNBinary);
                                        msNBinary.Position = 0;
                                        if (Settings.StoreTracesToDisk)
                                            await TraceDiskStorage.StoreAsync(traceInfo, msNBinary, ".nbin.gz").ConfigureAwait(false);
                                        else
                                            session.Advanced.Attachments.Store(traceInfo.Id, "Trace", msNBinary, traceItem.TraceObject?.GetType().FullName);
                                    }
                                    catch (Exception ex)
                                    {
                                        Core.Log.Write(ex);
                                    }
                                    #endregion
                                }

                                var writeInXml = Settings.WriteInXml || Settings.ForceXmlOnApp.Contains(traceItem.ApplicationName, StringComparer.OrdinalIgnoreCase);
                                if (writeInXml)
                                {
                                    #region Xml Serializer
                                    try
                                    {
                                        var bXml = false;
                                        if (traceItem.TraceObject is SerializedObject serObj)
                                        {
                                            var value = serObj.GetValue();
                                            switch (value)
                                            {
                                                case ResponseMessage rsMessage when rsMessage?.Body != null:
                                                    var bodyValueRS = rsMessage.Body.GetValue();
                                                    if (!(bodyValueRS is IDictionary))
                                                    {
                                                        XmlSerializer.Serialize(bodyValueRS, msXml);
                                                        bXml = true;
                                                    }
                                                    break;
                                                case RequestMessage rqMessage when rqMessage?.Body != null:
                                                    var bodyValueRQ = rqMessage.Body.GetValue();
                                                    if (!(bodyValueRQ is IDictionary))
                                                    {
                                                        XmlSerializer.Serialize(bodyValueRQ, msXml);
                                                        bXml = true;
                                                    }
                                                    break;
                                                default:
                                                    if (value != null && value.GetType() != typeof(string))
                                                    {
                                                        if (!(value is IDictionary))
                                                        {
                                                            XmlSerializer.Serialize(value, msXml);
                                                            bXml = true;
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        else if (!(traceItem.TraceObject is string) && !(traceItem.TraceObject is IDictionary))
                                        {
                                            XmlSerializer.Serialize(traceItem.TraceObject, msXml);
                                            bXml = true;
                                        }

                                        if (bXml)
                                        {
                                            msXml.Position = 0;
                                            if (Settings.StoreTracesToDisk)
                                                await TraceDiskStorage.StoreAsync(traceInfo, msXml, ".xml.gz").ConfigureAwait(false);
                                            else
                                                session.Advanced.Attachments.Store(traceInfo.Id, "TraceXml", msXml, traceItem.TraceObject?.GetType().FullName);
                                            lstExtensions.Add("XML");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Core.Log.Write(ex);
                                    }
                                    #endregion
                                }

                                var writeInJson = Settings.WriteInJson || Settings.ForceJsonOnApp.Contains(traceItem.ApplicationName, StringComparer.OrdinalIgnoreCase);
                                if (writeInJson)
                                {
                                    #region Json Serializer
                                    try
                                    {
                                        var bJson = false;
                                        if (traceItem.TraceObject is SerializedObject serObj)
                                        {
                                            var value = serObj.GetValue();
                                            switch (value)
                                            {
                                                case ResponseMessage rsMessage when rsMessage?.Body != null:
                                                    JsonSerializer.Serialize(rsMessage.Body.GetValue(), msJson);
                                                    bJson = true;
                                                    break;
                                                case RequestMessage rqMessage when rqMessage?.Body != null:
                                                    JsonSerializer.Serialize(rqMessage.Body.GetValue(), msJson);
                                                    bJson = true;
                                                    break;
                                                default:
                                                    if (value != null && value.GetType() != typeof(string))
                                                    {
                                                        JsonSerializer.Serialize(value, msJson);
                                                        bJson = true;
                                                    }
                                                    break;
                                            }
                                        }
                                        else if (!(traceItem.TraceObject is string))
                                        {
                                            JsonSerializer.Serialize(traceItem.TraceObject, msJson);
                                            bJson = true;
                                        }

                                        if (bJson)
                                        {
                                            msJson.Position = 0;
                                            if (Settings.StoreTracesToDisk)
                                                await TraceDiskStorage.StoreAsync(traceInfo, msJson, ".json.gz").ConfigureAwait(false);
                                            else
                                                session.Advanced.Attachments.Store(traceInfo.Id, "TraceJson", msJson, traceItem.TraceObject?.GetType().FullName);
                                            lstExtensions.Add("JSON");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Core.Log.Write(ex);
                                    }
                                    #endregion
                                }

                                #region String Serializer
                                try
                                {
                                    var bTxt = false;
                                    if (traceItem.TraceObject is SerializedObject serObj)
                                    {
                                        var value = serObj.GetValue();
                                        if (value is string txtValue)
                                        {
                                            msTxt.Write(Encoding.UTF8.GetBytes(txtValue).ToGzip().AsReadOnlySpan());
                                            bTxt = true;
                                        }
                                    }
                                    else if (traceItem.TraceObject is string strObj)
                                    {
                                        msTxt.Write(Encoding.UTF8.GetBytes(strObj).ToGzip().AsReadOnlySpan());
                                        bTxt = true;
                                    }

                                    if (bTxt)
                                    {
                                        msTxt.Position = 0;
                                        if (Settings.StoreTracesToDisk)
                                            await TraceDiskStorage.StoreAsync(traceInfo, msTxt, ".txt.gz").ConfigureAwait(false);
                                        else
                                            session.Advanced.Attachments.Store(traceInfo.Id, "TraceTxt", msTxt, traceItem.TraceObject?.GetType().FullName);
                                        lstExtensions.Add("TXT");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Core.Log.Write(ex);
                                }
                                #endregion

                                if (lstExtensions.Count > 0)
                                {
                                    traceInfo.Formats = lstExtensions.ToArray();
                                    await session.SaveChangesAsync().ConfigureAwait(false);
                                }
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
            if (message is null) return;
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

        public async Task ProcessCountersMessageAsync(List<ICounterItem> message)
        {
            if (message is null) return;
            using (var watch = Watch.Create($"Processing Counter item List Message [{message.Count} items]", LogLevel.InfoBasic))
            {
                var lstCounters = new List<NodeCountersValue>();

                foreach (var counter in message)
                {
                    NodeCountersItem cEntity = null;

                    await RavenHelper.ExecuteAsync(async session =>
                    {
                        cEntity = await session.Advanced.AsyncDocumentQuery<NodeCountersItem, Counters_CounterSelection>()
                            .WhereEquals(item => item.Environment, counter.Environment)
                            .WhereEquals(item => item.Application, counter.Application)
                            .WhereEquals(item => item.Category, counter.Category)
                            .WhereEquals(item => item.Name, counter.Name)
                            .FirstOrDefaultAsync().ConfigureAwait(false);

                        if (cEntity == null)
                        {
                            cEntity = new NodeCountersItem
                            {
                                Environment = counter.Environment,
                                Application = counter.Application,
                                CountersId = Guid.NewGuid(),
                                Category = counter.Category,
                                Name = counter.Name,
                                Level = counter.Level,
                                Kind = counter.Kind,
                                Unit = counter.Unit,
                                Type = counter.Type,
                                TypeOfValue = counter.TypeOfValue.Name
                            };
                            await session.StoreAsync(cEntity).ConfigureAwait(false);
                            await session.SaveChangesAsync().ConfigureAwait(false);
                        }
                    }).ConfigureAwait(false);

                    if (counter is CounterItem<int> intCounter)
                    {
                        foreach (var value in intCounter.Values)
                        {
                            var nValue = new NodeCountersValue
                            {
                                CountersId = cEntity.CountersId,
                                Timestamp = value.Timestamp,
                                Value = value.Value
                            };
                            lstCounters.Add(nValue);
                        }
                    }
                    else if (counter is CounterItem<double> doubleCounter)
                    {
                        foreach (var value in doubleCounter.Values)
                        {
                            var nValue = new NodeCountersValue
                            {
                                CountersId = cEntity.CountersId,
                                Timestamp = value.Timestamp,
                                Value = value.Value
                            };
                            lstCounters.Add(nValue);
                        }
                    }
                    else if (counter is CounterItem<decimal> decimalCounter)
                    {
                        foreach (var value in decimalCounter.Values)
                        {
                            var nValue = new NodeCountersValue
                            {
                                CountersId = cEntity.CountersId,
                                Timestamp = value.Timestamp,
                                Value = value.Value
                            };
                            lstCounters.Add(nValue);
                        }
                    }
                    
                }
                
                await RavenHelper.BulkInsertAsync(async bulkOp =>
                {
                    try
                    {
                        foreach (var value in lstCounters)
                            await bulkOp.StoreAsync(value).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                }).ConfigureAwait(false);
            }
        }

        private class StatusId
        {
            public string Id { get; set; }
        }
    }
}