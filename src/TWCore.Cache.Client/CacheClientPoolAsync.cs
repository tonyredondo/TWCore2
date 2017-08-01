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
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Diagnostics.Log;
using TWCore.Serialization;

namespace TWCore.Cache.Client
{
    /// <summary>
    /// Cache client connection pool
    /// </summary>
    public class CacheClientPoolAsync : IStorageAsync, IDisposable
    {
        #region Statics
        static object shared = new object();
        static PoolAsyncItemCollection AllItems = new PoolAsyncItemCollection();
        #endregion

        readonly PoolAsyncItemCollection Pool;
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
        /// <summary>
        /// Write network items to memory when get
        /// </summary>
        public bool WriteNetworkItemsToMemoryOnGet { get; set; } = false;
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
        public CacheClientPoolAsync(int pingDelay = 5000, int pingDelayOnError = 30000, PoolReadMode readMode = PoolReadMode.NormalRead, PoolWriteMode writeMode = PoolWriteMode.WritesFirstAndThenAsync, PoolOrder selectionOrder = PoolOrder.PingTime, string indexOrder = null)
        {
            Pool = new PoolAsyncItemCollection(pingDelay, pingDelayOnError, readMode, writeMode, selectionOrder, indexOrder);
            Counters = new CacheClientPoolCounters();
            Core.Log.LibVerbose("CachePool.PingDelay = {0}", pingDelay);
            Core.Log.LibVerbose("CachePool.PingDelayOnError = {0}", pingDelayOnError);
            Core.Log.LibVerbose("CachePool.ReadMode = {0}", readMode);
            Core.Log.LibVerbose("CachePool.WriteMode = {0}", writeMode);
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
        public void Add(string name, IStorageAsync storage, StorageItemMode mode = StorageItemMode.ReadAndWrite, bool inMemoryStorage = false)
        {
            PoolAsyncItem pItem = null;
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
                    pItem = new PoolAsyncItem(name, storage, mode, inMemoryStorage, Pool.PingDelay, Pool.PingDelayOnError);
                    AllItems.Add(pItem);
                }
            }
            Core.Log.LibVerbose("\tName = {0}, Mode = {1}, InMemoryStorage = {2}", name, pItem.Mode, pItem.InMemoryStorage);
            Pool.Add(pItem);
        }
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
            PoolAsyncItem pItem = null;
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
                    pItem = new PoolAsyncItem(name, new AsyncAdapter(storage), mode, inMemoryStorage, Pool.PingDelay, Pool.PingDelayOnError);
                    AllItems.Add(pItem);
                }
            }
            Core.Log.LibVerbose("\tName = {0}, Mode = {1}, InMemoryStorage = {2}", name, pItem.Mode, pItem.InMemoryStorage);
            Pool.Add(pItem);
        }
        #endregion

        #region IStorageAsync 

        #region Exist Key / Get Keys
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> ExistKeyAsync(string key)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.ReadAsync(item => item.Storage.ExistKeyAsync(key), r => r).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementExistKey);
                return res;
            }
        }
        /// <summary>
        /// Gets the keys of all items stored in the Storage
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<string[]> GetKeysAsync()
        {
            using (var w = Watch.Create())
            {
                var poolEnabled = Pool.WaitAndGetEnabled(StorageItemMode.Read);
                var tasks = new Task<string[]>[poolEnabled.Count];
                for (var i = 0; i < tasks.Length; i++)
                    tasks[i] = poolEnabled[i].Storage.GetKeysAsync();
                var stringBags = await Task.WhenAll(tasks).ConfigureAwait(false);
                var keys = stringBags.RemoveNulls().SelectMany(i => i).Distinct().ToArray();
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
        public async Task<DateTime?> GetCreationDateAsync(string key)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.ReadAsync(item => item.Storage.GetCreationDateAsync(key), r => r != null).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementGetCreationDate);
                return res;
            }
        }
        /// <summary>
        /// Gets the expiration date for a storage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the expiration date of the storage item, null if the item hasn't expiration date or the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<DateTime?> GetExpirationDateAsync(string key)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.ReadAsync(item => item.Storage.GetExpirationDateAsync(key), r => r != null).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementGetExpirationDate);
                return res;
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
        public async Task<StorageItemMeta> GetMetaAsync(string key)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.ReadAsync(item => item.Storage.GetMetaAsync(key), r => r != null).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementGetMeta);
                return res;
            }
        }
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItemMeta> GetMetaAsync(string key, TimeSpan lastTime)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.ReadAsync(item => item.Storage.GetMetaAsync(key, lastTime), r => r != null).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementGetMeta);
                return res;
            }
        }
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItemMeta> GetMetaAsync(string key, DateTime comparer)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.ReadAsync(item => item.Storage.GetMetaAsync(key, comparer), r => r != null).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementGetMeta);
                return res;
            }
        }
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItemMeta[]> GetMetaByTagAsync(string[] tags)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.ReadAsync(item => item.Storage.GetMetaByTagAsync(tags), r => r?.Any() == true).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementGetMetaByTag);
                return res;
            }
        }
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItemMeta[]> GetMetaByTagAsync(string[] tags, bool containingAll)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.ReadAsync(item => item.Storage.GetMetaByTagAsync(tags, containingAll), r => r?.Any() == true).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementGetMetaByTag);
                return res;
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
        public async Task<StorageItem> GetAsync(string key)
        {
            using (var w = Watch.Create())
            {
                PoolAsyncItem usedItem = null;
                var sto = await Pool.ReadAsync(item => { usedItem = item; return item.Storage.GetAsync(key); }, r => r != null).ConfigureAwait(false);
                if (sto?.Meta != null && WriteNetworkItemsToMemoryOnGet && usedItem?.InMemoryStorage == false)
                    await Pool.WriteAsync(item => item.Storage.SetAsync(sto), true).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementGet);
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
        public async Task<StorageItem> GetAsync(string key, TimeSpan lastTime)
        {
            using (var w = Watch.Create())
            {
                PoolAsyncItem usedItem = null;
                var sto = await Pool.ReadAsync(item => { usedItem = item; return item.Storage.GetAsync(key, lastTime); }, r => r != null).ConfigureAwait(false);
                if (sto?.Meta != null && WriteNetworkItemsToMemoryOnGet && usedItem?.InMemoryStorage == false)
                    await Pool.WriteAsync(item => item.Storage.SetAsync(sto), true).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementGet);
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
        public async Task<StorageItem> GetAsync(string key, DateTime comparer)
        {
            using (var w = Watch.Create())
            {
                PoolAsyncItem usedItem = null;
                var sto = await Pool.ReadAsync(item => { usedItem = item; return item.Storage.GetAsync(key, comparer); }, r => r != null).ConfigureAwait(false);
                if (sto?.Meta != null && WriteNetworkItemsToMemoryOnGet && usedItem?.InMemoryStorage == false)
                    await Pool.WriteAsync(item => item.Storage.SetAsync(sto), true).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementGet);
                return sto;
            }
        }
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem[]> GetByTagAsync(string[] tags)
        {
            using (var w = Watch.Create())
            {
                var stos = await Pool.ReadAsync(item => item.Storage.GetByTagAsync(tags), r => r?.Any() == true).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementGetByTag);
                return stos;
            }
        }
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem[]> GetByTagAsync(string[] tags, bool containingAll)
        {
            using (var w = Watch.Create())
            {
                var stos = await Pool.ReadAsync(item => item.Storage.GetByTagAsync(tags, containingAll), r => r?.Any() == true).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementGetByTag);
                return stos;
            }
        }
        #endregion

        #region Set Data
        /// <summary>
        /// Sets a new StorageItem with the given data
        /// </summary>
        /// <param name="key">StorageItem</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetAsync(StorageItem item)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.WriteAsync(p => p.Storage.SetAsync(item)).ConfigureAwait(false);
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
        public async Task<bool> SetAsync(string key, SerializedObject data)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.WriteAsync(p => p.Storage.SetAsync(key, data)).ConfigureAwait(false);
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
        public async Task<bool> SetAsync(string key, SerializedObject data, TimeSpan expirationDate)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.WriteAsync(p => p.Storage.SetAsync(key, data, expirationDate)).ConfigureAwait(false);
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
        public async Task<bool> SetAsync(string key, SerializedObject data, TimeSpan? expirationDate, string[] tags)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.WriteAsync(p => p.Storage.SetAsync(key, data, expirationDate, tags)).ConfigureAwait(false);
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
        public async Task<bool> SetAsync(string key, SerializedObject data, DateTime expirationDate)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.WriteAsync(p => p.Storage.SetAsync(key, data, expirationDate)).ConfigureAwait(false);
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
        public async Task<bool> SetAsync(string key, SerializedObject data, DateTime? expirationDate, string[] tags)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.WriteAsync(p => p.Storage.SetAsync(key, data, expirationDate, tags)).ConfigureAwait(false);
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
        public async Task<bool> UpdateDataAsync(string key, SerializedObject data)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.WriteAsync(p => p.Storage.UpdateDataAsync(key, data)).ConfigureAwait(false);
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
        public async Task<bool> RemoveAsync(string key)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.WriteAsync(p => p.Storage.RemoveAsync(key)).ConfigureAwait(false);
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
        public async Task<string[]> RemoveByTagAsync(string[] tags)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.WriteAsync(p => p.Storage.RemoveByTagAsync(tags)).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementRemove);
                return res;
            }
        }
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<string[]> RemoveByTagAsync(string[] tags, bool containingAll)
        {
            using (var w = Watch.Create())
            {
                var res = await Pool.WriteAsync(p => p.Storage.RemoveByTagAsync(tags, containingAll)).ConfigureAwait(false);
                w.StoreElapsed(Counters.IncrementRemove);
                return res;
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
        public async Task<StorageItem> GetOrSetAsync(string key, SerializedObject data)
        {
            using (var w = Watch.Create())
            {
                var sto = await GetAsync(key).ConfigureAwait(false);
                if (sto == null)
                {
                    await Pool.WriteAsync(a => a.Storage.SetAsync(key, data)).ConfigureAwait(false);
                    sto = await GetAsync(key).ConfigureAwait(false);
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
        public async Task<StorageItem> GetOrSetAsync(string key, SerializedObject data, TimeSpan expirationDate)
        {
            using (var w = Watch.Create())
            {
                var sto = await GetAsync(key, expirationDate).ConfigureAwait(false);
                if (sto == null)
                {
                    await Pool.WriteAsync(a => a.Storage.SetAsync(key, data, expirationDate)).ConfigureAwait(false);
                    sto = await GetAsync(key).ConfigureAwait(false);
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
        public async Task<StorageItem> GetOrSetAsync(string key, SerializedObject data, TimeSpan? expirationDate, string[] tags)
        {
            using (var w = Watch.Create())
            {
                var sto = expirationDate.HasValue ? await GetAsync(key, expirationDate.Value).ConfigureAwait(false) : await GetAsync(key).ConfigureAwait(false);
                if (sto == null)
                {
                    await Pool.WriteAsync(a => a.Storage.SetAsync(key, data, expirationDate, tags)).ConfigureAwait(false);
                    sto = await GetAsync(key).ConfigureAwait(false);
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
        public async Task<StorageItem> GetOrSetAsync(string key, SerializedObject data, DateTime expirationDate)
        {
            using (var w = Watch.Create())
            {
                var sto = await GetAsync(key, expirationDate).ConfigureAwait(false);
                if (sto == null)
                {
                    await Pool.WriteAsync(a => a.Storage.SetAsync(key, data, expirationDate)).ConfigureAwait(false);
                    sto = await GetAsync(key).ConfigureAwait(false);
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
        public async Task<StorageItem> GetOrSetAsync(string key, SerializedObject data, DateTime? expirationDate, string[] tags)
        {
            using (var w = Watch.Create())
            {
                var sto = expirationDate.HasValue ? await GetAsync(key, expirationDate.Value).ConfigureAwait(false) : await GetAsync(key).ConfigureAwait(false);
                if (sto == null)
                {
                    await Pool.WriteAsync(a => a.Storage.SetAsync(key, data, expirationDate, tags)).ConfigureAwait(false);
                    sto = await GetAsync(key).ConfigureAwait(false);
                }
                w.StoreElapsed(Counters.IncrementGetOrSet);
                return sto;
            }
        }
        #endregion

        /// <summary>
        /// Gets if the Storage is enabled.
        /// </summary>
        /// <returns>true if the storage is enabled; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> IsEnabledAsync() => Task.FromResult(Pool.AnyEnabled());
        /// <summary>
        /// Gets if the Storage is ready to be requested.
        /// </summary>
        /// <returns>true if the storage is ready; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> IsReadyAsync() => await Pool.ReadAsync(p => p.Storage.IsReadyAsync(), r => r).ConfigureAwait(false);
        #endregion

        #region IStorageAsync Extensions
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetAsync(string key, object data) => SetAsync(key, Serializer.GetSerializedObject(data));
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetAsync(string key, object data, TimeSpan expirationDate) => SetAsync(key, Serializer.GetSerializedObject(data), expirationDate);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetAsync(string key, object data, TimeSpan? expirationDate, string[] tags) => SetAsync(key, Serializer.GetSerializedObject(data), expirationDate, tags);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetAsync(string key, object data, DateTime expirationDate) => SetAsync(key, Serializer.GetSerializedObject(data), expirationDate);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetAsync(string key, object data, DateTime? expirationDate, string[] tags) => SetAsync(key, Serializer.GetSerializedObject(data), expirationDate, tags);
        /// <summary>
        /// Updates the data of an existing storage item.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">New item data</param>
        /// <returns>true if the data could be updated; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> UpdateDataAsync(string key, object data) => UpdateDataAsync(key, Serializer.GetSerializedObject(data));
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
