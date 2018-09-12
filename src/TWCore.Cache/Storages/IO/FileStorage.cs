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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Serialization;
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable ReturnTypeCanBeEnumerable.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace TWCore.Cache.Storages.IO
{
    /// <inheritdoc />
    /// <summary>
    /// File Cache Storage
    /// </summary>
    [StatusName("File Cache Storage")]
    public class FileStorage : StorageBase
    {
        private FolderHandler[] _handlers;
        private ConcurrentDictionary<string, StorageItemMeta> _metas;
        
        #region Properties
        /// <summary>
        /// Base path where the data is going to be saved.
        /// </summary>
        public string BasePath { get; set; }
        /// <summary>
        /// Gets the serializer in use to write the index file
        /// </summary>
        public BinarySerializer IndexSerializer { get; set; } = (BinarySerializer)SerializerManager.DefaultBinarySerializer;
        /// <summary>
        /// Number of subfolders to sparse the files.
        /// </summary>
        public byte NumberOfSubFolders { get; set; } = 50;
        /// <summary>
        /// Number of transaction before writing all the index file
        /// </summary>
        public int TransactionLogThreshold { get; set; } = 250;
        /// <summary>
        /// Maximún number of elements waiting for write before starting to slow down the storage to free the queue
        /// </summary>
        public int SlowDownWriteThreshold { get; set; } = 3000;
        /// <inheritdoc />
        /// <summary>
        /// Gets the Storage Type
        /// </summary>
        /// <value>The type.</value>
        public override StorageType Type => StorageType.File;
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// File Cache Storage
        /// </summary>
        public FileStorage(string basePath)
        {
            basePath = Factory.ResolveLowLowPath(basePath);
            BasePath = basePath;
            _metas = new ConcurrentDictionary<string, StorageItemMeta>();
            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(BasePath), BasePath);
                collection.Add("Count", _handlers?.Sum(i => i.Count) ?? 0, StatusItemValueStatus.Ok);
                collection.Add(nameof(IndexSerializer), IndexSerializer);
                collection.Add(nameof(NumberOfSubFolders), NumberOfSubFolders, NumberOfSubFolders > 10 ? StatusItemValueStatus.Ok : NumberOfSubFolders > 2 ? StatusItemValueStatus.Warning : StatusItemValueStatus.Error);
                collection.Add(nameof(TransactionLogThreshold), TransactionLogThreshold);
                collection.Add(nameof(SlowDownWriteThreshold), SlowDownWriteThreshold);
                if (_handlers is null) return;
                foreach (var hnd in _handlers)
                    Core.Status.AttachChild(hnd, this);
            }, this);
        }
        #endregion

        #region Private Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetFolderNumber(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;
            return (int)(key.GetMurmurHash2() % NumberOfSubFolders);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task InitAsync(CancellationToken cancellationToken = default)
        {
            Ensure.ReferenceNotNull(BasePath, "The FileStorage BasePath, is null.");
            Core.Log.InfoBasic("Initializing FileStorage...");
            if (_handlers?.Any() == true)
            {
                Core.Log.InfoBasic("Disposing previous instances...");
                foreach(var hnd in _handlers)
                    await hnd.DisposeAsync(true).ConfigureAwait(false);
                _handlers = null;
            }
            if (!Directory.Exists(BasePath))
            {
                Core.Log.InfoBasic("Creating base folder");
                Directory.CreateDirectory(BasePath);
            }
            Core.Log.InfoBasic("Configuring {0} Subfolders", NumberOfSubFolders);
            _handlers = new FolderHandler[NumberOfSubFolders];
            var loadTasks = new Task[NumberOfSubFolders];
            for (var i = 0; i < NumberOfSubFolders; i++)
            {
                var folder = Path.Combine(BasePath, i.ToString("00"));
                Core.Log.InfoBasic("Initializing Subfolder: {0} on {1}", i, folder);
                _handlers[i] = new FolderHandler(this, _metas, folder);
                loadTasks[i] = _handlers[i].LoadAsync(cancellationToken);
            }

            Core.Log.InfoBasic("Waiting the folder handlers to be loaded.");
            await Task.WhenAll(loadTasks).ConfigureAwait(false);

            if (_handlers.Any(hnd => hnd.Status != FolderHandlerStatus.Loaded))
            {
                Core.Log.Error("There were some errors loading folders, the Storage can be loaded.");
                return;
            }

            Core.Log.InfoBasic("All folder handlers are loaded, Index Count: {0}", _handlers.Sum(i => i.Count));
            SetReady(true);
        }

        #endregion

        #region Overrides
        /// <inheritdoc />
        /// <summary>
        /// Init this storage
        /// </summary>
        protected override void OnInit()
        {
            InitAsync().WaitAsync();
        }
        /// <summary>
        /// Storage items meta
        /// </summary>
        protected override IEnumerable<StorageItemMeta> Metas
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _metas.Values;
        }
        /// <summary>
        /// Gets if the storage has a key
        /// </summary>
        /// <param name="key">Key value</param>
        /// <returns>True if the storage contains a value for the key; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnExistKey(string key)
            => _metas.ContainsKey(key);
        /// <summary>
        /// Gets all the keys on the storage
        /// </summary>
        /// <returns>IEnumerable with all the keys</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IEnumerable<string> OnGetKeys()
            => _metas.Keys;
        /// <summary>
        /// Removes an item from the storage
        /// </summary>
        /// <param name="key">Key to remove</param>
        /// <param name="meta">Meta of the item</param>
        /// <returns>True if the item was successfully removed from the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnRemove(string key, out StorageItemMeta meta)
            => _handlers[GetFolderNumber(key)].TryRemove(key, out meta);
        /// <summary>
        /// Sets and creates an item on the storage
        /// </summary>
        /// <param name="meta">Item metadata to store</param>
        /// <param name="value">Item data to store</param>
        /// <returns>True if the item was successfully stored in the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnSet(StorageItemMeta meta, SerializedObject value)
            => _handlers[GetFolderNumber(meta.Key)].TrySet(meta, value);
        /// <summary>
        /// Tries to get an item from the storage
        /// </summary>
        /// <param name="key">Key of the item</param>
        /// <param name="value">StorageItem of the key</param>
        /// <param name="condition">Condition to evaluate before getting the key from the storage</param>
        /// <returns>True if the item was successfully loaded from the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnTryGet(string key, out StorageItem value, Predicate<StorageItemMeta> condition = null)
            => _handlers[GetFolderNumber(key)].TryGet(key, out value, condition);
        /// <summary>
        /// Tries to get the metadata from a item
        /// </summary>
        /// <param name="key">Key of the item</param>
        /// <param name="value">StorageItem metadata of the key</param>
        /// <param name="condition">Condition to evaluate before getting the key from the storage</param>
        /// <returns>True if the item was successfully loaded from the storage; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnTryGetMeta(string key, out StorageItemMeta value, Predicate<StorageItemMeta> condition = null)
        {
            value = null;
            if (!_metas.TryGetValue(key, out var metaValue)) return false;
            if (metaValue != null && !metaValue.IsExpired && (condition is null || condition(metaValue)))
            {
                value = metaValue;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Dispose all instance resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnDispose()
        {
            if (_handlers is null) return;
            Core.Log.InfoBasic("Disposing...");
            Task.WaitAll(_handlers.Select(hnd => hnd.DisposeAsync(true)).ToArray());
            _metas.Clear();
            _handlers = null;
            Core.Log.InfoBasic("Disposed.");
        }
        #endregion
    }
}
