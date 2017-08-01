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
    /// Cache pool item collection
    /// </summary>
    public class PoolItemCollection : IDisposable
    {
        ActionWorker Worker;
        internal List<PoolItem> Items;
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
            Worker = new ActionWorker();
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
        public void Add(PoolItem item)
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
        public List<PoolItem> WaitAndGetEnabled(StorageItemMode mode, bool onlyMemoryStorages = false)
        {
            List<PoolItem> lstEnabled = null;
            var sw = Stopwatch.StartNew();
            Core.Log.LibDebug($"Getting enabled cache for {mode}. [ForceAtLeastOneNetworkItemEnabled = {ForceAtLeastOneNetworkItemEnabled}]");

            //Sort index according the IndexOrder
            if (firstTime && SelectionOrder == PoolOrder.Index && IndexOrder?.Any() == true)
            {
                firstTime = false;
                var lstItems = new List<PoolItem>(Items);
                var nItems = new List<PoolItem>();
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
                IEnumerable<PoolItem> iWhere;
                
                if (onlyMemoryStorages)
                    iWhere = Items.Where(i => i.Enabled && i.Mode.HasFlag(mode) && i.InMemoryStorage);
                else
                    iWhere = Items.Where(i => i.Enabled && i.Mode.HasFlag(mode));

                if (SelectionOrder == PoolOrder.PingTime)
                    iWhere = iWhere.OrderBy(i => i.PingTime);

                if ((!ForceAtLeastOneNetworkItemEnabled && iWhere.Any()) || (ForceAtLeastOneNetworkItemEnabled && iWhere.Any(i => !i.InMemoryStorage)) || onlyMemoryStorages)
                {
                    lstEnabled = iWhere.ToList();
                    break;
                }
                Factory.Thread.Sleep(250);
            }

            if (lstEnabled == null || lstEnabled.Count == 0)
            {
                if (onlyMemoryStorages)
                {
                    Core.Log.Warning("The Cache Pool is configured to write network values to memory storages, but no memory storage were found or configurated.");
                }
                else
                {
                    Core.Log.Warning("Error looking for enabled Caches in Mode {0}. There is not connection to any cache server.", mode);
                    throw new Exception(string.Format("Error looking for enabled Caches in Mode {0}. There is not connection to any cache server. Please check configurations.", mode));
                }
            }
            return lstEnabled;

        }

        /// <summary>
        /// Read action on the Pool.
        /// </summary>
        /// <typeparam name="T">Return value type</typeparam>
        /// <param name="function">Function to execute on the storage pool item</param>
        /// <param name="responseCondition">Function to check if the result is good or not.</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(Func<PoolItem, T> function, Func<T, bool> responseCondition)
        {
            Core.Log.LibVerbose("Queue Pool Get - ReadMode: {0}", ReadMode);
            var items = WaitAndGetEnabled(StorageItemMode.Read);
            T response = default(T);
            bool resultFound = false;

            if (ReadMode == PoolReadMode.NormalRead || ReadMode == PoolReadMode.FastestOnlyRead)
            {
                foreach (var item in items)
                {
                    try
                    {
                        Core.Log.LibVerbose("\tLooking data in node: '{0}'.", item.Name);
                        response = function(item);
                        bool conditionResult = responseCondition(response);
                        if (conditionResult)
                        {
                            resultFound = true;
                            Core.Log.LibVerbose("\tFound in node: '{0}'.", item.Name);
                            break;
                        }
                        else if (!item.InMemoryStorage && ReadMode == PoolReadMode.FastestOnlyRead)
                            break;
                    }
                    catch (Exception ex)
                    {
                        Core.Log.LibVerbose("\tException on PoolGet '{0}'. Message: {1}", item.Name, ex.Message);
                    }
                }
            }
            if (!resultFound)
                Core.Log.LibVerbose("\tItem not Found in the pool.");
            return response;
        }
        /// <summary>
        /// Write action on the Pool
        /// </summary>
        /// <param name="action">Action to execute in the pool item</param>
        /// <param name="onlyMemoryStorages">Write only on memory storages</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Action<PoolItem> action, bool onlyMemoryStorages = false)
        {
            Core.Log.LibVerbose("Queue Pool Action - WriteMode: {0}", WriteMode);
            var lstEnabled = WaitAndGetEnabled(StorageItemMode.Write, onlyMemoryStorages);

            if (WriteMode == PoolWriteMode.WritesAllInSync)
            {
                foreach (var item in lstEnabled)
                {
                    try
                    {
                        action(item);
                        Core.Log.LibVerbose("\tAction executed on: '{0}'.", item.Name);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Error(ex, "\tAction Exception on '{0}': {1}", item.Name, ex.Message);
                    }
                }
            }
            else if (WriteMode == PoolWriteMode.WritesFirstAndThenAsync)
            {
                var writeList = new List<PoolItem>();
                foreach (var item in lstEnabled)
                {
                    writeList.Add(item);
                    if (!item.InMemoryStorage)
                        break;
                }
                var asyncList = lstEnabled.Skip(writeList.Count).ToList();
                foreach (var item in writeList)
                {
                    try
                    {
                        action(item);
                        Core.Log.LibVerbose("\tAction executed on: '{0}'.", item.Name);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Error(ex, "\tAction Exception on '{0}': {1}", item.Name, ex.Message);
                    }
                }
                Worker.Enqueue(() =>
                {
                    foreach (var item in asyncList)
                    {
                        try
                        {
                            if (item.Enabled)
                            {
                                action(item);
                                Core.Log.LibVerbose("\tAction executed on: '{0}'.", item.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Error(ex, "\tAction Exception on '{0}': {1}", item.Name, ex.Message);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Write action on the Pool
        /// </summary>
        /// <param name="function">Function to execute in the pool item</param>
        /// <param name="onlyMemoryStorages">Write only on memory storages</param>
        /// <returns>Return value from the function</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Write<T>(Func<PoolItem, T> function, bool onlyMemoryStorages = false)
        {
            Core.Log.LibVerbose("Queue Pool Action - WriteMode: {0}", WriteMode);
            var lstEnabled = WaitAndGetEnabled(StorageItemMode.Write, onlyMemoryStorages);

            T response = default(T);
            if (WriteMode == PoolWriteMode.WritesAllInSync)
            {
                foreach (var item in lstEnabled)
                {
                    try
                    {
                        response = function(item);
                        Core.Log.LibVerbose("\tAction executed on: '{0}'.", item.Name);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Error(ex, "\tAction Exception on '{0}': {1}", item.Name, ex.Message);
                    }
                }
            }
            else if (WriteMode == PoolWriteMode.WritesFirstAndThenAsync)
            {
                var writeList = new List<PoolItem>();
                foreach (var item in lstEnabled)
                {
                    writeList.Add(item);
                    if (!item.InMemoryStorage)
                        break;
                }
                var asyncList = lstEnabled.Skip(writeList.Count).ToList();
                foreach (var item in writeList)
                {
                    try
                    {
                        response = function(item);
                        Core.Log.LibVerbose("\tAction executed on: '{0}'.", item.Name);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Error(ex, "\tAction Exception on '{0}': {1}", item.Name, ex.Message);
                    }
                }
                Worker.Enqueue(() =>
                {
                    foreach (var item in asyncList)
                    {
                        try
                        {
                            if (item.Enabled)
                            {
                                function(item);
                                Core.Log.LibVerbose("\tAction executed on: '{0}'.", item.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Error(ex, "\tAction Exception on '{0}': {1}", item.Name, ex.Message);
                        }
                    }
                });
            }
            return response;
        }
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
