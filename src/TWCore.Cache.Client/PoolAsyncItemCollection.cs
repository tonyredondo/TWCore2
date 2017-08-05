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
using System.Threading.Tasks;

namespace TWCore.Cache.Client
{
	/// <summary>
	/// Storage action async delegate.
	/// </summary>
	public delegate Task StorageActionAsyncDelegate<A1>(PoolAsyncItem item, A1 arg1);
	/// <summary>
	/// Storage action async delegate.
	/// </summary>
	public delegate Task StorageActionAsyncDelegate<A1, A2>(PoolAsyncItem item, A1 arg1, A2 arg2);
	/// <summary>
	/// Storage action async delegate.
	/// </summary>
	public delegate Task StorageActionAsyncDelegate<A1, A2, A3>(PoolAsyncItem item, A1 arg1, A2 arg2, A3 arg3);
	/// <summary>
	/// Storage action async delegate.
	/// </summary>
	public delegate Task StorageActionAsyncDelegate<A1, A2, A3, A4>(PoolAsyncItem item, A1 arg1, A2 arg2, A3 arg3, A4 arg4);
	/// <summary>
	/// Storage func async delegate.
	/// </summary>
	public delegate Task<T> StorageFuncAsyncDelegate<T>(PoolAsyncItem item);
	/// <summary>
	/// Storage func async delegate.
	/// </summary>
	public delegate Task<T> StorageFuncAsyncDelegate<T, A1>(PoolAsyncItem item, A1 arg1);
	/// <summary>
	/// Storage func async delegate.
	/// </summary>
	public delegate Task<T> StorageFuncAsyncDelegate<T, A1, A2>(PoolAsyncItem item, A1 arg1, A2 arg2);
	/// <summary>
	/// Storage func async delegate.
	/// </summary>
	public delegate Task<T> StorageFuncAsyncDelegate<T, A1, A2, A3>(PoolAsyncItem item, A1 arg1, A2 arg2, A3 arg3);
	/// <summary>
	/// Storage func async delegate.
	/// </summary>
	public delegate Task<T> StorageFuncAsyncDelegate<T, A1, A2, A3, A4>(PoolAsyncItem item, A1 arg1, A2 arg2, A3 arg3, A4 arg4);

    /// <summary>
    /// Cache pool item collection
    /// </summary>
    public class PoolAsyncItemCollection : IDisposable
    {
		static object emptyObject = new object();
        ActionWorker Worker;
        internal List<PoolAsyncItem> Items;
        bool firstTime = true;

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
        #endregion

