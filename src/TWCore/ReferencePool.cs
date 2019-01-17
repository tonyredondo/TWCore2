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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly ConcurrentStack<T> _objectStack;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private T _firstItem;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Timer _dropTimer;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _count;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly int _dropmaxsizeThreshold;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Action<T> _resetAction;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Action<T> _onetimeInitAction;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Action<T> _dropAction;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly PoolResetMode _resetMode;

        /// <summary>
        /// Pool count
        /// </summary>
        public int Count => _count;

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
        /// <param name="dropMaxSizeThreashold">Drop max size threshold</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReferencePool(int initialBufferSize = 0, Action<T> resetAction = null, Action<T> onetimeInitAction = null, PoolResetMode resetMode = PoolResetMode.AfterUse, int dropTimeFrequencyInSeconds = 120, Action<T> dropAction = null, int dropMaxSizeThreashold = 15)
        {
            _objectStack = new ConcurrentStack<T>();
            _resetAction = resetAction;
            _onetimeInitAction = onetimeInitAction;
            _resetMode = resetMode;
            _dropAction = dropAction;
            _dropmaxsizeThreshold = dropMaxSizeThreashold;
            if (_dropmaxsizeThreshold < initialBufferSize)
                _dropmaxsizeThreshold = initialBufferSize;
            if (initialBufferSize > 0)
                Preallocate(initialBufferSize);
            if (dropTimeFrequencyInSeconds > 0 && _dropAction != null)
            {
                var frequency = TimeSpan.FromSeconds(dropTimeFrequencyInSeconds);
                _dropTimer = new Timer(DropTimerMethod, this, frequency, frequency);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DropTimerMethod(object state)
        {
            var oPool = (ReferencePool<T>)state;
            if (oPool != null && oPool._count > oPool._dropmaxsizeThreshold && oPool._objectStack.TryPop(out var item))
            {
                Interlocked.Decrement(ref oPool._count);
                oPool._dropAction?.Invoke(item);
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
            if (number < 2) return;
            var tAlloc = new T[number];
            for (var i = 0; i < number; i++)
            {
                var t = new T();
                _onetimeInitAction?.Invoke(t);
                tAlloc[i] = t;
                Interlocked.Increment(ref _count);
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
            var value = _firstItem;
            if (value != null && value == Interlocked.CompareExchange(ref _firstItem, null, value))
            {
                if (_resetMode == PoolResetMode.BeforeUse)
                    _resetAction?.Invoke(value);
                return value;
            }
            if (_objectStack.TryPop(out value))
            {
                Interlocked.Decrement(ref _count);
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
            if (obj is null) return;
            if (_resetMode == PoolResetMode.AfterUse)
                _resetAction?.Invoke(obj);
            if (_firstItem == null)
            {
                _firstItem = obj;
                return;
            }
            _objectStack.Push(obj);
            Interlocked.Increment(ref _count);
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
            Interlocked.Exchange(ref _count, 0);
        }
    }
}
