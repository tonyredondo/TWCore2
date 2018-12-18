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
using System.Threading.Tasks;
using TWCore.Diagnostics.Counters;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace.Storages;
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api.Models
{
	/// <summary>
	/// Diagnostic messages handler interface
	/// </summary>
	public interface IDiagnosticMessagesHandler
    {
        /// <summary>
        /// Initialize handler
        /// </summary>
        void Init();
        /// <summary>
        /// Process LogItems message
        /// </summary>
        /// <param name="message">Message to handle</param>
        /// <returns>Process task</returns>
        Task ProcessLogItemsMessageAsync(List<LogItem> message);
        /// <summary>
        /// Process GroupMetadata message
        /// </summary>
        /// <param name="message">Message to handle</param>
        /// <returns>Process task</returns>
        Task ProcessGroupMetadataMessageAsync(List<GroupMetadata> message);
        /// <summary>
        /// Process TraceItems message
        /// </summary>
        /// <param name="message">Message to handle</param>
        /// <returns>Process task</returns>
		Task ProcessTraceItemsMessageAsync(List<MessagingTraceItem> message);
        /// <summary>
        /// Process Status message
        /// </summary>
        /// <param name="message">Message to handle</param>
        /// <returns>Process task</returns>
		Task ProcessStatusMessageAsync(StatusItemCollection message);
        /// <summary>
        /// Process Counters message
        /// </summary>
        /// <param name="message">Message to handle</param>
        /// <returns>Process task</returns>
        Task ProcessCountersMessageAsync(List<ICounterItem> message);
    }
}