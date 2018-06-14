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
    /// <inheritdoc />
    /// <summary>
    /// Object Pool
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public sealed class ObjectPool<T> : IPool<T>
    {
        private readonly ConcurrentStack<T> _objectStack;
        private readonly ConcurrentQueue<PoolTime> _poolTimeQueue;
        private readonly int _maxUnusedTimePerItem;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Func<ObjectPool<T>, T> _createFunc;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Action<T> _resetAction;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly PoolResetMode _resetMode;

        public int Count => _objectStack.Count;

        private struct PoolTime
        {
            private readonly Task _removeTask;
            private readonly CancellationTokenSource _tokenSource;
            private bool Done => _tokenSource.IsCancellationRequested || _removeTask.IsCompleted;

            public PoolTime(ObjectPool<T> pool)
            {
                _tokenSource = new CancellationTokenSource();
                var token = _tokenSource.Token;
                _removeTask = Task.Delay(pool._maxUnusedTimePerItem, token).ContinueWith(tsk =>
                {
                    Core.Log.InfoBasic("Removing unused item from the pool.");
                    pool._objectStack.TryPop(out _);
                    pool._poolTimeQueue.TryDequeue(out _);
                }, token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
            }
            public void Cancel()
            {
                _tokenSource.Cancel();
            }
        }

        /// <summary>
        /// Object pool
        /// </summary>
        /// <param name="createFunc">Function to create a new object</param>
        /// <param name="resetAction">Reset action before storing back the item in the pool</param>
        /// <param name="initialBufferSize">Initial buffer size</param>
        /// <param name="resetMode">Pool reset mode</param>
        /// <param name="maxUnusedTimePerItemInSeconds">Max time of unused items in pool before removing it</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectPool(Func<ObjectPool<T>, T> createFunc, Action<T> resetAction = null, int initialBufferSize = 0, PoolResetMode resetMode = PoolResetMode.AfterUse, int maxUnusedTimePerItemInSeconds = 60)
        {
            _objectStack = new ConcurrentStack<T>();
            _poolTimeQueue = new ConcurrentQueue<PoolTime>();
            _createFunc = createFunc;
            _resetAction = resetAction;
            _resetMode = resetMode;
            _maxUnusedTimePerItem = maxUnusedTimePerItemInSeconds * 1000;
            if (initialBufferSize > 0)
                Preallocate(initialBufferSize);
        }
        /// <inheritdoc />
        /// <summary>
        /// Preallocate a number of objects on the pool
        /// </summary>
        /// <param name="number">Number of objects to allocate</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Preallocate(int number)
        {
            for (var i = 0; i < number; i++)
            {
                _objectStack.Push(_createFunc(this));
                _poolTimeQueue.Enqueue(new PoolTime(this));
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Get a new instance from the pool
        /// </summary>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T New()
        {
            if (!_objectStack.TryPop(out var value))
                return _createFunc(this);
            if (_resetMode == PoolResetMode.BeforeUse)
                _resetAction?.Invoke(value);
            if (_poolTimeQueue.TryDequeue(out var time))
                time.Cancel();
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
            _poolTimeQueue.Enqueue(new PoolTime(this));
        }
        /// <inheritdoc />
        /// <summary>
        /// Get current objects in the pool
        /// </summary>
        /// <returns>IEnumerable with the current objects</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetCurrentObjects()
        {
            return _objectStack.ToReadOnly();
        }
        /// <inheritdoc />
        /// <summary>
        /// Clear the current object stack
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _objectStack.Clear();
            while (_poolTimeQueue.TryDequeue(out var item))
                item.Cancel();
        }
    }

    /// <summary>
    /// Object pool
    /// </summary>
    public sealed class ObjectPool<T, TPoolObjectLifecycle> : IPool<T>
        where TPoolObjectLifecycle : struct, IPoolObjectLifecycle<T>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly ConcurrentStack<T> _objectStack;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private TPoolObjectLifecycle _allocator;

        /// <summary>
        /// Object pool
        /// </summary>
        public ObjectPool()
        {
            _objectStack = new ConcurrentStack<T>();
            for (var i = 0; i < _allocator.InitialSize; i++)
                _objectStack.Push(_allocator.New());
        }
        /// <inheritdoc />
        /// <summary>
        /// Preallocate a number of objects on the pool
        /// </summary>
        /// <param name="number">Number of objects to allocate</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Preallocate(int number)
        {
            for (var i = 0; i < number; i++)
                _objectStack.Push(_allocator.New());
        }
        /// <inheritdoc />
        /// <summary>
        /// Get a new instance from the pool
        /// </summary>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T New()
        {
            if (!_objectStack.TryPop(out var value))
                return _allocator.New();
            if (_allocator.ResetMode == PoolResetMode.BeforeUse)
                _allocator.Reset(value);
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
            if (_allocator.ResetMode == PoolResetMode.AfterUse)
                _allocator.Reset(obj);
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
            return _objectStack.ToReadOnly();
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
