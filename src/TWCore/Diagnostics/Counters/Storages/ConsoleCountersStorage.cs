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

namespace TWCore.Diagnostics.Counters.Storages
{
    /// <summary>
    /// Console counters storage
    /// </summary>
    public class ConsoleCountersStorage : ICountersStorage
    {
        /// <summary>
        /// Store counters 
        /// </summary>
        /// <param name="counterItems">Counters items enumerables</param>
        public void Store(List<ICounterItem> counterItems)
        {
            if (counterItems == null) return;
            foreach(var counter in counterItems)
            {
                Console.WriteLine($"Kind: {counter.Kind}, Category: {counter.Category}, Name: {counter.Name}, Type: {counter.Type}, ValueType: {counter.TypeOfValue.Name}, Count: {counter.Count}.");
            }
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
