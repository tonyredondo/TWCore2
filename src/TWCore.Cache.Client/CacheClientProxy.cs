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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Net.RPC.Client;
using TWCore.Net.RPC.Client.Transports;
using TWCore.Serialization;
// ReSharper disable InheritdocConsiderUsage
// ReSharper disable ClassNeverInstantiated.Global

namespace TWCore.Cache.Client
{
    /// <summary>
    /// Cache client RPC Proxy
    /// </summary>
    public class CacheClientProxy : RPCProxy, IStorageWithExtensionExecution, IStorageWithExtensionExecutionAsync
    {
        /// <summary>
        /// Gets the Storage Type
        /// </summary>
        /// <value>The type.</value>
        public StorageType Type => StorageType.Network;

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
        public bool ExistKey(string key) => Invoke<string, bool>(key);
        /// <inheritdoc />
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <returns>Dictionary true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, bool> ExistKey(string[] keys) => Invoke<string[], Dictionary<string, bool>>(keys);
        /// <summary>
        /// Get all storage keys.
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] GetKeys() => Invoke<string[]>();
        #endregion

        #region Get Dates
        /// <summary>
        /// Gets the creation date for astorage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the creation date of the storage item, null if the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime? GetCreationDate(string key) => Invoke<string, DateTime?>(key);
        /// <summary>
        /// Gets the expiration date for a storage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the expiration date of the storage item, null if the item hasn't expiration date or the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime? GetExpirationDate(string key) => Invoke<string, DateTime?>(key);
        #endregion

        #region Get MetaData
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta GetMeta(string key) => Invoke<string, StorageItemMeta>(key);
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta GetMeta(string key, TimeSpan lastTime) => Invoke<string, TimeSpan, StorageItemMeta>(key, lastTime);
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta GetMeta(string key, DateTime comparer) => Invoke<string, DateTime, StorageItemMeta>(key, comparer);
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta[] GetMetaByTag(string[] tags) => Invoke<string[], StorageItemMeta[]>(tags);
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta[] GetMetaByTag(string[] tags, bool containingAll) => Invoke<string[], bool, StorageItemMeta[]>(tags, containingAll);
        #endregion

        #region Get Data
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem Get(string key) => Invoke<string, StorageItem>(key);
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem Get(string key, TimeSpan lastTime) => Invoke<string, TimeSpan, StorageItem>(key, lastTime);
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem Get(string key, DateTime comparer) => Invoke<string, DateTime, StorageItem>(key, comparer);
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem[] GetByTag(string[] tags) => Invoke<string[], StorageItem[]>(tags);
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem[] GetByTag(string[] tags, bool containingAll) => Invoke<string[], bool, StorageItem[]>(tags, containingAll);
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, StorageItem> Get(string[] keys) => Invoke<string[], Dictionary<string, StorageItem>>(keys);
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, StorageItem> Get(string[] keys, TimeSpan lastTime) => Invoke<string[], TimeSpan, Dictionary<string, StorageItem>>(keys, lastTime);
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, StorageItem> Get(string[] keys, DateTime comparer) => Invoke<string[], DateTime, Dictionary<string, StorageItem>>(keys, comparer);
        #endregion

        #region Set Data
        /// <summary>
        /// Sets a new StorageItem with the given data
        /// </summary>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(StorageItem item) => Invoke<StorageItem, bool>(item);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="meta">Item Meta</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(StorageItemMeta meta, SerializedObject data) => Invoke<StorageItemMeta, SerializedObject, bool>(meta, data);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data) => Invoke<string, SerializedObject, bool>(key, data);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data, TimeSpan expirationDate) => Invoke<string, SerializedObject, TimeSpan, bool>(key, data, expirationDate);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data, TimeSpan? expirationDate, string[] tags) => Invoke<string, SerializedObject, TimeSpan?, string[], bool>(key, data, expirationDate, tags);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data, DateTime expirationDate) => Invoke<string, SerializedObject, DateTime, bool>(key, data, expirationDate);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data, DateTime? expirationDate, string[] tags) => Invoke<string, SerializedObject, DateTime?, string[], bool>(key, data, expirationDate, tags);
        #endregion

        #region Set Multi-Key Data
        /// <summary>
        /// Sets a new StorageItem with the given data
        /// </summary>
        /// <param name="items">StorageItem array</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetMulti(StorageItem[] items) => Invoke<StorageItem[], bool>(items);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetMulti(string[] keys, SerializedObject data) => Invoke<string[], SerializedObject, bool>(keys, data);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetMulti(string[] keys, SerializedObject data, TimeSpan expirationDate) => Invoke<string[], SerializedObject, TimeSpan, bool>(keys, data, expirationDate);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetMulti(string[] keys, SerializedObject data, TimeSpan? expirationDate, string[] tags) => Invoke<string[], SerializedObject, TimeSpan?, string[], bool>(keys, data, expirationDate, tags);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetMulti(string[] keys, SerializedObject data, DateTime expirationDate) => Invoke<string[], SerializedObject, DateTime, bool>(keys, data, expirationDate);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetMulti(string[] keys, SerializedObject data, DateTime? expirationDate, string[] tags) => Invoke<string[], SerializedObject, DateTime?, string[], bool>(keys, data, expirationDate, tags);
        #endregion

