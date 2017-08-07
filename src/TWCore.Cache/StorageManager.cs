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

namespace TWCore.Cache
{
    /// <summary>
    /// Storage Manager
    /// </summary>
    public class StorageManager : IStorage, IDisposable
    {
        readonly Stack<IStorage> StorageStack = new Stack<IStorage>();
        IStorage[] storages = null;

		/// <summary>
		/// Gets the Storage Type
		/// </summary>
		/// <value>The type.</value>
		public StorageType Type => StorageType.Unknown;

        #region .ctor
        /// <summary>
        /// Storage Manager
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageManager()
        {
            BindStatusHandlers();
        }
        /// <summary>
        /// Storage Manager
        /// </summary>
        /// <param name="storages">Storage collection</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageManager(IEnumerable<IStorage> storages)
        {
			StorageStack = new Stack<IStorage>(storages.RemoveNulls());
            storages = StorageStack.ToArray();
            BindStatusHandlers();
        }
        /// <summary>
        /// Storage destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~StorageManager()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void BindStatusHandlers()
        {
            Core.Status.Attach(collection =>
            {
                collection.AddGood("Stack Count", StorageStack.Count);
                collection.AddGood("Stack", StorageStack.Select(s => s.ToString()).Join(", "));
                StorageStack.Each(s => Core.Status.AttachChild(s, this));
            });
        }
		#endregion