		#region Nested Type
		class WriteItem<A1, A2, A3, A4>
		{
			public int Index;
			public PoolAsyncItem[] Items;
			public A1 Arg1;
			public A2 Arg2;
			public A3 Arg3;
			public A4 Arg4;
			public StorageActionAsyncDelegate<A1, A2, A3, A4> Action;
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
            Worker = new ActionWorker();
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
                Core.Status.AttachChild(Worker, this);
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
        public void Add(PoolAsyncItem item)
        {
            if (item != null)
            {
                item.PingDelay = PingDelay;
                item.PingDelayOnError = PingDelayOnError;
                Items.Add(item);
            }
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
		public PoolAsyncItem[] WaitAndGetEnabled(StorageItemMode mode, bool onlyMemoryStorages = false)
		{
			var sw = Stopwatch.StartNew();

			//Sort index according the IndexOrder
			if (firstTime && SelectionOrder == PoolOrder.Index && IndexOrder?.Any() == true)
			{
				firstTime = false;
				var lstItems = new List<PoolAsyncItem>(Items);
				var nItems = new List<PoolAsyncItem>();
				foreach(var idx in IndexOrder)
				{
					if (idx >=0 && idx < lstItems.Count)
					{
						if (lstItems[idx] != null)
						{
							nItems.Add(lstItems[idx]);
							lstItems[idx] = null;
						}
					}
				}
				foreach(var item in lstItems)
					if (item != null)
						nItems.Add(item);
				Items = nItems;
			}

			while (sw.Elapsed.TotalSeconds < 15)
			{
				IEnumerable<PoolAsyncItem> iWhere;

				if (onlyMemoryStorages)
					iWhere = Items.Where(i => i.Enabled && i.Mode.HasFlag(mode) && i.InMemoryStorage);
				else
					iWhere = Items.Where(i => i.Enabled && i.Mode.HasFlag(mode));

				if (SelectionOrder == PoolOrder.PingTime)
					iWhere = iWhere.OrderBy(i => i.PingTime);

				if (!ForceAtLeastOneNetworkItemEnabled && iWhere.Any())
					return iWhere.ToArray();

				var tmp = iWhere.ToArray();
				if (ForceAtLeastOneNetworkItemEnabled && tmp.Any(i => !i.InMemoryStorage))
					return tmp;

				if (onlyMemoryStorages)
					return tmp;

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
		public async Task<T> ReadAsync<T>(StorageFuncAsyncDelegate<T> function, ResponseConditionDelegate<T> responseCondition)
			=> (await ReadAsync(null, null, (PoolAsyncItem item, object a1, object a2) => function(item), responseCondition).ConfigureAwait(false)).Item1;
		/// <summary>
		/// Read action on the Pool.
		/// </summary>
		/// <typeparam name="T">Return value type</typeparam>
		/// <param name="function">Function to execute on the storage pool item</param>
		/// <param name="responseCondition">Function to check if the result is good or not.</param>
		/// <returns>Return value</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task<(T, PoolAsyncItem)> ReadAsync<T, A1>(A1 arg1, StorageFuncAsyncDelegate<T, A1> function, ResponseConditionDelegate<T> responseCondition)
			=> ReadAsync(arg1, null, (PoolAsyncItem item, A1 a1, object a2) => function(item, a1), responseCondition);
		/// <summary>
		/// Read action on the Pool.
		/// </summary>
		/// <typeparam name="T">Return value type</typeparam>
		/// <param name="function">Function to execute on the storage pool item</param>
		/// <param name="responseCondition">Function to check if the result is good or not.</param>
		/// <returns>Return value</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<(T, PoolAsyncItem)> ReadAsync<T, A1, A2>(A1 arg1, A2 arg2, StorageFuncAsyncDelegate<T, A1, A2> function, ResponseConditionDelegate<T> responseCondition)
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
					var response = await function(item, arg1, arg2).ConfigureAwait(false);
					if (responseCondition(ref response))
						return (response, item);
					if (!item.InMemoryStorage && ReadMode == PoolReadMode.FastestOnlyRead)
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
		/// <param name="action">Action to execute in the pool item</param>
		/// <param name="onlyMemoryStorages">Write only on memory storages</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task WriteAsync<A1>(A1 arg1, StorageActionAsyncDelegate<A1> action, bool onlyMemoryStorages = false)
			=> WriteAsync(arg1, null, null, null, 
		             (PoolAsyncItem item, A1 a1, object a2, object a3, object a4) => action(item, a1), onlyMemoryStorages);
		/// <summary>
		/// Write action on the Pool
		/// </summary>
		/// <param name="action">Action to execute in the pool item</param>
		/// <param name="onlyMemoryStorages">Write only on memory storages</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task WriteAsync<A1, A2>(A1 arg1, A2 arg2, StorageActionAsyncDelegate<A1, A2> action, bool onlyMemoryStorages = false)
			=> WriteAsync(arg1, arg2, null, null, 
		             (PoolAsyncItem item, A1 a1, A2 a2, object a3, object a4) => action(item, a1, a2), onlyMemoryStorages);
		/// <summary>
		/// Write action on the Pool
		/// </summary>
		/// <param name="action">Action to execute in the pool item</param>
		/// <param name="onlyMemoryStorages">Write only on memory storages</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task WriteAsync<A1, A2, A3>(ref A1 arg1, ref A2 arg2, ref A3 arg3, StorageActionAsyncDelegate<A1, A2, A3> action, bool onlyMemoryStorages = false)
			=> WriteAsync(arg1, arg2, arg3, null, 
		             (PoolAsyncItem item, A1 a1, A2 a2, A3 a3, object a4) => action(item, a1, a2, a3), onlyMemoryStorages);
		/// <summary>
		/// Write action on the Pool
		/// </summary>
		/// <param name="action">Action to execute in the pool item</param>
		/// <param name="onlyMemoryStorages">Write only on memory storages</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task WriteAsync<A1, A2, A3, A4>(A1 arg1, A2 arg2, A3 arg3, A4 arg4, StorageActionAsyncDelegate<A1, A2, A3, A4> action, bool onlyMemoryStorages = false)
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
						await action(item, arg1, arg2, arg3, arg4).ConfigureAwait(false);
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
						await action(item, arg1, arg2, arg3, arg4).ConfigureAwait(false);
						Core.Log.LibVerbose("\tAction executed on: '{0}'.", item.Name);
					}
					catch (Exception ex)
					{
						Core.Log.Error(ex, "\tAction Exception on '{0}': {1}", item.Name, ex.Message);
					}
					if (!item.InMemoryStorage)
						break;
				}
				idx++;
				if (idx < arrEnabled.Length) 
				{
					Worker.Enqueue(WorkerHandler, new WriteItem<A1, A2, A3, A4> { 
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
		/// <param name="function">Function to execute in the pool item</param>
		/// <param name="onlyMemoryStorages">Write only on memory storages</param>
		/// <returns>Return value from the function</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task<T> WriteAsync<T, A1>(A1 arg1, StorageFuncAsyncDelegate<T, A1> function, bool onlyMemoryStorages = false)
			=> WriteAsync(arg1, null, null, null, 
		             (PoolAsyncItem item, A1 a1, object a2, object a3, object a4) => function(item, a1), onlyMemoryStorages);
		/// <summary>
		/// Write action on the Pool
		/// </summary>
		/// <param name="function">Function to execute in the pool item</param>
		/// <param name="onlyMemoryStorages">Write only on memory storages</param>
		/// <returns>Return value from the function</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task<T> WriteAsync<T, A1, A2>(A1 arg1, A2 arg2, StorageFuncAsyncDelegate<T, A1, A2> function, bool onlyMemoryStorages = false)
			=> WriteAsync(arg1, arg2, null, null, 
		             (PoolAsyncItem item, A1 a1, A2 a2, object a3, object a4) => function(item, a1, a2), onlyMemoryStorages);
		/// <summary>
		/// Write action on the Pool
		/// </summary>
		/// <param name="function">Function to execute in the pool item</param>
		/// <param name="onlyMemoryStorages">Write only on memory storages</param>
		/// <returns>Return value from the function</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task<T> WriteAsync<T, A1, A2, A3>(A1 arg1, A2 arg2, A3 arg3, StorageFuncAsyncDelegate<T, A1, A2, A3> function, bool onlyMemoryStorages = false)
			=> WriteAsync(arg1, arg2, arg3, null, 
		         (PoolAsyncItem item, A1 a1, A2 a2, A3 a3, object a4) => function(item, a1, a2, a3), onlyMemoryStorages);
		/// <summary>
		/// Write action on the Pool
		/// </summary>
		/// <param name="function">Action to execute in the pool item</param>
		/// <param name="onlyMemoryStorages">Write only on memory storages</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<T> WriteAsync<T, A1, A2, A3, A4>(A1 arg1, A2 arg2, A3 arg3, A4 arg4, StorageFuncAsyncDelegate<T, A1, A2, A3, A4> function, bool onlyMemoryStorages = false)
		{
			Core.Log.LibVerbose("Queue Pool Action - WriteMode: {0}", WriteMode);
			var arrEnabled = WaitAndGetEnabled(StorageItemMode.Write, onlyMemoryStorages);

			T response = default(T);

			if (WriteMode == PoolWriteMode.WritesAllInSync)
			{
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
			}

			if (WriteMode == PoolWriteMode.WritesFirstAndThenAsync)
			{
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
					if (!item.InMemoryStorage)
						break;
				}
				idx++;
				if (idx < arrEnabled.Length) 
				{
					Worker.Enqueue(WorkerHandler, new WriteItem<A1, A2, A3, A4> { 
						Action = async (PoolAsyncItem item, A1 a1, A2 a2, A3 a3, A4 a4) => await function(item, a1, a2, a3, a4).ConfigureAwait(false), 
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

		static void WorkerHandler<A1, A2, A3, A4>(WriteItem<A1, A2, A3, A4> wItem)
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

        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            Worker?.Dispose();
            Worker = null;
            Items?.Each(i => i.Dispose());
            Items = null;
            Core.Status.DeAttachObject(this);
        }

        #endregion
    }
}
