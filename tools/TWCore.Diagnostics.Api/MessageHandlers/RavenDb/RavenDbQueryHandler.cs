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
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Documents;
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Diagnostics.Api.MessageHandlers.RavenDb.Indexes;
using TWCore.Diagnostics.Api.Models;
using TWCore.Diagnostics.Api.Models.Counters;
using TWCore.Diagnostics.Api.Models.Log;
using TWCore.Diagnostics.Api.Models.Status;
using TWCore.Diagnostics.Api.Models.Trace;
using TWCore.Diagnostics.Log;
using TWCore.Serialization;
using TWCore.Serialization.NSerializer;
// ReSharper disable UnusedMember.Global

namespace TWCore.Diagnostics.Api.MessageHandlers.RavenDb
{
    public class RavenDbQueryHandler : IDiagnosticQueryHandler
    {
        private static readonly DiagnosticsSettings Settings = Core.GetSettings<DiagnosticsSettings>();
        private static readonly ICompressor Compressor = new GZipCompressor();
        private static readonly NBinarySerializer NBinarySerializer = new NBinarySerializer
        {
            Compressor = Compressor
        };

        /// <summary>
        /// Initialize handler
        /// </summary>
        public void Init()
        {
            RavenHelper.Init();
        }
        /// <summary>
        /// Gets the environments
        /// </summary>
        /// <returns>List of BasicInfo</returns>
        public Task<List<string>> GetEnvironmentsAsync()
        {
            return RavenHelper.ExecuteAndReturnAsync(session =>
            {
                return session.Query<Environments_Availables.Result, Environments_Availables>().Select(x => x.Environment).ToListAsync();
            });
        }
        /// <summary>
        /// Gets the Applications with logs by environment
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="fromDate">From date and time</param>
        /// <param name="toDate">To date and time</param>
        /// <returns>List of applications</returns>
        public async Task<LogSummary> GetLogsApplicationsLevelsByEnvironmentAsync(string environment, DateTime fromDate, DateTime toDate)
        {
            var values = await RavenHelper.ExecuteAndReturnAsync(session =>
            {
                return session.Query<Logs_Summary.Result, Logs_Summary>()
                              .Where(x => x.Environment == environment)
                              .Where(x => x.Date >= fromDate && x.Date <= toDate)
                              .OrderBy(x => x.Application)
                              .ToListAsync();
            }).ConfigureAwait(false);

            var apps = values.GroupBy(x => x.Application).Select(x => new ApplicationsLevels
            {
                Application = x.Key,
                Levels = x.SelectMany(y => y.Levels).GroupBy(y => y.Name).Select(ix => new LogLevelQuantity
                {
                    Name = ix.Key,
                    Count = ix.Sum(i => i.Count)
                }).OrderBy(y => y.Name).ToArray()
            }).ToArray();

            var levels = values.SelectMany(col => col.Levels, (result, level) => new
            {
                Name = level.Name,
                Count = level.Count,
                Date = result.Date
            }).GroupBy(x => x.Name).Select(x => new LogLevelTimes
            {
                Name = x.Key,
                Count = x.Sum(i => i.Count),
                Series = x.GroupBy(i => i.Date).Select(i => new TimeCount
                {
                    Date = i.Key,
                    Count = i.Sum(k => k.Count)
                }).ToArray()
            }).OrderBy(x => x.Name).ToArray();

            return new LogSummary
            {
                Applications = apps,
                Levels = levels
            };
        }
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
        public Task<PagedList<NodeLogItem>> GetLogsByApplicationLevelsEnvironmentAsync(string environment, string application, LogLevel? level, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            return RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var documentQuery = session.Advanced.AsyncDocumentQuery<NodeLogItem, Logs_ByApplicationLevelsAndEnvironments>();
                var query = documentQuery
                        .Statistics(out var stats)
                        .WhereEquals(x => x.Environment, environment)
                        .WhereEquals(x => x.Application, application)
                        .WhereBetween(x => x.Timestamp, fromDate, toDate);

                if (level.HasValue)
                    query = query.WhereEquals(x => x.Level, level.Value);

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

        /// <summary>
        /// Gets the traces objects by environment and dates
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="fromDate">From date and time</param>
        /// <param name="toDate">To date and time</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Traces</returns>
        public Task<PagedList<TraceResult>> GetTracesByEnvironmentAsync(string environment, DateTime fromDate, DateTime toDate, int page, int pageSize = 50)
        {
            return RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var res = await session.Advanced.AsyncDocumentQuery<Traces_List.Result, Traces_List>()
                        .Statistics(out var stats)
                        .WhereEquals(x => x.Environment, environment)
                        .WhereBetween(x => x.Start, fromDate, toDate)
                        .OrderByDescending(x => x.Start)
                        .Skip(page * pageSize).Take(pageSize)
                        .ToListAsync().ConfigureAwait(false);

                return new PagedList<TraceResult>
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalResults = stats.TotalResults,
                    Data = res.Select(x => new TraceResult
                    {
                        Group = x.Group,
                        Count = x.Count,
                        Start = x.Start,
                        End = x.End,
                        HasErrors = x.HasError
                    }).ToList()
                };
            });
        }
        /// <summary>
        /// Get the traces from a Trace Group
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="groupName">Group name</param>
        /// <returns>Traces from that group</returns>
        public async Task<List<NodeTraceItem>> GetTracesByGroupIdAsync(string environment, string groupName)
        {
            var value = await RavenHelper.ExecuteAndReturnAsync(session =>
            {
                var documentQuery = session.Advanced.AsyncDocumentQuery<NodeTraceItem, Traces_ByGroupId>();
                var query = documentQuery
                    .WhereEquals(x => x.Environment, environment)
                    .WhereEquals(x => x.Group, groupName)
                    .OrderBy(x => x.Timestamp);
                return query.ToListAsync();
            });
            return value;
        }
        /// <summary>
        /// Gets the Trace object
        /// </summary>
        /// <returns>The trace object</returns>
        /// <param name="id">Trace object id</param>
        public async Task<SerializedObject> GetTraceObjectAsync(string id)
        {
            if (Settings.StoreTracesToDisk)
            {
                var traceItem = await RavenHelper.ExecuteAndReturnAsync(async session =>
                {
                    return await session.LoadAsync<NodeTraceItem>(id).ConfigureAwait(false);
                }).ConfigureAwait(false);

                try
                {
                    return await GetFromDisk(traceItem, ".nbin.gz").ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                    return await GetFromDatabase().ConfigureAwait(false);
                }
            }
            else
            {
                return await GetFromDatabase().ConfigureAwait(false);
            }

            #region Inner Methods
            async Task<SerializedObject> GetFromDisk(NodeTraceItem nodeTraceItem, string extension)
            {
                var bytes = await TraceDiskStorage.GetAsync(nodeTraceItem, extension).ConfigureAwait(false);
                if (bytes.IsGzip())
                    return NBinarySerializer.Deserialize<SerializedObject>(bytes);
                else
                    return bytes.DeserializeFromNBinary<SerializedObject>();
            }
            Task<SerializedObject> GetFromDatabase()
            {
                return RavenHelper.ExecuteAndReturnAsync(async session =>
                {
                    using (var attachment = await session.Advanced.Attachments.GetAsync(id, "Trace").ConfigureAwait(false))
                    {
                        if (attachment?.Stream is null) return null;
                        var bytes = await attachment.Stream.ReadAllBytesAsync().ConfigureAwait(false);
                        if (bytes.IsGzip())
                            return NBinarySerializer.Deserialize<SerializedObject>(bytes);
                        else
                            return bytes.DeserializeFromNBinary<SerializedObject>();
                    }
                });
            }
            #endregion
        }
        /// <summary>
        /// Gets the Trace object
        /// </summary>
        /// <returns>The trace object</returns>
        /// <param name="id">Trace object id</param>
        public async Task<string> GetTraceAsync(string id, string traceName)
        {
            if (Settings.StoreTracesToDisk)
            {
                var traceItem = await RavenHelper.ExecuteAndReturnAsync(async session =>
                {
                    return await session.LoadAsync<NodeTraceItem>(id).ConfigureAwait(false);
                }).ConfigureAwait(false);

                var extension = traceName == "TraceXml" ? ".xml.gz" : traceName == "TraceJson" ? ".json.gz" : ".txt.gz";
                try
                {
                    return await GetFromDisk(traceItem, extension).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                    return await GetFromDatabase().ConfigureAwait(false);
                }
            }
            else
            {
                return await GetFromDatabase().ConfigureAwait(false);
            }

            #region Inner Methods
            async Task<string> GetFromDisk(NodeTraceItem nodeTraceItem, string extension)
            {
                var bytes = await TraceDiskStorage.GetAsync(nodeTraceItem, extension).ConfigureAwait(false);
                if (bytes.IsGzip())
                {
                    var desBytes = Compressor.Decompress(bytes);
                    return Encoding.UTF8.GetString(desBytes.AsSpan());
                }
                else
                {
                    return Encoding.UTF8.GetString(bytes.AsSpan());
                }
            }
            async Task<string> GetFromDatabase()
            {
                return await RavenHelper.ExecuteAndReturnAsync(async session =>
                {
                    using (var attachment = await session.Advanced.Attachments.GetAsync(id, traceName).ConfigureAwait(false))
                    {
                        if (attachment?.Stream is null) return null;
                        var bytes = await attachment.Stream.ReadAllBytesAsync().ConfigureAwait(false);
                        if (bytes.IsGzip())
                        {
                            var desBytes = Compressor.Decompress(bytes);
                            return Encoding.UTF8.GetString(desBytes.AsSpan());
                        }
                        else
                        {
                            return Encoding.UTF8.GetString(bytes.AsSpan());
                        }
                    }
                }).ConfigureAwait(false);
            }
            #endregion
        }
        /// <summary>
        /// Gets the Trace object in xml
        /// </summary>
        /// <returns>The trace object</returns>
        /// <param name="id">Trace object id</param>
        public Task<string> GetTraceXmlAsync(string id)
            => GetTraceAsync(id, "TraceXml");
        /// <summary>
        /// Gets the Trace object in json
        /// </summary>
        /// <returns>The trace object</returns>
        /// <param name="id">Trace object id</param>
        public Task<string> GetTraceJsonAsync(string id)
            => GetTraceAsync(id, "TraceJson");
        /// <summary>
		/// Gets the Trace object in txt
		/// </summary>
		/// <returns>The trace object</returns>
		/// <param name="id">Trace object id</param>
        public Task<string> GetTraceTxtAsync(string id)
            => GetTraceAsync(id, "TraceTxt");
        /// <summary>
        /// Search a term in the database
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="searchTerm">Term to search in the database</param>
        /// <param name="fromDate">From date and time</param>
        /// <param name="toDate">To date and time</param>
        /// <returns>Search results</returns>
        public async Task<SearchResults> SearchAsync(string environment, string searchTerm, DateTime fromDate, DateTime toDate)
        {
            var logsSearch = RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var logQuery = session.Advanced.AsyncDocumentQuery<NodeLogItem, Logs_Search>()
                    .WhereEquals(x => x.Environment, environment)
                    .WhereBetween(x => x.Timestamp, fromDate, toDate)
                    .OpenSubclause()
                    .Search(x => x.Group, searchTerm)
                    .Search(x => x.Code, searchTerm)
                    .Search(x => x.Type, searchTerm)
                    .Search(x => x.Group, "*" + searchTerm + "*")
                    .Search(x => x.Message, "*" + searchTerm + "*")
                    .CloseSubclause()
                    .OrderBy(x => x.Timestamp);
                return await logQuery.Take(1000).ToListAsync().ConfigureAwait(false);
            });

            var tracesSearch = RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var traceQuery = session.Advanced.AsyncDocumentQuery<NodeTraceItem, Traces_Search>()
                    .WhereEquals(x => x.Environment, environment)
                    .WhereBetween(x => x.Timestamp, fromDate, toDate)
                    .OpenSubclause()
                    .Search(x => x.Group, searchTerm)
                    .Search(x => x.Tags, searchTerm)
                    .Search(x => x.Name, searchTerm)
                    .Search(x => x.Group, "*" + searchTerm + "*")
                    .Search(x => x.Tags, "*" + searchTerm + "*")
                    .Search(x => x.Name, "*" + searchTerm + "*")
                    .CloseSubclause()
                    .OrderBy(x => x.Timestamp);
                return await traceQuery.Take(1000).ToListAsync().ConfigureAwait(false);
            });

            await Task.WhenAll(logsSearch, tracesSearch).ConfigureAwait(false);

            var logsData = await logsSearch;
            var tracesData = await tracesSearch;

            var lstData = new List<NodeInfo>(logsData);
            lstData.AddRange(tracesData);
            lstData.Sort((ia, ib) => ia.Timestamp.CompareTo(ib.Timestamp));

            return new SearchResults
            {
                Data = lstData
            };
        }
        /// <summary>
        /// Get metadata from a group name
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <returns>List of metadatas</returns>
        public async Task<KeyValue[]> GetMetadatas(string groupName)
        {
            var metas = await RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                return await session.Advanced.AsyncDocumentQuery<GroupMetadata, Metadata_ByGroup>()
                    .WhereEquals(x => x.GroupName, groupName)
                    .OrderByDescending(x => x.Timestamp)
                    .ToListAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
            return metas.SelectMany(m => m.Items).GroupBy(m => m.Key).Select(m => m.First()).ToArray();
        }
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
        /// <summary>
        /// Get Current Statuses
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <param name="machine">Machine name or null</param>
        /// <param name="application">Application name or null</param>
        /// <returns>Get Current Status list</returns>
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
        /// <summary>
        /// Get Counters
        /// </summary>
        /// <param name="environment">Environment name</param>
        /// <returns>List of counters</returns>
        public async Task<List<NodeCountersQueryItem>> GetCounters(string environment)
        {
            return await RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                return await session.Query<NodeCountersItem, Counters_CounterSelection>()
                    .Where(x => x.Environment == environment)
                    .OrderBy(x => x.Category)
                    .ThenBy(x => x.Name)
                    .Select(i => new NodeCountersQueryItem
                    {
                        Application = i.Application,
                        CountersId = i.CountersId,
                        Category = i.Category,
                        Name = i.Name,
                        Type = i.Type,
                        Level = i.Level,
                        Kind = i.Kind,
                        Unit = i.Unit,
                        TypeOfValue = i.TypeOfValue
                    })
                    .ToListAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
        /// <summary>
        /// Get Counter by counterId
        /// </summary>
		/// <param name="counterId">Counter Id</param>
        /// <returns>Counter data</returns>
        public async Task<NodeCountersQueryItem> GetCounter(Guid counterId)
        {
            return await RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                return await session.Query<NodeCountersItem>()
                    .Where(x => x.CountersId == counterId)
                    .Select(i => new NodeCountersQueryItem
                    {
                        Application = i.Application,
                        CountersId = i.CountersId,
                        Category = i.Category,
                        Name = i.Name,
                        Type = i.Type,
                        Level = i.Level,
                        Kind = i.Kind,
                        Unit = i.Unit,
                        TypeOfValue = i.TypeOfValue
                    })
                    .FirstOrDefaultAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
        /// <summary>
        /// Get Counter Values
        /// </summary>
        /// <param name="counterId">Counter id</param>
        /// <param name="fromDate">From date and time</param>
		/// <param name="toDate">To date and time</param>
        /// <param name="limit">Value limit</param>
        /// <returns>List of counter values</returns>
        public async Task<List<NodeCountersQueryValue>> GetCounterValues(Guid counterId, DateTime fromDate, DateTime toDate, int limit = 3600)
        {
            return await RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                return await session.Query<NodeCountersValue, Counters_ValueSelection>()
                    .Where(x => x.CountersId == counterId)
                    .Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate)
                    .OrderByDescending(x => x.Timestamp)
                    .Take(limit)
                    .Select(i => new NodeCountersQueryValue
                    {
                        Id = i.Id,
                        Timestamp = i.Timestamp,
                        Value = i.Value
                    })
                    .ToListAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get Last Counter Values
        /// </summary>
        /// <param name="counterId">Counter id</param>
        /// <param name="valuesDivision">Counter values division</param>
        /// <param name="samples">Samples quantity</param>
        /// <returns>Values list</returns>
        public async Task<List<NodeLastCountersValue>> GetLastCounterValues(Guid counterId, CounterValuesDivision valuesDivision, int samples = 0, DateTime? lastDate = default)
        {
            var counterDataTask = GetCounter(counterId);
            var toDate = Core.Now;
            var fromDate = toDate;

            #region Values Division
            switch (valuesDivision)
            {
                case CounterValuesDivision.QuarterDay:
                    fromDate = toDate.AddHours(-6);
                    if (samples == 0) samples = 36;
                    break;
                case CounterValuesDivision.HalfDay:
                    fromDate = toDate.AddHours(-12);
                    if (samples == 0) samples = 48;
                    break;
                case CounterValuesDivision.Day:
                    fromDate = toDate.AddDays(-1);
                    if (samples == 0) samples = 48;
                    break;
                case CounterValuesDivision.Week:
                    fromDate = toDate.AddDays(-7);
                    if (samples == 0) samples = 84;
                    break;
                case CounterValuesDivision.Month:
                    fromDate = toDate.AddMonths(-1);
                    if (samples == 0) samples = 60;
                    break;
                case CounterValuesDivision.TwoMonths:
                    fromDate = toDate.AddMonths(-2);
                    if (samples == 0) samples = 60;
                    break;
                case CounterValuesDivision.QuarterYear:
                    fromDate = toDate.AddMonths(-3);
                    if (samples == 0) samples = 90;
                    break;
                case CounterValuesDivision.HalfYear:
                    fromDate = toDate.AddMonths(-6);
                    if (samples == 0) samples = 90;
                    break;
                case CounterValuesDivision.Year:
                    fromDate = toDate.AddYears(-1);
                    if (samples == 0) samples = 73;
                    break;
            }
            #endregion

            var timeInterval = toDate.Subtract(fromDate);
            var minutes = (timeInterval.TotalMinutes / samples);
            var lstValues = new List<NodeLastCountersValue>();

            #region Get Values
            var counterValuesTask = RavenHelper.ExecuteAndReturnAsync(async session =>
            {
                var query = session.Query<NodeCountersValue, Counters_ValueSelection>()
                    .Where(x => x.CountersId == counterId)
                    .Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate)
                    .OrderByDescending(x => x.Timestamp)
                    .Select(i => new NodeCountersQueryValue
                    {
                        Id = i.Id,
                        Timestamp = i.Timestamp,
                        Value = i.Value
                    });
                var lst = new List<NodeCountersQueryValue>();
                var enumerator = await session.Advanced.StreamAsync(query).ConfigureAwait(false);
                while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                    lst.Add(enumerator.Current.Document);
                return lst;
            });
            #endregion

            #region List Values
            for (var i = 0; i < samples; i++)
            {
                if (i == 0)
                    lstValues.Add(new NodeLastCountersValue { Timestamp = fromDate });
                else
                    lstValues.Add(new NodeLastCountersValue { Timestamp = lstValues[i - 1].Timestamp.AddMinutes(minutes) });
            }
            #endregion

            var counterData = await counterDataTask.ConfigureAwait(false);
            var counterValues = await counterValuesTask.ConfigureAwait(false);

            #region Fill Values
            for (var i = 0; i < lstValues.Count; i++)
            {
                IEnumerable<NodeCountersQueryValue> cValues;
                var currentItem = lstValues[i];
                if (i == lstValues.Count - 1)
                {
                    cValues = counterValues.Where(item => item.Timestamp >= currentItem.Timestamp);
                }
                else
                {
                    var tDate = lstValues[i + 1].Timestamp;
                    cValues = counterValues.Where(item => item.Timestamp >= currentItem.Timestamp && item.Timestamp < tDate);
                }
                double res = 0;
                switch(counterData.Type)
                {
                    case Counters.CounterType.Average:
                        res = cValues.Any() ? cValues.Average(item => (double)Convert.ChangeType(item.Value, TypeCode.Double)) : 0;
                        break;
                    case Counters.CounterType.Cumulative:
                        res = cValues.Sum(item => (double)Convert.ChangeType(item.Value, TypeCode.Double));
                        //if (i > 0)
                        //    res += (double)lstValues[i - 1].Value;
                        break;
                    case Counters.CounterType.Current:
                        res = cValues.Sum(item => (double)Convert.ChangeType(item.Value, TypeCode.Double));
                        break;
                }
                currentItem.Timestamp = currentItem.Timestamp.TruncateTo(TimeSpan.FromMinutes(1));
                currentItem.Value = res;
            }
            #endregion

            #region Find LastDate
            if (lastDate.HasValue)
            {
                var dateIndex = lstValues.FindLastIndex(item => item.Timestamp == lastDate.Value);
                if (dateIndex > -1)
                    return lstValues.Skip(dateIndex).ToList();
            }
            #endregion

            return lstValues;
        }

        #region Nested Types
        private class TraceTempResult
        {
            public string Group { get; set; }
            public DateTime Timestamp { get; set; }
            public string Tags { get; set; }
        }
        #endregion
    }
}