		#region Private Methods
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		R ReturnFromStack<R, A1>(A1 arg1, Func<IStorage, A1, R> functionValueToLook, Action<IStorage, A1, R> actionOnPreviousStorages = null, Func<R, bool> breakCondition = null, R defaultValue = default(R))
		{
			ReferencePool<Stack<IStorage>> refPool = null;
			Stack<IStorage> noDataStack = null;
			bool actionToPrevious = actionOnPreviousStorages != null;
			if (actionToPrevious) 
			{
				refPool = ReferencePool<Stack<IStorage>>.Shared;
				noDataStack = refPool.New();
			}
			var response = defaultValue;
			bool found = false;
			foreach (var storage in storages)
			{
				if (!storage.IsEnabled() || !storage.IsReady()) continue;
				var functionResponse = functionValueToLook(storage, arg1);
				var bCondition = breakCondition?.Invoke(functionResponse) ?? !EqualityComparer<R>.Default.Equals(functionResponse, defaultValue);
				if (bCondition)
				{
					response = functionResponse;
					found = true;
					break;
				}
				if (actionToPrevious) 
					noDataStack.Push(storage);
			}
			if (actionToPrevious)
			{
				if (found)
				{
					while (noDataStack.Count > 0) 
					{
						try 
						{
							actionOnPreviousStorages(noDataStack.Pop(), arg1, response);
						}
						catch(Exception ex) 
						{
							Core.Log.Write(ex);
						}
					}
				}
				else
					noDataStack.Clear();
				refPool.Store(noDataStack);
			}
			return response;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		R ReturnFromStack<R, A1, A2>(A1 arg1, A2 arg2, Func<IStorage, A1, A2, R> functionValueToLook, Action<IStorage, A1, A2, R> actionOnPreviousStorages = null, Func<R, bool> breakCondition = null, R defaultValue = default(R))
		{
			ReferencePool<Stack<IStorage>> refPool = null;
			Stack<IStorage> noDataStack = null;
			bool actionToPrevious = actionOnPreviousStorages != null;
			if (actionToPrevious)
			{
				refPool = ReferencePool<Stack<IStorage>>.Shared;
				noDataStack = refPool.New();
			}
			var response = defaultValue;
			bool found = false;
			foreach (var storage in storages)
			{
				if (!storage.IsEnabled() || !storage.IsReady()) continue;
				var functionResponse = functionValueToLook(storage, arg1, arg2);
				var bCondition = breakCondition?.Invoke(functionResponse) ?? !EqualityComparer<R>.Default.Equals(functionResponse, defaultValue);
				if (bCondition)
				{
					response = functionResponse;
					found = true;
					break;
				}
				if (actionToPrevious)
					noDataStack.Push(storage);
			}
			if (actionToPrevious)
			{
				if (found)
				{
					while (noDataStack.Count > 0) 
					{
						try 
						{
							actionOnPreviousStorages(noDataStack.Pop(), arg1, arg2, response);
						}
						catch(Exception ex) 
						{
							Core.Log.Write(ex);
						}
					}
				}
				else
					noDataStack.Clear();
				refPool.Store(noDataStack);
			}
			return response;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool ExecuteInAllStack<A1>(A1 arg1, Action<IStorage, A1> actionPushData)
		{
			bool ret = true;
			foreach (var storage in storages)
			{
				if (!storage.IsEnabled() || !storage.IsReady()) continue;
				try
				{
					actionPushData(storage, arg1);
				}
				catch (Exception ex)
				{
					ret = false;
					Core.Log.Write(ex);
				}
			}
			return ret;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool ExecuteInAllStack<A1, A2>(A1 arg1, A2 arg2, Action<IStorage, A1, A2> actionPushData)
		{
			bool ret = true;
			foreach (var storage in storages)
			{
				if (!storage.IsEnabled() || !storage.IsReady()) continue;
				try
				{
					actionPushData(storage, arg1, arg2);
				}
				catch (Exception ex)
				{
					ret = false;
					Core.Log.Write(ex);
				}
			}
			return ret;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool ExecuteInAllStack<A1, A2, A3>(A1 arg1, A2 arg2, A3 arg3, Action<IStorage, A1, A2, A3> actionPushData)
		{
			bool ret = true;
			foreach (var storage in storages)
			{
				if (!storage.IsEnabled() || !storage.IsReady()) continue;
				try
				{
					actionPushData(storage, arg1, arg2, arg3);
				}
				catch (Exception ex)
				{
					ret = false;
					Core.Log.Write(ex);
				}
			}
			return ret;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool ExecuteInAllStack<A1, A2, A3, A4>(A1 arg1, A2 arg2, A3 arg3, A4 arg4, Action<IStorage, A1, A2, A3, A4> actionPushData)
		{
			bool ret = true;
			foreach (var storage in storages)
			{
				if (!storage.IsEnabled() || !storage.IsReady()) continue;
				try
				{
					actionPushData(storage, arg1, arg2, arg3, arg4);
				}
				catch (Exception ex)
				{
					ret = false;
					Core.Log.Write(ex);
				}
			}
			return ret;
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        List<R> ExecuteInAllStackAndReturn<R>(Func<IStorage, R> functionPushData)
        {
            var responses = new List<R>();
            foreach (var storage in storages)
            {
                if (!storage.IsEnabled() || !storage.IsReady()) continue;
                R result = default(R);
                try
                {
                    result = functionPushData(storage);
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
                responses.Add(result);
            }
            return responses;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		List<R> ExecuteInAllStackAndReturn<R, A1>(A1 arg1, Func<IStorage, A1, R> functionPushData)
		{
			var responses = new List<R>();
			foreach (var storage in storages)
			{
				if (!storage.IsEnabled() || !storage.IsReady()) continue;
				R result = default(R);
				try
				{
					result = functionPushData(storage, arg1);
				}
				catch (Exception ex)
				{
					Core.Log.Write(ex);
				}
				responses.Add(result);
			}
			return responses;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		List<R> ExecuteInAllStackAndReturn<R, A1, A2>(A1 arg1, A2 arg2, Func<IStorage, A1, A2, R> functionPushData)
		{
			var responses = new List<R>();
			foreach (var storage in storages)
			{
				if (!storage.IsEnabled() || !storage.IsReady()) continue;
				R result = default(R);
				try
				{
					result = functionPushData(storage, arg1, arg2);
				}
				catch (Exception ex)
				{
					Core.Log.Write(ex);
				}
				responses.Add(result);
			}
			return responses;
		}
        #endregion

        #region Stack Methods
        /// <summary>
        /// Removes and returns the object at the top of the Stack
        /// </summary>
        /// <returns>The object removed from the top of the stack</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IStorage Pop()
        {
            var pop = StorageStack.Pop();
            storages = StorageStack.ToArray();
            return pop;
        }
        /// <summary>
        /// Inserts an object at the top of the Stack
        /// </summary>
        /// <param name="item">The object to push onto the stack</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(IStorage item)
        {
			if (item == null) return;
            StorageStack.Push(item);
            storages = StorageStack.ToArray();
        }
        #endregion

        #region IStorage
        /// <summary>
		/// Init this storage
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init() => storages?.Each(s => s.Init());

        #region Exist Key / Get Keys
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExistKey(string key)
            => ReturnFromStack(key, (sto, arg1) => sto.ExistKey(arg1));
        /// <summary>
        /// Gets the keys of all items stored in the Storage
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string[] GetKeys() 
		{
			var keys = new HashSet<string>(StringComparer.Ordinal);
			foreach (var storage in storages)
			{
				if (!storage.IsEnabled() || !storage.IsReady()) continue;
				try
				{
					var keyArray = storage.GetKeys();
					foreach(var key in keyArray)
						keys.Add(key);
				}
				catch (Exception ex)
				{
					Core.Log.Write(ex);
				}
			}
			return keys.ToArray();
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
            => ReturnFromStack(key, (sto, arg1) => sto.GetCreationDate(arg1));
        /// <summary>
        /// Gets the expiration date for a storage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the expiration date of the storage item, null if the item hasn't expiration date or the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime? GetExpirationDate(string key)
            => ReturnFromStack(key, (sto, arg1) => sto.GetExpirationDate(arg1));
        #endregion

        #region Get MetaData
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta GetMeta(string key)
            => ReturnFromStack(key, (sto, arg1) => sto.GetMeta(arg1));
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta GetMeta(string key, TimeSpan lastTime)
            => ReturnFromStack(key, lastTime, (sto, arg1, arg2) => sto.GetMeta(arg1, arg2));
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta GetMeta(string key, DateTime comparer)
			=> ReturnFromStack(key, comparer, (sto, arg1, arg2) => sto.GetMeta(arg1, arg2));
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
            var res = ExecuteInAllStackAndReturn(tags, containingAll, (sto, arg1, arg2) => sto.GetMetaByTag(arg1, arg2))
                .SelectMany(a => a)
                .DistinctBy(i => i.Key)
                .ToArray();
            return res;
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
            => ReturnFromStack(key, (sto, arg1) => sto.Get(arg1), (pSto, arg1, stoVal) => pSto.Set(stoVal));
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem Get(string key, TimeSpan lastTime)
            => ReturnFromStack(key, lastTime, (sto, arg1, arg2) => sto.Get(arg1, arg2), (pSto, arg1, arg2, stoVal) => pSto.Set(stoVal));
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem Get(string key, DateTime comparer)
            => ReturnFromStack(key, comparer, (sto, arg1, arg2) => sto.Get(arg1, arg2), (pSto, arg1, arg2, stoVal) => pSto.Set(stoVal));
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
            var res = ExecuteInAllStackAndReturn(tags, containingAll, (sto, arg1, arg2) => sto.GetByTag(arg1, arg2))
                .SelectMany(a => a)
                .DistinctBy(i => i.Meta.Key)
                .ToArray();
            return res;
        }
		#endregion

		#region Set Data
		/// <summary>
		/// Sets a new StorageItem with the given data
		/// </summary>
		/// <param name="item">Item</param>
		/// <returns>true if the data could be save; otherwise, false.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Set(StorageItem item)
			=> ExecuteInAllStack(item, (sto, arg1) => sto.Set(arg1));
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data)
            => ExecuteInAllStack(key, data, (sto, arg1, arg2) => sto.Set(arg1, arg2));
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data, TimeSpan expirationDate)
            => ExecuteInAllStack(key, data, expirationDate, (sto, arg1, arg2, arg3) => sto.Set(arg1, arg2, arg3));
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
            => ExecuteInAllStack(key, data, expirationDate, tags, (sto, arg1, arg2, arg3, arg4) => sto.Set(arg1, arg2, arg3, arg4));
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <param name="expirationDate">Item expiration date</param>
        /// <param name="tags">Items meta tags</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data, DateTime expirationDate)
			=> ExecuteInAllStack(key, data, expirationDate, (sto, arg1, arg2, arg3) => sto.Set(arg1, arg2, arg3));
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
			=> ExecuteInAllStack(key, data, expirationDate, tags, (sto, arg1, arg2, arg3, arg4) => sto.Set(arg1, arg2, arg3, arg4));
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
            => ExecuteInAllStack(key, data, (sto, arg1, arg2) => sto.UpdateData(arg1, arg2));
        /// <summary>
        /// Removes a StorageItem with the Key specified.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <returns>true if the data could be removed; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(string key)
            => ExecuteInAllStack(key, (sto, arg1) => sto.Remove(arg1));
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
			var res = ExecuteInAllStackAndReturn(tags, containingAll, (sto, arg1, arg2) => sto.RemoveByTag(arg1, arg2))
				.SelectMany(a => a)
				.Distinct()
				.ToArray();
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
        public StorageItem GetOrSet(string key, SerializedObject data)
        {
            var stoData = Get(key);
            if (stoData == null)
            {
                Set(key, data);
                stoData = Get(key);
            }
            return stoData;
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
            var stoData = Get(key);
            if (stoData == null)
            {
                Set(key, data, expirationDate);
                stoData = Get(key);
            }
            return stoData;
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
            var stoData = Get(key);
            if (stoData == null)
            {
                Set(key, data, expirationDate, tags);
                stoData = Get(key);
            }
            return stoData;
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
            var stoData = Get(key);
            if (stoData == null)
            {
                Set(key, data, expirationDate);
                stoData = Get(key);
            }
            return stoData;
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
            var stoData = Get(key);
            if (stoData == null)
            {
                Set(key, data, expirationDate, tags);
                stoData = Get(key);
            }
            return stoData;
        }
        #endregion

        /// <summary>
        /// Gets if the Storage is enabled.
        /// </summary>
        /// <returns>true if the storage is enabled; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEnabled()
        {
            foreach(var sto in storages)
            {
                if (sto.IsEnabled())
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Gets if the Storage is ready to be requested.
        /// </summary>
        /// <returns>true if the storage is ready; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsReady()
        {
            foreach (var sto in storages)
            {
                if (!sto.IsReady())
                    return false;
            }
            return true;
        }
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
                    StorageStack.Each(s =>
                    {
                        var sM = s as StorageBase;
                        sM?.Dispose();
                    });
                    StorageStack.Clear();
                }
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
