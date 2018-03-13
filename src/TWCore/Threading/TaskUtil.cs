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
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TWCore.Threading
{
    /// <summary>
    /// Task utilities
    /// </summary>
    public static class TaskUtil
    {
        /// <summary>
        /// Complete Task True
        /// </summary>
        public static readonly Task<bool> CompleteTrue = Task.FromResult(true);
        /// <summary>
        /// Complete Task False
        /// </summary>
        public static readonly Task<bool> CompleteFalse = Task.FromResult(false);
        /// <summary>
        /// Complete String Empty
        /// </summary>
        public static readonly Task<string> CompleteEmpty = Task.FromResult(string.Empty);
        /// <summary>
        /// Complete value 1
        /// </summary>
        public static readonly Task<int> CompleteValuePlus1 = Task.FromResult(1);
        /// <summary>
        /// Complete value -1
        /// </summary>
        public static readonly Task<int> CompleteValueMinus1 = Task.FromResult(-1);


        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="token">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task SleepUntil(Func<bool> condition, CancellationToken token)
        {
            while (!token.IsCancellationRequested && !condition())
                await Task.Delay(Factory.Thread.SleepTimeBetweenConditionCheck, token).ConfigureAwait(false);
        }
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="time">Maximum waiting time for the condition to be true.</param>
        /// <param name="token">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task SleepUntil(Func<bool> condition, TimeSpan time, CancellationToken token)
            => SleepUntil(condition, (int)time.TotalMilliseconds, token);
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="milliseconds">Maximum waiting time for the condition to be true</param>
        /// <param name="token">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task SleepUntil(Func<bool> condition, int milliseconds, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;
            var time = Factory.Thread.SleepTimeBetweenConditionCheck < milliseconds ? Factory.Thread.SleepTimeBetweenConditionCheck : milliseconds;
            var sw = Stopwatch.StartNew();
            while (!token.IsCancellationRequested && !condition() && sw.ElapsedMilliseconds < milliseconds)
                await Task.Delay(time, token).ConfigureAwait(false);
        }
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="time">Maximum waiting time for the condition to be true.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task SleepUntil(Func<bool> condition, TimeSpan time)
            => SleepUntil(condition, (int)time.TotalMilliseconds);
        /// <summary>
        /// Sleeps the thread until a condition is true
        /// </summary>
        /// <param name="condition">Condition that sleeps the thread</param>
        /// <param name="milliseconds">Maximum waiting time for the condition to be true</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task SleepUntil(Func<bool> condition, int? milliseconds = null)
        {
            if (condition()) return;
            if (milliseconds.HasValue)
            {
                var ms = milliseconds.Value;
                var time = Factory.Thread.SleepTimeBetweenConditionCheck < ms
                    ? Factory.Thread.SleepTimeBetweenConditionCheck : ms;
                var sw = Stopwatch.StartNew();
                while (!condition() && sw.ElapsedMilliseconds < ms)
                    await Task.Delay(time).ConfigureAwait(false);
                return;
            }
            while (!condition())
                await Task.Delay(Factory.Thread.SleepTimeBetweenConditionCheck).ConfigureAwait(false);
        }
    }
}