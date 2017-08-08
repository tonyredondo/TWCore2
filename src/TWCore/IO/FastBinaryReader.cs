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
    /// Reads primitive data types as binary values in a specific encoding.
    /// </summary>
	[System.Diagnostics.DebuggerNonUserCode]
    public class FastBinaryReader : BinaryReader
    {
        private bool m_isMemoryStream;

        #region .ctor
        /// <summary>Initializes a new instance of the <see cref="T:TWCore.IO.FasterBinaryReader" /> class based on the supplied stream and using <see cref="T:System.Text.UTF8Encoding" />.</summary>
        /// <param name="input">A stream. </param>
        /// <exception cref="T:System.ArgumentException">The stream does not support reading, the stream is null, or the stream is already closed. </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastBinaryReader(Stream input) : base(input)
        {
            m_isMemoryStream = (input.GetType() == typeof(MemoryStream));
        }
        /// <summary>Initializes a new instance of the <see cref="T:TWCore.IO.FasterBinaryReader" /> class based on the supplied stream and a specific character encoding.</summary>
        /// <param name="input">The supplied stream. </param>
        /// <param name="encoding">The character encoding. </param>
        /// <exception cref="T:System.ArgumentException">The stream does not support reading, the stream is null, or the stream is already closed. </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="encoding" /> is null. </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
            m_isMemoryStream = (input.GetType() == typeof(MemoryStream));
        }
        /// <summary>Initializes a new instance of the <see cref="T:TWCore.IO.FasterBinaryReader" /> class based on the supplied stream and a specific character encoding.</summary>
        /// <param name="input">The supplied stream. </param>
        /// <param name="encoding">The character encoding. </param>
        /// <param name="leaveOpen">Leave the stream open. </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
            m_isMemoryStream = (input.GetType() == typeof(MemoryStream));
        }
        #endregion

        /// <summary>Reads a decimal value from the current stream and advances the current position of the stream by sixteen bytes.</summary>
        /// <returns>A decimal value read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe decimal ReadDecimal()
        {
            const int SignMask = unchecked((int)0x80000000);
            const int ScaleMask = 0x00FF0000;

            var m_buffer = ReadBytes(16);
            fixed (byte* ptr = m_buffer)
            {
                int* pBuffer = (int*)ptr;
                int lo = pBuffer[0];
                int mid = pBuffer[1];
                int hi = pBuffer[2];
                int flags = pBuffer[3];
     
                // This logic mirrors the code in Decimal(int[]) ctor.
                if (!((flags & ~(SignMask | ScaleMask)) == 0 && (flags & ScaleMask) <= (28 << 16)))
                {
                    // Invalid decimal
                    throw new IOException("Invalid Decimal (Arg_DecBitCtor)");
                }
                bool isNegative = (flags & SignMask) != 0;
                byte scale = (byte)(flags >> 16);
                return new decimal(lo, mid, hi, isNegative, scale);
            }
        }
        /// <summary>Reads an 8-byte floating point value from the current stream and advances the current position of the stream by eight bytes.</summary>
        /// <returns>An 8-byte floating point value read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe double ReadDouble()
        {
            var m_buffer = ReadBytes(8);
            fixed (byte* ptr = m_buffer)
            {
                uint* pBuffer = (uint*)ptr;
                uint lo = pBuffer[0];
                uint hi = pBuffer[1];
                ulong tmpBuffer = ((ulong)hi) << 32 | lo;
                return *((double*)&tmpBuffer);
            }
        }
        /// <summary>Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two bytes.</summary>
        /// <returns>A 2-byte signed integer read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe short ReadInt16()
        {
            var m_buffer = ReadBytes(2);
            fixed (byte* ptr = m_buffer)
                return *(short*)ptr;
        }
        /// <summary>Reads a 4-byte signed integer from the current stream and advances the current position of the stream by four bytes.</summary>
        /// <returns>A 4-byte signed integer read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe int ReadInt32()
        {
            if (m_isMemoryStream)
                return base.ReadInt32();
            var m_buffer = ReadBytes(4);
            fixed (byte* ptr = m_buffer)
                return *(int*)ptr;
        }
        /// <summary>Reads an 8-byte signed integer from the current stream and advances the current position of the stream by eight bytes.</summary>
        /// <returns>An 8-byte signed integer read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe long ReadInt64()
        {
            var m_buffer = ReadBytes(8);
            fixed (byte* ptr = m_buffer)
            {
                uint* pBuffer = (uint*)ptr;
                uint lo = pBuffer[0];
                uint hi = pBuffer[1];
                return (long)((ulong)hi) << 32 | lo;
            }
        }
        /// <summary>Reads a 4-byte floating point value from the current stream and advances the current position of the stream by four bytes.</summary>
        /// <returns>A 4-byte floating point value read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe float ReadSingle()
        {
            var m_buffer = ReadBytes(4);
            fixed (byte* ptr = m_buffer)
                return *(float*)ptr;
        }
        /// <summary>Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the position of the stream by two bytes.</summary>
        /// <returns>A 2-byte unsigned integer read from this stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe ushort ReadUInt16()
        {
            var m_buffer = ReadBytes(2);
            fixed (byte* ptr = m_buffer)
                return *(ushort*)ptr;
        }
        /// <summary>Reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.</summary>
        /// <returns>A 4-byte unsigned integer read from this stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe uint ReadUInt32()
        {
            if (m_isMemoryStream)
                return base.ReadUInt32();
            var m_buffer = ReadBytes(4);
            fixed (byte* ptr = m_buffer)
                return *(uint*)ptr;
        }
        /// <summary>Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight bytes.</summary>
        /// <returns>An 8-byte unsigned integer read from this stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
        /// <filterpriority>2</filterpriority>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe ulong ReadUInt64()
        {
            var m_buffer = ReadBytes(8);
            fixed (byte* ptr = m_buffer)
            {
                uint* pBuffer = (uint*)ptr;
                uint lo = pBuffer[0];
                uint hi = pBuffer[1];
                return ((ulong)hi) << 32 | lo;
            }
        }
    }
}