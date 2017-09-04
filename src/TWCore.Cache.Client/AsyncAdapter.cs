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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Serialization;

namespace TWCore.Cache.Client
{
    /// <summary>
    /// Async adapter for IStorage
    /// </summary>
    public class AsyncAdapter : IStorageAsync
    {
        IStorage _storage;

		/// <summary>
		/// Gets the Storage Type
		/// </summary>
		/// <value>The type.</value>
		public StorageType Type => _storage.Type;

        #region AsyncDelegates
        Func<string, Task<bool>> _existKeyAsync;
        Func<Task<string[]>> _getKeysAsync;

        Func<string, Task<DateTime?>> _getCreationDateAsync;
        Func<string, Task<DateTime?>> _getExpirationDateAsync;

        Func<string, Task<StorageItemMeta>> _getMetaAsync;
        Func<string, TimeSpan, Task<StorageItemMeta>> _getMetaAsync1;
        Func<string, DateTime, Task<StorageItemMeta>> _getMetaAsync2;
        Func<string[], Task<StorageItemMeta[]>> _getMetaByTagAsync;
        Func<string[], bool, Task<StorageItemMeta[]>> _getMetaByTagAsync1;

        Func<string, Task<StorageItem>> _getAsync;
        Func<string, TimeSpan, Task<StorageItem>> _getAsync1;
        Func<string, DateTime, Task<StorageItem>> _getAsync2;
        Func<string[], Task<StorageItem[]>> _getByTagAsync;
        Func<string[], bool, Task<StorageItem[]>> _getByTagAsync1;

        Func<StorageItem, Task<bool>> _setAsync;
        Func<string, SerializedObject, Task<bool>> _setAsync1;
        Func<string, SerializedObject, TimeSpan, Task<bool>> _setAsync2;
        Func<string, SerializedObject, TimeSpan?, string[], Task<bool>> _setAsync3;
        Func<string, SerializedObject, DateTime, Task<bool>> _setAsync4;
        Func<string, SerializedObject, DateTime?, string[], Task<bool>> _setAsync5;

        Func<string, SerializedObject, Task<bool>> _updateDataAsync;
        Func<string, Task<bool>> _removeAsync;
        Func<string[], Task<string[]>> _removeByTagAsync;
        Func<string[], bool, Task<string[]>> _removeByTagAsync1;

        Func<string, SerializedObject, Task<StorageItem>> _getOrSetAsync;
        Func<string, SerializedObject, TimeSpan, Task<StorageItem>> _getOrSetAsync1;
        Func<string, SerializedObject, TimeSpan?, string[], Task<StorageItem>> _getOrSetAsync2;
        Func<string, SerializedObject, DateTime, Task<StorageItem>> _getOrSetAsync3;
        Func<string, SerializedObject, DateTime?, string[], Task<StorageItem>> _getOrSetAsync4;

        Func<Task<bool>> _isEnabledAsync;
        Func<Task<bool>> _isReadyAsync;
        #endregion

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
            _existKeyAsync = FuncDelegate.CreateAsync<string, bool>(_storage.ExistKey);
            _getKeysAsync = FuncDelegate.CreateAsync(_storage.GetKeys);

            _getCreationDateAsync = FuncDelegate.CreateAsync<string, DateTime?>(_storage.GetCreationDate);
            _getExpirationDateAsync = FuncDelegate.CreateAsync<string, DateTime?>(_storage.GetExpirationDate);

            _getMetaAsync = FuncDelegate.CreateAsync<string, StorageItemMeta>(_storage.GetMeta);
            _getMetaAsync1 = FuncDelegate.CreateAsync<string, TimeSpan, StorageItemMeta>(_storage.GetMeta);
            _getMetaAsync2 = FuncDelegate.CreateAsync<string, DateTime, StorageItemMeta>(_storage.GetMeta);
            _getMetaByTagAsync = FuncDelegate.CreateAsync<string[], StorageItemMeta[]>(_storage.GetMetaByTag);
            _getMetaByTagAsync1 = FuncDelegate.CreateAsync<string[], bool, StorageItemMeta[]>(_storage.GetMetaByTag);

