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
    /// Awaitable manual event
    /// </summary>
    public class AwaitableManualEvent<T>
    {
        private Action<T> _fireAction = null;
        private long _wasFired = 0;
        private Awaiter _awaiter;
        private T _lastValue;

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
                _result = obj;
                _completed = true;
                _awaitableEvent = null;
                if (_continuation != null)
                    Task.Run(_continuation);
            }
        }
    }
}
