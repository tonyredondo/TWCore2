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
using System.Runtime.Serialization;
using System.Threading;
using TWCore.Collections;
using TWCore.Diagnostics.Status;
using TWCore.Serialization;

namespace TWCore.Cache
{
    /// <summary>
    /// Storage Base class
    /// </summary>
    public abstract partial class StorageBase : IStorage
    {
        static StorageItemMeta[] EmptyMeta = new StorageItemMeta[0];
        static StorageItem[] EmptyItem = new StorageItem[0];
        static string[] EmptyStrings = new string[0];
        ManualResetEventSlim _readyEventSlim = new ManualResetEventSlim(false);
        int expirationCheckTimeInMinutes = 30;
        bool _init = false;
        Timer expirationTimer;
        string mName;

        #region Properties
        /// <summary>
        /// Gets or sets if the storage is enabled
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Gets or sets if the storage is ready
        /// </summary>
        public bool Ready => _readyEventSlim.IsSet;
        /// <summary>
        /// Gets or sets the time in minutes to check if some items has expired.
        /// </summary>
        public int ExpirationCheckTimeInMinutes { get { return expirationCheckTimeInMinutes; } set { expirationCheckTimeInMinutes = value; SetExpirationTimeout(); } }
        /// <summary>
        /// Maximum duration per storage item
        /// </summary>
        public TimeSpan? MaximumItemDuration { get; set; }
        /// <summary>
        /// Overwrites the expiration date setted by each item in DateTime.
        /// </summary>
        public DateTime? ItemsExpirationAbsoluteDateOverwrite { get; set; }
        /// <summary>
        /// Overwrites the expiration date setted by each item in TimeSpan.
        /// </summary>
        public TimeSpan? ItemsExpirationDateOverwrite { get; set; }
		/// <summary>
		/// Gets the Storage Type
		/// </summary>
		/// <value>The type.</value>
		public abstract StorageType Type { get; }
        #endregion

        #region Events
        /// <summary>
        /// Event when a item has been removed from the storage.
        /// </summary>
        public event EventHandler<ItemRemovedEventArgs> ItemRemoved;
        #endregion

