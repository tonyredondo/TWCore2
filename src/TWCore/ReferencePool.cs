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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TWCore
{
    /// <summary>
    /// Object by reference Pool
    /// </summary>
    /// <typeparam name="T">Object type, must be a class with a default constructor</typeparam>
    public sealed class ReferencePool<T> where T : class, new()
    {
		#region Statics
		/// <summary>
		/// Gets the shared reference pool
		/// </summary>
		public static ReferencePool<T> Shared => Singleton<ReferencePool<T>>.Instance;
        #endregion

        readonly object _padLock = new object();
		readonly Stack<T> objectStack;
		Action<T> resetAction;
        Action<T> onetimeInitAction;
        PoolResetMode resetMode;
        int preallocationThreshold;
        bool allocating = false;

        /// <summary>
        /// Get the number of objects in the queue
        /// </summary>
        public int Count => objectStack.Count;

		/// <summary>
		/// Private .ctor for Singleton instance
		/// </summary>
		private ReferencePool() : this(0, null, null, PoolResetMode.BeforeUse, 0) { }

        /// <summary>
        /// Object by reference Pool
        /// </summary>
        /// <param name="initialBufferSize">Initial buffer size</param>
        /// <param name="resetAction">Reset action before storing back the item in the pool</param>
        /// <param name="onetimeInitAction">Action to execute after a new object creation</param>
        /// <param name="resetMode">Pool reset mode</param>
        /// <param name="preallocationThreshold">Number of items limit to create new allocations in another Task. Use 0 to disable, if is greater than the initial buffer then the initial buffer is set twice at this value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReferencePool(int initialBufferSize = 0, Action<T> resetAction = null, Action<T> onetimeInitAction = null, PoolResetMode resetMode = PoolResetMode.BeforeUse, int preallocationThreshold = 0)
        {
            objectStack = new Stack<T>(12);
            this.resetAction = resetAction;
            this.onetimeInitAction = onetimeInitAction;
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
                T t = new T();
                onetimeInitAction?.Invoke(t);
                lock (_padLock)
                    objectStack.Push(t);
            }
        }
        /// <summary>
        /// Get a new instance from the pool
        /// </summary>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T New()
        {
            T value = null;
            lock(_padLock)
            {
                var count = objectStack.Count;
                if (count > 0)
                {
                    value = objectStack.Pop();
                    if (count - 1 < preallocationThreshold && !allocating)
                    {
                        Task.Run(() =>
                        {
                            allocating = true;
                            Preallocate(preallocationThreshold * 2);
                            allocating = false;
                        });
                    }
                }
            }
            if (value == null)
            {
                value = new T();
                onetimeInitAction?.Invoke(value);
                return value;
            }
            if (resetMode == PoolResetMode.BeforeUse && resetAction != null)
                resetAction(value);
            return value;
        }
        /// <summary>
        /// Store the instance back to the pool
        /// </summary>
        /// <param name="obj">Object to store</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(T obj)
        {
            if (resetMode == PoolResetMode.AfterUse && resetAction != null)
                resetAction(obj);
            lock (_padLock)
                objectStack.Push(obj);
        }
        /// <summary>
        /// Get current objects in the pool
        /// </summary>
        /// <returns>IEnumerable with the current objects</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetCurrentObjects()
        {
            lock(_padLock)
                return objectStack.ToArray();
        }
        /// <summary>
        /// Clear the current object stack
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            lock(_padLock)
                objectStack.Clear();
        }
    }
}
