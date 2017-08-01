/*
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
    public class CircularBufferStream : Stream
    {
        #region Fields
        byte[] _buffer;
        int ReadPosition => BitConverter.ToInt32(_buffer, 0);
        int WritePosition => BitConverter.ToInt32(_buffer, 4);
        ManualResetEventSlim readEvent = new ManualResetEventSlim(true);
        ManualResetEventSlim writeEvent = new ManualResetEventSlim(true);
        bool firstTime = false;
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
            _buffer = new byte[bufferSize + 8];
            Buffer.BlockCopy(BitConverter.GetBytes(-1), 0, _buffer, 4, 4);
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
        public override long Seek(long offset, SeekOrigin origin) { return -1; }
        #endregion

        #region Override Methods
        byte[] byteBuffer = new byte[1];
        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadByte()
        {
            int res = Read(byteBuffer, 0, 1);
            if (res > 0)
                return byteBuffer[0];
            return res;
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
            if (WritePosition == -1)
                Factory.Thread.SleepUntil(() => WritePosition > -1);
            readEvent.Wait();
            readEvent.Reset();
            int totalRead = 0;
            while (true)
            {
                var writePosition = WritePosition;
                var readPosition = ReadPosition;
                var availLength = writePosition - readPosition;
                var bufferLength = (_buffer.Length - 8);
                if (availLength < 0)
                    availLength = bufferLength + availLength;

                var rCount = Math.Min(count, availLength);
                if (rCount > 0)
                {
                    if (writePosition > readPosition)
                    {
                        Buffer.BlockCopy(_buffer, readPosition + 8, buffer, offset, rCount);
                        readPosition += rCount;
                        if (readPosition == bufferLength) readPosition = 0;
                        Buffer.BlockCopy(BitConverter.GetBytes(readPosition), 0, _buffer, 0, 4);
                    }
                    else
                    {
                        var availTilEnd = bufferLength - readPosition;
                        var rCountTilEnd = Math.Min(rCount, availTilEnd);

                        Buffer.BlockCopy(_buffer, readPosition + 8, buffer, offset, rCountTilEnd);
                        readPosition += rCountTilEnd;
                        if (readPosition == bufferLength) readPosition = 0;
                        Buffer.BlockCopy(BitConverter.GetBytes(readPosition), 0, _buffer, 0, 4);

                        var remain = rCount - rCountTilEnd;
                        if (remain > 0)
                        {
                            Buffer.BlockCopy(_buffer, 8, buffer, offset + rCountTilEnd, remain);
                            readPosition = remain;
                            if (readPosition == bufferLength) readPosition = 0;
                            Buffer.BlockCopy(BitConverter.GetBytes(readPosition), 0, _buffer, 0, 4);
                        }
                    }
                    count -= rCount;
                    totalRead += rCount;
                }
                if (count == 0)
                    break;
                else
                    writeEvent.Wait();
            }
            readEvent.Set();
            return totalRead;
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
            writeEvent.Wait();
            writeEvent.Reset();
            while (true)
            {
                var writePosition = WritePosition;
                if (writePosition == -1) writePosition = 0;
                var readPosition = ReadPosition;
                var availLength = readPosition - writePosition;
                var bufferLength = (_buffer.Length - 8);
                if (availLength <= 0)
                    availLength = bufferLength + availLength;

                var wCount = Math.Min(count, availLength);
                if (wCount > 0)
                {
                    if (readPosition > writePosition)
                    {
                        Buffer.BlockCopy(buffer, offset, _buffer, writePosition + 8, wCount);
                        writePosition += wCount;
                        if (writePosition == bufferLength) writePosition = 0;
                        Buffer.BlockCopy(BitConverter.GetBytes(writePosition), 0, _buffer, 4, 4);
                    }
                    else
                    {
                        var availTilEnd = bufferLength - writePosition;
                        var wCountTilEnd = Math.Min(wCount, availTilEnd);

                        Buffer.BlockCopy(buffer, offset, _buffer, writePosition + 8, wCountTilEnd);
                        writePosition += wCountTilEnd;
                        if (writePosition == bufferLength) writePosition = 0;
                        Buffer.BlockCopy(BitConverter.GetBytes(writePosition), 0, _buffer, 4, 4);

                        var remain = wCount - wCountTilEnd;
                        if (remain > 0)
                        {
                            Buffer.BlockCopy(buffer, offset + wCountTilEnd, _buffer, 8, remain);
                            writePosition = remain;
                            if (writePosition == bufferLength) writePosition = 0;
                            Buffer.BlockCopy(BitConverter.GetBytes(writePosition), 0, _buffer, 4, 4);
                        }
                    }
                    count -= wCount;
                }
                if (count == 0)
                    break;
                else
                    readEvent.Wait();
            }
            writeEvent.Set();
            if (firstTime)
                readEvent.Set();
        }
        #endregion
    }
}
