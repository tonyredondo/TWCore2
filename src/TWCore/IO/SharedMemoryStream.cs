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
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TWCore.IO
{
    /// <inheritdoc />
    /// <summary>
    /// Shared Memory circular stream
    /// </summary>
#if NET5_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    public sealed class SharedMemoryStream : Stream
    {
        #region Fields
        private const int Start = 9;
        private readonly int _length;
        private readonly MemoryMappedViewAccessor _view;
        private readonly EventWaitHandle _readEvent;
        private readonly EventWaitHandle _writeEvent;
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
        /// <param name="name">Name of the shared memory</param>
        /// <param name="bufferSize">Buffer size, default size 262144 bytes</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SharedMemoryStream(string name, int bufferSize = 262144)
        {
            _length = bufferSize;
			var mmfBuffer = MemoryMappedFile.CreateOrOpen(name, _length + Start);
			//var mmfBuffer = MemoryMappedFile.CreateFromFile(name, FileMode.OpenOrCreate, null, _length + Start, MemoryMappedFileAccess.ReadWrite);
            _view = mmfBuffer.CreateViewAccessor();
            var readEventName = "Global\\CoreStream." + name + ".Read";
            var writeEventName = "Global\\CoreStream." + name + ".Write";
            if (!EventWaitHandle.TryOpenExisting(readEventName, out _readEvent))
                _readEvent = new EventWaitHandle(false, EventResetMode.ManualReset, readEventName, out bool _);
            if (!EventWaitHandle.TryOpenExisting(writeEventName, out _writeEvent))
                _writeEvent = new EventWaitHandle(false, EventResetMode.ManualReset, writeEventName, out bool _);
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
            var rPos = GetReadPosition();
            var wPos = GetWritePosition();
            while (GetReadFirst() == 0 && rPos == wPos)
            {
                _writeEvent.WaitOne();
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
                totalRead += _view.ReadArray(rPos + Start, buffer, offset, count);
                rPos += count;
                SetReadPosition(rPos == _length ? 0 : rPos);
            }
            else
            {
                var remain = count - bufRemain;
                totalRead += _view.ReadArray(rPos + Start, buffer, offset, bufRemain);
                rPos += bufRemain;
                if (rPos == wPos)
                {
                    SetReadPosition(rPos);
                }
                else
                {
                    var canRead = Math.Min(remain, wPos);
                    totalRead += _view.ReadArray(Start, buffer, offset + bufRemain, canRead);
                    SetReadPosition(canRead < remain ? canRead : remain);
                }
            }
            _readEvent.Set();
            return totalRead;
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
            while (true)
            {
                var rPos = GetReadPosition();
                var wPos = GetWritePosition();
                while (GetReadFirst() == 1 && rPos == wPos)
                {
                    _readEvent.WaitOne();
                    _readEvent.Reset();
                    rPos = GetReadPosition();
                }

                var cLength = rPos <= wPos ? _length : rPos;
                var bufRemain = cLength - wPos;
                if (bufRemain >= count)
                {
                    _view.WriteArray(wPos + Start, buffer, offset, count);
                    wPos += count;
                    SetWritePosition(wPos == _length ? 0 : wPos);
                    if (rPos == wPos)
                        SetReadFirst(1);
                    _writeEvent.Set();
                }
                else
                {
                    var remain = count - bufRemain;
                    _view.WriteArray(wPos + Start, buffer, offset, bufRemain);
                    wPos += bufRemain;
                    if (wPos == rPos)
                    {
                        SetWritePosition(wPos);
                        SetReadFirst(1);
                        _writeEvent.Set();
                        offset = offset + bufRemain;
                        count = remain;
                        continue;
                    }
                    
                    var canWrite = Math.Min(remain, rPos);
                    _view.WriteArray(Start, buffer, offset + bufRemain, canWrite);
                    if (canWrite < remain)
                    {
                        SetWritePosition(canWrite);
                        remain = remain - canWrite;
                        SetReadFirst(1);
                        _writeEvent.Set();
                        offset = offset + bufRemain + canWrite;
                        count = remain;
                        continue;
                    }
                    
                    if (rPos == remain)
                        SetReadFirst(1);
                    SetWritePosition(remain);
                    _writeEvent.Set();
                }
                break;
            }
        }

        #endregion


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetReadPosition() => _view.ReadInt32(0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetReadPosition(int value) => _view.Write(0, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWritePosition() => _view.ReadInt32(4);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetWritePosition(int value) => _view.Write(4, value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte GetReadFirst() => _view.ReadByte(8);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetReadFirst(byte value) => _view.Write(8, value);
    }
}