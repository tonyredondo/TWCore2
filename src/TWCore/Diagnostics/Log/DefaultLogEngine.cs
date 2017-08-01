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
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Log.Storages;
using TWCore.Diagnostics.Status;

namespace TWCore.Diagnostics.Log
{
    /// <summary>
    /// Default log engine
    /// </summary>
    [IgnoreStackFrameLog]
    public class DefaultLogEngine : ILogEngine
    {
        #region Private fields
        readonly Worker<ILogItem> _itemsWorker;
        readonly ConcurrentQueue<ILogItem> lastLogItems = new ConcurrentQueue<ILogItem>();
        readonly Worker<ILogItem> _lastLogItemsWorker;
        readonly ManualResetEventSlim completationHandler = new ManualResetEventSlim(true);
        #endregion

        #region Properties
        /// <summary>
        /// Log storages items
        /// </summary>
        [StatusReference]
        public LogStorageCollection Storage { get; private set; }
        /// <summary>
        /// Max log level to register in logs
        /// </summary>
        [StatusProperty]
        public LogLevel MaxLogLevel { get; set; }
        /// <summary>
        /// Gets or sets the log item factory
        /// </summary>
        [StatusProperty, StatusReference]
        public CreateLogItemDelegate ItemFactory { get; set; } = (LogLevel level, string code, string message, string groupName, Exception ex, string assemblyName, string typeName) =>
        {
            var lItem = new LogItem
            {
                Id = Factory.NewGuid(),
                EnvironmentName = Core.EnvironmentName,
                MachineName = Core.MachineName,
                Timestamp = Core.Now,
                ThreadId = Environment.CurrentManagedThreadId,
                ApplicationName = Core.ApplicationName,
                Level = level,
                Code = code,
                Message = message,
                GroupName = groupName,
                AssemblyName = assemblyName,
                TypeName = typeName,
                Exception = ex != null ? new SerializableException(ex) : null
            };
            return lItem;
        };
        /// <summary>
        /// Gets or sets the maximum items quantity on the queue
        /// </summary>
        [StatusProperty]
        public int MaximumItemsInQueue { get; set; } = Core.GlobalSettings.LogMaximumItemsInQueue;
        /// <summary>
        /// Gets a log done task that completes when the log has finished to write
        /// </summary>
        public Task LogDoneTask => Task.Run(() => completationHandler.Wait());
        /// <summary>
        /// Enable or Disable the Log engine
        /// </summary>
        [StatusProperty]
        public bool Enabled { get; set; } = Core.GlobalSettings.LogEnabled;
        #endregion

