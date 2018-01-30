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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Serialization;

namespace TWCore.Cache.Client
{
    /// <inheritdoc />
    /// <summary>
    /// Async adapter for IStorage
    /// </summary>
    public class AsyncAdapter : IStorageAsync
    {
        private readonly IStorage _storage;

		/// <summary>
		/// Gets the Storage Type
		/// </summary>
		/// <value>The type.</value>
		public StorageType Type => _storage.Type;

        #region .ctor
        /// <summary>
        /// Async adapter for IStorage
        /// </summary>
        /// <param name="storage">IStorage to adapt</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncAdapter(IStorage storage)
        {
            Ensure.ArgumentNotNull(storage, "The storage can't be null.");
            _storage = storage;
            Core.Status.AttachChild(storage, this);
        }

        public void Dispose()
        {
            _storage.Dispose();
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
        public Task<bool> ExistKeyAsync(string key)
        {
            var res = _storage.ExistKey(key);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <returns>Dictionary true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Dictionary<string, bool>> ExistKeyAsync(string[] keys)
        {
            var res = _storage.ExistKey(keys);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the keys of all items stored in the Storage
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<string[]> GetKeysAsync()
        {
            var res = _storage.GetKeys();
            return Task.FromResult(res);
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
        public Task<DateTime?> GetCreationDateAsync(string key)
        {
            var res = _storage.GetCreationDate(key);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the expiration date for a storage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the expiration date of the storage item, null if the item hasn't expiration date or the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<DateTime?> GetExpirationDateAsync(string key)
        {
            var res = _storage.GetExpirationDate(key);
            return Task.FromResult(res);
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
        public Task<StorageItemMeta> GetMetaAsync(string key)
        {
            var res = _storage.GetMeta(key);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItemMeta> GetMetaAsync(string key, TimeSpan lastTime)
        {
            var res = _storage.GetMeta(key, lastTime);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItemMeta> GetMetaAsync(string key, DateTime comparer)
        {
            var res = _storage.GetMeta(key, comparer);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItemMeta[]> GetMetaByTagAsync(string[] tags)
        {
            var res = _storage.GetMetaByTag(tags);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItemMeta[]> GetMetaByTagAsync(string[] tags, bool containingAll)
        {
            var res = _storage.GetMetaByTag(tags, containingAll);
            return Task.FromResult(res);
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
        public Task<StorageItem> GetAsync(string key)
        {
            var res = _storage.Get(key);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItem> GetAsync(string key, TimeSpan lastTime)
        {
            var res = _storage.Get(key, lastTime);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItem> GetAsync(string key, DateTime comparer)
        {
            var res = _storage.Get(key, comparer);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItem[]> GetByTagAsync(string[] tags)
        {
            var res = _storage.GetByTag(tags);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<StorageItem[]> GetByTagAsync(string[] tags, bool containingAll)
        {
            var res = _storage.GetByTag(tags, containingAll);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Dictionary<string, StorageItem>> GetAsync(string[] keys)
        {
            var res = _storage.Get(keys);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Dictionary<string, StorageItem>> GetAsync(string[] keys, TimeSpan lastTime)
        {
            var res = _storage.Get(keys, lastTime);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Dictionary<string, StorageItem>> GetAsync(string[] keys, DateTime comparer)
        {
            var res = _storage.Get(keys, comparer);
            return Task.FromResult(res);
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
        public Task<bool> SetAsync(StorageItem item)
        {
            var res = _storage.Set(item);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SetAsync(string key, SerializedObject data)
        {
            var res = _storage.Set(key, data);
            return Task.FromResult(res);
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
        public Task<bool> SetAsync(string key, SerializedObject data, TimeSpan expirationDate)
        {
            var res = _storage.Set(key, data, expirationDate);
            return Task.FromResult(res);
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
        public Task<bool> SetAsync(string key, SerializedObject data, TimeSpan? expirationDate, string[] tags)
        {
            var res = _storage.Set(key, data, expirationDate, tags);
            return Task.FromResult(res);
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
        public Task<bool> SetAsync(string key, SerializedObject data, DateTime expirationDate)
        {
            var res = _storage.Set(key, data, expirationDate);
            return Task.FromResult(res);
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
        public Task<bool> SetAsync(string key, SerializedObject data, DateTime? expirationDate, string[] tags)
        {
            var res = _storage.Set(key, data, expirationDate, tags);
            return Task.FromResult(res);
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
        public Task<bool> UpdateDataAsync(string key, SerializedObject data)
        {
            var res = _storage.UpdateData(key, data);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Removes a StorageItem with the Key specified.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <returns>true if the data could be removed; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> RemoveAsync(string key)
        {
            var res = _storage.Remove(key);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<string[]> RemoveByTagAsync(string[] tags)
        {
            var res = _storage.RemoveByTag(tags);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<string[]> RemoveByTagAsync(string[] tags, bool containingAll)
        {
            var res = _storage.RemoveByTag(tags, containingAll);
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Copies an item to a new key.
        /// </summary>
        /// <param name="key">Key of an existing item</param>
        /// <param name="newKey">New key value</param>
        /// <returns>true if the copy was successful; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> CopyAsync(string key, string newKey)
        {
            var res = _storage.Copy(key, newKey);
            return Task.FromResult(res);
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
        public Task<StorageItem> GetOrSetAsync(string key, SerializedObject data)
        {
            var res = _storage.GetOrSet(key, data);
            return Task.FromResult(res);
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
        public Task<StorageItem> GetOrSetAsync(string key, SerializedObject data, TimeSpan expirationDate)
        {
            var res = _storage.GetOrSet(key, data, expirationDate);
            return Task.FromResult(res);
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
        public Task<StorageItem> GetOrSetAsync(string key, SerializedObject data, TimeSpan? expirationDate, string[] tags)
        {
            var res = _storage.GetOrSet(key, data, expirationDate, tags);
            return Task.FromResult(res);
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
        public Task<StorageItem> GetOrSetAsync(string key, SerializedObject data, DateTime expirationDate)
        {
            var res = _storage.GetOrSet(key, data, expirationDate);
            return Task.FromResult(res);
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
        public Task<StorageItem> GetOrSetAsync(string key, SerializedObject data, DateTime? expirationDate, string[] tags)
        {
            var res = _storage.GetOrSet(key, data, expirationDate, tags);
            return Task.FromResult(res);
        }
        #endregion


        /// <inheritdoc />
        /// <summary>
        /// Gets if the Storage is enabled.
        /// </summary>
        /// <returns>true if the storage is enabled; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> IsEnabledAsync()
        {
            var res = _storage.IsEnabled();
            return Task.FromResult(res);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets if the Storage is ready to be requested.
        /// </summary>
        /// <returns>true if the storage is ready; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> IsReadyAsync()
        {
            var res = _storage.IsReady();
            return Task.FromResult(res);
        }
        #endregion
    }
}
