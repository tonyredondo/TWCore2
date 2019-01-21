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

using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Serialization;
using TWCore.Threading;

namespace TWCore.Cache.Client.Redis
{
    /// <inheritdoc />
    /// <summary>
    /// Redis Cache client
    /// </summary>
    public class RedisCacheClient : IStorageAsync
    {
        private ConnectionMultiplexer _connection;
        private IDatabase _database;
        private readonly string _category;
        private const string MetaKey = "Meta";
        private const string DataKey = "Data";

        #region Properties
        /// <summary>
        /// Storage type
        /// </summary>
		public StorageType Type => StorageType.Network;
        /// <summary>
        /// Disable not supported exceptions
        /// </summary>
        public bool DisableNotSupportedExceptions { get; set; }
        /// <summary>
        /// Data serializer
        /// </summary>
        public ISerializer Serializer { get; set; } = SerializerManager.DefaultBinarySerializer;
        #endregion

        #region .ctor
        /// <summary>
        /// Redis Cache client
        /// </summary>
        /// <param name="configuration">Redis connection multiplexer configuration</param>
        /// <param name="category">Cache category</param>
        public RedisCacheClient(string configuration, string category = null)
        {
            _connection = Extensions.InvokeWithRetry(cfg => ConnectionMultiplexer.Connect(cfg), configuration, 5000, int.MaxValue).WaitAsync();
            _database = _connection.GetDatabase();
            _category = category ?? string.Empty;
        }
        #endregion

