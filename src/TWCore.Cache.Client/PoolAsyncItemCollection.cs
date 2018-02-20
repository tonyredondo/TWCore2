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
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable ForCanBeConvertedToForeach

namespace TWCore.Cache.Client
{
	/// <summary>
	/// Storage action async delegate.
	/// </summary>
	public delegate Task StorageActionAsyncDelegate<in TA1>(PoolAsyncItem item, TA1 arg1);
	/// <summary>
	/// Storage action async delegate.
	/// </summary>
	public delegate Task StorageActionAsyncDelegate<in TA1, in TA2>(PoolAsyncItem item, TA1 arg1, TA2 arg2);
	/// <summary>
	/// Storage action async delegate.
	/// </summary>
	public delegate Task StorageActionAsyncDelegate<in TA1, in TA2, in TA3>(PoolAsyncItem item, TA1 arg1, TA2 arg2, TA3 arg3);
	/// <summary>
	/// Storage action async delegate.
	/// </summary>
	public delegate Task StorageActionAsyncDelegate<in TA1, in TA2, in TA3, in TA4>(PoolAsyncItem item, TA1 arg1, TA2 arg2, TA3 arg3, TA4 arg4);
	/// <summary>
	/// Storage func async delegate.
	/// </summary>
	public delegate Task<T> StorageFuncAsyncDelegate<T>(PoolAsyncItem item);
	/// <summary>
	/// Storage func async delegate.
	/// </summary>
	public delegate Task<T> StorageFuncAsyncDelegate<T, in TA1>(PoolAsyncItem item, TA1 arg1);
	/// <summary>
	/// Storage func async delegate.
	/// </summary>
	public delegate Task<T> StorageFuncAsyncDelegate<T, in TA1, in TA2>(PoolAsyncItem item, TA1 arg1, TA2 arg2);
	/// <summary>
	/// Storage func async delegate.
	/// </summary>
	public delegate Task<T> StorageFuncAsyncDelegate<T, in TA1, in TA2, in TA3>(PoolAsyncItem item, TA1 arg1, TA2 arg2, TA3 arg3);
	/// <summary>
	/// Storage func async delegate.
	/// </summary>
	public delegate Task<T> StorageFuncAsyncDelegate<T, in TA1, in TA2, in TA3, in TA4>(PoolAsyncItem item, TA1 arg1, TA2 arg2, TA3 arg3, TA4 arg4);
    /// <summary>
    /// Response Condition Delegate
    /// </summary>
    public delegate bool ResponseConditionDelegate<in T>(T response);

    /// <inheritdoc />
    /// <summary>
    /// Cache pool item collection
    /// </summary>
    public class PoolAsyncItemCollection : IDisposable
    {
	    private ActionWorker _worker;
	    private bool _firstTime = true;
	    private bool _hasMemoryStorage;
	    internal List<PoolAsyncItem> Items;

