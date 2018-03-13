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
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Serialization;
using TWCore.Threading;
// ReSharper disable EventNeverSubscribedTo.Global

namespace TWCore.IO
{
    /// <summary>
    /// Create an object instance deserializing from a file, and keeps the instance updated if the changes.
    /// </summary>
    /// <typeparam name="T">Object type of the instance</typeparam>
    public class FileObject<T> where T : class
    {
        private readonly object _locker = new object();
        private FileSystemWatcher _fwatcher;
        private T _instance;
        private Guid _id = Guid.Empty;

        #region Properties
        /// <summary>
        /// Object file path
        /// </summary>
        public string FilePath { get; private set; }
        /// <summary>
        /// Current object instance
        /// </summary>
        public T Instance { get { lock (_locker) return _instance; } }
        /// <summary>
        /// Current object id
        /// </summary>
        public Guid Id { get { lock (_locker) return _id; } }
        /// <summary>
        /// Serializer used to load the file
        /// </summary>
        public ISerializer Serializer { get; }
        #endregion

        #region Events
        /// <summary>
        /// Event fired when the file changes and a new object is loaded
        /// </summary>
        public event EventHandler<FileObjectEventArgs<T>> OnChanged;
        /// <summary>
        /// Event fired when an exception occurs
        /// </summary>
        public event EventHandler<EventArgs<Exception>> OnException;
        #endregion

        #region .ctor
        /// <summary>
        /// Create an object instance deserializing from a file, and keeps the instance updated if the changes.
        /// </summary>
        /// <param name="filePath">Object file path</param>
        /// <param name="serializer">Serializer to use to load the file, if null the default is XmlTextSerializer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FileObject(string filePath, ISerializer serializer = null)
        {
            FilePath = filePath;
            Serializer = serializer ?? new XmlTextSerializer();
            LoadConfig();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~FileObject()
        {
            Dispose();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Load configuration file instance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LoadConfig()
        {
            lock (_locker)
            {
                var currentPath = Path.GetDirectoryName(AppContext.BaseDirectory);
                FilePath = Path.Combine(currentPath, FilePath);
                Try.Do(() =>
                {
                    if (_fwatcher == null) return;
                    _fwatcher.EnableRaisingEvents = false;
                    _fwatcher.Dispose();
                    _fwatcher = null;
                });
                if (!LoadFile())
                {
                    _instance = Activator.CreateInstance<T>();
                    _id = Guid.NewGuid();
                    Save();
                }
                _fwatcher = CreateWatcher();
            }
        }
        /// <summary>
        /// Save the current object instance to disk
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Save()
        {
            if (_fwatcher != null)
                _fwatcher.EnableRaisingEvents = false;
            Serializer?.SerializeToFile(_instance, FilePath);
            if (_fwatcher != null)
                _fwatcher.EnableRaisingEvents = true;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Load file method
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool LoadFile()
        {
            lock (this)
            {
                var fpath = FilePath;
                var exist = true;

                #region Check and get the real file path
                if (!File.Exists(fpath))
                {
                    exist = false;
                    foreach (var fExt in Serializer.Extensions)
                    {
                        if (File.Exists(FilePath + fExt))
                        {
                            fpath = FilePath + fExt;
                            exist = true;
                            break;
                        }
                        if (Serializer.Compressor != null && File.Exists(FilePath + fExt + Serializer.Compressor.FileExtension))
                        {
                            fpath = FilePath + fExt + Serializer.Compressor.FileExtension;
                            exist = true;
                        }
                    }
                    if (!exist)
                    {
                        if (Serializer.Compressor != null && File.Exists(FilePath + Serializer.Compressor.FileExtension))
                        {
                            fpath = FilePath + Serializer.Compressor.FileExtension;
                            exist = true;
                        }
                    }
                }
                #endregion

                if (!exist) return false;
                FilePath = fpath;
                _instance = Serializer.DeserializeFromFile<T>(FilePath);
                _id = Guid.NewGuid();
                return true;
            }
        }
        /// <summary>
        /// Creates a new file watcher
        /// </summary>
        /// <returns>FileSystemWatcher object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private FileSystemWatcher CreateWatcher()
        {
            var watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(FilePath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Security | NotifyFilters.Size | NotifyFilters.CreationTime,
                Filter = Path.GetFileName(FilePath)
            };
            // Add event handlers.
            watcher.Changed += OnFileWatcherChanged;
            watcher.Error += Watcher_Error;
            // Begin watching.
            watcher.EnableRaisingEvents = true;
            return watcher;
        }
        /// <summary>
        /// Watcher error method
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Error event args</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void Watcher_Error(object sender, ErrorEventArgs e)
        {
            try
            {
                var watchException = e.GetException();
                OnException?.Invoke(this, new EventArgs<Exception>(watchException));
                FileObjectEvents.FireException(this, watchException);
                Try.Do(() =>
                {
                    if (_fwatcher == null) return;
                    _fwatcher.EnableRaisingEvents = false;
                    _fwatcher.Dispose();
                });
                _fwatcher = null;
                var nWatcher = CreateWatcher();
                while (!nWatcher.EnableRaisingEvents)
                {
                    try
                    {
                        nWatcher = CreateWatcher();
                    }
                    catch
                    {
                        await Task.Delay(500).ConfigureAwait(false);
                    }
                }
                _fwatcher = nWatcher;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        /// <summary>
        /// On File Watcher Changed method
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="e">File System Event args</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnFileWatcherChanged(object source, FileSystemEventArgs e)
        {
            lock (this)
            {
                if (e.ChangeType == WatcherChangeTypes.Deleted) return;

                if (_fwatcher != null)
                    _fwatcher.EnableRaisingEvents = false;

                FileObjectEventArgs<T> evArgs = null;
                try
                {
                    Core.Log.Warning("File for object {0} => {1}. Reloading file: {2}", typeof(T).Name, e.ChangeType, FilePath);
                    var oldId = _id;
                    var oldValue = _instance;
                    LoadFile();
                    var newId = _id;
                    var newValue = _instance;
                    Core.Log.Warning("File for object {0} => File loaded.", typeof(T).Name, e.ChangeType);
                    evArgs = new FileObjectEventArgs<T>(FilePath, oldId, oldValue, newId, newValue);
                }
                catch (Exception ex)
                {
                    OnException?.Invoke(this, new EventArgs<Exception>(ex));
                    FileObjectEvents.FireException(this, ex);
                    Core.Log.Write(ex);
                }
                finally
                {
                    if (_fwatcher != null)
                        _fwatcher.EnableRaisingEvents = true;
                    if (evArgs != null)
                    {
                        OnChanged?.Invoke(this, evArgs);
                        FileObjectEvents.FireFileObjectChanged(this, evArgs.FilePath, evArgs.OldId, evArgs.OldValue, evArgs.NewId, evArgs.NewValue);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Dispose object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            OnChanged = null;
            if (_fwatcher == null) return;
            _fwatcher.EnableRaisingEvents = false;
            _fwatcher = null;
        }
    }
}