        #region .ctors
        /// <summary>
        /// Default log engine
        /// </summary>
        /// <param name="itemFactory">Log item factory</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DefaultLogEngine(CreateLogItemDelegate itemFactory = null)
        {
            MaxLogLevel = LogLevel.LibVerbose;
            Storage = new LogStorageCollection();
            if (itemFactory != null)
                ItemFactory = itemFactory;
            _itemsWorker = new Worker<ILogItem>(() => Storage?.Count > 0, item =>
            {
                try
                {
                    if (item != null)
                    {
                        if (item is NewLineLogItem)
                            Storage.WriteEmptyLine();
                        else
                            Storage.Write(item);
                    }
                }
                catch (Exception)
                {
                }
            }, false);
            _itemsWorker.OnWorkDone += (s, e) => completationHandler.Set();
            _lastLogItemsWorker = new Worker<ILogItem>(item =>
            {
                try
                {
                    if (lastLogItems.Count > 10)
                        lastLogItems.TryDequeue(out var oItem);
                    if (!(item is NewLineLogItem))
                        lastLogItems.Enqueue(item);
                }
                catch (Exception) { }
            }, false);
            Core.Status.AttachObject(this);
            Core.Status.AttachChild(_itemsWorker, this);
            Core.Status.Attach(() =>
            {
                var sItem = new StatusItem();
                var lItems = lastLogItems.ToList();
                if (lItems.Count > 0)
                {
                    sItem.Name = $"Last {lItems.Count} error messages";
                    for (var i = 0; i < lItems.Count; i++)
                        sItem.Values.Add("Error " + i, lItems[i].Message.RemoveInvalidXMLChars() + "\r\nStacktrace:\r\n" + lItems[i].Exception?.StackTrace.RemoveInvalidXMLChars(), StatusItemValueStatus.Red);
                }
                else
                    sItem.Name = "There are no error messages in the log";
                return sItem;
            }, this);
        }
        /// <summary>
        /// Instance destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~DefaultLogEngine()
        {
            Dispose(false);
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        /// <summary>
        /// Dispose the instance resources
        /// </summary>
        /// <param name="disposing">true if should dispose managed resources.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _itemsWorker?.Stop(50);
                    _itemsWorker?.Clear();
                    _lastLogItemsWorker?.Stop(50);
                    _lastLogItemsWorker?.Clear();
                    if (Storage != null)
                    {
                        Storage.Dispose();
                    }
                }
                disposedValue = true;
            }
        }
        /// <summary>
        /// Dispose the instance resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Log Methods
        /// <summary>
        /// Write a log item into the log storages
        /// </summary>
        /// <param name="item">Log item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ILogItem item)
        {
            if (Enabled && item != null && item.Level <= MaxLogLevel && item.Level <= Storage.GetMaxLogLevel())
            {
                if (_itemsWorker?.Count < MaximumItemsInQueue)
                {
                    completationHandler.Reset();
                    _itemsWorker?.Enqueue(item);
                }
                if (item.Level == LogLevel.Error)
                    _lastLogItemsWorker?.Enqueue(item);
            }
        }
        /// <summary>
        /// Write a log item into the log storages
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="code">Item code</param>
        /// <param name="message">Item message</param>
        /// <param name="groupName">Item group category name</param>
        /// <param name="ex">Related exception if is available</param>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="typeName">Type name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(LogLevel level, string code, string message, string groupName, Exception ex = null, string assemblyName = null, string typeName = null)
        {
            if (Enabled && level <= MaxLogLevel && level <= Storage.GetMaxLogLevel())
            {
                var item = ItemFactory(level, code, message, groupName, ex, assemblyName, typeName);
                Write(item);
                if (level == LogLevel.Error)
                    Start();
            }
        }
        /// <summary>
        /// Write a log item into the log storages
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="code">Item code</param>
        /// <param name="message">Item message</param>
        /// <param name="ex">Related exception if is available</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(LogLevel level, string code, string message, Exception ex = null) => Write(level, code, message, null, ex);
        /// <summary>
        /// Write a log item into the log storages
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="message">Item message</param>
        /// <param name="ex">Related exception if is available</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(LogLevel level, string message, Exception ex = null) => Write(level, null, message, null, ex);
        /// <summary>
        /// Write a log item into the log storages
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="ex">Related exception if is available</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(LogLevel level, Exception ex) => Write(level, null, (ex as SerializableException.WrappedException)?.Message ?? ex.Message, null, ex);
        /// <summary>
        /// Write a log item into the log storages
        /// </summary>
        /// <param name="ex">Related exception if is available</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Exception ex) => Write(LogLevel.Error, null, (ex as SerializableException.WrappedException)?.Message ?? ex.Message, null, ex);

        /// <summary>
        /// Write a debug item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Debug(string message, params object[] args)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.Debug))
                WriteUnsafe(LogLevel.Debug, message, args);
        }
        /// <summary>
        /// Write a verbose item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Verbose(string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.Verbose))
                WriteUnsafe(LogLevel.Verbose, message, args);
        }
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(Exception ex, string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, message, args, ex);
        }
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, message, args);
        }
        /// <summary>
        /// Write a warning item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warning(string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.Warning))
                WriteUnsafe(LogLevel.Warning, message, args);
        }
        /// <summary>
        /// Write a InfoLevel1 item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoBasic(string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.InfoBasic))
                WriteUnsafe(LogLevel.InfoBasic, message, args);
        }
        /// <summary>
        /// Write a InfoMedium item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoMedium(string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.InfoMedium))
                WriteUnsafe(LogLevel.InfoMedium, message, args);
        }
        /// <summary>
        /// Write a InfoDetailed item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoDetail(string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.InfoDetail))
                WriteUnsafe(LogLevel.InfoDetail, message, args);
        }
        /// <summary>
        /// Write a Stats item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stats(string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.Stats))
                WriteUnsafe(LogLevel.Stats, message, args);
        }
        /// <summary>
        /// Write a LibDebug item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LibDebug(string message, params object[] args)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.LibDebug))
                WriteUnsafe(LogLevel.LibDebug, message, args);
        }
        /// <summary>
        /// Write a LibVerbose item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LibVerbose(string message, params object[] args)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.LibVerbose))
                WriteUnsafe(LogLevel.LibVerbose, message, args);
        }
        /// <summary>
        /// Write a log empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEmptyLine()
        {
            Write(new NewLineLogItem());
        }
        #endregion

        #region Pending Items
        /// <summary>
        /// Get all pending to write log items
        /// </summary>
        /// <returns>Pending log items array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogItem[] GetPendingItems() => _itemsWorker?.QueueItems?.ToArray();
        /// <summary>
        /// Enqueue to write the log items to the log storages
        /// </summary>
        /// <param name="items">Log items range</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnqueueItemsArray(ILogItem[] items) => items?.Each(i => _itemsWorker.Enqueue(i));
        /// <summary>
        /// Starts the Log Engine
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start() 
        {
            _itemsWorker.Start();
            _lastLogItemsWorker.Start();
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsLogLevelValid(LogLevel logLevel)
        {
            if (Storage.Count > 0 && (logLevel > MaxLogLevel || logLevel > Storage.GetMaxLogLevel()))
                return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WriteUnsafe(LogLevel level, string message, object[] args, Exception ex = null)
        {
            if (Enabled && _itemsWorker?.Count < MaximumItemsInQueue)
            {
                if (args?.Length > 0)
                    message = string.Format(message, args);
                var item = ItemFactory(level, null, message, null, ex, null, null);
                if (item != null) 
                {
                    _itemsWorker?.Enqueue(item);
                    if (item.Level == LogLevel.Error) 
                    {
                        _lastLogItemsWorker?.Enqueue(item);
                        Start();
                    }
                }
            }
        }
        #endregion
    }
}
