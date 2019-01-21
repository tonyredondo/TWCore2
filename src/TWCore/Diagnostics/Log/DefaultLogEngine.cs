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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Diagnostics.Log.Storages;
using TWCore.Diagnostics.Status;
using TWCore.Serialization;
using TWCore.Threading;
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Diagnostics.Log
{
    /// <inheritdoc />
    /// <summary>
    /// Default log engine
    /// </summary>
    [IgnoreStackFrameLog]
    [StatusName("Application Information\\Log")]
    public sealed class DefaultLogEngine : ILogEngine
    {
        #region Private fields
        private readonly Worker<IGroupItem> _itemsWorker;
        private readonly ConcurrentQueue<IGroupItem> _lastLogItems = new ConcurrentQueue<IGroupItem>();
        private readonly AsyncManualResetEvent _completationHandler = new AsyncManualResetEvent(true);
        #endregion

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Log storages items
        /// </summary>
        [StatusReference]
        public LogStorageCollection Storages { get; }
        /// <inheritdoc />
        /// <summary>
        /// Max log level to register in logs
        /// </summary>
        [StatusProperty]
        public LogLevel MaxLogLevel { get; set; }
        /// <summary>
        /// Gets or sets the maximum items quantity on the queue
        /// </summary>
        [StatusProperty]
        public int MaximumItemsInQueue { get; set; } = Core.GlobalSettings.LogMaximumItemsInQueue;
        /// <summary>
        /// Gets a log done task that completes when the log has finished to write
        /// </summary>
        public Task LogDoneTask => _completationHandler.WaitAsync();
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DefaultLogEngine()
        {
            MaxLogLevel = LogLevel.LibVerbose;
            Storages = new LogStorageCollection();
            _itemsWorker = new Worker<IGroupItem>(() => Storages?.Count > 0, item =>
            {
                switch (item)
                {
                    case null:
                        return Task.CompletedTask;
                    case NewLineLogItem nlineItem:
                        ReferencePool<NewLineLogItem>.Shared.Store(nlineItem);
                        return Storages.WriteEmptyLineAsync();
                    case LogItem lItem:
                        return Storages.WriteAsync(lItem).ContinueWith((tsk, state) => LogItem.Store((LogItem)state), lItem, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                    case ILogItem iLitem:
                        return Storages.WriteAsync(iLitem);
                    case IGroupMetadata gMeta:
                        return Storages.WriteAsync(gMeta);
                    default:
                        return Task.CompletedTask;
                }
            }, false, true, false);
            _itemsWorker.OnWorkDone += (s, e) => _completationHandler.Set();
            Core.Status.AttachObject(this);
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
                _itemsWorker?.StopAsync(int.MaxValue).WaitAsync();
                _itemsWorker?.Clear();
                Storages?.Dispose();
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
        
        #region Write Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a log item into the log storages
        /// </summary>
        /// <param name="item">Log item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ILogItem item)
        {
            if (!Enabled || item is null || item.Level > MaxLogLevel || item.Level > Storages.GetMaxLogLevel()) return;
            if (_itemsWorker?.Count < MaximumItemsInQueue)
            {
                _completationHandler.Reset();
                _itemsWorker?.Enqueue(item);
            }
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
            if (!Enabled || level > MaxLogLevel || level > Storages.GetMaxLogLevel()) return;
            var item = CreateLogItem(level, code, message, groupName, ex, assemblyName, typeName);
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
        #endregion
        
        #region Basic Members
        
        #region Debug Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a debug item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Debug(string message)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.Debug))
                WriteUnsafe(LogLevel.Debug, null, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a debug item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Debug<T1>(string message, in T1 arg1)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.Debug))
                WriteUnsafe(LogLevel.Debug, null, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a debug item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Debug<T1, T2>(string message, in T1 arg1, in T2 arg2)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.Debug))
                WriteUnsafe(LogLevel.Debug, null, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a debug item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Debug<T1, T2, T3>(string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.Debug))
                WriteUnsafe(LogLevel.Debug, null, message, arg1, arg2, arg3);
        }
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
        #endregion
        
        #region Verbose Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a verbose item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Verbose(string message)
        {
            if (IsLogLevelValid(LogLevel.Verbose))
                WriteUnsafe(LogLevel.Verbose, null, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a verbose item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Verbose<T1>(string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.Verbose))
                WriteUnsafe(LogLevel.Verbose, null, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a verbose item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Verbose<T1, T2>(string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.Verbose))
                WriteUnsafe(LogLevel.Verbose, null, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a verbose item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Verbose<T1, T2, T3>(string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.Verbose))
                WriteUnsafe(LogLevel.Verbose, null, message, arg1, arg2, arg3);
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
        #endregion
        
        #region Error Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(Exception ex, string message)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, null, message, ex);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error<T1>(Exception ex, string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, null, message, arg1, ex);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error<T1, T2>(Exception ex, string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, null, message, arg1, arg2, ex);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error<T1, T2, T3>(Exception ex, string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, null, message, arg1, arg2, arg3, ex);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(string message)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, null, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error<T1>(string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, null, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error<T1, T2>(string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, null, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error<T1, T2, T3>(string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, null, message, arg1, arg2, arg3);
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
        #endregion
        
        #region Warning Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a warning item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warning(string message)
        {
            if (IsLogLevelValid(LogLevel.Warning))
                WriteUnsafe(LogLevel.Warning, null, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a warning item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warning<T1>(string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.Warning))
                WriteUnsafe(LogLevel.Warning, null, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a warning item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warning<T1, T2>(string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.Warning))
                WriteUnsafe(LogLevel.Warning, null, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a warning item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warning<T1, T2, T3>(string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.Warning))
                WriteUnsafe(LogLevel.Warning, null, message, arg1, arg2, arg3);
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
        #endregion
        
        #region InfoBasic Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoLevel1 item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoBasic(string message)
        {
            if (IsLogLevelValid(LogLevel.InfoBasic))
                WriteUnsafe(LogLevel.InfoBasic, null, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoLevel1 item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoBasic<T1>(string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.InfoBasic))
                WriteUnsafe(LogLevel.InfoBasic, null, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoLevel1 item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoBasic<T1, T2>(string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.InfoBasic))
                WriteUnsafe(LogLevel.InfoBasic, null, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoLevel1 item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoBasic<T1, T2, T3>(string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.InfoBasic))
                WriteUnsafe(LogLevel.InfoBasic, null, message, arg1, arg2, arg3);
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
        #endregion
        
        #region InfoMedium Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoMedium item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoMedium(string message)
        {
            if (IsLogLevelValid(LogLevel.InfoMedium))
                WriteUnsafe(LogLevel.InfoMedium, null, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoMedium item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoMedium<T1>(string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.InfoMedium))
                WriteUnsafe(LogLevel.InfoMedium, null, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoMedium item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoMedium<T1, T2>(string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.InfoMedium))
                WriteUnsafe(LogLevel.InfoMedium, null, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoMedium item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoMedium<T1, T2, T3>(string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.InfoMedium))
                WriteUnsafe(LogLevel.InfoMedium, null, message, arg1, arg2, arg3);
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
        #endregion
        
        #region InfoDetail Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoDetailed item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoDetail(string message)
        {
            if (IsLogLevelValid(LogLevel.InfoDetail))
                WriteUnsafe(LogLevel.InfoDetail, null, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoDetailed item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoDetail<T1>(string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.InfoDetail))
                WriteUnsafe(LogLevel.InfoDetail, null, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoDetailed item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoDetail<T1, T2>(string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.InfoDetail))
                WriteUnsafe(LogLevel.InfoDetail, null, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoDetailed item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoDetail<T1, T2, T3>(string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.InfoDetail))
                WriteUnsafe(LogLevel.InfoDetail, null, message, arg1, arg2, arg3);
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
        #endregion
        
        #region Stats Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a Stats item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stats(string message)
        {
            if (IsLogLevelValid(LogLevel.Stats))
                WriteUnsafe(LogLevel.Stats, null, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a Stats item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stats<T1>(string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.Stats))
                WriteUnsafe(LogLevel.Stats, null, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a Stats item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stats<T1, T2>(string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.Stats))
                WriteUnsafe(LogLevel.Stats, null, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a Stats item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stats<T1, T2, T3>(string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.Stats))
                WriteUnsafe(LogLevel.Stats, null, message, arg1, arg2, arg3);
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

        #region LibDebug Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a LibDebug item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LibDebug(string message)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.LibDebug))
                WriteUnsafe(LogLevel.LibDebug, null, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a LibDebug item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LibDebug<T1>(string message, in T1 arg1)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.LibDebug))
                WriteUnsafe(LogLevel.LibDebug, null, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a LibDebug item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LibDebug<T1, T2>(string message, in T1 arg1, in T2 arg2)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.LibDebug))
                WriteUnsafe(LogLevel.LibDebug, null, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a LibDebug item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LibDebug<T1, T2, T3>(string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.LibDebug))
                WriteUnsafe(LogLevel.LibDebug, null, message, arg1, arg2, arg3);
        }
        /// <summary>
        /// Write a LibDebug item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        /// <param name="arg4">Fourth argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LibDebug<T1, T2, T3, T4>(string message, in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.LibDebug))
                WriteUnsafe(LogLevel.LibDebug, null, message, arg1, arg2, arg3, arg4);
        }
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
        #endregion

        #region LibVerbose Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a LibVerbose item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LibVerbose(string message)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.LibVerbose))
                WriteUnsafe(LogLevel.LibVerbose, null, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a LibVerbose item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LibVerbose<T1>(string message, in T1 arg1)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.LibVerbose))
                WriteUnsafe(LogLevel.LibVerbose, null, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a LibVerbose item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LibVerbose<T1, T2>(string message, in T1 arg1, in T2 arg2)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.LibVerbose))
                WriteUnsafe(LogLevel.LibVerbose, null, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a LibVerbose item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LibVerbose<T1, T2, T3>(string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.LibVerbose))
                WriteUnsafe(LogLevel.LibVerbose, null, message, arg1, arg2, arg3);
        }
        /// <summary>
        /// Write a LibVerbose item into the log storages
        /// </summary>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        /// <param name="arg4">Fourth argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LibVerbose<T1, T2, T3, T4>(string message, in T1 arg1, in T2 arg2, in T3 arg3, in T4 arg4)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.LibVerbose))
                WriteUnsafe(LogLevel.LibVerbose, null, message, arg1, arg2, arg3, arg4);
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
        #endregion

        #endregion

        #region Basic Group Members

        #region DebugGroup Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a debug item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DebugGroup(string groupName, string message)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.Debug))
                WriteUnsafe(LogLevel.Debug, groupName, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a debug item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DebugGroup<T1>(string groupName, string message, in T1 arg1)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.Debug))
                WriteUnsafe(LogLevel.Debug, groupName, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a debug item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DebugGroup<T1, T2>(string groupName, string message, in T1 arg1, in T2 arg2)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.Debug))
                WriteUnsafe(LogLevel.Debug, groupName, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a debug item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DebugGroup<T1, T2, T3>(string groupName, string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (!Core.DebugMode) return;
            if (IsLogLevelValid(LogLevel.Debug))
                WriteUnsafe(LogLevel.Debug, groupName, message, arg1, arg2, arg3);
        }
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
        #endregion
        
        #region VerboseGroup Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a verbose item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VerboseGroup(string groupName, string message)
        {
            if (IsLogLevelValid(LogLevel.Verbose))
                WriteUnsafe(LogLevel.Verbose, groupName, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a verbose item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VerboseGroup<T1>(string groupName, string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.Verbose))
                WriteUnsafe(LogLevel.Verbose, groupName, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a verbose item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VerboseGroup<T1, T2>(string groupName, string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.Verbose))
                WriteUnsafe(LogLevel.Verbose, groupName, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a verbose item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VerboseGroup<T1, T2, T3>(string groupName, string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.Verbose))
                WriteUnsafe(LogLevel.Verbose, groupName, message, arg1, arg2, arg3);
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
        #endregion

        #region ErrorGroup Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="groupName">Group name</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorGroup(Exception ex, string groupName)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, groupName, ex?.Message, ex);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorGroup(Exception ex, string groupName, string message)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, groupName, message, ex);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorGroup<T1>(Exception ex, string groupName, string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, groupName, message, arg1, ex);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorGroup<T1, T2>(Exception ex, string groupName, string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, groupName, message, arg1, arg2, ex);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorGroup<T1, T2, T3>(Exception ex, string groupName, string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, groupName, message, arg1, arg2, arg3, ex);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorGroup(string groupName, string message)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, groupName, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorGroup<T1>(string groupName, string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, groupName, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorGroup<T1, T2>(string groupName, string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, groupName, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a error item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorGroup<T1, T2, T3>(string groupName, string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.Error))
                WriteUnsafe(LogLevel.Error, groupName, message, arg1, arg2, arg3);
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
        #endregion
        
        #region WarningGroup Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a warning item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WarningGroup(string groupName, string message)
        {
            if (IsLogLevelValid(LogLevel.Warning))
                WriteUnsafe(LogLevel.Warning, groupName, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a warning item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WarningGroup<T1>(string groupName, string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.Warning))
                WriteUnsafe(LogLevel.Warning, groupName, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a warning item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WarningGroup<T1, T2>(string groupName, string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.Warning))
                WriteUnsafe(LogLevel.Warning, groupName, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a warning item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WarningGroup<T1, T2, T3>(string groupName, string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.Warning))
                WriteUnsafe(LogLevel.Warning, groupName, message, arg1, arg2, arg3);
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
        #endregion
        
        #region InfoBasicGroup Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoLevel1 item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoBasicGroup(string groupName, string message)
        {
            if (IsLogLevelValid(LogLevel.InfoBasic))
                WriteUnsafe(LogLevel.InfoBasic, groupName, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoLevel1 item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoBasicGroup<T1>(string groupName, string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.InfoBasic))
                WriteUnsafe(LogLevel.InfoBasic, groupName, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoLevel1 item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoBasicGroup<T1, T2>(string groupName, string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.InfoBasic))
                WriteUnsafe(LogLevel.InfoBasic, groupName, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoLevel1 item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoBasicGroup<T1, T2, T3>(string groupName, string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.InfoBasic))
                WriteUnsafe(LogLevel.InfoBasic, groupName, message, arg1, arg2, arg3);
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
        #endregion
        
        #region InfoMediumGroup Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoMedium item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoMediumGroup(string groupName, string message)
        {
            if (IsLogLevelValid(LogLevel.InfoMedium))
                WriteUnsafe(LogLevel.InfoMedium, groupName, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoMedium item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoMediumGroup<T1>(string groupName, string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.InfoMedium))
                WriteUnsafe(LogLevel.InfoMedium, groupName, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoMedium item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoMediumGroup<T1, T2>(string groupName, string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.InfoMedium))
                WriteUnsafe(LogLevel.InfoMedium, groupName, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoMedium item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoMediumGroup<T1, T2, T3>(string groupName, string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.InfoMedium))
                WriteUnsafe(LogLevel.InfoMedium, groupName, message, arg1, arg2, arg3);
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
        #endregion
        
        #region InfoDetailGroup Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoDetailed item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoDetailGroup(string groupName, string message)
        {
            if (IsLogLevelValid(LogLevel.InfoDetail))
                WriteUnsafe(LogLevel.InfoDetail, groupName, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoDetailed item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoDetailGroup<T1>(string groupName, string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.InfoDetail))
                WriteUnsafe(LogLevel.InfoDetail, groupName, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoDetailed item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoDetailGroup<T1, T2>(string groupName, string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.InfoDetail))
                WriteUnsafe(LogLevel.InfoDetail, groupName, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a InfoDetailed item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InfoDetailGroup<T1, T2, T3>(string groupName, string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.InfoDetail))
                WriteUnsafe(LogLevel.InfoDetail, groupName, message, arg1, arg2, arg3);
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

        #region StatsGroup Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a Stats item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StatsGroup(string groupName, string message)
        {
            if (IsLogLevelValid(LogLevel.Stats))
                WriteUnsafe(LogLevel.Stats, groupName, message);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a Stats item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StatsGroup<T1>(string groupName, string message, in T1 arg1)
        {
            if (IsLogLevelValid(LogLevel.Stats))
                WriteUnsafe(LogLevel.Stats, groupName, message, arg1);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a Stats item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StatsGroup<T1, T2>(string groupName, string message, in T1 arg1, in T2 arg2)
        {
            if (IsLogLevelValid(LogLevel.Stats))
                WriteUnsafe(LogLevel.Stats, groupName, message, arg1, arg2);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a Stats item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="arg1">First argument</param>
        /// <param name="arg2">Second argument</param>
        /// <param name="arg3">Third argument</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StatsGroup<T1, T2, T3>(string groupName, string message, in T1 arg1, in T2 arg2, in T3 arg3)
        {
            if (IsLogLevelValid(LogLevel.Stats))
                WriteUnsafe(LogLevel.Stats, groupName, message, arg1, arg2, arg3);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a Stats item into the log storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="message">Item message with pattern support</param>
        /// <param name="args">Arguments to bing with the pattern</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StatsGroup(string groupName, string message, params object[] args)
        {
            if (IsLogLevelValid(LogLevel.Stats))
                WriteUnsafe(LogLevel.Stats, groupName, message, args);
        }
        #endregion

        #endregion
        
        #region WriteEmptyLine Method
        /// <inheritdoc />
        /// <summary>
        /// Write a log empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEmptyLine()
        {
            Write(ReferencePool<NewLineLogItem>.Shared.New());
        }
        #endregion

        #endregion

        #region Group Metadata
        /// <inheritdoc />
        /// <summary>
        /// Adds metadata values to the group name
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="keyValues">Key value pairs to add</param>
        public void AddGroupMetadata(string groupName, params KeyValue[] keyValues)
        {
            if (string.IsNullOrWhiteSpace(groupName) || keyValues is null) return;
            _itemsWorker?.Enqueue(new GroupMetadata
            {
                InstanceId = Core.InstanceId,
                Timestamp = Core.Now,
                GroupName = groupName,
                Items = keyValues
            });
        }
        /// <inheritdoc />
        /// <summary>
        /// Adds metadata values to the group name
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="keyValues">Key value pairs to add</param>
        public void AddGroupMetadata(string groupName, params (string Key, string Value)[] keyValues)
        {
            if (string.IsNullOrWhiteSpace(groupName) || keyValues is null) return;
            var nValues = new KeyValue[keyValues.Length];
            for (var i = 0; i < keyValues.Length; i++)
                nValues[i] = new KeyValue(keyValues[i].Key, keyValues[i].Value);
            _itemsWorker?.Enqueue(new GroupMetadata
            {
                InstanceId = Core.InstanceId,
                Timestamp = Core.Now,
                GroupName = groupName,
                Items = nValues
            });
        }
        #endregion


        #region Pending Items
        /// <inheritdoc />
        /// <summary>
        /// Get all pending to write log items
        /// </summary>
        /// <returns>Pending log items array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IGroupItem[] GetPendingItems() => _itemsWorker?.QueueItems?.ToArray();
        /// <inheritdoc />
        /// <summary>
        /// Enqueue to write the log items to the log storages
        /// </summary>
        /// <param name="items">Log items range</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnqueueItemsArray(IGroupItem[] items)
        {
            if (items is null) return;
            foreach (var i in items)
                _itemsWorker.Enqueue(i);
		} 
        /// <inheritdoc />
        /// <summary>
        /// Starts the Log Engine
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start() 
        {
            _itemsWorker.Start();
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsLogLevelValid(LogLevel logLevel)
            => Storages.Count <= 0 || (logLevel <= MaxLogLevel && logLevel <= Storages.GetMaxLogLevel());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteUnsafe(LogLevel level, string groupName, string message, Exception ex = null)
        {
            if (!Enabled || !(_itemsWorker?.Count < MaximumItemsInQueue)) return;
            WriteUnsafeInternal(level, groupName, message, ex);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteUnsafe(LogLevel level, string groupName, string message, object arg1, Exception ex = null)
        {
            if (!Enabled || !(_itemsWorker?.Count < MaximumItemsInQueue)) return;
            message = string.Format(message, arg1);
            WriteUnsafeInternal(level, groupName, message, ex);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteUnsafe(LogLevel level, string groupName, string message, object arg1, object arg2, Exception ex = null)
        {
            if (!Enabled || !(_itemsWorker?.Count < MaximumItemsInQueue)) return;
            message = string.Format(message, arg1, arg2);
            WriteUnsafeInternal(level, groupName, message, ex);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteUnsafe(LogLevel level, string groupName, string message, object arg1, object arg2, object arg3, Exception ex = null)
        {
            if (!Enabled || !(_itemsWorker?.Count < MaximumItemsInQueue)) return;
            message = string.Format(message, arg1, arg2, arg3);
            WriteUnsafeInternal(level, groupName, message, ex);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteUnsafe(LogLevel level, string groupName, string message, object arg1, object arg2, object arg3, object arg4, Exception ex = null)
        {
            if (!Enabled || !(_itemsWorker?.Count < MaximumItemsInQueue)) return;
            message = string.Format(message, arg1, arg2, arg3, arg4);
            WriteUnsafeInternal(level, groupName, message, ex);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteUnsafe(LogLevel level, string groupName, string message, object[] args, Exception ex = null)
        {
            if (!Enabled || !(_itemsWorker?.Count < MaximumItemsInQueue)) return;
            if (args?.Length > 0)
                message = string.Format(message, args);
            WriteUnsafeInternal(level, groupName, message, ex);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteUnsafeInternal(LogLevel level, string groupName, string message, Exception ex = null)
        {
            var item = CreateLogItem(level, null, message, groupName, ex, null, null);
            if (item is null) return;
            _itemsWorker?.Enqueue(item);
            if (item.Level != LogLevel.Error) return;
            Start();
        }
        #endregion

        #region Log Item Creation
        private static readonly (string AssemblyName, string TypeName) DefaultMValue = (null, null);
        private static readonly ConcurrentDictionary<MethodBase, (string AssemblyName, string TypeName)> MethodValues = new ConcurrentDictionary<MethodBase, (string AssemblyName, string TypeName)>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static LogItem CreateLogItem(LogLevel level, string code, string message, string groupName, Exception ex, string assemblyName, string typeName)
        {
            if (assemblyName is null || typeName is null)
            {
                var stack = new StackTrace(2, false);
                var frames = stack.GetFrames();
                foreach (var frame in frames)
                {
                    var method = frame.GetMethod();
                    if (method is null) continue;
                    var value = MethodValues.GetOrAdd(method, cMethod =>
                    {
                        #region Name Attr
                        var attrs = method.GetCustomAttributes(false);
                        for (var i = 0; i < attrs.Length; i++)
                        {
                            switch (attrs[i])
                            {
                                case IgnoreStackFrameLogAttribute _:
                                    return DefaultMValue;
                                case StackFrameLogAttribute nA:
                                    assemblyName = method.DeclaringType.Assembly.FullName;
                                    typeName = nA.ClassName;
                                    return (assemblyName, typeName);
                            }
                        }
                        #endregion

                        var declarationType = method.DeclaringType;
                        if (declarationType is null) return DefaultMValue;

                        #region Name Type Attr
                        var typeAttrs = declarationType.GetCustomAttributes(false);
                        for (var i = 0; i < typeAttrs.Length; i++)
                        {
                            switch (typeAttrs[i])
                            {
                                case IgnoreStackFrameLogAttribute _:
                                    return DefaultMValue;
                                case StackFrameLogAttribute nA:
                                    assemblyName = declarationType.Assembly.FullName;
                                    typeName = nA.ClassName;
                                    return (assemblyName, typeName);
                            }
                        }
                        #endregion

                        if (method.Name.Contains("MoveNext"))
                        {
                            var actualType = declarationType.DeclaringType;
                            if (actualType.Assembly == typeof(Core).Assembly) return DefaultMValue;

                            #region Actual type attrs
                            var actualTypeAttrs = actualType.GetCustomAttributes(false);
                            for (var i = 0; i < actualTypeAttrs.Length; i++)
                            {
                                switch (actualTypeAttrs[i])
                                {
                                    case IgnoreStackFrameLogAttribute _:
                                        return DefaultMValue;
                                    case StackFrameLogAttribute nA:
                                        assemblyName = actualType.Assembly.FullName;
                                        typeName = nA.ClassName;
                                        return (assemblyName, typeName);
                                }
                            }
                            #endregion

                            if (actualType.AssemblyQualifiedName.Contains("System.Private")) return DefaultMValue;
                            if (actualType.AssemblyQualifiedName.Contains("mscorlib")) return DefaultMValue;

                            assemblyName = actualType.Assembly.FullName;
                            typeName = actualType.Name;
                            if (actualType.ReflectedType != null && typeName?.Contains("<") == true)
                                typeName = actualType.ReflectedType.Name;
                            return(assemblyName, typeName);
                        }

                        if (!method.Name.Contains("<") &&
                            !declarationType.Name.Contains("<") &&
                            !declarationType.AssemblyQualifiedName.Contains("System.Private") &&
                            !declarationType.AssemblyQualifiedName.Contains("mscorlib"))
                        {
                            if (declarationType.Name.Contains("ConcurrentDictionary"))
                                return DefaultMValue;
                            if (declarationType.Name.Contains("CacheCollectionBase`3"))
                                return DefaultMValue;
                            assemblyName = declarationType.Assembly.FullName;
                            typeName = declarationType.Name;
                            return (assemblyName, typeName);
                        }

                        return DefaultMValue;
                    });
                    if (value.AssemblyName == DefaultMValue.AssemblyName && value.TypeName == DefaultMValue.TypeName) continue;
                    assemblyName = value.AssemblyName;
                    typeName = value.TypeName;
                    break;
                }
            }
            if (!Core.DebugMode && assemblyName == typeof(Core).Assembly.FullName && level > LogLevel.Stats)
                return null;

            var logId = Guid.NewGuid();

            if (Core.GlobalSettings.DumpDeserializerExceptionGenericObject && ex is DeserializerException dEx)
            {
                try
                {
                    var file = $"{logId}.DEX.json";
                    dEx.SerializeToJsonFile(file);
                    ex.Data["ExceptionFileName"] = file;
                }
                catch
                {
                }
            }

            var lItem = LogItem.Retrieve();
            lItem.InstanceId = Core.InstanceId;
            lItem.Id = logId;
            lItem.EnvironmentName = Core.EnvironmentName;
            lItem.MachineName = Core.MachineName;
            lItem.Timestamp = Core.Now;
            lItem.ApplicationName = Core.ApplicationName;
            lItem.Level = level;
            lItem.Code = code;
            lItem.Message = message;
            lItem.GroupName = groupName;
            lItem.AssemblyName = assemblyName;
            lItem.TypeName = typeName;
            lItem.Exception = ex != null ? new SerializableException(ex) : null;
            return lItem;
        }
        #endregion
    }
}
