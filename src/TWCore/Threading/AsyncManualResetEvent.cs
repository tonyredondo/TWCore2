﻿/*
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
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable MethodSupportsCancellation
// ReSharper disable UnusedMember.Global
#pragma warning disable 420

namespace TWCore.Threading
{
    /// <inheritdoc />
    /// <summary>
    /// Async Manual Reset Event
    /// </summary>
    public class AsyncManualResetEvent : IDisposable
    {
        private volatile TaskCompletionSource<bool> _mTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private static readonly NonBlocking.ConcurrentDictionary<CancellationToken, Task> CTasks = new NonBlocking.ConcurrentDictionary<CancellationToken, Task>();
        private static readonly Task DisposedExceptionTask = Task.FromException(new Exception("The instance has been disposed."));

        #region Properties
        /// <summary>
        /// Gets if the Event has been setted
        /// </summary>
        public bool IsSet => _mTcs.Task.IsCompleted;
        #endregion

        #region .ctor
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
        #endregion

        #region Wait Methods
        /// <summary>
        /// Wait Async for the set event
        /// </summary>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WaitAsync()
            => _mTcs?.Task ?? DisposedExceptionTask;
        /// <summary>
        /// Wait Async for the set event
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WaitAsync(CancellationToken cancellationToken)
        {
            if (_mTcs == null) return DisposedExceptionTask;
            if (_mTcs.Task.IsCompleted) return Task.CompletedTask;
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
            var cTask = CTasks.GetOrAdd(cancellationToken, token => token.WhenCanceledAsync());
            if (cTask.IsCompleted) return Task.CompletedTask;
            return Task.WhenAny(_mTcs.Task, cTask);
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
            if (_mTcs.Task.IsCompleted) return true;
            if (_mTcs.Task.IsCanceled) return false;
            if (_mTcs.Task.IsFaulted) return false;
            var delayCancellation = new CancellationTokenSource();
            var delayTask = Task.Delay(milliseconds, delayCancellation.Token);
            var resTask = await Task.WhenAny(delayTask, _mTcs.Task).ConfigureAwait(false);
            if (resTask == delayTask)
                return false;
            delayCancellation.Cancel();
            return true;
        }
        /// <summary>
        /// Wait Async for the set event
        /// </summary>
        /// <param name="timeout">TimeSpan timeout value</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> WaitAsync(TimeSpan timeout)
            => WaitAsync((int)timeout.TotalMilliseconds);
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
            if (cancellationToken.IsCancellationRequested) return false;
            if (_mTcs.Task.IsCompleted) return true;
            if (_mTcs.Task.IsCanceled) return false;
            if (_mTcs.Task.IsFaulted) return false;
            var delayCancellation = new CancellationTokenSource();
            var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(delayCancellation.Token, cancellationToken);
            var delayTask = Task.Delay(milliseconds, linkedCancellation.Token);
            var resTask = await Task.WhenAny(delayTask, _mTcs.Task).ConfigureAwait(false);
            if (resTask == delayTask)
                return false;
            delayCancellation.Cancel();
            if (cancellationToken.IsCancellationRequested)
                return false;
            return true;
        }
        /// <summary>
        /// Wait Async for the set event
        /// </summary>
        /// <param name="timeout">TimeSpan timeout value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
            => WaitAsync((int)timeout.TotalMilliseconds, cancellationToken);
        #endregion

        #region Set Methods
        /// <summary>
        /// Sets the event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set()
        {
            if (_mTcs == null) return;
            var tcs = _mTcs;
            if (tcs.Task.IsCompleted) return;
            tcs.TrySetResult(true);
        }
        /// <summary>
        /// Sets the event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task SetAsync()
        {
            if (_mTcs == null) return Task.CompletedTask;
            var tcs = _mTcs;
            if (tcs.Task.IsCompleted) return Task.CompletedTask;
            Task.Factory.StartNew(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs, CancellationToken.None);
            return tcs.Task;
        }
        #endregion

        #region Reset Method
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
                if (!tcs.Task.IsCompleted || Interlocked.CompareExchange(ref _mTcs, new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously), tcs) == tcs)
                    return;
            }
        }
        #endregion

        /// <summary>
        /// Dispose instance resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _mTcs = null;
        }
    }
}
