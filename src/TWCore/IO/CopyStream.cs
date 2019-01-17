﻿/*
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
using System.Threading.Tasks;
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace TWCore.IO
{
    /// <inheritdoc />
    /// <summary>
    /// Stream decorator for copy the writing in another stream
    /// </summary>
    public sealed class CopyStream : Stream
    {
        private Stream _baseStream;
        private Stream _copyingStream;

        #region Properties
        /// <inheritdoc />
        /// <summary>
        ///  Gets a value indicating whether the current stream supports reading.
        /// </summary>
		public override bool CanRead => _baseStream.CanRead;
        /// <inheritdoc />
        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
		public override bool CanSeek => _baseStream.CanSeek;
        /// <inheritdoc />
        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
		public override bool CanWrite => _baseStream.CanWrite;
        /// <inheritdoc />
        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
		public override long Length => _baseStream.Length;
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _baseStream.Position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _baseStream.Position = value;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets a value that determines whether the current stream can time out.
        /// </summary>
		public override bool CanTimeout => _baseStream.CanTimeout;
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets a value, in miliseconds, that determines how long the stream will attempt to read before timing out.
        /// </summary>
        public override int ReadTimeout
        {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _baseStream.ReadTimeout;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _baseStream.ReadTimeout = value;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets a value, in miliseconds, that determines how long the stream will attempt to write before timing out.
        /// </summary>
        public override int WriteTimeout
        {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _baseStream.WriteTimeout;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _baseStream.WriteTimeout = value;
        }
        /// <summary>
        /// Stream base object instance
        /// </summary>
		public Stream BaseStream
        {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _baseStream;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _baseStream = value;
        }
        /// <summary>
        /// Copy stream object instance
        /// </summary>
		public Stream CopyingStream
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _copyingStream;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _copyingStream = value;
        }
        #endregion

        #region .ctor
	    /// <inheritdoc />
	    /// <summary>
	    /// Stream decorator for copy the writing in another stream
	    /// </summary>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	    public CopyStream()
	    {
	    }
        /// <inheritdoc />
        /// <summary>
        /// Stream decorator for copy the writing in another stream
        /// </summary>
        /// <param name="baseStream">Stream base to decorate</param>
        /// <param name="copyingStream">Stream were data is gonna be copied</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CopyStream(Stream baseStream, Stream copyingStream)
        {
			_baseStream = baseStream;
            _copyingStream = copyingStream;
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override void Flush()
        {
            _baseStream.Flush();
            _copyingStream.Flush();
        }
        /// <inheritdoc />
        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public sealed override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);
        /// <inheritdoc />
        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public sealed override void SetLength(long value) => _baseStream.SetLength(value);
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
			return _baseStream.Read(buffer, offset, count);
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
			_baseStream.Write(buffer, offset, count);
            _copyingStream.Write(buffer, offset, count);
        }
        /// <inheritdoc />
        /// <summary>
        /// Asynchronously reads the bytes from the current stream and writes them to another stream, using a specified buffer size and cancellation token.
        /// </summary>
        /// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
        /// <param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero. The default size is 4096.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>A task that represents the asynchronous copy operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public sealed override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) 
		=> _baseStream.CopyToAsync(destination, bufferSize, cancellationToken);
        /// <inheritdoc />
        /// <summary>
        /// Releases the unmanaged resources used by the System.IO.Stream and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected sealed override void Dispose(bool disposing) => _baseStream.Dispose();
        /// <inheritdoc />
        /// <summary>
        /// Asynchronously clears all buffers for this stream, causes any buffered data to be written to the underlying device, and monitors cancellation requests.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAll(
                _baseStream.FlushAsync(cancellationToken),
                _copyingStream.FlushAsync(cancellationToken)
                ).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter contains the total number of bytes read into the buffer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public sealed override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _baseStream.ReadAsync(buffer, offset, count, cancellationToken);
        /// <inheritdoc />
        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public sealed override int ReadByte() => _baseStream.ReadByte();
        /// <inheritdoc />
        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in buffer from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await Task.WhenAll(
                _baseStream.WriteAsync(buffer, offset, count, cancellationToken),
                _copyingStream.WriteAsync(buffer, offset, count, cancellationToken)
                ).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override void WriteByte(byte value)
        {
            _baseStream.WriteByte(value);
            _copyingStream.WriteByte(value);
        }
        #endregion
    }
}
