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
using System.Threading;

namespace TWCore.Threading
{
    /// <summary>
    /// Thread helper methods
    /// </summary>
    public interface IThread
    {
        /// <summary>
        /// Sleep time between condition checks
        /// </summary>
        int SleepTimeBetweenConditionCheck { get; set; }
        /// <summary>
        /// Sleeps a thread for a specific time
        /// </summary>
        /// <param name="time">Timespan for the thread to sleep</param>
        void Sleep(TimeSpan time);
        /// <summary>
        /// Sleeps a thread for a specific time
        /// </summary>
        /// <param name="milliseconds">Milliseconds for the thread to sleep</param>
        void Sleep(int milliseconds);
        /// <summary>
        /// Sleeps a thread for a specific time with a cancellation token
        /// </summary>
        /// <param name="time">Timespan for the thread to sleep</param>
        /// <param name="token">Cancellation token</param>
        void Sleep(TimeSpan time, CancellationToken token);
        /// <summary>
        /// Sleeps a thread for a specific time
        /// </summary>
        /// <param name="milliseconds">Milliseconds for the thread to sleep</param>
        /// <param name="token">Cancellation token</param>
        void Sleep(int milliseconds, CancellationToken token);
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="time">Maximum waiting time for the condition to be true.</param>
        void SleepUntil(Func<bool> condition, TimeSpan time);
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="token">Cancellation token</param>
        void SleepUntil(Func<bool> condition, CancellationToken token);
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="milliseconds">Maximum waiting time for the condition to be true</param>
        void SleepUntil(Func<bool> condition, int? milliseconds = null);
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="time">Maximum waiting time for the condition to be true.</param>
        /// <param name="token">Cancellation token</param>
        void SleepUntil(Func<bool> condition, TimeSpan time, CancellationToken token);
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="milliseconds">Maximum waiting time for the condition to be true</param>
        /// <param name="token">Cancellation token</param>
        void SleepUntil(Func<bool> condition, int milliseconds, CancellationToken token);
    }
}