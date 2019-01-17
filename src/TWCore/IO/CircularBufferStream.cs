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
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TWCore.IO
{
    /// <inheritdoc />
    /// <summary>
    /// Circular buffer stream
    /// </summary>
    public sealed class CircularBufferStream : Stream
    {
        #region Fields
        private const int Start = 9;
        private readonly int _length;
        private readonly byte[] _buffer;
        private readonly ManualResetEventSlim _readEvent = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _writeEvent = new ManualResetEventSlim(false);
        private readonly object _readLock = new object();
        private readonly object _writeLock = new object();
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
        public override bool CanSeek => false;
        /// <inheritdoc />
        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => true;
        /// <inheritdoc />
        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length => -1;
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position { get { return -1; } set { } }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Circular buffer stream
        /// </summary>
        /// <param name="bufferSize">Buffer size, default size 65536 bytes</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CircularBufferStream(int bufferSize = 65536)
        {
            _length = bufferSize;
            _buffer = new byte[_length + Start];
        }
        #endregion

        #region Not Implemented Methods
        /// <inheritdoc />
        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override void Flush() { }
        /// <inheritdoc />
        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override void SetLength(long value) { }
        /// <inheritdoc />
        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override long Seek(long offset, SeekOrigin origin) => -1;
        #endregion

        #region Override Methods
        /// <inheritdoc />
        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override int Read(byte[] buffer, int offset, int count)
        {
            lock (_readLock)
            {
                var rPos = GetReadPosition();
                var wPos = GetWritePosition();
                while (GetReadFirst() == 0 && rPos == wPos)
                {
                    _writeEvent.Wait();
                    _writeEvent.Reset();
                    wPos = GetWritePosition();
                }
                var totalRead = 0;
                int cLength;
                if (GetReadFirst() == 0)
                    cLength = rPos > wPos ? _length : wPos;
                else
                    cLength = rPos >= wPos ? _length : wPos;
                var bufRemain = cLength - rPos;
                if (bufRemain >= count)
                {
                    Buffer.BlockCopy(_buffer, rPos + Start, buffer, offset, count);
                    totalRead += count;
                    rPos += count;
                    SetReadPosition(rPos == _length ? 0 : rPos);
                }
                else
                {
                    var remain = count - bufRemain;
                    Buffer.BlockCopy(_buffer, rPos + Start, buffer, offset, bufRemain);
                    totalRead += bufRemain;
                    rPos += bufRemain;
                    if (rPos == wPos)
                    {
                        SetReadPosition(rPos);
                    }
                    else
                    {
                        var canRead = Math.Min(remain, wPos);
                        Buffer.BlockCopy(_buffer, Start, buffer, offset + bufRemain, canRead);
                        totalRead += canRead;
                        SetReadPosition(canRead < remain ? canRead : remain);
                    }
                }
                _readEvent.Set();
                return totalRead;
            }
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
            lock (_writeLock)
            {
                var rPos = GetReadPosition();
                var wPos = GetWritePosition();
                while (GetReadFirst() == 1 && rPos == wPos)
                {
                    _readEvent.Wait();
                    _readEvent.Reset();
                    rPos = GetReadPosition();
                }
                var cLength = rPos <= wPos ? _length : rPos;
                var bufRemain = cLength - wPos;
                if (bufRemain >= count)
                {
                    Buffer.BlockCopy(buffer, offset, _buffer, wPos + Start, count);
                    wPos += count;
                    SetWritePosition(wPos == _length ? 0 : wPos);
                    if (rPos == wPos)
                        SetReadFirst(1);
                    _writeEvent.Set();
                }
                else
                {
                    var remain = count - bufRemain;
                    Buffer.BlockCopy(buffer, offset, _buffer, wPos + Start, bufRemain);
                    wPos += bufRemain;
                    if (wPos == rPos)
                    {
                        SetWritePosition(wPos);
                        SetReadFirst(1);
                        _writeEvent.Set();
                        Write(buffer, offset + bufRemain, remain);
                    }
                    else
                    {
                        var canWrite = Math.Min(remain, rPos);
                        Buffer.BlockCopy(buffer, offset + bufRemain, _buffer, Start, canWrite);
                        if (canWrite < remain)
                        {
                            SetWritePosition(canWrite);
                            remain = remain - canWrite;
                            SetReadFirst(1);
                            _writeEvent.Set();
                            Write(buffer, offset + bufRemain + canWrite, remain);
                        }
                        else
                        {
                            if (rPos == remain)
                                SetReadFirst(1);
                            SetWritePosition(remain);
                            _writeEvent.Set();
                        }
                    }
                }
            }
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe int GetReadPosition()
        {
            fixed (byte* rPos = &_buffer[0])
                return *(int*)rPos;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void SetReadPosition(int value)
        {
            fixed (byte* b = &_buffer[0])
                *((int*)b) = value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe int GetWritePosition()
        {
            fixed (byte* rPos = &_buffer[4])
                return *(int*)rPos;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void SetWritePosition(int value)
        {
            fixed (byte* b = &_buffer[4])
                *((int*)b) = value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte GetReadFirst() => _buffer[8];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetReadFirst(byte value) => _buffer[8] = value;
    }
}
