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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Serialization;
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable ReturnTypeCanBeEnumerable.Local

namespace TWCore.Cache.Storages.IO
{
    /// <inheritdoc />
    /// <summary>
    /// File Cache Storage
    /// </summary>
    public class FileStorage : StorageBase
    {
        private FolderStorage[] _storages;

        #region Properties
        /// <summary>
        /// Base path where the data is going to be saved.
        /// </summary>
        public string BasePath { get; set; }
        /// <summary>
        /// Gets or sets the serializer to use when writting to disk
        /// </summary>
        public BinarySerializer Serializer { get; set; } = (BinarySerializer)SerializerManager.DefaultBinarySerializer;
        /// <summary>
        /// Gets the json serializer in use to write the meta file
        /// </summary>
        public BinarySerializer MetaSerializer { get; set; } = (BinarySerializer)SerializerManager.DefaultBinarySerializer;
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
        public int SlowDownWriteThreshold { get; set; } = 1000;
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
            BasePath = basePath;
            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(BasePath), BasePath);
                collection.Add("Count", Metas.Count(), StatusItemValueStatus.Green);
                collection.Add(nameof(Serializer), Serializer);
                collection.Add(nameof(MetaSerializer), MetaSerializer);
                collection.Add(nameof(NumberOfSubFolders), NumberOfSubFolders, NumberOfSubFolders > 10 ? StatusItemValueStatus.Green : NumberOfSubFolders > 2 ? StatusItemValueStatus.Yellow : StatusItemValueStatus.Red);
                collection.Add(nameof(TransactionLogThreshold), TransactionLogThreshold);
                collection.Add(nameof(SlowDownWriteThreshold), SlowDownWriteThreshold);
                if (_storages == null) return;
                foreach (var sto in _storages)
                    Core.Status.AttachChild(sto, this);
            });
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetFolderNumber(string key)
            => (int?)(key?.GetJenkinsHash() % NumberOfSubFolders) ?? 0;
        #endregion

        #region Overrides
        /// <inheritdoc />
        /// <summary>
        /// Init this storage
        /// </summary>
        protected override void OnInit()
        {
            Ensure.ReferenceNotNull(BasePath, "The FileStorage BasePath, is null.");
            Core.Log.InfoBasic("Initializing FileStorage...");
            if (_storages?.Any() == true)
            {
                Core.Log.InfoBasic("Disposing previous instances...");
                _storages.Each(fsto => fsto.Dispose());
                _storages = null;
            }
            if (!Directory.Exists(BasePath))
            {
                Core.Log.InfoBasic("Creating base folder");
                Directory.CreateDirectory(BasePath);
            }
            Core.Log.InfoBasic("Configuring {0} Subfolders", NumberOfSubFolders);
            _storages = new FolderStorage[NumberOfSubFolders];
            for (var i = 0; i < NumberOfSubFolders; i++)
            {
                var folder = Path.Combine(BasePath, i.ToString());
                Core.Log.InfoBasic("Initializing Subfolder: {0} on {1}", i, folder);
                _storages[i] = new FolderStorage(folder, this);
            }
            Core.Log.InfoBasic("Waiting the storages to be loaded.");
            while (_storages.Any(s => !s.Loaded))
                Thread.Sleep(100);
            Core.Log.InfoBasic("All folder storages are loaded, Index Count: {0}", Metas.Count());
            SetReady(true);
        }
        protected override IEnumerable<StorageItemMeta> Metas
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _storages.SelectMany(s => s.Metas).ToArray();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnExistKey(string key)
            => _storages[GetFolderNumber(key)].OnExistKey(key);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string[] OnGetKeys()
            => _storages.SelectMany(s => s.OnGetKeys()).ToArray();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnRemove(string key, out StorageItemMeta meta)
            => _storages[GetFolderNumber(key)].OnRemove(key, out meta);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnSet(StorageItemMeta meta, SerializedObject value)
            => _storages[GetFolderNumber(meta.Key)].OnSet(meta, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnTryGet(string key, out StorageItem value, Predicate<StorageItemMeta> condition = null)
            => _storages[GetFolderNumber(key)].OnTryGet(key, out value, condition);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnTryGetMeta(string key, out StorageItemMeta value, Predicate<StorageItemMeta> condition = null)
            => _storages[GetFolderNumber(key)].OnTryGetMeta(key, out value, condition);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnDispose()
        {
            if (_storages == null) return;
            Core.Log.InfoBasic("Disposing...");
            _storages.ParallelEach(s => s.Dispose());
            _storages = null;
        }
        #endregion

        #region Inner Types
        private sealed class FolderStorage : IDisposable
        {
            private static readonly byte[] BytesEmpty = new byte[0];
            private const string IndexFileName = "Index";
            private const string TransactionLogFileName = "Index.journal";
            private const string DataExtension = ".data";

            #region Fields
            // ReSharper disable once NotAccessedField.Local
            private Task _loadTask;
            private Dictionary<string, StorageItemMeta> _metas;
            private int _currentTransactionLogLength;
            private FileStream _transactionStream;
            private readonly object _pendingLock = new object();
            private readonly object _metasLock = new object();
            private readonly Dictionary<string, SerializedObject> _pendingItems;
            private readonly Worker<(StorageItemMeta, FileStorageMetaLog.TransactionType)> _storageWorker;
            private readonly string _transactionLogFilePath;
            private readonly string _indexFilePath;
            private readonly FileStorageMetaLog _currentTransaction;
            #endregion

            #region Properties
            /// <summary>
            /// Base path where the data is going to be saved.
            /// </summary>
            public string BasePath { get; private set; }
            /// <summary>
            /// Gets or sets the serializer to use when writting to disk
            /// </summary>
            public BinarySerializer Serializer { get; private set; }
            /// <summary>
            /// Gets the serializer in use to write the index file
            /// </summary>
            public BinarySerializer IndexSerializer { get; private set; }
            /// <summary>
            /// Number of transaction before writing all the index file
            /// </summary>
            public int TransactionLogThreshold { get; private set; }
            /// <summary>
            /// Maximún number of elements waiting for write before starting to slow down the storage to free the queue
            /// </summary>
            public int SlowDownWriteThreshold { get; private set; }
            /// <summary>
            /// Gets a value indicating whether this <see cref="T:TWCore.Cache.Storages.IO.NFileStorage.FolderStorage"/> is loaded.
            /// </summary>
            /// <value><c>true</c> if loaded; otherwise, <c>false</c>.</value>
            public bool Loaded { get; private set; }
            /// <summary>
            /// Gets a value indicating whether this <see cref="T:TWCore.Cache.Storages.IO.NFileStorage.FolderStorage"/> loading failed.
            /// </summary>
            /// <value><c>true</c> if loading failed; otherwise, <c>false</c>.</value>
            public bool LoadingFailed { get; private set; }
            #endregion

            #region .ctor
            public FolderStorage(string basePath, FileStorage storage)
            {
                var saveMetadataBuffered = ActionDelegate.Create(SaveMetadata).CreateBufferedAction(1000);
                BasePath = basePath;
                Serializer = storage.Serializer;
                IndexSerializer = storage.MetaSerializer;
                TransactionLogThreshold = storage.TransactionLogThreshold;
                SlowDownWriteThreshold = storage.SlowDownWriteThreshold;
                Loaded = false;
                LoadingFailed = false;
                _transactionLogFilePath = Path.Combine(BasePath, TransactionLogFileName + IndexSerializer.Extensions[0]);
                var oldTransactionLogFilePath = _transactionLogFilePath + ".old";
                _indexFilePath = Path.Combine(BasePath, IndexFileName + IndexSerializer.Extensions[0]);
                var oldindexFilePath = _indexFilePath + ".old";
                List<FileStorageMetaLog> transactionLog = null;
                _metas = new Dictionary<string, StorageItemMeta>();
                _pendingItems = new Dictionary<string, SerializedObject>();
                var tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;
                _storageWorker = new Worker<(StorageItemMeta, FileStorageMetaLog.TransactionType)>(WorkerProcess)
                {
                    EnableWaitTimeout = false
                };
                _storageWorker.OnWorkDone += (sender, e) => saveMetadataBuffered();
                _currentTransaction = new FileStorageMetaLog();

                if (!Directory.Exists(BasePath))
                {
                    Core.Log.InfoBasic("Creating SubFolder Directory: {0}", BasePath);
                    Directory.CreateDirectory(BasePath);
                }
                //Checks to avoid errors
                if (!File.Exists(_transactionLogFilePath))
                    File.WriteAllBytes(_transactionLogFilePath, BytesEmpty);
                if (!File.Exists(_indexFilePath))
                    IndexSerializer.SerializeToFile(new List<StorageItemMeta>(), _indexFilePath);

                //Start...
                _loadTask = Task.Run(() =>
                {
                    lock (_metasLock)
                        lock (_pendingLock)
                        {
                            try
                            {
                                if (token.IsCancellationRequested) return;

                                #region Loading index file
                                var indexLoaded = false;
                                if (File.Exists(_indexFilePath))
                                {
                                    try
                                    {
                                        Core.Log.InfoBasic("Loading Index File: {0}", _indexFilePath);
                                        var index = IndexSerializer.DeserializeFromFile<List<StorageItemMeta>>(_indexFilePath);
                                        if (index != null)
                                        {
                                            _metas = index.ToDictionary(k => k.Key, v => v);
                                            indexLoaded = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Core.Log.Write(ex);
                                    }
                                }

                                if (!indexLoaded)
                                {
                                    try
                                    {
                                        if (File.Exists(oldindexFilePath))
                                        {
                                            Core.Log.Warning("Trying to load old copy of the index file: {0}", oldindexFilePath);
                                            var index = IndexSerializer.DeserializeFromFile<List<StorageItemMeta>>(oldindexFilePath);
                                            if (index != null)
                                            {
                                                _metas = index.ToDictionary(k => k.Key, v => v);
                                                indexLoaded = true;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Core.Log.Write(ex);
                                    }
                                }

                                if (!indexLoaded)
                                {
                                    Core.Log.Warning("The index doesn't exist or couldn't be loaded. Generating new index file.");
                                    var dateNow = Core.Now;
                                    var eTime = dateNow.AddDays(5);
                                    if (storage.MaximumItemDuration.HasValue)
                                        eTime = dateNow.Add(storage.MaximumItemDuration.Value);
                                    if (storage.ItemsExpirationDateOverwrite.HasValue)
                                        eTime = dateNow.Add(storage.ItemsExpirationDateOverwrite.Value);
                                    if (storage.ItemsExpirationAbsoluteDateOverwrite.HasValue)
                                        eTime = storage.ItemsExpirationAbsoluteDateOverwrite.Value;

                                    if (_metas == null) _metas = new Dictionary<string, StorageItemMeta>();

                                    var allFiles = Directory.EnumerateFiles(BasePath, "*" + DataExtension, SearchOption.AllDirectories);
                                    var idx = 0;
                                    foreach (var file in allFiles)
                                    {
                                        var cTime = File.GetCreationTime(file);
                                        var key = Path.GetFileNameWithoutExtension(file);
                                        _metas.Add(key, new StorageItemMeta { Key = key, CreationDate = cTime, ExpirationDate = eTime });
                                        if (idx % 100 == 0)
                                            Core.Log.InfoBasic("Number of files loaded: {0}", idx);
                                        idx++;
                                    }
                                    Core.Log.InfoBasic("Index generated...");
                                    SaveMetadata();
                                }
                                #endregion

                                #region Loading transaction log file
                                var transactionLogLoaded = false;
                                if (File.Exists(_transactionLogFilePath))
                                {
                                    try
                                    {
                                        Core.Log.InfoBasic("Loading Transaction Log File: {0}", _transactionLogFilePath);
                                        var lstTransactions = new List<FileStorageMetaLog>();
                                        using (var fStream = File.Open(_transactionLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                        {
                                            while (fStream.Position != fStream.Length)
                                            {
                                                var item = IndexSerializer.Deserialize<FileStorageMetaLog>(fStream);
                                                if (item != null)
                                                    lstTransactions.Add(item);
                                            }
                                        }
                                        transactionLog = lstTransactions;
                                        transactionLogLoaded = true;
                                        File.Copy(_transactionLogFilePath, oldTransactionLogFilePath, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        Core.Log.Write(ex);
                                    }
                                }

                                if (!transactionLogLoaded)
                                {
                                    try
                                    {
                                        if (File.Exists(oldTransactionLogFilePath))
                                        {
                                            Core.Log.Warning("Trying to load old copy of the transaction log file: {0}", oldTransactionLogFilePath);
                                            var lstTransactions = new List<FileStorageMetaLog>();
                                            using (var fStream = File.Open(oldTransactionLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                            {
                                                while (fStream.Position != fStream.Length)
                                                {
                                                    var item = IndexSerializer.Deserialize<FileStorageMetaLog>(fStream);
                                                    if (item != null)
                                                        lstTransactions.Add(item);
                                                }
                                            }
                                            transactionLog = lstTransactions;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Core.Log.Write(ex);
                                    }
                                }
                                #endregion

                                #region Applying pending transactions
                                if (transactionLog?.Count > 0)
                                {
                                    Core.Log.InfoBasic("Applying {0} pending transactions", transactionLog.Count);
                                    foreach (var item in transactionLog)
                                    {
                                        switch (item.Type)
                                        {
                                            case FileStorageMetaLog.TransactionType.Add:
                                                _metas[item.Meta.Key] = item.Meta;
                                                break;
                                            case FileStorageMetaLog.TransactionType.Remove:
                                                if (!_metas.TryGetValue(item.Meta.Key, out var oldMeta)) continue;
                                                _metas.Remove(item.Meta.Key);
                                                oldMeta.Dispose();
                                                break;
                                        }
                                    }
                                    SaveMetadata();
                                }
                                #endregion

                                RemoveExpiredItems();
                                _metas.Each(m => m.Value.OnExpire += Meta_OnExpire);

                                _transactionStream = File.Open(_transactionLogFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

                                Core.Log.InfoBasic("Total item loaded: {0}", _metas.Count);
                                Core.Log.LibVerbose("All metadata loaded.");
                                Loaded = true;
                            }
                            catch (Exception ex)
                            {
                                Core.Log.Write(ex);
                                LoadingFailed = true;
                            }
                        }
                }, token);

                Core.Status.Attach(collection =>
                {
                    var percentWork = (double)_storageWorker.Count / SlowDownWriteThreshold;
                    collection.Add(nameof(BasePath), BasePath);
                    collection.Add("Count", _metas.Count, StatusItemValueStatus.Green);
                    collection.Add("Pending Count", _pendingItems.Count, StatusItemValueStatus.Green);
                    collection.Add("Worker Count", _storageWorker.Count, percentWork < 0.8 ? StatusItemValueStatus.Green : percentWork < 0.95 ? StatusItemValueStatus.Yellow : StatusItemValueStatus.Red);
                    collection.Add("Worker Process Percent", Math.Round(percentWork * 100, 2) + "%", percentWork < 0.8 ? StatusItemValueStatus.Green : percentWork < 0.95 ? StatusItemValueStatus.Yellow : StatusItemValueStatus.Red);
                    collection.Add("Transaction Log Length", _currentTransactionLogLength, StatusItemValueStatus.Green);
                    collection.Add("Index File", _indexFilePath, StatusItemValueStatus.Green);
                    collection.Add("Transaction File", _transactionLogFilePath, StatusItemValueStatus.Green);
                });
            }
            #endregion

            #region Private Methods
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private string GetDataPath(string key) => Path.Combine(BasePath, key + DataExtension + Serializer.Extensions[0]);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WorkerProcess((StorageItemMeta, FileStorageMetaLog.TransactionType) workerItem)
            {
                var meta = workerItem.Item1;
                var transaction = workerItem.Item2;
                if (transaction == FileStorageMetaLog.TransactionType.Add && meta.IsExpired) return;
                if (_currentTransactionLogLength >= TransactionLogThreshold) _currentTransactionLogLength = 0;

                #region Save Transaction Log
                try
                {
                    lock (_transactionStream)
                    {
                        Try.Do(() => File.Copy(_transactionLogFilePath, _transactionLogFilePath + ".old", true), ex => Core.Log.Warning("The Transaction log copy can't be created: {0}", ex.Message));
                        _currentTransaction.Meta = meta;
                        _currentTransaction.Type = transaction;
                        IndexSerializer.Serialize(_currentTransaction, _transactionStream);
                        _transactionStream.Flush();
                        _currentTransaction.Meta = null;
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
                #endregion

                #region Process Transaction
                try
                {
                    var filePath = GetDataPath(meta.Key);
                    switch (transaction)
                    {
                        case FileStorageMetaLog.TransactionType.Remove:
                            Core.Log.LibVerbose("Removing element from filesystem '{0}'.", meta.Key);
                            File.Delete(filePath);
                            return;
                        case FileStorageMetaLog.TransactionType.Add:
                            Core.Log.LibVerbose("Writing element to filesystem '{0}'.", meta.Key);
                            SerializedObject serObj;
                            lock (_pendingLock)
                                _pendingItems.TryGetValue(meta.Key, out serObj);
                            if (serObj != null)
                                Serializer.SerializeToFile(serObj, filePath);
                            lock (_pendingLock)
                                _pendingItems.Remove(meta.Key);
                            return;
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
                #endregion

                #region Save Metadata
                if (_currentTransactionLogLength == 0)
                    SaveMetadata();
                #endregion

                _currentTransactionLogLength++;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void SaveMetadata()
            {
                Core.Log.LibVerbose("Writing Index.");
                try
                {
                    lock (_metasLock)
                    {
                        if (_disposedValue) return;
                        Try.Do(() => File.Copy(_indexFilePath, _indexFilePath + ".old", true), ex => Core.Log.Warning("The Index copy can't be created: {0}", ex.Message));
                        IndexSerializer.SerializeToFile(_metas.Values.ToList(), _indexFilePath);
                        if (_transactionStream == null) return;
                        lock (_transactionStream)
                        {
                            _transactionStream.Position = 0;
                            _transactionStream.SetLength(0);
                            _transactionStream.Flush();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Meta_OnExpire(object sender, EventArgs e)
            {
                if (!(sender is StorageItemMeta meta)) return;
                OnRemove(meta.Key, out meta);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void RemoveExpiredItems()
            {
                StorageItemMeta[] expiredItems;
                lock (_metasLock)
                    expiredItems = _metas.Where(m => m.Value.IsExpired).Select(m => m.Value).ToArray();
                Core.Log.InfoMedium("Removing {0} expired items.", expiredItems.Length);
                foreach (var item in expiredItems)
                {
                    Core.Log.InfoDetail("Removing: {0}", item.Key);
                    OnRemove(item.Key, out var _);
                }
                Core.Log.InfoMedium("All expired items where removed.");
                SaveMetadata();
            }
            #endregion

            #region Overrides
            public IEnumerable<StorageItemMeta> Metas
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    lock (_metasLock)
                        return _metas.Values.ToArray();
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool OnExistKey(string key)
            {
                lock (_metasLock)
                    return _metas.ContainsKey(key);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public string[] OnGetKeys()
            {
                lock (_metasLock)
                    return _metas.Keys.ToArray();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool OnRemove(string key, out StorageItemMeta meta)
            {
                if (_storageWorker.Status != WorkerStatus.Started && _storageWorker.Status != WorkerStatus.Stopped)
                {
                    Core.Log.Warning("The storage is disposing, modifying the collection is forbidden.");
                    meta = null;
                    return false;
                }
                var response = false;
                lock (_pendingLock)
                    _pendingItems.Remove(key);
                lock (_metasLock)
                {
                    if (_metas.TryGetValue(key, out meta))
                    {
                        response = _metas.Remove(key);
                        meta.Dispose();
                    }
                }
                while (_storageWorker.Count >= SlowDownWriteThreshold)
                {
                    Thread.Sleep(100);
                    Core.Log.Warning("The storage working has reached his maximum capacity, slowing down the collection modification.");
                }
                _storageWorker.Enqueue((meta, FileStorageMetaLog.TransactionType.Remove));
                return response;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool OnSet(StorageItemMeta meta, SerializedObject value)
            {
                if (_storageWorker.Status != WorkerStatus.Started && _storageWorker.Status != WorkerStatus.Stopped)
                {
                    Core.Log.Warning("The storage is disposing, modifying the collection is forbidden.");
                    return false;
                }
                lock (_pendingLock)
                    _pendingItems[meta.Key] = value;
                lock (_metasLock)
                {
                    if (_metas.TryGetValue(meta.Key, out var _))
                    {
                        _metas.Remove(meta.Key);
                        meta.Dispose();
                    }
                    if (meta.IsExpired) return false;
                    _metas[meta.Key] = meta;
                    meta.OnExpire += Meta_OnExpire;
                }
                while (_storageWorker.Count >= SlowDownWriteThreshold)
                {
                    Thread.Sleep(100);
                    Core.Log.Warning("The storage working has reached his maximum capacity, slowing down the collection modification.");
                }
                _storageWorker.Enqueue((meta, FileStorageMetaLog.TransactionType.Add));
                return true;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool OnTryGet(string key, out StorageItem value, Predicate<StorageItemMeta> condition = null)
            {
                try
                {
                    StorageItemMeta metaValue;
                    SerializedObject serObj;
                    lock (_pendingLock)
                        _pendingItems.TryGetValue(key, out serObj);
                    lock (_metasLock)
                        _metas.TryGetValue(key, out metaValue);
                    if (metaValue != null && !metaValue.IsExpired && (condition == null || condition(metaValue)))
                    {
                        serObj = serObj ?? Serializer.DeserializeFromFile<SerializedObject>(GetDataPath(key));
                        value = new StorageItem(metaValue, serObj);
                        return true;
                    }
                    value = null;
                    return false;
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
                value = null;
                return false;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool OnTryGetMeta(string key, out StorageItemMeta value, Predicate<StorageItemMeta> condition = null)
            {
                StorageItemMeta metaValue;
                lock (_metasLock)
                    _metas.TryGetValue(key, out metaValue);
                if (metaValue != null && !metaValue.IsExpired && (condition == null || condition(metaValue)))
                {
                    value = metaValue;
                    return true;
                }
                value = null;
                return false;
            }
            #endregion

            #region IDisposable Support
            private bool _disposedValue;

            // To detect redundant calls

            private void Dispose(bool disposing)
            {
                if (_disposedValue) return;
                if (disposing)
                {
                    
                }
                _storageWorker.Stop(int.MaxValue);
                SaveMetadata();
                lock (_transactionStream)
                {
                    _transactionStream.Flush();
                    _transactionStream.Dispose();
                    _transactionStream = null;
                }
                lock (_metasLock)
                    _metas.Clear();
                lock (_pendingLock)
                    _pendingItems.Clear();

                _disposedValue = true;
            }

            ~FolderStorage()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            #endregion
        }
        #endregion
    }
}
