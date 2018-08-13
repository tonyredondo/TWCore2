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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IList<T[]> _listOfArrays;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly int _offset;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly int _count;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly int _segmentLength;

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
        public int ArrayCount => _listOfArrays.Count;
        #endregion
        
        #region .ctors
        /// <summary>
        /// Provides a MultiArray implementation without copying buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray(T[] array)
        {
            _listOfArrays = new[] { array };
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
            _listOfArrays = new[] { array ?? throw new ArgumentNullException(nameof(array)) };
            Ensure.GreaterEqualThan(offset, 0, "The offset should be a positive number.");
            Ensure.GreaterEqualThan(count, 0, "The count should be a positive number.");
            if (array.Length - offset < count)
                throw new ArgumentOutOfRangeException(nameof(count), "The count is invalid.");
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
            _listOfArrays = new[] { array.Array };
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
                    throw new ArgumentOutOfRangeException(nameof(segments), "All segments must has the same length");
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
                    throw new ArgumentOutOfRangeException(nameof(segments), "All segments must has the same length");
            var maxCount = _segmentLength * sCount;
            if (maxCount - offset < count)
                throw new ArgumentOutOfRangeException(nameof(count), "The count is invalid.");
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
                    index = Array.IndexOf(_listOfArrays[rowIndex], item, fromPosition, fromRowIndex != toRowIndex ? _segmentLength : _count);
                }
                else if (rowIndex == toRowIndex)
                {
                    index = Array.IndexOf(_listOfArrays[rowIndex], item, 0, toPosition + 1);                    
                }
                else
                {
                    index = Array.IndexOf(_listOfArrays[rowIndex], item, 0, _segmentLength);                                        
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
            => Slice(index, _count - index);
        /// <summary>
        /// Slice the MultiArray and Reduce it
        /// </summary>
        /// <param name="index">Index from the slice begins</param>
        /// <returns>New MultiArray instance</returns>
        public MultiArray<T> SliceAndReduce(int index)
            => SliceAndReduce(index, _count - index);
        /// <summary>
        /// Slice the MultiArray
        /// </summary>
        /// <param name="index">Index from the slice begins</param>
        /// <param name="count">Number of element of the slice</param>
        /// <returns>New MultiArray instance</returns>
        public MultiArray<T> Slice(int index, int count)
        {
            Ensure.GreaterEqualThan(index, 0, "Index should be a positive number.");
            Ensure.GreaterEqualThan(count, 0, "Count should be a positive number.");
            if (index > _count)
                throw new ArgumentOutOfRangeException(nameof(index), "The index should be lower than the total Array Count");
            if (_count - index < count)
                throw new ArgumentOutOfRangeException(nameof(count), "The count is invalid");
            return new MultiArray<T>(_listOfArrays, _offset + index, count);
        }
        /// <summary>
        /// Slice the MultiArray and Reduce it
        /// </summary>
        /// <param name="index">Index from the slice begins</param>
        /// <param name="count">Number of element of the slice</param>
        /// <returns>New MultiArray instance</returns>
        public MultiArray<T> SliceAndReduce(int index, int count)
        {
            Ensure.GreaterEqualThan(index, 0, "Index should be a positive number.");
            Ensure.GreaterEqualThan(count, 0, "Count should be a positive number.");
            if (index > _count)
                throw new ArgumentOutOfRangeException(nameof(index), "The index should be lower than the total Array Count");
            if (_count - index < count)
                throw new ArgumentOutOfRangeException(nameof(count), "The count is invalid");
            var (fromRowIndex, fromPosition) = FromGlobalIndex(index);
            var (toRowIndex, toPosition) = FromGlobalIndex(index + count - 1);
            var lst = new List<T[]>();
            for(var row = fromRowIndex; row <= toRowIndex; row++)
                lst.Add(_listOfArrays[row]);
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
            return "[MultiArray<" + typeof(T).Name + "> - Arrays: " + _listOfArrays.Count + " - Offset: " + _offset + " - Count: " + _count + "]";
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
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count - 1);
            for (var rowIndex = fromRowIndex; rowIndex <= toRowIndex; rowIndex++)
            {
                Span<byte> span;
                if (rowIndex == fromRowIndex)
                    span = arrays[rowIndex].AsSpan(fromPosition, fromRowIndex != toRowIndex ? _segmentLength - fromPosition : (toPosition - fromPosition) + 1);
                else if (rowIndex == toRowIndex)
                    span = arrays[rowIndex].AsSpan(0, toPosition);
                else
                    span = arrays[rowIndex].AsSpan(0, _segmentLength);
                stream.Write(span);
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
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count - 1);
            for (var rowIndex = fromRowIndex; rowIndex <= toRowIndex; rowIndex++)
            {
                Memory<byte> memory;
                if (rowIndex == fromRowIndex)
                    memory = arrays[rowIndex].AsMemory(fromPosition, fromRowIndex != toRowIndex ? _segmentLength - fromPosition : (toPosition - fromPosition) + 1);
                else if (rowIndex == toRowIndex)
                    memory = arrays[rowIndex].AsMemory(0, toPosition);
                else
                    memory = arrays[rowIndex].AsMemory(0, _segmentLength);
                await stream.WriteAsync(memory).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// Copy the MultiArray content to an Array 
        /// </summary>
        /// <param name="array">Array object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array) => WriteTo(array.AsSpan());
        /// <summary>
        /// Copy the MultiArray content to an Array 
        /// </summary>
        /// <param name="array">Array object</param>
        /// <param name="arrayIndex">Starting offset in the destination array</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex) => WriteTo(array.AsSpan(arrayIndex));

        /// <summary>
        /// Write data to a span
        /// </summary>
        /// <param name="span">Destination span</param>
        /// <return>Number of copied items</return>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int WriteTo(Span<T> span)
        {
            var copyLength = _count <= span.Length ? _count : span.Length;
            var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
            var (toRowIndex, toPosition) = FromGlobalIndex(_offset + copyLength - 1);
            int writeCount = 0;
            for (var rowIndex = fromRowIndex; rowIndex <= toRowIndex; rowIndex++)
            {
                Span<T> sourceSpan;
                if (rowIndex == fromRowIndex)
                    sourceSpan = _listOfArrays[rowIndex].AsSpan(fromPosition, fromRowIndex != toRowIndex ? _segmentLength - fromPosition : (toPosition - fromPosition) + 1);
                else if (rowIndex == toRowIndex)
                    sourceSpan = _listOfArrays[rowIndex].AsSpan(0, toPosition);
                else
                    sourceSpan = _listOfArrays[rowIndex].AsSpan(0, _segmentLength);
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
                return new MultiArrayReadOnlyStream(mBytes);
            throw new NotSupportedException("The type of MultiArray is not bytes");
        }
        /// <summary>
        /// Get a ReadOnlySequence from this MultiArray instance
        /// </summary>
        /// <returns>ReadOnlySequence instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySequence<T> AsReadOnlySequence()
        {
            if (_listOfArrays == null) return ReadOnlySequence<T>.Empty;
            if (_listOfArrays.Count == 0) return ReadOnlySequence<T>.Empty;
            ReadOnlySequence<T> sequence;
            if (_listOfArrays.Count == 1)
            {
                sequence = new ReadOnlySequence<T>(_listOfArrays[0]);
            }
            else
            {
                var (fromRowIndex, fromPosition) = FromGlobalIndex(_offset);
                var (toRowIndex, toPosition) = FromGlobalIndex(_offset + _count);
                var firstSegment = new SequenceSegment(_listOfArrays[fromRowIndex]);
                var lastSegment = firstSegment;
                for (var rowIndex = fromRowIndex + 1; rowIndex <= toRowIndex; rowIndex++)
                    lastSegment = lastSegment.Add(_listOfArrays[rowIndex]);
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
            WriteTo(arr.AsSpan());
            return arr;
        }
        /// <summary>
        /// Return the MultiArray as Span
        /// </summary>
        /// <returns>Span instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            return new Span<T>(ToArray());
        }
        /// <summary>
        /// Return the MultiArray as ReadOnlySpan
        /// </summary>
        /// <returns>Span instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            return new ReadOnlySpan<T>(ToArray());
        }
        /// <summary>
        /// Return the MultiArray as Memory
        /// </summary>
        /// <returns>Span instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory()
        {
            return new Memory<T>(ToArray());
        }
        /// <summary>
        /// Return the MultiArray as ReadOnlyMemory
        /// </summary>
        /// <returns>Span instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> AsReadOnlyMemory()
        {
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
                lst.Add(_listOfArrays[row]);
            return new MultiArray<T>(lst, fromPosition, _count);
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
            return (globalIndex / _segmentLength, globalIndex % _segmentLength);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ToGlobalIndex(int arrayIndex, int position)
        {
            return (arrayIndex * _segmentLength) + position;
        }
        #endregion
        
        #region Nested Types
        /// <inheritdoc />
        /// <summary>
        /// MultiArray Readonly Stream
        /// </summary>
        public class MultiArrayReadOnlyStream : Stream
        {
            private readonly MultiArray<byte> _source;
            private int _position;
            
            #region Properties
            public override bool CanRead => true;
            public override bool CanSeek => true;
            public override bool CanWrite => false;
            public override long Length => _source._count;
            public override long Position
            {
                get => _position;
                set => _position = (int) value;
            }
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
                if (_position == _source._count)
                    return 0;
                var writeSpan = buffer.AsSpan(offset, count);
                var bytes = _source.Slice(_position).WriteTo(writeSpan);
                _position += bytes;
                return bytes;
            }
            public override int ReadByte()
            {
                if (_position == _source._count)
                    return -1;
                return _source[_position++];
            }
            public override int Read(Span<byte> buffer)
            {
                if (_position == _source._count)
                    return 0;
                var bytes = _source.Slice(_position).WriteTo(buffer);
                _position += bytes;
                return bytes;
            }

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

        private class SequenceSegment : ReadOnlySequenceSegment<T>
        {
            public SequenceSegment(T[] segmentItem)
                => Memory = new ReadOnlyMemory<T>(segmentItem);
            
            public SequenceSegment Add(T[] segmentItem)
            {
                var segment = new SequenceSegment(segmentItem);
                segment.RunningIndex = RunningIndex + Memory.Length;
                Next = segment;
                return segment;
            }
        }
        #endregion
    }
}
