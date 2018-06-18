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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Local

namespace TWCore
{
    /// <inheritdoc />
    /// <summary>
    /// Object by reference Pool
    /// </summary>
    /// <typeparam name="T">Object type, must be a class with a default constructor</typeparam>
    public sealed class ReferencePool<T> : IPool<T> where T : class, new()
    {
        #region Statics
        /// <summary>
        /// Gets the shared reference pool
        /// </summary>
        public static ReferencePool<T> Shared => Singleton<ReferencePool<T>>.Instance;
        #endregion

        private readonly ConcurrentStack<T> _objectStack;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Action<T> _resetAction;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Action<T> _onetimeInitAction;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly PoolResetMode _resetMode;

        /// <inheritdoc />
        /// <summary>
        /// Private .ctor for Singleton instance
        /// </summary>
        private ReferencePool() : this(0) { }

        /// <summary>
        /// Object by reference Pool
        /// </summary>
        /// <param name="initialBufferSize">Initial buffer size</param>
        /// <param name="resetAction">Reset action before storing back the item in the pool</param>
        /// <param name="onetimeInitAction">Action to execute after a new object creation</param>
        /// <param name="resetMode">Pool reset mode</param>
        /// <param name="dropTimeFrequencyInSeconds">Drop time frequency in seconds</param>
        /// <param name="dropAction">Drop action over the drop item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReferencePool(int initialBufferSize = 0, Action<T> resetAction = null, Action<T> onetimeInitAction = null, PoolResetMode resetMode = PoolResetMode.AfterUse, int dropTimeFrequencyInSeconds = 60, Action<T> dropAction = null)
        {
            _objectStack = new ConcurrentStack<T>();
            _resetAction = resetAction;
            _onetimeInitAction = onetimeInitAction;
            _resetMode = resetMode;
            if (initialBufferSize > 0)
                Preallocate(initialBufferSize);
            if (dropTimeFrequencyInSeconds > 0)
            {
                var cancellationTokenSource = new CancellationTokenSource();
                dropTimeFrequencyInSeconds = dropTimeFrequencyInSeconds * 1000;
                var token = cancellationTokenSource.Token;
                Task.Delay(dropTimeFrequencyInSeconds, token).ContinueWith(async tsk =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        var count = _objectStack.Count;
                        if (count > 2 && _objectStack.TryPop(out var item))
                            dropAction?.Invoke(item);
                        await Task.Delay(dropTimeFrequencyInSeconds, token).ConfigureAwait(false);                        
                    }
                }, token);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Preallocate a number of objects on the pool
        /// </summary>
        /// <param name="number">Number of objects to allocate</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Preallocate(int number)
        {
            var tAlloc = new T[number];
            for (var i = 0; i < number; i++)
            {
                var t = new T();
                _onetimeInitAction?.Invoke(t);
                tAlloc[i] = t;
            }
            _objectStack.PushRange(tAlloc);
        }
        /// <inheritdoc />
        /// <summary>
        /// Get a new instance from the pool
        /// </summary>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T New()
        {
            if (_objectStack.TryPop(out var value))
            {
                if (_resetMode == PoolResetMode.BeforeUse)
                    _resetAction?.Invoke(value);
                return value;
            }
            value = new T();
            _onetimeInitAction?.Invoke(value);
            return value;
        }
        /// <inheritdoc />
        /// <summary>
        /// Store the instance back to the pool
        /// </summary>
        /// <param name="obj">Object to store</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(T obj)
        {
            if (_resetMode == PoolResetMode.AfterUse)
                _resetAction?.Invoke(obj);
            _objectStack.Push(obj);
        }
        /// <inheritdoc />
        /// <summary>
        /// Get current objects in the pool
        /// </summary>
        /// <returns>IEnumerable with the current objects</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetCurrentObjects()
        {
            return _objectStack.ToArray();
        }
        /// <inheritdoc />
        /// <summary>
        /// Clear the current object stack
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _objectStack.Clear();
        }
    }
}