        #region Exist Key / Get Keys
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> ExistKeyAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) return TaskHelper.CompleteFalse;
            return _database.KeyExistsAsync(_category + key + DataKey);
        }
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <returns>Dictionary true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Dictionary<string, bool>> ExistKeyAsync(string[] keys)
        {
            if (keys?.Any() != true) return null;
            var keysResponse = await keys.Select(key => ExistKeyAsync(_category + key + DataKey)).ConfigureAwait(false);
            var dct = new Dictionary<string, bool>();
            for (var i = 0; i < keys.Length; i++)
                dct[keys[i]] = keysResponse[i];
            return dct;
        }
        /// <summary>
        /// Gets the keys of all items stored in the Storage
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<string[]> GetKeysAsync()
        {
            if (DisableNotSupportedExceptions) return Task.FromResult<string[]>(null);
            throw new NotSupportedException();
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
            if (string.IsNullOrEmpty(key)) return null;
            var stoBytes = await _database.StringGetAsync(_category + key + MetaKey).ConfigureAwait(false);
            if (stoBytes.IsNullOrEmpty) return null;
            var sobj = SerializedObject.FromMultiArray((byte[])stoBytes);
            var meta = (StorageItemMeta)sobj.GetValue();
            return meta.CreationDate;
        }
        /// <summary>
        /// Gets the expiration date for a storage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the expiration date of the storage item, null if the item hasn't expiration date or the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<DateTime?> GetExpirationDateAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            var expirationTime = await _database.KeyTimeToLiveAsync(key).ConfigureAwait(false);
            if (expirationTime.HasValue)
                return Core.Now.Add(expirationTime.Value);
            return null;
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
            if (string.IsNullOrEmpty(key)) return null;
            var stoBytes = await _database.StringGetAsync(_category + key + MetaKey).ConfigureAwait(false);
            if (stoBytes.IsNullOrEmpty) return null;
            var sobj = SerializedObject.FromMultiArray((byte[])stoBytes);
            var meta = (StorageItemMeta)sobj.GetValue();
            return meta;
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
            if (string.IsNullOrEmpty(key)) return null;
            var stoBytes = await _database.StringGetAsync(_category + key + MetaKey).ConfigureAwait(false);
            if (stoBytes.IsNullOrEmpty) return null;
            var sobj = SerializedObject.FromMultiArray((byte[])stoBytes);
            var meta = (StorageItemMeta)sobj.GetValue();
            if (Core.Now - meta.CreationDate > lastTime)
                return null;
            return meta;
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
            if (string.IsNullOrEmpty(key)) return null;
            var stoBytes = await _database.StringGetAsync(_category + key + MetaKey).ConfigureAwait(false);
            if (stoBytes.IsNullOrEmpty) return null;
            var sobj = SerializedObject.FromMultiArray((byte[])stoBytes);
            var meta = (StorageItemMeta)sobj.GetValue();
            if (comparer > meta.CreationDate)
                return null;
            return meta;

        }
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItemMeta[]> GetMetaByTagAsync(string[] tags)
        {
            if (DisableNotSupportedExceptions) return Task.FromResult<StorageItemMeta[]>(null);
            throw new NotSupportedException();
        }
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItemMeta[]> GetMetaByTagAsync(string[] tags, bool containingAll)
        {
            if (DisableNotSupportedExceptions) return Task.FromResult<StorageItemMeta[]>(null);
            throw new NotSupportedException();
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
            if (string.IsNullOrEmpty(key)) return null;
            var metaBytes = await _database.StringGetAsync(_category + key + MetaKey).ConfigureAwait(false);
            if (metaBytes.IsNullOrEmpty) return null;
            var metaSerObj = SerializedObject.FromMultiArray((byte[])metaBytes);
            var meta = (StorageItemMeta)metaSerObj.GetValue();
            var dataBytes = await _database.StringGetAsync(_category + key + DataKey).ConfigureAwait(false);
            if (dataBytes.IsNullOrEmpty) return null;
            var dataSerObj = SerializedObject.FromMultiArray((byte[])dataBytes);
            return new StorageItem(meta, dataSerObj);
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
            if (string.IsNullOrEmpty(key)) return null;
            var metaBytes = await _database.StringGetAsync(_category + key + MetaKey).ConfigureAwait(false);
            if (metaBytes.IsNullOrEmpty) return null;
            var metaSerObj = SerializedObject.FromMultiArray((byte[])metaBytes);
            var meta = (StorageItemMeta)metaSerObj.GetValue();
            if (Core.Now - meta.CreationDate > lastTime)
                return null;
            var dataBytes = await _database.StringGetAsync(_category + key + DataKey).ConfigureAwait(false);
            if (dataBytes.IsNullOrEmpty) return null;
            var dataSerObj = SerializedObject.FromMultiArray((byte[])dataBytes);
            return new StorageItem(meta, dataSerObj);

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
            if (string.IsNullOrEmpty(key)) return null;
            var metaBytes = await _database.StringGetAsync(_category + key + MetaKey).ConfigureAwait(false);
            if (metaBytes.IsNullOrEmpty) return null;
            var metaSerObj = SerializedObject.FromMultiArray((byte[])metaBytes);
            var meta = (StorageItemMeta)metaSerObj.GetValue();
            if (comparer > meta.CreationDate)
                return null;
            var dataBytes = await _database.StringGetAsync(_category + key + DataKey).ConfigureAwait(false);
            if (dataBytes.IsNullOrEmpty) return null;
            var dataSerObj = SerializedObject.FromMultiArray((byte[])dataBytes);
            return new StorageItem(meta, dataSerObj);
        }
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItem[]> GetByTagAsync(string[] tags)
        {
            if (DisableNotSupportedExceptions) return Task.FromResult<StorageItem[]>(null);
            throw new NotSupportedException();
        }
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItem[]> GetByTagAsync(string[] tags, bool containingAll)
        {
            if (DisableNotSupportedExceptions) return Task.FromResult<StorageItem[]>(null);
            throw new NotSupportedException();
        }
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Dictionary<string, StorageItem>> GetAsync(string[] keys)
        {
            if (keys?.Any() != true) return null;
            var keysResponse = await keys.Select(GetAsync).ConfigureAwait(false);
            var dct = new Dictionary<string, StorageItem>();
            for (var i = 0; i < keys.Length; i++)
                dct[keys[i]] = keysResponse[i];
            return dct;
        }
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Dictionary<string, StorageItem>> GetAsync(string[] keys, TimeSpan lastTime)
        {
            if (keys?.Any() != true) return null;
            var keysResponse = await keys.Select(GetAsync, lastTime).ConfigureAwait(false);
            var dct = new Dictionary<string, StorageItem>();
            for (var i = 0; i < keys.Length; i++)
                dct[keys[i]] = keysResponse[i];
            return dct;
        }
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Dictionary<string, StorageItem>> GetAsync(string[] keys, DateTime comparer)
        {
            if (keys?.Any() != true) return null;
            var keysResponse = await keys.Select(GetAsync, comparer).ConfigureAwait(false);
            var dct = new Dictionary<string, StorageItem>();
            for (var i = 0; i < keys.Length; i++)
                dct[keys[i]] = keysResponse[i];
            return dct;
        }
        #endregion

        #region Set Data
        /// <summary>
        /// Sets a new StorageItem with the given data
        /// </summary>
        /// <param name="item">StorageItem</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetAsync(StorageItem item)
        {
            if (item?.Meta == null) return false;
            var metaSerObj = new SerializedObject(item.Meta, Serializer);
            var metaMulti = metaSerObj.ToMultiArray();
            var dataMulti = item.Data?.ToMultiArray() ?? MultiArray<byte>.Empty;
            var timeSpan = item.Meta.ExpirationDate?.Subtract(Core.Now);
            var setResult = await Task.WhenAll(
                _database.StringSetAsync(_category + item.Meta.Key + DataKey, dataMulti.AsArray(), timeSpan),
                _database.StringSetAsync(_category + item.Meta.Key + MetaKey, metaMulti.AsArray(), timeSpan)).ConfigureAwait(false);
            return setResult.All(r => r);
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
            if (string.IsNullOrEmpty(key)) return false;
            return await SetAsync(new StorageItem(new StorageItemMeta()
            {
                Key = key,
                CreationDate = Core.Now
            }, data)).ConfigureAwait(false);
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
            if (string.IsNullOrEmpty(key)) return false;
            return await SetAsync(new StorageItem(new StorageItemMeta()
            {
                Key = key,
                CreationDate = Core.Now,
                ExpirationDate = Core.Now.Add(expirationDate)
            }, data)).ConfigureAwait(false);
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
            if (string.IsNullOrEmpty(key)) return false;
            return await SetAsync(new StorageItem(new StorageItemMeta()
            {
                Key = key,
                CreationDate = Core.Now,
                ExpirationDate = expirationDate != null ? Core.Now.Add(expirationDate.Value) : (DateTime?)null,
                Tags = tags.ToList()
            }, data)).ConfigureAwait(false);
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
            if (string.IsNullOrEmpty(key)) return false;
            return await SetAsync(new StorageItem(new StorageItemMeta()
            {
                Key = key,
                CreationDate = Core.Now,
                ExpirationDate = expirationDate
            }, data)).ConfigureAwait(false);
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
            if (string.IsNullOrEmpty(key)) return false;
            return await SetAsync(new StorageItem(new StorageItemMeta()
            {
                Key = key,
                CreationDate = Core.Now,
                ExpirationDate = expirationDate,
                Tags = tags.ToList()
            }, data)).ConfigureAwait(false);
        }
        #endregion

        #region Set Multi-Key Data
        /// <summary>
        /// Sets a new StorageItem with the given data
        /// </summary>
        /// <param name="items">StorageItem array</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetMultiAsync(StorageItem[] items)
        {
            if (items?.Any() != true) return false;
            var sMulti = (await items.Select(SetAsync).ConfigureAwait(false));
            return sMulti.All(s => s);
        }
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetMultiAsync(string[] keys, SerializedObject data)
        {
            var storages = keys.Select(key => new StorageItem(new StorageItemMeta()
            {
                Key = key,
                CreationDate = Core.Now
            }, data));
            var sMulti = await storages.Select(SetAsync).ConfigureAwait(false);
            return sMulti.All(s => s);
        }
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
            var storages = keys.Select(key => new StorageItem(new StorageItemMeta()
            {
                Key = key,
                CreationDate = Core.Now,
                ExpirationDate = Core.Now.Add(expirationDate)
            }, data));
            var sMulti = await storages.Select(SetAsync).ConfigureAwait(false);
            return sMulti.All(s => s);
        }
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
            var storages = keys.Select(key => new StorageItem(new StorageItemMeta()
            {
                Key = key,
                CreationDate = Core.Now,
                ExpirationDate = expirationDate != null ? Core.Now.Add(expirationDate.Value) : (DateTime?)null,
                Tags = tags.ToList()
            }, data));
            var sMulti = await storages.Select(SetAsync).ConfigureAwait(false);
            return sMulti.All(s => s);
        }
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
            var storages = keys.Select(key => new StorageItem(new StorageItemMeta()
            {
                Key = key,
                CreationDate = Core.Now,
                ExpirationDate = expirationDate
            }, data));
            var sMulti = await storages.Select(SetAsync).ConfigureAwait(false);
            return sMulti.All(s => s);
        }
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
            var storages = keys.Select(key => new StorageItem(new StorageItemMeta()
            {
                Key = key,
                CreationDate = Core.Now,
                ExpirationDate = expirationDate,
                Tags = tags.ToList()
            }, data));
            var sMulti = await storages.Select(SetAsync).ConfigureAwait(false);
            return sMulti.All(s => s);
        }
        #endregion

        #region Update/Remove Data/Copy
        /// <summary>
        /// Updates the data of an existing storage item.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">New item data</param>
        /// <returns>true if the data could be updated; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> UpdateDataAsync(string key, SerializedObject data)
        {
            if (string.IsNullOrEmpty(key)) return false;
            var dataMulti = data?.ToMultiArray() ?? MultiArray<byte>.Empty;
            return await _database.StringSetAsync(_category + key + DataKey, dataMulti.AsArray()).ConfigureAwait(false);
        }
        /// <summary>
        /// Removes a StorageItem with the Key specified.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <returns>true if the data could be removed; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            await _database.KeyDeleteAsync(_category + key + DataKey).ConfigureAwait(false);
            return await _database.KeyDeleteAsync(_category + key + MetaKey).ConfigureAwait(false);
        }
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<string[]> RemoveByTagAsync(string[] tags)
        {
            if (DisableNotSupportedExceptions) return Task.FromResult<string[]>(null);
            throw new NotSupportedException();
        }
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<string[]> RemoveByTagAsync(string[] tags, bool containingAll)
        {
            if (DisableNotSupportedExceptions) return Task.FromResult<string[]>(null);
            throw new NotSupportedException();
        }
        /// <summary>
        /// Copies an item to a new key.
        /// </summary>
        /// <param name="key">Key of an existing item</param>
        /// <param name="newKey">New key value</param>
        /// <returns>true if the copy was successful; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> CopyAsync(string key, string newKey)
        {
            if (string.IsNullOrEmpty(key)) return false;
            if (string.IsNullOrEmpty(newKey)) return false;
            var gObj = await GetAsync(key).ConfigureAwait(false);
            if (gObj?.Meta != null)
            {
                gObj.Meta.Key = newKey;
                return await SetAsync(gObj).ConfigureAwait(false);
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Gets if the Storage is enabled.
        /// </summary>
        /// <returns>true if the storage is enabled; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> IsEnabledAsync()
            => TaskHelper.CompleteTrue;
        /// <summary>
        /// Gets if the Storage is ready to be requested.
        /// </summary>
        /// <returns>true if the storage is ready; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> IsReadyAsync()
            => _connection.IsConnected ? TaskHelper.CompleteTrue : TaskHelper.CompleteFalse;

        /// <summary>
        /// Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_connection == null) return;
            _connection.Close();
            _connection = null;
            _database = null;
        }
    }
}
