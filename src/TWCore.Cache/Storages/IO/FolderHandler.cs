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
using TWCore.IO;
using TWCore.Serialization;
using TWCore.Threading;

namespace TWCore.Cache.Storages.IO
{
    /// <summary>
    /// Folder handler
    /// </summary>
    [StatusName("Folder Handler")]
    public sealed class FolderHandler : IDisposable
    {
        private const string IndexFileName = "Index";
        private const string TransactionLogFileName = "Index.journal";
        private const string DataExtension = ".data";
        private static readonly List<StorageItemMeta> EmptyMetaList = new List<StorageItemMeta>();
        private static readonly ReferencePool<FileStorageMetaLog> FileStoragePool = new ReferencePool<FileStorageMetaLog>();
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        #region Fields
        private readonly FileStorage _storage;
        private readonly BinarySerializer _indexSerializer;
        private readonly string _transactionLogFilePath;
        private readonly string _indexFilePath;
        private readonly string _oldTransactionLogFilePath;
        private readonly string _oldIndexFilePath;
        private readonly string _dataPathPattern;
        private FileStream _transactionStream;

        private int _savingMetadata;
        private int _removingExpiredItems;
        private int _metasCount;
        private int _pendingItemsCount;

        private int _currentTransactionLogLength;
        private ConcurrentDictionary<string, StorageItemMeta> _globalMetas;
        private ConcurrentDictionary<string, StorageItemMeta> _metas;
        private readonly ConcurrentDictionary<string, SerializedObject> _pendingItems;
        private readonly Worker<FileStorageMetaLog> _storageWorker;

        private readonly Action _saveMetadataBuffered;
        #endregion

