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
using System.Threading.Tasks;

namespace TWCore.Diagnostics.Trace.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// A collection to write and read on multiple storages
    /// </summary>
    public class TraceStorageCollection : ITraceStorage
    {
        private readonly object _locker = new object();
        private readonly List<ITraceStorage> _items = new List<ITraceStorage>();
        private List<ITraceStorage> _cItems;
        private volatile bool _isDirty;

        #region .ctor
        /// <summary>
        /// A collection to write and read on multiple storages
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TraceStorageCollection()
        {
            Core.Status.Attach(collection =>
            {
                collection.Add("Items", _items.Join(", "));
                foreach (var i in _items)
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
            lock (_locker)
            {
                _items.Add(storage);
                _isDirty = true;
            }
        }
        /// <summary>
        /// Gets the storage quantities inside the collection
        /// </summary>
        public int Count => _items?.Count ?? 0;
        /// <summary>
        /// Clears the collection
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            lock (_locker)
            {
                _items.Clear();
                _isDirty = true;
            }
        }
        #endregion

        #region ITraceStorage Members
        /// <inheritdoc />
        /// <summary>
        /// Writes a trace item to the storage
        /// </summary>
        /// <param name="item">Trace item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task WriteAsync(TraceItem item)
        {
            if (_isDirty || _cItems == null)
            {
                if (_items == null) return;
                lock (_locker)
                {
                    _cItems = new List<ITraceStorage>(_items);
                }
            }
            var tsk = _cItems.Select(async i =>
            {
                try
                {
                    await i.WriteAsync(item).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            });
            await Task.WhenAll(tsk).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all the object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            lock (_locker)
                _items.Clear();
        }
        #endregion
    }
}
