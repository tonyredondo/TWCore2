using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TWCore
{
    /// <inheritdoc />
    /// <summary>
    /// Byte MultiArray Comparer
    /// </summary>
    public class MultiArrayBytesComparer: IEqualityComparer<MultiArray<byte>>
    {
        /// <summary>
        /// Byte MultiArray comparer instance
        /// </summary>
        public static readonly MultiArrayBytesComparer Instance = new MultiArrayBytesComparer();

        private MultiArrayBytesComparer()
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
        public bool Equals(MultiArray<byte> x, MultiArray<byte> y)
        {
            if (x.Count != y.Count) return false;
            for (var i = 0; i < x.Count; i++)
            {
                if (x[i] != y[i])
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
        public int GetHashCode(MultiArray<byte> data)
        {
            var res = 0x2D2816FE;
            var step = (data.Count / 64) + 1;
            for (var i = 0; i < data.Count; i += step)
                res = res * 31 + data[i];
            return res;
        }
    }
}