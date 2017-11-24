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
// ReSharper disable MethodSupportsCancellation
// ReSharper disable UnusedMember.Global

namespace TWCore.Threading
{
    /// <inheritdoc />
    /// <summary>
    /// Async Manual Reset Event
    /// </summary>
    public class AsyncManualResetEvent : IDisposable
    {
        private volatile TaskCompletionSource<bool> _mTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Async Manual Reset Event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncManualResetEvent()
        {
        }
        /// <summary>
        /// Async Manual Reset Event
        /// </summary>
        /// <param name="initialState">Initial state</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncManualResetEvent(bool initialState)
        {
            if (initialState)
                _mTcs.SetResult(true);
        }

        /// <summary>
        /// Wait Async for the set event
        /// </summary>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WaitAsync()
            => _mTcs?.Task ?? Task.FromException(new Exception("The instance has been disposed."));

        /// <summary>
        /// Wait Async for the set event
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task WaitAsync(CancellationToken cancellationToken)
        {
            if (_mTcs == null) throw new Exception("The instance has been disposed.");
            await Task.WhenAny(_mTcs.Task, cancellationToken.WaitHandle.WaitOneAsync()).ConfigureAwait(false);
        }

        /// <summary>
        /// Wait Async for the set event
        /// </summary>
        /// <param name="milliseconds">Milliseconds</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> WaitAsync(int milliseconds)
        {
            if (_mTcs == null) return false;
            var delayTask = Task.Delay(milliseconds);
            var finalTask = await Task.WhenAny(delayTask, _mTcs.Task).ConfigureAwait(false);
            return finalTask != delayTask;
        }
        /// <summary>
        /// Wait Async for the set event
        /// </summary>
        /// <param name="timeout">TimeSpan timeout value</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> WaitAsync(TimeSpan timeout)
        {
            if (_mTcs == null) return false;
            var delayTask = Task.Delay(timeout);
            var finalTask = await Task.WhenAny(delayTask, _mTcs.Task).ConfigureAwait(false);
            return finalTask != delayTask;
        }
        /// <summary>
        /// Wait Async for the set event
        /// </summary>
        /// <param name="milliseconds">Milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> WaitAsync(int milliseconds, CancellationToken cancellationToken)
        {
            if (_mTcs == null) return false;
            var finalTask = await Task.WhenAny(_mTcs.Task, cancellationToken.WaitHandle.WaitOneAsync(milliseconds)).ConfigureAwait(false);
            return finalTask == _mTcs.Task;
        }
        /// <summary>
        /// Wait Async for the set event
        /// </summary>
        /// <param name="timeout">TimeSpan timeout value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (_mTcs == null) return false;
            var finalTask = await Task.WhenAny(_mTcs.Task, cancellationToken.WaitHandle.WaitOneAsync((int)timeout.TotalMilliseconds)).ConfigureAwait(false);
            return finalTask == _mTcs.Task;
        }

        /// <summary>
        /// Sets the event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set()
        {
            if (_mTcs == null) return;
            var tcs = _mTcs;
            Task.Factory.StartNew(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default);
            tcs.Task.Wait();
        }
        /// <summary>
        /// Sets the event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task SetAsync()
        {
            if (_mTcs == null) return Task.CompletedTask;
            var tcs = _mTcs;
            Task.Factory.StartNew(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default);
            return tcs.Task;
        }

        /// <summary>
        /// Reset the event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            if (_mTcs == null) return;
            while (true)
            {
                var tcs = _mTcs;
                if (!tcs.Task.IsCompleted)
                    return;
#pragma warning disable 420
                if (Interlocked.CompareExchange(ref _mTcs, new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously), tcs) == tcs)
#pragma warning restore 420
                    return;
            }
        }

        public void Dispose()
        {
            _mTcs = null;
        }
    }
}
