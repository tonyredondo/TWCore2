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
    public class AwaitableManualEvent<T>
    {
        private Action<T> _fireAction;
        private long _wasFired;
        private Awaiter _awaiter;
        private T _lastValue;

        /// <summary>
        /// Gets if the event was fired.
        /// </summary>
        public bool Fired => Interlocked.Read(ref _wasFired) == 1;
        
        /// <summary>
        /// Awaitable manual event
        /// </summary>
        public AwaitableManualEvent()
        {
            _awaiter = new Awaiter(this);
        }
        /// <summary>
        /// Fire the event
        /// </summary>
        /// <param name="item">Item to send</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fire(T item)
        {
            if (Interlocked.CompareExchange(ref _wasFired, 1, 0) == 0)
            {
                _lastValue = item;
                _fireAction(item);
            }
        }

        /// <summary>
        /// Reset event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            if (Interlocked.CompareExchange(ref _wasFired, 0, 1) == 1)
            {
                _awaiter = new Awaiter(this);
                _lastValue = default;
            }
        }

        /// <summary>
        /// Wait for event fire
        /// </summary>
        /// <returns>Awaiter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter WaitAsync()
        {
            return _awaiter;
        }

        /// <inheritdoc />
        /// <summary>
        /// Manual Event Awaiter
        /// </summary>
        public class Awaiter : INotifyCompletion
        {
            private volatile bool _completed;
            private T _result;
            private Action _continuation;
            private AwaitableManualEvent<T> _awaitableEvent;

            internal Awaiter(AwaitableManualEvent<T> awaitableEvent)
            {
                _awaitableEvent = awaitableEvent;
                _completed = Interlocked.Read(ref _awaitableEvent._wasFired) == 1;
                _result = awaitableEvent._lastValue;
                if (!_completed)
                    _awaitableEvent._fireAction = FiredEvent;
            }

            /// <summary>
            /// Gets the awaiter
            /// </summary>
            /// <returns>This awaiter instance</returns>
            public Awaiter GetAwaiter()
            {
                return this;
            }

            /// <summary>
            /// Get if the awaitable has been completed
            /// </summary>
            public bool IsCompleted => _completed;

            /// <summary>
            /// Gets the result
            /// </summary>
            /// <returns>Result</returns>
            public T GetResult()
            {
                if (_completed) return _result;
                var wait = new SpinWait();
                while (!_completed)
                    wait.SpinOnce();
                return _result;
            }


            void INotifyCompletion.OnCompleted(Action continuation)
            {
                if (_completed)
                {
                    continuation();
                    return;
                }
                _continuation = continuation;
            }

            private void FiredEvent(T obj)
            {
                var oldContinuation = Interlocked.Exchange(ref _continuation, null);
                _result = obj;
                _completed = true;
                _awaitableEvent = null;
                if (oldContinuation != null)
                    Task.Run(oldContinuation);
            }
        }
    }
    
    
    /// <summary>
    /// Awaitable manual event
    /// </summary>
    public class AwaitableManualEvent : IValueTaskSource
    {
        private long _wasFired;
        private Action<object> _continuation;
        private object _state;
        private Task _currentTask;

        /// <summary>
        /// Awaitable manual event
        /// </summary>
        public AwaitableManualEvent()
        {
            _currentTask = new ValueTask(this, 0).AsTask();
        }
        
        #region Interface
        void IValueTaskSource.GetResult(short token)
        {
            _continuation = null;
            _state = null;
        }

        ValueTaskSourceStatus IValueTaskSource.GetStatus(short token)
        {
            return Interlocked.Read(ref _wasFired) == 1 ? ValueTaskSourceStatus.Succeeded : ValueTaskSourceStatus.Pending;
        }

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
            if (Interlocked.CompareExchange(ref _wasFired, 1, 0) != 0) return;
            var oldContinuation = Interlocked.Exchange(ref _continuation, null);
            if (oldContinuation != null)
                ThreadPool.QueueUserWorkItem(oldContinuation, _state, true);
        }

        /// <summary>
        /// Reset event
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            if (Interlocked.CompareExchange(ref _wasFired, 0, 1) == 1)
                _currentTask = new ValueTask(this, 0).AsTask();
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
