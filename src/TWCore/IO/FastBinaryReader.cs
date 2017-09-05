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

using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace TWCore.IO
{
    /// <inheritdoc />
    /// <summary>
    /// Reads primitive data types as binary values in a specific encoding.
    /// </summary>
    public class FastBinaryReader : BinaryReader
    {
        private readonly bool _isMemoryStream;

        #region .ctor
        /// <inheritdoc />
        /// <summary>Initializes a new instance of the <see cref="T:TWCore.IO.FasterBinaryReader" /> class based on the supplied stream and using <see cref="T:System.Text.UTF8Encoding" />.</summary>
        /// <param name="input">A stream. </param>
        /// <exception cref="T:System.ArgumentException">The stream does not support reading, the stream is null, or the stream is already closed. </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastBinaryReader(Stream input) : base(input)
        {
            _isMemoryStream = (input.GetType() == typeof(MemoryStream));
        }
        /// <inheritdoc />
        /// <summary>Initializes a new instance of the <see cref="T:TWCore.IO.FasterBinaryReader" /> class based on the supplied stream and a specific character encoding.</summary>
        /// <param name="input">The supplied stream. </param>
        /// <param name="encoding">The character encoding. </param>
        /// <exception cref="T:System.ArgumentException">The stream does not support reading, the stream is null, or the stream is already closed. </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="encoding" /> is null. </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
            _isMemoryStream = (input.GetType() == typeof(MemoryStream));
        }
        /// <inheritdoc />
        /// <summary>Initializes a new instance of the <see cref="T:TWCore.IO.FasterBinaryReader" /> class based on the supplied stream and a specific character encoding.</summary>
        /// <param name="input">The supplied stream. </param>
        /// <param name="encoding">The character encoding. </param>
        /// <param name="leaveOpen">Leave the stream open. </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
            _isMemoryStream = (input.GetType() == typeof(MemoryStream));
        }
        #endregion

        /// <inheritdoc />
        /// <summary>Reads a decimal value from the current stream and advances the current position of the stream by sixteen bytes.</summary>
        /// <returns>A decimal value read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe decimal ReadDecimal()
        {
            const int signMask = unchecked((int)0x80000000);
            const int scaleMask = 0x00FF0000;

            var mBuffer = ReadBytes(16);
            fixed (byte* ptr = mBuffer)
            {
                var pBuffer = (int*)ptr;
                var lo = pBuffer[0];
                var mid = pBuffer[1];
                var hi = pBuffer[2];
                var flags = pBuffer[3];
     
                // This logic mirrors the code in Decimal(int[]) ctor.
                if (!((flags & ~(signMask | scaleMask)) == 0 && (flags & scaleMask) <= (28 << 16)))
                {
                    // Invalid decimal
                    throw new IOException("Invalid Decimal (Arg_DecBitCtor)");
                }
                var isNegative = (flags & signMask) != 0;
                var scale = (byte)(flags >> 16);
                return new decimal(lo, mid, hi, isNegative, scale);
            }
        }
        /// <inheritdoc />
        /// <summary>Reads an 8-byte floating point value from the current stream and advances the current position of the stream by eight bytes.</summary>
        /// <returns>An 8-byte floating point value read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe double ReadDouble()
        {
            var mBuffer = ReadBytes(8);
            fixed (byte* ptr = mBuffer)
            {
                var pBuffer = (uint*)ptr;
                var lo = pBuffer[0];
                var hi = pBuffer[1];
                var tmpBuffer = ((ulong)hi) << 32 | lo;
                return *((double*)&tmpBuffer);
            }
        }
        /// <inheritdoc />
        /// <summary>Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two bytes.</summary>
        /// <returns>A 2-byte signed integer read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe short ReadInt16()
        {
            var mBuffer = ReadBytes(2);
            fixed (byte* ptr = mBuffer)
                return *(short*)ptr;
        }
        /// <inheritdoc />
        /// <summary>Reads a 4-byte signed integer from the current stream and advances the current position of the stream by four bytes.</summary>
        /// <returns>A 4-byte signed integer read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe int ReadInt32()
        {
            if (_isMemoryStream)
                return base.ReadInt32();
            var mBuffer = ReadBytes(4);
            fixed (byte* ptr = mBuffer)
                return *(int*)ptr;
        }
        /// <inheritdoc />
        /// <summary>Reads an 8-byte signed integer from the current stream and advances the current position of the stream by eight bytes.</summary>
        /// <returns>An 8-byte signed integer read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe long ReadInt64()
        {
            var mBuffer = ReadBytes(8);
            fixed (byte* ptr = mBuffer)
            {
                var pBuffer = (uint*)ptr;
                var lo = pBuffer[0];
                var hi = pBuffer[1];
                return (long)hi << 32 | lo;
            }
        }
        /// <inheritdoc />
        /// <summary>Reads a 4-byte floating point value from the current stream and advances the current position of the stream by four bytes.</summary>
        /// <returns>A 4-byte floating point value read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe float ReadSingle()
        {
            var mBuffer = ReadBytes(4);
            fixed (byte* ptr = mBuffer)
                return *(float*)ptr;
        }
        /// <inheritdoc />
        /// <summary>Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the position of the stream by two bytes.</summary>
        /// <returns>A 2-byte unsigned integer read from this stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe ushort ReadUInt16()
        {
            var mBuffer = ReadBytes(2);
            fixed (byte* ptr = mBuffer)
                return *(ushort*)ptr;
        }
        /// <inheritdoc />
        /// <summary>Reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.</summary>
        /// <returns>A 4-byte unsigned integer read from this stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe uint ReadUInt32()
        {
            if (_isMemoryStream)
                return base.ReadUInt32();
            var mBuffer = ReadBytes(4);
            fixed (byte* ptr = mBuffer)
                return *(uint*)ptr;
        }
        /// <inheritdoc />
        /// <summary>Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight bytes.</summary>
        /// <returns>An 8-byte unsigned integer read from this stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe ulong ReadUInt64()
        {
            var mBuffer = ReadBytes(8);
            fixed (byte* ptr = mBuffer)
            {
                var pBuffer = (uint*)ptr;
                var lo = pBuffer[0];
                var hi = pBuffer[1];
                return ((ulong)hi) << 32 | lo;
            }
        }
    }
}