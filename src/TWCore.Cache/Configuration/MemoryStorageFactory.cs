/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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
using TWCore.Cache.Storages;
using TWCore.Collections;

namespace TWCore.Cache.Configuration
{
    /// <inheritdoc />
    /// <summary>
    /// Memory storage factory using the configuration parameters
    /// </summary>
    public class MemoryStorageFactory : CacheStorageFactoryBase
    {
        /// <inheritdoc />
        /// <summary>
        /// Create a new storage from a KeyValueCollection parameters
        /// </summary>
        /// <param name="parameters">Parameters to create the storage</param>
        /// <returns>Storage</returns>
        protected override StorageBase CreateStorage(KeyValueCollection parameters)
        {
            var capacity = parameters["Capacity"].ParseTo(1024);
            var name = parameters["Name"];

            if (string.Equals(name, "LRUStorage", StringComparison.OrdinalIgnoreCase))
                return new LRUStorage(capacity);

            if (string.Equals(name, "LRU2QSimpleStorage", StringComparison.OrdinalIgnoreCase))
            {
                var thresholdDiv = parameters["ThresholdDiv"].ParseTo(4);
                return new LRU2QSimpleStorage(capacity, capacity / thresholdDiv);
            }

            if (string.Equals(name, "LFUStorage", StringComparison.OrdinalIgnoreCase))
            {
                var agePolicy = parameters["AgePolicy"].ParseTo(1000);
                return new LFUStorage(capacity, agePolicy);
            }

            if (string.Equals(name, "LRU2QStorage", StringComparison.OrdinalIgnoreCase))
            {
                var kInDiv = parameters["KInDiv"].ParseTo(4);
                var kOutDiv = parameters["KOutDiv"].ParseTo(2);
                return new LRU2QStorage(capacity, capacity / kInDiv, capacity / kOutDiv);
            }

            return null;
        }
    }
}
