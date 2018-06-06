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
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
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
		/// <inheritdoc />
		/// <summary>
		/// Gets the environments and apps list
		/// </summary>
		/// <returns>List of BasicInfo</returns>
		public async Task<List<BasicInfo>> GetEnvironmentsAndApps()
		{
			return await RavenHelper.ExecuteAndReturnAsync(async session =>
			{
				var results = session.Query<NodeLogItem>()
					.GroupBy(x => new
					{
						x.Environment,
						x.Machine,
						x.Application
					})
					.Select(x => new BasicInfo
					{
						Environment = x.Key.Environment,
						Machine = x.Key.Machine,
						Application = x.Key.Application
					});
				return await results.ToListAsync().ConfigureAwait(false);
			}).ConfigureAwait(false);
		}
		/// <inheritdoc />
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
		public async Task<PagedList<NodeLogItem>> GetLogsByGroup(string environment, string group, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
		{
			return await RavenHelper.ExecuteAndReturnAsync(async session =>
			{
				var documentQuery = session.Advanced.AsyncDocumentQuery<NodeLogItem>();
				var query = documentQuery
						.Statistics(out var stats)
						.WhereEquals(x => x.Environment, environment)
						.WhereBetween(x => x.Timestamp, fromDate, toDate);

				if (!string.IsNullOrWhiteSpace(application))
					query = query.WhereEquals(x => x.Application, application);

				if (!string.IsNullOrWhiteSpace(group))
					query = query.Search(x => x.Group, "*" + group + "*");

				query = query.Skip(page * pageSize).Take(pageSize);
				
				var data = await query.ToListAsync().ConfigureAwait(false);
				return new PagedList<NodeLogItem>
				{
					PageNumber = page,
					PageSize = pageSize,
					TotalResults = stats.TotalResults,
					Data = data
				};
			}).ConfigureAwait(false);
		}
		/// <inheritdoc />
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
		public async Task<PagedList<NodeLogItem>> GetLogsAsync(string environment, string search, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
		{
			return await RavenHelper.ExecuteAndReturnAsync(async session =>
			{
				var documentQuery = session.Advanced.AsyncDocumentQuery<NodeLogItem>();
				var query = documentQuery
					.Statistics(out var stats)
					.WhereEquals(x => x.Environment, environment)
					.WhereBetween(x => x.Timestamp, fromDate, toDate);

				if (!string.IsNullOrWhiteSpace(application))
					query = query.WhereEquals(x => x.Application, application);

				if (!string.IsNullOrWhiteSpace(search))
					query = query
						.Search(x => x.Message, "*" + search + "*")
						.Search(x => x.Group, "*" + search + "*")
						.Search(x => x.Code, "*" + search + "*");
				
				query = query.Skip(page * pageSize).Take(pageSize);

				var data = await query.ToListAsync().ConfigureAwait(false);
				return new PagedList<NodeLogItem>
				{
					PageNumber = page,
					PageSize = pageSize,
					TotalResults = stats.TotalResults,
					Data = data
				};
			}).ConfigureAwait(false);
		}
		/// <inheritdoc />
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
		public async Task<PagedList<NodeLogItem>> GetLogsAsync(string environment, string search, string application, LogLevel level, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
		{
			return await RavenHelper.ExecuteAndReturnAsync(async session =>
			{
				var documentQuery = session.Advanced.AsyncDocumentQuery<NodeLogItem>();
				var query = documentQuery
					.Statistics(out var stats)
					.WhereEquals(x => x.Environment, environment)
					.WhereBetween(x => x.Timestamp, fromDate, toDate);

				query = query.WhereEquals(x => x.Level, level);

				if (!string.IsNullOrWhiteSpace(application))
					query = query.WhereEquals(x => x.Application, application);

				if (!string.IsNullOrWhiteSpace(search))
					query = query
						.Search(x => x.Message, "*" + search + "*")
						.Search(x => x.Group, "*" + search + "*")
						.Search(x => x.Code, "*" + search + "*");

				query = query.Skip(page * pageSize).Take(pageSize);

				var data = await query.ToListAsync().ConfigureAwait(false);
				return new PagedList<NodeLogItem>
				{
					PageNumber = page,
					PageSize = pageSize,
					TotalResults = stats.TotalResults,
					Data = data
				};

			}).ConfigureAwait(false);
		}
		/// <inheritdoc />
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
		public async Task<PagedList<NodeTraceItem>> GetTracesAsync(string environment, string search, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
		{
			return await RavenHelper.ExecuteAndReturnAsync(async session =>
			{
				var documentQuery = session.Advanced.AsyncDocumentQuery<NodeTraceItem>();
				var query = documentQuery
					.Statistics(out var stats)
					.WhereEquals(x => x.Environment, environment)
					.WhereBetween(x => x.Timestamp, fromDate, toDate);

				if (!string.IsNullOrWhiteSpace(application))
					query = query.WhereEquals(x => x.Application, application);

				if (!string.IsNullOrWhiteSpace(search))
					query = query
						.Search(x => x.Group, "*" + search + "*")
						.Search(x => x.Name, "*" + search + "*");

				query = query.Skip(page * pageSize).Take(pageSize);

				var data = await query.ToListAsync().ConfigureAwait(false);
				return new PagedList<NodeTraceItem>
				{
					PageNumber = page,
					PageSize = pageSize,
					TotalResults = stats.TotalResults,
					Data = data
				};

			}).ConfigureAwait(false);
		}
		/// <inheritdoc />
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
		public async Task<PagedList<NodeTraceItem>> GetTracesByGroupAsync(string environment, string group, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
		{
			return await RavenHelper.ExecuteAndReturnAsync(async session =>
			{
				var documentQuery = session.Advanced.AsyncDocumentQuery<NodeTraceItem>();
				var query = documentQuery
					.Statistics(out var stats)
					.WhereEquals(x => x.Environment, environment)
					.WhereBetween(x => x.Timestamp, fromDate, toDate);

				if (!string.IsNullOrWhiteSpace(application))
					query = query.WhereEquals(x => x.Application, application);

				if (!string.IsNullOrWhiteSpace(group))
					query = query
						.Search(x => x.Group, "*" + group + "*");

				query = query.Skip(page * pageSize).Take(pageSize);

				var data = await query.ToListAsync().ConfigureAwait(false);
				return new PagedList<NodeTraceItem>
				{
					PageNumber = page,
					PageSize = pageSize,
					TotalResults = stats.TotalResults,
					Data = data
				};

			}).ConfigureAwait(false);
		}
		/// <inheritdoc />
		/// <summary>
		/// Gets the Trace object
		/// </summary>
		/// <returns>The trace object</returns>
		/// <param name="id">Trace object id</param>
		public async Task<SerializedObject> GetTraceObjectAsync(string id)
		{
			return await RavenHelper.ExecuteAndReturnAsync(async session =>
			{
				var attachment = await session.Advanced.Attachments.GetAsync(id, "Trace").ConfigureAwait(false);
				var traceObject = attachment.Stream.DeserializeFromNBinary<object>();
				return (SerializedObject)traceObject;

			}).ConfigureAwait(false);
		}
		/// <inheritdoc />
		/// <summary>
		/// Gets the statuses
		/// </summary>
		/// <returns>The statuses from the query</returns>
		/// <param name="environment">Environment name or null</param>
		/// <param name="machine">Machine name or null</param>
		/// <param name="application">Application name or null</param>
		/// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
		/// <param name="page">Page number</param>
		/// <param name="pageSize">Page size</param>
		public async Task<PagedList<NodeStatusItem>> GetStatusesAsync(string environment, string machine, string application, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
		{
			return await RavenHelper.ExecuteAndReturnAsync(async session =>
			{
				var documentQuery = session.Advanced.AsyncDocumentQuery<NodeStatusItem>();
				var query = documentQuery
					.Statistics(out var stats)
					.WhereEquals(x => x.Environment, environment)
					.WhereBetween(x => x.Timestamp, fromDate, toDate);

				if (!string.IsNullOrWhiteSpace(machine))
					query = query.WhereEquals(x => x.Machine, machine);

				if (!string.IsNullOrWhiteSpace(application))
					query = query.WhereEquals(x => x.Application, application);

				query = query.Skip(page * pageSize).Take(pageSize);

				var data = await query.ToListAsync().ConfigureAwait(false);
				return new PagedList<NodeStatusItem>
				{
					PageNumber = page,
					PageSize = pageSize,
					TotalResults = stats.TotalResults,
					Data = data
				};

			}).ConfigureAwait(false);
		}
	}
}