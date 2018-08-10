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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore
{
    /// <summary>
    /// MultiArray for each delegate
    /// </summary>
    public delegate void MultiArrayForEach<T>(ref T value);
    /// <summary>
    /// MultiArray for each delegate
    /// </summary>
    public delegate void MultiArrayForEach<T, TA1>(ref T value, ref TA1 arg1);
    /// <summary>
	/// MultiArray for each delegate
	/// </summary>
	public delegate void MultiArrayForEach<T, TA1, TA2>(ref T value, ref TA1 arg1, ref TA2 arg2);


    /// <summary>
    /// Provides a MultiArray implementation without copying buffer.
    /// </summary>
    /// <typeparam name="T">Type of element</typeparam>
    [DataContract]
    [Serializable]
    public struct MultiArray<T>
    {
        /// <summary>
        /// Empty MultiArray instance
        /// </summary>
        public static MultiArray<T> Empty = new MultiArray<T>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IList<T[]> _listOfArrays;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly int _offset;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly int _count;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly int _segmentLength;

        #region .ctors
        /// <summary>
        /// Provides a MultiArray implementation without copying buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray(T[] array)
        {
            _listOfArrays = new T[][] { array };
            _offset = 0;
            _count = array?.Length ?? 0;
            _segmentLength = _count;
        }
        /// <summary>
        /// Provides a MultiArray implementation without copying buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray(T[] array, int offset, int count)
        {
            _listOfArrays = new T[][] { array ?? throw new ArgumentNullException("array") };
            Ensure.GreaterEqualThan(offset, 0, "The offset should be a positive number.");
            Ensure.GreaterEqualThan(count, 0, "The count should be a positive number.");
            if (array.Length - offset < count)
                throw new ArgumentOutOfRangeException("The count is invalid.");
            _offset = offset;
            _count = count;
            _segmentLength = _count;
        }
        /// <summary>
        /// Provides a MultiArray implementation without copying buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray(ArraySegment<T> array)
        {
            _listOfArrays = new T[][] { array.Array };
            _offset = array.Offset;
            _count = array.Count;
            _segmentLength = _count;
        }
        /// <summary>
        /// Provides a MultiArray implementation without copying buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray(IList<T[]> segments)
        {
            _listOfArrays = segments;
            _offset = 0;
            var sCount = segments.Count;
            _segmentLength = sCount > 0 ? segments[0].Length : 0;
            for (var i = 1; i < sCount; i++)
                if (segments[i].Length != _segmentLength)
                    throw new ArgumentOutOfRangeException("All segments must has the same length");
            _count = _segmentLength * sCount;
        }
        /// <summary>
        /// Provides a MultiArray implementation without copying buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray(IList<T[]> segments, int offset, int count)
        {
            _listOfArrays = segments;
            Ensure.GreaterEqualThan(offset, 0, "The offset should be a positive number.");
            Ensure.GreaterEqualThan(count, 0, "The count should be a positive number.");
            var sCount = segments.Count;
            _segmentLength = sCount > 0 ? segments[0].Length : 0;
            for (var i = 1; i < sCount; i++)
                if (segments[i].Length != _segmentLength)
                    throw new ArgumentOutOfRangeException("All segments must has the same length");
            var maxCount = _segmentLength * sCount;
            if (maxCount - offset < count)
                throw new ArgumentOutOfRangeException("The count is invalid.");
            _offset = offset;
            _count = count;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Reference to an element inside the array
        /// </summary>
        /// <param name="index">Item index</param>
        /// <returns>Reference to the element of the index</returns>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var (arrayIndex, position) = FromGlobalIndex(index + _offset);
                return ref _listOfArrays[arrayIndex][position];
            }
        }
        /// <summary>
        /// Gets the index of an item
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns>Index of the item inside the SubArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item)
        {
            var fromIndex = FromGlobalIndex(_offset);
            var position = fromIndex.Position;
            var remain = _count;
            for(var rowIndex = fromIndex.ArrayIndex; rowIndex < _listOfArrays.Count; rowIndex++)
            {
                var localCount = Math.Min(remain, _segmentLength);
                var index = Array.IndexOf(_listOfArrays[rowIndex], item, position, localCount);
                if (index > -1)
                {
                    var globalIndex = ToGlobalIndex(rowIndex, index);
                    return globalIndex - _offset;
                }
                remain -= localCount;
                position = 0;
            }
            return -1;
        }
        /// <summary>
        /// Slice the MultiArray
        /// </summary>
        /// <param name="index">Index from the slice begins</param>
        /// <returns>New MultiArray instance</returns>
        public MultiArray<T> Slice(int index)
            => Slice(index, _count - index);
        /// <summary>
        /// Slice the MultiArray
        /// </summary>
        /// <param name="index">Index from the slice begins</param>
        /// <param name="count">Number of element of the slice</param>
        /// <returns>New MultiArray instance</returns>
        public MultiArray<T> Slice(int index, int count)
        {
            Ensure.GreaterEqualThan(index, 0, "Index should be a possitive number.");
            Ensure.GreaterEqualThan(count, 0, "Count should be a possitive number.");
            if (index > _count)
                throw new ArgumentOutOfRangeException("The index should be lower than the total Array Count");
            if (_count - index < count)
                throw new ArgumentOutOfRangeException("The count is invalid");
            return new MultiArray<T>(_listOfArrays, _offset + index, count);
        }
        /// <summary>
        /// Gets if the MultiArray contains the item
        /// </summary>
        /// <param name="item">Item to look in the MultiArray instance</param>
        /// <returns>true if the MultiArray contains the element; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) => IndexOf(item) >= 0;
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (int ArrayIndex, int Position) FromGlobalIndex(int globalIndex)
        {
            var val = ((double)globalIndex / _segmentLength);
            var arrayIndex = (int)val;
            var position = (int)((val - arrayIndex) * _segmentLength);
            return (arrayIndex, position);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ToGlobalIndex(int arrayIndex, int position)
        {
            return (arrayIndex * _segmentLength) + position;
        }
        #endregion
    }
}
