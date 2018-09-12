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

// ReSharper disable CheckNamespace

using System.Collections.Concurrent;

namespace TWCore.Messaging
{
    /// <summary>
    /// Memory Queue Manager
    /// </summary>
    public static class MemoryQueueManager
    {
        private static readonly ConcurrentDictionary<(string Route, string Name), MemoryQueue> QueuesDictionary = new ConcurrentDictionary<(string Route, string Name), MemoryQueue>();

        /// <summary>
        /// Get a MemoryQueue instance from route and name pair
        /// </summary>
        /// <param name="route">Route pair</param>
        /// <param name="name">Name pair</param>
        /// <returns>MemoryQueue instance</returns>
        public static MemoryQueue GetQueue(string route, string name)
            => QueuesDictionary.GetOrAdd((route, name), _ => new MemoryQueue());
    }
}