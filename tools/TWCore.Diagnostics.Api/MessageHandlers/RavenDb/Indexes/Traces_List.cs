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
using System.Linq;
using Raven.Client.Documents.Indexes;
using TWCore.Diagnostics.Api.Models.Trace;

namespace TWCore.Diagnostics.Api.MessageHandlers.RavenDb.Indexes
{
    public class Traces_List : AbstractIndexCreationTask<NodeTraceItem, Traces_List.Result>
    {
        public class Result
        {
            public string Environment { get; set; }
            public string Group { get; set; }
            public int Count { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public bool HasError { get; set; }
        }

        public Traces_List()
        {
            Map = traces => from trace in traces
                            select new
                            {
                                Environment = trace.Environment,
                                Group = trace.Group,
                                Count = 1,
                                Start = trace.Timestamp,
                                End = trace.Timestamp,
                                HasError = trace.Tags.Contains("Status: Error")
                            };
            Reduce = results => from item in results
                                group item by new { item.Group, item.Environment } into g
                                select new
                                {
                                    Environment = g.Key.Environment,
                                    Group = g.Key.Group,
                                    Count = g.Sum(i => i.Count),
                                    Start = g.Min(i => i.Start),
                                    End = g.Max(i => i.End),
                                    HasError = g.Any(i => i.HasError)
                                };
        }
    }
}