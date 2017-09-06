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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TWCore.Cache.Client
{
	/// <summary>
	/// Storage action delegate.
	/// </summary>
	public delegate void StorageActionDelegate<in TA1>(PoolItem item, TA1 arg1);
	/// <summary>
	/// Storage action delegate.
	/// </summary>
	public delegate void StorageActionDelegate<in TA1, in TA2>(PoolItem item, TA1 arg1, TA2 arg2);
	/// <summary>
	/// Storage action delegate.
	/// </summary>
	public delegate void StorageActionDelegate<in TA1, in TA2, in TA3>(PoolItem item, TA1 arg1, TA2 arg2, TA3 arg3);
	/// <summary>
	/// Storage action delegate.
	/// </summary>
	public delegate void StorageActionDelegate<in TA1, in TA2, in TA3, in TA4>(PoolItem item, TA1 arg1, TA2 arg2, TA3 arg3, TA4 arg4);
	/// <summary>
	/// Storage func delegate.
	/// </summary>
	public delegate T StorageFuncDelegate<out T>(PoolItem item);
	/// <summary>
	/// Storage func delegate.
	/// </summary>
	public delegate T StorageFuncDelegate<out T, in TA1>(PoolItem item, TA1 arg1);
	/// <summary>
	/// Storage func delegate.
	/// </summary>
	public delegate T StorageFuncDelegate<out T, in TA1, in TA2>(PoolItem item, TA1 arg1, TA2 arg2);
	/// <summary>
	/// Storage func delegate.
	/// </summary>
	public delegate T StorageFuncDelegate<out T, in TA1, in TA2, in TA3>(PoolItem item, TA1 arg1, TA2 arg2, TA3 arg3);
	/// <summary>
	/// Storage func delegate.
	/// </summary>
	public delegate T StorageFuncDelegate<out T, in TA1, in TA2, in TA3, in TA4>(PoolItem item, TA1 arg1, TA2 arg2, TA3 arg3, TA4 arg4);
	/// <summary>
	/// Response Condition Delegate
	/// </summary>
	public delegate bool ResponseConditionDelegate<in T>(T response);

    /// <inheritdoc />
    /// <summary>
    /// Cache pool item collection
    /// </summary>
    public class PoolItemCollection : IDisposable
    {
	    private ActionWorker _worker;
	    private bool _firstTime = true;
	    private bool _hasMemoryStorage;
	    internal List<PoolItem> Items;

        #region Properties
        /// <summary>
        /// Delays between ping tries in milliseconds
        /// </summary>
        public int PingDelay { get; private set; }
        /// <summary>
        /// Delay after a ping error for next try
        /// </summary>
        public int PingDelayOnError { get; private set; }
        /// <summary>
        /// Cache pool Read Mode
        /// </summary>
        public PoolReadMode ReadMode { get; set; }
        /// <summary>
        /// Cache pool Write Mode
        /// </summary>
        public PoolWriteMode WriteMode { get; set; }
        /// <summary>
        /// Pool item selection order for Read and Write
        /// </summary>
        public PoolOrder SelectionOrder { get; set; }
        /// <summary>
        /// Index Order
        /// </summary>
        public int[] IndexOrder { get; set; }
        /// <summary>
        /// Force at least one network item enabled
        /// </summary>
        public bool ForceAtLeastOneNetworkItemEnabled { get; set; } = true;
		/// <summary>
		/// Gets is the pool has a memory storage
		/// </summary>
		/// <value><c>true</c> if has memory storage; otherwise, <c>false</c>.</value>
		public bool HasMemoryStorage => _hasMemoryStorage;
        #endregion

		#region Nested Type
	    private class WriteItem<TA1, TA2, TA3, TA4>
		{
			public int Index;
			public PoolItem[] Items;
			public TA1 Arg1;
			public TA2 Arg2;
			public TA3 Arg3;
			public TA4 Arg4;
			public StorageActionDelegate<TA1, TA2, TA3, TA4> Action;
		}
		#endregion

        #region .ctor
        /// <summary>
        /// Cache pool item collection
        /// </summary>
        /// <param name="pingDelay">Delays between ping tries in milliseconds</param>
        /// <param name="pingDelayOnError">Delay after a ping error for next try</param>
        /// <param name="readMode">Cache pool Read Mode</param>
        /// <param name="writeMode">Cache pool Write Mode</param>
        /// <param name="selectionOrder">Pool item selection order for Read and Write</param>
        /// <param name="indexOrder">Index order</param>
        public PoolItemCollection(int pingDelay = 5000, int pingDelayOnError = 30000, PoolReadMode readMode = PoolReadMode.NormalRead, PoolWriteMode writeMode = PoolWriteMode.WritesFirstAndThenAsync, PoolOrder selectionOrder = PoolOrder.PingTime, string indexOrder = null)
        {
            PingDelay = pingDelay;
            PingDelayOnError = pingDelayOnError;
            WriteMode = writeMode;
            ReadMode = readMode;
            SelectionOrder = selectionOrder;
            IndexOrder = indexOrder?.SplitAndTrim(",")?.Select(s => s.ParseTo(-1)).Where(s => s > 0).Distinct().ToArray();
            _worker = new ActionWorker();
            Items = new List<PoolItem>();

            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(PingDelay), PingDelay);
                collection.Add(nameof(PingDelayOnError), PingDelayOnError);
                collection.Add(nameof(ReadMode), ReadMode);
                collection.Add(nameof(WriteMode), WriteMode);
                collection.Add(nameof(SelectionOrder), SelectionOrder);
                collection.Add(nameof(IndexOrder), IndexOrder?.Join(","));
                collection.Add(nameof(ForceAtLeastOneNetworkItemEnabled), ForceAtLeastOneNetworkItemEnabled);
                Core.Status.AttachChild(_worker, this);
                if (Items != null)
                {
                    collection.Add(nameof(Items.Count), Items.Count);
                    foreach (var item in Items)
                        Core.Status.AttachChild(item, this);
                }
            });
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add a PoolItem in the collection
        /// </summary>
        /// <param name="item">Pool item to be added in the collection</param>
        public void Add(PoolItem item)
        {
            if (item == null) return;
            item.PingDelay = PingDelay;
            item.PingDelayOnError = PingDelayOnError;
            Items.Add(item);
            _hasMemoryStorage |= item.Storage.Type == StorageType.Memory;
        }
        /// <summary>
        /// Clear all the pool collection
        /// </summary>
        public void Clear()
            => Items.Clear();
        /// <summary>
        /// Gets if a storage name is on the Pool item collection
        /// </summary>
        /// <param name="name">Name of the storage</param>
        /// <returns>true if the storage is on the collection; otherwise, false.</returns>
        public bool Contains(string name)
            => Items.Any(i => i.Name == name);
        /// <summary>
        /// Gets if a storage name is on the Pool item collection
        /// </summary>
        /// <param name="name">Name of the storage</param>
        /// <returns>true if the storage is on the collection; otherwise, false.</returns>
        public PoolItem GetByName(string name)
            => Items.FirstOrDefault(i => i.Name == name);
        /// <summary>
        /// Gets if any storage is enabled
        /// </summary>
        /// <returns>true if there at least one cache is enabled; otherwise, false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AnyEnabled() => Items.Any(i => i.Enabled);
        /// <summary>
        /// Waits and gets the list of enabled storages ordered by Ping time
        /// </summary>
        /// <param name="mode">Storage item mode</param>
        /// <param name="onlyMemoryStorages">Only on memory storages</param>
        /// <returns>List of Enabled Pool items</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PoolItem[] WaitAndGetEnabled(StorageItemMode mode, bool onlyMemoryStorages = false)
        {
            var sw = Stopwatch.StartNew();

            //Sort index according the IndexOrder
            if (_firstTime && SelectionOrder == PoolOrder.Index && IndexOrder?.Any() == true)
            {
                _firstTime = false;
                var lstItems = new List<PoolItem>(Items);
                var nItems = new List<PoolItem>();
                foreach(var idx in IndexOrder)
                {
                    if (idx >=0 && idx < lstItems.Count)
                    {
                        if (lstItems[idx] == null) continue;
                        nItems.Add(lstItems[idx]);
                        lstItems[idx] = null;
                    }
                }
                foreach(var item in lstItems)
                    if (item != null)
                        nItems.Add(item);
                Items = nItems;
            }

            while (sw.Elapsed.TotalSeconds < 15)
            {
	            var iWhere = onlyMemoryStorages ? 
		            Items.Where(i => i.Enabled && i.Mode.HasFlag(mode) && i.Storage.Type == StorageType.Memory) : 
		            Items.Where(i => i.Enabled && i.Mode.HasFlag(mode));

                if (SelectionOrder == PoolOrder.PingTime)
                    iWhere = iWhere.OrderBy(i => i.PingTime);

	            var poolItems = iWhere.ToArray();
	            if (!ForceAtLeastOneNetworkItemEnabled && poolItems.Any())
					return poolItems;

				if (ForceAtLeastOneNetworkItemEnabled && poolItems.Any(i => i.Storage.Type != StorageType.Memory))
					return poolItems;

				if (onlyMemoryStorages)
					return poolItems;

                Factory.Thread.Sleep(250);
            }

			Core.Log.Warning("Error looking for enabled Caches in Mode {0}. There is not connection to any cache server.", mode);
			throw new Exception(string.Format("Error looking for enabled Caches in Mode {0}. There is not connection to any cache server. Please check configurations.", mode));
        }

		#region Read Methods
        /// <summary>
        /// Read action on the Pool.
        /// </summary>
        /// <typeparam name="T">Return value type</typeparam>
        /// <param name="function">Function to execute on the storage pool item</param>
        /// <param name="responseCondition">Function to check if the result is good or not.</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Read<T>(StorageFuncDelegate<T> function, ResponseConditionDelegate<T> responseCondition)
			=> Read((object)null, (object)null, (item, a1, a2) => function(item), responseCondition).Item1;
	    /// <summary>
	    /// Read action on the Pool.
	    /// </summary>
	    /// <typeparam name="T">Return value type</typeparam>
	    /// <typeparam name="TA1">Argument type</typeparam>
	    /// <param name="arg1">Argument 1</param>
	    /// <param name="function">Function to execute on the storage pool item</param>
	    /// <param name="responseCondition">Function to check if the result is good or not.</param>
	    /// <returns>Return value</returns>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T, PoolItem) Read<T, TA1>(TA1 arg1, StorageFuncDelegate<T, TA1> function, ResponseConditionDelegate<T> responseCondition)
			=> Read(arg1, (object)null, (item, a1, a2) => function(item, a1), responseCondition);
	    /// <summary>
	    /// Read action on the Pool.
	    /// </summary>
	    /// <typeparam name="T">Return value type</typeparam>
	    /// <typeparam name="TA1">Argument 1 type</typeparam>
	    /// <typeparam name="TA2">Argument 2 type</typeparam>
	    /// <param name="arg1">Argument 1</param>
	    /// <param name="arg2">Argument 2</param>
	    /// <param name="function">Function to execute on the storage pool item</param>
	    /// <param name="responseCondition">Function to check if the result is good or not.</param>
	    /// <returns>Return value</returns>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T, PoolItem) Read<T, TA1, TA2>(TA1 arg1, TA2 arg2, StorageFuncDelegate<T, TA1, TA2> function, ResponseConditionDelegate<T> responseCondition)
		{
			Core.Log.LibVerbose("Queue Pool Get - ReadMode: {0}", ReadMode);
			var items = WaitAndGetEnabled(StorageItemMode.Read);
			if (ReadMode != PoolReadMode.NormalRead && ReadMode != PoolReadMode.FastestOnlyRead) 
				return (default(T), null);
			var iCount = items.Length;
			for(var i = 0; i < iCount; i++) 
			{
				var item = items[i];
				try 
				{
					var response = function(item, arg1, arg2);
					if (responseCondition(response))
						return (response, item);
					if (item.Storage.Type != StorageType.Memory && ReadMode == PoolReadMode.FastestOnlyRead)
						break;
				}
				catch(Exception ex) 
				{
					Core.Log.LibVerbose("\tException on PoolGet '{0}'. Message: {1}", item.Name, ex.Message);
				}
			}
			Core.Log.LibVerbose("\tItem not Found in the pool.");
			return (default(T), null);
		}
		#endregion

		#region Write Methods
	    /// <summary>
	    /// Write action on the Pool
	    /// </summary>
	    /// <param name="arg1">Argument 1</param>
	    /// <param name="action">Action to execute in the pool item</param>
	    /// <param name="onlyMemoryStorages">Write only on memory storages</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<TA1>(TA1 arg1, StorageActionDelegate<TA1> action, bool onlyMemoryStorages = false)
			=> Write(arg1, (object)null, (object)null, (object)null, (item, a1, a2, a3, a4) => action(item, a1), onlyMemoryStorages);
	    /// <summary>
	    /// Write action on the Pool
	    /// </summary>
	    /// <param name="action">Action to execute in the pool item</param>
	    /// <param name="onlyMemoryStorages">Write only on memory storages</param>
	    /// <param name="arg1">Argument 1</param>
	    /// <param name="arg2">Argument 2</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write<TA1, TA2>(TA1 arg1, TA2 arg2, StorageActionDelegate<TA1, TA2> action, bool onlyMemoryStorages = false)
			=> Write(arg1, arg2, (object)null, (object)null, (item, a1, a2, a3, a4) => action(item, a1, a2), onlyMemoryStorages);
	    /// <summary>
	    /// Write action on the Pool
	    /// </summary>
	    /// <param name="action">Action to execute in the pool item</param>
	    /// <param name="onlyMemoryStorages">Write only on memory storages</param>
	    /// <param name="arg1">Argument 1</param>
	    /// <param name="arg2">Argument 2</param>
	    /// <param name="arg3">Argument 3</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write<TA1, TA2, TA3>(TA1 arg1, TA2 arg2, TA3 arg3, StorageActionDelegate<TA1, TA2, TA3> action, bool onlyMemoryStorages = false)
			=> Write(arg1, arg2, arg3, (object)null, (item, a1, a2, a3, a4) => action(item, a1, a2, a3), onlyMemoryStorages);

	    /// <summary>
	    /// Write action on the Pool
	    /// </summary>
	    /// <param name="action">Action to execute in the pool item</param>
	    /// <param name="onlyMemoryStorages">Write only on memory storages</param>
	    /// <param name="arg1">Argument 1</param>
	    /// <param name="arg2">Argument 2</param>
	    /// <param name="arg3">Argument 3</param>
	    /// <param name="arg4">Argument 4</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write<TA1, TA2, TA3, TA4>(TA1 arg1, TA2 arg2, TA3 arg3, TA4 arg4, StorageActionDelegate<TA1, TA2, TA3, TA4> action, bool onlyMemoryStorages = false)
		{
			Core.Log.LibVerbose("Queue Pool Action - WriteMode: {0}", WriteMode);
			var arrEnabled = WaitAndGetEnabled(StorageItemMode.Write, onlyMemoryStorages);

			if (WriteMode == PoolWriteMode.WritesAllInSync)
			{
				for(var i = 0; i < arrEnabled.Length; i++)
				{
					var item = arrEnabled[i];
					try
					{
						action(item, arg1, arg2, arg3, arg4);
						Core.Log.LibVerbose("\tAction executed on: '{0}'.", item.Name);
					}
					catch (Exception ex)
					{
						Core.Log.Error(ex, "\tAction Exception on '{0}': {1}", item.Name, ex.Message);
					}
				}
				return;
			}

			if (WriteMode == PoolWriteMode.WritesFirstAndThenAsync)
			{
				var idx = 0;
				for(; idx < arrEnabled.Length; idx++)
				{
					var item = arrEnabled[idx];
					try
					{
						action(item, arg1, arg2, arg3, arg4);
						Core.Log.LibVerbose("\tAction executed on: '{0}'.", item.Name);
					}
					catch (Exception ex)
					{
						Core.Log.Error(ex, "\tAction Exception on '{0}': {1}", item.Name, ex.Message);
					}
					if (item.Storage.Type != StorageType.Memory)
						break;
				}
				idx++;
				if (idx < arrEnabled.Length) 
				{
					_worker.Enqueue(WorkerHandler, new WriteItem<TA1, TA2, TA3, TA4> 
					{ 
						Action = action, 
						Arg1 = arg1, 
						Arg2 = arg2, 
						Arg3 = arg3, 
						Arg4 = arg4, 
						Index = idx, 
						Items = arrEnabled 
					});
				}
			}
		}
	    /// <summary>
	    /// Write action on the Pool
	    /// </summary>
	    /// <param name="arg1">Argument 1</param>
	    /// <param name="function">Function to execute in the pool item</param>
	    /// <param name="onlyMemoryStorages">Write only on memory storages</param>
	    /// <returns>Return value from the function</returns>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Write<T, TA1>(TA1 arg1, StorageFuncDelegate<T, TA1> function, bool onlyMemoryStorages = false)
			=> Write(arg1, (object)null, (object)null, (object)null, (item, a1, a2, a3, a4) => function(item, a1), onlyMemoryStorages);
	    /// <summary>
	    /// Write action on the Pool
	    /// </summary>
	    /// <param name="function">Function to execute in the pool item</param>
	    /// <param name="onlyMemoryStorages">Write only on memory storages</param>
	    /// <param name="arg1">Argument 1</param>
	    /// <param name="arg2">Argument 2</param>
	    /// <returns>Return value from the function</returns>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Write<T, TA1, TA2>(TA1 arg1, TA2 arg2, StorageFuncDelegate<T, TA1, TA2> function, bool onlyMemoryStorages = false)
			=> Write(arg1, arg2, (object)null, (object)null, (item, a1, a2, a3, a4) => function(item, a1, a2), onlyMemoryStorages);
	    /// <summary>
	    /// Write action on the Pool
	    /// </summary>
	    /// <param name="function">Function to execute in the pool item</param>
	    /// <param name="onlyMemoryStorages">Write only on memory storages</param>
	    /// <param name="arg1">Argument 1</param>
	    /// <param name="arg2">Argument 2</param>
	    /// <param name="arg3">Argument 3</param>
	    /// <returns>Return value from the function</returns>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Write<T, TA1, TA2, TA3>(TA1 arg1, TA2 arg2, TA3 arg3, StorageFuncDelegate<T, TA1, TA2, TA3> function, bool onlyMemoryStorages = false)
			=> Write(arg1, arg2, arg3, (object)null, (item, a1, a2, a3, a4) => function(item, a1, a2, a3), onlyMemoryStorages);
	    /// <summary>
	    /// Write action on the Pool
	    /// </summary>
	    /// <param name="function">Action to execute in the pool item</param>
	    /// <param name="onlyMemoryStorages">Write only on memory storages</param>
	    /// <param name="arg1">Argument 1</param>
	    /// <param name="arg2">Argument 2</param>
	    /// <param name="arg3">Argument 3</param>
	    /// <param name="arg4">Argument 4</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Write<T, TA1, TA2, TA3, TA4>(TA1 arg1, TA2 arg2, TA3 arg3, TA4 arg4, StorageFuncDelegate<T, TA1, TA2, TA3, TA4> function, bool onlyMemoryStorages = false)
		{
			Core.Log.LibVerbose("Queue Pool Action - WriteMode: {0}", WriteMode);
			var arrEnabled = WaitAndGetEnabled(StorageItemMode.Write, onlyMemoryStorages);

			var response = default(T);

			if (WriteMode == PoolWriteMode.WritesAllInSync)
			{
				for(var i = 0; i < arrEnabled.Length; i++)
				{
					var item = arrEnabled[i];
					try
					{
						response = function(item, arg1, arg2, arg3, arg4);
						Core.Log.LibVerbose("\tAction executed on: '{0}'.", item.Name);
					}
					catch (Exception ex)
					{
						Core.Log.Error(ex, "\tAction Exception on '{0}': {1}", item.Name, ex.Message);
					}
				}
				return response;
			}

			if (WriteMode == PoolWriteMode.WritesFirstAndThenAsync)
			{
				var idx = 0;
				for(; idx < arrEnabled.Length; idx++)
				{
					var item = arrEnabled[idx];
					try
					{
						response = function(item, arg1, arg2, arg3, arg4);
						Core.Log.LibVerbose("\tAction executed on: '{0}'.", item.Name);
					}
					catch (Exception ex)
					{
						Core.Log.Error(ex, "\tAction Exception on '{0}': {1}", item.Name, ex.Message);
					}
					if (item.Storage.Type != StorageType.Memory)
						break;
				}
				idx++;
				if (idx < arrEnabled.Length) 
				{
					_worker.Enqueue(WorkerHandler, new WriteItem<TA1, TA2, TA3, TA4> 
					{ 
						Action = (item, a1, a2, a3, a4) => function(item, a1, a2, a3, a4), 
						Arg1 = arg1, 
						Arg2 = arg2, 
						Arg3 = arg3, 
						Arg4 = arg4, 
						Index = idx, 
						Items = arrEnabled 
					});
				}
			}

			return response;
		}

	    private static void WorkerHandler<TA1, TA2, TA3, TA4>(WriteItem<TA1, TA2, TA3, TA4> wItem)
		{
			for(; wItem.Index < wItem.Items.Length; wItem.Index++)
			{
				var item = wItem.Items[wItem.Index];
				try
				{
					if (item.Enabled)
					{
						wItem.Action(item, wItem.Arg1, wItem.Arg2, wItem.Arg3, wItem.Arg4);
						Core.Log.LibVerbose("\tAction executed on: '{0}'.", item.Name);
					}
				}
				catch (Exception ex)
				{
					Core.Log.Error(ex, "\tAction Exception on '{0}': {1}", item.Name, ex.Message);
				}
			}
		}
		#endregion

        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            _worker?.Dispose();
            _worker = null;
            Items?.Each(i => i.Dispose());
            Items = null;
            Core.Status.DeAttachObject(this);
        }

        #endregion
    }
}
