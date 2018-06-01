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
using System.Threading.Tasks;
using TWCore.Diagnostics.Api.Models;
using TWCore.Diagnostics.Api.Models.Log;
using TWCore.Diagnostics.Api.Models.Status;
using TWCore.Diagnostics.Api.Models.Trace;
using TWCore.Diagnostics.Log;
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api.MessageHandlers.RavenDb
{
	public class RavenDbQueryHandler : IDiagnosticQueryHandler
	{
		/// <summary>
		/// Get the los from a query
		/// </summary>
		/// <returns>Logs instance</returns>
		/// <param name="search">Search term</param>
		/// <param name="application">Application name or null</param>
		/// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		public async Task<NodeLogItem[]> GetLogs(string search, string application, DateTime fromDate, DateTime toDate)
		{
			return await RavenHelper.ExecuteAndReturnAsync<NodeLogItem[]>(session =>
			{
				return null;
			}).ConfigureAwait(false);
		}
		/// <summary>
		/// Get the los from a query
		/// </summary>
		/// <returns>Logs instance</returns>
		/// <param name="search">Search term</param>
		/// <param name="application">Application name or null</param>
		/// <param name="level">Log level</param>
		/// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		public async Task<NodeLogItem[]> GetLogs(string search, string application, LogLevel level, DateTime fromDate, DateTime toDate)
		{
			return await RavenHelper.ExecuteAndReturnAsync<NodeLogItem[]>(session =>
			{
				return null;
			}).ConfigureAwait(false);
		}
		/// <summary>
		/// Get the traces form a query
		/// </summary>
		/// <returns>Traces instance</returns>
		/// <param name="search">Search term</param>
		/// <param name="application">Application name or null</param>
		/// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		public async Task<NodeTraceItem[]> GetTraces(string search, string application, DateTime fromDate, DateTime toDate)
		{
			return await RavenHelper.ExecuteAndReturnAsync<NodeTraceItem[]>(session =>
			{
				return null;
			}).ConfigureAwait(false);
		}
		/// <summary>
		/// Gets the traces by group.
		/// </summary>
		/// <returns>The traces by group.</returns>
		/// <param name="group">Group name</param>
		/// <param name="application">Application name or null</param>
		/// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		public async Task<NodeTraceItem[]> GetTracesByGroup(string group, string application, DateTime fromDate, DateTime toDate)
		{
			return await RavenHelper.ExecuteAndReturnAsync<NodeTraceItem[]>(session =>
			{
				return null;
			}).ConfigureAwait(false);
		}
		/// <summary>
		/// Gets the statuses
		/// </summary>
		/// <returns>The statuses from the query</returns>
		/// <param name="environment">Environment name or null</param>
		/// <param name="machine">Machine name or null</param>
		/// <param name="application">Application name or null</param>
		/// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		public async Task<NodeStatusItem[]> GetStatuses(string environment, string machine, string application, DateTime fromDate, DateTime toDate)
		{
			return await RavenHelper.ExecuteAndReturnAsync<NodeStatusItem[]>(session =>
			{
				return null;
			}).ConfigureAwait(false);
		}
	}
}