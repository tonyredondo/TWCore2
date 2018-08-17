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
                    var length = end.Position - start.Position + 1;
                    var remain = length % 8;
                    var xRow = xList[start.ArrayIndex].AsSpan(start.Position, length);
                    var yRow = yList[start.ArrayIndex].AsSpan(start.Position, length);
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
                }
                #endregion
                
                #region List Comparison

                var startLength = xSegmentsLength - start.Position;
                var startRemain = startLength % 8;

                var endLength = end.Position + 1;
                var endRemain = endLength % 8;
                
                int segmentRemain = 0;
                if (end.ArrayIndex - start.ArrayIndex > 1)
                    segmentRemain = xSegmentsLength % 8;

                for (var row = start.ArrayIndex; row <= end.ArrayIndex; row++)
                {
                    Span<byte> xRow;
                    Span<byte> yRow;
                    if (row == start.ArrayIndex)
                    {
                        xRow = xList[row].AsSpan(start.Position, startLength);
                        yRow = yList[row].AsSpan(start.Position, startLength);
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
                    else if (row == end.ArrayIndex)
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
            var start = data.FromGlobalIndex(offset);
            var end = data.FromGlobalIndex(offset + count - 1);
            var list = data.ListOfArrays;

            #region Single List Hash
            if (start.ArrayIndex == end.ArrayIndex)
            {
                var length = end.Position - start.Position + 1;
                var row = list[start.ArrayIndex].AsSpan(start.Position, length);
                var cRow = MemoryMarshal.Cast<byte, long>(row);
                var step = (cRow.Length / 16) + 1;
                for (var i = 0; i < cRow.Length; i += step)
                    res ^= (int)cRow[i];
                return res;
            }
            #endregion

            #region List Comparison
            for (var row = start.ArrayIndex; row <= end.ArrayIndex; row++)
            {
                Span<byte> lRow;
                if (row == start.ArrayIndex)
                    lRow = list[row].AsSpan(start.Position, segmentsLength - start.Position);
                else if (row == end.ArrayIndex)
                    lRow = list[row].AsSpan(0, end.Position + 1);
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