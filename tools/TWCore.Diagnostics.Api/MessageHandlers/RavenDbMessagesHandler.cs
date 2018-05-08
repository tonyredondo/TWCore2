using System.Collections.Generic;
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
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api.MessageHandlers
{
    public class RavenDbMessagesHandler : IDiagnosticMessagesHandler
    {
        private readonly object _locker = new object();

        public Task ProcessLogItemsMessage(List<LogItem> message)
        {
            Core.Log.InfoBasic("Storing Log Info...");
            foreach (var logItem in message)
            {
                RavenHelper.Execute(session =>
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
                });
            }

            return Task.CompletedTask;
        }

        public Task ProcessTraceItemsMessage(List<MessagingTraceItem> message)
        {
            Core.Log.InfoBasic("Storing Trace Info...");
            foreach (var traceItem in message)
            {
                RavenHelper.Execute(session =>
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
                });
            }
            return Task.CompletedTask;
        }

        public Task ProcessStatusMessage(StatusItemCollection message)
        {
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
                    session.Store(newStatus);
                }
                else
                {
                    nodeStatus.Timestamp = message.Timestamp;
                    nodeStatus.Children = message.Items?.Select(GetNodeStatusChild).ToList();
                }
                session.SaveChanges();
            });
            return Task.CompletedTask;
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