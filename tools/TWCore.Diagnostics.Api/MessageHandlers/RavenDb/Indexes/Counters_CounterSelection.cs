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

using System.Linq;
using Raven.Client.Documents.Indexes;
using TWCore.Diagnostics.Api.Models.Counters;

namespace TWCore.Diagnostics.Api.MessageHandlers.RavenDb.Indexes
{
    public class Counters_CounterSelection: AbstractIndexCreationTask<NodeCountersItem>
    {
        public Counters_CounterSelection()
        {
            Map = counters => from counter in counters
                              select new
                              {
                                  Environment = counter.Environment,
                                  Application = counter.Application,
                                  Category = counter.Category,
                                  Name = counter.Name
                              };
        }
    }
}