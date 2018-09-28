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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace TWCore
{
    /// <summary>
    /// Helper to calculate the execution time of functions and actions.
    /// </summary>
    [IgnoreStackFrameLog]
    public static class Watch
    {
        /// <summary>
        /// Internal action worker for async statistics
        /// </summary>
        private static readonly ActionWorker<Stopwatch> Worker = new ActionWorker<Stopwatch>();

        #region Functions
        /// <summary>
        /// Invokes the function and returns the value, calls the onMethodEnds function at the end of the function with the execution time.
        /// </summary>
        /// <typeparam name="T">Type of the function</typeparam>
        /// <param name="invokeMethod">Function to invoke</param>
        /// <param name="onMethodEnds">Action to be executed at the end of the invoked method with the execution time data</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>Function return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Invoke<T>(Func<T> invokeMethod, Action<Stopwatch> onMethodEnds, bool asyncRun = false)
        {
            var sw = Stopwatch.StartNew();
            var res = invokeMethod();
            sw.Stop();
            if (asyncRun)
                Worker.Enqueue(onMethodEnds, sw);
            else
                onMethodEnds(sw);
            return res;
        }
        /// <summary>
        /// Invokes the function and returns the value, save the stats message on the log instance.
        /// </summary>
        /// <typeparam name="T">Type of the function</typeparam>
        /// <param name="invokeMethod">Function to invoke</param>
        /// <param name="statsMessage">Stats message to write in the log instance</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>Function return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Invoke<T>(Func<T> invokeMethod, string statsMessage, bool asyncRun = false)
        {
            return Invoke(invokeMethod, sw => Core.Log.Stats($"Execution Time: {sw.Elapsed.TotalMilliseconds}ms, for: {statsMessage}"), asyncRun);
        }
        /// <summary>
        /// Invokes the task and returns the value, calls the onMethodEnds function at the end of the function with the execution time.
        /// </summary>
        /// <typeparam name="T">Type of the function</typeparam>
        /// <param name="task">Task to invoke</param>
        /// <param name="onMethodEnds">Action to be executed at the end of the invoked method with the execution time data</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>Function return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<T> InvokeAsync<T>(Func<Task<T>> task, Action<Stopwatch> onMethodEnds, bool asyncRun = false)
        {
            var sw = Stopwatch.StartNew();
            var res = await task().ConfigureAwait(false);
            sw.Stop();
            if (asyncRun)
                Worker.Enqueue(onMethodEnds, sw);
            else
                onMethodEnds(sw);
            return res;
        }
        /// <summary>
        /// Invokes the task and returns the value, save the stats message on the log instance.
        /// </summary>
        /// <typeparam name="T">Type of the function</typeparam>
        /// <param name="task">Task to invoke</param>
        /// <param name="statsMessage">Stats message to write in the log instance</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>Function return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<T> InvokeAsync<T>(Func<Task<T>> task, string statsMessage, bool asyncRun = false)
        {
            return InvokeAsync(task, sw => Core.Log.Stats($"Execution Time: {sw.Elapsed.TotalMilliseconds}ms, for: {statsMessage}"), asyncRun);
        }
        #endregion

        #region Actions
        /// <summary>
        /// Invokes the action and calls the onMethodEnds function at the end of the function with the execution time.
        /// </summary>
        /// <param name="invokeMethod">Action to invoke</param>
        /// <param name="onMethodEnds">Action to be executed at the end of the invoked method with the execution time data</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>Function return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invoke(Action invokeMethod, Action<Stopwatch> onMethodEnds, bool asyncRun = false)
        {
            var sw = Stopwatch.StartNew();
            invokeMethod();
            sw.Stop();
            if (asyncRun)
                Worker.Enqueue(onMethodEnds, sw);
            else
                onMethodEnds(sw);
        }
        /// <summary>
        /// Invokes the action and save the stats message on the log instance.
        /// </summary>
        /// <param name="invokeMethod">Action to invoke</param>
        /// <param name="statsMessage">Stats message to write in the log instance</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>Function return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invoke(Action invokeMethod, string statsMessage, bool asyncRun = false)
        {
            Invoke(invokeMethod, sw => Core.Log.Stats($"Execution Time: {sw.Elapsed.TotalMilliseconds}ms, for: {statsMessage}"), asyncRun);
        }
        /// <summary>
        /// Invokes the task and calls the onMethodEnds function at the end of the function with the execution time.
        /// </summary>
        /// <param name="task">Task to invoke</param>
        /// <param name="onMethodEnds">Action to be executed at the end of the invoked method with the execution time data</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>Function return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task InvokeAsync(Func<Task> task, Action<Stopwatch> onMethodEnds, bool asyncRun = false)
        {
            var sw = Stopwatch.StartNew();
            await task().ConfigureAwait(false);
            sw.Stop();
            if (asyncRun)
                Worker.Enqueue(onMethodEnds, sw);
            else
                onMethodEnds(sw);
        }
        /// <summary>
        /// Invokes the task and save the stats message on the log instance.
        /// </summary>
        /// <param name="task">Task to invoke</param>
        /// <param name="statsMessage">Stats message to write in the log instance</param>
        /// <param name="asyncRun">Execute the onMethodsEnd action in a separated thread to avoid performance impact.</param>
        /// <returns>Function return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task InvokeAsync(Func<Task> task, string statsMessage, bool asyncRun = false)
        {
            return InvokeAsync(task, sw => Core.Log.Stats($"Execution Time: {sw.Elapsed.TotalMilliseconds}ms, for: {statsMessage}"), asyncRun);
        }
        #endregion

        /// <summary>
        /// Create a new watch item to log execution times
        /// </summary>
        /// <returns>Watch item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WItem Create() => WItem.CreateItem();
        /// <summary>
        /// Create a new watch item to log execution times
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>Watch item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WItem Create(LogLevel level) => WItem.CreateItem(level);
        /// <summary>
        /// Create a new watch item to log execution times
        /// </summary>
        /// <param name="message">Message before disposal</param>
        /// <returns>Watch item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WItem Create(string message) => WItem.CreateItem(message);
        /// <summary>
        /// Create a new watch item to log execution times
        /// </summary>
        /// <param name="message">Message before disposal</param>
        /// <param name="level">Log level</param>
        /// <returns>Watch item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WItem Create(string message, LogLevel level) => WItem.CreateItem(message, level);
        /// <summary>
        /// Create a new watch item to log execution times
        /// </summary>
        /// <param name="startMessage">Message start</param>
        /// <param name="endMessage">Message before disposal</param>
        /// <returns>Watch item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WItem Create(string startMessage, string endMessage) => WItem.CreateItem(startMessage, endMessage);
        /// <summary>
        /// Create a new watch item to log execution times
        /// </summary>
        /// <param name="startMessage">Message start</param>
        /// <param name="endMessage">Message before disposal</param>
        /// <param name="level">Log level</param>
        /// <returns>Watch item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WItem Create(string startMessage, string endMessage, LogLevel level) => WItem.CreateItem(startMessage, endMessage, level);
        /// <summary>
        /// Create a new watch item to log execution times
        /// </summary>
        /// <param name="message">Message before disposal</param>
        /// <param name="level">Log level</param>
        /// <param name="group">Group value</param>
        /// <returns>Watch item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WItem Create(string message, LogLevel level, string group) => WItem.CreateItem(message, level, group);
        /// <summary>
        /// Create a new watch item to log execution times
        /// </summary>
        /// <param name="startMessage">Message start</param>
        /// <param name="endMessage">Message before disposal</param>
        /// <param name="group">Group value</param>
        /// <returns>Watch item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WItem Create(string startMessage, string endMessage, string group) => WItem.CreateItem(startMessage, endMessage, group);
        /// <summary>
        /// Create a new watch item to log execution times
        /// </summary>
        /// <param name="startMessage">Message start</param>
        /// <param name="endMessage">Message before disposal</param>
        /// <param name="level">Log level</param>
        /// <param name="group">Group value</param>
        /// <returns>Watch item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WItem Create(string startMessage, string endMessage, LogLevel level, string group) => WItem.CreateItem(startMessage, endMessage, level, group);
        /// <summary>
        /// Enable Watch stats lines indent
        /// </summary>
        public static bool EnableIndent { get; set; } = true;

        #region Nested Class
        /// <inheritdoc />
        /// <summary>
        /// Watch item
        /// </summary>
        [IgnoreStackFrameLog]
        public sealed class WItem : IDisposable
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static readonly ObjectPool<WItem, ItemPoolAllocator> ItemPools = new ObjectPool<WItem, ItemPoolAllocator>();
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static readonly ReferencePool<LogStatItem> LogStatItemPool = new ReferencePool<LogStatItem>();
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static readonly Worker<LogStatItem> LogStatsWorker = new Worker<LogStatItem>(action: WorkerMethod);
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static readonly LRU2QCollection<string, StatusCounter> Counters = new LRU2QCollection<string, StatusCounter>(100);
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static long _frequency = Stopwatch.Frequency;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static int _watcherCount;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static readonly ConcurrentDictionary<int, string> IndentTexts = new ConcurrentDictionary<int, string>();
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static readonly double FrequencyTime = 1000d / Stopwatch.Frequency;

            private struct ItemPoolAllocator : IPoolObjectLifecycle<WItem>
            {
                public int InitialSize => 0;
                public PoolResetMode ResetMode => PoolResetMode.BeforeUse;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public WItem New() => new WItem();
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset(WItem value) => value.Reset();
                public int DropTimeFrequencyInSeconds => 60;
                public void DropAction(WItem value)
                {
                }
            }

            [IgnoreStackFrameLog]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void WorkerMethod(LogStatItem item)
            {
                if (!Core.Log.MaxLogLevel.HasFlag(item.Level))
                {
                    LogStatItemPool.Store(item);
                    return;
                }

                StatusCounter counter = null;
                if (!string.IsNullOrEmpty(item.CounterKey))
                    counter = Counters.GetOrAdd(item.CounterKey, msg => new StatusCounter(CounterPrefix + msg));
                
                double gTime;
                string indent = null;
                string gValue;
                if (string.IsNullOrEmpty(item.Group))
                {
                    gValue = $"{item.Id:00}";
                    if (EnableIndent)
                        indent = IndentTexts.GetOrAdd(item.Id, num => new string(' ', (num - 1) * 3));
                }
                else
                {
                    gValue = item.Group;
                }
                switch (item.Type)
                {
                    case 0:
                        Core.Log.Write(item.Level, null, indent + "[START] " + item.Message, gValue);
                        break;
                    case 1:
                        var cTime = item.LastTapTicks * FrequencyTime;
                        gTime = item.GlobalTicks * FrequencyTime;
                        Core.Log.Write(item.Level, null, indent + $"  [Time = {cTime:0.0000}ms, Cumulated = {gTime:0.0000}ms] {item.Message}", gValue);
                        counter?.Register(item.Message, cTime);
                        break;
                    case 2:
                        gTime = item.GlobalTicks * FrequencyTime;
                        Core.Log.Write(item.Level, null, indent + $"[END: Total Time = {gTime:0.0000}ms] {item.Message}", gValue);
                        counter?.Register(counter.Name == CounterPrefix + item.Message ? "Total" : item.Message, gTime);
                        break;
                }
                LogStatItemPool.Store(item);
            }

            static WItem()
            {
                Counters.NodeRemoved += (key, value) => value.Dispose();
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private double _initTicks;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private double _ticksTimestamp;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private double _lastTapTicks;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private LogLevel _level;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _groupValue;
            private int _id;
            private string _lastMessage;
            private string _counterKey;
            private const string CounterPrefix = "Watch Counters: ";

            #region Properties
            /// <summary>
            /// Elapsed milliseconds since last tap
            /// </summary>
            public double ElapsedMilliseconds => ((Stopwatch.GetTimestamp() - _lastTapTicks) / _frequency) * 1000;
            /// <summary>
            /// Elapsed milliseconds of the last tap
            /// </summary>
            public double LastTapElapsedMilliseconds => (_lastTapTicks / _frequency) * 1000;
            /// <summary>
            /// Elapsed milliseconds since watch creation.
            /// </summary>
            public double GlobalElapsedMilliseconds => ((Stopwatch.GetTimestamp() - _initTicks) / _frequency) * 1000;
            #endregion

            #region Nested Types
            private class LogStatItem
            {
                public int Id;
                public double GlobalTicks;
                public double LastTapTicks;
                public string Message;
                public int Type;
                public LogLevel Level;
                public string Group;
                public string CounterKey;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void SetData(int id, LogLevel level, double globalTicks, double lastTapTicks, string message, string group, string counterKey, int type)
                {
                    Id = id;
                    Level = level;
                    GlobalTicks = globalTicks;
                    LastTapTicks = lastTapTicks;
                    Message = message;
                    Group = group;
                    CounterKey = counterKey;
                    Type = type;
                }
            }
            #endregion

            #region .ctor
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private WItem()
            {
                _frequency = Stopwatch.Frequency;
                _ticksTimestamp = Stopwatch.GetTimestamp();
                _initTicks = _ticksTimestamp;
                _lastTapTicks = _ticksTimestamp;
                _level = LogLevel.Stats;
                _groupValue = null;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static WItem CreateItem()
            {
                var item = ItemPools.New();
                item._id = Interlocked.Increment(ref _watcherCount);
                item._groupValue = null;
                item._counterKey = null;
                return item;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static WItem CreateItem(LogLevel level)
            {
                var item = ItemPools.New();
                item._id = Interlocked.Increment(ref _watcherCount);
                item._groupValue = null;
                item._level = level;
                item._counterKey = null;
                return item;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static WItem CreateItem(string message)
            {
                var item = ItemPools.New();
                item._lastMessage = message;
                item._id = Interlocked.Increment(ref _watcherCount);
                item._groupValue = null;
                item._counterKey = message;
                item.StartedTap(message);
                return item;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static WItem CreateItem(string message, LogLevel level)
            {
                var item = ItemPools.New();
                item._lastMessage = message;
                item._id = Interlocked.Increment(ref _watcherCount);
                item._groupValue = null;
                item._level = level;
                item._counterKey = message;
                item.StartedTap(message);
                return item;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static WItem CreateItem(string startMessage, string lastMessage)
            {
                var item = ItemPools.New();
                item._lastMessage = lastMessage;
                item._id = Interlocked.Increment(ref _watcherCount);
                item._groupValue = null;
                item._counterKey = startMessage;
                item.StartedTap(startMessage ?? lastMessage);
                return item;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static WItem CreateItem(string startMessage, string lastMessage, LogLevel level)
            {
                var item = ItemPools.New();
                item._lastMessage = lastMessage;
                item._id = Interlocked.Increment(ref _watcherCount);
                item._groupValue = null;
                item._level = level;
                item._counterKey = startMessage;
                item.StartedTap(startMessage ?? lastMessage);
                return item;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static WItem CreateItem(string message, LogLevel level, string groupValue)
            {
                var item = ItemPools.New();
                item._lastMessage = message;
                item._id = Interlocked.Increment(ref _watcherCount);
                item._groupValue = groupValue;
                item._level = level;
                item._counterKey = message;
                item.StartedTap(message);
                return item;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static WItem CreateItem(string startMessage, string lastMessage, string groupValue)
            {
                var item = ItemPools.New();
                item._lastMessage = lastMessage;
                item._id = Interlocked.Increment(ref _watcherCount);
                item._groupValue = groupValue;
                item._counterKey = startMessage;
                item.StartedTap(startMessage ?? lastMessage);
                return item;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static WItem CreateItem(string startMessage, string lastMessage, LogLevel level, string groupValue)
            {
                var item = ItemPools.New();
                item._lastMessage = lastMessage;
                item._id = Interlocked.Increment(ref _watcherCount);
                item._groupValue = groupValue;
                item._level = level;
                item._counterKey = startMessage;
                item.StartedTap(startMessage ?? lastMessage);
                return item;
            }
            #endregion


            #region Methods
            /// <summary>
            /// Write a new elapsed time to the stat logs
            /// </summary>
            /// <param name="message">Message of the tap</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Tap(string message)
            {
                var currentTicks = Stopwatch.GetTimestamp();
                _lastTapTicks = currentTicks - _ticksTimestamp;
                var lsi = LogStatItemPool.New();
                lsi.SetData(_id, _level, currentTicks - _initTicks, _lastTapTicks, message, _groupValue, _counterKey, 1);
                LogStatsWorker.Enqueue(lsi);
                _ticksTimestamp = currentTicks;
            }
            /// <summary>
            /// Write a new elapsed time to the stat logs
            /// </summary>
            /// <param name="message">Message of the tap</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void StartedTap(string message)
            {
                var currentTicks = Stopwatch.GetTimestamp();
                _lastTapTicks = currentTicks - _ticksTimestamp;
                if (message != null)
                {
                    var lsi = LogStatItemPool.New();
                    lsi.SetData(_id, _level, currentTicks - _initTicks, _lastTapTicks, message, _groupValue, _counterKey, 0);
                    LogStatsWorker.Enqueue(lsi);
                }
                _ticksTimestamp = currentTicks;
            }
            /// <summary>
            /// Write a new elapsed time to the stat logs
            /// </summary>
            /// <param name="message">Message of the tap</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void EndTap(string message)
            {
                var currentTicks = Stopwatch.GetTimestamp();
                _lastTapTicks = currentTicks - _ticksTimestamp;
                if (message != null)
                {
                    var lsi = LogStatItemPool.New();
                    lsi.SetData(_id, _level, currentTicks - _initTicks, _lastTapTicks, message, _groupValue, _counterKey, 2);
                    LogStatsWorker.Enqueue(lsi);
                }
                _ticksTimestamp = currentTicks;
            }
            /// <summary>
            /// Calls a function to store the elapsed milliseconds
            /// </summary>
            /// <param name="storeFunction">Store function</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void StoreElapsed(Action<double> storeFunction)
            {
                storeFunction?.Invoke(GlobalElapsedMilliseconds);
            }

            /// <summary>
            /// Reset watch
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _level = LogLevel.Stats;
                _disposedValue = false;
                _lastMessage = null;
                _ticksTimestamp = Stopwatch.GetTimestamp();
                _initTicks = _ticksTimestamp;
                _lastTapTicks = _ticksTimestamp;
            }
            #endregion

            #region IDisposable Support
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private bool _disposedValue;
            /// <summary>
            /// Dispose Object
            /// </summary>
            /// <param name="disposing"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Dispose(bool disposing)
            {
                if (_disposedValue) return;
                if (disposing)
                {
                    EndTap(_lastMessage);
                    _lastMessage = null;
                    Interlocked.Decrement(ref _watcherCount);
                    ItemPools.Store(this);
                }
                _disposedValue = true;
            }
            /// <inheritdoc />
            /// <summary>
            /// Dispose Object
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                Dispose(true);
            }
            #endregion

        }
        #endregion
    }
}
