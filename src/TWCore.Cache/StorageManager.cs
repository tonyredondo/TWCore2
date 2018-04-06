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
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Serialization;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace TWCore.Cache
{
    /// <inheritdoc />
    /// <summary>
    /// Storage Manager
    /// </summary>
    public class StorageManager : IStorage
    {
	    private readonly Stack<IStorage> _storageStack = new Stack<IStorage>();
	    private IStorage[] _storages;
        private delegate TR RefFunc<TArg, TR>(ref TArg arg1);
        private delegate TR RefFunc<TArg, TArg2, TR>(ref TArg arg1, ref TArg2 arg2);
        private delegate TR RefFunc<TArg, TArg2, TArg3, TR>(ref TArg arg1, ref TArg2 arg2, ref TArg3 arg3);
        private delegate void RefAction<TArg, TArg2>(ref TArg arg1, ref TArg2 arg2);
        private delegate void RefAction<TArg, TArg2, TArg3>(ref TArg arg1, ref TArg2 arg2, ref TArg3 arg3);
        private delegate void RefAction<TArg, TArg2, TArg3, TArg4>(ref TArg arg1, ref TArg2 arg2, ref TArg3 arg3, ref TArg4 arg4);
        private delegate void RefAction<TArg, TArg2, TArg3, TArg4, TArg5>(ref TArg arg1, ref TArg2 arg2, ref TArg3 arg3, ref TArg4 arg4, ref TArg5 arg5);

        /// <inheritdoc />
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
			_storageStack = new Stack<IStorage>(storages.RemoveNulls());
            _storages = _storageStack.ToArray();
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
        private void BindStatusHandlers()
        {
            Core.Status.Attach(collection =>
            {
                collection.AddOk("Stack Count", _storageStack.Count);
                collection.AddOk("Stack", _storageStack.Select(s => s.ToString()).Join(", "));
                _storageStack.Each(s => Core.Status.AttachChild(s, this));
            });
        }
		#endregion

		#region Private Methods
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TR ReturnFromStack<TR, TA1>(ref TA1 arg1, RefFunc<IStorage, TA1, TR> functionValueToLook, RefAction<IStorage, TA1, TR> actionOnPreviousStorages = null, RefFunc<TR, bool> breakCondition = null, in TR defaultValue = default)
		{
			ReferencePool<Stack<IStorage>> refPool = null;
			Stack<IStorage> noDataStack = null;
			var actionToPrevious = actionOnPreviousStorages != null;
			if (actionToPrevious) 
			{
				refPool = ReferencePool<Stack<IStorage>>.Shared;
				noDataStack = refPool.New();
			}
			var response = defaultValue;
			var found = false;
            for(var i = 0; i < _storages.Length; i++)
			{
                var storage = _storages[i];
				if (!storage.IsEnabled() || !storage.IsReady()) continue;
				var functionResponse = functionValueToLook(ref storage, ref arg1);
				var bCondition = breakCondition?.Invoke(ref functionResponse) ?? !EqualityComparer<TR>.Default.Equals(functionResponse, defaultValue);
				if (bCondition)
				{
					response = functionResponse;
					found = true;
					break;
				}
				if (actionToPrevious) 
					noDataStack.Push(storage);
			}
			if (!actionToPrevious) return response;
			if (found)
			{
				while (noDataStack.Count > 0) 
				{
					try 
					{
                        var nds = noDataStack.Pop();
                        actionOnPreviousStorages(ref nds, ref arg1, ref response);
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
			return response;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TR ReturnFromStack<TR, TA1, TA2>(ref TA1 arg1, ref TA2 arg2, RefFunc<IStorage, TA1, TA2, TR> functionValueToLook, RefAction<IStorage, TA1, TA2, TR> actionOnPreviousStorages = null, RefFunc<TR, bool> breakCondition = null, in TR defaultValue = default)
		{
			ReferencePool<Stack<IStorage>> refPool = null;
			Stack<IStorage> noDataStack = null;
			var actionToPrevious = actionOnPreviousStorages != null;
			if (actionToPrevious)
			{
				refPool = ReferencePool<Stack<IStorage>>.Shared;
				noDataStack = refPool.New();
			}
			var response = defaultValue;
			var found = false;
            for (var i = 0; i < _storages.Length; i++)
            {
                var storage = _storages[i];
                if (!storage.IsEnabled() || !storage.IsReady()) continue;
				var functionResponse = functionValueToLook(ref storage, ref arg1, ref arg2);
				var bCondition = breakCondition?.Invoke(ref functionResponse) ?? !EqualityComparer<TR>.Default.Equals(functionResponse, defaultValue);
				if (bCondition)
				{
					response = functionResponse;
					found = true;
					break;
				}
				if (actionToPrevious)
					noDataStack.Push(storage);
			}
			if (!actionToPrevious) return response;
			if (found)
			{
				while (noDataStack.Count > 0) 
				{
					try 
					{
                        var nds = noDataStack.Pop();
                        actionOnPreviousStorages(ref nds, ref arg1, ref arg2, ref response);
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
			return response;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool ExecuteInAllStack<TA1>(ref TA1 arg1, RefAction<IStorage, TA1> actionPushData)
		{
			var ret = true;
            for (var i = 0; i < _storages.Length; i++)
			{
                var storage = _storages[i];
				if (!storage.IsEnabled() || !storage.IsReady()) continue;
				try
				{
					actionPushData(ref storage, ref arg1);
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
		private bool ExecuteInAllStack<TA1, TA2>(ref TA1 arg1, ref TA2 arg2, RefAction<IStorage, TA1, TA2> actionPushData)
		{
			var ret = true;
            for (var i = 0; i < _storages.Length; i++)
            {
                var storage = _storages[i];
                if (!storage.IsEnabled() || !storage.IsReady()) continue;
				try
				{
					actionPushData(ref storage, ref arg1, ref arg2);
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
		private bool ExecuteInAllStack<TA1, TA2, TA3>(ref TA1 arg1, ref TA2 arg2, ref TA3 arg3, RefAction<IStorage, TA1, TA2, TA3> actionPushData)
		{
			var ret = true;
            for (var i = 0; i < _storages.Length; i++)
			{
                var storage = _storages[i];
                if (!storage.IsEnabled() || !storage.IsReady()) continue;
				try
				{
					actionPushData(ref storage, ref arg1, ref arg2, ref arg3);
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
		private bool ExecuteInAllStack<TA1, TA2, TA3, TA4>(ref TA1 arg1, ref TA2 arg2, ref TA3 arg3, ref TA4 arg4, RefAction<IStorage, TA1, TA2, TA3, TA4> actionPushData)
		{
			var ret = true;
            for (var i = 0; i < _storages.Length; i++)
			{
                var storage = _storages[i];
                if (!storage.IsEnabled() || !storage.IsReady()) continue;
				try
				{
					actionPushData(ref storage, ref arg1, ref arg2, ref arg3, ref arg4);
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
        private List<TR> ExecuteInAllStackAndReturn<TR, TA1>(TA1 arg1, Func<IStorage, TA1, TR> functionPushData)
        {
            var responses = _storages
                .Where(sto => sto.IsEnabled() && sto.IsReady())
                .AsParallel()
                .Select(sto =>
                {
                    try
                    {
                        return functionPushData(sto, arg1);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                        return default;
                    }
                })
                .ToList();
            return responses;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		private List<TR> ExecuteInAllStackAndReturn<TR, TA1, TA2>(TA1 arg1, TA2 arg2, Func<IStorage, TA1, TA2, TR> functionPushData)
		{
		    var responses = _storages
		        .Where(sto => sto.IsEnabled() && sto.IsReady())
		        .AsParallel()
		        .Select(sto => 
		        {
		            try
		            {
		                return functionPushData(sto, arg1, arg2);
		            }
		            catch (Exception ex)
		            {
		                Core.Log.Write(ex);
		                return default;
		            }
                })
		        .ToList();
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
            var pop = _storageStack.Pop();
            _storages = _storageStack.ToArray();
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
            _storageStack.Push(item);
            _storages = _storageStack.ToArray();
        }
        #endregion

        #region IStorage
        /// <inheritdoc />
        /// <summary>
        /// Init this storage
        /// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init() => _storages?.Each(s => s.Init());

        #region Exist Key / Get Keys
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ExistLook(ref IStorage sto, ref string key) => sto.ExistKey(key);

        /// <inheritdoc />
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExistKey(string key)
            => ReturnFromStack(ref key, ExistLook);
        /// <inheritdoc />
        /// <summary>
        /// Checks if a key exist on the storage.
        /// </summary>
        /// <param name="keys">Keys to look on the storage</param>
        /// <returns>Dictionary true if the key exist on the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, bool> ExistKey(string[] keys)
        {
            if (keys == null) return null;
            var dictionary = new Dictionary<string, bool>();
            for (var i = 0; i < keys.Length; i++)
                    dictionary[keys[i]] = ReturnFromStack(ref keys[i], ExistLook);
            return dictionary;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the keys of all items stored in the Storage
        /// </summary>
        /// <returns>String array with the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string[] GetKeys() 
		{
			var keys = new HashSet<string>(StringComparer.Ordinal);
			foreach (var storage in _storages)
			{
				if (!storage.IsEnabled() || !storage.IsReady()) continue;
				try
				{
                    keys.UnionWith(storage.GetKeys());
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DateTime? GetCreationDateLook(ref IStorage sto, ref string key) => sto.GetCreationDate(key);

        /// <inheritdoc />
        /// <summary>
        /// Gets the creation date for astorage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the creation date of the storage item, null if the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime? GetCreationDate(string key)
            => ReturnFromStack(ref key, GetCreationDateLook);
        /// <inheritdoc />
        /// <summary>
        /// Gets the expiration date for a storage item with the key specified.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>DateTime with the expiration date of the storage item, null if the item hasn't expiration date or the key wasn't found in the storage</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime? GetExpirationDate(string key)
            => ReturnFromStack(ref key, GetCreationDateLook);
        #endregion

        #region Get MetaData
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static StorageItemMeta GetMetaLook(ref IStorage sto, ref string key) => sto.GetMeta(key);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static StorageItemMeta GetMetaLook(ref IStorage sto, ref string key, ref TimeSpan lastTime) => sto.GetMeta(key, lastTime);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static StorageItemMeta GetMetaLook(ref IStorage sto, ref string key, ref DateTime comparer) => sto.GetMeta(key, comparer);

        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta GetMeta(string key)
            => ReturnFromStack(ref key, GetMetaLook);
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta GetMeta(string key, TimeSpan lastTime)
            => ReturnFromStack(ref key, ref lastTime, GetMetaLook);
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItemMeta information of a key in the storage.
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item metadata instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItemMeta GetMeta(string key, DateTime comparer)
			=> ReturnFromStack(ref key, ref comparer, GetMetaLook);
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
            var res = ExecuteInAllStackAndReturn(tags, containingAll, (sto, arg1, arg2) => sto.GetMetaByTag(arg1, arg2))
                .SelectMany(a => a)
                .DistinctBy(i => i.Key)
                .ToArray();
            return res;
        }
        #endregion

        #region Get Data
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static StorageItem GetLook(ref IStorage sto, ref string key) => sto.Get(key);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static StorageItem GetLook(ref IStorage sto, ref string key, ref TimeSpan lastTime) => sto.Get(key, lastTime);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static StorageItem GetLook(ref IStorage sto, ref string key, ref DateTime comparer) => sto.Get(key, comparer);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetSetAction(ref IStorage sto, ref string key, ref StorageItem item) => sto.Set(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetSetAction(ref IStorage sto, ref string key, ref TimeSpan lastTime, ref StorageItem item) => sto.Set(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetSetAction(ref IStorage sto, ref string key, ref DateTime comparer, ref StorageItem item) => sto.Set(item);

        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem Get(string key)
            => ReturnFromStack(ref key, GetLook, GetSetAction);
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="lastTime">Defines a time period before DateTime.Now to look for the data</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem Get(string key, TimeSpan lastTime)
            => ReturnFromStack(ref key, ref lastTime, GetLook, GetSetAction);
        /// <inheritdoc />
        /// <summary>
        /// Gets the StorageItem of a key in the storage
        /// </summary>
        /// <param name="key">Key to look on the storage</param>
        /// <param name="comparer">Defines a time to compare the storage item</param>
        /// <returns>Storage item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StorageItem Get(string key, DateTime comparer)
            => ReturnFromStack(ref key, ref comparer, GetLook, GetSetAction);
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
            var res = ExecuteInAllStackAndReturn(tags, containingAll, (sto, arg1, arg2) => sto.GetByTag(arg1, arg2))
                .SelectMany(a => a)
                .DistinctBy(i => i.Meta.Key)
                .ToArray();
            return res;
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
            if (keys == null) return null;
            var dictionary = new Dictionary<string, StorageItem>();
            for (var i = 0; i < keys.Length; i++)
                dictionary[keys[i]] = ReturnFromStack(ref keys[i], GetLook, GetSetAction);
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
            if (keys == null) return null;
            var dictionary = new Dictionary<string, StorageItem>();
            for (var i = 0; i < keys.Length; i++)
                    dictionary[keys[i]] = ReturnFromStack(ref keys[i], ref lastTime, GetLook, GetSetAction);
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
            if (keys == null) return null;
            var dictionary = new Dictionary<string, StorageItem>();
            for (var i = 0; i < keys.Length; i++)
                dictionary[keys[i]] = ReturnFromStack(ref keys[i], ref comparer, GetLook, GetSetAction);
            return dictionary;
        }
        #endregion

        #region Set Data
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetAction(ref IStorage sto, ref StorageItem item) => sto.Set(item);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetAction(ref IStorage sto, ref string key, ref SerializedObject data) => sto.Set(key, data);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetAction(ref IStorage sto, ref string key, ref SerializedObject data, ref TimeSpan expirationDate) => sto.Set(key, data, expirationDate);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetAction(ref IStorage sto, ref string key, ref SerializedObject data, ref TimeSpan? expirationDate, ref string[] tags) => sto.Set(key, data, expirationDate, tags);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetAction(ref IStorage sto, ref string key, ref SerializedObject data, ref DateTime expirationDate) => sto.Set(key, data, expirationDate);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetAction(ref IStorage sto, ref string key, ref SerializedObject data, ref DateTime? expirationDate, ref string[] tags) => sto.Set(key, data, expirationDate, tags);

        /// <inheritdoc />
        /// <summary>
        /// Sets a new StorageItem with the given data
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Set(StorageItem item)
			=> ExecuteInAllStack(ref item, SetAction);
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="key">Item Key</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(string key, SerializedObject data)
            => ExecuteInAllStack(ref key, ref data, SetAction);
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
            => ExecuteInAllStack(ref key, ref data, ref expirationDate, SetAction);
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
            => ExecuteInAllStack(ref key, ref data, ref expirationDate, ref tags, SetAction);
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
			=> ExecuteInAllStack(ref key, ref data, ref expirationDate, SetAction);
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
			=> ExecuteInAllStack(ref key, ref data, ref expirationDate, ref tags, SetAction);
        #endregion

        #region Set Multi-Key Data
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetMultiAction(ref IStorage sto, ref StorageItem[] items) => sto.SetMulti(items);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetMultiAction(ref IStorage sto, ref string[] keys, ref SerializedObject data) => sto.SetMulti(keys, data);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetMultiAction(ref IStorage sto, ref string[] keys, ref SerializedObject data, ref TimeSpan expirationDate) => sto.SetMulti(keys, data, expirationDate);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetMultiAction(ref IStorage sto, ref string[] keys, ref SerializedObject data, ref TimeSpan? expirationDate, ref string[] tags) => sto.SetMulti(keys, data, expirationDate, tags);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetMultiAction(ref IStorage sto, ref string[] keys, ref SerializedObject data, ref DateTime expirationDate) => sto.SetMulti(keys, data, expirationDate);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetMultiAction(ref IStorage sto, ref string[] keys, ref SerializedObject data, ref DateTime? expirationDate, ref string[] tags) => sto.SetMulti(keys, data, expirationDate, tags);

        /// <inheritdoc />
        /// <summary>
        /// Sets a new StorageItem with the given data
        /// </summary>
        /// <param name="items">StorageItem array</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetMulti(StorageItem[] items)
            => ExecuteInAllStack(ref items, SetMultiAction);
        /// <inheritdoc />
        /// <summary>
        /// Sets and create a new StorageItem with the given data
        /// </summary>
        /// <param name="keys">Items Keys</param>
        /// <param name="data">Item Data</param>
        /// <returns>true if the data could be save; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetMulti(string[] keys, SerializedObject data)
            => ExecuteInAllStack(ref keys, ref data, SetMultiAction);
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
            => ExecuteInAllStack(ref keys, ref data, ref expirationDate, SetMultiAction);
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
            => ExecuteInAllStack(ref keys, ref data, ref expirationDate, ref tags, SetMultiAction);
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
            => ExecuteInAllStack(ref keys, ref data, ref expirationDate, SetMultiAction);
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
            => ExecuteInAllStack(ref keys, ref data, ref expirationDate, ref tags, SetMultiAction);
        #endregion

        #region Update/Remove Data/Copy
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateDataAction(ref IStorage sto, ref string key, ref SerializedObject data) => sto.UpdateData(key, data);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RemoveAction(ref IStorage sto, ref string key) => sto.Remove(key);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopyAction(ref IStorage sto, ref string key, ref string newKey) => sto.Copy(key, newKey);

        /// <inheritdoc />
        /// <summary>
        /// Updates the data of an existing storage item.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="data">New item data</param>
        /// <returns>true if the data could be updated; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UpdateData(string key, SerializedObject data)
            => ExecuteInAllStack(ref key, ref data, UpdateDataAction);
        /// <inheritdoc />
        /// <summary>
        /// Removes a StorageItem with the Key specified.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <returns>true if the data could be removed; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(string key)
            => ExecuteInAllStack(ref key, RemoveAction);
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
			var res = ExecuteInAllStackAndReturn(tags, containingAll, (sto, arg1, arg2) => sto.RemoveByTag(arg1, arg2))
				.SelectMany(a => a)
				.Distinct()
				.ToArray();
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
	    public bool Copy(string key, string newKey)
	    	=> ExecuteInAllStack(ref key, ref newKey, CopyAction);
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Gets if the Storage is enabled.
        /// </summary>
        /// <returns>true if the storage is enabled; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEnabled()
        {
            for (var i = 0; i < _storages.Length; i++)
            {
                if (_storages[i].IsEnabled())
                    return true;
            }
            return false;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets if the Storage is ready to be requested.
        /// </summary>
        /// <returns>true if the storage is ready; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsReady()
        {
            for (var i = 0; i < _storages.Length; i++)
            {
                if (!_storages[i].IsReady())
                    return false;
            }
            return true;
        }
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
		        _storageStack.Each(s =>
		        {
			        var sM = s as StorageBase;
			        sM?.Dispose();
		        });
		        _storageStack.Clear();
	        }
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
