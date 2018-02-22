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
        #endregion

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Trace storages items
        /// </summary>
        [StatusReference]
        public TraceStorageCollection Storage { get; }
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the trace item factory
        /// </summary>
        public CreateTraceItemDelegate ItemFactory { get; set; } = Factory.CreateTraceItem;
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
        /// <param name="itemFactory">Trace item factory</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DefaultTraceEngine(CreateTraceItemDelegate itemFactory = null)
        {
            Storage = new TraceStorageCollection();
            if (itemFactory != null)
                ItemFactory = itemFactory;

            _itemsWorker = new Worker<TraceItem>(() => Storage?.Count > 0, async item =>
            {
                if (item == null) return;
                try
                {
                    await Storage.WriteAsync(item).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Core.Log.Error(ex, "Error writing the Trace item to disk.");
                }
            }, true, true);
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
            if (Enabled && item != null)
                _itemsWorker?.Enqueue(item);
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
            if (!Enabled) return;
            var item = ItemFactory(groupName, traceName, traceObject);
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
        /// Dispose all the object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _itemsWorker?.StopAsync(50).WaitAsync();
            _itemsWorker?.Clear();
            Storage?.Clear();
        }
        #endregion
    }
}
