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

using System.Collections.Generic;

namespace TWCore
{
    /// <summary>
    /// Pool Interface
    /// </summary>
    /// <typeparam name="T">Type of the pool</typeparam>
    public interface IPool<T>
    {

        /// <summary>
        /// Pool count
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Preallocate a number of objects on the pool
        /// </summary>
        /// <param name="number">Number of objects to allocate</param>
        void Preallocate(int number);
        /// <summary>
        /// Get a new instance from the pool
        /// </summary>
        /// <returns>Object instance</returns>
        T New();
        /// <summary>
        /// Store the instance back to the pool
        /// </summary>
        /// <param name="obj">Object to store</param>
        void Store(T obj);
        /// <summary>
        /// Get current objects in the pool
        /// </summary>
        /// <returns>IEnumerable with the current objects</returns>
        IEnumerable<T> GetCurrentObjects();
        /// <summary>
        /// Clear the current object stack
        /// </summary>
        void Clear();
    }
}