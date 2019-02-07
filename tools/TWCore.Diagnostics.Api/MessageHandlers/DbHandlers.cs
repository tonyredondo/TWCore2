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
using TWCore.Diagnostics.Counters;
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace TWCore.Diagnostics.Api
{
	public class DbHandlers : IDiagnosticHandler
	{
		public static DbHandlers Instance => Singleton<DbHandlers>.Instance;

		private readonly IDiagnosticMessagesHandler[] _messageHandlers;

		#region Public Properties
		public IDiagnosticMessagesHandler Messages { get; } 
		public IDiagnosticQueryHandler Query { get; }

        public event EventHandler<LogItem> ErrorLogMessage;
        public event EventHandler<ICounterItem> CounterReceived;
		#endregion

		private DbHandlers()
		{
			_messageHandlers = Core.Injector.GetAllInstances<IDiagnosticMessagesHandler>();
			Query = Core.Injector.New<IDiagnosticQueryHandler>();
			
			if (_messageHandlers is null)
				throw new Exception("Messages handlers are not defined.");
			if (Query is null)
				throw new Exception("Query handler are not defined.");
			
			Messages = new MessagesHandler(this);
		}

        private void FireAsyncErrorLogMessage(LogItem item)
        {
            if (ErrorLogMessage == null) return;
            Task.Run(() =>
            {
                ErrorLogMessage?.Invoke(this, item);
            });
        }
        private void FireAsyncCounterReceived(ICounterItem item)
        {
            if (CounterReceived == null) return;
            Task.Run(() =>
            {
                CounterReceived?.Invoke(this, item);
            });
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
            public void Init()
            {
                foreach (var item in _parent._messageHandlers)
                    item.Init();
            }
            public async Task ProcessLogItemsMessageAsync(List<LogItem> message)
			{
                if (message == null) return;
                if (message.Count == 0) return;
                foreach(var mItem in message)
                {
                    if (mItem.Level == LogLevel.Error)
                        _parent.FireAsyncErrorLogMessage(mItem);
                }
                foreach (var item in _parent._messageHandlers)
                    await item.ProcessLogItemsMessageAsync(message).ConfigureAwait(false);
			}
            public async Task ProcessGroupMetadataMessageAsync(List<GroupMetadata> message)
            {
                if (message == null) return;
                if (message.Count == 0) return;
                foreach (var item in _parent._messageHandlers)
					await item.ProcessGroupMetadataMessageAsync(message).ConfigureAwait(false);
            }
			public async Task ProcessTraceItemsMessageAsync(List<MessagingTraceItem> message)
			{
                if (message == null) return;
                if (message.Count == 0) return;
                foreach (var item in _parent._messageHandlers)
					await item.ProcessTraceItemsMessageAsync(message).ConfigureAwait(false);
			}
			public async Task ProcessStatusMessageAsync(StatusItemCollection message)
			{
                if (message == null) return;
                foreach (var item in _parent._messageHandlers)
					await item.ProcessStatusMessageAsync(message).ConfigureAwait(false);
			}
            public async Task ProcessCountersMessageAsync(List<ICounterItem> message)
            {
                if (message == null) return;
                if (message.Count == 0) return;
                foreach (var counter in message)
                {
                    _parent.FireAsyncCounterReceived(counter);
                }
                foreach (var item in _parent._messageHandlers)
                    await item.ProcessCountersMessageAsync(message).ConfigureAwait(false);
            }
            #endregion
        }
		#endregion
	}
}
