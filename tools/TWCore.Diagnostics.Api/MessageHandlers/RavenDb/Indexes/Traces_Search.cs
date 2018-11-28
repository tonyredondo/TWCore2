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

    public class Traces_Search : AbstractIndexCreationTask<NodeTraceItem>
    {
        public Traces_Search()
        {
            Map = traces => from trace in traces
                          select new
                          {
                              Environment = trace.Environment,
                              Timestamp = trace.Timestamp,
                              Name = trace.Name,
                              Group = trace.Group,
                              Tags = trace.Tags,
                              //Application = trace.Application,
                              //Machine = trace.Machine
                          };

            Index(x => x.Group, FieldIndexing.Search);
            Index(x => x.Name, FieldIndexing.Search);
            Index(x => x.Tags, FieldIndexing.Search);
        }
    }
}