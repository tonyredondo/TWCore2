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

using System.IO;
using System.Runtime.CompilerServices;
// ReSharper disable CheckNamespace

namespace TWCore
{
    /// <summary>
    /// Helper to SubArray common operations
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Get the MultiArray from the MemoryStream buffer instance.
        /// </summary>
        /// <param name="mStream">MemoryStream instance</param>
        /// <returns>MultiArray instance from the buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MultiArray<byte> ToMultiArray(this MemoryStream mStream)
            => mStream != null ? new MultiArray<byte>(mStream.GetBuffer(), 0, (int)mStream.Length) : MultiArray<byte>.Empty;
        /// <summary>
        /// Gets the MultiArray from a string
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>MultiArray instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MultiArray<char> ToMultiArray(this string value)
            => value?.ToCharArray() ?? MultiArray<char>.Empty;
    }
}
