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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Serialization;

namespace TWCore.Cache.Client
{
    /// <inheritdoc />
    /// <summary>
    /// Cache client connection pool
    /// </summary>
    public class CacheClientPool : IStorage
    {
        #region Statics
        private static readonly object Shared = new object();
        private static readonly PoolItemCollection AllItems = new PoolItemCollection();
        #endregion

        private readonly PoolItemCollection _pool;
        private readonly CacheClientPoolCounters _counters;

        #region Properties
        /// <summary>
        /// Data Serializer
        /// </summary>
        public ISerializer Serializer { get; set; } = SerializerManager.DefaultBinarySerializer;
        /// <summary>
        /// Force at least one network item enabled
        /// </summary>
        public bool ForceAtLeastOneNetworkItemEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _pool.ForceAtLeastOneNetworkItemEnabled; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { _pool.ForceAtLeastOneNetworkItemEnabled = value; }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the Storage Type
        /// </summary>
        /// <value>The type.</value>
        public StorageType Type => StorageType.Unknown;
        #endregion

        #region .ctor
        /// <summary>
        /// Cache client connection pool
        /// </summary>
        /// <param name="pingDelay">Delays between ping tries in milliseconds</param>
        /// <param name="pingDelayOnError">Delay after a ping error for next try</param>
        /// <param name="readMode">Cache pool Read Mode</param>
        /// <param name="writeMode">Cache pool Write Mode</param>
        /// <param name="selectionOrder">Pool item selection order for Read and Write</param>
        /// <param name="indexOrder">Index order</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CacheClientPool(int pingDelay = 5000, int pingDelayOnError = 30000, PoolReadMode readMode = PoolReadMode.NormalRead, PoolWriteMode writeMode = PoolWriteMode.WritesFirstAndThenAsync, PoolOrder selectionOrder = PoolOrder.PingTime, string indexOrder = null)
        {
            _pool = new PoolItemCollection(pingDelay, pingDelayOnError, readMode, writeMode, selectionOrder, indexOrder);
            _counters = new CacheClientPoolCounters();
            Core.Log.LibVerbose("CachePool.PingDelay = {0}", pingDelay);
            Core.Log.LibVerbose("CachePool.PingDelayOnError = {0}", pingDelayOnError);
            Core.Log.LibVerbose("CachePool.ReadMode = {0}", readMode);
            Core.Log.LibVerbose("CachePool.WriteMode = {0}", writeMode);
            Core.Log.LibVerbose("CachePool.SelectionOrder = {0}", selectionOrder);
            Core.Log.LibVerbose("CachePool.IndexOrder = {0}", indexOrder);
            Core.Status.Attach(col =>
            {
                Core.Status.AttachChild(_pool, this);
                Core.Status.AttachChild(_counters, this);
            }, this);
            Core.Status.DeAttachObject(AllItems);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a cache storage to the pool
        /// </summary>
        /// <param name="name">Pool item name</param>
        /// <param name="storage">Storage instance</param>
        /// <param name="mode">Storage mode inside the pool</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string name, IStorage storage, StorageItemMode mode = StorageItemMode.ReadAndWrite)
        {
            PoolItem pItem;
            lock (Shared)
            {
                if (AllItems.Contains(name))
                {
                    Core.Log.LibVerbose("Getting Cache Connection: {0}", name);
                    pItem = AllItems.GetByName(name);
                }
                else
                {
                    Core.Log.LibVerbose("Creating Cache Connection: {0}", name);
                    pItem = new PoolItem(name, storage, mode, _pool.PingDelay, _pool.PingDelayOnError);
                    AllItems.Add(pItem);
                }
            }
            Core.Log.LibVerbose("\tName = {0}, Mode = {1}, Type = {2}", name, pItem.Mode, pItem.Storage.Type);
            _pool.Add(pItem);
        }
        #endregion

        #region IStorage
        /// <inheritdoc />
        /// <summary>
        /// Init this storage
        /// </summary>
        public void Init()
        {
        }

        #region Exist Key / Get Keys
        /// <inheritdoc />
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExistKey(string key)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Read(key, (item, a1) => item.Storage.ExistKey(a1), r => r);
                w.StoreElapsed(_counters.IncrementExistKey);
                return res.Item1;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Get all storage keys.
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] GetKeys()
        {
            using (var w = Watch.Create())
            {
				var hKeys = new HashSet<string>(StringComparer.Ordinal);
                var poolEnabled = _pool.WaitAndGetEnabled (StorageItemMode.Read);
				foreach (var pItem in poolEnabled)
				{
				    var kList = pItem.Storage.GetKeys();
				    if (kList == null) continue;
				    foreach (var key in kList)
				        hKeys.Add(key);
				}
				var lKeys = hKeys.ToArray();
                w.StoreElapsed(_counters.IncrementGetKeys);
                return lKeys;
            }
        }
        #endregion

