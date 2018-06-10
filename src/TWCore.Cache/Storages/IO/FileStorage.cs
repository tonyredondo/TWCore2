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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Threading;
using TWCore.Diagnostics.Status;
using TWCore.IO;
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
            basePath = Factory.ResolveLowLowPath(basePath);
            BasePath = basePath;
            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(BasePath), BasePath);
                collection.Add("Count", Metas?.Count(), StatusItemValueStatus.Ok);
                collection.Add(nameof(IndexSerializer), IndexSerializer);
                collection.Add(nameof(NumberOfSubFolders), NumberOfSubFolders, NumberOfSubFolders > 10 ? StatusItemValueStatus.Ok : NumberOfSubFolders > 2 ? StatusItemValueStatus.Warning : StatusItemValueStatus.Error);
                collection.Add(nameof(TransactionLogThreshold), TransactionLogThreshold);
                collection.Add(nameof(SlowDownWriteThreshold), SlowDownWriteThreshold);
                if (_handlers == null) return;
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
            return (int)(key.GetJenkinsHash() % NumberOfSubFolders);
        }

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
            if (_handlers?.Any() == true)
            {
                Core.Log.InfoBasic("Disposing previous instances...");
                _handlers.Each(fsto => fsto.Dispose());
                _handlers = null;
            }
            if (!Directory.Exists(BasePath))
            {
                Core.Log.InfoBasic("Creating base folder");
                Directory.CreateDirectory(BasePath);
            }
            Core.Log.InfoBasic("Configuring {0} Subfolders", NumberOfSubFolders);
            _handlers = new FolderHandler[NumberOfSubFolders];
            for (var i = 0; i < NumberOfSubFolders; i++)
            {
                var folder = Path.Combine(BasePath, i.ToString());
                Core.Log.InfoBasic("Initializing Subfolder: {0} on {1}", i, folder);
                _handlers[i] = new FolderHandler(folder, this);
            }
            Core.Log.InfoBasic("Waiting the folder handlers to be loaded.");
            TaskHelper.SleepUntil(() => _handlers.All(s => s.Loaded)).WaitAsync();
            Core.Log.InfoBasic("All folder handlers are loaded, Index Count: {0}", Metas.Count());
            SetReady(true);
        }
        protected override IEnumerable<StorageItemMeta> Metas
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handlers?.SelectMany(s => s.Metas).ToArray();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnExistKey(string key)
            => _handlers[GetFolderNumber(key)].OnExistKey(key);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string[] OnGetKeys()
            => _handlers.SelectMany(s => s.OnGetKeys()).ToArray();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnRemove(string key, out StorageItemMeta meta)
        {
            var res = _handlers[GetFolderNumber(key)].OnRemove(key).WaitAsync();
            meta = res.Item2;
            return res.Item1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnSet(StorageItemMeta meta, SerializedObject value)
            => _handlers[GetFolderNumber(meta.Key)].OnSet(meta, value).WaitAsync();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnTryGet(string key, out StorageItem value, Predicate<StorageItemMeta> condition = null)
        {
            var res = _handlers[GetFolderNumber(key)].OnTryGet(key, condition).WaitAsync();
            value = res;
            return res != null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool OnTryGetMeta(string key, out StorageItemMeta value,
            Predicate<StorageItemMeta> condition = null)
        {
            var res = _handlers[GetFolderNumber(key)].OnTryGetMeta(key, condition);
            value = res;
            return res != null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnDispose()
        {
            if (_handlers == null) return;
            Core.Log.InfoBasic("Disposing...");
            _handlers.ParallelEach(s => s.Dispose());
            _handlers = null;
            Core.Log.InfoBasic("Disposed.");
        }
        #endregion

        #region Inner Types
        private sealed class FolderHandler : IDisposable
        {
            private static readonly byte[] BytesEmpty = new byte[0];
            private const string IndexFileName = "Index";
            private const string TransactionLogFileName = "Index.journal";
            private const string DataExtension = ".data";

            #region Fields
            // ReSharper disable once NotAccessedField.Local
            private Task _loadTask;
            private NonBlocking.ConcurrentDictionary<string, StorageItemMeta> _metas;
            private int _currentTransactionLogLength;
            private FileStream _transactionStream;
            private readonly NonBlocking.ConcurrentDictionary<string, SerializedObject> _pendingItems;
            private readonly Worker<(StorageItemMeta, FileStorageMetaLog.TransactionType)> _storageWorker;
            private readonly string _transactionLogFilePath;
            private readonly string _indexFilePath;
            private readonly FileStorageMetaLog _currentTransaction;
            private readonly ManualResetEventSlim _storageWorkerEvent = new ManualResetEventSlim();
            private readonly Action _saveMetadataBuffered;
            private readonly AsyncLock _asyncLock = new AsyncLock();
            private readonly string _dataPathPattern;

            #endregion

            #region Properties
            /// <summary>
            /// Base path where the data is going to be saved.
            /// </summary>
            public string BasePath { get; private set; }
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
            public FolderHandler(string basePath, FileStorage storage)
            {
                _saveMetadataBuffered = ActionDelegate.Create(async () => await SaveMetadataAsync().ConfigureAwait(false)).CreateBufferedAction(1000);
                BasePath = basePath;
                IndexSerializer = storage.IndexSerializer;
                TransactionLogThreshold = storage.TransactionLogThreshold;
                SlowDownWriteThreshold = storage.SlowDownWriteThreshold;
                Loaded = false;
                LoadingFailed = false;
                _transactionLogFilePath = Path.Combine(BasePath, TransactionLogFileName + IndexSerializer.Extensions[0]);
                _dataPathPattern = Path.Combine(BasePath, "$FILE$" + DataExtension);
                var oldTransactionLogFilePath = _transactionLogFilePath + ".old";
                _indexFilePath = Path.Combine(BasePath, IndexFileName + IndexSerializer.Extensions[0]);
                var oldindexFilePath = _indexFilePath + ".old";
                _metas = new NonBlocking.ConcurrentDictionary<string, StorageItemMeta>();
                _pendingItems = new NonBlocking.ConcurrentDictionary<string, SerializedObject>();
                var tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;
                _storageWorker = new Worker<(StorageItemMeta, FileStorageMetaLog.TransactionType)>(function: WorkerProcess)
                {
                    EnableWaitTimeout = false
                };
                _storageWorker.OnWorkDone += (sender, e) => _saveMetadataBuffered();
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
                _loadTask = Task.Run<Task>(async () =>
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
                                    var pairEnumerable = index.Select(i => new KeyValuePair<string, StorageItemMeta>(i.Key, i));
                                    _metas = new NonBlocking.ConcurrentDictionary<string, StorageItemMeta>(pairEnumerable);
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
                                        var pairEnumerable = index.Select(i => new KeyValuePair<string, StorageItemMeta>(i.Key, i));
                                        _metas = new NonBlocking.ConcurrentDictionary<string, StorageItemMeta>(pairEnumerable);
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

                            if (_metas == null) _metas = new NonBlocking.ConcurrentDictionary<string, StorageItemMeta>();

                            var allFiles = Directory.EnumerateFiles(BasePath, "*" + DataExtension, SearchOption.AllDirectories);
                            var idx = 0;
                            foreach (var file in allFiles)
                            {
                                var cTime = File.GetCreationTime(file);
                                var key = Path.GetFileNameWithoutExtension(file);
                                _metas.TryAdd(key,
                                    new StorageItemMeta
                                    {
                                        Key = key,
                                        CreationDate = cTime,
                                        ExpirationDate = eTime
                                    });
                                if (idx % 100 == 0)
                                    Core.Log.InfoBasic("Number of files loaded: {0}", idx);
                                idx++;
                            }

                            Core.Log.InfoBasic("Index generated...");
                        }

                        #endregion

                        List<FileStorageMetaLog> transactionLog = null;

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
                                await FileHelper.CopyFileAsync(_transactionLogFilePath, oldTransactionLogFilePath, true).ConfigureAwait(false);
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

                        _transactionStream = File.Open(_transactionLogFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

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
                                        if (_metas.TryRemove(item.Meta.Key, out var oldMeta))
                                            oldMeta?.Dispose();
                                        break;
                                }
                            }
                        }

                        #endregion

                        await SaveMetadataAsync().ConfigureAwait(false);
                        await RemoveExpiredItemsAsync(false).ConfigureAwait(false);
                        _metas.Each(m => m.Value.OnExpire += Meta_OnExpire);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                        LoadingFailed = true;
                    }
                    if (!LoadingFailed)
                    {
                        Core.Log.InfoBasic("Total item loaded: {0}", _metas.Count);
                        Core.Log.LibVerbose("All metadata loaded.");
                        Loaded = true;
                    }
                }, token);

                Core.Status.Attach(collection =>
                {
                    var percentWork = (double)_storageWorker.Count / SlowDownWriteThreshold;
                    collection.Add(nameof(BasePath), BasePath);
                    collection.Add("Count", _metas.Count, StatusItemValueStatus.Ok, true);
                    collection.Add("Pending Count", _pendingItems.Count, StatusItemValueStatus.Ok, true);
                    collection.Add("Worker Count", _storageWorker.Count, percentWork < 0.8 ? StatusItemValueStatus.Ok : percentWork < 0.95 ? StatusItemValueStatus.Warning : StatusItemValueStatus.Error, true);
                    collection.Add("Worker Process Percent", Math.Round(percentWork * 100, 2) + "%", percentWork < 0.8 ? StatusItemValueStatus.Ok : percentWork < 0.95 ? StatusItemValueStatus.Warning : StatusItemValueStatus.Error);
                    collection.Add("Transaction Log Length", _currentTransactionLogLength, StatusItemValueStatus.Ok);
                    collection.Add("Index File", _indexFilePath, StatusItemValueStatus.Ok);
                    collection.Add("Transaction File", _transactionLogFilePath, StatusItemValueStatus.Ok);
                }, this);
            }
            #endregion

            #region Private Methods
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private string GetDataPath(string key) => _dataPathPattern.Replace("$FILE$", key);  //Path.Combine(BasePath, key + DataExtension + Serializer.Extensions[0]);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private async Task WorkerProcess((StorageItemMeta, FileStorageMetaLog.TransactionType) workerItem)
            {
                if (_storageWorker != null && _storageWorker.Count < SlowDownWriteThreshold)
                    _storageWorkerEvent.Set();
                var (meta, transaction) = workerItem;
                if (meta == null) return;
                if (transaction == FileStorageMetaLog.TransactionType.Add && meta.IsExpired) return;
                if (_currentTransactionLogLength >= TransactionLogThreshold) _currentTransactionLogLength = 0;

                #region Save Transaction Log
                using (await _asyncLock.LockAsync().ConfigureAwait(false))
                {

                    try
                    {
                        await FileHelper.CopyFileAsync(_transactionLogFilePath, _transactionLogFilePath + ".old", true).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Warning("The Transaction log copy can't be created: {0}", ex.Message);
                    }

                    try
                    {
                        _currentTransaction.Meta = meta;
                        _currentTransaction.Type = transaction;
                        IndexSerializer.Serialize(_currentTransaction, _transactionStream);
                        await _transactionStream.FlushAsync().ConfigureAwait(false);
                        _currentTransaction.Meta = null;
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                }
                #endregion

                #region Process Transaction
                try
                {
                    var filePath = GetDataPath(meta.Key);
                    switch (transaction)
                    {
                        case FileStorageMetaLog.TransactionType.Remove:
                            Core.Log.DebugGroup(meta.Key, "Removing element from filesystem.");
                            File.Delete(filePath);
                            break;
                        case FileStorageMetaLog.TransactionType.Add:
                            Core.Log.DebugGroup(meta.Key, "Writing element to filesystem.");
                            if (_pendingItems.TryGetValue(meta.Key, out var serObj))
                            {
                                await serObj.ToFileAsync(filePath).ConfigureAwait(false);
                                _pendingItems.TryRemove(meta.Key, out _);
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
                #endregion

                #region Save Metadata
                if (_currentTransactionLogLength == 0)
                    _saveMetadataBuffered();
                #endregion

                _currentTransactionLogLength++;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private async Task SaveMetadataAsync()
            {
                using (await _asyncLock.LockAsync().ConfigureAwait(false))
                {
                    if (_disposedValue) return;
                    if (_transactionStream == null) return;
                    if (!_transactionStream.CanWrite) return;
                    Core.Log.LibVerbose("Writing Index: {0}", _indexFilePath);
                    try
                    {
                        await FileHelper.CopyFileAsync(_indexFilePath, _indexFilePath + ".old", true).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Warning("The Index copy can't be created: {0}", ex.Message);
                    }
                    try
                    {
                        await IndexSerializer.SerializeToFileAsync(_metas.Values.ToList(), _indexFilePath).ConfigureAwait(false);
                        _transactionStream.Position = 0;
                        _transactionStream.SetLength(0);
                        await _transactionStream.FlushAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private async void Meta_OnExpire(object sender, EventArgs e)
            {
                try
                {
                    if (!(sender is StorageItemMeta meta)) return;
                    await OnRemove(meta.Key).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private async Task RemoveExpiredItemsAsync(bool saveBuffered = true)
            {
                var expiredItems = _metas.Where(m => m.Value.IsExpired).Select(m => m.Value).ToArray();
                if (expiredItems.Length > 0)
                {
                    Core.Log.InfoMedium("Removing {0} expired items.", expiredItems.Length);
                    foreach (var item in expiredItems)
                    {
                        Core.Log.InfoDetail("Removing: {0}", item.Key);
                        await OnRemove(item.Key).ConfigureAwait(false);
                    }
                    Core.Log.InfoMedium("All expired items where removed.");
                    if (saveBuffered)
                        _saveMetadataBuffered();
                    else
                        await SaveMetadataAsync().ConfigureAwait(false);
                }
            }
            #endregion

            #region Overrides
            public IEnumerable<StorageItemMeta> Metas => _metas.Values;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool OnExistKey(string key) => _metas.ContainsKey(key);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public string[] OnGetKeys() => _metas.Keys.ToArray();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async Task<(bool, StorageItemMeta)> OnRemove(string key)
            {
                if (_storageWorker.Status != WorkerStatus.Started && _storageWorker.Status != WorkerStatus.Stopped)
                {
                    Core.Log.Warning("The storage is disposing, modifying the collection is forbidden.");
                    return (false, null);
                }
                _pendingItems.TryRemove(key, out _);
                if (!_metas.TryRemove(key, out var meta)) return (false, null);
                meta.Dispose();
                if (_storageWorker.Count >= SlowDownWriteThreshold)
                {
                    Core.Log.Warning("The storage working has reached his maximum capacity, slowing down the collection modification.");
                    await TaskHelper.SleepUntil(() => _storageWorker.Count < SlowDownWriteThreshold).ConfigureAwait(false);
                }
                _storageWorker.Enqueue((meta, FileStorageMetaLog.TransactionType.Remove));
                return (true, meta);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async Task<bool> OnSet(StorageItemMeta meta, SerializedObject value)
            {
                if (_storageWorker.Status != WorkerStatus.Started && _storageWorker.Status != WorkerStatus.Stopped)
                {
                    Core.Log.Warning("The storage is disposing, modifying the collection is forbidden.");
                    return false;
                }
                _pendingItems.AddOrUpdate(meta.Key, value, (k, v) => value);
                if (_metas.TryRemove(meta.Key, out var oldMeta) && oldMeta!= meta)
                    oldMeta.Dispose();
                if (meta.IsExpired) return false;
                if (!_metas.TryAdd(meta.Key, meta)) return false;
                meta.OnExpire += Meta_OnExpire;
                if (_storageWorker.Count >= SlowDownWriteThreshold)
                {
                    Core.Log.Warning("The storage working has reached his maximum capacity, slowing down the collection modification.");
                    await TaskHelper.SleepUntil(() => _storageWorker.Count < SlowDownWriteThreshold).ConfigureAwait(false);
                }
                _storageWorker.Enqueue((meta, FileStorageMetaLog.TransactionType.Add));
                return true;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async Task<StorageItem> OnTryGet(string key, Predicate<StorageItemMeta> condition = null)
            {
                try
                {
                    _pendingItems.TryGetValue(key, out var serObj);
                    _metas.TryGetValue(key, out var metaValue);
                    if (metaValue != null && !metaValue.IsExpired && (condition == null || condition(metaValue)))
                    {
                        serObj = serObj ?? await SerializedObject.FromFileAsync(GetDataPath(key)).ConfigureAwait(false);
                        return new StorageItem(metaValue, serObj);
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
                return null;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public StorageItemMeta OnTryGetMeta(string key, Predicate<StorageItemMeta> condition = null)
            {
                if (!_metas.TryGetValue(key, out var metaValue)) return null;
                if (metaValue != null && !metaValue.IsExpired && (condition == null || condition(metaValue)))
                    return metaValue;
                return null;
            }
            #endregion

            #region IDisposable Support
            private bool _disposedValue;

            // To detect redundant calls

            private void Dispose(bool disposing)
            {
                DisposeAsync(disposing).WaitAsync();
            }

            private async Task DisposeAsync(bool disposing)
            {
                if (_disposedValue) return;
                if (disposing)
                {

                }
                Core.Log.InfoBasic("Stopping storage folder worker on: {0}", BasePath);
                await _storageWorker.StopAsync(int.MaxValue).ConfigureAwait(false);
                Core.Log.InfoBasic("Saving metadata on: {0}", BasePath);
                await SaveMetadataAsync().ConfigureAwait(false);
                _disposedValue = true;
                Core.Log.InfoBasic("Saving Journal on: {0}", BasePath);
                using (await _asyncLock.LockAsync().ConfigureAwait(false))
                {
                    if (_transactionStream.CanWrite)
                    {
                        await _transactionStream.FlushAsync().ConfigureAwait(false);
                        _transactionStream.Dispose();
                        _transactionStream = null;
                    }
                }
                _metas.Clear();
                _pendingItems.Clear();
                Core.Log.InfoBasic("Folder disposed: {0}", BasePath);
            }

            ~FolderHandler()
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
