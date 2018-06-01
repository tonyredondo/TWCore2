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
using System.Linq;
using System.Collections.Generic;
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
// ReSharper disable ClassNeverInstantiated.Global

namespace TWCore.Diagnostics.Api
{
	public class DbHandlers : IDiagnosticHandler
	{
		public static IDiagnosticHandler Instance => Singleton<DbHandlers>.Instance;

		private readonly IDiagnosticHandler[] _handlers;

		#region Public Methods
		public IDiagnosticMessagesHandler Messages { get; } 
		public IDiagnosticQueryHandler Query { get; }
		#endregion

		private DbHandlers()
		{
			_handlers = Core.Injector.GetAllInstances<IDiagnosticHandler>();
			if (_handlers == null)
				throw new Exception("Handlers are not defined.");
			Messages = new MessagesHandler(this);
			Query = new QueryHandler(this);
		}

		#region Nested Types
		
		private class MessagesHandler : IDiagnosticMessagesHandler
		{
			private readonly DbHandlers _parent;

			internal MessagesHandler(DbHandlers parent)
			{
				_parent = parent;
			}

			#region IDiagnosticMessagesHandler
			public async Task ProcessLogItemsMessageAsync(List<LogItem> message)
			{
				foreach (var item in _parent._handlers)
					await item.Messages.ProcessLogItemsMessageAsync(message).ConfigureAwait(false);
			}
			public async Task ProcessTraceItemsMessageAsync(List<MessagingTraceItem> message)
			{
				foreach (var item in _parent._handlers)
					await item.Messages.ProcessTraceItemsMessageAsync(message).ConfigureAwait(false);
			}
			public async Task ProcessStatusMessageAsync(StatusItemCollection message)
			{
				foreach (var item in _parent._handlers)
					await item.Messages.ProcessStatusMessageAsync(message).ConfigureAwait(false);
			}
			#endregion
		}
		private class QueryHandler : IDiagnosticQueryHandler
		{
			private readonly DbHandlers _parent;

			internal QueryHandler(DbHandlers parent)
			{
				_parent = parent;
			}

			#region IDiagnosticQueryHandler
			public async Task<List<NodeLogItem>> GetLogsByGroup(string group, string application, DateTime fromDate, DateTime toDate)
			{
				var lst = new List<NodeLogItem>();
				foreach (var item in _parent._handlers)
				{
					var res = await item.Query.GetLogsByGroup(group, application, fromDate, toDate).ConfigureAwait(false);
					lst.AddRange(res);
				}
				lst = lst.DistinctBy(x => x.LogId).ToList();
				return lst;
			}
			public async Task<List<NodeLogItem>> GetLogsAsync(string search, string application, DateTime fromDate, DateTime toDate)
			{
				var lst = new List<NodeLogItem>();
				foreach (var item in _parent._handlers)
				{
					var res = await item.Query.GetLogsAsync(search, application, fromDate, toDate).ConfigureAwait(false);
					lst.AddRange(res);
				}
				lst = lst.DistinctBy(x => x.LogId).ToList();
				return lst;
			}
			public async Task<List<NodeLogItem>> GetLogsAsync(string search, string application, LogLevel level, DateTime fromDate, DateTime toDate)
			{
				var lst = new List<NodeLogItem>();
				foreach (var item in _parent._handlers)
				{
					var res = await item.Query.GetLogsAsync(search, application, level, fromDate, toDate).ConfigureAwait(false);
					lst.AddRange(res);
				}
				lst = lst.DistinctBy(x => x.LogId).ToList();
				return lst;
			}
			public async Task<List<NodeTraceItem>> GetTracesAsync(string search, string application, DateTime fromDate, DateTime toDate)
			{
				var lst = new List<NodeTraceItem>();
				foreach (var item in _parent._handlers)
				{
					var res = await item.Query.GetTracesAsync(search, application, fromDate, toDate).ConfigureAwait(false);
					lst.AddRange(res);
				}
				lst = lst.DistinctBy(x => x.TraceId).ToList();
				return lst;
			}
			public async Task<List<NodeTraceItem>> GetTracesByGroupAsync(string group, string application, DateTime fromDate, DateTime toDate)
			{
				var lst = new List<NodeTraceItem>();
				foreach (var item in _parent._handlers)
				{
					var res = await item.Query.GetTracesByGroupAsync(group, application, fromDate, toDate).ConfigureAwait(false);
					lst.AddRange(res);
				}
				lst = lst.DistinctBy(x => x.TraceId).ToList();
				return lst;
			}
			public async Task<SerializedObject> GetTraceObjectAsync(NodeTraceItem item)
			{
				foreach (var handler in _parent._handlers)
				{
					var res = await handler.Query.GetTraceObjectAsync(item).ConfigureAwait(false);
					if (res != null)
						return res;
				}
				return null;
			}
			public async Task<List<NodeStatusItem>> GetStatusesAsync(string environment, string machine, string application, DateTime fromDate, DateTime toDate)
			{
				var lst = new List<NodeStatusItem>();
				foreach (var item in _parent._handlers)
				{
					var res = await item.Query.GetStatusesAsync(environment, machine, application, fromDate, toDate).ConfigureAwait(false);
					lst.AddRange(res);
				}
				lst = lst.DistinctBy(x => x.Timestamp + x.Environment + x.Machine + x.Application + x.Date).ToList();
				return lst;
			}
			#endregion
		}
		
		#endregion
	}
}
