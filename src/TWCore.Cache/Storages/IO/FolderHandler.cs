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
using TWCore.Diagnostics.Status;
using TWCore.IO;
using TWCore.Serialization;

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

        #region Fields
        private readonly FileStorage _storage;
        private readonly BinarySerializer _indexSerializer;
        private readonly string _transactionLogFilePath;
        private readonly string _indexFilePath;
        private readonly string _oldTransactionLogFilePath;
        private readonly string _oldIndexFilePath;
        private readonly string _dataPathPattern;
        private FileStream _transactionStream;

        private int _metasCount;
        private int _pendingItemsCount;
        private int _currentTransactionLogLength;
        private NonBlocking.ConcurrentDictionary<string, StorageItemMeta> _metas;
        private readonly NonBlocking.ConcurrentDictionary<string, SerializedObject> _pendingItems;
        private readonly Worker<(StorageItemMeta, FileStorageMetaLog.TransactionType)> _storageWorker;

        private readonly Action _saveMetadataBuffered;
        #endregion

        #region Properties
        /// <summary>
        /// Base path where the data is going to be saved.
        /// </summary>
        public string BasePath { get; private set; }
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
        /// <param name="basePath">Base path</param>
        public FolderHandler(FileStorage storage, string basePath)
        {
            BasePath = basePath;
            _storage = storage;
            _indexSerializer = storage.IndexSerializer;
            _saveMetadataBuffered = new Func<Task>(SaveMetadataAsync).CreateBufferedTask(1000);
            var extension = _indexSerializer.Extensions[0];
            _transactionLogFilePath = Path.Combine(BasePath, TransactionLogFileName + extension);
            _dataPathPattern = Path.Combine(BasePath, "$FILE$" + DataExtension);
            _indexFilePath = Path.Combine(BasePath, IndexFileName + extension);
            _oldTransactionLogFilePath = _transactionLogFilePath + ".old";
            _oldIndexFilePath = _indexFilePath + ".old";

            _pendingItems = new NonBlocking.ConcurrentDictionary<string, SerializedObject>();
            _storageWorker = new Worker<(StorageItemMeta, FileStorageMetaLog.TransactionType)>(WorkerProcess)
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

                var indexLoaded = await LoadIndexFileAsync(_indexFilePath).ConfigureAwait(false) || await LoadIndexFileAsync(_oldIndexFilePath).ConfigureAwait(false);
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

                    if (_metas == null) _metas = new NonBlocking.ConcurrentDictionary<string, StorageItemMeta>();

                    await Task.Run(() =>
                    {
                        var allFiles = Directory.EnumerateFiles(BasePath, "*" + DataExtension, SearchOption.AllDirectories);
                        var idx = 0;
                        foreach (var file in allFiles)
                        {
                            var cTime = File.GetCreationTime(file);
                            var key = Path.GetFileNameWithoutExtension(file);
                            _metas.TryAdd(key, new StorageItemMeta
                            {
                                Key = key,
                                CreationDate = cTime,
                                ExpirationDate = eTime
                            });
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
                if (transactionLog == null)
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
                                if (_metas.TryRemove(item.Meta.Key, out var oldAddedMeta) && oldAddedMeta != null)
                                    oldAddedMeta.Dispose();
                                _metas.TryAdd(item.Meta.Key, item.Meta);
                                break;
                            case FileStorageMetaLog.TransactionType.Remove:
                                if (_metas.TryRemove(item.Meta.Key, out var oldMeta) && oldMeta != null)
                                    oldMeta.Dispose();
                                break;
                        }
                    }
                }

                #endregion

                _transactionStream = File.Open(_transactionLogFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

                await SaveMetadataAsync().ConfigureAwait(false);
                await RemoveExpiredItemsAsync(false).ConfigureAwait(false);
                foreach (var metaItem in _metas)
                {
                    if (metaItem.Value == null) continue;
                    metaItem.Value.OnExpire += Meta_OnExpire;
                }

                var metasCount = _metas.Count;
                Interlocked.Exchange(ref _metasCount, metasCount);
                Core.Log.InfoBasic("Total item loaded: {0}", metasCount);
                Core.Log.LibVerbose("All metadata loaded.");
                Status = FolderHandlerStatus.Loaded;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                Status = FolderHandlerStatus.LoadFailed;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<bool> LoadIndexFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Core.Log.Warning("The Index file: {0} doesn't exist.", filePath);
                return false;
            }
            return await Task.Run(() =>
            {
                try
                {
                    Core.Log.InfoBasic("Loading Index File: {0}", filePath);
                    var index = _indexSerializer.DeserializeFromFile<List<StorageItemMeta>>(filePath);
                    if (index != null)
                    {
                        var pairEnumerable = index.Select(i => new KeyValuePair<string, StorageItemMeta>(i.Key, i));
                        _metas = new NonBlocking.ConcurrentDictionary<string, StorageItemMeta>(pairEnumerable);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
                return false;
            }).ConfigureAwait(false);
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
                    while (fStream.Position != fStream.Length)
                    {
                        var item = _indexSerializer.Deserialize<FileStorageMetaLog>(fStream);
                        if (item != null)
                            lstTransactions.Add(item);
                    }
                }
                await FileHelper.CopyFileAsync(_transactionLogFilePath, _oldTransactionLogFilePath, true).ConfigureAwait(false);
                return lstTransactions;
            }
            catch(Exception ex)
            {
                Core.Log.Write(ex);
            }
            return null;
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task WorkerProcess((StorageItemMeta, FileStorageMetaLog.TransactionType) workerItem)
        {
            return;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task SaveMetadataAsync()
        {
            return;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void Meta_OnExpire(object sender, EventArgs e)
        {
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task RemoveExpiredItemsAsync(bool saveBuffered = true)
        {
        }
        #endregion


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FolderHandler() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// Folder Handler Status
    /// </summary>
    public enum FolderHandlerStatus
    {
        Startup,
        Loaded,
        LoadFailed
    }
}
