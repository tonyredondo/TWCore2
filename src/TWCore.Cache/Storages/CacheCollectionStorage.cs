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
using System.Runtime.CompilerServices;
using TWCore.Collections;
using TWCore.Diagnostics.Status;
using TWCore.Serialization;

namespace TWCore.Cache.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// Cache storage adapter to use with an ICacheCollection instance
    /// </summary>
    public class CacheCollectionStorage : StorageBase
    {
        private ICacheCollection<string, (StorageItemMeta, SerializedObject)> _storage;

		/// <inheritdoc />
		/// <summary>
		/// Gets the Storage Type
		/// </summary>
		/// <value>The type.</value>
		public override StorageType Type => StorageType.Memory;

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Cache storage using LRU Algorithm
        /// </summary>
        /// <param name="cacheCollection">ICacheCollection instance</param>
        protected CacheCollectionStorage(ICacheCollection<string, (StorageItemMeta, SerializedObject)> cacheCollection)
        {
            _storage = cacheCollection;
            SetReady(true);
            Core.Status.Attach(collection =>
            {
                if (_storage == null)
                    return;
                var percent = (double)_storage.Count / _storage.Capacity;
                collection.Add(nameof(_storage.Capacity), _storage.Capacity);
                collection.Add(nameof(_storage.Count), _storage.Count, percent <= 0.8 ? StatusItemValueStatus.Ok : percent > 0.75 && percent < 1 ? StatusItemValueStatus.Warning : StatusItemValueStatus.Ok, true);
                collection.Add(nameof(_storage.Hits), _storage.Hits, true);
                collection.Add(nameof(_storage.Inserts), _storage.Inserts, true);
                collection.Add(nameof(_storage.Deletes), _storage.Deletes, true);
            });
        }
        #endregion

        #region Abstract storage base
		/// <inheritdoc />
		/// <summary>
		/// Gets the items metadata.
		/// </summary>
		/// <value>IEnumerable with all items metadata</value>
		protected override IEnumerable<StorageItemMeta> Metas 
		{ 
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _storage.Values.Select(v => v.Item1);
		}
        /// <inheritdoc />
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnExistKey(string key)
            => _storage.ContainsKey(key);
        /// <inheritdoc />
        /// <summary>
        /// Get all storage keys.
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IEnumerable<string> OnGetKeys()
            => _storage.Keys;
        /// <inheritdoc />
        /// <summary>
        /// Tries to set the data to the storage
        /// </summary>
        /// <param name="meta">Storage item meta data</param>
        /// <param name="value">Storage item data</param>
        /// <returns>true in case the item has been added; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnSet(StorageItemMeta meta, SerializedObject value)
        {
            if (meta == null) return false;
            _storage.AddOrUpdate(meta.Key, (meta, value), (k, v) => (meta, value));
            return true;
        }
        /// <inheritdoc />
        /// <summary>
        /// Tries to remove an item from the storage
        /// </summary>
        /// <param name="key">Key of the item</param>
        /// <param name="meta">Removed Storage item metadata</param>
        /// <returns>true in case the item has been removed; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnRemove(string key, out StorageItemMeta meta)
        {
            if (_storage.TryRemove(key, out var value))
            {
                meta = value.Item1;
                return true;
            }
            meta = null;
            return false;
        }
        /// <inheritdoc />
        /// <summary>
        /// Tries to get the data from the storage
        /// </summary>
        /// <param name="key">Key of the item</param>
        /// <param name="value">Storage item</param>
        /// <param name="condition">Condition the item has to accomplish</param>
        /// <returns>true in case the storage item data has been retrieved; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnTryGet(string key, out StorageItem value, Predicate<StorageItemMeta> condition = null)
        {
            if (_storage.TryGetValue(key, out var item) && (condition == null || condition(item.Item1)))
            {
                value = new StorageItem(item.Item1, item.Item2);
                return true;
            }
            value = null;
            return false;
        }
        /// <inheritdoc />
        /// <summary>
        /// Tries to get the metadata from the storage
        /// </summary>
        /// <param name="key">Key of the item</param>
        /// <param name="value">Storage Item Metadata instance</param>
        /// <param name="condition">Condition the item has to accomplish</param>
        /// <returns>true in case the storage item metadata has been retrieved; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnTryGetMeta(string key, out StorageItemMeta value, Predicate<StorageItemMeta> condition = null)
        {
            if (_storage.TryGetValue(key, out var item) && (condition == null || condition(item.Item1)))
            {
                value = item.Item1;
                return true;
            }
            value = null;
            return false;
        }
        #endregion

        #region Overrides
        /// <inheritdoc />
        /// <summary>
        /// Init this storage
        /// </summary>
        protected override void OnInit()
		{
			SetReady(true);
		}
        /// <inheritdoc />
        /// <summary>
        /// Release all resources
        /// </summary>
        protected override void OnDispose()
        {
            SetReady(false);
            _storage?.Clear();
            _storage = null;
            Core.Status.DeAttachObject(this);
        }
		#endregion
	}
}
