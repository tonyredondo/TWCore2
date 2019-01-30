using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
            var xCount = x.Count;
            var yCount = y.Count;
            if (xCount != yCount) return false;
            var xOffset = x.Offset;
            var xSegmentsLength = x.SegmentsLength;
            if (xOffset == y.Offset && xSegmentsLength == y.SegmentsLength)
            {
                var startIndex = x.FromGlobalIndex(xOffset, out var startPosition);
                var endIndex = x.FromGlobalIndex(xOffset + xCount - 1, out var endPosition);
                var xList = x.ListOfArrays;
                var yList = y.ListOfArrays;

                #region Single List Comparison
                if (startIndex == endIndex)
                {
                    var length = endPosition - startPosition + 1;
                    var xRow = new Span<byte>(xList[startIndex], startPosition, length);
                    var yRow = new Span<byte>(yList[startIndex], startPosition, length);

#if COMPATIBILITY
                    var remain = length % 8;
                    if (remain > 0)
                    {
                        var splitIndex = length - remain;
                        var ryRow = yRow.Slice(splitIndex);
                        var rxRow = xRow.Slice(splitIndex);
                        for (var i = 0; i < rxRow.Length; i++)
                        {
                            if (rxRow[i] != ryRow[i])
                                return false;
                        }
                    }
                    var cxRow = MemoryMarshal.Cast<byte, long>(xRow);
                    var cyRow = MemoryMarshal.Cast<byte, long>(yRow);
                    for (var i = 0; i < cyRow.Length; i++)
                    {
                        if (cxRow[i] != cyRow[i])
                            return false;
                    }
                    return true;
#else
                    var xVector = new Vector<byte>(xRow);
                    var yVector = new Vector<byte>(yRow);
                    return Vector.EqualsAll(xVector, yVector);
#endif
                }
                #endregion
                
                #region List Comparison

                var startLength = xSegmentsLength - startPosition;
                var startRemain = startLength % 8;

                var endLength = endPosition + 1;
                var endRemain = endLength % 8;
                
                int segmentRemain = 0;
                if (endIndex - startIndex > 1)
                    segmentRemain = xSegmentsLength % 8;

                for (var row = startIndex; row <= endIndex; row++)
                {
                    Span<byte> xRow;
                    Span<byte> yRow;

#if COMPATIBILITY
                    if (row == startIndex)
                    {
                        xRow = xList[row].AsSpan(startPosition, startLength);
                        yRow = yList[row].AsSpan(startPosition, startLength);
                        if (startRemain > 0)
                        {
                            var splitIndex = startLength - startRemain;
                            var rxRow = xRow.Slice(splitIndex);
                            var ryRow = yRow.Slice(splitIndex);
                            for (var i = 0; i < rxRow.Length; i++)
                            {
                                if (rxRow[i] != ryRow[i])
                                    return false;
                            }
                        }
                    }
                    else if (row == endIndex)
                    {
                        xRow = xList[row].AsSpan(0, endLength);
                        yRow = yList[row].AsSpan(0, endLength);
                        if (endRemain > 0)
                        {
                            var splitIndex = endLength - endRemain;
                            var rxRow = xRow.Slice(splitIndex);
                            var ryRow = yRow.Slice(splitIndex);
                            for (var i = 0; i < rxRow.Length; i++)
                            {
                                if (rxRow[i] != ryRow[i])
                                    return false;
                            }
                        }
                    }
                    else
                    {
                        xRow = xList[row].AsSpan(0, xSegmentsLength);
                        yRow = yList[row].AsSpan(0, xSegmentsLength);
                        if (segmentRemain > 0)
                        {
                            var splitIndex = xSegmentsLength - segmentRemain;
                            var rxRow = xRow.Slice(splitIndex);
                            var ryRow = yRow.Slice(splitIndex);
                            for (var i = 0; i < rxRow.Length; i++)
                            {
                                if (rxRow[i] != ryRow[i])
                                    return false;
                            }
                        }
                    }

                    var cxRow = MemoryMarshal.Cast<byte, long>(xRow);
                    var cyRow = MemoryMarshal.Cast<byte, long>(yRow);

                    for (var i = 0; i < cyRow.Length; i++)
                    {
                        if (cxRow[i] != cyRow[i])
                            return false;
                    }
#else
                    if (row == startIndex)
                    {
                        xRow = xList[row].AsSpan(startPosition, startLength);
                        yRow = yList[row].AsSpan(startPosition, startLength);
                    }
                    else if (row == endIndex)
                    {
                        xRow = xList[row].AsSpan(0, endLength);
                        yRow = yList[row].AsSpan(0, endLength);
                    }
                    else
                    {
                        xRow = xList[row].AsSpan(0, xSegmentsLength);
                        yRow = yList[row].AsSpan(0, xSegmentsLength);
                    }

                    var xVector = new Vector<byte>(xRow);
                    var yVector = new Vector<byte>(yRow);
                    if (!Vector.EqualsAll(xVector, yVector))
                        return false;
#endif
                }
                return true;
                #endregion
            }
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
            var count = data.Count;
            var offset = data.Offset;
            var segmentsLength = data.SegmentsLength;
            var startIndex = data.FromGlobalIndex(offset, out var startPosition);
            var endIndex = data.FromGlobalIndex(offset + count - 1, out var endPosition);
            var list = data.ListOfArrays;

            #region Single List Hash
            if (startIndex == endIndex)
            {
                var length = endPosition - startPosition + 1;
                var row = list[startIndex].AsSpan(startPosition, length);
                var cRow = MemoryMarshal.Cast<byte, long>(row);
                var step = (cRow.Length / 16) + 1;
                for (var i = 0; i < cRow.Length; i += step)
                    res ^= (int)cRow[i];
                return res;
            }
            #endregion

            #region List Comparison
            for (var row = startIndex; row <= endIndex; row++)
            {
                Span<byte> lRow;
                if (row == startIndex)
                    lRow = list[row].AsSpan(startPosition, segmentsLength - startPosition);
                else if (row == endIndex)
                    lRow = list[row].AsSpan(0, endPosition + 1);
                else
                    lRow = list[row].AsSpan(0, segmentsLength);

                var cRow = MemoryMarshal.Cast<byte, long>(lRow);
                var step = (cRow.Length / 16) + 1;
                for (var i = 0; i < cRow.Length; i += step)
                    res ^= (int)cRow[i];
            }
            #endregion
            
            return res;
        }
    }
}