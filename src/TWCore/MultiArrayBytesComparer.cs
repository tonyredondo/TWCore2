using System;
using System.Collections.Generic;
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
                var start = x.FromGlobalIndex(xOffset);
                var end = x.FromGlobalIndex(xOffset + xCount - 1);
                var xList = x.ListOfArrays;
                var yList = y.ListOfArrays;

                #region Single List Comparison
                if (start.ArrayIndex == end.ArrayIndex)
                {
                    var remain = (end.Position - start.Position + 1) % 8;
                    var length = (end.Position - start.Position + 1) - remain;
                    var xRow = xList[start.ArrayIndex].AsSpan(start.Position, length);
                    var yRow = yList[start.ArrayIndex].AsSpan(start.Position, length);
                    if (remain > 0)
                    {
                        var spanLength = end.Position + 1 - length;
                        var ryRow = yList[start.ArrayIndex].AsSpan(length, spanLength);
                        var rxRow = xList[start.ArrayIndex].AsSpan(length, spanLength);
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
                }
                #endregion
                
                #region List Comparison
                int segmentRemain = 0, segmentLengthForLong = 0;
                var startRemain = (xSegmentsLength - start.Position) % 8;
                var startSegmentLengthForLong = xSegmentsLength - start.Position - startRemain;
                var endRemain = (end.Position + 1) % 8;
                var endSegmentLengthForLong = (end.Position + 1) - endRemain;
                if (end.ArrayIndex - start.ArrayIndex > 1)
                {
                    segmentRemain = xSegmentsLength % 8;
                    segmentLengthForLong = xSegmentsLength - segmentRemain;
                }

                for (var row = start.ArrayIndex; row <= end.ArrayIndex; row++)
                {
                    Span<byte> xRow;
                    Span<byte> yRow;
                    if (row == start.ArrayIndex)
                    {
                        xRow = xList[row].AsSpan(start.Position, startSegmentLengthForLong);
                        yRow = yList[row].AsSpan(start.Position, startSegmentLengthForLong);
                        if (startRemain > 0)
                        {
                            var spanLength = xSegmentsLength - startSegmentLengthForLong;
                            var rxRow = xList[row].AsSpan(startSegmentLengthForLong, spanLength);
                            var ryRow = yList[row].AsSpan(startSegmentLengthForLong, spanLength);
                            for (var i = 0; i < rxRow.Length; i++)
                            {
                                if (rxRow[i] != ryRow[i])
                                    return false;
                            }
                        }
                    }
                    else if (row == end.ArrayIndex)
                    {
                        xRow = xList[row].AsSpan(0, endSegmentLengthForLong);
                        yRow = yList[row].AsSpan(0, endSegmentLengthForLong);
                        if (endRemain > 0)
                        {
                            var spanLength = end.Position + 1 - endSegmentLengthForLong;
                            var rxRow = xList[row].AsSpan(endSegmentLengthForLong, spanLength);
                            var ryRow = yList[row].AsSpan(endSegmentLengthForLong, spanLength);
                            for (var i = 0; i < rxRow.Length; i++)
                            {
                                if (rxRow[i] != ryRow[i])
                                    return false;
                            }
                        }
                    }
                    else
                    {
                        xRow = xList[row].AsSpan(0, segmentLengthForLong);
                        yRow = yList[row].AsSpan(0, segmentLengthForLong);
                        if (segmentRemain > 0)
                        {
                            var spanLength = xSegmentsLength - segmentLengthForLong;
                            var rxRow = xList[row].AsSpan(segmentLengthForLong, spanLength);
                            var ryRow = yList[row].AsSpan(segmentLengthForLong, spanLength);
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
            var step = (data.Count / 64) + 1;
            for (var i = 0; i < data.Count; i += step)
                res = res * 31 + data[i];
            return res;
        }
    }
}