        #region Update/Remove Data/Copy
        /// <summary>
        /// Updates the data of an existing storage item.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">New item data</param>
        /// <returns>true if the data could be updated; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UpdateData(string key, SerializedObject data) => Invoke<string, SerializedObject, bool>(key, data);
        /// <summary>
        /// Removes a StorageItem with the Key specified.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <returns>true if the data could be removed; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(string key) => Invoke<string, bool>(key);
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] RemoveByTag(string[] tags) => Invoke<string[], string[]>(tags);
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] RemoveByTag(string[] tags, bool containingAll) => Invoke<string[], bool, string[]>(tags, containingAll);
        /// <inheritdoc />
        /// <summary>
        /// Copies an item to a new key.
        /// </summary>
        /// <param name="key">Key of an existing item</param>
        /// <param name="newKey">New key value</param>
        /// <returns>true if the copy was successful; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Copy(string key, string newKey) => Invoke<string, string, bool>(key, newKey);
        #endregion

        /// <summary>
        /// Gets the keys of all items stored in the Storage
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEnabled() => Invoke<bool>();
        /// <summary>
        /// Gets if the Storage is ready to be requested.
        /// </summary>
        /// <returns>true if the storage is ready; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsReady() => Invoke<bool>();
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
        public Task<bool> ExistKeyAsync(string key) => InvokeAsAsync<string, bool>(key);
        /// <inheritdoc />
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <returns>Dictionary true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Dictionary<string, bool>> ExistKeyAsync(string[] keys) => InvokeAsAsync<string[], Dictionary<string, bool>>(keys);
        /// <inheritdoc />
        /// <summary>
        /// Gets the keys of all items stored in the Storage
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<string[]> GetKeysAsync() => InvokeAsAsync<string[]>();
        #endregion

        #region Get Dates
        /// <summary>
        /// Gets the creation date for astorage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the creation date of the storage item, null if the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<DateTime?> GetCreationDateAsync(string key) => InvokeAsAsync<string, DateTime?>(key);
        /// <summary>
        /// Gets the expiration date for a storage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the expiration date of the storage item, null if the item hasn't expiration date or the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<DateTime?> GetExpirationDateAsync(string key) => InvokeAsAsync<string, DateTime?>(key);
        #endregion

        #region Get MetaData
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItemMeta> GetMetaAsync(string key) => InvokeAsAsync<string, StorageItemMeta>(key);
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItemMeta> GetMetaAsync(string key, TimeSpan lastTime) => InvokeAsAsync<string, TimeSpan, StorageItemMeta>(key, lastTime);
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItemMeta> GetMetaAsync(string key, DateTime comparer) => InvokeAsAsync<string, DateTime, StorageItemMeta>(key, comparer);
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItemMeta[]> GetMetaByTagAsync(string[] tags) => InvokeAsAsync<string[], StorageItemMeta[]>(tags);
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItemMeta[]> GetMetaByTagAsync(string[] tags, bool containingAll) => InvokeAsAsync<string[], bool, StorageItemMeta[]>(tags, containingAll);
        #endregion

        #region Get Data
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItem> GetAsync(string key) => InvokeAsAsync<string, StorageItem>(key);
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItem> GetAsync(string key, TimeSpan lastTime) => InvokeAsAsync<string, TimeSpan, StorageItem>(key, lastTime);
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItem> GetAsync(string key, DateTime comparer) => InvokeAsAsync<string, DateTime, StorageItem>(key, comparer);
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItem[]> GetByTagAsync(string[] tags) => InvokeAsAsync<string[], StorageItem[]>(tags);
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItem[]> GetByTagAsync(string[] tags, bool containingAll) => InvokeAsAsync<string[], bool, StorageItem[]>(tags, containingAll);
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Dictionary<string, StorageItem>> GetAsync(string[] keys) => InvokeAsAsync<string[], Dictionary<string, StorageItem>>(keys);
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Dictionary<string, StorageItem>> GetAsync(string[] keys, TimeSpan lastTime) => InvokeAsAsync<string[], TimeSpan, Dictionary<string, StorageItem>>(keys, lastTime);
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Dictionary<string, StorageItem>> GetAsync(string[] keys, DateTime comparer) => InvokeAsAsync<string[], DateTime, Dictionary<string, StorageItem>>(keys, comparer);
        #endregion

        #region Set Data
        /// <summary>
        /// Sets a new StorageItem with the given data
        /// </summary>
        /// <param name="item">StorageItem</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetAsync(StorageItem item) => InvokeAsAsync<StorageItem, bool>(item);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetAsync(string key, SerializedObject data) => InvokeAsAsync<string, SerializedObject, bool>(key, data);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetAsync(string key, SerializedObject data, TimeSpan expirationDate) => InvokeAsAsync<string, SerializedObject, TimeSpan, bool>(key, data, expirationDate);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetAsync(string key, SerializedObject data, TimeSpan? expirationDate, string[] tags) => InvokeAsAsync<string, SerializedObject, TimeSpan?, string[], bool>(key, data, expirationDate, tags);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetAsync(string key, SerializedObject data, DateTime expirationDate) => InvokeAsAsync<string, SerializedObject, DateTime, bool>(key, data, expirationDate);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetAsync(string key, SerializedObject data, DateTime? expirationDate, string[] tags) => InvokeAsAsync<string, SerializedObject, DateTime?, string[], bool>(key, data, expirationDate, tags);
        #endregion

        #region Set Multi-Key Data
        /// <summary>
        /// Sets a new StorageItem with the given data
        /// </summary>
        /// <param name="items">StorageItem array</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetMultiAsync(StorageItem[] items) => InvokeAsAsync<StorageItem[], bool>(items);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetMultiAsync(string[] keys, SerializedObject data) => InvokeAsAsync<string[], SerializedObject, bool>(keys, data);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetMultiAsync(string[] keys, SerializedObject data, TimeSpan expirationDate) => InvokeAsAsync<string[], SerializedObject, TimeSpan, bool>(keys, data, expirationDate);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetMultiAsync(string[] keys, SerializedObject data, TimeSpan? expirationDate, string[] tags) => InvokeAsAsync<string[], SerializedObject, TimeSpan?, string[], bool>(keys, data, expirationDate, tags);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetMultiAsync(string[] keys, SerializedObject data, DateTime expirationDate) => InvokeAsAsync<string[], SerializedObject, DateTime, bool>(keys, data, expirationDate);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetMultiAsync(string[] keys, SerializedObject data, DateTime? expirationDate, string[] tags) => InvokeAsAsync<string[], SerializedObject, DateTime?, string[], bool>(keys, data, expirationDate, tags);
        #endregion

        #region Update/Remove Data/Copy
        /// <summary>
        /// Updates the data of an existing storage item.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">New item data</param>
        /// <returns>true if the data could be updated; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> UpdateDataAsync(string key, SerializedObject data) => InvokeAsAsync<string, SerializedObject, bool>(key, data);
        /// <summary>
        /// Removes a StorageItem with the Key specified.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <returns>true if the data could be removed; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> RemoveAsync(string key) => InvokeAsAsync<string, bool>(key);
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<string[]> RemoveByTagAsync(string[] tags) => InvokeAsAsync<string[], string[]>(tags);
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<string[]> RemoveByTagAsync(string[] tags, bool containingAll) => InvokeAsAsync<string[], bool, string[]>(tags, containingAll);
        /// <inheritdoc />
        /// <summary>
        /// Copies an item to a new key.
        /// </summary>
        /// <param name="key">Key of an existing item</param>
        /// <param name="newKey">New key value</param>
        /// <returns>true if the copy was successful; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> CopyAsync(string key, string newKey) => InvokeAsAsync<string, string, bool>(key, newKey);
        #endregion

        /// <summary>
        /// Gets if the Storage is enabled.
        /// </summary>
        /// <returns>true if the storage is enabled; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> IsEnabledAsync() => InvokeAsAsync<bool>();
        /// <summary>
        /// Gets if the Storage is ready to be requested.
        /// </summary>
        /// <returns>true if the storage is ready; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> IsReadyAsync() => InvokeAsAsync<bool>();
        #endregion

        //
        #region IStorageWithExtensionExecution
        /// <summary>
        /// Execute an extension command
        /// </summary>
        /// <param name="extensionName">Extension name</param>
        /// <param name="command">Command to execute</param>
        /// <param name="args">Arguments of the command</param>
        /// <returns>Command response</returns>
        public object ExecuteExtension(string extensionName, string command, object[] args)
            => Invoke<string, string, object[], object>(extensionName, command, args);
        #endregion

        #region IStorageWithExtensionExecutionAsync
        /// <summary>
        /// Execute an extension command
        /// </summary>
        /// <param name="extensionName">Extension name</param>
        /// <param name="command">Command to execute</param>
        /// <param name="args">Arguments of the command</param>
        /// <returns>Command response</returns>
        public Task<object> ExecuteExtensionAsync(string extensionName, string command, object[] args)
            => InvokeAsAsync<string, string, object[], object>(extensionName, command, args);
        #endregion
        //

        #region Static Methods
        /// <summary>
        /// Get cache proxy client from a rpc transport client
        /// </summary>
        /// <param name="transport">Transport client object</param>
        /// <returns>Cache client proxy</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<CacheClientProxy> GetClientAsync(ITransportClient transport)
        {
            var rpcClient = new RPCClient(transport);
            var proxy = await rpcClient.CreateProxyAsync<CacheClientProxy>().ConfigureAwait(false);
            Core.Status.AttachChild(rpcClient, proxy);
            return proxy;
        }
        #endregion
    }
}
