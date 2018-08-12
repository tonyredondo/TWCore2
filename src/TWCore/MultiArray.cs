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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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
    public readonly struct MultiArray<T>
    {
        /// <summary>
        /// Empty MultiArray instance
        /// </summary>
        public static MultiArray<T> Empty = new MultiArray<T>();

        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
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
                if (index >= _count) throw new IndexOutOfRangeException();
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
            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count);
            for(var rowIndex = fromRowIndex; rowIndex <= toRowIndex; rowIndex++)
            {
                int index;
                if (rowIndex == fromRowIndex)
                {
                    index = Array.IndexOf(_listOfArrays[rowIndex], fromPosition, fromRowIndex != toRowIndex ? _segmentLength : _count);
                }
                else if (rowIndex == toRowIndex)
                {
                    index = Array.IndexOf(_listOfArrays[rowIndex], 0, toPosition + 1);                    
                }
                else
                {
                    index = Array.IndexOf(_listOfArrays[rowIndex], 0, _segmentLength);                                        
                }

                if (index > -1)
                {
                    var globalIndex = ToGlobalIndex(rowIndex, index);
                    return globalIndex - _offset;
                }
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
                throw new ArgumentOutOfRangeException(nameof(index), "The index should be lower than the total Array Count");
            if (_count - index < count)
                throw new ArgumentOutOfRangeException(nameof(count), "The count is invalid");
            return new MultiArray<T>(_listOfArrays, _offset + index, count);
        }
        /// <summary>
        /// Gets if the MultiArray contains the item
        /// </summary>
        /// <param name="item">Item to look in the MultiArray instance</param>
        /// <returns>true if the MultiArray contains the element; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) => IndexOf(item) >= 0;
        /// <summary>
        /// Gets the MultiArray HashCode
        /// </summary>
        /// <returns>HashCode</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => (_listOfArrays == null ? 0 : _listOfArrays.GetHashCode() ^_listOfArrays.Count) ^ _offset ^ _count ^ _segmentLength;
        /// <summary>
        /// Gets if the MultiArray is equal to another object
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>true if the object is equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is SubArray<T> sobj && Equals(sobj);
        /// <summary>
        /// Gets if the MultiArray is equal to another MultiArray
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>true if the object is equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(MultiArray<T> obj)
        {
            if (obj._offset != _offset) return false;
            if (obj._count != _count) return false;
            if (obj._segmentLength != _segmentLength) return false;
            if (obj._listOfArrays == null && _listOfArrays != null) return false;
            if (obj._listOfArrays != null && _listOfArrays == null) return false;
            if (obj._listOfArrays == null && _listOfArrays == null) return true;
            return obj._listOfArrays.SequenceEqual(_listOfArrays);
        }
        /// <summary>
        /// Gets if the MultiArray is equal to another MultiArray
        /// </summary>
        /// <param name="a">First SubArray instance</param>
        /// <param name="b">Second SubArray instance</param>
        /// <returns>true if the object is equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(MultiArray<T> a, MultiArray<T> b) => a.Equals(b);
        /// <summary>
        /// Gets if the MultiArray is different to another MultiArray
        /// </summary>
        /// <param name="a">First MultiArray instance</param>
        /// <param name="b">Second MultiArray instance</param>
        /// <returns>true if the object is different; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(MultiArray<T> a, MultiArray<T> b) => !(a == b);
        /// <summary>
        /// Get the String representation of the instance.
        /// </summary>
        /// <returns>String value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return "[MultiArray: " + typeof(T).Name + " - Offset: " + _offset + " - Count: " + _count + "]";
        }
        /// <summary>
        /// For each method in the inner array by reference
        /// </summary>
        /// <param name="delegate">ForEach delegate</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach(MultiArrayForEach<T> @delegate)
        {
            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count);
            for (var rowIndex = fromRowIndex; rowIndex <= toRowIndex; rowIndex++)
            {
                if (rowIndex == fromRowIndex)
                {
                    for (var index = fromPosition; index < (fromRowIndex != toRowIndex ? _segmentLength : toPosition); index++)
                        @delegate(ref _listOfArrays[rowIndex][index]);
                }
                else if (rowIndex == toRowIndex)
                {
                    for (var index = 0; index < toPosition; index++)
                        @delegate(ref _listOfArrays[rowIndex][index]);
                }
                else
                {
                    for (var index = 0; index < _segmentLength; index++)
                        @delegate(ref _listOfArrays[rowIndex][index]);
                }
            }
        }
        /// <summary>
        /// For each method in the inner array by reference
        /// </summary>
        /// <param name="delegate">ForEach delegate</param>
        /// <param name="arg1">Argument 1</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<TA1>(MultiArrayForEach<T, TA1> @delegate, ref TA1 arg1)
        {
            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count);
            for (var rowIndex = fromRowIndex; rowIndex <= toRowIndex; rowIndex++)
            {
                if (rowIndex == fromRowIndex)
                {
                    for (var index = fromPosition; index < (fromRowIndex != toRowIndex ? _segmentLength : toPosition); index++)
                        @delegate(ref _listOfArrays[rowIndex][index], ref arg1);
                }
                else if (rowIndex == toRowIndex)
                {
                    for (var index = 0; index < toPosition; index++)
                        @delegate(ref _listOfArrays[rowIndex][index], ref arg1);
                }
                else
                {
                    for (var index = 0; index < _segmentLength; index++)
                        @delegate(ref _listOfArrays[rowIndex][index], ref arg1);
                }
            }
        }
        /// <summary>
        /// For each method in the inner array by reference
        /// </summary>
        /// <param name="delegate">ForEach delegate</param>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<TA1, TA2>(MultiArrayForEach<T, TA1, TA2> @delegate, ref TA1 arg1, ref TA2 arg2)
        {
            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count);
            for (var rowIndex = fromRowIndex; rowIndex <= toRowIndex; rowIndex++)
            {
                if (rowIndex == fromRowIndex)
                {
                    for (var index = fromPosition; index < (fromRowIndex != toRowIndex ? _segmentLength : toPosition); index++)
                        @delegate(ref _listOfArrays[rowIndex][index], ref arg1, ref arg2);
                }
                else if (rowIndex == toRowIndex)
                {
                    for (var index = 0; index < toPosition; index++)
                        @delegate(ref _listOfArrays[rowIndex][index], ref arg1, ref arg2);
                }
                else
                {
                    for (var index = 0; index < _segmentLength; index++)
                        @delegate(ref _listOfArrays[rowIndex][index], ref arg1, ref arg2);
                }
            }
        }
        /// <summary>
        /// Copy data to the stream
        /// </summary>
        /// <param name="stream">Stream instance</param>
        public void CopyTo(Stream stream)
        {
            if (!(_listOfArrays is IList<byte[]> arrays))
                throw new NotSupportedException("The type of MultiArray is not bytes");

            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count);
            for (var rowIndex = fromRowIndex; rowIndex <= toRowIndex; rowIndex++)
            {
                if (rowIndex == fromRowIndex)
                {
                    var span = arrays[rowIndex].AsSpan(fromPosition, fromRowIndex != toRowIndex ? _segmentLength : toPosition);
                    stream.Write(span);
                }
                else if (rowIndex == toRowIndex)
                {
                    var span = arrays[rowIndex].AsSpan(0, toPosition);
                    stream.Write(span);
                }
                else
                {
                    var span = arrays[rowIndex].AsSpan(0, _segmentLength);
                    stream.Write(span);
                }
            }
        }
        /// <summary>
        /// Copy data to the stream
        /// </summary>
        /// <param name="stream">Stream instance</param>
        public async Task CopyToAsync(Stream stream)
        {
            if (!(_listOfArrays is IList<byte[]> arrays))
                throw new NotSupportedException("The type of MultiArray is not bytes");
            
            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count);
            for (var rowIndex = fromRowIndex; rowIndex <= toRowIndex; rowIndex++)
            {
                if (rowIndex == fromRowIndex)
                {
                    var memory = arrays[rowIndex].AsMemory(fromPosition, fromRowIndex != toRowIndex ? _segmentLength : toPosition);
                    await stream.WriteAsync(memory).ConfigureAwait(false);
                }
                else if (rowIndex == toRowIndex)
                {
                    var memory = arrays[rowIndex].AsMemory(0, toPosition);
                    await stream.WriteAsync(memory).ConfigureAwait(false);
                }
                else
                {
                    var memory = arrays[rowIndex].AsMemory(0, _segmentLength);
                    await stream.WriteAsync(memory).ConfigureAwait(false);
                }
            }
        }
        /// <summary>
        /// Get a Readonly stream from this MultiArray instance.
        /// </summary>
        /// <returns>Readonly stream</returns>
        public Stream AsReadOnlyStream()
        {
            if (this is MultiArray<byte> mBytes)
                return new MultiArrayReadOnlyStream(mBytes);
            throw new NotSupportedException("The type of MultiArray is not bytes");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MultiArray<T>(T[] array) => new MultiArray<T>(array);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MultiArray<T>(ArraySegment<T> arraySegment) => new MultiArray<T>(arraySegment);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MultiArray<T>(List<T[]> listOfSegments) => new MultiArray<T>(listOfSegments);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MultiArray<T>(T[][] listOfSegments) => new MultiArray<T>(listOfSegments);
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (int ArrayIndex, int Position) FromGlobalIndex(int globalIndex)
        {
            var val = (double)globalIndex / _segmentLength;
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
        
        #region Nested Types
        /// <summary>
        /// MultiArray Readonly Stream
        /// </summary>
        public class MultiArrayReadOnlyStream : Stream
        {
            private MultiArray<byte> _source;

            #region Properties
            public override bool CanRead => true;
            public override bool CanSeek => true;
            public override bool CanWrite => false;
            public override long Length => _source._count;
            public override long Position { get; set; }
            #endregion

            #region .ctor
            /// <summary>
            /// MultiArray Readonly Stream
            /// </summary>
            /// <param name="source">MultiArray source</param>
            internal MultiArrayReadOnlyStream(MultiArray<byte> source)
            {
                _source = source;
            }
            #endregion
            
            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            
            public override void Flush()
            {
                throw new IOException("The stream is read only.");
            }
            public override void SetLength(long value)
            {
                throw new IOException("The stream is read only.");
            }
            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new IOException("The stream is read only.");
            }
        }
        #endregion
    }
}
