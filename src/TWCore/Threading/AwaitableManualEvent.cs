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
using System.Threading.Tasks.Sources;

// ReSharper disable UnusedMember.Global

namespace TWCore.Threading
{
    /// <summary>
    /// Awaitable manual event
    /// </summary>
    public sealed class AwaitableManualEvent<T> : IValueTaskSource<T>
    {
        private long _wasFired;
        private Action<object> _continuation;
        private object _state;
        private Task<T> _currentTask;
        private T _value;
        private readonly bool _preferLocal;
        private readonly object _locker;

        /// <summary>
        /// Awaitable manual event
        /// </summary>
        /// <param name="preferLocal">Prefer continuations on local ThreadPool queue</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AwaitableManualEvent(bool preferLocal = true)
        {
            _locker = new object();
            _preferLocal = preferLocal;
            _currentTask = new ValueTask<T>(this, 0).AsTask();
        }

        #region Interface
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        T IValueTaskSource<T>.GetResult(short token)
        {
            return _value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ValueTaskSourceStatus IValueTaskSource<T>.GetStatus(short token)
        {
            return Interlocked.Read(ref _wasFired) == 1 ? ValueTaskSourceStatus.Succeeded : ValueTaskSourceStatus.Pending;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IValueTaskSource<T>.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            if (Interlocked.Read(ref _wasFired) == 1)
            {
                continuation(state);
                return;
            }
            _continuation = continuation;
            _state = state;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets if the event was fired.
        /// </summary>
        public bool Fired => Interlocked.Read(ref _wasFired) == 1;

        /// <summary>
        /// Fire the event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fire(T item)
        {
            if (Interlocked.CompareExchange(ref _wasFired, 1, 0) == 1) return;
            lock (_locker)
            {
                _value = item;
                if (_continuation != null)
                {
#if COMPATIBILITY
                    var nCont = _continuation;
                    ThreadPool.QueueUserWorkItem(state => nCont(state), _state);
#else
                    ThreadPool.QueueUserWorkItem(_continuation, _state, _preferLocal);
#endif
                }
            }
        }

        /// <summary>
        /// Reset event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            if (Interlocked.CompareExchange(ref _wasFired, 0, 1) == 0) return;
            lock (_locker)
            {
                _value = default;
                _currentTask = new ValueTask<T>(this, 0).AsTask();
            }
        }

        /// <summary>
        /// Gets the Value task to await for the event to be fired
        /// </summary>
        /// <returns>ValueTask instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<T> WaitValueAsync()
        {
            if (Interlocked.Read(ref _wasFired) == 1)
                return new ValueTask<T>(_value);
            return new ValueTask<T>(this, 0);
        }
        /// <summary>
        /// Gets the Value task to await for the event to be fired
        /// </summary>
        /// <returns>ValueTask instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> WaitAsync()
        {
            if (Interlocked.Read(ref _wasFired) == 1)
                return Task.FromResult(_value);
            return _currentTask;
        }
        #endregion
    }
    
    /// <summary>
    /// Awaitable manual event
    /// </summary>
    public sealed class AwaitableManualEvent : IValueTaskSource
    {
        private long _wasFired;
        private Action<object> _continuation;
        private object _state;
        private Task _currentTask;
        private readonly bool _preferLocal;
        private readonly object _locker;

        /// <summary>
        /// Awaitable manual event
        /// </summary>
        /// <param name="preferLocal">Prefer continuations on local ThreadPool queue</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AwaitableManualEvent(bool preferLocal = true)
        {
            _locker = new object();
            _preferLocal = preferLocal;
            _currentTask = new ValueTask(this, 0).AsTask();
        }

        #region Interface
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IValueTaskSource.GetResult(short token)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ValueTaskSourceStatus IValueTaskSource.GetStatus(short token)
        {
            return Interlocked.Read(ref _wasFired) == 1 ? ValueTaskSourceStatus.Succeeded : ValueTaskSourceStatus.Pending;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IValueTaskSource.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            if (Interlocked.Read(ref _wasFired) == 1)
            {
                continuation(state);
                return;
            }
            _continuation = continuation;
            _state = state;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets if the event was fired.
        /// </summary>
        public bool Fired => Interlocked.Read(ref _wasFired) == 1;

        /// <summary>
        /// Fire the event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fire()
        {
            if (Interlocked.CompareExchange(ref _wasFired, 1, 0) == 1) return;
            lock (_locker)
            {
                if (_continuation != null)
                {
#if COMPATIBILITY
                    var nCont = _continuation;
                    ThreadPool.QueueUserWorkItem(state => nCont(state), _state);
#else
                    ThreadPool.QueueUserWorkItem(_continuation, _state, _preferLocal);
#endif
                }
                _continuation = null;
                _state = null;
            }
        }

        /// <summary>
        /// Reset event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            if (Interlocked.CompareExchange(ref _wasFired, 0, 1) == 0) return;
            lock (_locker)
            {
                _currentTask = new ValueTask(this, 0).AsTask();
            }
        }

        /// <summary>
        /// Gets the Value task to await for the event to be fired
        /// </summary>
        /// <returns>ValueTask instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask WaitValueAsync()
        {
            if (Interlocked.Read(ref _wasFired) == 1)
                return new ValueTask(Task.CompletedTask);
            return new ValueTask(this, 0);
        }
        /// <summary>
        /// Gets the Value task to await for the event to be fired
        /// </summary>
        /// <returns>ValueTask instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WaitAsync()
        {
            if (Interlocked.Read(ref _wasFired) == 1)
                return Task.CompletedTask;
            return _currentTask;
        }
        #endregion
    }
}
