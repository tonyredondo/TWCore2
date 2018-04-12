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

namespace TWCore.Serialization.NSerializer.Types
{
    public abstract class TypeSerializer
    {
        protected Stream _stream;
        private byte[] _buffer = new byte[9];

        /// <summary>
        /// Type serializer initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Init();
        /// <summary>
        /// Clear serializer cache
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte type, byte value)
        {
            _buffer[0] = type;
            _buffer[1] = value;
            _stream.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUshort(byte type, ushort value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _stream.Write(_buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt(byte type, int value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value >> 16);
            _buffer[4] = (byte)(value >> 24);
            _stream.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteDouble(byte type, double value)
        {
            var tmpValue = *(ulong*)&value;
            _buffer[0] = type;
            _buffer[1] = (byte)tmpValue;
            _buffer[2] = (byte)(tmpValue >> 8);
            _buffer[3] = (byte)(tmpValue >> 16);
            _buffer[4] = (byte)(tmpValue >> 24);
            _buffer[5] = (byte)(tmpValue >> 32);
            _buffer[6] = (byte)(tmpValue >> 40);
            _buffer[7] = (byte)(tmpValue >> 48);
            _buffer[8] = (byte)(tmpValue >> 56);
            _stream.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteFloat(byte type, float value)
        {
            var tmpValue = *(uint*)&value;
            _buffer[0] = type;
            _buffer[1] = (byte)tmpValue;
            _buffer[2] = (byte)(tmpValue >> 8);
            _buffer[3] = (byte)(tmpValue >> 16);
            _buffer[4] = (byte)(tmpValue >> 24);
            _stream.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLong(byte type, long value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value >> 16);
            _buffer[4] = (byte)(value >> 24);
            _buffer[5] = (byte)(value >> 32);
            _buffer[6] = (byte)(value >> 40);
            _buffer[7] = (byte)(value >> 48);
            _buffer[8] = (byte)(value >> 56);
            _stream.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteULong(byte type, ulong value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value >> 16);
            _buffer[4] = (byte)(value >> 24);
            _buffer[5] = (byte)(value >> 32);
            _buffer[6] = (byte)(value >> 40);
            _buffer[7] = (byte)(value >> 48);
            _buffer[8] = (byte)(value >> 56);
            _stream.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt(byte type, uint value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value >> 16);
            _buffer[4] = (byte)(value >> 24);
            _stream.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShort(byte type, short value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _stream.Write(_buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChar(byte type, char value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _stream.Write(_buffer, 0, 3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUshort(ushort value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _stream.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt(int value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _stream.Write(_buffer, 0, 4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteDouble(double value)
        {
            var tmpValue = *(ulong*)&value;
            _buffer[0] = (byte)tmpValue;
            _buffer[1] = (byte)(tmpValue >> 8);
            _buffer[2] = (byte)(tmpValue >> 16);
            _buffer[3] = (byte)(tmpValue >> 24);
            _buffer[4] = (byte)(tmpValue >> 32);
            _buffer[5] = (byte)(tmpValue >> 40);
            _buffer[6] = (byte)(tmpValue >> 48);
            _buffer[7] = (byte)(tmpValue >> 56);
            _stream.Write(_buffer, 0, 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteFloat(float value)
        {
            var tmpValue = *(uint*)&value;
            _buffer[0] = (byte)tmpValue;
            _buffer[1] = (byte)(tmpValue >> 8);
            _buffer[2] = (byte)(tmpValue >> 16);
            _buffer[3] = (byte)(tmpValue >> 24);
            _stream.Write(_buffer, 0, 4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLong(long value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _buffer[4] = (byte)(value >> 32);
            _buffer[5] = (byte)(value >> 40);
            _buffer[6] = (byte)(value >> 48);
            _buffer[7] = (byte)(value >> 56);
            _stream.Write(_buffer, 0, 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteULong(ulong value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _buffer[4] = (byte)(value >> 32);
            _buffer[5] = (byte)(value >> 40);
            _buffer[6] = (byte)(value >> 48);
            _buffer[7] = (byte)(value >> 56);
            _stream.Write(_buffer, 0, 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt(uint value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _buffer[2] = (byte)(value >> 16);
            _buffer[3] = (byte)(value >> 24);
            _stream.Write(_buffer, 0, 4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShort(short value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _stream.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChar(char value)
        {
            _buffer[0] = (byte)value;
            _buffer[1] = (byte)(value >> 8);
            _stream.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteDecimal(decimal value)
        {
            var val = decimal.ToDouble(value);
            var tmpValue = *(ulong*)&val;
            _buffer[0] = (byte)tmpValue;
            _buffer[1] = (byte)(tmpValue >> 8);
            _buffer[2] = (byte)(tmpValue >> 16);
            _buffer[3] = (byte)(tmpValue >> 24);
            _buffer[4] = (byte)(tmpValue >> 32);
            _buffer[5] = (byte)(tmpValue >> 40);
            _buffer[6] = (byte)(tmpValue >> 48);
            _buffer[7] = (byte)(tmpValue >> 56);
            _stream.Write(_buffer, 0, 8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSByte(sbyte value)
        {
            var bytes = BitConverter.GetBytes(value);
            _stream.Write(bytes, 0, bytes.Length);
        }
    }
}