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
        private readonly AwaitableManualEvent _event = new AwaitableManualEvent();
        private static readonly ConcurrentDictionary<CancellationToken, Task> CTasks = new ConcurrentDictionary<CancellationToken, Task>();
        private static readonly ObjectPool<Task[]> TaskArrayPool = new ObjectPool<Task[]>(_ => new Task[2], i => Array.Clear(i, 0, 2));

        #region Properties
        /// <summary>
        /// Gets if the Event has been set
        /// </summary>
        public bool IsSet => _event.Fired;
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
                _event.Fire();
        }
        #endregion

        #region Wait Methods
        /// <summary>
        /// Wait Async for the set event
        /// </summary>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WaitAsync()
            => _event.WaitAsync();
        /// <summary>
        /// Wait Async for the set event
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task WaitAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;
            var task = _event.WaitAsync();
            if (task.IsCompleted) return;
            var cTask = CTasks.GetOrAdd(cancellationToken, token => token.WhenCanceledAsync());
            if (cTask.IsCompleted) return;
            var tArray = TaskArrayPool.New();
            tArray[0] = task;
            tArray[1] = cTask;
            await Task.WhenAny(tArray).ConfigureAwait(false);
            TaskArrayPool.Store(tArray);
        }
        /// <summary>
        /// Wait Async for the set event
        /// </summary>
        /// <param name="milliseconds">Milliseconds</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> WaitAsync(int milliseconds)
        {
            var task = _event.WaitAsync();
            if (task.IsCompleted) return true;
            var delayCancellation = new CancellationTokenSource();
            var delayTask = Task.Delay(milliseconds, delayCancellation.Token);
            var tArray = TaskArrayPool.New();
            tArray[0] = task;
            tArray[1] = delayTask;
            var resTask = await Task.WhenAny(tArray).ConfigureAwait(false);
            TaskArrayPool.Store(tArray);
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
            if (cancellationToken.IsCancellationRequested) return false;
            var task = _event.WaitAsync();
            if (task.IsCompleted) return true;
            var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancellationToken.None);
            var delayTask = Task.Delay(milliseconds, linkedCancellation.Token);
            var tArray = TaskArrayPool.New();
            tArray[0] = task;
            tArray[1] = delayTask;
            var resTask = await Task.WhenAny(tArray).ConfigureAwait(false);
            TaskArrayPool.Store(tArray);
            if (resTask == delayTask)
                return false;
            linkedCancellation.Cancel();
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
            _event.Fire();
        }
        /// <summary>
        /// Sets the event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task SetAsync()
        {
            var task = _event.WaitAsync();
            if (task.IsCompleted) return Task.CompletedTask;
            Task.Factory.StartNew(s => ((AwaitableManualEvent)s).Fire(), _event, CancellationToken.None);
            return task;
        }
        #endregion

        #region Reset Method
        /// <summary>
        /// Reset the event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _event.Reset();
        }
        #endregion

        /// <summary>
        /// Dispose instance resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
        }
    }
}
