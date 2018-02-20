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
using System.Runtime.CompilerServices;

namespace TWCore
{
    /// <inheritdoc />
    /// <summary>
    /// Byte array comparer
    /// </summary>
    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        /// <inheritdoc />
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">Byte array to compare</param>
        /// <param name="y">Byte array to compare</param>
        /// <returns>true if both arrays has the same items; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(byte[] x, byte[] y)
            => Factory.BytesEquals(x, y);
        /// <inheritdoc />
        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="data">Byte array to get the hash code</param>
        /// <returns>Byte array hash value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(byte[] data)
        {
            var res = 0x2D2816FE;
            var step = (data.Length / 64) + 1;
            for (var i = 0; i < data.Length; i += step)
                res = res * 31 + data[i];
            return res;
        }
    }
}
