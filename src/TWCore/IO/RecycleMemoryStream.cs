﻿/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace TWCore.IO
{
    /// <summary>
    /// Recycle ByteArray MemoryStream
    /// </summary>
    public class RecycleMemoryStream : Stream
    {
        static ConcurrentQueue<byte[]> _pool = new ConcurrentQueue<byte[]>();
        int _maxLength = 255;
        bool _canWrite = true;
        int _length = 0;
        int _maxRow = 0;
        int _rowIndex = 0;
        int _position = 0;
        List<byte[]> _buffer;
        byte[] _currentBuffer;
        byte[] _singleByte = new byte[1];

        #region Properties
        /// <summary>
        ///  Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => true;
        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => true;
        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => _canWrite;
        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length => ((_buffer.Count - 1) * _maxLength) + _length;
        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (_rowIndex * _maxLength) + _position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var val = ((double)value / _maxLength);
                _rowIndex = (int)val;
                _position = (int)((val - _rowIndex) * _maxLength);
                _currentBuffer = _buffer[_rowIndex];
            }
        }
        #endregion

        #region .ctor
        /// <summary>
        /// Recycle ByteArray MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream() : this(null, 0, 0, true) { }
        /// <summary>
        /// Recycle ByteArray MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream(byte[] buffer) : this(buffer, 0, buffer?.Length ?? 0, true) { }
        /// <summary>
        /// Recycle ByteArray MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream(byte[] buffer, bool writable) : this(buffer, 0, buffer?.Length ?? 0, writable) { }
        /// <summary>
        /// Recycle ByteArray MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream(byte[] buffer, int index, int count) : this(buffer, index, count, true) { }
        /// <summary>
        /// Recycle ByteArray MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream(byte[] buffer, int index, int count, bool writable)
        {
            _currentBuffer = GetArray();
            _buffer = new List<byte[]> { _currentBuffer };
            _maxRow = 0;
            if (buffer != null)
                Write(buffer, index, count);
            _canWrite = writable;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~RecycleMemoryStream()
        {
            Dispose(true);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Dispose(bool disposing)
        {
            _currentBuffer = null;
            if (_buffer != null)
            {
                foreach (var array in _buffer)
                    StoreArray(array);
                _buffer.Clear();
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Abstract Override Methods
        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Flush() { }
        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long Seek(long offset, SeekOrigin origin)
        {
            var length = ((_buffer.Count - 1) * _maxLength) + _length;
            var currentPosition = (_rowIndex * _maxLength) + _position;
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
            }
            Position = currentPosition;
            return currentPosition;
        }
        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetLength(long value)
        {
            var val = ((double)value / _maxLength);
            var rowIndex = (int)val;
            var nRows = rowIndex + 1;
            var nlength = (int)((val - rowIndex) * _maxLength);
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
                    StoreArray(_currentBuffer);
                    var lastIdx = _buffer.Count - 1;
                    _currentBuffer = _buffer[lastIdx];
                    _buffer.RemoveAt(lastIdx);
                    _length = nlength;
                }
                Array.Clear(_currentBuffer, _length - 1, _maxLength - _length);
            }
            if (nRows > _buffer.Count)
            {
                for (var i = _buffer.Count; i < nRows; i++)
                {
                    _currentBuffer = GetArray();
                    _buffer.Add(_currentBuffer);
                    _maxRow++;
                }
            }
        }
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
            int total = 0;
            while (count > 0)
            {
                var clength = (_rowIndex == _maxRow) ? _length : _maxLength;
                var remain = clength - _position;
                if (remain == 0)
                {
                    if (_rowIndex < _maxRow)
                    {
                        _rowIndex++;
                        _currentBuffer = _buffer[_rowIndex];
                        _position = 0;
                        remain = (_rowIndex == _maxRow) ? _length : _maxLength;
                    }
                    else
                        return total;
                }
                var canRead = remain < count ? remain : count;
                Buffer.BlockCopy(_currentBuffer, _position, buffer, offset, canRead);
                count -= canRead;
                offset += canRead;
                total += canRead;
                _position += canRead;
            }
            return total;
        }
        /// <summary>
        ///  When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                var remain = _maxLength - _position;
                if (remain == 0)
                {
                    _currentBuffer = GetArray();
                    _buffer.Add(_currentBuffer);
                    _maxRow++;
                    _rowIndex++;
                    _position = 0;
                    _length = 0;
                    remain = _maxLength;
                }
                var canWrite = remain < count ? remain : count;
                Buffer.BlockCopy(buffer, offset, _currentBuffer, _position, canWrite);
                count -= canWrite;
                offset += canWrite;
                _position += canWrite;
                if (_position > _length) _length = _position;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteByte(byte value)
        {
            _singleByte[0] = value;
            Write(_singleByte, 0, 1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadByte()
        {
            if (Read(_singleByte, 0, 1) > 0)
                return _singleByte[0];
            return -1;
        }
        #endregion

        #region Pool Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte[] GetArray()
        {
            if (!_pool.TryDequeue(out var res))
                res = new byte[_maxLength];
            return res;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void StoreArray(byte[] array)
        {
            if (array.Length != _maxLength) return;
            Array.Clear(array, 0, _maxLength);
            _pool.Enqueue(array);
        }
        #endregion
    }
}
