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
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global

// ReSharper disable MemberCanBePrivate.Global

namespace TWCore
{
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
        internal readonly IList<T[]> ListOfArrays;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly int _offset;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly int _count;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly int _segmentsLength;

        #region Properties
        /// <summary>
        /// Items count
        /// </summary>
        public int Count => _count;
        /// <summary>
        /// Offset
        /// </summary>
        public int Offset => _offset;
        /// <summary>
        /// Arrays count
        /// </summary>
        public int ArrayCount => ListOfArrays.Count;
        /// <summary>
        /// Segments length
        /// </summary>
        public int SegmentsLength => _segmentsLength;
        /// <summary>
        /// Get if the MultiArray is empty
        /// </summary>
        public bool IsEmpty => _count == 0;
        #endregion
        
        #region .ctors
        /// <summary>
        /// Provides a MultiArray implementation without copying buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray(T[] array)
        {
            ListOfArrays = new[] { array };
            _offset = 0;
            _count = array?.Length ?? 0;
            _segmentsLength = _count;
        }
        /// <summary>
        /// Provides a MultiArray implementation without copying buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray(T[] array, int offset, int count)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "The offset should be a positive number.");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "The count should be a positive number.");
            if (array.Length - offset < count)
                throw new ArgumentOutOfRangeException(nameof(count), "The count is invalid.");
            ListOfArrays = new[] { array };
            _offset = offset;
            _count = count;
            _segmentsLength = _count;
        }
        /// <summary>
        /// Provides a MultiArray implementation without copying buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray(ArraySegment<T> array)
        {
            ListOfArrays = new[] { array.Array };
            _offset = array.Offset;
            _count = array.Count;
            _segmentsLength = _count;
        }
        /// <summary>
        /// Provides a MultiArray implementation without copying buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray(IList<T[]> segments)
        {
            ListOfArrays = segments;
            _offset = 0;
            var sCount = segments.Count;
            _segmentsLength = sCount > 0 ? segments[0].Length : 0;
            for (var i = 1; i < sCount; i++)
                if (segments[i].Length != _segmentsLength)
                    throw new ArgumentOutOfRangeException(nameof(segments), "All segments must has the same length");
            _count = _segmentsLength * sCount;
        }
        /// <summary>
        /// Provides a MultiArray implementation without copying buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray(IList<T[]> segments, int offset, int count)
        {
            ListOfArrays = segments;
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "The offset should be a positive number.");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "The count should be a positive number.");
            var sCount = segments.Count;
            _segmentsLength = sCount > 0 ? segments[0].Length : 0;
            for (var i = 1; i < sCount; i++)
                if (segments[i].Length != _segmentsLength)
                    throw new ArgumentOutOfRangeException(nameof(segments), "All segments must has the same length");
            var maxCount = _segmentsLength * sCount;
            if (maxCount - offset < count)
                throw new ArgumentOutOfRangeException(nameof(count), "The count is invalid.");
            _offset = offset;
            _count = count;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MultiArray(IList<T[]> segments, int offset, int count, int segmentsLength)
        {
            ListOfArrays = segments;
            _offset = offset;
            _count = count;
            _segmentsLength = segmentsLength;
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
                var globalIndex = index + _offset;
                var arrayIndex = globalIndex / _segmentsLength;
                var position = globalIndex % _segmentsLength;
                return ref ListOfArrays[arrayIndex][position];
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
                    index = Array.IndexOf(ListOfArrays[rowIndex], item, fromPosition, fromRowIndex != toRowIndex ? _segmentsLength : _count);
                }
                else if (rowIndex == toRowIndex)
                {
                    index = Array.IndexOf(ListOfArrays[rowIndex], item, 0, toPosition + 1);                    
                }
                else
                {
                    index = Array.IndexOf(ListOfArrays[rowIndex], item, 0, _segmentsLength);                                        
                }
                if (index == -1) continue;
                var globalIndex = ToGlobalIndex(rowIndex, index);
                return globalIndex - _offset;
            }
            return -1;
        }
        /// <summary>
        /// Slice the MultiArray
        /// </summary>
        /// <param name="index">Index from the slice begins</param>
        /// <returns>New MultiArray instance</returns>
        public MultiArray<T> Slice(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Index should be a positive number.");
            if (index > _count)
                throw new ArgumentOutOfRangeException(nameof(index), "The index should be lower than the total Array Count.");
            return new MultiArray<T>(ListOfArrays, _offset + index, _count - index, _segmentsLength);
        }
        /// <summary>
        /// Slice the MultiArray and Reduce it
        /// </summary>
        /// <param name="index">Index from the slice begins</param>
        /// <returns>New MultiArray instance</returns>
        public MultiArray<T> SliceAndReduce(int index)
            => SliceAndReduce(index, _count - index);
        /// <summary>
        /// Slice the MultiArray
        /// </summary>s
        /// <param name="index">Index from the slice begins</param>
        /// <param name="count">Number of element of the slice</param>
        /// <returns>New MultiArray instance</returns>
        public MultiArray<T> Slice(int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Index should be a positive number.");
            if (_count < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Count should be a positive number.");
            if (index > _count)
                throw new ArgumentOutOfRangeException(nameof(index), "The index should be lower than the total Array Count");
            if (_count - index < count)
                throw new ArgumentOutOfRangeException(nameof(count), "The count is invalid");
            return new MultiArray<T>(ListOfArrays, _offset + index, count, _segmentsLength);
        }
        /// <summary>
        /// Slice the MultiArray and Reduce it
        /// </summary>
        /// <param name="index">Index from the slice begins</param>
        /// <param name="count">Number of element of the slice</param>
        /// <returns>New MultiArray instance</returns>
        public MultiArray<T> SliceAndReduce(int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Index should be a positive number.");
            if (_count < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Count should be a positive number.");
            if (index > _count)
                throw new ArgumentOutOfRangeException(nameof(index), "The index should be lower than the total Array Count");
            if (_count - index < count)
                throw new ArgumentOutOfRangeException(nameof(count), "The count is invalid");
            var (fromRowIndex, fromPosition) = FromGlobalIndex(index);
            var (toRowIndex, toPosition) = FromGlobalIndex(index + count - 1);
            var lst = new List<T[]>();
            for(var row = fromRowIndex; row <= toRowIndex; row++)
                lst.Add(ListOfArrays[row]);
            return new MultiArray<T>(lst, fromPosition, count);
        }
        /// <summary>
        /// Gets if the MultiArray contains the item
        /// </summary>
        /// <param name="item">Item to look in the MultiArray instance</param>
        /// <returns>true if the MultiArray contains the element; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) => IndexOf(item) >= 0;
        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>Byte array hash value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            if (ListOfArrays is IList<byte[]> arrays)
                return MultiArrayBytesComparer.Instance.GetHashCode(new MultiArray<byte>(arrays, _offset, _count, _segmentsLength));
            var res = 0x2D2816FE;
            var step = (_count / 64) + 1;
            for (var i = 0; i < _count; i += step)
                res = res * 31 + this[i].GetHashCode();
            return res;
        }
        /// <summary>
        /// Gets if the MultiArray is equal to another object
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>true if the object is equal; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is MultiArray<T> sobj && Equals(sobj);
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
            if (obj._segmentsLength != _segmentsLength) return false;
            if (obj.ListOfArrays is null && ListOfArrays != null) return false;
            if (obj.ListOfArrays != null && ListOfArrays is null) return false;
            if (obj.ListOfArrays is null && ListOfArrays is null) return true;
            return obj.ListOfArrays.SequenceEqual(ListOfArrays);
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
            return "[MultiArray<" + typeof(T).Name + "> - Arrays: " + ListOfArrays.Count + " - Offset: " + _offset + " - Count: " + _count + "]";
        }
        /// <summary>
        /// Copy data to the stream
        /// </summary>
        /// <param name="stream">Stream instance</param>
        public void CopyTo(Stream stream)
        {
            if (!(ListOfArrays is IList<byte[]> arrays))
                throw new NotSupportedException("The type of MultiArray is not bytes");

            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count - 1);
            for (var rowIndex = fromRowIndex; rowIndex <= toRowIndex; rowIndex++)
            {
#if COMPATIBILITY
                byte[] tmpArray;
                int offset, length;
                if (rowIndex == fromRowIndex)
                {
                    offset = fromPosition;
                    length = fromRowIndex != toRowIndex ? _segmentsLength - fromPosition : (toPosition - fromPosition) + 1;
                    tmpArray = arrays[rowIndex];
                }
                else if (rowIndex == toRowIndex)
                {
                    offset = 0;
                    length = toPosition + 1;
                    tmpArray = arrays[rowIndex];
                }
                else
                {
                    offset = 0;
                    length = _segmentsLength;
                    tmpArray = arrays[rowIndex];
                }
                stream.Write(tmpArray, offset, length);
#else
                Span<byte> span;
                if (rowIndex == fromRowIndex)
                    span = arrays[rowIndex].AsSpan(fromPosition, fromRowIndex != toRowIndex ? _segmentsLength - fromPosition : (toPosition - fromPosition) + 1);
                else if (rowIndex == toRowIndex)
                    span = arrays[rowIndex].AsSpan(0, toPosition + 1);
                else
                    span = arrays[rowIndex].AsSpan(0, _segmentsLength);
                stream.Write(span);
#endif
            }
        }
        /// <summary>
        /// Copy data to the stream
        /// </summary>
        /// <param name="stream">Stream instance</param>
        public async Task CopyToAsync(Stream stream)
        {
            if (!(ListOfArrays is IList<byte[]> arrays))
                throw new NotSupportedException("The type of MultiArray is not bytes");
            
            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count - 1);
            for (var rowIndex = fromRowIndex; rowIndex <= toRowIndex; rowIndex++)
            {
#if COMPATIBILITY
                byte[] tmpArray;
                int offset, length;
                if (rowIndex == fromRowIndex)
                {
                    offset = fromPosition;
                    length = fromRowIndex != toRowIndex ? _segmentsLength - fromPosition : (toPosition - fromPosition) + 1;
                    tmpArray = arrays[rowIndex];
                }
                else if (rowIndex == toRowIndex)
                {
                    offset = 0;
                    length = toPosition + 1;
                    tmpArray = arrays[rowIndex];
                }
                else
                {
                    offset = 0;
                    length = _segmentsLength;
                    tmpArray = arrays[rowIndex];
                }
                await stream.WriteAsync(tmpArray, offset, length).ConfigureAwait(false);
#else
                Memory<byte> memory;
                if (rowIndex == fromRowIndex)
                    memory = arrays[rowIndex].AsMemory(fromPosition, fromRowIndex != toRowIndex ? _segmentsLength - fromPosition : (toPosition - fromPosition) + 1);
                else if (rowIndex == toRowIndex)
                    memory = arrays[rowIndex].AsMemory(0, toPosition + 1);
                else
                    memory = arrays[rowIndex].AsMemory(0, _segmentsLength);
                await stream.WriteAsync(memory).ConfigureAwait(false);
#endif
            }
        }
        /// <summary>
        /// Copy the MultiArray content to an Array 
        /// </summary>
        /// <param name="array">Array object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array) => CopyTo(array.AsSpan());
        /// <summary>
        /// Copy the MultiArray content to an Array 
        /// </summary>
        /// <param name="array">Array object</param>
        /// <param name="arrayIndex">Starting offset in the destination array</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex) => CopyTo(array.AsSpan(arrayIndex));

        /// <summary>
        /// Write data to a span
        /// </summary>
        /// <param name="span">Destination span</param>
        /// <return>Number of copied items</return>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CopyTo(Span<T> span)
        {
            var copyLength = _count <= span.Length ? _count : span.Length;
            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + copyLength - 1);
            int writeCount = 0;
            for (var rowIndex = fromRowIndex; rowIndex <= toRowIndex; rowIndex++)
            {
                Span<T> sourceSpan;
                if (rowIndex == fromRowIndex)
                    sourceSpan = ListOfArrays[rowIndex].AsSpan(fromPosition, fromRowIndex != toRowIndex ? _segmentsLength - fromPosition : (toPosition - fromPosition) + 1);
                else if (rowIndex == toRowIndex)
                    sourceSpan = ListOfArrays[rowIndex].AsSpan(0, toPosition + 1);
                else
                    sourceSpan = ListOfArrays[rowIndex].AsSpan(0, _segmentsLength);
                sourceSpan.CopyTo(span);
                span = span.Slice(sourceSpan.Length);
                writeCount += sourceSpan.Length;
            }
            return writeCount;
        }
        /// <summary>
        /// Get a Readonly stream from this MultiArray instance.
        /// </summary>
        /// <returns>Readonly stream</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Stream AsReadOnlyStream()
        {
            if (this is MultiArray<byte> mBytes)
                return MultiArrayReadOnlyStream.New(mBytes);
            throw new NotSupportedException("The type of MultiArray is not bytes");
        }
        /// <summary>
        /// Get a ReadOnlySequence from this MultiArray instance
        /// </summary>
        /// <returns>ReadOnlySequence instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySequence<T> AsReadOnlySequence()
        {
            if (ListOfArrays is null) return ReadOnlySequence<T>.Empty;
            if (ListOfArrays.Count == 0) return ReadOnlySequence<T>.Empty;
            ReadOnlySequence<T> sequence;
            if (ListOfArrays.Count == 1)
            {
                sequence = new ReadOnlySequence<T>(ListOfArrays[0]);
            }
            else
            {
                var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
                var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count);
                var firstSegment = new SequenceSegment(ListOfArrays[fromRowIndex]);
                var lastSegment = firstSegment;
                for (var rowIndex = fromRowIndex + 1; rowIndex <= toRowIndex; rowIndex++)
                    lastSegment = lastSegment.Add(ListOfArrays[rowIndex]);
                sequence = new ReadOnlySequence<T>(firstSegment, fromPosition, lastSegment, toPosition);
            }
            return sequence;
        }
        /// <summary>
        /// Convert the MultiArray to a single array
        /// </summary>
        /// <returns>Array instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            var arr = new T[_count];
            CopyTo(arr.AsSpan());
            return arr;
        }
        /// <summary>
        /// Gets the inner array when is a simple MultiArray with only one array
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] AsArray()
        {
            if (_offset == 0 && _count == _segmentsLength)
                return ListOfArrays[0];
            return ToArray();
        }
        /// <summary>
        /// Return the MultiArray as Span
        /// </summary>
        /// <returns>Span instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            if (ListOfArrays.Count == 1)
                return new Span<T>(ListOfArrays[0], _offset, _count);
            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count - 1);
            if (fromRowIndex == toRowIndex)
                return new Span<T>(ListOfArrays[fromRowIndex], fromPosition, (toPosition - fromPosition) + 1);
            return new Span<T>(ToArray());
        }
        /// <summary>
        /// Return the MultiArray as ReadOnlySpan
        /// </summary>
        /// <returns>Span instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            if (ListOfArrays.Count == 1)
                return new ReadOnlySpan<T>(ListOfArrays[0], _offset, _count);
            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count - 1);
            if (fromRowIndex == toRowIndex)
                return new ReadOnlySpan<T>(ListOfArrays[fromRowIndex], fromPosition, (toPosition - fromPosition) + 1);
            return new ReadOnlySpan<T>(ToArray());
        }
        /// <summary>
        /// Return the MultiArray as Memory
        /// </summary>
        /// <returns>Span instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory()
        {
            if (ListOfArrays.Count == 1)
                return new Memory<T>(ListOfArrays[0], _offset, _count);
            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count - 1);
            if (fromRowIndex == toRowIndex)
                return new Memory<T>(ListOfArrays[fromRowIndex], fromPosition, (toPosition - fromPosition) + 1);
            return new Memory<T>(ToArray());
        }
        /// <summary>
        /// Return the MultiArray as ReadOnlyMemory
        /// </summary>
        /// <returns>Span instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> AsReadOnlyMemory()
        {
            if (ListOfArrays.Count == 1)
                return new ReadOnlyMemory<T>(ListOfArrays[0], _offset, _count);
            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count - 1);
            if (fromRowIndex == toRowIndex)
                return new ReadOnlyMemory<T>(ListOfArrays[fromRowIndex], fromPosition, (toPosition - fromPosition) + 1);
            return new ReadOnlyMemory<T>(ToArray());
        }
        /// <summary>
        /// Get a MultiArray instance with only the necessary arrays
        /// </summary>
        /// <returns>MultiArray instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray<T> Reduce()
        {
            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count - 1);
            var lst = new List<T[]>();
            for(var row = fromRowIndex; row <= toRowIndex; row++)
                lst.Add(ListOfArrays[row]);
            return new MultiArray<T>(lst, fromPosition, _count);
        }
        
        /// <summary>
        /// Creates a MultiArray instance from an Array
        /// </summary>
        /// <param name="array">Array source</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MultiArray<T>(T[] array) => new MultiArray<T>(array);
        /// <summary>
        /// Creates a MultiArray instance from an ArraySegment instance
        /// </summary>
        /// <param name="arraySegment">ArraySegment source</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MultiArray<T>(ArraySegment<T> arraySegment) => new MultiArray<T>(arraySegment);
        /// <summary>
        /// Creates a MultiArray instance from a list of Arrays
        /// </summary>
        /// <param name="listOfSegments">List of arrays sources</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MultiArray<T>(List<T[]> listOfSegments) => new MultiArray<T>(listOfSegments);
        /// <summary>
        /// Creates a MultiArray instance from an Array of Arrays
        /// </summary>
        /// <param name="listOfSegments">Array of arrays sources</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MultiArray<T>(T[][] listOfSegments) => new MultiArray<T>(listOfSegments);
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal (int ArrayIndex, int Position) FromGlobalIndex(int globalIndex)
        {
            return (globalIndex / _segmentsLength, globalIndex % _segmentsLength);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int ToGlobalIndex(int arrayIndex, int position)
        {
            return (arrayIndex * _segmentsLength) + position;
        }
        #endregion
        
        #region Nested Types
        /// <inheritdoc />
        /// <summary>
        /// MultiArray Readonly Stream
        /// </summary>
        public class MultiArrayReadOnlyStream : Stream
        {
            private static ObjectPool<MultiArrayReadOnlyStream> StreamPool = new ObjectPool<MultiArrayReadOnlyStream>(i => new MultiArrayReadOnlyStream());

            private MultiArray<byte> _source;
            private int _position;
            private bool _disposed;

            #region Properties
            /// <inheritdoc />
            /// <summary>
            ///  Gets a value indicating whether the current stream supports reading.
            /// </summary>
            public override bool CanRead => true;
            /// <inheritdoc />
            /// <summary>
            /// Gets a value indicating whether the current stream supports seeking.
            /// </summary>
            public override bool CanSeek => true;
            /// <inheritdoc />
            /// <summary>
            /// Gets a value indicating whether the current stream supports writing.
            /// </summary>
            public override bool CanWrite => false;
            /// <inheritdoc />
            /// <summary>
            /// Gets the length in bytes of the stream.
            /// </summary>
            public override long Length => _source._count;
            /// <inheritdoc />
            /// <summary>
            /// Gets or sets the position within the current stream.
            /// </summary>
            public override long Position
            {
                get => _position;
                set => _position = (int) value;
            }
            #endregion

            #region Internals
            /// <summary>
            /// MultiArray Readonly Stream
            /// </summary>
            private MultiArrayReadOnlyStream()
            {
            }
            /// <summary>
            /// MultiArray Readonly stream finalizer
            /// </summary>
            ~MultiArrayReadOnlyStream()
            {
                if (_disposed) return;
                _source = MultiArray<byte>.Empty;
                _position = 0;
                _disposed = true;
                StreamPool.Store(this);
            }
            /// <summary>
            /// Creates a MultiArrayReadOnlyStream instance from a MultiArray source
            /// </summary>
            /// <param name="source">MultiArray source</param>
            /// <returns>A MultiArrayReadOnlyStream instance from the pool</returns>
            public static MultiArrayReadOnlyStream New(MultiArray<byte> source)
            {
                var stream = StreamPool.New();
                stream._source = source;
                stream._disposed = false;
                return stream;
            }
            /// <summary>
            /// Dispose all resources
            /// </summary>
            /// <param name="disposing"></param>
            protected override void Dispose(bool disposing)
            {
                if (_disposed) return;
                _source = MultiArray<byte>.Empty;
                _position = 0;
                _disposed = true;
                StreamPool.Store(this);
            }
            #endregion

            /// <inheritdoc />
            /// <summary>
            /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
            /// </summary>
            /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
            /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
            /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
            /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_position == _source._count)
                    return 0;
                var writeSpan = buffer.AsSpan(offset, count);
                var bytes = _source.Slice(_position).CopyTo(writeSpan);
                _position += bytes;
                return bytes;
            }
            /// <inheritdoc />
            /// <summary>
            /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
            /// </summary>
            /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int ReadByte()
            {
                if (_position == _source._count)
                    return -1;
                return _source[_position++];
            }
            /// <summary>
            /// Read and fills the span with a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
            /// </summary>
            /// <param name="buffer">Span buffer to fill</param>
            /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
