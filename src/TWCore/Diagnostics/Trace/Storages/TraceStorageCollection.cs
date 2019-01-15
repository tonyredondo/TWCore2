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
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;

namespace TWCore.Diagnostics.Trace.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// A collection to write and read on multiple storages
    /// </summary>
    [StatusName("Storages")]
    public class TraceStorageCollection : ITraceStorage
    {
        private readonly object _locker = new object();
        private readonly List<ITraceStorage> _items = new List<ITraceStorage>();
        private readonly ReferencePool<List<Task>> _procTaskPool = new ReferencePool<List<Task>>();
        private ITraceStorage[] _cItems;

        #region .ctor
        /// <summary>
        /// A collection to write and read on multiple storages
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TraceStorageCollection()
        {
            Core.Status.Attach(collection =>
            {
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
                _cItems = _items.ToArray();
            }
        }
        /// <summary>
        /// Gets the storage quantities inside the collection
        /// </summary>
        public int Count => _cItems?.Length ?? 0;
        /// <summary>
        /// Clears the collection
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            lock (_locker)
            {
                _items.Clear();
                _cItems = _items.ToArray();
            }
        }
        /// <summary>
        /// Get all storages
        /// </summary>
        /// <returns>ITraceStorage array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITraceStorage[] GetAllStorages()
        {
            return _cItems ?? Array.Empty<ITraceStorage>();
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
            var cItems = _cItems;
            var tsks = _procTaskPool.New();
            for (var i = 0; i < cItems.Length; i++)
                tsks.Add(InternalWriteAsync(_cItems[i], item));
            await Task.WhenAll(tsks).ConfigureAwait(false);
            tsks.Clear();
            _procTaskPool.Store(tsks);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async Task InternalWriteAsync(ITraceStorage storage, TraceItem item)
        {
            try
            {
                await storage.WriteAsync(item).ConfigureAwait(false);
            }
            catch
            {
                //
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all the object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            lock (_locker)
            {
                _items.Clear();
                _cItems = null;
            }
        }
        #endregion
    }
}
