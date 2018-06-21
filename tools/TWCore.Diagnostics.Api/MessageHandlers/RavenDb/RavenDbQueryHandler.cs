﻿/*
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
using RabbitMQ.Client.Impl;
using Raven.Client.Documents;
using TWCore.Diagnostics.Api.Models;
using TWCore.Diagnostics.Api.Models.Log;
using TWCore.Diagnostics.Api.Models.Status;
using TWCore.Diagnostics.Api.Models.Trace;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
using TWCore.Serialization;
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api.MessageHandlers.RavenDb
{
    public class RavenDbQueryHandler : IDiagnosticQueryHandler
    {
        public async Task<List<string>> GetEnvironments()
        {
            var logsEnvironments = await RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var results = session.Query<NodeLogItem>()
                    .Select(x => x.Environment)
                    .Distinct();
                return await results.ToListAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
            var traceEnvironments = await RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var results = session.Query<NodeTraceItem>()
                    .Select(x => x.Environment)
                    .Distinct();
                return await results.ToListAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
            var statusEnvironment = await RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var results = session.Query<NodeStatusItem>()
                    .Select(x => x.Environment)
                    .Distinct();
                return await results.ToListAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

            return logsEnvironments.Concat(traceEnvironments).Concat(statusEnvironment).Distinct().RemoveNulls().ToList();
        }

        public async Task<List<ApplicationsLevels>> GetLogsApplicationsLevelsByEnvironment(string environment, DateTime fromDate, DateTime toDate)
        {
            return await RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var results = session.Query<NodeLogItem>()
                    .Where(x => x.Environment == environment)
                    .Where(x => x.Timestamp.Date >= fromDate && x.Timestamp.Date <= toDate)
                    .OrderBy(x => x.Application)
                    .GroupBy(x => new
                    {
                        x.Environment,
                        x.Application,
                        x.Level,
                        x.Timestamp.Date
                    })
                    .Select(x => new
                    {
                        x.Key.Application,
                        x.Key.Level,
                        x.Key.Date
                    })
                    .Distinct();
                var value = await results.ToListAsync().ConfigureAwait(false);

                return value.GroupBy(x => x.Application).Select(x => new ApplicationsLevels
                {
                    Application = x.Key,
                    Levels = x.Select(i => i.Level).OrderBy(i => i).ToArray()
                }).ToList();
            }).ConfigureAwait(false);
        }

        public async Task<PagedList<NodeLogItem>> GetLogsByApplicationLevelsEnvironment(string environment, string application, LogLevel level, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            return await RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var documentQuery = session.Advanced.AsyncDocumentQuery<NodeLogItem>();
                var query = documentQuery
                        .Statistics(out var stats)
                        .WhereEquals(x => x.Environment, environment)
                        .WhereEquals(x => x.Application, application)
                        .WhereBetween(x => x.Timestamp, fromDate, toDate);

                if (
                    level == LogLevel.Error || level == LogLevel.Warning || level == LogLevel.Debug ||
                    level == LogLevel.InfoBasic || level == LogLevel.InfoDetail || level == LogLevel.InfoMedium ||
                    level == LogLevel.LibDebug || level == LogLevel.LibVerbose ||
                    level == LogLevel.Stats || level == LogLevel.Verbose
                    )
                    query = query.WhereEquals(x => x.Level, level);

                query = query.OrderByDescending(x => x.Timestamp);
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

                query = query.OrderByDescending(x => x.Timestamp);
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

                query = query.OrderByDescending(x => x.Timestamp);
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

                query = query.OrderByDescending(x => x.Timestamp);
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
                {
                    query = query
                        .Search(x => x.Group, "*" + search + "*")
                        .Search(x => x.Name, "*" + search + "*")
                        .Search(x => x.Tags, "*" + search + "*");
                }

                query = query.OrderByDescending(x => x.Timestamp);
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

                query = query.OrderByDescending(x => x.Timestamp);
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

        public async Task<SerializedObject> GetTraceObjectAsync(string id)
        {
            return await RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var attachment = await session.Advanced.Attachments.GetAsync(id, "Trace").ConfigureAwait(false);
                var traceObject = attachment.Stream.DeserializeFromNBinary<object>();
                return (SerializedObject)traceObject;

            }).ConfigureAwait(false);
        }


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

                query = query.OrderByDescending(x => x.Timestamp);
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

        public async Task<List<NodeStatusItem>> GetCurrentStatus(string environment, string machine, string application)
        {
            return await RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var documentQuery = session.Advanced.AsyncDocumentQuery<NodeStatusItem>();
                var query = documentQuery
                    .WhereEquals(x => x.Environment, environment);

                if (!string.IsNullOrWhiteSpace(machine))
                    query = query.WhereEquals(x => x.Machine, machine);

                if (!string.IsNullOrWhiteSpace(application))
                    query = query.WhereEquals(x => x.Application, application);

                query = query
                    //.WhereGreaterThanOrEqual(i => i.Timestamp, DateTime.Now.AddMinutes(-30))
                    .OrderByDescending(i => i.Timestamp);

                var data = await query.ToListAsync().ConfigureAwait(false);
                var rData = data
                    .GroupBy(i => new { i.Environment, i.Machine, i.Application })
                    .Select(i => i.First())
                    .ToList();

                return rData;
            }).ConfigureAwait(false);
        }
    }
}