            _getAsync = FuncDelegate.CreateAsync<string, StorageItem>(_storage.Get);
            _getAsync1 = FuncDelegate.CreateAsync<string, TimeSpan, StorageItem>(_storage.Get);
            _getAsync2 = FuncDelegate.CreateAsync<string, DateTime, StorageItem>(_storage.Get);
            _getByTagAsync = FuncDelegate.CreateAsync<string[], StorageItem[]>(_storage.GetByTag);
            _getByTagAsync1 = FuncDelegate.CreateAsync<string[], bool, StorageItem[]>(_storage.GetByTag);

            _setAsync = FuncDelegate.CreateAsync<StorageItem, bool>(_storage.Set);
            _setAsync1 = FuncDelegate.CreateAsync<string, SerializedObject, bool>(_storage.Set);
            _setAsync2 = FuncDelegate.CreateAsync<string, SerializedObject, TimeSpan, bool>(_storage.Set);
            _setAsync3 = FuncDelegate.CreateAsync<string, SerializedObject, TimeSpan?, string[], bool>(_storage.Set);
            _setAsync4 = FuncDelegate.CreateAsync<string, SerializedObject, DateTime, bool>(_storage.Set);
            _setAsync5 = FuncDelegate.CreateAsync<string, SerializedObject, DateTime?, string[], bool>(_storage.Set);

            _updateDataAsync = FuncDelegate.CreateAsync<string, SerializedObject, bool>(_storage.UpdateData);
            _removeAsync = FuncDelegate.CreateAsync<string, bool>(_storage.Remove);
            _removeByTagAsync = FuncDelegate.CreateAsync<string[], string[]>(_storage.RemoveByTag);
            _removeByTagAsync1 = FuncDelegate.CreateAsync<string[], bool, string[]>(_storage.RemoveByTag);

            _getOrSetAsync = FuncDelegate.CreateAsync<string, SerializedObject, StorageItem>(_storage.GetOrSet);
            _getOrSetAsync1 = FuncDelegate.CreateAsync<string, SerializedObject, TimeSpan, StorageItem>(_storage.GetOrSet);
            _getOrSetAsync2 = FuncDelegate.CreateAsync<string, SerializedObject, TimeSpan?, string[], StorageItem>(_storage.GetOrSet);
            _getOrSetAsync3 = FuncDelegate.CreateAsync<string, SerializedObject, DateTime, StorageItem>(_storage.GetOrSet);
            _getOrSetAsync4 = FuncDelegate.CreateAsync<string, SerializedObject, DateTime?, string[], StorageItem>(_storage.GetOrSet);

            _isEnabledAsync = FuncDelegate.CreateAsync(_storage.IsEnabled);
            _isReadyAsync = FuncDelegate.CreateAsync(_storage.IsReady);
        }

