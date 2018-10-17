﻿/*
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
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using TWCore.Diagnostics.Status;
using TWCore.Serialization;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace TWCore.Cache
{
    /// <inheritdoc />
    /// <summary>
    /// Storage Base class
    /// </summary>
    public abstract class StorageBase : IStorage
    {
        private static readonly StorageItemMeta[] EmptyMeta = Array.Empty<StorageItemMeta>();
        private static readonly StorageItem[] EmptyItem = Array.Empty<StorageItem>();
        private readonly ManualResetEventSlim _readyEventSlim = new ManualResetEventSlim(false);
        private readonly string _name;
        private int _expirationCheckTimeInMinutes = 30;
        private bool _init;
        private Timer _expirationTimer;
        private volatile bool _expirationRunning;

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
        public int ExpirationCheckTimeInMinutes 
        { 
            get => _expirationCheckTimeInMinutes;
            set { _expirationCheckTimeInMinutes = value; SetExpirationTimeout(); } 
        }
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
		/// <inheritdoc />
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
            _name = GetType().Name;
            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(Enabled), Enabled, Enabled ? StatusItemValueStatus.Ok : StatusItemValueStatus.Error);
                collection.Add(nameof(Ready), Ready, Ready ? StatusItemValueStatus.Ok : StatusItemValueStatus.Error);
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
        private void SetExpirationTimeout()
        {
            _expirationTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            if (ExpirationCheckTimeInMinutes <= 0) return;
            var timeout = TimeSpan.FromMinutes(ExpirationCheckTimeInMinutes);
            _expirationTimer = new Timer(item => CheckItemExpiration(), this, timeout, timeout);
        }
        /// <summary>
        /// Checks items expiration
        /// </summary>
        /// <param name="initial">true if is the initial call of the method</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckItemExpiration(bool initial = false)
        {
            if (_expirationRunning) return;
            _expirationRunning = true;
            try
            {
                Core.Log.LibVerbose("Checking items expiration");
				var expiredItems = Metas.Where(m => m.IsExpired).ToArray();
                Core.Log.LibVerbose("Removing {0} expired items", expiredItems.Length);
                foreach (var item in expiredItems)
                {
                    var key = item.Key;
                    if (string.IsNullOrEmpty(key)) continue;
                    Core.Log.LibVerbose("Remove: {0}, on: {1}", key, _name);
                    if (!OnRemove(key, out var meta) || initial) continue;
                    if (meta != null)
                        meta.OnExpire = null;
                    ItemRemoved?.Invoke(this, new ItemRemovedEventArgs(key, meta?.Tags));
                    meta?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            _expirationRunning = false;
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
            meta.OnExpire = null;
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
        protected abstract IEnumerable<string> OnGetKeys();
		/// <summary>
		/// Init this storage
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected abstract void OnInit();
        #endregion

        #region Public Methods
        /// <inheritdoc />
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
        /// <inheritdoc />
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
        /// <inheritdoc />
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <returns>Dictionary true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, bool> ExistKey(string[] keys)
        {
            if (!Ready || keys is null) return null;
            var dictionary = new Dictionary<string, bool>();
            foreach (var key in keys)
                dictionary[key] = OnExistKey(key);
            return dictionary;
        }
        /// <inheritdoc />
        /// <summary>
        /// Get all storage keys.
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] GetKeys()
        {
            if (!Ready)
                return null;
            var keys = OnGetKeys();
            return keys as string[] ?? keys.ToArray();
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGetMeta(key, out var meta) ? meta.CreationDate : (DateTime?)null;
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGetMeta(key, out var meta) ? meta.IsExpired ? null : meta.ExpirationDate : null;
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGetMeta(key, out var meta) ? meta : null;
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGetMeta(key, out var meta, i => (Core.Now - i.CreationDate) <= lastTime) ? meta : null;
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGetMeta(key, out var meta, i => i.CreationDate >= comparer) ? meta : null;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item metadata array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta[] GetMetaByTag(string[] tags)
            => GetMetaByTag(tags, false);
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
            if (!Ready || tags is null) return null;
            if (tags.Length == 0) return EmptyMeta;
            return Metas.AsParallel().Where(item =>
            {
                if (item.Tags is null) return false;
                var iLst = item.Tags.Intersect(tags);
                return containingAll ? iLst.Count() == tags.Length : iLst.Any();
            }).ToArray();
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGet(key, out var item) ? item : null;
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGet(key, out var item, i => (Core.Now - i.CreationDate) <= lastTime) ? item : null;
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
            if (!Ready || string.IsNullOrEmpty(key)) return null;
            return OnTryGet(key, out var item, i => i.CreationDate >= comparer) ? item : null;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem searching the items with the tags 
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>Storage item array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem[] GetByTag(string[] tags)
            => GetByTag(tags, false);
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
            if (!Ready || tags is null) return null;
            if (tags.Length == 0) return EmptyItem;
            return Metas.AsParallel().Where(item =>
            {
                if (item.Tags is null) return false;
                var iLst = item.Tags.Intersect(tags).ToArray();
                return containingAll ? iLst.Length == tags.Length : iLst.Length > 0;
            }).Select(s => OnTryGet(s.Key, out var item) ? item : null).RemoveNulls().ToArray();
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <returns>Storage items Dictionary</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, StorageItem> Get(string[] keys)
        {
            if (!Ready || keys is null) return null;
            var dictionary = new Dictionary<string, StorageItem>();
            foreach (var key in keys)
                dictionary[key] = OnTryGet(key, out var item) ? item : null;
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
        public Dictionary<string, StorageItem> Get(string[] keys, TimeSpan lastTime)
        {
            if (!Ready || keys is null) return null;
            var dictionary = new Dictionary<string, StorageItem>();
            foreach (var key in keys)
                dictionary[key] = OnTryGet(key, out var item, i => (Core.Now - i.CreationDate) <= lastTime) ? item : null;
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
        public Dictionary<string, StorageItem> Get(string[] keys, DateTime comparer)
        {
            if (!Ready || keys is null) return null;
            var dictionary = new Dictionary<string, StorageItem>();
            foreach (var key in keys)
                dictionary[key] = OnTryGet(key, out var item, i => i.CreationDate >= comparer) ? item : null;
            return dictionary;
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
            if (!Ready || item is null) return false;
            item.Meta.OnExpire = null;
            if (!OnSet(item.Meta, item.Data)) return false;
            item.Meta.OnExpire = OnItemExpire;
            return true;
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="meta">Item Meta</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(StorageItemMeta meta, SerializedObject data)
        {
            if (!Ready || meta is null) return false;
            meta.OnExpire = null;
            if (!OnSet(meta, data)) return false;
            meta.OnExpire = OnItemExpire;
            return true;
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
            => InternalSet(key, data);
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
            => InternalSet(key, data, Core.Now.Add(expirationDate));
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
            => InternalSet(key, data, expirationDate.HasValue ? Core.Now.Add(expirationDate.Value) : (DateTime?)null, tags);
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
            => InternalSet(key, data, expirationDate);
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
            => InternalSet(key, data, expirationDate, tags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool InternalSet(string key, SerializedObject data, DateTime? expirationDate = null, string[] tags = null)
        {
            if (!Ready || string.IsNullOrEmpty(key)) return false;
            var dateNow = Core.Now;
            var expDate = expirationDate;
            if (ItemsExpirationAbsoluteDateOverwrite.HasValue)
                expDate = ItemsExpirationAbsoluteDateOverwrite.Value;
            else if (ItemsExpirationDateOverwrite.HasValue)
                expDate = dateNow.Add(ItemsExpirationDateOverwrite.Value);
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

            Core.Log.LibVerbose("{0}-Set: {1}, with expiration: {2}", _name, key, expDate.HasValue ? expDate.Value.ToString(CultureInfo.CurrentCulture) : "NO EXPIRE");

            var sMeta = StorageItemMeta.Create(key, expDate, tags);
            if (!OnSet(sMeta, data)) return false;
            sMeta.OnExpire = OnItemExpire;
            return true;
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
        public bool SetMulti(StorageItem[] items)
        {
            if (!Ready || items is null) return false;
            foreach (var item in items)
                Set(item);
            return true;
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetMulti(string[] keys, SerializedObject data)
        {
            if (!Ready || keys is null) return false;
            foreach (var key in keys)
                Set(key, data);
            return true;
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
        public bool SetMulti(string[] keys, SerializedObject data, TimeSpan expirationDate)
        {
            if (!Ready || keys is null) return false;
            foreach (var key in keys)
                Set(key, data, expirationDate);
            return true;
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
        public bool SetMulti(string[] keys, SerializedObject data, TimeSpan? expirationDate, string[] tags)
        {
            if (!Ready || keys is null) return false;
            foreach (var key in keys)
                Set(key, data, expirationDate, tags);
            return true;
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
        public bool SetMulti(string[] keys, SerializedObject data, DateTime expirationDate)
        {
            if (!Ready || keys is null) return false;
            foreach (var key in keys)
                Set(key, data, expirationDate);
            return true;
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
        public bool SetMulti(string[] keys, SerializedObject data, DateTime? expirationDate, string[] tags)
        {
            if (!Ready || keys is null) return false;
            foreach (var key in keys)
                Set(key, data, expirationDate, tags);
            return true;
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
        public bool UpdateData(string key, SerializedObject data)
        {
            if (!Ready || string.IsNullOrEmpty(key)) return false;
            var meta = GetMeta(key);
            return meta != null && OnSet(meta, data);
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
            if (!Ready || string.IsNullOrEmpty(key)) return false;
            if (!OnRemove(key, out var meta)) return false;
            if (meta != null)
                meta.OnExpire = null;
            ItemRemoved?.Invoke(this, new ItemRemovedEventArgs(key, meta?.Tags));
            meta?.Dispose();
            return true;
        }
        /// <inheritdoc />
        /// <summary>
        /// Removes a series of StorageItems with the given tags.
        /// </summary>
        /// <param name="tags">Tags array to look on the storage items</param>
        /// <returns>String array with the keys of the items removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] RemoveByTag(string[] tags)
            => RemoveByTag(tags, false);
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
            if (!Ready) return null;
            var stoMetas = GetMetaByTag(tags, containingAll);
            if (stoMetas is null || stoMetas.Length == 0) return null;
            return stoMetas.Select(s =>
            {
                if (!OnRemove(s.Key, out var meta)) return null;
                if (meta != null)
                    meta.OnExpire = null;
                ItemRemoved?.Invoke(this, new ItemRemovedEventArgs(s.Key, meta?.Tags));
                meta?.Dispose();
                return s.Key;
            }).RemoveNulls().ToArray();
        }
        /// <inheritdoc />
        /// <summary>
        /// Copies an item to a new key.
        /// </summary>
        /// <param name="key">Key of an existing item</param>
        /// <param name="newKey">New key value</param>
        /// <returns>true if the copy was successful; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Copy(string key, string newKey)
        {
            if (!Ready || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(newKey)) return false;
            if (!OnTryGet(key, out var value)) return false;
            var sItem = new StorageItem(new StorageItemMeta
            {
                CreationDate = value.Meta.CreationDate,
                ExpirationDate = value.Meta.ExpirationDate,
                Key = newKey,
                Tags = value.Meta.Tags
            }, value.Data);
            return Set(sItem);
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Gets the keys of all items stored in the Storage
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEnabled() => Enabled;
        /// <inheritdoc />
        /// <summary>
        /// Gets if the Storage is ready to be requested.
        /// </summary>
        /// <returns>true if the storage is ready; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsReady() => Ready;
        #endregion

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls
        /// <summary>
        /// Release all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                if (_expirationTimer != null)
                {
                    _expirationTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _expirationTimer = null;
                }
            }
            OnDispose();
            _disposedValue = true;
        }
        /// <inheritdoc />
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
