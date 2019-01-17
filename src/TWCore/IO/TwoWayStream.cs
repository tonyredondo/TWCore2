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
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global

namespace TWCore.IO
{
    /// <inheritdoc />
    /// <summary>
    /// Two Way Stream decorator
    /// </summary>
    public sealed class TwoWayStream : Stream
    {
        private readonly Stream _readStream;
        private readonly Stream _writeStream;

        #region Properties
        /// <inheritdoc />
        /// <summary>
        ///  Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => _readStream.CanRead;
        /// <inheritdoc />
        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => false;
        /// <inheritdoc />
        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => _writeStream.CanWrite;
        /// <inheritdoc />
        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                throw new NotSupportedException();
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                throw new NotSupportedException();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                throw new NotSupportedException();
            }
        }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Two Way Stream decorator
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TwoWayStream(Stream readStream, Stream writeStream)
        {
            _readStream = readStream;
            _writeStream = writeStream;
        }
        #endregion

        #region Methods
        /// <inheritdoc />
        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override void Flush()
            => _writeStream.Flush();
        /// <inheritdoc />
        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override int ReadByte()
            => _readStream.ReadByte();
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
            => _readStream.Read(buffer, offset, count);
        /// <inheritdoc />
        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => _readStream.ReadAsync(buffer, offset, count, cancellationToken);
        /// <inheritdoc />
        /// <summary>
        ///  When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override void Write(byte[] buffer, int offset, int count)
            => _writeStream.Write(buffer, offset, count);
        /// <inheritdoc />
        /// <summary>
        ///  When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => _writeStream.WriteAsync(buffer, offset, count, cancellationToken);
        /// <inheritdoc />
        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        /// <inheritdoc />
        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override void SetLength(long value) => throw new NotSupportedException();
        /// <summary>
        /// Dispose all resources of the instance
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _readStream?.Dispose();
                _writeStream?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
