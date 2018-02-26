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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable 649

namespace TWCore
{
    /// <summary>
    /// Object Pool
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public sealed class ObjectPool<T>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly ConcurrentStack<T> _objectStack;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Func<ObjectPool<T>, T> _createFunc;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Action<T> _resetAction;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _count;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly PoolResetMode _resetMode;

        /// <summary>
        /// Get the number of objects in the queue
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Object pool
        /// </summary>
        /// <param name="createFunc">Function to create a new object</param>
        /// <param name="resetAction">Reset action before storing back the item in the pool</param>
        /// <param name="initialBufferSize">Initial buffer size</param>
        /// <param name="resetMode">Pool reset mode</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectPool(Func<ObjectPool<T>, T> createFunc, Action<T> resetAction = null, int initialBufferSize = 0, PoolResetMode resetMode = PoolResetMode.AfterUse)
        {
            _objectStack = new ConcurrentStack<T>();
            _createFunc = createFunc;
            _resetAction = resetAction;
            _resetMode = resetMode;
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
            _objectStack.PushRange(Enumerable.Range(0, number).Select(i => _createFunc(this)).ToArray());
            Interlocked.Add(ref _count, number);
        }
        /// <summary>
        /// Get a new instance from the pool
        /// </summary>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T New()
        {
            if (!_objectStack.TryPop(out var value))
                return _createFunc(this);
            Interlocked.Decrement(ref _count);
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
            _objectStack.Push(obj);
            Interlocked.Increment(ref _count);
        }
        /// <summary>
        /// Get current objects in the pool
        /// </summary>
        /// <returns>IEnumerable with the current objects</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetCurrentObjects()
        {
            return _objectStack.ToReadOnly();
        }
        /// <summary>
        /// Clear the current object stack
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Interlocked.Exchange(ref _count, 0);
            _objectStack.Clear();
        }
    }


    /// <summary>
    /// Pool Object Lifecycle
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPoolObjectLifecycle<T>
    {
        /// <summary>
        /// Pool initial size
        /// </summary>
        int InitialSize { get; }
        /// <summary>
        /// Pool reset mode
        /// </summary>
        PoolResetMode ResetMode { get; }
        /// <summary>
        /// Creates a new item in the pool
        /// </summary>
        /// <returns>New item instance</returns>
        T New();
        /// <summary>
        /// Reset item
        /// </summary>
        /// <param name="value">Item instance</param>
        void Reset(T value);
    }

    /// <summary>
    /// Object pool
    /// </summary>
    public sealed class ObjectPool<T, TPoolObjectLifecycle>
        where TPoolObjectLifecycle : struct, IPoolObjectLifecycle<T>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly ConcurrentStack<T> _objectStack;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private TPoolObjectLifecycle _allocator;// = new TPoolObjectLifecycle();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _count;

        /// <summary>
        /// Object pool
        /// </summary>
        public ObjectPool()
        {
            _objectStack = new ConcurrentStack<T>();
            for (var i = 0; i < _allocator.InitialSize; i++)
                _objectStack.Push(_allocator.New());
            Interlocked.Add(ref _count, _allocator.InitialSize);
        }

        /// <summary>
        /// Get a new instance from the pool
        /// </summary>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T New()
        {
            if (!_objectStack.TryPop(out var value))
                return _allocator.New();

            Interlocked.Decrement(ref _count);
            if (_allocator.ResetMode == PoolResetMode.BeforeUse)
                _allocator.Reset(value);
            return value;
        }
        /// <summary>
        /// Store the instance back to the pool
        /// </summary>
        /// <param name="obj">Object to store</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(T obj)
        {
            if (_allocator.ResetMode == PoolResetMode.AfterUse)
                _allocator.Reset(obj);
            _objectStack.Push(obj);
            Interlocked.Increment(ref _count);
        }
        /// <summary>
        /// Get current objects in the pool
        /// </summary>
        /// <returns>IEnumerable with the current objects</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetCurrentObjects()
        {
            return _objectStack.ToReadOnly();
        }
        /// <summary>
        /// Clear the current object stack
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Interlocked.Exchange(ref _count, 0);
            _objectStack.Clear();
        }
    }
}
