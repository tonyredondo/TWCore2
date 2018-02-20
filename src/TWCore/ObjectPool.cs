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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TWCore
{
    /// <summary>
    /// Object Pool
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public sealed class ObjectPool<T>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly object _padLock = new object();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Stack<T> _objectStack;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Func<ObjectPool<T>, T> _createFunc;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Action<T> _resetAction;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _allocating;
        private readonly PoolResetMode _resetMode;
        private readonly int _preallocationThreshold;

        /// <summary>
        /// Get the number of objects in the queue
        /// </summary>
        public int Count => _objectStack.Count;

        /// <summary>
        /// Object pool
        /// </summary>
        /// <param name="createFunc">Function to create a new object</param>
        /// <param name="resetAction">Reset action before storing back the item in the pool</param>
        /// <param name="initialBufferSize">Initial buffer size</param>
        /// <param name="resetMode">Pool reset mode</param>
        /// <param name="preallocationThreshold">Number of items limit to create new allocations in another Task. Use 0 to disable, if is greater than the initial buffer then the initial buffer is set twice at this value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectPool(Func<ObjectPool<T>, T> createFunc, Action<T> resetAction = null, int initialBufferSize = 0, PoolResetMode resetMode = PoolResetMode.BeforeUse, int preallocationThreshold = 0)
        {
            _objectStack = new Stack<T>(25);
            _createFunc = createFunc;
            _resetAction = resetAction;
            _resetMode = resetMode;
            _preallocationThreshold = preallocationThreshold;
            if (_preallocationThreshold > initialBufferSize)
                initialBufferSize = _preallocationThreshold * 2;
            if (initialBufferSize > 0)
                Preallocate(initialBufferSize);
        }
        /// <summary>
        /// Preallocate a number of objects on the pool
        /// </summary>
        /// <param name="number">Number of objects to allocate</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Preallocate(int number)
        {
            for (var i = 0; i < number; i++)
            {
                var value = _createFunc(this);
                lock (_padLock)
                    _objectStack.Push(value);
            }
        }
        /// <summary>
        /// Get a new instance from the pool
        /// </summary>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T New()
        {
            var cNew = true;
            var value = default(T);
            int count;

            lock (_padLock)
            {
                count = _objectStack.Count;
                if (count > 0)
                {
                    value = _objectStack.Pop();
                    cNew = false;
                }
            }
            if (count <= _preallocationThreshold && !_allocating)
            {
                Task.Run(() =>
                {
                    _allocating = true;
                    Preallocate(_preallocationThreshold * 2);
                    _allocating = false;
                });
            }
            
            if (cNew)
                return _createFunc(this);
            if (_resetMode == PoolResetMode.BeforeUse)
                _resetAction?.Invoke(value);
            return value;
        }
        /// <summary>
        /// Store the instance back to the pool
        /// </summary>
        /// <param name="obj">Object to store</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(T obj)
        {
            if (_resetMode == PoolResetMode.AfterUse)
                _resetAction?.Invoke(obj);
            lock(_padLock)
                _objectStack.Push(obj);
        }
        /// <summary>
        /// Get current objects in the pool
        /// </summary>
        /// <returns>IEnumerable with the current objects</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetCurrentObjects()
        {
            lock(_padLock)
                return _objectStack.ToReadOnly();
        }
        /// <summary>
        /// Clear the current object stack
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            lock(_padLock)
                _objectStack.Clear();
        }
    }
}
