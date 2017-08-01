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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TWCore.Threading
{
    /// <summary>
    /// Thread helper methods
    /// </summary>
    public class Thread : IThread
    {
        /// <summary>
        /// Sleep time between condition checks
        /// </summary>
        public int SleepTimeBetweenConditionCheck { get; set; } = 10;

        /// <summary>
        /// Sleeps a thread for a specific time
        /// </summary>
        /// <param name="milliseconds">Milliseconds for the thread to sleep</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sleep(int milliseconds)
        {
            using (var tmpEvent = new ManualResetEventSlim(false))
                tmpEvent.Wait(milliseconds);
        }
        /// <summary>
        /// Sleeps a thread for a specific time
        /// </summary>
        /// <param name="milliseconds">Milliseconds for the thread to sleep</param>
        /// <param name="token">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sleep(int milliseconds, CancellationToken token)
        {
            token.WaitHandle.WaitOne(milliseconds);
        }
        /// <summary>
        /// Sleeps a thread for a specific time
        /// </summary>
        /// <param name="time">Timespan for the thread to sleep</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sleep(TimeSpan time)
        {
            using (var tmpEvent = new ManualResetEventSlim(false))
                tmpEvent.Wait(time);
        }
        /// <summary>
        /// Sleeps a thread for a specific time with a cancellation token
        /// </summary>
        /// <param name="time">Timespan for the thread to sleep</param>
        /// <param name="token">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sleep(TimeSpan time, CancellationToken token)
        {
            token.WaitHandle.WaitOne(time);
        }

        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="token">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SleepUntil(Func<bool> condition, CancellationToken token)
        {
            if (!token.IsCancellationRequested && !condition())
            {
                using (var tmpEvent = new ManualResetEventSlim(false))
                {
                    Task.Factory.StartNew(objEvent =>
                    {
                        while (!token.IsCancellationRequested && !condition())
                            token.WaitHandle.WaitOne(SleepTimeBetweenConditionCheck);
                        ((ManualResetEventSlim)objEvent).Set();
                    }, tmpEvent, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    tmpEvent.Wait();
                }
            }
        }
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="time">Maximum waiting time for the condition to be true.</param>
        /// <param name="token">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SleepUntil(Func<bool> condition, TimeSpan time, CancellationToken token)
            => SleepUntil(condition, (int)time.TotalMilliseconds, token);
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="milliseconds">Maximum waiting time for the condition to be true</param>
        /// <param name="token">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SleepUntil(Func<bool> condition, int milliseconds, CancellationToken token)
        {
            if (!token.IsCancellationRequested && !condition())
            {
                using (var tmpEvent = new ManualResetEventSlim(false))
                {
                    Task.Factory.StartNew(objEvent =>
                    {
                        while (!token.IsCancellationRequested && !condition())
                            token.WaitHandle.WaitOne(SleepTimeBetweenConditionCheck);
                        ((ManualResetEventSlim)objEvent).Set();
                    }, tmpEvent, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    tmpEvent.Wait(milliseconds);
                }
            }
        }
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="time">Maximum waiting time for the condition to be true.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SleepUntil(Func<bool> condition, TimeSpan time)
            => SleepUntil(condition, (int)time.TotalMilliseconds);
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="milliseconds">Maximum waiting time for the condition to be true</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SleepUntil(Func<bool> condition, int? milliseconds = null)
        {
            if (!condition())
            {
                using (var tmpEvent = new ManualResetEventSlim(false))
                {
                    Task.Factory.StartNew(objEvent =>
                    {
                        while (!condition())
                            Sleep(SleepTimeBetweenConditionCheck);
                        ((ManualResetEventSlim)objEvent).Set();
                    }, tmpEvent, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    if (milliseconds.HasValue)
                        tmpEvent.Wait(milliseconds.Value);
                    else
                        tmpEvent.Wait();
                }
            }
        }
    }
}
