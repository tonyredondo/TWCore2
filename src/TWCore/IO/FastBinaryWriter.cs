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
    /// <summary>
    /// Writes primitive data types as binary values in a specific encoding.
    /// </summary>
    public class FastBinaryWriter : BinaryWriter
    {
        byte[] _buffer = new byte[8];

        #region .ctor
        /// <summary>Initializes a new instance of the <see cref="T:TWCore.IO.FasterBinaryWriter" /> class based on the supplied stream and using <see cref="T:System.Text.UTF8Encoding" />.</summary>
        /// <param name="input">A stream. </param>
        /// <exception cref="T:System.ArgumentException">The stream does not support reading, the stream is null, or the stream is already closed. </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastBinaryWriter(Stream input) : base(input)
        {
        }
        /// <summary>Initializes a new instance of the <see cref="T:TWCore.IO.FasterBinaryWriter" /> class based on the supplied stream and a specific character encoding.</summary>
        /// <param name="input">The supplied stream. </param>
        /// <param name="encoding">The character encoding. </param>
        /// <exception cref="T:System.ArgumentException">The stream does not support reading, the stream is null, or the stream is already closed. </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="encoding" /> is null. </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastBinaryWriter(Stream input, Encoding encoding) : base(input, encoding)
        {
        }
        /// <summary>Initializes a new instance of the <see cref="T:TWCore.IO.FasterBinaryWriter" /> class based on the supplied stream and a specific character encoding.</summary>
        /// <param name="input">The supplied stream. </param>
        /// <param name="encoding">The character encoding. </param>
        /// <param name="leaveOpen">Leave the stream open. </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastBinaryWriter(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }
        #endregion

        /// <summary>Writes an eight-byte floating-point value to the current stream and advances the stream position by eight bytes.</summary>
        /// <param name="value">The eight-byte floating-point value to write. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <filterpriority>1</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe void Write(double value)
        {
            fixed (byte* b = _buffer)
                *((long*)b) = *(long*)&value;
            base.Write(_buffer, 0, 8);
        }
        /// <summary>Writes a four-byte floating-point value to the current stream and advances the stream position by four bytes.</summary>
        /// <param name="value">The four-byte floating-point value to write. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <filterpriority>1</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe void Write(float value)
        {
            fixed (byte* b = _buffer)
                *((uint*)b) = *(uint*)&value;
            base.Write(_buffer, 0, 4);
        }
        /// <summary>Writes a four-byte signed integer to the current stream and advances the stream position by four bytes.</summary>
        /// <param name="value">The four-byte signed integer to write. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <filterpriority>1</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe void Write(int value)
        {
            fixed (byte* b = _buffer)
                *((int*)b) = value;
            base.Write(_buffer, 0, 4);
        }
        /// <summary>Writes an eight-byte signed integer to the current stream and advances the stream position by eight bytes.</summary>
        /// <param name="value">The eight-byte signed integer to write. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <filterpriority>1</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe void Write(long value)
        {
            fixed (byte* b = _buffer)
                *((long*)b) = value;
            base.Write(_buffer, 0, 8);
        }
        /// <summary>Writes a two-byte signed integer to the current stream and advances the stream position by two bytes.</summary>
        /// <param name="value">The two-byte signed integer to write. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <filterpriority>1</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe void Write(short value)
        {
            fixed (byte* b = _buffer)
                *((short*)b) = value;
            base.Write(_buffer, 0, 2);
        }
        /// <summary>Writes a four-byte unsigned integer to the current stream and advances the stream position by four bytes.</summary>
        /// <param name="value">The four-byte unsigned integer to write. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <filterpriority>1</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe void Write(uint value)
        {
            fixed (byte* b = _buffer)
                *((uint*)b) = value;
            base.Write(_buffer, 0, 4);
        }
        /// <summary>Writes an eight-byte unsigned integer to the current stream and advances the stream position by eight bytes.</summary>
        /// <param name="value">The eight-byte unsigned integer to write. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <filterpriority>1</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe void Write(ulong value)
        {
            fixed (byte* b = _buffer)
                *((long*)b) = *(long*)&value;
            base.Write(_buffer, 0, 8);
        }
        /// <summary>Writes a two-byte unsigned integer to the current stream and advances the stream position by two bytes.</summary>
        /// <param name="value">The two-byte unsigned integer to write. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <filterpriority>1</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe void Write(ushort value)
        {
            fixed (byte* b = _buffer)
                *((short*)b) = *(short*)&value;
            base.Write(_buffer, 0, 2);
        }
    }
}