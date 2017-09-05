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

using TWCore.Settings;
// ReSharper disable InconsistentNaming

namespace TWCore.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// Collections Settings
    /// </summary>
    [SettingsContainer("Core.Collections")]
    public class CoreSettings : SettingsBase
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static CoreSettings Instance => Singleton<CoreSettings>.Instance;
        
        /// <summary>
        /// LFUCollection Default Capacity
        /// </summary>
        public int LFUCollectionDefaultCapacity { get; set; } = ushort.MaxValue;
        /// <summary>
        /// LFUCollection Default AgePolicy
        /// </summary>
        public int LFUCollectionDefaultAgePolicy { get; set; } = 1000;
        /// <summary>
        /// LRU2QCollection Default Capacity
        /// </summary>
        public int LRU2QCollectionDefaultCapacity { get; set; } = ushort.MaxValue;
        /// <summary>
        /// LRU2QSimpleCollection Default Capacity
        /// </summary>
        public int LRU2QSimpleCollectionDefaultCapacity { get; set; } = ushort.MaxValue;
        /// <summary>
        /// LRUCollection Default Capacity
        /// </summary>
        public int LRUCollectionDefaultCapacity { get; set; } = ushort.MaxValue;
        /// <summary>
        /// WeakDictionary Throttled time in milliseconds to remove empty references.
        /// </summary>
        public int WeakDictionaryRemoveReferenceThrottledTimeInMs { get; set; } = 60000;
    }
}
