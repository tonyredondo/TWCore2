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
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static readonly ObjectPool<byte[], BytePoolAllocator> ByteArrayPool = new ObjectPool<byte[], BytePoolAllocator>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static readonly ObjectPool<List<byte[]>, ListBytePoolAllocator> ListByteArrayPool = new ObjectPool<List<byte[]>, ListBytePoolAllocator>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private const int MaxLength = 8192;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private List<byte[]> _buffers;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly bool _canWrite;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _isClosed;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _collectPoolItems = true;
        private MultiPosition _position;
        private MultiPosition _maxLength;

        #region Allocators
        private struct BytePoolAllocator : IPoolObjectLifecycle<byte[]>
        {
            public int InitialSize => 4;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            public int DropTimeFrequencyInSeconds => 120;
            public void DropAction(byte[] value) { }
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

        #region Nested Types
        private readonly struct MultiPosition
        {
            public readonly int Row;
            public readonly int Index;
            public readonly int GlobalIndex;

            #region .ctor
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public MultiPosition(int globalIndex)
            {
                Row = (globalIndex / MaxLength);
                Index = (globalIndex % MaxLength);
                GlobalIndex = globalIndex;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public MultiPosition(int row, int index)
            {
                Row = row;
                Index = index;
                GlobalIndex = (row * MaxLength) + index;
            }
            #endregion

            #region Public Methods
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public MultiPosition FromOffset(int offset)
            {
                return new MultiPosition(GlobalIndex + offset);
            }
            #endregion

            #region Override
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static MultiPosition operator +(MultiPosition a, MultiPosition b)
                => new MultiPosition(a.GlobalIndex + b.GlobalIndex);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static MultiPosition operator +(MultiPosition a, int offset)
                => new MultiPosition(a.GlobalIndex + offset);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static MultiPosition operator +(int offset, MultiPosition b)
                => new MultiPosition(offset + b.GlobalIndex);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static MultiPosition operator -(MultiPosition a, MultiPosition b)
                => new MultiPosition(a.GlobalIndex - b.GlobalIndex);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static MultiPosition operator -(MultiPosition a, int offset)
                => new MultiPosition(a.GlobalIndex - offset);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static MultiPosition operator -(int offset, MultiPosition b)
                => new MultiPosition(offset - b.GlobalIndex);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(MultiPosition a, MultiPosition b)
                => a.GlobalIndex == b.GlobalIndex;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(MultiPosition a, MultiPosition b)
                => a.GlobalIndex != b.GlobalIndex;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator <(MultiPosition a, MultiPosition b)
                => a.GlobalIndex < b.GlobalIndex;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator <=(MultiPosition a, MultiPosition b)
                => a.GlobalIndex <= b.GlobalIndex;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator >(MultiPosition a, MultiPosition b)
                => a.GlobalIndex > b.GlobalIndex;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator >=(MultiPosition a, MultiPosition b)
                => a.GlobalIndex >= b.GlobalIndex;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator MultiPosition(int a)
                => new MultiPosition(a);
            #endregion

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Equals(object obj)
            {
                if (obj is MultiPosition mPos)
                    return GlobalIndex == mPos.GlobalIndex;
                return false;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode()
            {
                return GlobalIndex.GetHashCode();
            }
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
        public override long Length => _maxLength.GlobalIndex;
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get => _position.GlobalIndex;
            set
            {
                if (value >= _maxLength.GlobalIndex)
                    _position = _maxLength;
                else if (value <= 0)
                    _position = 0;
                else
                    _position = (int)value;
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
            Close();
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
            var cPos = _position;
            if (cPos >= _maxLength) return -1;
            var res = _buffers[cPos.Row][cPos.Index];
            _position = _position + 1;
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
            var fromPos = _position;
            if (fromPos >= _maxLength) return -1;
            if (bLength > 1)
            {
                var toPos = _position + bLength;
                if (toPos > _maxLength)
                    toPos = _maxLength;

                if (fromPos.Row == toPos.Row)
                {
                    var source = _buffers[fromPos.Row].AsSpan(fromPos.Index, toPos.Index - fromPos.Index);
                    source.CopyTo(buffer);
                    _position += source.Length;
                    return source.Length;
                }

                int readLength = 0;
                for (var i = fromPos.Row; i <= toPos.Row; i++)
                {
                    Span<byte> source;

                    if (i == fromPos.Row)
                        source = _buffers[i].AsSpan(fromPos.Index, MaxLength - fromPos.Index);
                    else if (i == toPos.Row)
                        source = _buffers[i].AsSpan(0, toPos.Index);
                    else
                        source = _buffers[i].AsSpan();

                    source.CopyTo(buffer);
                    buffer = buffer.Slice(source.Length);
                    readLength += source.Length;
                }
                _position += readLength;
                return readLength;
            }
            buffer[0] = _buffers[fromPos.Row][fromPos.Index];
            _position = _position + 1;
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
            var cPos = _position;
            if (cPos.Row >= _buffers.Count)
                _buffers.Add(ByteArrayPool.New());
            _buffers[cPos.Row][cPos.Index] = value;
            _position = _position + 1;
            if (_position > _maxLength)
                _maxLength = _position;
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
            var cPos = _position;
            var fPos = _position + buffer.Length;
            var missRows = (fPos.Row + 1) - _buffers.Count;
            for (var i = 0; i < missRows; i++)
                _buffers.Add(ByteArrayPool.New());

            if (cPos.Row == fPos.Row)
            {
                var destination = _buffers[cPos.Row].AsSpan(cPos.Index, fPos.Index - cPos.Index);
                buffer.CopyTo(destination);
                buffer = buffer.Slice(destination.Length);
                if (buffer.Length > 0)
                    throw new IOException("Write error");
                _position = fPos;
                if (_position > _maxLength)
                    _maxLength = _position;
                return;
            }

            int writeLength = 0;
            for (var i = cPos.Row; i <= fPos.Row; i++)
            {
                Span<byte> destination;

                if (i == cPos.Row)
                    destination = _buffers[i].AsSpan(cPos.Index, MaxLength - cPos.Index);
                else if (i == fPos.Row)
                    destination = _buffers[i].AsSpan(0, fPos.Index);
                else
                    destination = _buffers[i].AsSpan();

                var bcopy = buffer.Slice(0, destination.Length);
                bcopy.CopyTo(destination);
                buffer = buffer.Slice(destination.Length);
                writeLength += destination.Length;
            }
            if (buffer.Length != 0)
                throw new IOException("Write error");
            _position = fPos;

            if (_position > _maxLength)
                _maxLength = _position;
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
            var mArray = new MultiArray<byte>(_buffers, (int)Position, (int)Length);
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
            var mArray = new MultiArray<byte>(_buffers, 0, (int)Length);
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
                Position = _maxLength.GlobalIndex + offset;
            }
            else
            {
                _position = _position + (int)offset;
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
            _maxLength = (int) value;
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
            => new MultiArray<byte>(_buffers, 0, (int)Length).ToArray();
        /// <summary>
        /// Get the internal buffer as MultiArray
        /// </summary>
        /// <returns>MultiArray instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray<byte> GetMultiArray()
        {
            _collectPoolItems = false;
            return new MultiArray<byte>(_buffers, 0, (int)Length);
        }
        #endregion
    }
}