        #region Properties
        /// <summary>
        /// Delays between ping tries in milliseconds
        /// </summary>
        public int PingDelay { get; }
        /// <summary>
        /// Delay after a ping error for next try
        /// </summary>
        public int PingDelayOnError { get; }
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
			public PoolAsyncItem[] Items;
			public TA1 Arg1;
			public TA2 Arg2;
			public TA3 Arg3;
			public TA4 Arg4;
			public StorageActionAsyncDelegate<TA1, TA2, TA3, TA4> Action;
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
        /// <param name="selectionOrder">Algorithm to select the first item on the collection</param>
        /// <param name="indexOrder">Index order</param>
        public PoolAsyncItemCollection(int pingDelay = 5000, int pingDelayOnError = 30000, PoolReadMode readMode = PoolReadMode.NormalRead, PoolWriteMode writeMode = PoolWriteMode.WritesFirstAndThenAsync, PoolOrder selectionOrder = PoolOrder.PingTime, string indexOrder = null)
        {
            PingDelay = pingDelay;
            PingDelayOnError = pingDelayOnError;
            WriteMode = writeMode;
            ReadMode = readMode;
            SelectionOrder = selectionOrder;
            IndexOrder = indexOrder?.SplitAndTrim(",")?.Select(s => s.ParseTo(-1)).Where(s => s > 0).Distinct().ToArray();
            _worker = new ActionWorker();
            Items = new List<PoolAsyncItem>();

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
	            if (Items == null) return;
	            collection.Add(nameof(Items.Count), Items.Count);
	            foreach (var item in Items)
		            Core.Status.AttachChild(item, this);
            });
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add a PoolItem in the collection
        /// </summary>
        /// <param name="item">Pool item to be added in the collection</param>
        public void Add(PoolAsyncItem item)
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
        public PoolAsyncItem GetByName(string name)
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
		public async Task<PoolAsyncItem[]> WaitAndGetEnabledAsync(StorageItemMode mode, bool onlyMemoryStorages = false)
		{
			var sw = Stopwatch.StartNew();

			//Sort index according the IndexOrder
			if (_firstTime && SelectionOrder == PoolOrder.Index && IndexOrder?.Any() == true)
			{
				_firstTime = false;
				var lstItems = new List<PoolAsyncItem>(Items);
				var nItems = new List<PoolAsyncItem>();
				foreach(var idx in IndexOrder)
				{
					if (idx < 0 || idx >= lstItems.Count) continue;
					if (lstItems[idx] == null) continue;
					nItems.Add(lstItems[idx]);
					lstItems[idx] = null;
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

				var poolAsyncItems = iWhere.ToArray();
				if (!ForceAtLeastOneNetworkItemEnabled && poolAsyncItems.Any())
					return poolAsyncItems;

				if (ForceAtLeastOneNetworkItemEnabled && poolAsyncItems.Any(i => i.Storage.Type != StorageType.Memory))
					return poolAsyncItems;

				if (onlyMemoryStorages)
					return poolAsyncItems;

			    await Task.Delay(250).ConfigureAwait(false);
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
		public async Task<T> ReadAsync<T>(StorageFuncAsyncDelegate<T> function, ResponseConditionDelegate<T> responseCondition)
			=> (await ReadAsync(null, null, (PoolAsyncItem item, object a1, object a2) => function(item), responseCondition).ConfigureAwait(false)).Item1;
	    /// <summary>
	    /// Read action on the Pool.
	    /// </summary>
	    /// <typeparam name="T">Return value type</typeparam>
	    /// <typeparam name="TA1">Argument 1 type</typeparam>
	    /// <param name="arg1">Argument 1</param>
	    /// <param name="function">Function to execute on the storage pool item</param>
	    /// <param name="responseCondition">Function to check if the result is good or not.</param>
	    /// <returns>Return value</returns>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task<(T, PoolAsyncItem)> ReadAsync<T, TA1>(TA1 arg1, StorageFuncAsyncDelegate<T, TA1> function, ResponseConditionDelegate<T> responseCondition)
			=> ReadAsync(arg1, null, (PoolAsyncItem item, TA1 a1, object a2) => function(item, a1), responseCondition);
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
		public async Task<(T, PoolAsyncItem)> ReadAsync<T, TA1, TA2>(TA1 arg1, TA2 arg2, StorageFuncAsyncDelegate<T, TA1, TA2> function, ResponseConditionDelegate<T> responseCondition)
		{
			Core.Log.LibVerbose("Queue Pool Get - ReadMode: {0}", ReadMode);
			var items = await WaitAndGetEnabledAsync(StorageItemMode.Read).ConfigureAwait(false);
			if (ReadMode != PoolReadMode.NormalRead && ReadMode != PoolReadMode.FastestOnlyRead) 
				return (default(T), null);
			var iCount = items.Length;
			for(var i = 0; i < iCount; i++) 
			{
				var item = items[i];
				try 
				{
					var response = await function(item, arg1, arg2).ConfigureAwait(false);
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

		#region WriteAsync Methods

	    /// <summary>
	    /// Write action on the Pool
	    /// </summary>
	    /// <param name="arg1">Argument 1</param>
	    /// <param name="action">Action to execute in the pool item</param>
	    /// <param name="onlyMemoryStorages">Write only on memory storages</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task WriteAsync<TA1>(TA1 arg1, StorageActionAsyncDelegate<TA1> action, bool onlyMemoryStorages = false)
			=> WriteAsync(arg1, null, null, null, 
		             (PoolAsyncItem item, TA1 a1, object a2, object a3, object a4) => action(item, a1), onlyMemoryStorages);
	    /// <summary>
	    /// Write action on the Pool
	    /// </summary>
	    /// <param name="arg1">Argument 1</param>
	    /// <param name="arg2">Argument 2</param>
	    /// <param name="action">Action to execute in the pool item</param>
	    /// <param name="onlyMemoryStorages">Write only on memory storages</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task WriteAsync<TA1, TA2>(TA1 arg1, TA2 arg2, StorageActionAsyncDelegate<TA1, TA2> action, bool onlyMemoryStorages = false)
			=> WriteAsync(arg1, arg2, null, null, 
		             (PoolAsyncItem item, TA1 a1, TA2 a2, object a3, object a4) => action(item, a1, a2), onlyMemoryStorages);
		/// <summary>
		/// Write action on the Pool
		/// </summary>
		/// <param name="arg1">Argument 1</param>
		/// <param name="arg2">Argument 2</param>
		/// <param name="arg3">Argument 3</param>
		/// <param name="action">Action to execute in the pool item</param>
		/// <param name="onlyMemoryStorages">Write only on memory storages</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task WriteAsync<TA1, TA2, TA3>(ref TA1 arg1, ref TA2 arg2, ref TA3 arg3, StorageActionAsyncDelegate<TA1, TA2, TA3> action, bool onlyMemoryStorages = false)
			=> WriteAsync(arg1, arg2, arg3, null, 
		             (PoolAsyncItem item, TA1 a1, TA2 a2, TA3 a3, object a4) => action(item, a1, a2, a3), onlyMemoryStorages);
		/// <summary>
		/// Write action on the Pool
		/// </summary>
		/// <param name="arg1">Argument 1</param>
		/// <param name="arg2">Argument 2</param>
		/// <param name="arg3">Argument 3</param>
		/// <param name="arg4">Argument 4</param>
		/// <param name="action">Action to execute in the pool item</param>
		/// <param name="onlyMemoryStorages">Write only on memory storages</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task WriteAsync<TA1, TA2, TA3, TA4>(TA1 arg1, TA2 arg2, TA3 arg3, TA4 arg4, StorageActionAsyncDelegate<TA1, TA2, TA3, TA4> action, bool onlyMemoryStorages = false)
		{
			Core.Log.LibVerbose("Queue Pool Action - WriteMode: {0}", WriteMode);
			var arrEnabled = await WaitAndGetEnabledAsync(StorageItemMode.Write, onlyMemoryStorages).ConfigureAwait(false);

			switch (WriteMode)
			{
				case PoolWriteMode.WritesAllInSync:
					for(var i = 0; i < arrEnabled.Length; i++)
					{
						var item = arrEnabled[i];
						try
						{
							await action(item, arg1, arg2, arg3, arg4).ConfigureAwait(false);
							Core.Log.LibVerbose("\tAction executed on: '{0}'.", item.Name);
						}
						catch (Exception ex)
						{
							Core.Log.Error(ex, "\tAction Exception on '{0}': {1}", item.Name, ex.Message);
						}
					}
					return;
				case PoolWriteMode.WritesFirstAndThenAsync:
					var idx = 0;
					for(; idx < arrEnabled.Length; idx++)
					{
						var item = arrEnabled[idx];
						try
						{
							await action(item, arg1, arg2, arg3, arg4).ConfigureAwait(false);
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
                        _worker.Enqueue(action: WorkerHandler, state: new WriteItem<TA1, TA2, TA3, TA4> 
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
					break;
				default:
					throw new ArgumentOutOfRangeException();
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
		public Task<T> WriteAsync<T, TA1>(TA1 arg1, StorageFuncAsyncDelegate<T, TA1> function, bool onlyMemoryStorages = false)
			=> WriteAsync(arg1, null, null, null, 
		             (PoolAsyncItem item, TA1 a1, object a2, object a3, object a4) => function(item, a1), onlyMemoryStorages);
		/// <summary>
		/// Write action on the Pool
		/// </summary>
		/// <param name="arg1">Argument 1</param>
		/// <param name="arg2">Argument 2</param>
		/// <param name="function">Function to execute in the pool item</param>
		/// <param name="onlyMemoryStorages">Write only on memory storages</param>
		/// <returns>Return value from the function</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task<T> WriteAsync<T, TA1, TA2>(TA1 arg1, TA2 arg2, StorageFuncAsyncDelegate<T, TA1, TA2> function, bool onlyMemoryStorages = false)
			=> WriteAsync(arg1, arg2, null, null, 
		             (PoolAsyncItem item, TA1 a1, TA2 a2, object a3, object a4) => function(item, a1, a2), onlyMemoryStorages);
		/// <summary>
		/// Write action on the Pool
		/// </summary>
		/// <param name="arg1">Argument 1</param>
		/// <param name="arg2">Argument 2</param>
		/// <param name="arg3">Argument 3</param>
		/// <param name="function">Function to execute in the pool item</param>
		/// <param name="onlyMemoryStorages">Write only on memory storages</param>
		/// <returns>Return value from the function</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task<T> WriteAsync<T, TA1, TA2, TA3>(TA1 arg1, TA2 arg2, TA3 arg3, StorageFuncAsyncDelegate<T, TA1, TA2, TA3> function, bool onlyMemoryStorages = false)
			=> WriteAsync(arg1, arg2, arg3, null, 
		         (PoolAsyncItem item, TA1 a1, TA2 a2, TA3 a3, object a4) => function(item, a1, a2, a3), onlyMemoryStorages);
		/// <summary>
		/// Write action on the Pool
		/// </summary>
		/// <param name="arg1">Argument 1</param>
		/// <param name="arg2">Argument 2</param>
		/// <param name="arg3">Argument 3</param>
		/// <param name="arg4">Argument 4</param>
		/// <param name="function">Action to execute in the pool item</param>
		/// <param name="onlyMemoryStorages">Write only on memory storages</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<T> WriteAsync<T, TA1, TA2, TA3, TA4>(TA1 arg1, TA2 arg2, TA3 arg3, TA4 arg4, StorageFuncAsyncDelegate<T, TA1, TA2, TA3, TA4> function, bool onlyMemoryStorages = false)
		{
			Core.Log.LibVerbose("Queue Pool Action - WriteMode: {0}", WriteMode);
			var arrEnabled = await WaitAndGetEnabledAsync(StorageItemMode.Write, onlyMemoryStorages).ConfigureAwait(false);

			var response = default(T);

			switch (WriteMode)
			{
				case PoolWriteMode.WritesAllInSync:
					for(var i = 0; i < arrEnabled.Length; i++)
					{
						var item = arrEnabled[i];
						try
						{
							response = await function(item, arg1, arg2, arg3, arg4).ConfigureAwait(false);
							Core.Log.LibVerbose("\tAction executed on: '{0}'.", item.Name);
						}
						catch (Exception ex)
						{
							Core.Log.Error(ex, "\tAction Exception on '{0}': {1}", item.Name, ex.Message);
						}
					}
					return response;
				case PoolWriteMode.WritesFirstAndThenAsync:
					var idx = 0;
					for(; idx < arrEnabled.Length; idx++)
					{
						var item = arrEnabled[idx];
						try
						{
							response = await function(item, arg1, arg2, arg3, arg4).ConfigureAwait(false);
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
						_worker.Enqueue(action: WorkerHandler, state: new WriteItem<TA1, TA2, TA3, TA4> 
						{ 
							Action = async (item, a1, a2, a3, a4) => await function(item, a1, a2, a3, a4).ConfigureAwait(false), 
							Arg1 = arg1, 
							Arg2 = arg2, 
							Arg3 = arg3, 
							Arg4 = arg4, 
							Index = idx, 
							Items = arrEnabled 
						});
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
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
						wItem.Action(item, wItem.Arg1, wItem.Arg2, wItem.Arg3, wItem.Arg4).WaitAsync();
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
            Items?.Clear();
            Items = null;
            Core.Status.DeAttachObject(this);
        }

        #endregion
    }
}
