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
using System.Threading.Tasks;
using Raven.Client.Documents;
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

            RavenHelper.Execute(session =>
            {
                foreach (var logItem in message)
                {
                    var nodeInfo = (from node in session.Query<NodeInfo>()
                        where node.Date == logItem.Timestamp.Date &&
                              node.Machine == logItem.MachineName &&
                              node.Environment == logItem.EnvironmentName &&
                              node.Application == logItem.ApplicationName
                        select  node).FirstOrDefault();
                   
                    if (nodeInfo == null)
                    {
                        nodeInfo = new NodeInfo
                        {
                            Application =  logItem.ApplicationName,
                            Environment = logItem.EnvironmentName,
                            Machine = logItem.MachineName,
                            Date = logItem.Timestamp.Date
                        };
                        session.Store(nodeInfo);
                        session.SaveChanges();
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