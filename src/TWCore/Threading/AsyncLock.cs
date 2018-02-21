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
        private readonly Task<IDisposable> _cachedReleaser;

        /// <summary>
        /// Creates a new <see cref="AsyncLock"/> instance.
        /// </summary>
        public AsyncLock()
        {
            _semaphore = new SemaphoreSlim(1);
            _cachedReleaser = Task.FromResult((IDisposable) new Releaser(this));
        }

        /// <summary>
        /// Asynchronously locks the <see cref="AsyncLock"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> task that will complete when the <see cref="AsyncLock"/> 
        /// has been taken with a <see cref="Releaser"/> result.  Disposing of the <see cref="Releaser"/> 
        /// will release the <see cref="AsyncLock"/>.
        /// </returns>
        public Task<IDisposable> LockAsync() => LockAsync(CancellationToken.None);

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
        public Task<IDisposable> LockAsync(CancellationToken cancellationToken)
        {
            var wait = _semaphore.WaitAsync(cancellationToken);
            return wait.IsCompleted || wait.IsCompleted || wait.IsFaulted ?
                _cachedReleaser :
                wait.ContinueWith((_, state) => (IDisposable)state, _cachedReleaser.Result, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
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
                _asyncLock?._semaphore.Release();
            }
        }
    }
}