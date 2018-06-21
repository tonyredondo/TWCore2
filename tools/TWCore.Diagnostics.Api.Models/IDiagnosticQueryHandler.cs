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
using TWCore.Diagnostics.Api.Models;
using TWCore.Diagnostics.Api.Models.Log;
using TWCore.Diagnostics.Api.Models.Status;
using TWCore.Diagnostics.Api.Models.Trace;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace.Storages;
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
        /// Gets the environments
        /// </summary>
        /// <returns>List of BasicInfo</returns>
        Task<List<string>> GetEnvironments();
        /// <summary>
        /// Gets the Applications with logs by environment
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="fromDate">From date and time</param>
        /// <param name="toDate">To date and time</param>
        /// <returns>List of applications</returns>
        Task<List<ApplicationsLevels>> GetLogsApplicationsLevelsByEnvironment(string environment, DateTime fromDate, DateTime toDate);
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
        Task<PagedList<NodeLogItem>> GetLogsByApplicationLevelsEnvironment(string environment, string application, LogLevel level, DateTime fromDate, DateTime toDate, int page, int pageSize = 50);

        /// <summary>
        /// Get the logs by group
        /// </summary>
        /// <returns>Logs instance</returns>
        /// <param name="environment">Environment name</param>
        /// <param name="group">Group</param>
        /// <param name="application">Application name or null</param>
        /// <param name="fromDate">From date and time</param>
        /// <param name="toDate">To date and time</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        Task<PagedList<NodeLogItem>> GetLogsByGroup(string environment, string group, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50);
		/// <summary>
		/// Get the logs from a query
		/// </summary>
		/// <returns>Logs instance</returns>
		/// <param name="environment">Environment name</param>
		/// <param name="search">Search term</param>
		/// <param name="application">Application name or null</param>
		/// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		/// <param name="page">Page number</param>
		/// <param name="pageSize">Page size</param>
		Task<PagedList<NodeLogItem>> GetLogsAsync(string environment, string search, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50);
		/// <summary>
		/// Get the logs from a query
		/// </summary>
		/// <returns>Logs instance</returns>
		/// <param name="environment">Environment name</param>
		/// <param name="search">Search term</param>
		/// <param name="application">Application name or null</param>
		/// <param name="level">Log level</param>
		/// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		/// <param name="page">Page number</param>
		/// <param name="pageSize">Page size</param>
		Task<PagedList<NodeLogItem>> GetLogsAsync(string environment, string search, string application, LogLevel level, DateTime fromDate, DateTime toDate, int page, int pageSize = 50);
		/// <summary>
		/// Get the traces form a query
		/// </summary>
		/// <returns>Traces instance</returns>
		/// <param name="environment">Environment name</param>
		/// <param name="search">Search term</param>
		/// <param name="application">Application name or null</param>
		/// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		/// <param name="page">Page number</param>
		/// <param name="pageSize">Page size</param>
		Task<PagedList<NodeTraceItem>> GetTracesAsync(string environment, string search, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50);
		/// <summary>
		/// Gets the traces by group.
		/// </summary>
		/// <returns>The traces by group.</returns>
		/// <param name="environment">Environment name</param>
		/// <param name="group">Group name</param>
		/// <param name="application">Application name or null</param>
		/// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		/// <param name="page">Page number</param>
		/// <param name="pageSize">Page size</param>
		Task<PagedList<NodeTraceItem>> GetTracesByGroupAsync(string environment, string group, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50);
		/// <summary>
		/// Gets the Trace object
		/// </summary>
		/// <returns>The trace object</returns>
		/// <param name="id">Trace object id</param>
		Task<SerializedObject> GetTraceObjectAsync(string id);
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
    }
}