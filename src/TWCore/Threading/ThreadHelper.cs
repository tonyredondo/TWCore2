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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

// ReSharper disable MethodSupportsCancellation

namespace TWCore.Threading
{
    /// <summary>
    /// Thread helper methods
    /// </summary>
    public static class ThreadHelper
    {
        /// <summary>
        /// Sleep time between condition checks
        /// </summary>
        public static int SleepTimeBetweenConditionCheck { get; set; } = 200;
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="token">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SleepUntil(Func<bool> condition, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;
            var handle = token.WaitHandle;
            while (!token.IsCancellationRequested && !condition())
                handle.WaitOne(SleepTimeBetweenConditionCheck);
        }
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="time">Maximum waiting time for the condition to be true.</param>
        /// <param name="token">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SleepUntil(Func<bool> condition, TimeSpan time, CancellationToken token)
            => SleepUntil(condition, (int)time.TotalMilliseconds, token);
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="milliseconds">Maximum waiting time for the condition to be true</param>
        /// <param name="token">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SleepUntil(Func<bool> condition, int milliseconds, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;
            var handle = token.WaitHandle;
            var time = SleepTimeBetweenConditionCheck < milliseconds ? SleepTimeBetweenConditionCheck : milliseconds;
            var sw = Stopwatch.StartNew();
            while (!token.IsCancellationRequested && !condition() && sw.ElapsedMilliseconds < milliseconds)
                handle.WaitOne(time);
        }
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="time">Maximum waiting time for the condition to be true.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SleepUntil(Func<bool> condition, TimeSpan time)
            => SleepUntil(condition, (int)time.TotalMilliseconds);
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="milliseconds">Maximum waiting time for the condition to be true</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SleepUntil(Func<bool> condition, int? milliseconds = null)
        {
            if (condition()) return;
            using (var tmpEvent = new ManualResetEventSlim(false))
            {
                if (milliseconds.HasValue)
                {
                    var sw = Stopwatch.StartNew();
                    while (!condition() && sw.ElapsedMilliseconds < milliseconds.Value)
                        tmpEvent.Wait(SleepTimeBetweenConditionCheck);
                    return;
                }
                while (!condition())
                    tmpEvent.Wait(SleepTimeBetweenConditionCheck);
            }
        }
    }
}
