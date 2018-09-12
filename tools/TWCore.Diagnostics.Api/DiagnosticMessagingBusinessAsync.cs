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
using TWCore.Messaging;
using TWCore.Services.Messaging;

namespace TWCore.Diagnostics.Api
{
	public class DiagnosticMessagingBusinessAsync : BusinessAsyncBase<List<LogItem>, List<MessagingTraceItem>, StatusItemCollection>
	{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override async Task<object> OnProcessAsync(List<LogItem> message)
        {
			if (message is null || message.Count == 0) return ResponseMessage.NoResponse;

			await DbHandlers.Instance.Messages.ProcessLogItemsMessageAsync(message).ConfigureAwait(false);
	        
			return ResponseMessage.NoResponse;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override async Task<object> OnProcessAsync(List<MessagingTraceItem> message)
        {
	        if (message is null || message.Count == 0) return ResponseMessage.NoResponse;

			await DbHandlers.Instance.Messages.ProcessTraceItemsMessageAsync(message).ConfigureAwait(false);

	        return ResponseMessage.NoResponse;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override async Task<object> OnProcessAsync(StatusItemCollection message)
        {
			if (message is null) return ResponseMessage.NoResponse;

			await DbHandlers.Instance.Messages.ProcessStatusMessageAsync(message).ConfigureAwait(false);
            
            return ResponseMessage.NoResponse;
        }
    }
}