﻿/*
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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Log;

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
        static ActionWorker worker = new ActionWorker();

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
                worker.Enqueue(onMethodEnds, sw);
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
                worker.Enqueue(onMethodEnds, sw);
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
        public static async Task<T> InvokeAsync<T>(Func<Task<T>> task, string statsMessage, bool asyncRun = false)
        {
            return await InvokeAsync(task, sw => Core.Log.Stats($"Execution Time: {sw.Elapsed.TotalMilliseconds}ms, for: {statsMessage}"), asyncRun);
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
                worker.Enqueue(onMethodEnds, sw);
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
                worker.Enqueue(onMethodEnds, sw);
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
        public static async Task InvokeAsync(Func<Task> task, string statsMessage, bool asyncRun = false)
        {
            await InvokeAsync(task, sw => Core.Log.Stats($"Execution Time: {sw.Elapsed.TotalMilliseconds}ms, for: {statsMessage}"), asyncRun);
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
        /// <param name="message">Message before disposal</param>
        /// <returns>Watch item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WItem Create(string message) => WItem.CreateItem(message);
        /// <summary>
        /// Create a new watch item to log execution times
        /// </summary>
        /// <param name="startMessage">Message start</param>
        /// <param name="endMessage">Message before disposal</param>
        /// <returns>Watch item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WItem Create(string startMessage, string endMessage) => WItem.CreateItem(startMessage, endMessage);

        #region Nested Class
        /// <summary>
        /// Watch item
        /// </summary>
        [IgnoreStackFrameLog]
        public class WItem : IDisposable
        {
            static long _frequency = Stopwatch.Frequency;
            static int _watcherCount;
            static ObjectPool<WItem> itemPools = new ObjectPool<WItem>(() => new WItem(), i => i.Reset());
            static Worker<LogStatItem> logStatsWorker = new Worker<LogStatItem>(WorkerMethod);

            [IgnoreStackFrameLog]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void WorkerMethod(LogStatItem item)
            {
                double gTime;
                double cTime;
                switch (item.Type)
                {
                    case 0:
                        Core.Log.Stats(new string(' ', (item.Id - 1) * 2) + "[{0:00}] {1}", item.Id, item.Message);
                        break;
                    case 1:
                        cTime = (item.LastTapTicks / _frequency) * 1000;
                        if (item.LastTapTicks != item.GlobalTicks)
                        {
                            gTime = (item.GlobalTicks / _frequency) * 1000;
                            Core.Log.Stats(new string(' ', (item.Id - 1) * 2) + "[{0:00}, Time: {1:0.0000}ms, Cumulated = {2:0.0000}] {3}", item.Id, cTime, gTime, item.Message);
                        }
                        else
                        {
                            Core.Log.Stats(new string(' ', (item.Id - 1) * 2) + "[{0:00}, Time: {1:0.0000}ms] {2}", item.Id, cTime, item.Message);
                        }
                        break;
                    case 2:
                        cTime = (item.LastTapTicks / _frequency) * 1000;
                        gTime = (item.GlobalTicks / _frequency) * 1000;
                        Core.Log.Stats(new string(' ', (item.Id - 1) * 2) + "[{0:00}, Time: {1:0.0000}ms, Total = {2:0.0000}] {3}", item.Id, cTime, gTime, item.Message);
                        break;
                }
            }


            double _initTicks;
            double _ticksTimestamp;
            double _lastTapTicks;
            int _id;
            string _lastMessage = null;

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
            struct LogStatItem
            {
                public int Id;
                public double GlobalTicks;
                public double LastTapTicks;
                public long CurrentTicks;
                public string Message;
                public int Type;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public LogStatItem(int id, double globalTicks, double lastTapTicks, long currentTicks, string message, int type)
                {
                    Id = id;
                    GlobalTicks = globalTicks;
                    LastTapTicks = lastTapTicks;
                    CurrentTicks = currentTicks;
                    Message = message;
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
                _lastTapTicks = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static WItem CreateItem()
            {
                var item = itemPools.New();
                item._id = Interlocked.Increment(ref _watcherCount);
                return item;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static WItem CreateItem(string lastMessage)
            {
                var item = itemPools.New();
                item._lastMessage = lastMessage;
                item._id = Interlocked.Increment(ref _watcherCount);
                return item;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static WItem CreateItem(string startMessage, string lastMessage)
            {
                var item = itemPools.New();
                item._lastMessage = lastMessage;
                item._id = Interlocked.Increment(ref _watcherCount);
                item.StartedTap(startMessage);
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
                if (Core.Log.MaxLogLevel.HasFlag(LogLevel.Stats))
                {
                    var _globalTicks = currentTicks - _initTicks;
                    logStatsWorker.Enqueue(new LogStatItem(_id, _globalTicks, _lastTapTicks, currentTicks, message, 1));
                }
                _ticksTimestamp = currentTicks;
            }
            /// <summary>
            /// Write a new elapsed time to the stat logs
            /// </summary>
            /// <param name="message">Message of the tap</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void StartedTap(string message)
            {
                var currentTicks = Stopwatch.GetTimestamp();
                _lastTapTicks = currentTicks - _ticksTimestamp;
                if (message != null && Core.Log.MaxLogLevel.HasFlag(LogLevel.Stats))
                {
                    var _globalTicks = currentTicks - _initTicks;
                    logStatsWorker.Enqueue(new LogStatItem(_id, _globalTicks, _lastTapTicks, currentTicks, message, 0));
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
                if (message != null && Core.Log.MaxLogLevel.HasFlag(LogLevel.Stats))
                {
                    var _globalTicks = currentTicks - _initTicks;
                    logStatsWorker.Enqueue(new LogStatItem(_id, _globalTicks, _lastTapTicks, currentTicks, message, 2));
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
                _ticksTimestamp = Stopwatch.GetTimestamp();
                _initTicks = _ticksTimestamp;
                _lastTapTicks = 0;
                disposedValue = false;
            }
            #endregion

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls
                                                /// <summary>
                                                /// Dispose Object
                                                /// </summary>
                                                /// <param name="disposing"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        if (_lastMessage != null)
                        {
                            EndTap(_lastMessage);
                            _lastMessage = null;
                        }
                        Interlocked.Decrement(ref _watcherCount);
                        itemPools.Store(this);
                    }
                    disposedValue = true;
                }
            }
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
