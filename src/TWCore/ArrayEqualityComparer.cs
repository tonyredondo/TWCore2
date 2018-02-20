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
    /// <summary>
    /// Array Equality Comparer
    /// </summary>
    public static class ArrayEqualityComparer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEqualityComparer<T[]> Create<T>(IEqualityComparer<T> comparer)
            => new ArrayEqualityComparer<T>(comparer);
    }

    /// <inheritdoc />
    /// <summary>
    /// Array Equality Comparer
    /// </summary>
    public sealed class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
    {
        public static IEqualityComparer<T[]> Default { get; } = new ArrayEqualityComparer<T>();
        private readonly IEqualityComparer<T> _elementComparer;

        /// <inheritdoc />
        /// <summary>
        /// Array Equality Comparer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArrayEqualityComparer() : this(EqualityComparer<T>.Default) { }
        /// <summary>
        /// Array Equality Comparer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArrayEqualityComparer(IEqualityComparer<T> elementComparer)
        {
            _elementComparer = elementComparer;
        }

        /// <inheritdoc />
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">Byte array to compare</param>
        /// <param name="y">Byte array to compare</param>
        /// <returns>true if both arrays has the same items; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(T[] x, T[] y)
        {
            if (x == y)
                return true;
            if (x == null || y == null || x.Length != y.Length)
                return false;
            for (var i = 0; i < x.Length; i++)
            {
                if (!_elementComparer.Equals(x[i], y[i]))
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
        public int GetHashCode(T[] data)
        {
            var res = 0x2D2816FE;
            var step = (data.Length / 64) + 1;
            for (var i = 0; i < data.Length; i += step)
                res = res * 31 + _elementComparer.GetHashCode(data[i]);
            return res;
        }
    }
}
