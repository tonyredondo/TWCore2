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
// ReSharper disable IntroduceOptionalParameters.Global

namespace TWCore.IO
{
    /// <inheritdoc />
    /// <summary>
    /// Recycle ByteArray MemoryStream
    /// </summary>
    public class RecycleMemoryStream : Stream
    {
        private static readonly ObjectPool<byte[], BytePoolAllocator> ByteArrayPool = new ObjectPool<byte[], BytePoolAllocator>();
        private static readonly ObjectPool<List<byte[]>, ListBytePoolAllocator> ListByteArrayPool = new ObjectPool<List<byte[]>, ListBytePoolAllocator>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private const int MaxLength = 1024;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly bool _canWrite;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _length;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _maxRow;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _rowIndex;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _position;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private List<byte[]> _buffer;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private byte[] _currentBuffer;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _collectPoolItems = true;

        #region Allocators
        private struct BytePoolAllocator : IPoolObjectLifecycle<byte[]>
        {
            public int InitialSize => 4;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            public int DropTimeFrequencyInSeconds => 120;
            public void DropAction(byte[] value) {}
            public byte[] New() => new byte[MaxLength];
            public void Reset(byte[] value) => Array.Clear(value, 0, MaxLength);
        }
        private struct ListBytePoolAllocator : IPoolObjectLifecycle<List<byte[]>>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            public int DropTimeFrequencyInSeconds => 60;
            public void DropAction(List<byte[]> value) { }
            public List<byte[]> New() => new List<byte[]>(10);
            public void Reset(List<byte[]> value) => value.Clear();
        }
        #endregion

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
        public override bool CanWrite => _canWrite;
        /// <inheritdoc />
        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
		public override long Length => (_maxRow * MaxLength) + _length;
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (_rowIndex * MaxLength) + _position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _rowIndex = (int)value / MaxLength;
                _position = (int)value % MaxLength;
                _currentBuffer = _buffer[_rowIndex];
            }
        }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Recycle ByteArray MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream() : this(null, 0, 0, true) { }
        /// <inheritdoc />
        /// <summary>
        /// Recycle ByteArray MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream(byte[] buffer) : this(buffer, 0, buffer?.Length ?? 0, true) { }
        /// <inheritdoc />
        /// <summary>
        /// Recycle ByteArray MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream(byte[] buffer, bool writable) : this(buffer, 0, buffer?.Length ?? 0, writable) { }
        /// <inheritdoc />
        /// <summary>
        /// Recycle ByteArray MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream(byte[] buffer, int index, int count) : this(buffer, index, count, true) { }
        /// <inheritdoc />
        /// <summary>
        /// Recycle ByteArray MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream(byte[] buffer, int index, int count, bool writable)
        {
            _currentBuffer = ByteArrayPool.New();
            _buffer = ListByteArrayPool.New();
            _buffer.Add(_currentBuffer);
            _maxRow = 0;
            if (buffer != null)
                Write(buffer, index, count);
            _canWrite = writable;
        }
        /// <summary>
        /// RecycleMemoryStream finalizer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~RecycleMemoryStream()
        {
            Dispose(true);
        }
        /// <summary>
        /// Dispose all internal resources
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Dispose(bool disposing)
        {
            if (_buffer != null)
            {
                _currentBuffer = null;
                if (_collectPoolItems)
                {
                    foreach (var array in _buffer)
                        ByteArrayPool.Store(array);
                    ListByteArrayPool.Store(_buffer);
                }
                _buffer = null;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Abstract Override Methods
        /// <inheritdoc />
        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Flush() { }
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
            var length = ((_buffer.Count - 1) * MaxLength) + _length;
            var currentPosition = (_rowIndex * MaxLength) + _position;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    currentPosition = (int)offset;
                    break;
                case SeekOrigin.Current:
                    currentPosition += (int)offset;
                    break;
                case SeekOrigin.End:
                    currentPosition = length + (int)offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }
            Position = currentPosition;
            return currentPosition;
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetLength(long value)
        {
            var val = ((double)value / MaxLength);
            var rowIndex = (int)val;
            var nRows = rowIndex + 1;
            var nlength = (int)((val - rowIndex) * MaxLength);
            if (nRows == _buffer.Count)
            {
                if (_length > nlength)
                    Array.Clear(_currentBuffer, nlength - 1, _length - nlength);
                return;
            }
            if (nRows < _buffer.Count)
            {
                while (nRows < _buffer.Count)
                {
                    ByteArrayPool.Store(_currentBuffer);
                    var lastIdx = _buffer.Count - 1;
                    _currentBuffer = _buffer[lastIdx];
                    _buffer.RemoveAt(lastIdx);
                    _length = nlength;
                }
                Array.Clear(_currentBuffer, _length - 1, MaxLength - _length);
            }
            if (nRows <= _buffer.Count) return;
            for (var i = _buffer.Count; i < nRows; i++)
            {
                _currentBuffer = ByteArrayPool.New();
                _buffer.Add(_currentBuffer);
                _maxRow++;
            }
        }
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
            var total = 0;
            while (count > 0)
            {
                var clength = (_rowIndex == _maxRow) ? _length : MaxLength;
                var remain = clength - _position;
                if (remain == 0)
                {
                    if (_rowIndex < _maxRow)
                    {
                        _rowIndex++;
                        _currentBuffer = _buffer[_rowIndex];
                        _position = 0;
                        remain = (_rowIndex == _maxRow) ? _length : MaxLength;
                    }
                    else
                        return total;
                }
                var canRead = remain < count ? remain : count;
                Buffer.BlockCopy(_currentBuffer, _position, buffer, offset, canRead);
                total += canRead;
                _position += canRead;
                offset += canRead;
                count -= canRead;
            }
            return total;
        }
        /// <inheritdoc />
        /// <summary>
        ///  When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                var remain = MaxLength - _position;
                if (remain == 0)
                {
                    _currentBuffer = ByteArrayPool.New();
                    _buffer.Add(_currentBuffer);
                    _maxRow++;
                    _rowIndex++;
                    _position = 0;
                    _length = 0;
                    remain = MaxLength;
                }
                var canWrite = remain < count ? remain : count;
                Buffer.BlockCopy(buffer, offset, _currentBuffer, _position, canWrite);
                offset += canWrite;
                _position += canWrite;
                if (_position > _length) _length = _position;
                count -= canWrite;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteByte(byte value)
        {
            if (MaxLength > _position)
            {
                _currentBuffer[_position] = value;
                _position++;
                if (_position > _length) _length = _position;
                return;
            }
            _currentBuffer = ByteArrayPool.New();
            _buffer.Add(_currentBuffer);
            _maxRow++;
            _rowIndex++;
            _position = 1;
            _length = 1;
            _currentBuffer[0] = value;
        }
        /// <inheritdoc />
        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadByte()
        {
            if (_rowIndex == _maxRow)
                return (_length > _position) ? _currentBuffer[_position++] : -1;
            if (MaxLength > _position)
                return _currentBuffer[_position++];
            _rowIndex++;
            _currentBuffer = _buffer[_rowIndex];
            _position = 1;
            return _currentBuffer[0];
        }
        /// <summary>
        /// Writes the stream contents to a byte array, regardless of the Position property
        /// </summary>
        /// <returns>A new byte array</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ToArray()
        {
            var tmp = new byte[Length];
            for (var i = 0; i < _maxRow; i++)
                Buffer.BlockCopy(_buffer[i], 0, tmp, i * MaxLength, MaxLength);
            Buffer.BlockCopy(_buffer[_maxRow], 0, tmp, _maxRow * MaxLength, _length);
            return tmp;
        }
        /// <summary>
        /// Get the internal buffer as MultiArray
        /// </summary>
        /// <returns>MultiArray instance</returns>
        public MultiArray<byte> GetMultiArray()
        {
            _collectPoolItems = false;
            return new MultiArray<byte>(_buffer, 0, (int)Length);
        }
        #endregion
    }
}