#if COMPATIBILITY
            public int Read(Span<byte> buffer)
#else
            public override int Read(Span<byte> buffer)
#endif
            {
                if (_position == _source._count)
                    return 0;
                var bytes = _source.Slice(_position).CopyTo(buffer);
                _position += bytes;
                return bytes;
            }
            /// <inheritdoc />
            /// <summary>
            /// Sets the position within the current stream.
            /// </summary>
            /// <param name="offset">A byte offset relative to the origin parameter.</param>
            /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position.</param>
            /// <returns>The new position within the current stream.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override long Seek(long offset, SeekOrigin origin)
            {
                long res;
                if (origin == SeekOrigin.Begin)
                {
                    res = offset;
                }
                else if (origin == SeekOrigin.Current)
                {
                    res = Position + offset;
                }
                else
                {
                    res = _source._count + offset;
                }
                if (res < 0 || res > _source._count)
                    throw new ArgumentOutOfRangeException();
                Position = res;
                return Position;
            }
            /// <inheritdoc />
            /// <summary>
            /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void Flush()
            {
                throw new IOException("The stream is read only.");
            }
            /// <inheritdoc />
            /// <summary>
            /// Sets the length of the current stream.
            /// </summary>
            /// <param name="value">The desired length of the current stream in bytes.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void SetLength(long value)
            {
                throw new IOException("The stream is read only.");
            }
            /// <inheritdoc />
            /// <summary>
            ///  When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
            /// </summary>
            /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
            /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
            /// <param name="count">The number of bytes to be written to the current stream.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new IOException("The stream is read only.");
            }
        }

        private class SequenceSegment : ReadOnlySequenceSegment<T>
        {
            public SequenceSegment(T[] segmentItem)
                => Memory = new ReadOnlyMemory<T>(segmentItem);
            
            public SequenceSegment Add(T[] segmentItem)
            {
                var segment = new SequenceSegment(segmentItem)
                {
                    RunningIndex = RunningIndex + Memory.Length
                };
                Next = segment;
                return segment;
            }
        }
#endregion
    }
}
