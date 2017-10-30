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
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Diagnostics.Log
{
    /// <inheritdoc />
    /// <summary>
    /// Default log engine
    /// </summary>
    [IgnoreStackFrameLog]
    public sealed class DefaultLogEngine : ILogEngine
    {
        #region Private fields
        private readonly Worker<ILogItem> _itemsWorker;
        private readonly ConcurrentQueue<ILogItem> _lastLogItems = new ConcurrentQueue<ILogItem>();
        private readonly Worker<ILogItem> _lastLogItemsWorker;
        private readonly ManualResetEventSlim _completationHandler = new ManualResetEventSlim(true);
        #endregion

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Log storages items
        /// </summary>
        [StatusReference]
        public LogStorageCollection Storage { get; }
        /// <inheritdoc />
        /// <summary>
        /// Max log level to register in logs
        /// </summary>
        [StatusProperty]
        public LogLevel MaxLogLevel { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the log item factory
        /// </summary>
        public CreateLogItemDelegate ItemFactory { get; set; } = (level, code, message, groupName, ex, assemblyName, typeName) =>
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
        public Task LogDoneTask => Task.Run(() => _completationHandler.Wait());
        /// <inheritdoc />
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
                    switch (item)
                    {
                        case null:
                            return;
                        case NewLineLogItem _:
                            Storage.WriteEmptyLine();
                            break;
                        default:
                            Storage.Write(item);
                            break;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }, false);
            _itemsWorker.OnWorkDone += (s, e) => _completationHandler.Set();
            _lastLogItemsWorker = new Worker<ILogItem>(item =>
            {
                try
                {
                    if (_lastLogItems.Count > 10)
                        _lastLogItems.TryDequeue(out var _);
                    if (!(item is NewLineLogItem))
                        _lastLogItems.Enqueue(item);
                }
                catch (Exception)
                {
                    // ignored
                }
            }, false);
            Core.Status.Attach(() =>
            {
                var sItem = new StatusItem();
                var lItems = _lastLogItems.ToList();
                if (lItems.Count > 0)
                {
                    sItem.Name = $"Last {lItems.Count} error messages";
                    for (var i = 0; i < lItems.Count; i++)
                    {
                        var item = lItems[i];
                        sItem.Values.Add("Error " + i, 
                            new StatusItemValueItem("Date", item.Timestamp),
                            new StatusItemValueItem("Group", item.GroupName),
                            new StatusItemValueItem("Type", item.TypeName),
                            new StatusItemValueItem("Message", item.Message.RemoveInvalidXmlChars()),
                            new StatusItemValueItem("StackTrace", item.Exception?.StackTrace.RemoveInvalidXmlChars())
                        );
                    }
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
        private bool _disposedValue; // To detect redundant calls
        /// <summary>
        /// Dispose the instance resources
        /// </summary>
        /// <param name="disposing">true if should dispose managed resources.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                _itemsWorker?.Stop(int.MaxValue);
                _itemsWorker?.Clear();
                _lastLogItemsWorker?.Stop(50);
                _lastLogItemsWorker?.Clear();
                Storage?.Dispose();
            }
            _disposedValue = true;
        }
        /// <inheritdoc />
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
        /// <inheritdoc />
        /// <summary>
        /// Write a log item into the log storages
        /// </summary>
        /// <param name="item">Log item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ILogItem item)
        {
            if (!Enabled || item == null || item.Level > MaxLogLevel || item.Level > Storage.GetMaxLogLevel()) return;
            if (_itemsWorker?.Count < MaximumItemsInQueue)
            {
                _completationHandler.Reset();
                _itemsWorker?.Enqueue(item);
            }
            if (item.Level == LogLevel.Error)
                _lastLogItemsWorker?.Enqueue(item);
        }
        /// <inheritdoc />
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
            if (!Enabled || level > MaxLogLevel || level > Storage.GetMaxLogLevel()) return;
            var item = ItemFactory(level, code, message, groupName, ex, assemblyName, typeName);
            Write(item);
            if (level == LogLevel.Error)
                Start();
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a log item into the log storages
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="code">Item code</param>
        /// <param name="message">Item message</param>
        /// <param name="ex">Related exception if is available</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(LogLevel level, string code, string message, Exception ex = null) => Write(level, code, message, null, ex);
        /// <inheritdoc />
        /// <summary>
        /// Write a log item into the log storages
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="message">Item message</param>
        /// <param name="ex">Related exception if is available</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(LogLevel level, string message, Exception ex = null) => Write(level, null, message, null, ex);
        /// <inheritdoc />
        /// <summary>
        /// Write a log item into the log storages
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="ex">Related exception if is available</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(LogLevel level, Exception ex) => Write(level, null, (ex as SerializableException.WrappedException)?.Message ?? ex.Message, null, ex);
        /// <inheritdoc />
        /// <summary>
        /// Write a log item into the log storages
        /// </summary>
        /// <param name="ex">Related exception if is available</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Exception ex) => Write(LogLevel.Error, null, (ex as SerializableException.WrappedException)?.Message ?? ex.Message, null, ex);
        
        #region Basic Members
        /// <inheritdoc />
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
                WriteUnsafe(LogLevel.Debug, null, message, args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a verbose item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Verbose(string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.Verbose))
                WriteUnsafe(LogLevel.Verbose, null, message, args);
        }
        /// <inheritdoc />
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
                WriteUnsafe(LogLevel.Error, null, message, args, ex);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, null, message, args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a warning item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warning(string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.Warning))
                WriteUnsafe(LogLevel.Warning, null, message, args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoLevel1 item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoBasic(string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.InfoBasic))
                WriteUnsafe(LogLevel.InfoBasic, null, message, args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoMedium item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoMedium(string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.InfoMedium))
                WriteUnsafe(LogLevel.InfoMedium, null, message, args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoDetailed item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoDetail(string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.InfoDetail))
                WriteUnsafe(LogLevel.InfoDetail, null, message, args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a Stats item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stats(string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.Stats))
                WriteUnsafe(LogLevel.Stats, null, message, args);
        }
        #endregion
        
        #region Basic Group Members
        /// <inheritdoc />
        /// <summary>
        /// Write a debug item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DebugGroup(string groupName, string message, params object[] args)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.Debug))
                WriteUnsafe(LogLevel.Debug, groupName, message, args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a verbose item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VerboseGroup(string groupName, string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.Verbose))
                WriteUnsafe(LogLevel.Verbose, groupName, message, args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorGroup(Exception ex, string groupName, string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, groupName, message, args, ex);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorGroup(string groupName, string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, groupName, message, args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a warning item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WarningGroup(string groupName, string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.Warning))
                WriteUnsafe(LogLevel.Warning, groupName, message, args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoLevel1 item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoBasicGroup(string groupName, string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.InfoBasic))
                WriteUnsafe(LogLevel.InfoBasic, groupName, message, args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoMedium item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoMediumGroup(string groupName, string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.InfoMedium))
                WriteUnsafe(LogLevel.InfoMedium, groupName, message, args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoDetailed item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoDetailGroup(string groupName, string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.InfoDetail))
                WriteUnsafe(LogLevel.InfoDetail, groupName, message, args);
        }
        #endregion
        
        /// <inheritdoc />
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
                WriteUnsafe(LogLevel.LibDebug, null, message, args);
        }
        /// <inheritdoc />
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
                WriteUnsafe(LogLevel.LibVerbose, null, message, args);
        }
        /// <inheritdoc />
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
        /// <inheritdoc />
        /// <summary>
        /// Get all pending to write log items
        /// </summary>
        /// <returns>Pending log items array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogItem[] GetPendingItems() => _itemsWorker?.QueueItems?.ToArray();
        /// <inheritdoc />
        /// <summary>
        /// Enqueue to write the log items to the log storages
        /// </summary>
        /// <param name="items">Log items range</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnqueueItemsArray(ILogItem[] items) => items?.Each(i => _itemsWorker.Enqueue(i));
        /// <inheritdoc />
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
        private bool IsLogLevelValid(LogLevel logLevel)
        {
            return Storage.Count <= 0 || (logLevel <= MaxLogLevel && logLevel <= Storage.GetMaxLogLevel());
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteUnsafe(LogLevel level, string groupName, string message, object[] args, Exception ex = null)
        {
            if (!Enabled || !(_itemsWorker?.Count < MaximumItemsInQueue)) return;
            if (args?.Length > 0)
                message = string.Format(message, args);
            var item = ItemFactory(level, null, message, groupName, ex, null, null);
            if (item == null) return;
            _itemsWorker?.Enqueue(item);
            if (item.Level != LogLevel.Error) return;
            _lastLogItemsWorker?.Enqueue(item);
            Start();
        }
        #endregion
    }
}
