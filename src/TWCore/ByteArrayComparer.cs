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
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TWCore
{
    /// <inheritdoc />
    /// <summary>
    /// Byte array comparer
    /// </summary>
    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        /// <summary>
        /// Byte Array comparer instace
        /// </summary>
        public static ByteArrayComparer Instance = new ByteArrayComparer();

        private ByteArrayComparer()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">Byte array to compare</param>
        /// <param name="y">Byte array to compare</param>
        /// <returns>true if both arrays has the same items; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(byte[] x, byte[] y)
        {
            if (x == y)
                return true;
            if (x is null || y is null || x.Length != y.Length)
                return false;

            var length = x.Length;
            var xs = x.AsSpan();
            var ys = y.AsSpan();

            var remain = length % 8;
            if (remain > 0)
            {
                var splitIndex = length - remain;
                var rxRow = xs.Slice(splitIndex);
                var ryRow = ys.Slice(splitIndex);
                for (var i = 0; i < rxRow.Length; i++)
                {
                    if (rxRow[i] != ryRow[i])
                        return false;
                }
            }
            var cxRow = MemoryMarshal.Cast<byte, long>(xs);
            var cyRow = MemoryMarshal.Cast<byte, long>(ys);
            for (var i = 0; i < cyRow.Length; i++)
            {
                if (cxRow[i] != cyRow[i])
                    return false;
            }
            return true;
        }
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
            var row = data.AsSpan();
            var cRow = MemoryMarshal.Cast<byte, long>(row);
            var step = (cRow.Length / 16) + 1;
            for (var i = 0; i < cRow.Length; i += step)
                res ^= (int)cRow[i];
            return res;
        }
    }
}
