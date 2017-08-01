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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TWCore
{
    /// <summary>
    /// Object Pool
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public sealed class ObjectPool<T>
    {
		readonly ConcurrentStack<T> objectStack;
		Func<T> createFunc;
        Action<T> resetAction;
        PoolResetMode resetMode;
        int count;
        int preallocationThreshold;
        bool allocating = false;

        /// <summary>
        /// Get the number of objects in the queue
        /// </summary>
        public int Count => count;

        /// <summary>
        /// Object pool
        /// </summary>
        /// <param name="createFunc">Function to create a new object</param>
        /// <param name="resetAction">Reset action before storing back the item in the pool</param>
        /// <param name="initialBufferSize">Initial buffer size</param>
        /// <param name="resetMode">Pool reset mode</param>
        /// <param name="preallocationThreshold">Number of items limit to create new allocations in another Task. Use 0 to disable, if is greater than the initial buffer then the initial buffer is set twice at this value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectPool(Func<T> createFunc, Action<T> resetAction = null, int initialBufferSize = 0, PoolResetMode resetMode = PoolResetMode.BeforeUse, int preallocationThreshold = 0)
        {
            objectStack = new ConcurrentStack<T>();
            this.createFunc = createFunc;
            this.resetAction = resetAction;
            this.resetMode = resetMode;
            this.preallocationThreshold = preallocationThreshold;
            if (this.preallocationThreshold > initialBufferSize)
                initialBufferSize = this.preallocationThreshold * 2;
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
                objectStack.Push(createFunc());
                Interlocked.Increment(ref this.count);
            }
        }
        /// <summary>
        /// Get a new instance from the pool
        /// </summary>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T New()
        {
            if (objectStack.TryPop(out var item))
            {
                if (Interlocked.Decrement(ref count) < preallocationThreshold && !allocating)
                {
                    Task.Run(() =>
                    {
                        allocating = true;
                        Preallocate(preallocationThreshold * 2);
                        allocating = false;
                    });
                }
                if (resetMode == PoolResetMode.BeforeUse)
                    resetAction?.Invoke(item);
                return item;
            }
            return createFunc();
        }
        /// <summary>
        /// Store the instance back to the pool
        /// </summary>
        /// <param name="obj">Object to store</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(T obj)
        {
            if (resetMode == PoolResetMode.AfterUse)
                resetAction?.Invoke(obj);
            objectStack.Push(obj);
            Interlocked.Increment(ref count);
        }
        /// <summary>
        /// Get current objects in the pool
        /// </summary>
        /// <returns>IEnumerable with the current objects</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetCurrentObjects()
        {
            return objectStack.ToReadOnly();
        }
        /// <summary>
        /// Clear the current object stack
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            objectStack.Clear();
            Interlocked.Exchange(ref count, 0);
        }
    }
}
