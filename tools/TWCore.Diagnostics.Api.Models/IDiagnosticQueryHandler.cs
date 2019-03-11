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
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Diagnostics.Api.Models.Counters;
using TWCore.Diagnostics.Api.Models.Log;
using TWCore.Diagnostics.Api.Models.Status;
using TWCore.Diagnostics.Api.Models.Trace;
using TWCore.Diagnostics.Log;
using TWCore.Serialization;
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api.Models
{
	/// <summary>
	/// Diagnostic query handler interface
	/// </summary>
	public interface IDiagnosticQueryHandler
	{
        /// <summary>
        /// Initialize handler
        /// </summary>
        void Init();
        /// <summary>
        /// Gets the environments
        /// </summary>
        /// <returns>List of BasicInfo</returns>
        Task<List<string>> GetEnvironmentsAsync();
        /// <summary>
        /// Gets the Applications with logs by environment
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="fromDate">From date and time</param>
        /// <param name="toDate">To date and time</param>
        /// <returns>List of applications</returns>
        Task<LogSummary> GetLogsApplicationsLevelsByEnvironmentAsync(string environment, DateTime fromDate, DateTime toDate);
        /// <summary>
        /// Gets the Logs by Application Levels and Environment
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="application">Application name</param>
        /// <param name="level">Log level</param>
        /// <param name="fromDate">From date and time</param>
        /// <param name="toDate">To date and time</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Logs</returns>
        Task<PagedList<NodeLogItem>> GetLogsByApplicationLevelsEnvironmentAsync(string environment, string application, LogLevel? level, DateTime fromDate, DateTime toDate, int page, int pageSize = 50);
        /// <summary>
        /// Gets the traces objects by environment and dates
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="fromDate">From date and time</param>
        /// <param name="toDate">To date and time</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Traces</returns>
        Task<PagedList<TraceResult>> GetTracesByEnvironmentAsync(string environment, DateTime fromDate, DateTime toDate, int page, int pageSize = 50);
        /// <summary>
        /// Get the traces from a Trace Group
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="groupName">Group name</param>
        /// <returns>Traces from that group</returns>
        Task<List<NodeTraceItem>> GetTracesByGroupIdAsync(string environment, string groupName);
		/// <summary>
		/// Gets the Trace object
		/// </summary>
		/// <returns>The trace object</returns>
		/// <param name="id">Trace object id</param>
		Task<SerializedObject> GetTraceObjectAsync(string id);
		/// <summary>
		/// Gets the Trace object in xml
		/// </summary>
		/// <returns>The trace object</returns>
		/// <param name="id">Trace object id</param>
		Task<string> GetTraceXmlAsync(string id);
		/// <summary>
		/// Gets the Trace object in json
		/// </summary>
		/// <returns>The trace object</returns>
		/// <param name="id">Trace object id</param>
		Task<string> GetTraceJsonAsync(string id);
        /// <summary>
		/// Gets the Trace object in txt
		/// </summary>
		/// <returns>The trace object</returns>
		/// <param name="id">Trace object id</param>
		Task<string> GetTraceTxtAsync(string id);

        /// <summary>
        /// Search a term in the database
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="searchTerm">Term to search in the database</param>
        /// <param name="fromDate">From date and time</param>
        /// <param name="toDate">To date and time</param>
        /// <returns>Search results</returns>
        Task<SearchResults> SearchAsync(string environment, string searchTerm, DateTime fromDate, DateTime toDate);
        /// <summary>
        /// Get metadata from a group name
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <returns>List of metadatas</returns>
		Task<KeyValue[]> GetMetadatas(string groupName);
		
		/// <summary>
		/// Gets the statuses
		/// </summary>
		/// <returns>The statuses from the query</returns>
		/// <param name="environment">Environment name</param>
		/// <param name="machine">Machine name or null</param>
		/// <param name="application">Application name or null</param>
		/// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		/// <param name="page">Page number</param>
		/// <param name="pageSize">Page size</param>
		Task<PagedList<NodeStatusItem>> GetStatusesAsync(string environment, string machine, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50);
        /// <summary>
        /// Get Current Statuses
        /// </summary>
		/// <param name="environment">Environment name</param>
		/// <param name="machine">Machine name or null</param>
		/// <param name="application">Application name or null</param>
        /// <returns>Get Current Status list</returns>
        Task<List<NodeStatusItem>> GetCurrentStatus(string environment, string machine, string application);
        /// <summary>
        /// Get Counters
        /// </summary>
		/// <param name="environment">Environment name</param>
        /// <returns>List of counters</returns>
        Task<List<NodeCountersQueryItem>> GetCounters(string environment);
        /// <summary>
        /// Get Counter by counterId
        /// </summary>
		/// <param name="counterId">Counter Id</param>
        /// <returns>Counter data</returns>
        Task<NodeCountersQueryItem> GetCounter(Guid counterId);
        /// <summary>
        /// Get Counter Values
        /// </summary>
        /// <param name="counterId">Counter id</param>
        /// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
        /// <param name="limit">Value limit</param>
        /// <returns>List of counter values</returns>
        Task<List<NodeCountersQueryValue>> GetCounterValues(Guid counterId, DateTime fromDate, DateTime toDate, int limit = 3600);
        /// <summary>
        /// Get Last Counter Values
        /// </summary>
        /// <param name="counterId">Counter id</param>
        /// <param name="valuesDivision">Counter values division</param>
        /// <param name="samples">Samples quantity</param>
        /// <returns>Values list</returns>
        Task<List<NodeLastCountersValue>> GetLastCounterValues(Guid counterId, CounterValuesDivision valuesDivision, int samples = 250, DateTime? lastDate = default);
    }
}