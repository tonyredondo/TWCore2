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
        public Task<List<string>> GetEnvironmentsAsync()
        {
            return RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var logsEnvQuery = session.Query<NodeLogItem>()
                    .Select(x => x.Environment)
                    .Distinct();
                var logsEnvTask = session.Query<NodeLogItem>()
                    .Select(x => x.Environment)
                    .Distinct()
                    .LazilyAsync();
                var tracesEnvTask = session.Query<NodeTraceItem>()
                    .Select(x => x.Environment)
                    .Distinct()
                    .LazilyAsync();
                var statusEnvTask = session.Query<NodeStatusItem>()
                    .Select(x => x.Environment)
                    .Distinct()
                    .LazilyAsync();

                await session.Advanced.Eagerly.ExecuteAllPendingLazyOperationsAsync().ConfigureAwait(false);
                var logsEnv = await logsEnvTask.Value.ConfigureAwait(false);
                var tracesEnv = await tracesEnvTask.Value.ConfigureAwait(false);
                var statusEnv = await statusEnvTask.Value.ConfigureAwait(false);

                return logsEnv.Concat(tracesEnv).Concat(statusEnv).Distinct().RemoveNulls().ToList();
            });
        }

        // Logs
        public async Task<LogSummary> GetLogsApplicationsLevelsByEnvironmentAsync(string environment, DateTime fromDate, DateTime toDate)
        {
            var value = await RavenHelper.ExecuteAndReturnAsync(session =>
            {
                return session.Query<NodeLogItem>()
                    .Where(x => x.Environment == environment)
                    .Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate)
                    .OrderBy(x => x.Application)
                    .GroupBy(x => new
                    {
                        x.Environment,
                        x.Application,
                        x.Level,
                        x.Timestamp
                    })
                    .Select(x => new
                    {
                        x.Key.Application,
                        x.Key.Level,
                        x.Key.Timestamp
                    })
                    .Distinct()
                    .ToListAsync();
            }).ConfigureAwait(false);

            var summary = new LogSummary
            {
                Applications = value.GroupBy(x => x.Application).Select(x => new ApplicationsLevels
                {
                    Application = x.Key,
                    Levels = x.GroupBy(i => i.Level).Select(ix => new LogLevelQuantity
                    {
                        Name = ix.Key,
                        Count = ix.Count()
                    }).OrderBy(i => i.Name).ToArray()
                }).ToArray(),
                Levels = value.GroupBy(x => x.Level).Select(x => new LogLevelTimes
                {
                    Name = x.Key,
                    Count = x.Count(),
                    Series = x.GroupBy(i => i.Timestamp.Date).Select(i => new TimeCount
                    {
                        Date = i.Key,
                        Count = i.Count()
                    }).ToArray()
                }).OrderBy(x => x.Name).ToArray()
            };
            return summary;
        }
        public Task<PagedList<NodeLogItem>> GetLogsByApplicationLevelsEnvironmentAsync(string environment, string application, LogLevel level, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            return RavenHelper.ExecuteAndReturnAsync(async session =>
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
            });
        }
        public Task<List<NodeLogItem>> GetLogsBySearch(string environment, string searchTerm, DateTime fromDate, DateTime toDate)
        {
            return RavenHelper.ExecuteAndReturnAsync(session =>
            {
                return session.Advanced.AsyncDocumentQuery<NodeLogItem>()
                    .WhereEquals(x => x.Environment, environment)
                    .WhereBetween(x => x.Timestamp, fromDate, toDate)
                    .OpenSubclause()
                    .Search(x => x.Message, "*" + searchTerm + "*")
                    .Search(x => x.Group, "*" + searchTerm + "*")
                    .CloseSubclause()
                    .OrderBy(x => x.Timestamp)
                    .Take(150)
                    .ToListAsync();
            });
        }

        // Traces
        public async Task<PagedList<TraceResult>> GetTracesByEnvironmentAsync(string environment, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            var value = await RavenHelper.ExecuteAndReturnAsync(session =>
            {
                var res = session.Advanced.AsyncRawQuery<TraceTempResult>(@"
                    from NodeTraceItems as trace
                    group by trace.Group, trace.Environment, trace.Timestamp, trace.Tags 
                    where (trace.Environment = $environment) and (trace.Timestamp between $fromDate and $toDate) 
                    order by Timestamp desc 
                    select trace.Group, trace.Timestamp, trace.Tags");
                res.AddParameter("environment", environment);
                res.AddParameter("fromDate", fromDate);
                res.AddParameter("toDate", toDate);

                return res.ToListAsync();
            }).ConfigureAwait(false);

            var valGroup = value.GroupBy(i => i.Group);
            var totalResult = valGroup.Count();

            var val = valGroup.Select(i => new TraceResult
            {
                Group = i.Key,
                Count = i.Count(),
                Start = i.Min(x => x.Timestamp),
                End = i.Max(x => x.Timestamp),
                HasErrors = i.Any(x => x.Tags.Contains("Status: Error"))
            }).Skip(page * pageSize).Take(pageSize).ToList();

            return new PagedList<TraceResult>
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalResults = totalResult,
                Data = val
            };
        }
        public async Task<List<NodeTraceItem>> GetTracesByGroupIdAsync(string environment, string groupName)
        {
            var value = await RavenHelper.ExecuteAndReturnAsync(session =>
            {
                var documentQuery = session.Advanced.AsyncDocumentQuery<NodeTraceItem>();
                var query = documentQuery
                    .WhereEquals(x => x.Environment, environment)
                    .WhereEquals(x => x.Group, groupName)
                    .OrderBy(x => x.Timestamp);
                return query.ToListAsync();
            });

            return value;
        }
        public Task<SerializedObject> GetTraceObjectAsync(string id)
        {
            return RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var attachment = await session.Advanced.Attachments.GetAsync(id, "Trace").ConfigureAwait(false);
                if (attachment == null) return null;
                var traceObject = attachment.Stream.DeserializeFromNBinary<object>();
                return (SerializedObject)traceObject;
            });
        }
        public Task<string> GetTraceXmlAsync(string id)
        {
            return RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var attachment = await session.Advanced.Attachments.GetAsync(id, "TraceXml").ConfigureAwait(false);
                if (attachment?.Stream == null) return null;
                return await attachment.Stream.TextReadToEndAsync().ConfigureAwait(false);
            });
        }
        public Task<string> GetTraceJsonAsync(string id)
        {
            return RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var attachment = await session.Advanced.Attachments.GetAsync(id, "TraceJson").ConfigureAwait(false);
                if (attachment?.Stream == null) return null;
                return await attachment.Stream.TextReadToEndAsync().ConfigureAwait(false);
            });
        }
        
        private class TraceTempResult
        {
            public string Group { get; set; }
            public DateTime Timestamp { get; set; }
            public string Tags { get; set; }
        }


        // Search
        public Task<SearchResults> SearchAsync(string environment, string searchTerm, DateTime fromDate, DateTime toDate)
        {
            return RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var logQuery = session.Advanced.AsyncDocumentQuery<NodeLogItem>()
                    .WhereEquals(x => x.Environment, environment)
                    .WhereBetween(x => x.Timestamp, fromDate, toDate)
                    .OpenSubclause()
                    .Search(x => x.Message, "*" + searchTerm + "*")
                    .Search(x => x.Group, "*" + searchTerm + "*")
                    .Search(x => x.Level, searchTerm)
                    .Search(x => x.Code, searchTerm)
                    .Search(x => x.Type, searchTerm)
                    .Search(x => x.Application, searchTerm)
                    .Search(x => x.Machine, searchTerm)
                    .CloseSubclause()
                    .OrderBy(x => x.Timestamp)
                    .Take(200)
                    .LazilyAsync();

                var traceQuery = session.Advanced.AsyncDocumentQuery<NodeTraceItem>()
                    .WhereEquals(x => x.Environment, environment)
                    .WhereBetween(x => x.Timestamp, fromDate, toDate)
                    .OpenSubclause()
                    .Search(x => x.Group, "*" + searchTerm + "*")
                    .Search(x => x.Name, "*" + searchTerm + "*")
                    .Search(x => x.Tags, "*" + searchTerm + "*")
                    .Search(x => x.Application, searchTerm)
                    .Search(x => x.Machine, searchTerm)
                    .CloseSubclause()
                    .OrderBy(x => x.Timestamp)
                    .Take(200)
                    .LazilyAsync();

                await session.Advanced.Eagerly.ExecuteAllPendingLazyOperationsAsync().ConfigureAwait(false);
                var logResults = await logQuery.Value.ConfigureAwait(false);
                var traceResults = await traceQuery.Value.ConfigureAwait(false);
                    
                return new SearchResults { Logs = logResults.ToList(), Traces = traceResults.ToList() };
            });
        }

        
        
        
        
        
        //Others
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