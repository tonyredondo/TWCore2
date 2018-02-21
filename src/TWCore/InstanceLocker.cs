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

namespace TWCore
{
    /// <summary>
    /// Instance Lock helps to create locks object based on instance values
    /// </summary>
    public class InstanceLocker<T>
    {
        private readonly object[] _lockers;

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Instance Lock helps to create locks object based on instance values
        /// </summary>
        public InstanceLocker() : this(Environment.ProcessorCount)
        {
        }
        /// <summary>
        /// Instance Lock helps to create locks object based on instance values
        /// </summary>
        /// <param name="concurrencyLevel">Number of locks for all instance values</param>
        public InstanceLocker(int concurrencyLevel)
        {
            _lockers = new object[concurrencyLevel];
            for (var i = 0; i < concurrencyLevel; i++)
                _lockers[i] = new object();
        }
        #endregion

        /// <summary>
        /// Get a Lock for use with lock(){} block
        /// </summary>
        /// <param name="key">Key to make a lock</param>
        /// <returns>Object for lock use</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetLock(T key)
        {
            var idx = (key?.GetHashCode() ?? 0) % _lockers.Length;
            return _lockers[idx];
        }

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
            var idx = (key?.GetHashCode() ?? 0) % _lockers.Length;
            lock (_lockers[idx])
                res = body();
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
            var idx = (key?.GetHashCode() ?? 0) % _lockers.Length;
            lock (_lockers[idx])
                body();
        }
    }
}