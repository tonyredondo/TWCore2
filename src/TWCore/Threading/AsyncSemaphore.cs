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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TWCore.Threading
{
    /// <summary>
    /// Async Semaphore
    /// </summary>
    public class AsyncSemaphore
    {
        private readonly Queue<TaskCompletionSource<bool>> _waiters = new Queue<TaskCompletionSource<bool>>();
        private int _currentCount;

        /// <summary>
        /// Async Semaphore
        /// </summary>
        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0) throw new ArgumentOutOfRangeException("initialCount");
            _currentCount = initialCount;
        }

        /// <summary>
        /// Wait
        /// </summary>
        /// <returns>Wait Task</returns>
        public Task WaitAsync()
        {
            lock (_waiters)
            {
                if (_currentCount > 0)
                {
                    --_currentCount;
                    return TaskHelper.CompleteTrue;
                }

                var waiter = new TaskCompletionSource<bool>();
                _waiters.Enqueue(waiter);
                return waiter.Task;
            }
        }
        /// <summary>
        /// Wait with CancellationToken
        /// </summary>
        /// <param name="cancellationToken">CancellationToken instance</param>
        /// <returns>Wait Task</returns>
        public Task WaitAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
            lock (_waiters)
            {
                if (_currentCount > 0)
                {
                    --_currentCount;
                    return TaskHelper.CompleteTrue;
                }

                var waiter = new TaskCompletionSource<bool>();
                var registration = cancellationToken.Register(obj => waiter.TrySetCanceled((CancellationToken) obj), cancellationToken, false);
                waiter.Task.ContinueWith(
                    (oldTask, state) => ((CancellationTokenRegistration) state).Dispose(), registration,
                    CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                _waiters.Enqueue(waiter);
                return waiter.Task;
            }
        }
        /// <summary>
        /// Release the semaphore
        /// </summary>
        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (_waiters)
            {
                if (_waiters.Count > 0)
                    toRelease = _waiters.Dequeue();
                else
                    ++_currentCount;
            }
            toRelease?.SetResult(true);
        }
    }
}