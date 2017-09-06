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
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable IntroduceOptionalParameters.Global

namespace TWCore.Diagnostics.Log.Storages
{
    /// <inheritdoc />
    /// <summary>
    /// A collection to write and read on multiple storages
    /// </summary>
    public class LogStorageCollection : ILogStorage
    {
        public const LogLevel AllLevels = LogLevel.Error | LogLevel.Warning | LogLevel.InfoBasic | LogLevel.InfoMedium | LogLevel.InfoDetail | LogLevel.Debug | LogLevel.Verbose | LogLevel.Stats | LogLevel.LibDebug | LogLevel.LibVerbose;
        private readonly object _locker = new object();
        private readonly List<Tuple<ILogStorage, LogLevel>> _items = new List<Tuple<ILogStorage, LogLevel>>();
        private volatile bool _isDirty;
        private LogLevel _lastMaxLogLevel = LogLevel.Error;
        private List<Tuple<ILogStorage, LogLevel>> _cItems;

        #region.ctor
        /// <summary>
        /// A collection to write and read on multiple storages
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogStorageCollection()
        {
            Core.Status.Attach(collection =>
            {
                collection.Add("Items", _items.Select(i => i.Item1 + "[" + i.Item2 + "]").Join(", "));
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
                if (_items.All(i => i.Item1 != storage))
                {
                    _items.Add(Tuple.Create(storage, writeLevel));
                    _isDirty = true;
                    CalculateMaxLogLevel();
                }
            }
            Core.Status.AttachChild(storage, this);
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
            => _items.FirstOrDefault(i => i.Item1.GetType().IsAssignableFrom(storageType))?.Item1;
        /// <summary>
        /// Removes a existing storage from the collection
        /// </summary>
        /// <param name="storage">Storage instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ILogStorage storage)
        {
            _items.RemoveAll(i => i.Item1 == storage);
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
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_locker)
                    return _items.Count;
            }
        }
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
                return _items.Select(i => i.Item1).ToArray() ?? new ILogStorage[0];
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
            var levels = _items.Select(i => i.Item2).ToArray();
            _lastMaxLogLevel = levels.Length > 0 ? levels.Max() : LogLevel.Error;
        }
        #endregion

        #region ILogStorage methods
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ILogItem item)
        {
            if (_items == null) return;
            if (_isDirty || _cItems == null)
            {
                lock (_locker)
                {
                    _cItems = new List<Tuple<ILogStorage, LogLevel>>(_items);
                    _isDirty = false;
                }
            }
            foreach (var sto in _cItems)
            {
                try
                {
                    if (sto.Item2.HasFlag(item.Level))
                        sto.Item1.Write(item);
                }
                catch
                {
                    // ignored
                }
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEmptyLine()
        {
            if (_isDirty || _cItems == null)
            {
                lock (_locker)
                {
                    _cItems = new List<Tuple<ILogStorage, LogLevel>>(_items);
                    _isDirty = false;
                }
            }
            foreach (var sto in _cItems)
            {
                try
                {
                    sto.Item1.WriteEmptyLine();
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
                    _items.ParallelEach(t => Try.Do(() => t.Item1.Dispose(), false));
                    _items?.Clear();
                    _cItems?.Clear();
                    _isDirty = true;
                }
            }
            _disposedValue = true;
        }

        /// <summary>
        /// LogStorage destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~LogStorageCollection()
        {
            Dispose(false);
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