        #region Properties
        /// <summary>
        /// Base path where the data is going to be saved.
        /// </summary>
        public string BasePath { get; private set; }
        /// <summary>
        /// Metas Count
        /// </summary>
        public int Count => _metasCount;
        /// <summary>
        /// Gets the status of the handler.
        /// </summary>
        public FolderHandlerStatus Status { get; private set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Folder handler
        /// </summary>
        /// <param name="storage">Storage base</param>
        /// <param name="globalMetas">Global Metas</param>
        /// <param name="basePath">Base path</param>
        public FolderHandler(FileStorage storage, ConcurrentDictionary<string, StorageItemMeta> globalMetas, string basePath)
        {
            BasePath = basePath;
            _storage = storage;
            _globalMetas = globalMetas;
            _indexSerializer = storage.IndexSerializer;
            _saveMetadataBuffered = ActionDelegate.Create(SaveMetadata).CreateBufferedAction(1000);
            var extension = _indexSerializer.Extensions[0];
            _transactionLogFilePath = Path.Combine(BasePath, TransactionLogFileName + extension);
            _dataPathPattern = Path.Combine(BasePath, "$FILE$" + DataExtension);
            _indexFilePath = Path.Combine(BasePath, IndexFileName + extension);
            _oldTransactionLogFilePath = _transactionLogFilePath + ".old";
            _oldIndexFilePath = _indexFilePath + ".old";

            _pendingItems = new ConcurrentDictionary<string, SerializedObject>();
            _storageWorker = new Worker<FileStorageMetaLog>(WorkerProcess)
            {
                EnableWaitTimeout = false
            };
            _storageWorker.OnWorkDone += (s, e) => _saveMetadataBuffered();

            Status = FolderHandlerStatus.Startup;

            Core.Status.Attach(collection =>
            {
                var workerCount = _storageWorker.Count;
                var percentWork = (double)workerCount / _storage.SlowDownWriteThreshold;
                collection.Add(nameof(BasePath), BasePath);
                collection.Add("Count", _metasCount, StatusItemValueStatus.Ok, true);
                collection.Add("Pending Count", _pendingItemsCount, StatusItemValueStatus.Ok, true);
                collection.Add("Worker Count", workerCount, percentWork < 0.8 ? StatusItemValueStatus.Ok : percentWork < 0.95 ? StatusItemValueStatus.Warning : StatusItemValueStatus.Error, true);
                collection.Add("Worker Process Percent", Math.Round(percentWork * 100, 2) + "%", percentWork < 0.8 ? StatusItemValueStatus.Ok : percentWork < 0.95 ? StatusItemValueStatus.Warning : StatusItemValueStatus.Error);
                collection.Add("Transaction Log Length", _currentTransactionLogLength, StatusItemValueStatus.Ok);
                collection.Add("Index File", _indexFilePath, StatusItemValueStatus.Ok);
                collection.Add("Transaction File", _transactionLogFilePath, StatusItemValueStatus.Ok);
            }, this);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Load folder
        /// </summary>
        /// <param name="cancellationToken">CancellationToken instance</param>
        /// <returns>Folder load task</returns>
        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                //Ensure Directory
                if (!Directory.Exists(BasePath))
                {
                    Core.Log.InfoBasic("Creating SubFolder Directory: {0}", BasePath);
                    Directory.CreateDirectory(BasePath);
                }

                //Initialize files in case doesn't exists
                if (!File.Exists(_transactionLogFilePath))
                    File.WriteAllBytes(_transactionLogFilePath, Array.Empty<byte>());
                if (!File.Exists(_indexFilePath))
                    _indexSerializer.SerializeToFile(EmptyMetaList, _indexFilePath);

                //Start loading
                if (cancellationToken.IsCancellationRequested) return;

                #region Loading index file

                var indexLoaded = await Task.Run(() => LoadIndexFile(_indexFilePath) || LoadIndexFile(_oldIndexFilePath)).ConfigureAwait(false);
                if (!indexLoaded)
                {
                    Core.Log.Warning("The index doesn't exist or couldn't be loaded. Generating new index file.");
                    var dateNow = Core.Now;
                    var eTime = dateNow.AddDays(5);
                    if (_storage.MaximumItemDuration.HasValue)
                        eTime = dateNow.Add(_storage.MaximumItemDuration.Value);
                    if (_storage.ItemsExpirationDateOverwrite.HasValue)
                        eTime = dateNow.Add(_storage.ItemsExpirationDateOverwrite.Value);
                    if (_storage.ItemsExpirationAbsoluteDateOverwrite.HasValue)
                        eTime = _storage.ItemsExpirationAbsoluteDateOverwrite.Value;

                    if (_metas is null) _metas = new ConcurrentDictionary<string, StorageItemMeta>();

                    await Task.Run(() =>
                    {
                        var allFiles = Directory.EnumerateFiles(BasePath, "*" + DataExtension, SearchOption.AllDirectories);
                        var idx = 0;
                        foreach (var file in allFiles)
                        {
                            var cTime = File.GetCreationTime(file);
                            var key = Path.GetFileNameWithoutExtension(file);
                            var stoMeta = new StorageItemMeta
                            {
                                Key = key,
                                CreationDate = cTime,
                                ExpirationDate = eTime
                            };
                            _metas.TryAdd(key, stoMeta);
                            _globalMetas.TryAdd(key, stoMeta);
                            if (idx % 100 == 0)
                                Core.Log.InfoBasic("Number of files loaded: {0}", idx);
                            idx++;
                        }
                    }).ConfigureAwait(false);

                    Core.Log.InfoBasic("Index generated...");
                }

                #endregion

                #region Loading transaction log file

                var transactionLog = await LoadTransactionFileAsync(_transactionLogFilePath).ConfigureAwait(false);
                if (transactionLog is null)
                    transactionLog = await LoadTransactionFileAsync(_oldTransactionLogFilePath).ConfigureAwait(false);

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
                                _globalMetas.TryRemove(item.Meta.Key, out _);
                                if (_metas.TryRemove(item.Meta.Key, out var oldAddedMeta) && oldAddedMeta != null)
                                    oldAddedMeta.Dispose();
                                _metas.TryAdd(item.Meta.Key, item.Meta);
                                _globalMetas.TryAdd(item.Meta.Key, item.Meta);
                                break;
                            case FileStorageMetaLog.TransactionType.Remove:
                                _globalMetas.TryRemove(item.Meta.Key, out _);
                                if (_metas.TryRemove(item.Meta.Key, out var oldMeta) && oldMeta != null)
                                    oldMeta.Dispose();
                                break;
                        }
                    }
                }

