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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TWCore.Threading
{
    /// <summary>
    /// An asynchronous locking mechanism.
    /// </summary>
    public class AsyncLock
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly Task<IDisposable> _cachedTaskReleaser;
        private readonly IDisposable _cachedReleaser;

        /// <summary>
        /// Creates a new <see cref="AsyncLock"/> instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncLock()
        {
            _semaphore = new SemaphoreSlim(1);
            _cachedReleaser = new Releaser(this);
            _cachedTaskReleaser = Task.FromResult(_cachedReleaser);
        }

        /// <summary>
        /// Asynchronously locks the <see cref="AsyncLock"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> task that will complete when the <see cref="AsyncLock"/> 
        /// has been taken with a <see cref="Releaser"/> result.  Disposing of the <see cref="Releaser"/> 
        /// will release the <see cref="AsyncLock"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<IDisposable> LockAsync()
        {
            var waitTask = _semaphore.WaitAsync();
            return waitTask.IsCompleted ? 
                _cachedTaskReleaser : 
                waitTask.ContinueWith((oldTask, state) => (IDisposable) state, _cachedReleaser, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
        /// <summary>
        /// Asynchronously locks the <see cref="AsyncLock"/>, while observing a
        /// <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> token to observe.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> task that will complete when the <see cref="AsyncLock"/> 
        /// has been taken with a <see cref="Releaser"/> result.  Disposing of the <see cref="Releaser"/> 
        /// will release the <see cref="AsyncLock"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<IDisposable> LockAsync(CancellationToken cancellationToken)
        {
            var waitTask = _semaphore.WaitAsync(cancellationToken);
            return waitTask.IsCompleted || waitTask.IsCanceled
                ? _cachedTaskReleaser
                : waitTask.ContinueWith((oldTask, state) => (IDisposable) state, _cachedReleaser, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        /// <summary>
        /// Locks the <see cref="AsyncLock"/>.
        /// </summary>
        /// <param name="action">Action to execute in the lock</param>
        public void Lock(Action action)
        {
            _semaphore.Wait();
            try
            {
                action();
            }
            finally
            {
                _semaphore.Release();
            }
        }
        /// <summary>
        /// Locks the <see cref="AsyncLock"/>.
        /// </summary>
        /// <param name="action">Action to execute in the lock</param>
        /// <param name="state">State instance</param>
        public void Lock<T>(Action<T> action, T state)
        {
            _semaphore.Wait();
            try
            {
                action(state);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        /// <summary>
        /// Locks the <see cref="AsyncLock"/>.
        /// </summary>
        /// <param name="func">Func to execute in the lock</param>
        public TResult Lock<TResult>(Func<TResult> func)
        {
            _semaphore.Wait();
            try
            {
                return func();
            }
            finally
            {
                _semaphore.Release();
            }
        }
        /// <summary>
        /// Locks the <see cref="AsyncLock"/>.
        /// </summary>
        /// <param name="func">Func to execute in the lock</param>
        /// <param name="state">State instance</param>
        public TResult Lock<T, TResult>(Func<T, TResult> func, T state)
        {
            _semaphore.Wait();
            try
            {
                return func(state);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        /// <summary>
        /// <see cref="T:TWCore.AsyncLock.Releaser" /> enables holding an <see cref="T:TWCore.AsyncLock" /> with a using scope.
        /// </summary>
        public struct Releaser : IDisposable
        {
            private readonly AsyncLock _asyncLock;
            internal Releaser(AsyncLock asyncLock)
            {
                _asyncLock = asyncLock;
            }
            /// <inheritdoc />
            /// <summary>
            /// Releases the held <see cref="T:TWCore.AsyncLock" />.
            /// </summary>
            public void Dispose()
            {
                _asyncLock._semaphore.Release();
            }
        }
    }
}