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
using System.Runtime.CompilerServices;
using TWCore.Diagnostics.Status;
using TWCore.Diagnostics.Trace.Storages;

namespace TWCore.Diagnostics.Trace
{
    /// <inheritdoc />
    /// <summary>
    /// Default trace engine
    /// </summary>
    [IgnoreStackFrameLog]
    [StatusName("Application Information\\Trace")]
    public class DefaultTraceEngine : ITraceEngine
    {
        #region Private fields
        private readonly Worker<TraceItem> _itemsWorker;
        private volatile bool _disposed;
        #endregion

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Trace storages items
        /// </summary>
        [StatusReference]
        public TraceStorageCollection Storages { get; }
        /// <inheritdoc />
        /// <summary>
        /// Enable or Disable the Trace engine
        /// </summary>
        [StatusProperty]
        public bool Enabled { get; set; } = Core.GlobalSettings.TraceEnabled;
        #endregion

        #region .ctor
        /// <summary>
        /// Default trace engine
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DefaultTraceEngine()
        {
            Storages = new TraceStorageCollection();
            _itemsWorker = new Worker<TraceItem>(() => !_disposed && Storages.Count > 0, async item =>
            {
                if (item is null) return;
                try
                {
                    await Storages.WriteAsync(item).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Core.Log.Error(ex, "Error writing the Trace item to disk.");
                }
            }, true, true, true);
            Core.Status.AttachObject(this);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~DefaultTraceEngine()
        {
            Dispose();
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Write a trace item into the trace storages
        /// </summary>
        /// <param name="item">Trace item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(TraceItem item)
        {
            if (_disposed) return;
            if (!Enabled || item is null) return;
            if (Storages.Count == 0)
            {
                Core.Log.Warning("There are any trace storage defined. The item can't be traced.");
                return;
            }
            if (_itemsWorker.Count > Core.GlobalSettings.TraceQueueLimit)
            {
                Core.Log.Warning("The trace queue has reached the limit ({0} items), check the configuration to " +
                    "increase the limit or change the trace storage to a faster one. This item will be skipped.", Core.GlobalSettings.TraceQueueLimit);
                return;
            }
            _itemsWorker.Enqueue(item);
        }

        /// <inheritdoc />
        /// <summary>
        /// Write a trace item into the trace storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="traceName">Trace name</param>
        /// <param name="traceObject">Trace object</param>
        /// <param name="tags">Tags</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string groupName, string traceName, object traceObject, params string[] tags)
        {
            if (_disposed) return;
            if (!Enabled) return;
            var item = CreateTraceItem(groupName, traceName, traceObject, tags);
            Write(item);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a trace item into the trace storages
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="traceName">Trace name</param>
        /// <param name="traceObject">Trace object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string groupName, string traceName, object traceObject)
        {
            if (_disposed) return;
            if (!Enabled) return;
            var item = CreateTraceItem(groupName, traceName, traceObject, null);
            Write(item);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a trace item into the trace storages
        /// </summary>
        /// <param name="traceName">Trace name</param>
        /// <param name="traceObject">Trace object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string traceName, object traceObject) => Write(null, traceName, traceObject);
        /// <inheritdoc />
        /// <summary>
        /// Write a trace item into the trace storages
        /// </summary>
        /// <param name="traceObject">Trace object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(object traceObject) => Write(null, traceObject);

        /// <inheritdoc />
        /// <summary>
        /// Write a trace item into the trace storages if is in Debug Mode
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="traceName">Trace name</param>
        /// <param name="traceObject">Trace object</param>
        /// <param name="tags">Tags</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDebug(string groupName, string traceName, object traceObject, params string[] tags)
        {
            if (_disposed) return;
            if (!Enabled) return;
            if (!Core.DebugMode) return;
            var item = CreateTraceItem(groupName, traceName, traceObject, tags);
            Write(item);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a trace item into the trace storages if is in Debug Mode
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="traceName">Trace name</param>
        /// <param name="traceObject">Trace object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDebug(string groupName, string traceName, object traceObject)
        {
            if (_disposed) return;
            if (!Enabled) return;
            if (!Core.DebugMode) return;
            var item = CreateTraceItem(groupName, traceName, traceObject, null);
            Write(item);
        }
        /// <inheritdoc />
        /// <summary>
        /// Write a trace item into the trace storages if is in Debug Mode
        /// </summary>
        /// <param name="traceName">Trace name</param>
        /// <param name="traceObject">Trace object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDebug(string traceName, object traceObject) => WriteDebug(null, traceName, traceObject);
        /// <inheritdoc />
        /// <summary>
        /// Write a trace item into the trace storages if is in Debug Mode
        /// </summary>
        /// <param name="traceObject">Trace object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDebug(object traceObject) => WriteDebug(null, traceObject);

        /// <inheritdoc />
        /// <summary>
        /// Dispose all the object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _itemsWorker?.Stop(true);
            _itemsWorker?.Clear();
            Storages?.Clear();
        }
        #endregion

        #region Trace
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TraceItem CreateTraceItem(string groupName, string traceName, object traceObject, string[] tags)
        {
            return new TraceItem
            {
                InstanceId = Core.InstanceId,
                Id = Factory.NewGuid(),
                Tags = tags,
                Timestamp = Core.Now,
                GroupName = groupName,
                TraceName = traceName,
                TraceObject = traceObject
            };
        }
        #endregion
    }
}
