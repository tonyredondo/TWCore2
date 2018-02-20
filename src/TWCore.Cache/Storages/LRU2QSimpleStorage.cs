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

using TWCore.Collections;
using TWCore.Serialization;
// ReSharper disable InconsistentNaming

namespace TWCore.Cache.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Cache storage using a collection with a fixed capacity and LRU 2Q replacement logic
    /// </summary>
    public class LRU2QSimpleStorage : CacheCollectionStorage
    {
        /// <inheritdoc />
        /// <summary>
        /// Cache storage using a collection with a fixed capacity and LRU 2Q replacement logic
        /// </summary>
        public LRU2QSimpleStorage() : base(new LRU2QSimpleCollection<string, (StorageItemMeta, SerializedObject)>()) { }
        /// <inheritdoc />
        /// <summary>
        /// Cache storage using a collection with a fixed capacity and LRU 2Q replacement logic
        /// </summary>
        /// <param name="capacity">Capacity of the storage</param>
        public LRU2QSimpleStorage(int capacity) : base(new LRU2QSimpleCollection<string, (StorageItemMeta, SerializedObject)>(capacity)) { }
        /// <inheritdoc />
        /// <summary>
        /// Cache storage using a collection with a fixed capacity and LRU 2Q replacement logic
        /// </summary>
        /// <param name="capacity">Capacity of the storage</param>
        /// <param name="threshold">Threshold of the storage</param>
        public LRU2QSimpleStorage(int capacity, int threshold) : base(new LRU2QSimpleCollection<string, (StorageItemMeta, SerializedObject)>(capacity, threshold)) { }
    }
}
