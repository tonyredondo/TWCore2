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
using TWCore.Diagnostics.Api.Models.Log;
using TWCore.Diagnostics.Log;

namespace TWCore.Diagnostics.Api.MessageHandlers.RavenDb.Indexes
{
    public class Logs_Summary : AbstractIndexCreationTask<NodeLogItem, Logs_Summary.Result>
    {
        public class Result
        {
            public string Environment { get; set; }
            public string Application { get; set; }
            public DateTime Date { get; set; }
            public Level[] Levels { get; set; }

            public class Level
            {
                public LogLevel Name { get; set; }
                public int Count { get; set; }
            }
        }
        
        public Logs_Summary()
        {
            Map = logs =>
                from item in logs
                group item by new { item.Environment, item.Application, item.Timestamp.Date } into g
                select new
                {
                    Environment = g.Key.Environment,
                    Application = g.Key.Application,
                    Date = g.Key.Date,
                    Levels = g.GroupBy(x => x.Level).Select(x => new
                    {
                        Name = x.Key,
                        Count = 1
                    })
                };

            Reduce = results =>
                from item in results
                group item by new { item.Environment, item.Application, item.Date } into g
                select new
                {
                    Environment = g.Key.Environment,
                    Application = g.Key.Application,
                    Date = g.Key.Date,
                    Levels = g.SelectMany(x => x.Levels).GroupBy(x => x.Name).Select(x => new
                    {
                        Name = x.Key,
                        Count = x.Sum(y => y.Count)
                    })
                };
        }
    }
}