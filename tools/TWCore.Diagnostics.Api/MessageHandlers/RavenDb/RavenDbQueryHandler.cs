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
using TWCore.Serialization;
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api.MessageHandlers.RavenDb
{
	public class RavenDbQueryHandler : IDiagnosticQueryHandler
	{
		/// <summary>
		/// Get the logs by group
		/// </summary>
		/// <returns>Logs instance</returns>
		/// <param name="group">Group</param>
		/// <param name="application">Application name or null</param>
		/// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		public async Task<List<NodeLogItem>> GetLogsByGroup(string group, string application, DateTime fromDate, DateTime toDate)
		{
			return await RavenHelper.ExecuteAndReturnAsync<List<NodeLogItem>>(async session =>
			{
				var documentQuery = session.Advanced.AsyncDocumentQuery<NodeLogItem>();
				var query = documentQuery.WhereBetween(x => x.Timestamp, fromDate, toDate);

				if (!string.IsNullOrWhiteSpace(application))
					query = query.WhereEquals(x => x.Application, application);

				if (!string.IsNullOrWhiteSpace(group))
					query = query.Search(x => x.Group, "*" + group + "*");

				return await query.ToListAsync().ConfigureAwait(false);
			}).ConfigureAwait(false);
		}
		/// <summary>
		/// Get the logs from a query
		/// </summary>
		/// <returns>Logs instance</returns>
		/// <param name="search">Search term</param>
		/// <param name="application">Application name or null</param>
		/// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		public async Task<List<NodeLogItem>> GetLogsAsync(string search, string application, DateTime fromDate, DateTime toDate)
		{
			return await RavenHelper.ExecuteAndReturnAsync<List<NodeLogItem>>(async session =>
			{
				var documentQuery = session.Advanced.AsyncDocumentQuery<NodeLogItem>();
				var query = documentQuery.WhereBetween(x => x.Timestamp, fromDate, toDate);

				if (!string.IsNullOrWhiteSpace(application))
					query = query.WhereEquals(x => x.Application, application);

				if (!string.IsNullOrWhiteSpace(search))
					query = query
						.Search(x => x.Message, "*" + search + "*")
						.Search(x => x.Group, "*" + search + "*")
						.Search(x => x.Code, "*" + search + "*");

				return await query.ToListAsync().ConfigureAwait(false);
			}).ConfigureAwait(false);
		}
		/// <summary>
		/// Get the logs from a query
		/// </summary>
		/// <returns>Logs instance</returns>
		/// <param name="search">Search term</param>
		/// <param name="application">Application name or null</param>
		/// <param name="level">Log level</param>
		/// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		public async Task<List<NodeLogItem>> GetLogsAsync(string search, string application, LogLevel level, DateTime fromDate, DateTime toDate)
		{
			return await RavenHelper.ExecuteAndReturnAsync<List<NodeLogItem>>(async session =>
			{
				var documentQuery = session.Advanced.AsyncDocumentQuery<NodeLogItem>();
				var query = documentQuery.WhereBetween(x => x.Timestamp, fromDate, toDate);

				query = query.WhereEquals(x => x.Level, level);

				if (!string.IsNullOrWhiteSpace(application))
					query = query.WhereEquals(x => x.Application, application);

				if (!string.IsNullOrWhiteSpace(search))
					query = query
						.Search(x => x.Message, "*" + search + "*")
						.Search(x => x.Group, "*" + search + "*")
						.Search(x => x.Code, "*" + search + "*");

				return await query.ToListAsync().ConfigureAwait(false);

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
		public async Task<List<NodeTraceItem>> GetTracesAsync(string search, string application, DateTime fromDate, DateTime toDate)
		{
			return await RavenHelper.ExecuteAndReturnAsync<List<NodeTraceItem>>(async session =>
			{
				var documentQuery = session.Advanced.AsyncDocumentQuery<NodeTraceItem>();
				var query = documentQuery.WhereBetween(x => x.Timestamp, fromDate, toDate);

				if (!string.IsNullOrWhiteSpace(application))
					query = query.WhereEquals(x => x.Application, application);

				if (!string.IsNullOrWhiteSpace(search))
					query = query
						.Search(x => x.Group, "*" + search + "*")
						.Search(x => x.Name, "*" + search + "*");

				return await query.ToListAsync().ConfigureAwait(false);

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
		public async Task<List<NodeTraceItem>> GetTracesByGroupAsync(string group, string application, DateTime fromDate, DateTime toDate)
		{
			return await RavenHelper.ExecuteAndReturnAsync<List<NodeTraceItem>>(async session =>
			{
				var documentQuery = session.Advanced.AsyncDocumentQuery<NodeTraceItem>();
				var query = documentQuery.WhereBetween(x => x.Timestamp, fromDate, toDate);

				if (!string.IsNullOrWhiteSpace(application))
					query = query.WhereEquals(x => x.Application, application);

				if (!string.IsNullOrWhiteSpace(group))
					query = query
						.Search(x => x.Group, "*" + group + "*");

				return await query.ToListAsync().ConfigureAwait(false);

			}).ConfigureAwait(false);
		}
		/// <summary>
		/// Gets the Trace object
		/// </summary>
		/// <returns>The trace object</returns>
		/// <param name="item">Trace item to retrieve the trace object</param>
		public async Task<SerializedObject> GetTraceObjectAsync(NodeTraceItem item)
		{
			return await RavenHelper.ExecuteAndReturnAsync<SerializedObject>(async session =>
			{
				var attachment = await session.Advanced.Attachments.GetAsync(item.Id, "Trace").ConfigureAwait(false);
				var traceObject = attachment.Stream.DeserializeFromNBinary<object>();
				return (SerializedObject)traceObject;

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
		public async Task<List<NodeStatusItem>> GetStatusesAsync(string environment, string machine, string application, DateTime fromDate, DateTime toDate)
		{
			return await RavenHelper.ExecuteAndReturnAsync<List<NodeStatusItem>>(async session =>
			{
				var documentQuery = session.Advanced.AsyncDocumentQuery<NodeStatusItem>();
				var query = documentQuery.WhereBetween(x => x.Timestamp, fromDate, toDate);

				if (!string.IsNullOrWhiteSpace(environment))
					query = query.WhereEquals(x => x.Environment, environment);

				if (!string.IsNullOrWhiteSpace(machine))
					query = query.WhereEquals(x => x.Machine, machine);

				if (!string.IsNullOrWhiteSpace(application))
					query = query.WhereEquals(x => x.Application, application);

				return await query.ToListAsync().ConfigureAwait(false);

			}).ConfigureAwait(false);
		}
	}
}