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
using System.Collections.Generic;
using System.Threading.Tasks;
using TWCore.Diagnostics.Api.Models;
using TWCore.Diagnostics.Api.Models.Log;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace.Storages;
using TWCore.Messaging;
using TWCore.Services;
using TWCore.Services.Messaging;

namespace TWCore.Diagnostics.Api
{
    public class DiagnosticMessagingBusiness : BusinessAsyncBase<List<LogItem>, List<MessagingTraceItem>, StatusItemCollection>
    {
        public const string ConnectionString = "Filename=mydb.db;Mode=Exclusive";
        public const string NodeInfoCollectionName = "nodes";
        public const string LogCollectionName = "logs";
        
        
        protected override Task<object> OnProcessAsync(List<LogItem> message)
        {
            if (message == null || message.Count == 0) return Task.FromResult(ResponseMessage.NoResponse);

            /*
            using (var db = new LiteDatabase(ConnectionString))
            {
                var nodeCol = db.GetCollection<NodeInfo>(NodeInfoCollectionName);
                nodeCol.EnsureIndex(i => i.Machine);
                nodeCol.EnsureIndex(i => i.Application);
                nodeCol.EnsureIndex(i => i.Environment);
                nodeCol.EnsureIndex(i => i.Date);

                foreach (var logItem in message)
                {
                    var nodeInfo = nodeCol.FindOne(i => 
                        i.Date == logItem.Timestamp.Date && 
                        i.Machine == logItem.MachineName &&
                        i.Environment == logItem.EnvironmentName &&
                        i.Application == logItem.ApplicationName);

                    if (nodeInfo == null)
                    {
                        nodeInfo = new NodeInfo
                        {
                            Application =  logItem.ApplicationName,
                            Environment = logItem.EnvironmentName,
                            Machine = logItem.MachineName,
                            Date = logItem.Timestamp.Date
                        };
                        nodeCol.Insert(nodeInfo);
                    }

                    var col = db.GetCollection<NodeLogItem>(LogCollectionName);
                    col.EnsureIndex(i => i.NodeInfoId);
                    col.Insert(new NodeLogItem
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
                    });
                }
            }
            */
            
            Core.Log.Warning("Log Items Received.");
            return Task.FromResult(ResponseMessage.NoResponse);
        }

        protected override Task<object> OnProcessAsync(List<MessagingTraceItem> message)
        {
            Core.Log.Warning("Trace Items Received.");
            return Task.FromResult(ResponseMessage.NoResponse);
        }

        protected override Task<object> OnProcessAsync(StatusItemCollection message)
        {
            Core.Log.Warning("Status Received.");
            return Task.FromResult(ResponseMessage.NoResponse);
        }
    }
}