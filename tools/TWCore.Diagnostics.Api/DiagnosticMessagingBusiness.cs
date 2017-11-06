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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Diagnostics.Api.Models;
using TWCore.Diagnostics.Api.Models.Log;
using TWCore.Diagnostics.Api.Models.Status;
using TWCore.Diagnostics.Api.Models.Trace;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace;
using TWCore.Diagnostics.Trace.Storages;
using TWCore.Messaging;
using TWCore.Services.Messaging;

namespace TWCore.Diagnostics.Api
{
    public class DiagnosticMessagingBusiness : BusinessBase<List<LogItem>, List<MessagingTraceItem>, StatusItemCollection>
    {
        private readonly object _locker = new object();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override object OnProcess(List<LogItem> message)
        {
            if (message == null || message.Count == 0) return ResponseMessage.NoResponse;
            Core.Log.InfoBasic("Storing Log Info...");
            RavenHelper.Execute(session =>
            {
                foreach (var logItem in message)
                {
                    NodeInfo nodeInfo;
                    lock (_locker)
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
                            session.Store(nodeInfo);
                            session.SaveChanges();
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
                    session.Store(logInfo);
                    session.SaveChanges();
                }
            });
            return ResponseMessage.NoResponse;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override object OnProcess(List<MessagingTraceItem> message)
        {
            if (message == null || message.Count == 0) return ResponseMessage.NoResponse;
            Core.Log.InfoBasic("Storing Trace Info...");
            RavenHelper.Execute(session =>
            {
                foreach (var traceItem in message)
                {
                    NodeInfo nodeInfo;
                    lock (_locker)
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
                            session.Store(nodeInfo);
                            session.SaveChanges();
                        }
                    }

                    var traceInfo = new NodeTraceItem
                    {
                        TraceId = traceItem.Id,
                        NodeInfoId = nodeInfo.Id,
                        Group = traceItem.GroupName,
                        Name = traceItem.TraceName,
                        TraceObject = traceItem.TraceObject,
                        Timestamp = traceItem.Timestamp
                    };
                    session.Store(traceInfo);
                    session.SaveChanges();
                }
            });
            return ResponseMessage.NoResponse;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override object OnProcess(StatusItemCollection message)
        {
            if (message == null) return ResponseMessage.NoResponse;

            Core.Log.Warning("Status Received.");
            Core.Log.InfoBasic("Storing Status Info...");
            RavenHelper.Execute(session =>
            {
                NodeInfo nodeInfo;
                lock (_locker)
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
                        session.Store(nodeInfo);
                        session.SaveChanges();
                    }
                }

                var nodeStatus = (from node in session.Query<NodeStatusItem>()
                                  where node.NodeInfoId == nodeInfo.Id &&
                                        node.Date == message.Timestamp.Date &&
                                        node.StartTime == message.StartTime
                                  select node).FirstOrDefault();

                var newStatus = new NodeStatusItem
                {
                    Id = nodeStatus?.Id,
                    NodeInfoId = nodeInfo.Id,
                    Date = message.Timestamp.Date,
                    StartTime = message.StartTime,
                    Timestamp = message.Timestamp,
                    Children = message.Items?.Select(GetNodeStatusChild).ToList()
                };
                session.Store(newStatus);
                session.SaveChanges();
            });
            return ResponseMessage.NoResponse;
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

        #region Not usable methods
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static object GetNodeById(StatusItem item, string id)
        //{
        //    if (item.Id == id)
        //        return item;
        //    if (item.Values != null)
        //    {
        //        foreach (var itemValue in item.Values)
        //        {
        //            if (itemValue.Id == id)
        //                return itemValue;
        //            if (itemValue.Values == null) continue;
        //            foreach (var statusItemValueItem in itemValue.Values)
        //            {
        //                if (statusItemValueItem.Id == id)
        //                    return statusItemValueItem;
        //            }
        //        }
        //    }
        //    if (item.Children == null) return null;
        //    foreach (var child in item.Children)
        //    {
        //        var value = GetNodeById(child, id);
        //        if (value != null)
        //            return value;
        //    }
        //    return null;
        //}
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static object GetNodeById(StatusItemCollection collection, string id)
        //{
        //    if (collection?.Items == null) return null;
        //    foreach (var item in collection.Items)
        //    {
        //        var value = GetNodeById(item, id);
        //        if (value != null)
        //            return value;
        //    }
        //    return null;
        //}
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static void MergeStatus(StatusItemCollection newCollection, StatusItemCollection oldCollection)
        //{
        //    if (newCollection?.Items != null && oldCollection?.Items != null)
        //    {
        //        foreach (var item in newCollection.Items)
        //            MergeStatus(item, GetNodeById(oldCollection, item.Id) as StatusItem);
        //    }
        //}
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static void MergeStatus(StatusItem newItem, StatusItem oldItem)
        //{
        //    if (oldItem == null) return;
        //    if (newItem.Values != null)
        //    {
        //        foreach (var itemValue in newItem.Values)
        //        {

        //        }
        //    }
        //}
        #endregion
    }
}