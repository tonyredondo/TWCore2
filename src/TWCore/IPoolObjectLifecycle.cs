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

namespace TWCore
{
    /// <summary>
    /// Pool Object Lifecycle
    /// </summary>
    /// <typeparam name="T">Type of object</typeparam>
    public interface IPoolObjectLifecycle<T>
    {
        /// <summary>
        /// Pool initial size
        /// </summary>
        int InitialSize { get; }
        /// <summary>
        /// Pool maximum size
        /// </summary>
        int MaximumSize { get; }
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
        /// <summary>
        /// Drop time frequency in seconds
        /// </summary>
        int DropTimeFrequencyInSeconds { get; }
        /// <summary>
        /// Drop size threshold
        /// </summary>
        int DropMaxSizeThreshold { get; }
        /// <summary>
        /// Drop quantity per time
        /// </summary>
        int DropQuantity { get; }
        /// <summary>
        /// Drop action
        /// </summary>
        /// <param name="value">Drop value</param>
        void DropAction(T value);
    }
}