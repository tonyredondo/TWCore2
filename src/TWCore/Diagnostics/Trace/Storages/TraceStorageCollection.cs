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

namespace TWCore.Diagnostics.Trace.Storages
{
    /// <summary>
    /// A collection to write and read on multiple storages
    /// </summary>
    public class TraceStorageCollection : ITraceStorage
    {
        readonly object locker = new object();
        volatile bool IsDirty;
        readonly List<ITraceStorage> Items = new List<ITraceStorage>();
        List<ITraceStorage> CItems = null;

        #region .ctor
        /// <summary>
        /// A collection to write and read on multiple storages
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TraceStorageCollection()
        {
            Core.Status.Attach(collection =>
            {
                collection.Add("Items", Items.Join(", "));
                foreach (var i in Items)
                    Core.Status.AttachChild(i, this);
            });
        }
        #endregion

        #region Collection Methods
        /// <summary>
        /// Adds a new storage to the collection
        /// </summary>
        /// <param name="storage">Trace storage object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ITraceStorage storage)
        {
            lock (locker)
            {
                Items.Add(storage);
                IsDirty = true;
            }
        }
        /// <summary>
        /// Gets the storage quantities inside the collection
        /// </summary>
        public int Count => Items?.Count ?? 0;
        /// <summary>
        /// Clears the collection
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            lock (locker)
            {
                Items.Clear();
                IsDirty = true;
            }
        }
        #endregion

        #region ITraceStorage Members
        /// <summary>
        /// Writes a trace item to the storage
        /// </summary>
        /// <param name="item">Trace item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(TraceItem item)
        {
            if (Items == null) return;
            lock(locker)
            {
                if (IsDirty || CItems == null)
                    CItems = new List<ITraceStorage>(Items);
            }
            foreach(var i in CItems)
            {
                try
                {
                    i.Write(item);
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
        }
        /// <summary>
        /// Dispose all the object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            lock (locker)
                Items.Clear();
        }
        #endregion
    }
}