                #endregion

                _transactionStream = File.Open(_transactionLogFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

                //await SaveMetadataAsync().ConfigureAwait(false);
                SaveMetadata();
                RemoveExpiredItems(false);
                foreach (var metaItem in _metas)
                {
                    if (metaItem.Value is null) continue;
                    metaItem.Value.OnExpire = Meta_OnExpire;
                }

                var metasCount = _metas.Count;
                Interlocked.Exchange(ref _metasCount, metasCount);
                Core.Log.InfoBasic("Total item loaded in {0}: {1}", BasePath, metasCount);
                Status = FolderHandlerStatus.Loaded;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                Status = FolderHandlerStatus.LoadFailed;
            }
        }
        /// <summary>
        /// Exist Key
        /// </summary>
        /// <param name="key">Key value</param>
        /// <returns>True if the key exist in the metadata; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExistKey(string key) => _metas.ContainsKey(key);
        /// <summary>
        /// Remove an item
        /// </summary>
        /// <param name="key">Key value</param>
        /// <param name="removedMeta">Removed meta</param>
        /// <returns>Remove result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(string key, out StorageItemMeta removedMeta)
        {
            if (_storageWorker.Status != WorkerStatus.Started && _storageWorker.Status != WorkerStatus.Stopped)
            {
                Core.Log.Warning("The storage is disposing, modifying the collection is forbidden.");
                removedMeta = null;
                return false;
            }

            if (_pendingItems.TryRemove(key, out _))
                Interlocked.Decrement(ref _pendingItemsCount);

            _globalMetas.TryRemove(key, out _);
            if (!_metas.TryRemove(key, out var meta))
            {
                removedMeta = null;
                return false;
            }

            Interlocked.Decrement(ref _metasCount);
            meta.Dispose();

            if (_storageWorker.Count >= _storage.SlowDownWriteThreshold)
            {
                Core.Log.Warning("The storage working has reached his maximum capacity, slowing down the collection modification.");
                TaskHelper.SleepUntil(() => _storageWorker.Count < _storage.SlowDownWriteThreshold).WaitAsync();
            }

            var fstoItem = FileStoragePool.New();
            fstoItem.Meta = meta;
            fstoItem.Type = FileStorageMetaLog.TransactionType.Remove;
            _storageWorker.Enqueue(fstoItem);
            removedMeta = meta;
            return true;
        }
        /// <summary>
        /// Set an item
        /// </summary>
        /// <param name="meta">Item Meta</param>
        /// <param name="value">Item Value</param>
        /// <returns>Save result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySet(StorageItemMeta meta, SerializedObject value)
        {
            if (_storageWorker.Status != WorkerStatus.Started && _storageWorker.Status != WorkerStatus.Stopped)
            {
                Core.Log.Warning("The storage is disposing, modifying the collection is forbidden.");
                return false;
            }

            _pendingItems.AddOrUpdate(meta.Key, k =>
            {
                Interlocked.Increment(ref _pendingItemsCount);
                return value;
            }, (k, v) =>
            {
                return value;
            });

            _globalMetas.TryRemove(meta.Key, out _);
            if (_metas.TryRemove(meta.Key, out var oldMeta))
            {
                Interlocked.Decrement(ref _metasCount);
                if (oldMeta != null && oldMeta != meta)
                    oldMeta.Dispose();
            }

            if (meta.IsExpired)
            {
                if (_pendingItems.TryRemove(meta.Key, out _))
                    Interlocked.Decrement(ref _pendingItemsCount);
                return false;
            }

            if (!_metas.TryAdd(meta.Key, meta))
                return false;

            _globalMetas.TryAdd(meta.Key, meta);
            Interlocked.Increment(ref _metasCount);
            meta.OnExpire = Meta_OnExpire;

            if (_storageWorker.Count >= _storage.SlowDownWriteThreshold)
            {
                Core.Log.Warning("The storage working has reached his maximum capacity, slowing down the collection modification.");
                TaskHelper.SleepUntil(() => _storageWorker.Count < _storage.SlowDownWriteThreshold).WaitAsync();
            }
            
            var fstoItem = FileStoragePool.New();
            fstoItem.Meta = meta;
            fstoItem.Type = FileStorageMetaLog.TransactionType.Add;
            _storageWorker.Enqueue(fstoItem);
            return true;
        }
        /// <summary>
        /// TryGet async
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="value">StorageItem value</param>
        /// <param name="condition">Get condition</param>
        /// <returns>True if the value exists, otherwise; false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(string key, out StorageItem value, Predicate<StorageItemMeta> condition = null)
        {
            value = null;
            if (!_metas.TryGetValue(key, out var metaValue))
                return false;
            try
            {
                if (metaValue != null && !metaValue.IsExpired && (condition is null || condition(metaValue)))
                {
                    if (_pendingItems.TryGetValue(key, out var serObj))
                    {
                        value = new StorageItem(metaValue, serObj);
                        return true;
                    }
                    serObj = SerializedObject.FromFile(GetDataPath(key));
                    value = new StorageItem(metaValue, serObj);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            return false;
        }
        /// <summary>
        /// Try Get Meta
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="value">StorageItem value</param>
        /// <param name="condition">Get condition</param>
        /// <returns>Get meta results</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetMeta(string key, out StorageItemMeta value, Predicate<StorageItemMeta> condition = null)
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
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool LoadIndexFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Core.Log.Warning("The Index file: {0} doesn't exist.", filePath);
                return false;
            }
            try
            {
                Core.Log.InfoBasic("Loading Index File: {0}", filePath);
                var index = _indexSerializer.DeserializeFromFile<List<StorageItemMeta>>(filePath);
                if (index != null)
                {
                    var pairEnumerable = index.Select(i => new KeyValuePair<string, StorageItemMeta>(i.Key, i)).ToArray();
                    _metas = new ConcurrentDictionary<string, StorageItemMeta>(pairEnumerable);
                    foreach (var valuePair in pairEnumerable)
                        _globalMetas.TryAdd(valuePair.Key, valuePair.Value);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<List<FileStorageMetaLog>> LoadTransactionFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                Core.Log.InfoBasic("Loading Transaction Log File: {0}", filePath);
                var lstTransactions = new List<FileStorageMetaLog>();
                using (var fStream = File.Open(_transactionLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    try
                    {
                        while (fStream.Position != fStream.Length)
                        {
                            var item = _indexSerializer.Deserialize<FileStorageMetaLog>(fStream);
                            if (item != null)
                                lstTransactions.Add(item);
                        }
                    }
                    catch(Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                }
                await FileHelper.CopyFileAsync(_transactionLogFilePath, _oldTransactionLogFilePath, true).ConfigureAwait(false);
                return lstTransactions;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            return null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Meta_OnExpire(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is StorageItemMeta meta)) return;
                TryRemove(meta.Key, out _);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetDataPath(string key)
        {
            foreach (var invalidChar in InvalidPathChars)
            {
                if (key.IndexOf(invalidChar) > -1)
                    key = key.Replace(invalidChar, '-');
            }
            if (key.IndexOf('\\') > -1)
                key = key.Replace('\\', '-');
            if (key.IndexOf('/') > -1)
                key = key.Replace('/', '-');

            key = key.TruncateTo(200);

            return _dataPathPattern.Replace("$FILE$", key);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WorkerProcess(FileStorageMetaLog workerItem)
        {
            if (workerItem.Meta is null) return;
            if (workerItem.Type == FileStorageMetaLog.TransactionType.Add && workerItem.Meta.IsExpired) return;
            if (_currentTransactionLogLength >= _storage.TransactionLogThreshold) _currentTransactionLogLength = 0;

            #region Save Transaction Log
            try
            {
                _indexSerializer.Serialize(workerItem, _transactionStream);
                _transactionStream.Flush();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            #endregion

            #region Process Transaction
            try
            {
                var filePath = GetDataPath(workerItem.Meta.Key);
                switch (workerItem.Type)
                {
                    case FileStorageMetaLog.TransactionType.Remove:
                        Core.Log.DebugGroup(workerItem.Meta.Key, "Removing element from filesystem.");
                        File.Delete(filePath);
                        break;
                    case FileStorageMetaLog.TransactionType.Add:
                        Core.Log.DebugGroup(workerItem.Meta.Key, "Writing element to filesystem.");
                        if (_pendingItems.TryGetValue(workerItem.Meta.Key, out var serObj))
                        {
                            serObj.ToFile(filePath);
                            _pendingItems.TryRemove(workerItem.Meta.Key, out _);
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

            workerItem.Meta = null;
            FileStoragePool.Store(workerItem);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SaveMetadata()
        {
            if (Interlocked.CompareExchange(ref _savingMetadata, 1, 0) == 1) return;
            if (_disposedValue) return;
            if (_transactionStream is null) return;
            if (!_transactionStream.CanWrite) return;
            Core.Log.LibVerbose("Writing Index: {0}", _indexFilePath);
            try
            {
                File.Copy(_indexFilePath, _indexFilePath + ".old", true);
            }
            catch (Exception ex)
            {
                Core.Log.Warning("The Index copy can't be created: {0}", ex.Message);
            }
            try
            {
                _indexSerializer.SerializeToFile(_metas.Values.ToList(), _indexFilePath);
                _transactionStream.Position = 0;
                _transactionStream.SetLength(0);
                _transactionStream.Flush();
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            Interlocked.Exchange(ref _savingMetadata, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveExpiredItems(bool saveBuffered = true)
        {
            if (Interlocked.CompareExchange(ref _removingExpiredItems, 1, 0) == 1) return;
            try
            {
                Core.Log.InfoBasic("Removing expired items.");
                foreach (var meta in _metas)
                {
                    if (meta.Value is null)
                    {
                        Core.Log.Warning("Metadata for {0} is null.", meta.Key);
                        _metas.TryRemove(meta.Key, out _);
                        _globalMetas.TryRemove(meta.Key, out _);
                        continue;
                    }
                    if (!meta.Value.IsExpired) continue;

                    Core.Log.InfoDetail("Removing by expiration: {0}", meta.Key);
                    TryRemove(meta.Key, out _);
                }
                if (saveBuffered)
                    _saveMetadataBuffered();
                else
                    SaveMetadata();
            }
            catch(Exception ex)
            {
                Core.Log.Write(ex);
            }
            Interlocked.Exchange(ref _removingExpiredItems, 0);
        }
        #endregion

        #region IDisposable Support
        private bool _disposedValue;

        // To detect redundant calls

        private void Dispose(bool disposing)
        {
            DisposeAsync(disposing).WaitAsync();
        }
        /// <summary>
        /// Dispose the folder handler in async
        /// </summary>
        /// <param name="disposing"></param>
        /// <returns>Task of the disposing</returns>
        public async Task DisposeAsync(bool disposing)
        {
            if (_disposedValue) return;
            Core.Log.InfoBasic("Stopping storage folder worker on: {0}", BasePath);
            await _storageWorker.StopAsync(int.MaxValue).ConfigureAwait(false);
            Core.Log.InfoBasic("Saving metadata on: {0}", BasePath);
            SaveMetadata();
            //await SaveMetadataAsync().ConfigureAwait(false);
            _disposedValue = true;
            Core.Log.InfoBasic("Saving Journal on: {0}", BasePath);
            while(_removingExpiredItems != 0 || _savingMetadata != 0)
                await Task.Delay(100).ConfigureAwait(false);
            if (_transactionStream.CanWrite)
            {
                await _transactionStream.FlushAsync().ConfigureAwait(false);
                _transactionStream.Dispose();
                _transactionStream = null;
            }
            _metas.Clear();
            _pendingItems.Clear();
            Core.Log.InfoBasic("Folder disposed: {0}", BasePath);
        }

        /// <summary>
        /// FolderHandler finalizer
        /// </summary>
        ~FolderHandler()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose the folder handler
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// Folder Handler Status
    /// </summary>
    public enum FolderHandlerStatus
    {
        /// <summary>
        /// Folder handler has just initialized
        /// </summary>
        Startup,
        /// <summary>
        /// Folder handler has been loaded
        /// </summary>
        Loaded,
        /// <summary>
        /// Folder handler failed to load.
        /// </summary>
        LoadFailed
    }
}