        #region Get Dates
        /// <inheritdoc />
        /// <summary>
        /// Gets the creation date for astorage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the creation date of the storage item, null if the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime? GetCreationDate(string key)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Read(key, (item, a1) => item.Storage.GetCreationDate(a1), r => r != null);
				_counters.IncrementGetCreationDate(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the expiration date for a storage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the expiration date of the storage item, null if the item hasn't expiration date or the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime? GetExpirationDate(string key)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Read(key, (item, a1) => item.Storage.GetExpirationDate(a1), r => r != null);
				_counters.IncrementGetExpirationDate(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
        #endregion

        #region Get MetaData
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta GetMeta(string key)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Read(key, (item, a1) => item.Storage.GetMeta(a1), r => r != null);
				_counters.IncrementGetMeta(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta GetMeta(string key, TimeSpan lastTime)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Read(key, lastTime, (item, a1, a2) => item.Storage.GetMeta(a1, a2), r => r != null);
				_counters.IncrementGetMeta(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta GetMeta(string key, DateTime comparer)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Read(key, comparer, (item, a1, a2) => item.Storage.GetMeta(a1, a2), r => r != null);
				_counters.IncrementGetMeta(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta[] GetMetaByTag(string[] tags) 
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Read(tags, (item, a1) => item.Storage.GetMetaByTag(a1), r => r?.Any() == true);
				_counters.IncrementGetMetaByTag(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta[] GetMetaByTag(string[] tags, bool containingAll)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Read(tags, containingAll, (item, a1, a2) => item.Storage.GetMetaByTag(a1, a2), r => r?.Any() == true);
				_counters.IncrementGetMetaByTag(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
        #endregion

        #region Get Data
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem Get(string key)
        {
            using (var w = Watch.Create())
            {
				var (sto, usedItem) = _pool.Read(key, (item, a1) => item.Storage.Get(a1), r => r != null);
				if (_pool.HasMemoryStorage && sto?.Meta != null && usedItem?.Storage.Type != StorageType.Memory)
					_pool.Write(sto, (item, a1) => item.Storage.Set(a1), true);
				_counters.IncrementGet(w.ElapsedMilliseconds);
                return sto;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem Get(string key, TimeSpan lastTime)
        {
            using (var w = Watch.Create())
            {
				var (sto, usedItem) = _pool.Read(key, lastTime, (item, a1, a2) => item.Storage.Get(a1, a2), r => r != null);
				if (_pool.HasMemoryStorage && sto?.Meta != null && usedItem?.Storage.Type != StorageType.Memory)
					_pool.Write(sto, (item, a1) => item.Storage.Set(a1), true);
				_counters.IncrementGet(w.ElapsedMilliseconds);
                return sto;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem Get(string key, DateTime comparer)
        {
            using (var w = Watch.Create())
            {
				var (sto, usedItem) = _pool.Read(key, comparer, (item, a1, a2) => item.Storage.Get(a1, a2), r => r != null);
				if (_pool.HasMemoryStorage && sto?.Meta != null && usedItem?.Storage.Type != StorageType.Memory)
					_pool.Write(sto, (item, a1) => item.Storage.Set(a1), true);
				_counters.IncrementGet(w.ElapsedMilliseconds);
                return sto;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem[] GetByTag(string[] tags)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Read(tags, (item, a1) => item.Storage.GetByTag(a1), r => r?.Any() == true);
				_counters.IncrementGetByTag(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem[] GetByTag(string[] tags, bool containingAll)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Read(tags, containingAll, (item, a1, a2) => item.Storage.GetByTag(a1, a2), r => r?.Any() == true);
				_counters.IncrementGetByTag(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
        #endregion

        #region Set Data
        /// <inheritdoc />
        /// <summary>
        /// Sets a new StorageItem with the given data
        /// </summary>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(StorageItem item)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Write(item, (p, arg1) => p.Storage.Set(arg1));
				_counters.IncrementSet(w.ElapsedMilliseconds);
                return res;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Write(key, data, (p, arg1, arg2) => p.Storage.Set(arg1, arg2));
				_counters.IncrementSet(w.ElapsedMilliseconds);
                return res;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data, TimeSpan expirationDate)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Write(key, data, expirationDate, (p, arg1, arg2, arg3) => p.Storage.Set(arg1, arg2, arg3));
				_counters.IncrementSet(w.ElapsedMilliseconds);
                return res;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data, TimeSpan? expirationDate, string[] tags)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Write(key, data, expirationDate, tags, (p, arg1, arg2, arg3, arg4) => p.Storage.Set(arg1, arg2, arg3, arg4));
				_counters.IncrementSet(w.ElapsedMilliseconds);
                return res;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data, DateTime expirationDate)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Write(key, data, expirationDate, (p, arg1, arg2, arg3) => p.Storage.Set(arg1, arg2, arg3));
				_counters.IncrementSet(w.ElapsedMilliseconds);
                return res;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data, DateTime? expirationDate, string[] tags)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Write(key, data, expirationDate, tags, (p, arg1, arg2, arg3, arg4) => p.Storage.Set(arg1, arg2, arg3, arg4));
				_counters.IncrementSet(w.ElapsedMilliseconds);
                return res;
            }
        }
        #endregion

        #region Update/Remove Data
        /// <inheritdoc />
        /// <summary>
        /// Updates the data of an existing storage item.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">New item data</param>
        /// <returns>true if the data could be updated; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UpdateData(string key, SerializedObject data)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Write(key, data, (item, arg1, arg2) => item.Storage.UpdateData(arg1, arg2));
				_counters.IncrementUpdateData(w.ElapsedMilliseconds);
                return res;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Removes a StorageItem with the Key specified.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <returns>true if the data could be removed; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(string key)
        {
            using (var w = Watch.Create())
            {
				var res = _pool.Write(key, (item, arg1) => item.Storage.Remove(arg1));
				_counters.IncrementRemove(w.ElapsedMilliseconds);
                return res;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] RemoveByTag(string[] tags)
        {
            using (var w = Watch.Create())
            {
                var poolEnabled = _pool.WaitAndGetEnabled(StorageItemMode.Write);
                var keys = poolEnabled.AsParallel().Select(pItem => pItem.Storage.RemoveByTag(tags)).RemoveNulls();
                var lstKeys = new List<string>();
                keys.Each(i => lstKeys.AddRange(i));
                var result = lstKeys.Distinct().ToArray();
				_counters.IncrementRemoveByTag(w.ElapsedMilliseconds);
                return result;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] RemoveByTag(string[] tags, bool containingAll)
        {
            using (var w = Watch.Create())
            {
                var poolEnabled = _pool.WaitAndGetEnabled(StorageItemMode.Write);
                var keys = poolEnabled.AsParallel().Select(pItem => pItem.Storage.RemoveByTag(tags, containingAll)).RemoveNulls();
                var lstKeys = new List<string>();
                keys.Each(i => lstKeys.AddRange(i));
                var result = lstKeys.Distinct().ToArray();
				_counters.IncrementRemoveByTag(w.ElapsedMilliseconds);
                return result;
            }
        }
        #endregion

        #region GetOrSet
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key, if the key doesn't exist then create one using the given values
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">Item data</param>
        /// <returns>Storage item instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem GetOrSet(string key, SerializedObject data)
        {
            using (var w = Watch.Create())
            {
                var sto = Get(key);
                if (sto == null)
                {
					_pool.Write(key, data, (item, arg1, arg2) => item.Storage.Set(arg1, arg2));
                    sto = Get(key);
                }
				_counters.IncrementGetOrSet(w.ElapsedMilliseconds);
                return sto;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key, if the key doesn't exist then create one using the given values
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">Item data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>Storage item instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem GetOrSet(string key, SerializedObject data, TimeSpan expirationDate)
        {
            using (var w = Watch.Create())
            {
                var sto = Get(key, expirationDate);
                if (sto == null)
                {
					_pool.Write(key, data, expirationDate, (item, arg1, arg2, arg3) => item.Storage.Set(arg1, arg2, arg3));
                    sto = Get(key);
                }
				_counters.IncrementGetOrSet(w.ElapsedMilliseconds);
                return sto;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key, if the key doesn't exist then create one using the given values
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">Item data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">String array with the Metadata tags</param>
        /// <returns>Storage item instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem GetOrSet(string key, SerializedObject data, TimeSpan? expirationDate, string[] tags)
        {
            using (var w = Watch.Create())
            {
                var sto = expirationDate.HasValue ? Get(key, expirationDate.Value) : Get(key);
                if (sto == null)
                {
					_pool.Write(key, data, expirationDate, tags, (item, arg1, arg2, arg3, arg4) => item.Storage.Set(arg1, arg2, arg3, arg4));
                    sto = Get(key);
                }
				_counters.IncrementGetOrSet(w.ElapsedMilliseconds);
                return sto;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key, if the key doesn't exist then create one using the given values
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">Item data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>Storage item instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem GetOrSet(string key, SerializedObject data, DateTime expirationDate)
        {
            using (var w = Watch.Create())
            {
                var sto = Get(key, expirationDate);
                if (sto == null)
                {
					_pool.Write(key, data, expirationDate, (item, arg1, arg2, arg3) => item.Storage.Set(arg1, arg2, arg3));
                    sto = Get(key);
                }
				_counters.IncrementGetOrSet(w.ElapsedMilliseconds);
                return sto;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key, if the key doesn't exist then create one using the given values
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">Item data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">String array with the Metadata tags</param>
        /// <returns>Storage item instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem GetOrSet(string key, SerializedObject data, DateTime? expirationDate, string[] tags)
        {
            using (var w = Watch.Create())
            {
                var sto = expirationDate.HasValue ? Get(key, expirationDate.Value) : Get(key);
                if (sto == null)
                {
					_pool.Write(key, data, expirationDate, tags, (item, arg1, arg2, arg3, arg4) => item.Storage.Set(arg1, arg2, arg3, arg4));
                    sto = Get(key);
                }
				_counters.IncrementGetOrSet(w.ElapsedMilliseconds);
                return sto;
            }
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Gets the keys of all items stored in the Storage
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEnabled() => _pool.AnyEnabled();
        /// <inheritdoc />
        /// <summary>
        /// Gets if the Storage is ready to be requested.
        /// </summary>
        /// <returns>true if the storage is ready; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsReady() => _pool.Read(p => p.Storage.IsReady(), r => r);
        #endregion

        #region IStorage Extensions
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, object data) => Set(key, Serializer.GetSerializedObject(data));
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, object data, TimeSpan expirationDate) => Set(key, Serializer.GetSerializedObject(data), expirationDate);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, object data, TimeSpan? expirationDate, string[] tags) => Set(key, Serializer.GetSerializedObject(data), expirationDate, tags);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, object data, DateTime expirationDate) => Set(key, Serializer.GetSerializedObject(data), expirationDate);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, object data, DateTime? expirationDate, string[] tags) => Set(key, Serializer.GetSerializedObject(data), expirationDate, tags);
        /// <summary>
        /// Updates the data of an existing storage item.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">New item data</param>
        /// <returns>true if the data could be updated; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UpdateData(string key, object data) => UpdateData(key, Serializer.GetSerializedObject(data));
        #endregion

        #region IDispose
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            _pool?.Items?.Each(i => AllItems.Items.Remove(i));
            _pool?.Dispose();
            Core.Status.DeAttachObject(this);
        }
        #endregion
    }
}
