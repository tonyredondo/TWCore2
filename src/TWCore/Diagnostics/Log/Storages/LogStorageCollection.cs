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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace TWCore.Diagnostics.Log.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// A collection to write and read on multiple storages
    /// </summary>
    [StatusName("Storages")]
    public class LogStorageCollection : ILogStorage
    {
        /// <summary>
        /// All Log Storage Levels
        /// </summary>
        public const LogLevel AllLevels = LogLevel.Error | LogLevel.Warning | LogLevel.InfoBasic | LogLevel.InfoMedium | LogLevel.InfoDetail | LogLevel.Debug | LogLevel.Verbose | LogLevel.Stats | LogLevel.LibDebug | LogLevel.LibVerbose;
        private readonly object _locker = new object();
        private readonly List<(ILogStorage, LogLevel)> _items = new List<(ILogStorage, LogLevel)>();
        private readonly ReferencePool<List<Task>> _procTaskPool = new ReferencePool<List<Task>>();
        private LogLevel _lastMaxLogLevel = LogLevel.Error;
        private (ILogStorage, LogLevel)[] _cItems;
        private int _itemsCount;

        #region.ctor
        /// <summary>
        /// A collection to write and read on multiple storages
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogStorageCollection()
        {
            Core.Status.Attach(collection =>
            {
                var cItems = _cItems;
                foreach (var item in cItems)
                {
                    if (item.Item1 is null) continue;
                    collection.Add(item.Item1.ToString(), item.Item2);
                }
            });
        }
        #endregion

        #region Collection Methods
        /// <summary>
        /// Adds a new storage to the collection
        /// </summary>
        /// <param name="storage">Log storage object</param>
        /// <param name="writeLevel">Write level for the log storage</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ILogStorage storage, LogLevel writeLevel)
        {
            lock (_locker)
            {
                if (_items.All((i, mStorage) => i.Item1 != mStorage, storage))
                {
                    _items.Add((storage, writeLevel));
                    _cItems = _items.ToArray();
                    CalculateMaxLogLevel();
                    Interlocked.Exchange(ref _itemsCount, _items.Count);
                    Core.Status.AttachChild(storage, this);
                }
            }
        }
        /// <summary>
        /// Adds a new storage to the collection
        /// </summary>
        /// <param name="storage">Log storage object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ILogStorage storage)
        {
            Add(storage, AllLevels);
        }
        /// <summary>
        /// Get a storage item from a given type
        /// </summary>
        /// <param name="storageType">Type of Storage</param>
        /// <returns>Storage instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogStorage Get(Type storageType)
            => _items.FirstOrDefault((i, sType) => i.Item1.GetType().IsAssignableFrom(sType), storageType).Item1;
        /// <summary>
        /// Removes a existing storage from the collection
        /// </summary>
        /// <param name="storage">Storage instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ILogStorage storage)
        {
            lock (_locker)
            {
                if (_items.RemoveAll(i => i.Item1 == storage) > 0)
                {
                    _cItems = _items.ToArray();
                    CalculateMaxLogLevel();
                    Interlocked.Exchange(ref _itemsCount, _items.Count);
                }
            }
        }
        /// <summary>
        /// Change the log level of a given storage
        /// </summary>
        /// <param name="storage">Storage instance</param>
        /// <param name="level">New log level</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeStorageLogLevel(ILogStorage storage, LogLevel level)
        {
            if (_items.RemoveAll(i => i.Item1 == storage) > 0)
                Add(storage, level);
        }
        /// <summary>
        /// Gets the storage quantities inside the collection
        /// </summary>
        public int Count => _itemsCount;
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
                Interlocked.Exchange(ref _itemsCount, _items.Count);
                CalculateMaxLogLevel();
            }
        }
        /// <summary>
        /// Get all storages
        /// </summary>
        /// <returns>ILogStorage array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogStorage[] GetAllStorages()
        {
            lock (_locker)
                return _items.Select(i => i.Item1).ToArray() ?? Array.Empty<ILogStorage>();
        }
        /// <summary>
        /// Get Max LogLevel in Storages
        /// </summary>
        /// <returns>LogLevel</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogLevel GetMaxLogLevel() => _lastMaxLogLevel;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CalculateMaxLogLevel()
        {
            var cItems = _items;
            var maxError = LogLevel.Error;
            foreach(var i in cItems)
            {
                if (i.Item2 > maxError)
                    maxError = i.Item2;
            }
            _lastMaxLogLevel = maxError;
        }
        #endregion

        #region ILogStorage methods
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task WriteAsync(ILogItem item)
        {
            var cItems = _cItems;
            var tsks = _procTaskPool.New();
            for (var i = 0; i < cItems.Length; i++)
            {
                if ((cItems[i].Item2 & item.Level) != 0)
                    tsks.Add(InternalWriteAsync(cItems[i].Item1, item));
            }
            await Task.WhenAll(tsks).ConfigureAwait(false);
            tsks.Clear();
            _procTaskPool.Store(tsks);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async Task InternalWriteAsync(ILogStorage storage, ILogItem logItem)
        {
            try
            {
                await storage.WriteAsync(logItem).ConfigureAwait(false);
            }
            catch
            {
                //
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task WriteEmptyLineAsync()
        {
            var cItems = _cItems;
            foreach (var sto in cItems)
            {
                try
                {
                    await sto.Item1.WriteEmptyLineAsync().ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }
            }
        }
        /// <summary>
        /// Writes a group metadata item to the storage
        /// </summary>
        /// <param name="item">Group metadata item</param>
        /// <returns>Task process</returns>
        public async Task WriteAsync(IGroupMetadata item)
        {
            var cItems = _cItems;
            foreach (var sto in cItems)
            {
                try
                {
                    await sto.Item1.WriteAsync(item).ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }
            }
        }

        #endregion

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls
        /// <summary>
        /// Dispose the current object resources
        /// </summary>
        /// <param name="disposing">Value if the object needs to be disposed or not</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                lock (_locker)
                {
                    foreach (var t in _items)
                    {
                        try
                        {
                            t.Item1.Dispose();
                        }
                        catch (Exception e)
                        {
                            Core.Log.Write(e);
                        }
                    }
                    _items?.Clear();
                    _cItems = null;
                }
            }
            _disposedValue = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Dispose the current object resources
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
