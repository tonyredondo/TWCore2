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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TWCore
{
    /// <summary>
    /// Func Delegate Helper Methods
    /// </summary>
    public static class FuncDelegate
    {
        /// <summary>
        /// Create Func delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<TResult> Create<TResult>(Func<TResult> result) => result;
        /// <summary>
        /// Create Func delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, TResult> Create<T1, TResult>(Func<T1, TResult> result) => result;
        /// <summary>
        /// Create Func delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, TResult> Create<T1, T2, TResult>(Func<T1, T2, TResult> result) => result;
        /// <summary>
        /// Create Func delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, TResult> Create<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> result) => result;
        /// <summary>
        /// Create Func delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, T4, TResult> Create<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> result) => result;
        /// <summary>
        /// Create Func delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, T4, T5, TResult> Create<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> result) => result;
        /// <summary>
        /// Create Async Func delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Task<TResult>> CreateAsync<TResult>(Func<TResult> result) => result.CreateAsync();
        /// <summary>
        /// Create Async Func delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, Task<TResult>> CreateAsync<T1, TResult>(Func<T1, TResult> result) => result.CreateAsync();
        /// <summary>
        /// Create Async Func delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, Task<TResult>> CreateAsync<T1, T2, TResult>(Func<T1, T2, TResult> result) => result.CreateAsync();
        /// <summary>
        /// Create Async Func delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, Task<TResult>> CreateAsync<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> result) => result.CreateAsync();
        /// <summary>
        /// Create Async Func delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, T4, Task<TResult>> CreateAsync<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> result) => result.CreateAsync();
        /// <summary>
        /// Create Async Func delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T1, T2, T3, T4, T5, Task<TResult>> CreateAsync<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> result) => result.CreateAsync();
    }
}
