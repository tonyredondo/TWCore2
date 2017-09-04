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
using System.Runtime.CompilerServices;

namespace TWCore
{
    /// <summary>
    /// Instance Lock helps to create locks object based on instance values using a ConcurrentDictionary internally
    /// </summary>
    public class InstanceLocker<T>
    {
        private readonly ConcurrentDictionary<T, object> _lockDict = new ConcurrentDictionary<T, object>();

        /// <summary>
        /// Get a Lock for use with lock(){} block
        /// </summary>
        /// <param name="key">Key to make a lock</param>
        /// <returns>Object for lock use</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetLock(T key) 
            => _lockDict.GetOrAdd(key, s => new object());

        /// <summary>
        /// Run a short lock inline using a lambda
        /// </summary>
        /// <param name="key">Key to make a lock</param>
        /// <param name="body">Function to be executed in lock</param>
        /// <returns>Return value of the function</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult RunWithLock<TResult>(T key, Func<TResult> body)
        {
            TResult res;
            lock (_lockDict.GetOrAdd(key, s => new object()))
                res = body();
            _lockDict.TryRemove(key, out var _);
            return res;
        }

        /// <summary>
        /// Run a short lock inline using a lambda
        /// </summary>
        /// <param name="key">Key to make a lock</param>
        /// <param name="body">Action to be executed in lock</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RunWithLock(T key, Action body)
        {
            lock (_lockDict.GetOrAdd(key, s => new object()))
                body();
            _lockDict.TryRemove(key, out var _);
        }

        /// <summary>
        /// Remove an old lock object that is no longer needed
        /// </summary>
        /// <param name="key">Key to make a lock</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveLock(T key)
        {
            _lockDict.TryRemove(key, out object _);
        }
    }
}