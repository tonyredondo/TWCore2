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
using System.Threading.Tasks;

namespace TWCore
{
    /// <summary>
    /// Action Delegate Helper Methods
    /// </summary>
    public static class ActionDelegate
    {
        /// <summary>
        /// Create Action delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action Create(Action result) => result;
        /// <summary>
        /// Create Action delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T1> Create<T1>(Action<T1> result) => result;
        /// <summary>
        /// Create Action delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T1, T2> Create<T1, T2>(Action<T1, T2> result) => result;
        /// <summary>
        /// Create Action delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T1, T2, T3> Create<T1, T2, T3>(Action<T1, T2, T3> result) => result;
        /// <summary>
        /// Create Action delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T1, T2, T3, T4> Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> result) => result;
        /// <summary>
        /// Create Action delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> result) => result;
        /// <summary>
        /// Create Async Action delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Task> CreateAsync(Action result) => result.CreateAsync();
        /// <summary>
        /// Create Async Action delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, Task> CreateAsync<T1>(Action<T1> result) => result.CreateAsync();
        /// <summary>
        /// Create Async Action delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, Task> CreateAsync<T1, T2>(Action<T1, T2> result) => result.CreateAsync();
        /// <summary>
        /// Create Async Action delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, Task> CreateAsync<T1, T2, T3>(Action<T1, T2, T3> result) => result.CreateAsync();
        /// <summary>
        /// Create Async Action delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, T4, Task> CreateAsync<T1, T2, T3, T4>(Action<T1, T2, T3, T4> result) => result.CreateAsync();
        /// <summary>
        /// Create Async Action delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, T4, T5, Task> CreateAsync<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> result) => result.CreateAsync();
    }
}
