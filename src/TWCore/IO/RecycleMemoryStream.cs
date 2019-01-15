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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable IntroduceOptionalParameters.Global

namespace TWCore.IO
{
    /// <inheritdoc />
    /// <summary>
    /// Recycle Arrays MemoryStream
    /// </summary>
    public class RecycleMemoryStream : Stream
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal static readonly ObjectPool<byte[], BytePoolAllocator> ByteArrayPool = new ObjectPool<byte[], BytePoolAllocator>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static readonly ObjectPool<List<byte[]>, ListBytePoolAllocator> ListByteArrayPool = new ObjectPool<List<byte[]>, ListBytePoolAllocator>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] public const int MaxLength = 1024;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private List<byte[]> _buffers;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly bool _canWrite;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _isClosed;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _collectPoolItems = true;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _currentPosition;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _totalLength;

        #region Allocators
        internal readonly struct BytePoolAllocator : IPoolObjectLifecycle<byte[]>
        {
            public int InitialSize => 4;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            public int DropTimeFrequencyInSeconds => 120;
            public void DropAction(byte[] value) { }
            public byte[] New() => new byte[MaxLength];
            public void Reset(byte[] value) => Array.Clear(value, 0, MaxLength);
            public int DropMaxSizeThreshold => 10;
        }
        private readonly struct ListBytePoolAllocator : IPoolObjectLifecycle<List<byte[]>>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            public int DropTimeFrequencyInSeconds => 60;
            public void DropAction(List<byte[]> value) { }
            public List<byte[]> New() => new List<byte[]>(10);
            public void Reset(List<byte[]> value) => value.Clear();
            public int DropMaxSizeThreshold => 10;
        }
        #endregion

        #region Properties
        /// <inheritdoc />
        /// <summary>
        ///  Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead { get; } = true;
        /// <inheritdoc />
        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek { get; } = true;
        /// <inheritdoc />
        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => _canWrite;
        /// <inheritdoc />
        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length => _totalLength;
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get => _currentPosition;
            set
            {
                if (value >= _totalLength)
                    _currentPosition = _totalLength;
                else if (value <= 0)
                    _currentPosition = 0;
                else
                    _currentPosition = (int)value;
            }
        }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Recycle Arrays MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream() : this(null, 0, 0, true) { }
        /// <inheritdoc />
        /// <summary>
        /// Recycle Arrays MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream(byte[] buffer) : this(buffer, 0, buffer?.Length ?? 0, true) { }
        /// <inheritdoc />
        /// <summary>
        /// Recycle Arrays MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream(byte[] buffer, bool writable) : this(buffer, 0, buffer?.Length ?? 0, writable) { }
        /// <inheritdoc />
        /// <summary>
        /// Recycle Arrays MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream(byte[] buffer, int index, int count) : this(buffer, index, count, true) { }
        /// <inheritdoc />
        /// <summary>
        /// Recycle Arrays MemoryStream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RecycleMemoryStream(byte[] buffer, int index, int count, bool writable)
        {
            _buffers = ListByteArrayPool.New();
            _buffers.Add(ByteArrayPool.New());
            _canWrite = writable;
            if (buffer != null)
                Write(buffer.AsSpan(index, count));
        }
        /// <summary>
        /// Dispose all internal resources
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Dispose(bool disposing)
        {
            Close();
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Read
        /// <inheritdoc />
        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadByte()
        {
            if (_isClosed)
                throw new IOException("The stream is closed.");
            if (_currentPosition >= _totalLength) return -1;
            var row = Math.DivRem(_currentPosition, MaxLength, out var index);
            var res = _buffers[row][index];
            _currentPosition++;
            return res;
        }
        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">Buffer. When this method returns, the buffer contains the values between offset and (offset + count - 1).</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        /// <exception cref="IOException">Exception when the stream is closed.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if COMPATIBILITY
        public int Read(Span<byte> buffer)
#else
        public override int Read(Span<byte> buffer)
#endif
        {
            if (_isClosed)
                throw new IOException("The stream is closed.");
            var bLength = buffer.Length;
            if (bLength == 0) return 0;
            if (_currentPosition >= _totalLength) return 0;
            var fromRow = Math.DivRem(_currentPosition, MaxLength, out var fromIndex);
            if (bLength > 1)
            {
                var toPos = _currentPosition + bLength;
                if (toPos > _totalLength)
                    toPos = _totalLength;
                var toRow = Math.DivRem(toPos, MaxLength, out var toIndex);

                if (fromRow == toRow)
                {
                    var source = _buffers[fromRow].AsSpan(fromIndex, toIndex - fromIndex);
                    source.CopyTo(buffer);
                    _currentPosition += source.Length;
                    return source.Length;
                }

                int readLength = 0;
                for (var i = fromRow; i <= toRow; i++)
                {
                    Span<byte> source;

                    if (i == fromRow)
                        source = _buffers[i].AsSpan(fromIndex, MaxLength - fromIndex);
                    else if (i == toRow)
                        source = _buffers[i].AsSpan(0, toIndex);
                    else
                        source = _buffers[i].AsSpan();

                    source.CopyTo(buffer);
                    buffer = buffer.Slice(source.Length);
                    readLength += source.Length;
                }
                _currentPosition += readLength;
                return readLength;
            }
            buffer[0] = _buffers[fromRow][fromIndex];
            _currentPosition++;
            return 1;
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
            => Read(buffer.AsSpan(offset, count));
        /// <inheritdoc />
        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => Task.FromResult(Read(buffer.AsSpan(offset, count)));
        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">Buffer. When this method returns, the buffer contains the values between offset and (offset + count - 1).</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        /// <exception cref="IOException">Exception when the stream is closed.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if COMPATIBILITY
        public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
#else
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
#endif
            => new ValueTask<int>(Read(buffer.Span));
        #endregion

        #region Write
        /// <inheritdoc />
        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteByte(byte value)
        {
            if (_isClosed)
                throw new IOException("The stream is closed.");
            if (!_canWrite)
                throw new IOException("The stream is readonly.");
            var cRow = Math.DivRem(_currentPosition, MaxLength, out var cIndex);
            if (cRow >= _buffers.Count)
                _buffers.Add(ByteArrayPool.New());
            _buffers[cRow][cIndex] = value;
            _currentPosition++;
            if (_currentPosition > _totalLength)
                _totalLength = _currentPosition;
        }
        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">A buffer to write the values</param>
        /// <exception cref="IOException">Exception when the stream is closed or is readonly.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if COMPATIBILITY
        public void Write(ReadOnlySpan<byte> buffer)
#else
        public override void Write(ReadOnlySpan<byte> buffer)
#endif
        {
            if (_isClosed)
                throw new IOException("The stream is closed.");
            if (!_canWrite)
                throw new IOException("The stream is readonly.");
            var finalPosition = _currentPosition + buffer.Length;
            var cRow = Math.DivRem(_currentPosition, MaxLength, out var cIndex);
            var fRow = Math.DivRem(finalPosition, MaxLength, out var fIndex);
            var missRows = (fRow + 1) - _buffers.Count;
            for (var i = 0; i < missRows; i++)
                _buffers.Add(ByteArrayPool.New());
            if (cRow == fRow)
            {
                var destination = _buffers[cRow].AsSpan(cIndex, fIndex - cIndex);
                buffer.CopyTo(destination);
                buffer = buffer.Slice(destination.Length);
                goto final;
            }

            int writeLength = 0;
            for (var i = cRow; i <= fRow; i++)
            {
                Span<byte> destination;

                if (i == cRow)
                    destination = _buffers[i].AsSpan(cIndex, MaxLength - cIndex);
                else if (i == fRow)
                    destination = _buffers[i].AsSpan(0, fIndex);
                else
                    destination = _buffers[i].AsSpan();

                var bcopy = buffer.Slice(0, destination.Length);
                bcopy.CopyTo(destination);
                buffer = buffer.Slice(destination.Length);
                writeLength += destination.Length;
            }

            final:
            if (buffer.Length != 0)
                throw new IOException("Write error");
            _currentPosition = finalPosition;
            if (_currentPosition > _totalLength)
                _totalLength = _currentPosition;
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
            => Write(buffer.AsSpan(offset, count));
        /// <inheritdoc />
        /// <summary>
        ///  When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            Write(buffer.AsSpan(offset, count));
            return Task.CompletedTask;
        }
        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">A buffer to write the values</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="IOException">Exception when the stream is closed or is readonly.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if COMPATIBILITY
        public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
#else
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
#endif
        {
            Write(buffer.Span);
            return new ValueTask();
        }
        #endregion

        #region Flush
        /// <inheritdoc />
        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Flush()
        {
        }
        /// <inheritdoc />
        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        #endregion

        #region CopyTo
        /// <inheritdoc />
        /// <summary>
        /// Copy stream data to other stream
        /// </summary>
        /// <param name="destination">Destination stream</param>
        /// <param name="bufferSize">Buffer size. (not used)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if COMPATIBILITY
        public new void CopyTo(Stream destination, int bufferSize)
#else
        public override void CopyTo(Stream destination, int bufferSize)
#endif
        {
            var mArray = new MultiArray<byte>(_buffers, _currentPosition, _totalLength - _currentPosition);
            mArray.CopyTo(destination);
        }
        /// <inheritdoc />
        /// <summary>
        /// Copy stream data to other stream
        /// </summary>
        /// <param name="destination">Destination stream</param>
        /// <param name="bufferSize">Buffer size. (not used)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            var mArray = new MultiArray<byte>(_buffers, _currentPosition, _totalLength - _currentPosition);
            return mArray.CopyToAsync(destination);
        }
        #endregion

        #region Other Methods
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
            if (origin == SeekOrigin.Begin)
            {
                Position = offset;
            }
            else if (origin == SeekOrigin.End)
            {
                Position = _totalLength + offset;
            }
            else
            {
                _currentPosition += (int)offset;
            }
            return Position;
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetLength(long value)
        {
            if (value < 0)
                value = 0;
            _totalLength = (int)value;
        }
        /// <summary>
        /// Close stream
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Close()
        {
            if (_buffers != null)
            {
                if (_collectPoolItems)
                {
                    foreach (var array in _buffers)
                        ByteArrayPool.Store(array);
                    ListByteArrayPool.Store(_buffers);
                }
                _buffers = null;
                _isClosed = true;
            }
        }
        /// <summary>
        /// Writes the stream contents to a byte array, regardless of the Position property
        /// </summary>
        /// <returns>A new byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ToArray()
            => new MultiArray<byte>(_buffers, 0, _totalLength).ToArray();
        /// <summary>
        /// Get the internal buffer as MultiArray
        /// </summary>
        /// <returns>MultiArray instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray<byte> GetMultiArray()
        {
            _collectPoolItems = false;
            return new MultiArray<byte>(_buffers, 0, _totalLength);
        }
        /// <summary>
        /// Get the internal buffer as MultiArray
        /// </summary>
        /// <returns>MultiArray instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _currentPosition = 0;
            _totalLength = 0;
            if (_collectPoolItems) return;
            _buffers = ListByteArrayPool.New();
            _buffers.Add(ByteArrayPool.New());
            _collectPoolItems = true;
        }
        #endregion
    }
}
