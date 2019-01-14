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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Serialization;
using TWCore.Threading;

namespace TWCore.Cache.Client
{
    /// <inheritdoc />
    /// <summary>
    /// Cache client connection pool
    /// </summary>
    [StatusName("Cache Client")]
    public class CacheClientPoolAsync : IStorageWithExtensionExecutionAsync
    {
        private readonly PoolAsyncItemCollection _pool;
        private readonly CacheClientPoolCounters _counters;
        private int _poolItemsCount;

        #region Properties
        /// <summary>
        /// Client name
        /// </summary>
        public string Name { get; }
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
        /// <summary>
        /// Write network items to memory when get
        /// </summary>
        public bool WriteNetworkItemsToMemoryOnGet { get; set; } = false;
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
        /// <param name="name">Client name</param>
        /// <param name="pingDelay">Delays between ping tries in milliseconds</param>
        /// <param name="pingDelayOnError">Delay after a ping error for next try</param>
        /// <param name="readMode">Cache pool Read Mode</param>
        /// <param name="writeMode">Cache pool Write Mode</param>
        /// <param name="selectionOrder">Pool item selection order for Read and Write</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CacheClientPoolAsync(string name, int pingDelay = 5000, int pingDelayOnError = 30000, PoolReadMode readMode = PoolReadMode.NormalRead, PoolWriteMode writeMode = PoolWriteMode.WritesFirstAndThenAsync, PoolOrder selectionOrder = PoolOrder.PingTime)
        {
            Name = name;
            _pool = new PoolAsyncItemCollection(pingDelay, pingDelayOnError, readMode, writeMode, selectionOrder);
            _counters = new CacheClientPoolCounters(name);
            Core.Log.LibVerbose("CachePool.PingDelay = {0}", pingDelay);
            Core.Log.LibVerbose("CachePool.PingDelayOnError = {0}", pingDelayOnError);
            Core.Log.LibVerbose("CachePool.ReadMode = {0}", readMode);
            Core.Log.LibVerbose("CachePool.WriteMode = {0}", writeMode);
            Core.Status.Attach(col =>
            {
                col.Add(nameof(Name), Name);
                col.Add(nameof(WriteNetworkItemsToMemoryOnGet), WriteNetworkItemsToMemoryOnGet);
                Core.Status.AttachChild(_pool, this);
            }, this);
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
        public void Add(string name, IStorageAsync storage, StorageItemMode mode = StorageItemMode.ReadAndWrite)
        {
            Core.Log.LibVerbose("Creating Cache Connection: {0}", name);
            var pItem = new PoolAsyncItem(_poolItemsCount++, name, storage, mode, _pool.PingDelay, _pool.PingDelayOnError);
            Core.Log.LibVerbose("\tName = {0}, Mode = {1}, Type = {2}", name, pItem.Mode, pItem.Storage.Type);
            _pool.Add(pItem);
        }
        /// <summary>
        /// Adds a cache storage to the pool
        /// </summary>
        /// <param name="name">Pool item name</param>
        /// <param name="clientProxy">ClientProxy instance</param>
        /// <param name="mode">Storage mode inside the pool</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string name, CacheClientProxy clientProxy, StorageItemMode mode = StorageItemMode.ReadAndWrite)
            => Add(name, (IStorageAsync)clientProxy, mode);

        /// <summary>
        /// Adds a cache storage to the pool
        /// </summary>
        /// <param name="name">Pool item name</param>
        /// <param name="storage">Storage instance</param>
        /// <param name="mode">Storage mode inside the pool</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string name, IStorage storage, StorageItemMode mode = StorageItemMode.ReadAndWrite)
        {
            Core.Log.LibVerbose("Creating Cache Connection: {0}", name);
            var pItem = new PoolAsyncItem(_poolItemsCount++, name, new AsyncAdapter(storage), mode, _pool.PingDelay, _pool.PingDelayOnError);
            Core.Log.LibVerbose("\tName = {0}, Mode = {1}, Type = {2}", name, pItem.Mode, pItem.Storage.Type);
            _pool.Add(pItem);
        }
        #endregion

        #region IStorageAsync

        #region Exist Key / Get Keys
        /// <inheritdoc />
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> ExistKeyAsync(string key)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.ReadAsync(key, (item, arg1) => item.Storage.ExistKeyAsync(arg1), r => r).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementExistKey(execTime);
            return res.Item1;
        }
        /// <inheritdoc />
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <returns>Dictionary true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Dictionary<string, bool>> ExistKeyAsync(string[] keys)
        {
            var startTime = Stopwatch.GetTimestamp();
            var poolEnabled = await _pool.WaitAndGetEnabledAsync(StorageItemMode.Read).ConfigureAwait(false);
            var tasks = new Task<Dictionary<string, bool>>[poolEnabled.Length];
            for (var i = 0; i < tasks.Length; i++)
                tasks[i] = poolEnabled[i].Storage.ExistKeyAsync(keys);
            var tasksResults = await Task.WhenAll(tasks).ConfigureAwait(false);
            var dictionary = new Dictionary<string, bool>();
            foreach (var innerDictio in tasksResults)
            {
                if (innerDictio is null) continue;
                foreach (var item in innerDictio)
                {
                    dictionary.TryGetValue(item.Key, out var value);
                    if (item.Value || !value)
                        dictionary[item.Key] = item.Value;
                }
            }
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementExistKey(execTime);
            return dictionary;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the keys of all items stored in the Storage
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<string[]> GetKeysAsync()
        {
            var startTime = Stopwatch.GetTimestamp();
            var poolEnabled = await _pool.WaitAndGetEnabledAsync(StorageItemMode.Read).ConfigureAwait(false);
            var tasks = new Task<string[]>[poolEnabled.Length];
            for (var i = 0; i < tasks.Length; i++)
                tasks[i] = poolEnabled[i].Storage.GetKeysAsync();
            var stringBags = await Task.WhenAll(tasks).ConfigureAwait(false);
            var keys = stringBags.RemoveNulls().SelectMany(i => i).Distinct().ToArray();
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetKeys(execTime);
            return keys;
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
        public async Task<DateTime?> GetCreationDateAsync(string key)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.ReadAsync(key, (item, arg1) => item.Storage.GetCreationDateAsync(arg1), r => r.HasValue).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetCreationDate(execTime);
            return res.Item1;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the expiration date for a storage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the expiration date of the storage item, null if the item hasn't expiration date or the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<DateTime?> GetExpirationDateAsync(string key)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.ReadAsync(key, (item, arg1) => item.Storage.GetExpirationDateAsync(arg1), r => r.HasValue).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetExpirationDate(execTime);
            return res.Item1;
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
        public async Task<StorageItemMeta> GetMetaAsync(string key)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.ReadAsync(key, (item, arg1) => item.Storage.GetMetaAsync(arg1), r => r != null).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetMeta(execTime);
            return res.Item1;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItemMeta> GetMetaAsync(string key, TimeSpan lastTime)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.ReadAsync(key, lastTime, (item, arg1, arg2) => item.Storage.GetMetaAsync(arg1, arg2), r => r != null).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetMeta(execTime);
            return res.Item1;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItemMeta> GetMetaAsync(string key, DateTime comparer)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.ReadAsync(key, comparer, (item, arg1, arg2) => item.Storage.GetMetaAsync(arg1, arg2), r => r != null).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetMeta(execTime);
            return res.Item1;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItemMeta[]> GetMetaByTagAsync(string[] tags)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.ReadAsync(tags, (item, arg1) => item.Storage.GetMetaByTagAsync(arg1), r => r?.Any() == true).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetMetaByTag(execTime);
            return res.Item1;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItemMeta[]> GetMetaByTagAsync(string[] tags, bool containingAll)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.ReadAsync(tags, containingAll, (item, arg1, arg2) => item.Storage.GetMetaByTagAsync(arg1, arg2), r => r?.Any() == true).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetMetaByTag(execTime);
            return res.Item1;
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
        public async Task<StorageItem> GetAsync(string key)
        {
            var startTime = Stopwatch.GetTimestamp();
            var (sto, usedItem) = await _pool.ReadAsync(key, (item, a1) => item.Storage.GetAsync(a1), r => r != null).ConfigureAwait(false);
            if (_pool.HasMemoryStorage && sto?.Meta != null && usedItem?.Storage.Type != StorageType.Memory)
                await _pool.WriteAsync(sto, (item, arg1) => item.Storage.SetAsync(arg1), true).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGet(execTime);
            return sto;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem> GetAsync(string key, TimeSpan lastTime)
        {
            var startTime = Stopwatch.GetTimestamp();
            var (sto, usedItem) = await _pool.ReadAsync(key, lastTime, (item, a1, a2) => item.Storage.GetAsync(a1, a2), r => r != null).ConfigureAwait(false);
            if (_pool.HasMemoryStorage && sto?.Meta != null && usedItem?.Storage.Type != StorageType.Memory)
                await _pool.WriteAsync(sto, (item, arg1) => item.Storage.SetAsync(arg1), true).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGet(execTime);
            return sto;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem> GetAsync(string key, DateTime comparer)
        {
            var startTime = Stopwatch.GetTimestamp();
            var (sto, usedItem) = await _pool.ReadAsync(key, comparer, (item, a1, a2) => item.Storage.GetAsync(a1, a2), r => r != null).ConfigureAwait(false);
            if (_pool.HasMemoryStorage && sto?.Meta != null && usedItem?.Storage.Type != StorageType.Memory)
                await _pool.WriteAsync(sto, (item, arg1) => item.Storage.SetAsync(arg1), true).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGet(execTime);
            return sto;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem[]> GetByTagAsync(string[] tags)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.ReadAsync(tags, (item, arg1) => item.Storage.GetByTagAsync(arg1), r => r?.Any() == true).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetByTag(execTime);
            return res.Item1;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem[]> GetByTagAsync(string[] tags, bool containingAll)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.ReadAsync(tags, containingAll, (item, arg1, arg2) => item.Storage.GetByTagAsync(arg1, arg2), r => r?.Any() == true).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetByTag(execTime);
            return res.Item1;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Dictionary<string, StorageItem>> GetAsync(string[] keys)
        {
            var startTime = Stopwatch.GetTimestamp();
            var poolEnabled = await _pool.WaitAndGetEnabledAsync(StorageItemMode.Read).ConfigureAwait(false);
            var tasks = new Task<Dictionary<string, StorageItem>>[poolEnabled.Length];
            for (var i = 0; i < tasks.Length; i++)
                tasks[i] = poolEnabled[i].Storage.GetAsync(keys);
            var tasksResults = await Task.WhenAll(tasks).ConfigureAwait(false);
            var dictionary = new Dictionary<string, StorageItem>();
            foreach (var innerDictio in tasksResults)
            {
                if (innerDictio is null) continue;
                foreach (var item in innerDictio)
                {
                    dictionary.TryGetValue(item.Key, out var value);
                    if (value is null)
                        dictionary[item.Key] = item.Value;
                }
            }
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementExistKey(execTime);
            return dictionary;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Dictionary<string, StorageItem>> GetAsync(string[] keys, TimeSpan lastTime)
        {
            var startTime = Stopwatch.GetTimestamp();
            var poolEnabled = await _pool.WaitAndGetEnabledAsync(StorageItemMode.Read).ConfigureAwait(false);
            var tasks = new Task<Dictionary<string, StorageItem>>[poolEnabled.Length];
            for (var i = 0; i < tasks.Length; i++)
                tasks[i] = poolEnabled[i].Storage.GetAsync(keys, lastTime);
            var tasksResults = await Task.WhenAll(tasks).ConfigureAwait(false);
            var dictionary = new Dictionary<string, StorageItem>();
            foreach (var innerDictio in tasksResults)
            {
                if (innerDictio is null) continue;
                foreach (var item in innerDictio)
                {
                    dictionary.TryGetValue(item.Key, out var value);
                    if (value is null)
                        dictionary[item.Key] = item.Value;
                }
            }
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementExistKey(execTime);
            return dictionary;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Dictionary<string, StorageItem>> GetAsync(string[] keys, DateTime comparer)
        {
            var startTime = Stopwatch.GetTimestamp();
            var poolEnabled = await _pool.WaitAndGetEnabledAsync(StorageItemMode.Read).ConfigureAwait(false);
            var tasks = new Task<Dictionary<string, StorageItem>>[poolEnabled.Length];
            for (var i = 0; i < tasks.Length; i++)
                tasks[i] = poolEnabled[i].Storage.GetAsync(keys, comparer);
            var tasksResults = await Task.WhenAll(tasks).ConfigureAwait(false);
            var dictionary = new Dictionary<string, StorageItem>();
            foreach (var innerDictio in tasksResults)
            {
                if (innerDictio is null) continue;
                foreach (var item in innerDictio)
                {
                    dictionary.TryGetValue(item.Key, out var value);
                    if (value is null)
                        dictionary[item.Key] = item.Value;
                }
            }
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementExistKey(execTime);
            return dictionary;
        }
        #endregion

        #region Set Data
        /// <inheritdoc />
        /// <summary>
        /// Sets a new StorageItem with the given data
        /// </summary>
        /// <param name="item">StorageItem</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetAsync(StorageItem item)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(item, (p, arg1) => p.Storage.SetAsync(arg1)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementSet(execTime);
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetAsync(string key, SerializedObject data)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(key, data, (item, arg1, arg2) => item.Storage.SetAsync(arg1, arg2)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementSet(execTime);
            return res;
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
        public async Task<bool> SetAsync(string key, SerializedObject data, TimeSpan expirationDate)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(key, data, expirationDate, (item, arg1, arg2, arg3) => item.Storage.SetAsync(arg1, arg2, arg3)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementSet(execTime);
            return res;
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
        public async Task<bool> SetAsync(string key, SerializedObject data, TimeSpan? expirationDate, string[] tags)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(key, data, expirationDate, tags, (item, arg1, arg2, arg3, arg4) => item.Storage.SetAsync(arg1, arg2, arg3, arg4)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementSet(execTime);
            return res;
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
        public async Task<bool> SetAsync(string key, SerializedObject data, DateTime expirationDate)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(key, data, expirationDate, (item, arg1, arg2, arg3) => item.Storage.SetAsync(arg1, arg2, arg3)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementSet(execTime);
            return res;
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
        public async Task<bool> SetAsync(string key, SerializedObject data, DateTime? expirationDate, string[] tags)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(key, data, expirationDate, tags, (item, arg1, arg2, arg3, arg4) => item.Storage.SetAsync(arg1, arg2, arg3, arg4)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementSet(execTime);
            return res;
        }
        #endregion

        #region Set Multi-Key Data
        /// <inheritdoc />
        /// <summary>
        /// Sets a new StorageItem with the given data
        /// </summary>
        /// <param name="items">StorageItem array</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetMultiAsync(StorageItem[] items)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(items, (p, arg1) => p.Storage.SetMultiAsync(arg1)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementSetMulti(execTime);
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetMultiAsync(string[] keys, SerializedObject data)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(keys, data, (item, arg1, arg2) => item.Storage.SetMultiAsync(arg1, arg2)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementSetMulti(execTime);
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetMultiAsync(string[] keys, SerializedObject data, TimeSpan expirationDate)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(keys, data, expirationDate, (item, arg1, arg2, arg3) => item.Storage.SetMultiAsync(arg1, arg2, arg3)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementSetMulti(execTime);
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetMultiAsync(string[] keys, SerializedObject data, TimeSpan? expirationDate, string[] tags)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(keys, data, expirationDate, tags, (item, arg1, arg2, arg3, arg4) => item.Storage.SetMultiAsync(arg1, arg2, arg3, arg4)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementSetMulti(execTime);
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetMultiAsync(string[] keys, SerializedObject data, DateTime expirationDate)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(keys, data, expirationDate, (item, arg1, arg2, arg3) => item.Storage.SetMultiAsync(arg1, arg2, arg3)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementSetMulti(execTime);
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetMultiAsync(string[] keys, SerializedObject data, DateTime? expirationDate, string[] tags)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(keys, data, expirationDate, tags, (item, arg1, arg2, arg3, arg4) => item.Storage.SetMultiAsync(arg1, arg2, arg3, arg4)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementSetMulti(execTime);
            return res;
        }
        #endregion

        #region Update/Remove Data/Copy
        /// <inheritdoc />
        /// <summary>
        /// Updates the data of an existing storage item.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">New item data</param>
        /// <returns>true if the data could be updated; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> UpdateDataAsync(string key, SerializedObject data)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(key, data, (item, arg1, arg2) => item.Storage.UpdateDataAsync(arg1, arg2)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementUpdateData(execTime);
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Removes a StorageItem with the Key specified.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <returns>true if the data could be removed; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> RemoveAsync(string key)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(key, (item, arg1) => item.Storage.RemoveAsync(arg1)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementRemove(execTime);
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<string[]> RemoveByTagAsync(string[] tags)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(tags, (item, arg1) => item.Storage.RemoveByTagAsync(arg1)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementRemove(execTime);
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<string[]> RemoveByTagAsync(string[] tags, bool containingAll)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(tags, containingAll, (item, arg1, arg2) => item.Storage.RemoveByTagAsync(arg1, arg2)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementRemove(execTime);
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Copies an item to a new key.
        /// </summary>
        /// <param name="key">Key of an existing item</param>
        /// <param name="newKey">New key value</param>
        /// <returns>true if the copy was successful; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> CopyAsync(string key, string newKey)
        {
            var startTime = Stopwatch.GetTimestamp();
            var res = await _pool.WriteAsync(key, newKey, (item, arg1, arg2) => item.Storage.CopyAsync(arg1, arg2)).ConfigureAwait(false);
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementCopy(execTime);
            return res;
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
            var startTime = Stopwatch.GetTimestamp();
            var sto = await GetAsync(key).ConfigureAwait(false);
            if (sto is null)
            {
                await _pool.WriteAsync(key, data, (item, arg1, arg2) => item.Storage.SetAsync(arg1, arg2)).ConfigureAwait(false);
                sto = await GetAsync(key).ConfigureAwait(false);
            }
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetOrSet(execTime);
            return sto;
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
            var startTime = Stopwatch.GetTimestamp();
            var sto = await GetAsync(key, expirationDate).ConfigureAwait(false);
            if (sto is null)
            {
                await _pool.WriteAsync(key, data, expirationDate, (item, arg1, arg2, arg3) => item.Storage.SetAsync(arg1, arg2, arg3)).ConfigureAwait(false);
                sto = await GetAsync(key).ConfigureAwait(false);
            }
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetOrSet(execTime);
            return sto;
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
            var startTime = Stopwatch.GetTimestamp();
            var sto = expirationDate.HasValue ? await GetAsync(key, expirationDate.Value).ConfigureAwait(false) : await GetAsync(key).ConfigureAwait(false);
            if (sto is null)
            {
                await _pool.WriteAsync(key, data, expirationDate, tags, (item, arg1, arg2, arg3, arg4) => item.Storage.SetAsync(arg1, arg2, arg3, arg4)).ConfigureAwait(false);
                sto = await GetAsync(key).ConfigureAwait(false);
            }
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetOrSet(execTime);
            return sto;
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
            var startTime = Stopwatch.GetTimestamp();
            var sto = await GetAsync(key, expirationDate).ConfigureAwait(false);
            if (sto is null)
            {
                await _pool.WriteAsync(key, data, expirationDate, (item, arg1, arg2, arg3) => item.Storage.SetAsync(arg1, arg2, arg3)).ConfigureAwait(false);
                sto = await GetAsync(key).ConfigureAwait(false);
            }
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetOrSet(execTime);
            return sto;
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
            var startTime = Stopwatch.GetTimestamp();
            var sto = expirationDate.HasValue ? await GetAsync(key, expirationDate.Value).ConfigureAwait(false) : await GetAsync(key).ConfigureAwait(false);
            if (sto is null)
            {
                await _pool.WriteAsync(key, data, expirationDate, tags, (item, arg1, arg2, arg3, arg4) => item.Storage.SetAsync(arg1, arg2, arg3, arg4)).ConfigureAwait(false);
                sto = await GetAsync(key).ConfigureAwait(false);
            }
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementGetOrSet(execTime);
            return sto;
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Gets if the Storage is enabled.
        /// </summary>
        /// <returns>true if the storage is enabled; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> IsEnabledAsync() => _pool.AnyEnabled() ? TaskHelper.CompleteTrue : TaskHelper.CompleteFalse;
        /// <inheritdoc />
        /// <summary>
        /// Gets if the Storage is ready to be requested.
        /// </summary>
        /// <returns>true if the storage is ready; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> IsReadyAsync() => await _pool.ReadAsync(p => p.Storage.IsReadyAsync(), r => r).ConfigureAwait(false);
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
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Item Keys</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetMultiAsync(string[] keys, object data) => SetMultiAsync(keys, Serializer.GetSerializedObject(data));
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Item Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetMultiAsync(string[] keys, object data, TimeSpan expirationDate) => SetMultiAsync(keys, Serializer.GetSerializedObject(data), expirationDate);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Item Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetMultiAsync(string[] keys, object data, TimeSpan? expirationDate, string[] tags) => SetMultiAsync(keys, Serializer.GetSerializedObject(data), expirationDate, tags);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Item Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetMultiAsync(string[] keys, object data, DateTime expirationDate) => SetMultiAsync(keys, Serializer.GetSerializedObject(data), expirationDate);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetMultiAsync(string[] keys, object data, DateTime? expirationDate, string[] tags) => SetMultiAsync(keys, Serializer.GetSerializedObject(data), expirationDate, tags);
        #endregion

        #region IStorageWithExtensionExecutionAsync
        /// <summary>
        /// Execute an extension command
        /// </summary>
        /// <param name="extensionName">Extension name</param>
        /// <param name="command">Command to execute</param>
        /// <param name="args">Arguments of the command</param>
        /// <returns>Command response</returns>
        public async Task<object> ExecuteExtensionAsync(string extensionName, string command, object[] args)
        {
            var startTime = Stopwatch.GetTimestamp();
            object response = null;
            var poolEnabled = await _pool.WaitAndGetEnabledAsync(StorageItemMode.Read).ConfigureAwait(false);
            foreach (var item in poolEnabled)
            {
                if (item.Storage is IStorageWithExtensionExecutionAsync sto)
                {
                    response = await sto.ExecuteExtensionAsync(extensionName, command, args).ConfigureAwait(false);
                    if (response != null) break;
                }
            }
            var execTime = (Stopwatch.GetTimestamp() - startTime) * 1000d / Stopwatch.Frequency;
            _counters.IncrementExecuteExtension(execTime);
            return response;
        }
        #endregion

        #region IDispose
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            _pool?.Dispose();
            Core.Status.DeAttachObject(this);
        }
        #endregion
    }
}
