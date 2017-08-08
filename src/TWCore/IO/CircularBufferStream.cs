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
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TWCore.IO
{
    /// <summary>
    /// Circular buffer stream
    /// </summary>
	[System.Diagnostics.DebuggerNonUserCode]
    public class CircularBufferStream : Stream
    {
        #region Fields
        int start = 9;
        int length = 0;
        byte[] _buffer;
        readonly ManualResetEventSlim readEvent = new ManualResetEventSlim(false);
        readonly ManualResetEventSlim writeEvent = new ManualResetEventSlim(false);
        readonly object readLock = new object();
        readonly object writeLock = new object();
        #endregion

        #region Properties
        /// <summary>
        ///  Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => true;
        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => false;
        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => true;
        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length => -1;
        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position { get { return -1; } set { } }
        #endregion

        #region .ctor
        /// <summary>
        /// Circular buffer stream
        /// </summary>
        /// <param name="bufferSize">Buffer size, default size 65536 bytes</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CircularBufferStream(int bufferSize = 65536)
        {
            length = bufferSize;
            _buffer = new byte[length + start];
        }
        #endregion

        #region Not Implemented Methods
        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Flush() { }
        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetLength(long value) { }
        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override long Seek(long offset, SeekOrigin origin) => -1;
        #endregion

        #region Override Methods
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
            lock (readLock)
            {
                var rPos = GetReadPosition();
                var wPos = GetWritePosition();
                while (GetReadFirst() == 0 && rPos == wPos)
                {
                    writeEvent.Wait();
                    writeEvent.Reset();
                    wPos = GetWritePosition();
                }
                int totalRead = 0;
                int cLength = 0;
                if (GetReadFirst() == 0)
                    cLength = rPos > wPos ? length : wPos;
                else
                    cLength = rPos >= wPos ? length : wPos;
                var bufRemain = cLength - rPos;
                if (bufRemain >= count)
                {
                    Buffer.BlockCopy(_buffer, rPos + start, buffer, offset, count);
                    totalRead += count;
                    rPos += count;
                    if (rPos == length)
                        SetReadPosition(0);
                    else
                        SetReadPosition(rPos);
                }
                else
                {
                    var remain = count - bufRemain;
                    Buffer.BlockCopy(_buffer, rPos + start, buffer, offset, bufRemain);
                    totalRead += bufRemain;
                    rPos += bufRemain;
                    if (rPos == wPos)
                    {
                        SetReadPosition(rPos);
                    }
                    else
                    {
                        var canRead = Math.Min(remain, wPos);
                        Buffer.BlockCopy(_buffer, start, buffer, offset + bufRemain, canRead);
                        totalRead += canRead;
                        if (canRead < remain)
                            SetReadPosition(canRead);
                        else
                            SetReadPosition(remain);
                    }
                }
                readEvent.Set();
                return totalRead;
            }
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
            lock (writeLock)
            {
                var rPos = GetReadPosition();
                var wPos = GetWritePosition();
                while (GetReadFirst() == 1 && rPos == wPos)
                {
                    readEvent.Wait();
                    readEvent.Reset();
                    rPos = GetReadPosition();
                }
                int cLength = rPos <= wPos ? length : rPos;
                var bufRemain = cLength - wPos;
                if (bufRemain >= count)
                {
                    Buffer.BlockCopy(buffer, offset, _buffer, wPos + start, count);
                    wPos += count;
                    if (wPos == length)
                        SetWritePosition(0);
                    else
                        SetWritePosition(wPos);
                    if (rPos == wPos)
                        SetReadFirst(1);
                    writeEvent.Set();
                }
                else
                {
                    var remain = count - bufRemain;
                    Buffer.BlockCopy(buffer, offset, _buffer, wPos + start, bufRemain);
                    wPos += bufRemain;
                    if (wPos == rPos)
                    {
                        SetWritePosition(wPos);
                        SetReadFirst(1);
                        writeEvent.Set();
                        Write(buffer, offset + bufRemain, remain);
                    }
                    else
                    {
                        var canWrite = Math.Min(remain, rPos);
                        Buffer.BlockCopy(buffer, offset + bufRemain, _buffer, start, canWrite);
                        if (canWrite < remain)
                        {
                            SetWritePosition(canWrite);
                            remain = remain - canWrite;
                            SetReadFirst(1);
                            writeEvent.Set();
                            Write(buffer, offset + bufRemain + canWrite, remain);
                        }
                        else
                        {
                            if (rPos == remain)
                                SetReadFirst(1);
                            SetWritePosition(remain);
                            writeEvent.Set();
                        }
                    }
                }
            }
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe int GetReadPosition()
        {
            fixed (byte* rPos = &_buffer[0])
                return *(int*)rPos;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void SetReadPosition(int value)
        {
            fixed (byte* b = &_buffer[0])
                *((int*)b) = value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe int GetWritePosition()
        {
            fixed (byte* rPos = &_buffer[4])
                return *(int*)rPos;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void SetWritePosition(int value)
        {
            fixed (byte* b = &_buffer[4])
                *((int*)b) = value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte GetReadFirst() => _buffer[8];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetReadFirst(byte value) => _buffer[8] = value;
    }
}