        public void Dispose()
        {
            _storage.Dispose();
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
        public async Task<bool> ExistKeyAsync(string key) => await _existKeyAsync(key).ConfigureAwait(false);
        /// <summary>
        /// Gets the keys of all items stored in the Storage
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<string[]> GetKeysAsync() => await _getKeysAsync().ConfigureAwait(false);
        #endregion

        #region Get Dates
        /// <summary>
        /// Gets the creation date for astorage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the creation date of the storage item, null if the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<DateTime?> GetCreationDateAsync(string key) => await _getCreationDateAsync(key).ConfigureAwait(false);
        /// <summary>
        /// Gets the expiration date for a storage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the expiration date of the storage item, null if the item hasn't expiration date or the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<DateTime?> GetExpirationDateAsync(string key) => await _getExpirationDateAsync(key).ConfigureAwait(false);
        #endregion

        #region Get MetaData
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItemMeta> GetMetaAsync(string key) => await _getMetaAsync(key).ConfigureAwait(false);
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItemMeta> GetMetaAsync(string key, TimeSpan lastTime) => await _getMetaAsync1(key, lastTime).ConfigureAwait(false);
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItemMeta> GetMetaAsync(string key, DateTime comparer) => await _getMetaAsync2(key, comparer).ConfigureAwait(false);
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItemMeta[]> GetMetaByTagAsync(string[] tags) => await _getMetaByTagAsync(tags).ConfigureAwait(false);
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItemMeta[]> GetMetaByTagAsync(string[] tags, bool containingAll) => await _getMetaByTagAsync1(tags, containingAll).ConfigureAwait(false);
        #endregion

        #region Get Data
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem> GetAsync(string key) => await _getAsync(key).ConfigureAwait(false);
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem> GetAsync(string key, TimeSpan lastTime) => await _getAsync1(key, lastTime).ConfigureAwait(false);
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem> GetAsync(string key, DateTime comparer) => await _getAsync2(key, comparer).ConfigureAwait(false);
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem[]> GetByTagAsync(string[] tags) => await _getByTagAsync(tags).ConfigureAwait(false);
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem[]> GetByTagAsync(string[] tags, bool containingAll) => await _getByTagAsync1(tags, containingAll).ConfigureAwait(false);
        #endregion

        #region Set Data
        /// <summary>
        /// Sets a new StorageItem with the given data
        /// </summary>
        /// <param name="item">StorageItem</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetAsync(StorageItem item) => await _setAsync(item).ConfigureAwait(false);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetAsync(string key, SerializedObject data) => await _setAsync1(key, data).ConfigureAwait(false);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetAsync(string key, SerializedObject data, TimeSpan expirationDate) => await _setAsync2(key, data, expirationDate).ConfigureAwait(false);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetAsync(string key, SerializedObject data, TimeSpan? expirationDate, string[] tags) => await _setAsync3(key, data, expirationDate, tags).ConfigureAwait(false);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetAsync(string key, SerializedObject data, DateTime expirationDate) => await _setAsync4(key, data, expirationDate).ConfigureAwait(false);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SetAsync(string key, SerializedObject data, DateTime? expirationDate, string[] tags) => await _setAsync5(key, data, expirationDate, tags).ConfigureAwait(false);
        #endregion

        #region Update/Remove Data
        /// <summary>
        /// Updates the data of an existing storage item.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">New item data</param>
        /// <returns>true if the data could be updated; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> UpdateDataAsync(string key, SerializedObject data) => await _updateDataAsync(key, data).ConfigureAwait(false);
        /// <summary>
        /// Removes a StorageItem with the Key specified.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <returns>true if the data could be removed; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> RemoveAsync(string key) => await _removeAsync(key).ConfigureAwait(false);
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<string[]> RemoveByTagAsync(string[] tags) => await _removeByTagAsync(tags).ConfigureAwait(false);
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<string[]> RemoveByTagAsync(string[] tags, bool containingAll) => await _removeByTagAsync1(tags, containingAll).ConfigureAwait(false);
        #endregion

        #region GetOrSet
        /// <summary>
        /// Gets the StorageItem of a key, if the key doesn't exist then create one using the given values
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">Item data</param>
        /// <returns>Storage item instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem> GetOrSetAsync(string key, SerializedObject data) => await _getOrSetAsync(key, data).ConfigureAwait(false);
        /// <summary>
        /// Gets the StorageItem of a key, if the key doesn't exist then create one using the given values
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">Item data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>Storage item instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem> GetOrSetAsync(string key, SerializedObject data, TimeSpan expirationDate) => await _getOrSetAsync1(key, data, expirationDate).ConfigureAwait(false);
        /// <summary>
        /// Gets the StorageItem of a key, if the key doesn't exist then create one using the given values
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">Item data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">String array with the Metadata tags</param>
        /// <returns>Storage item instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem> GetOrSetAsync(string key, SerializedObject data, TimeSpan? expirationDate, string[] tags) => await _getOrSetAsync2(key, data, expirationDate, tags).ConfigureAwait(false);
        /// <summary>
        /// Gets the StorageItem of a key, if the key doesn't exist then create one using the given values
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">Item data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>Storage item instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem> GetOrSetAsync(string key, SerializedObject data, DateTime expirationDate) => await _getOrSetAsync3(key, data, expirationDate).ConfigureAwait(false);
        /// <summary>
        /// Gets the StorageItem of a key, if the key doesn't exist then create one using the given values
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">Item data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">String array with the Metadata tags</param>
        /// <returns>Storage item instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<StorageItem> GetOrSetAsync(string key, SerializedObject data, DateTime? expirationDate, string[] tags) => await _getOrSetAsync4(key, data, expirationDate, tags).ConfigureAwait(false);
        #endregion

        /// <summary>
        /// Gets if the Storage is enabled.
        /// </summary>
        /// <returns>true if the storage is enabled; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> IsEnabledAsync() => await _isEnabledAsync().ConfigureAwait(false);
        /// <summary>
        /// Gets if the Storage is ready to be requested.
        /// </summary>
        /// <returns>true if the storage is ready; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> IsReadyAsync() => await _isReadyAsync().ConfigureAwait(false);
        #endregion
    }
}
