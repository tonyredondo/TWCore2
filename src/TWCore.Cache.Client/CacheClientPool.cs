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
    /// <summary>
    /// Cache client connection pool
    /// </summary>
    public class CacheClientPool : IStorage, IDisposable
    {
        #region Statics
        static object shared = new object();
        static PoolItemCollection AllItems = new PoolItemCollection();
        #endregion

        readonly PoolItemCollection Pool;
        readonly CacheClientPoolCounters Counters;

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
            get { return Pool.ForceAtLeastOneNetworkItemEnabled; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Pool.ForceAtLeastOneNetworkItemEnabled = value; }
        }
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
            Pool = new PoolItemCollection(pingDelay, pingDelayOnError, readMode, writeMode, selectionOrder, indexOrder);
            Counters = new CacheClientPoolCounters();
            Core.Log.LibVerbose("CachePool.PingDelay = {0}", pingDelay);
            Core.Log.LibVerbose("CachePool.PingDelayOnError = {0}", pingDelayOnError);
            Core.Log.LibVerbose("CachePool.ReadMode = {0}", readMode);
            Core.Log.LibVerbose("CachePool.WriteMode = {0}", writeMode);
            Core.Log.LibVerbose("CachePool.SelectionOrder = {0}", selectionOrder);
            Core.Log.LibVerbose("CachePool.IndexOrder = {0}", indexOrder);
            Core.Status.Attach(col =>
            {
                Core.Status.AttachChild(Pool, this);
                Core.Status.AttachChild(Counters, this);
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
        /// <param name="inMemoryStorage">Gets if the storage is in memory mode (Ignores the Pool write mode to ensure the write in a physical cache)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string name, IStorage storage, StorageItemMode mode = StorageItemMode.ReadAndWrite, bool inMemoryStorage = false)
        {
            PoolItem pItem = null;
            lock (shared)
            {
                if (AllItems.Contains(name))
                {
                    Core.Log.LibVerbose("Getting Cache Connection: {0}", name);
                    pItem = AllItems.GetByName(name);
                }
                else
                {
                    Core.Log.LibVerbose("Creating Cache Connection: {0}", name);
                    pItem = new PoolItem(name, storage, mode, inMemoryStorage, Pool.PingDelay, Pool.PingDelayOnError);
                    AllItems.Add(pItem);
                }
            }
            Core.Log.LibVerbose("\tName = {0}, Mode = {1}, InMemoryStorage = {2}", name, pItem.Mode, pItem.InMemoryStorage);
            Pool.Add(pItem);
        }
        #endregion

        #region IStorage
        /// <summary>
		/// Init this storage
		/// </summary>
        public void Init()
        {
        }

        #region Exist Key / Get Keys
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
				var res = Pool.Read(ref key, (PoolItem item, ref string a1) => item.Storage.ExistKey(a1), (ref bool r) => r);
                w.StoreElapsed(Counters.IncrementExistKey);
                return res.Item1;
            }
        }
        /// <summary>
        /// Get all storage keys.
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] GetKeys()
        {
            using (var w = Watch.Create())
            {
                var poolEnabled = Pool.WaitAndGetEnabled(StorageItemMode.Read);
                var keys = poolEnabled.AsParallel()
                    .Select(pItem => pItem.Storage.GetKeys())
                    .RemoveNulls()
                    .SelectMany(i => i)
                    .Distinct()
                    .ToArray();
                w.StoreElapsed(Counters.IncrementGetKeys);
                return keys;
            }
        }
        #endregion

        #region Get Dates
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
				var res = Pool.Read(ref key, (PoolItem item, ref string a1) => item.Storage.GetCreationDate(a1), (ref DateTime? r) => r != null);
				Counters.IncrementGetCreationDate(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
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
				var res = Pool.Read(ref key, (PoolItem item, ref string a1) => item.Storage.GetExpirationDate(a1), (ref DateTime? r) => r != null);
				Counters.IncrementGetExpirationDate(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
        #endregion

        #region Get MetaData
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
				var res = Pool.Read(ref key, (PoolItem item, ref string a1) => item.Storage.GetMeta(a1), (ref StorageItemMeta r) => r != null);
				Counters.IncrementGetMeta(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
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
				var res = Pool.Read(ref key, ref lastTime, (PoolItem item, ref string a1, ref TimeSpan a2) => item.Storage.GetMeta(a1, a2), (ref StorageItemMeta r) => r != null);
				Counters.IncrementGetMeta(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
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
				var res = Pool.Read(ref key, ref comparer, (PoolItem item, ref string a1, ref DateTime a2) => item.Storage.GetMeta(a1, a2), (ref StorageItemMeta r) => r != null);
				Counters.IncrementGetMeta(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
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
				var res = Pool.Read(ref tags, (PoolItem item, ref string[] a1) => item.Storage.GetMetaByTag(a1), (ref StorageItemMeta[] r) => r?.Any() == true);
				Counters.IncrementGetMetaByTag(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
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
				var res = Pool.Read(ref tags, ref containingAll, (PoolItem item, ref string[] a1, ref bool a2) => item.Storage.GetMetaByTag(a1, a2), (ref StorageItemMeta[] r) => r?.Any() == true);
				Counters.IncrementGetMetaByTag(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
        #endregion

        #region Get Data
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
				var (sto, usedItem) = Pool.Read(ref key, 
				                                (PoolItem item, ref string a1) => item.Storage.Get(a1), 
				                                (ref StorageItem r) => r != null);
				if (Pool.HasMemoryStorage && sto?.Meta != null && usedItem?.InMemoryStorage == false)
					Pool.Write(ref sto, (PoolItem item, ref StorageItem arg1) => item.Storage.Set(arg1), true);
				Counters.IncrementGet(w.ElapsedMilliseconds);
                return sto;
            }
        }
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
				var (sto, usedItem) = Pool.Read(ref key, ref lastTime, 
				                                (PoolItem item, ref string a1, ref TimeSpan a2) => item.Storage.Get(a1, a2), 
				                                (ref StorageItem r) => r != null);
				if (Pool.HasMemoryStorage && sto?.Meta != null && usedItem?.InMemoryStorage == false)
					Pool.Write(ref sto, (PoolItem item, ref StorageItem arg1) => item.Storage.Set(arg1), true);
				Counters.IncrementGet(w.ElapsedMilliseconds);
                return sto;
            }
        }
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
				var (sto, usedItem) = Pool.Read(ref key, ref comparer, 
				                                (PoolItem item, ref string a1, ref DateTime a2) => item.Storage.Get(a1, a2), 
				                                (ref StorageItem r) => r != null);
				if (Pool.HasMemoryStorage && sto?.Meta != null && usedItem?.InMemoryStorage == false)
					Pool.Write(ref sto, (PoolItem item, ref StorageItem arg1) => item.Storage.Set(arg1), true);
				Counters.IncrementGet(w.ElapsedMilliseconds);
                return sto;
            }
        }
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
				var res = Pool.Read(ref tags, 
				                    (PoolItem item, ref string[] a1) => item.Storage.GetByTag(a1), 
				                    (ref StorageItem[] r) => r?.Any() == true);
				Counters.IncrementGetByTag(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
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
				var res = Pool.Read(ref tags, ref containingAll, 
				                    (PoolItem item, ref string[] a1, ref bool a2) => item.Storage.GetByTag(a1, a2), 
				                    (ref StorageItem[] r) => r?.Any() == true);
				Counters.IncrementGetByTag(w.ElapsedMilliseconds);
                return res.Item1;
            }
        }
        #endregion

        #region Set Data
        /// <summary>
        /// Sets a new StorageItem with the given data
        /// </summary>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(StorageItem item)
        {
            using (var w = Watch.Create())
            {
				var res = Pool.Write(ref item, (PoolItem p, ref StorageItem arg1) => p.Storage.Set(arg1));
                w.StoreElapsed(Counters.IncrementSet);
                return res;
            }
        }
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
				var res = Pool.Write(ref key, ref data, (PoolItem item, ref string arg1, ref SerializedObject arg2) => item.Storage.Set(arg1, arg2));
                w.StoreElapsed(Counters.IncrementSet);
                return res;
            }
        }
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
				var res = Pool.Write(ref key, ref data, ref expirationDate,
				                     (PoolItem item, ref string arg1, ref SerializedObject arg2, ref TimeSpan arg3) => item.Storage.Set(arg1, arg2, arg3));
                w.StoreElapsed(Counters.IncrementSet);
                return res;
            }
        }
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
				var res = Pool.Write(ref key, ref data, ref expirationDate, ref tags,
				                     (PoolItem item, ref string arg1, ref SerializedObject arg2, ref TimeSpan? arg3, ref string[] arg4) => 
				                     	item.Storage.Set(arg1, arg2, arg3, arg4));
                w.StoreElapsed(Counters.IncrementSet);
                return res;
            }
        }
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
				var res = Pool.Write(ref key, ref data, ref expirationDate,
				                     (PoolItem item, ref string arg1, ref SerializedObject arg2, ref DateTime arg3) => item.Storage.Set(arg1, arg2, arg3));
                w.StoreElapsed(Counters.IncrementSet);
                return res;
            }
        }
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
				var res = Pool.Write(ref key, ref data, ref expirationDate, ref tags,
				                     (PoolItem item, ref string arg1, ref SerializedObject arg2, ref DateTime? arg3, ref string[] arg4) => 
				                     	item.Storage.Set(arg1, arg2, arg3, arg4));
                w.StoreElapsed(Counters.IncrementSet);
                return res;
            }
        }
        #endregion

        #region Update/Remove Data
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
				var res = Pool.Write(ref key, ref data, (PoolItem item, ref string arg1, ref SerializedObject arg2) => item.Storage.UpdateData(arg1, arg2));
                w.StoreElapsed(Counters.IncrementUpdateData);
                return res;
            }
        }
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
				var res = Pool.Write(ref key, (PoolItem item, ref string arg1) => item.Storage.Remove(arg1));
                w.StoreElapsed(Counters.IncrementRemove);
                return res;
            }
        }
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
                var poolEnabled = Pool.WaitAndGetEnabled(StorageItemMode.Write);
                var keys = poolEnabled.AsParallel().Select(pItem => pItem.Storage.RemoveByTag(tags)).RemoveNulls();
                var lstKeys = new List<string>();
                keys.Each(i => lstKeys.AddRange(i));
                var result = lstKeys.Distinct().ToArray();
                w.StoreElapsed(Counters.IncrementRemoveByTag);
                return result;
            }
        }
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
                var poolEnabled = Pool.WaitAndGetEnabled(StorageItemMode.Write);
                var keys = poolEnabled.AsParallel().Select(pItem => pItem.Storage.RemoveByTag(tags, containingAll)).RemoveNulls();
                var lstKeys = new List<string>();
                keys.Each(i => lstKeys.AddRange(i));
                var result = lstKeys.Distinct().ToArray();
                w.StoreElapsed(Counters.IncrementRemoveByTag);
                return result;
            }
        }
        #endregion

        #region GetOrSet
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
                    Pool.Write(a => a.Storage.Set(key, data));
                    sto = Get(key);
                }
                w.StoreElapsed(Counters.IncrementGetOrSet);
                return sto;
            }
        }
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
                    Pool.Write(a => a.Storage.Set(key, data, expirationDate));
                    sto = Get(key);
                }
                w.StoreElapsed(Counters.IncrementGetOrSet);
                return sto;
            }
        }
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
                    Pool.Write(a => a.Storage.Set(key, data, expirationDate, tags));
                    sto = Get(key);
                }
                w.StoreElapsed(Counters.IncrementGetOrSet);
                return sto;
            }
        }
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
                    Pool.Write(a => a.Storage.Set(key, data, expirationDate));
                    sto = Get(key);
                }
                w.StoreElapsed(Counters.IncrementGetOrSet);
                return sto;
            }
        }
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
                    Pool.Write(a => a.Storage.Set(key, data, expirationDate, tags));
                    sto = Get(key);
                }
                w.StoreElapsed(Counters.IncrementGetOrSet);
                return sto;
            }
        }
        #endregion

        /// <summary>
        /// Gets the keys of all items stored in the Storage
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEnabled() => Pool.AnyEnabled();
        /// <summary>
        /// Gets if the Storage is ready to be requested.
        /// </summary>
        /// <returns>true if the storage is ready; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsReady() => Pool.Read(p => p.Storage.IsReady(), (ref bool r) => r);
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
        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            Pool?.Items?.Each(i => AllItems.Items.Remove(i));
            Pool.Dispose();
            Core.Status.DeAttachObject(this);
        }
        #endregion
    }
}
