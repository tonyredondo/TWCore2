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

namespace TWCore.Serialization.WSerializer
{
    /// <summary>
    /// Definition for a type serializer
    /// </summary>
    public abstract class TypeSerializer
    {
        private readonly byte[] _buffer = new byte[9];

        /// <summary>
        /// Type serializer initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Init(SerializerMode mode);
        /// <summary>
        /// Gets if the type serializer can write the type
        /// </summary>
        /// <param name="type">Type of the value to write</param>
        /// <returns>true if the type serializer can write the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool CanWrite(Type type);
        /// <summary>
        /// Gets if the type serializer can read the data type
        /// </summary>
        /// <param name="type">DataType value</param>
        /// <returns>true if the type serializer can read the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public abstract bool CanRead(byte type);
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Write(BinaryWriter writer, object value);
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract object Read(BinaryReader reader, byte type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteByte(BinaryWriter bw, byte type, byte value)
        {
            _buffer[0] = type;
            _buffer[1] = value;
            bw.Write(_buffer, 0, 2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteUshort(BinaryWriter bw, byte type, ushort value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            bw.Write(_buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteInt(BinaryWriter bw, byte type, int value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value >> 16);
            _buffer[4] = (byte)(value >> 24);
            bw.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe void WriteDouble(BinaryWriter bw, byte type, double value)
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
            bw.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe void WriteFloat(BinaryWriter bw, byte type, float value)
        {
            var tmpValue = *(uint*)&value;
            _buffer[0] = type;
            _buffer[1] = (byte)tmpValue;
            _buffer[2] = (byte)(tmpValue >> 8);
            _buffer[3] = (byte)(tmpValue >> 16);
            _buffer[4] = (byte)(tmpValue >> 24);
            bw.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteLong(BinaryWriter bw, byte type, long value)
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
            bw.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteULong(BinaryWriter bw, byte type, ulong value)
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
            bw.Write(_buffer, 0, 9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteUInt(BinaryWriter bw, byte type, uint value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value >> 16);
            _buffer[4] = (byte)(value >> 24);
            bw.Write(_buffer, 0, 5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteShort(BinaryWriter bw, byte type, short value)
        {
            _buffer[0] = type;
            _buffer[1] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            bw.Write(_buffer, 0, 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void WriteChar(BinaryWriter bw, byte type, char value)
        {
            bw.Write(type);
            bw.Write(value);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Definition for a type serializer
    /// </summary>
    public abstract class TypeSerializer<T> : TypeSerializer
    {
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void WriteValue(BinaryWriter writer, T value);
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract T ReadValue(BinaryReader reader, byte type);
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract T ReadValue(BinaryReader reader);
    }
}
