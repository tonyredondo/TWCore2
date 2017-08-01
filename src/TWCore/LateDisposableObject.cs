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

namespace TWCore
{
    /// <summary>
    /// Late disposable object
    /// </summary>
    public abstract class LateDisposableObject : IDisposable
    {
        #region Fields
        long _locks = 0;
        volatile bool _disposeCalled = false;
        CancellationTokenSource cancellationTokenSource;
        CancellationToken cancellationToken;
        Task disposeTask;
        #endregion

        #region Properties
        /// <summary>
        /// Get if the current instance is locked for not disposal
        /// </summary>
        public bool IsLocked
            => Interlocked.Read(ref _locks) > 0;
        #endregion

        #region Public Methods
        /// <summary>
        /// Locks the instance for not disposal
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Lock()
            => Interlocked.Increment(ref _locks);

        /// <summary>
        /// Unlock the instance for disposal if is requested
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unlock()
        {
            if (Interlocked.Decrement(ref _locks) < 0)
                Interlocked.Exchange(ref _locks, 0);
            if (_disposeCalled && Interlocked.Read(ref _locks) == 0)
                LateDispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _disposeCalled = true;
            if (Interlocked.Read(ref _locks) == 0)
                LateDispose();
        }
        #endregion

        /// <summary>
        /// Late dispose task handling
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LateDispose()
        {
            if (cancellationTokenSource != null)
            {
                Core.Log.LibDebug("Cancelling previous disposal task. [{0}]", GetType().Name);
                cancellationTokenSource.Cancel();
                disposeTask.Wait();
            }
            Core.Log.LibDebug("Creating a disposal task. [{0}]", GetType().Name);
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            disposeTask = Task.Delay(500, cancellationToken).ContinueWith(tsk =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    if (Interlocked.Read(ref _locks) < 1)
                    {
                        Core.Log.LibDebug("Disposing object. [{0}]", GetType().Name);
                        OnDispose();
                        GC.SuppressFinalize(this);
                        Core.Log.LibDebug("Object disposed. [{0}]", GetType().Name);
                    }
                    else
                        Core.Log.LibDebug("Object not disposed because there is a lock. [{0}]", GetType().Name);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Method called when the dispose is fire and there are not locks on the instance.
        /// </summary>
        protected abstract void OnDispose();
    }
}