        #region .ctor
        /// <summary>
        /// Storage Base class
        /// </summary>
        protected StorageBase()
        {
            Enabled = true;
            SetExpirationTimeout();
            mName = GetType().Name;
            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(Enabled), Enabled, Enabled ? StatusItemValueStatus.Green : StatusItemValueStatus.Red);
                collection.Add(nameof(Ready), Ready, Ready ? StatusItemValueStatus.Green : StatusItemValueStatus.Red);
                collection.Add(nameof(ExpirationCheckTimeInMinutes), ExpirationCheckTimeInMinutes);
                collection.Add(nameof(MaximumItemDuration), MaximumItemDuration);
                collection.Add(nameof(ItemsExpirationAbsoluteDateOverwrite), ItemsExpirationAbsoluteDateOverwrite);
                collection.Add(nameof(ItemsExpirationDateOverwrite), ItemsExpirationDateOverwrite);
            });
        }
        /// <summary>
        /// Storage destructor
        /// </summary>
        ~StorageBase()
        {
            Dispose(false);
        }
        #endregion

        #region Private/Protected Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetExpirationTimeout()
        {
            expirationTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            if (ExpirationCheckTimeInMinutes > 0)
            {
                var timeout = TimeSpan.FromMinutes(ExpirationCheckTimeInMinutes);
                expirationTimer = new Timer(item => CheckItemExpiration(), this, timeout, timeout);
            }
        }
        /// <summary>
        /// Checks items expiration
        /// </summary>
        /// <param name="initial">true if is the initial call of the method</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckItemExpiration(bool initial = false)
        {
            try
            {
                Core.Log.LibVerbose("Checking items expiration");
				var expiredItems = Metas.Where(m => m.IsExpired).ToArray();
                Core.Log.LibVerbose("Removing {0} expired items", expiredItems.Length);
                foreach (var item in expiredItems)
                {
                    var key = item.Key;
                    if (!string.IsNullOrEmpty(key))
                    {
						Core.Log.LibVerbose("Remove: {0}, on: {1}", key, mName);
                        if (OnRemove(key, out var meta) && !initial)
                        {
                            if (meta != null)
                                meta.OnExpire -= OnItemExpire;
                            ItemRemoved?.Invoke(this, new ItemRemovedEventArgs(key));
                            meta?.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        /// <summary>
        /// Method that handles when a item is expiring
        /// </summary>
        /// <param name="sender">Storage item meta expiring</param>
        /// <param name="e">Event args</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnItemExpire(object sender, EventArgs e)
        {
            var meta = (StorageItemMeta)sender;
            Core.Log.LibVerbose("Item Expired: {0}, on: {1}", meta.Key, GetType().Name);
            meta.OnExpire -= OnItemExpire;
            Remove(meta.Key);
        }
        /// <summary>
        /// Set Storage Ready value
        /// </summary>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetReady(bool value)
        {
            if (value)
                _readyEventSlim.Set();
            else
                _readyEventSlim.Reset();
        }
        #endregion

        #region Abstract Methods
		/// <summary>
		/// Gets the items metadata.
		/// </summary>
		/// <value>IEnumerable with all items metadata</value>
		protected abstract IEnumerable<StorageItemMeta> Metas { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        /// <summary>
        /// Tries to get the metadata from the storage
        /// </summary>
        /// <param name="key">Key of the item</param>
        /// <param name="value">Storage Item Metadata instance</param>
        /// <param name="condition">Condition the item has to accomplish</param>
        /// <returns>true in case the storage item metadata has been retrieved; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract bool OnTryGetMeta(string key, out StorageItemMeta value, Predicate<StorageItemMeta> condition = null);
        /// <summary>
        /// Tries to get the data from the storage
        /// </summary>
        /// <param name="key">Key of the item</param>
        /// <param name="value">Storage item</param>
        /// <param name="condition">Condition the item has to accomplish</param>
        /// <returns>true in case the storage item data has been retrieved; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract bool OnTryGet(string key, out StorageItem value, Predicate<StorageItemMeta> condition = null);
        /// <summary>
        /// Tries to set the data to the storage
        /// </summary>
        /// <param name="meta">Storage item meta data</param>
        /// <param name="value">Storage item data</param>
        /// <returns>true in case the item has been added; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract bool OnSet(StorageItemMeta meta, SerializedObject value);
        /// <summary>
        /// Tries to remove an item from the storage
        /// </summary>
        /// <param name="key">Key of the item</param>
        /// <param name="meta">Removed Storage item metadata</param>
        /// <returns>true in case the item has been removed; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract bool OnRemove(string key, out StorageItemMeta meta);
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnDispose() { }
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract bool OnExistKey(string key);
        /// <summary>
        /// Get all storage keys.
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract string[] OnGetKeys();
		/// <summary>
		/// Init this storage
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract void OnInit();
        #endregion

        #region Public Methods
        /// <summary>
		/// Init this storage
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            if (_init) return;
            _init = true;
            try
            {
                OnInit();
            }
            catch
            {
                _init = false;
                throw;
            }
        }
        /// <summary>
        /// Wait for the storage to be ready
        /// </summary>
        /// <param name="millisecondsTimeout">Timeout of the wait</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WaitForReady(int millisecondsTimeout = 0)
        {
            if (_readyEventSlim.IsSet) return;
            Core.Log.LibVerbose("Waiting for the storage to be ready to use.");
            if (millisecondsTimeout <= 0)
                _readyEventSlim.Wait();
            else
                _readyEventSlim.Wait(millisecondsTimeout);
            Core.Log.LibVerbose("The storage is ready.");
        }
        #endregion

        #region IStorage

        #region Exist Key / Get Keys
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExistKey(string key)
        {
            if (!Ready || string.IsNullOrEmpty(key)) return false;
            return OnExistKey(key);
        }
        /// <summary>
        /// Get all storage keys.
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] GetKeys()
        {
            if (!Ready) return null;
            return OnGetKeys();
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGetMeta(key, out var meta) ? meta.CreationDate : (DateTime?)null;
        }
        /// <summary>
        /// Gets the expiration date for a storage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the expiration date of the storage item, null if the item hasn't expiration date or the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime? GetExpirationDate(string key)
        {
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGetMeta(key, out var meta) ? meta.IsExpired ? null : meta.ExpirationDate : null;
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGetMeta(key, out var meta) ? meta : null;
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGetMeta(key, out var meta, i => (Core.Now - i.CreationDate) <= lastTime) ? meta : null;
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGetMeta(key, out var meta, i => i.CreationDate >= comparer) ? meta : null;
        }
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta[] GetMetaByTag(string[] tags)
            => GetMetaByTag(tags, false);
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta[] GetMetaByTag(string[] tags, bool containingAll)
        {
            if (!Ready || tags == null) return null;
            if (tags.Length == 0) return EmptyMeta;
            return Metas.AsParallel().Where(item =>
            {
                if (item.Tags == null) return false;
                var iLst = item.Tags.Intersect(tags);
                return containingAll ? iLst.Count() == tags.Length : iLst.Any();
            }).ToArray();
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGet(key, out var item) ? item : null;
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGet(key, out var item, i => (Core.Now - i.CreationDate) <= lastTime) ? item : null;
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGet(key, out var item, i => i.CreationDate >= comparer) ? item : null;
        }
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem[] GetByTag(string[] tags)
            => GetByTag(tags, false);
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem[] GetByTag(string[] tags, bool containingAll)
        {
            if (!Ready || tags == null) return null;
            if (tags.Length == 0) return EmptyItem;
            return Metas.AsParallel().Where(item =>
            {
                if (item.Tags == null) return false;
                var iLst = item.Tags.Intersect(tags).ToArray();
                return containingAll ? iLst.Length == tags.Length : iLst.Length > 0;
            }).Select(s => OnTryGet(s.Key, out var item) ? item : null).RemoveNulls().ToArray();
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
            if (!Ready || item == null) return false;
            if (OnSet(item.Meta, item.Data))
            {
                item.Meta.OnExpire -= OnItemExpire;
                item.Meta.OnExpire += OnItemExpire;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data)
            => InternalSet(key, data);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data, TimeSpan expirationDate)
            => InternalSet(key, data, Core.Now.Add(expirationDate));
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
            => InternalSet(key, data, expirationDate.HasValue ? Core.Now.Add(expirationDate.Value) : (DateTime?)null, tags);
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data, DateTime expirationDate)
            => InternalSet(key, data, expirationDate);
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
            => InternalSet(key, data, expirationDate, tags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool InternalSet(string key, SerializedObject data, DateTime? expirationDate = null, string[] tags = null)
        {
            if (!Ready || string.IsNullOrEmpty(key)) return false;
            var dateNow = Core.Now;
            if (expirationDate.HasValue && expirationDate.Value < dateNow) return false;
            var expDate = expirationDate;
            if (ItemsExpirationAbsoluteDateOverwrite.HasValue)
            {
                expDate = ItemsExpirationAbsoluteDateOverwrite.Value;
            }
            else if (ItemsExpirationDateOverwrite.HasValue)
            {
                expDate = dateNow.Add(ItemsExpirationDateOverwrite.Value);
            }
            if (MaximumItemDuration.HasValue)
            {
                if (expDate.HasValue)
                {
                    var expTime = expDate.Value - dateNow;
                    expDate = dateNow.Add(TimeSpan.FromMilliseconds(Math.Min(expTime.TotalMilliseconds, MaximumItemDuration.Value.TotalMilliseconds)));
                }
                else
                    expDate = dateNow.Add(MaximumItemDuration.Value);
            }

            Core.Log.LibVerbose("{0}-Set: {1}, with expiration: {2}", mName, key, expDate.HasValue ? expDate.Value.ToString() : "NO EXPIRE");

            var sMeta = new StorageItemMeta
            {
                CreationDate = dateNow,
                ExpirationDate = expDate,
                Key = key,
                Tags = tags?.Distinct().ToList()
            };
            if (OnSet(sMeta, data))
            {
                sMeta.OnExpire += OnItemExpire;
                return true;
            }
            return false;
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
            if (!Ready || string.IsNullOrEmpty(key)) return false;
            var meta = GetMeta(key);
            if (meta == null) return false;
            return OnSet(meta, data);
        }
        /// <summary>
        /// Removes a StorageItem with the Key specified.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <returns>true if the data could be removed; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(string key)
        {
            if (!Ready || string.IsNullOrEmpty(key)) return false;
            if (OnRemove(key, out var meta))
            {
                if (meta != null)
                    meta.OnExpire -= OnItemExpire;
                ItemRemoved?.Invoke(this, new ItemRemovedEventArgs(key));
                meta?.Dispose();
                return true;
            }
            return false;
        }
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] RemoveByTag(string[] tags)
            => RemoveByTag(tags, false);
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <param name="containingAll">true if the results items needs to have all tags, false if the items needs to have at least one of the tags.</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] RemoveByTag(string[] tags, bool containingAll)
        {
            if (!Ready) return null;
            var stoMetas = GetMetaByTag(tags, containingAll);
            if (stoMetas == null || stoMetas.Length == 0) return null;
            return stoMetas.Select(s =>
            {
                if (OnRemove(s.Key, out var meta))
                {
                    if (meta != null)
                        meta.OnExpire -= OnItemExpire;
                    ItemRemoved?.Invoke(this, new ItemRemovedEventArgs(s.Key));
                    meta?.Dispose();
                    return s.Key;
                }
                return null;
            }).RemoveNulls().ToArray();
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
            => InternalGetOrSet(key, data);
        /// <summary>
        /// Gets the StorageItem of a key, if the key doesn't exist then create one using the given values
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">Item data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>Storage item instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem GetOrSet(string key, SerializedObject data, TimeSpan expirationDate)
            => InternalGetOrSet(key, data, Core.Now.Add(expirationDate));
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
            => InternalGetOrSet(key, data, expirationDate.HasValue ? (DateTime?)Core.Now.Add(expirationDate.Value) : null, tags);
        /// <summary>
        /// Gets the StorageItem of a key, if the key doesn't exist then create one using the given values
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">Item data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>Storage item instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem GetOrSet(string key, SerializedObject data, DateTime expirationDate)
            => InternalGetOrSet(key, data, expirationDate);
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
            => InternalGetOrSet(key, data, expirationDate, tags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        StorageItem InternalGetOrSet(string key, SerializedObject data, DateTime? expirationDate = null, string[] tags = null)
        {
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            if (!OnTryGet(key, out var value))
            {
                Set(key, data, expirationDate, tags);
                return Get(key);
            }
            return value;
        }
        #endregion

        /// <summary>
        /// Gets the keys of all items stored in the Storage
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEnabled() => Enabled;
        /// <summary>
        /// Gets if the Storage is ready to be requested.
        /// </summary>
        /// <returns>true if the storage is ready; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsReady() => Ready;
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        /// <summary>
        /// Release all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (expirationTimer != null)
                    {
                        expirationTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        expirationTimer = null;
                    }
                }
				OnDispose();
                disposedValue = true;
            }
        }
        /// <summary>
        /// Release all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
