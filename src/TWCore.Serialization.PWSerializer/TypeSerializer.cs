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

using System;
using System.Runtime.CompilerServices;
using TWCore.IO;

namespace TWCore.Serialization.PWSerializer
{
    /// <summary>
    /// Definition for a type serializer
    /// </summary>
    public abstract class TypeSerializer
    {
		byte[] _buffer = new byte[9];

        /// <summary>
        /// Type serializer initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Init(SerializerMode mode) { }
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
        public abstract void Write(FastBinaryWriter writer, object value);
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract object Read(FastBinaryReader reader, byte type);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void WriteByte(FastBinaryWriter bw, byte type, byte value)
		{
			_buffer[0] = type;
			_buffer[1] = value;
			bw.Write(_buffer, 0, 2);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void WriteUshort(FastBinaryWriter bw, byte type, ushort value)
		{
			_buffer[0] = type;
			fixed (byte* b = &_buffer[1])
				*((ushort*)b) = value;
            bw.Write(_buffer, 0, 3);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void WriteInt(FastBinaryWriter bw, byte type, int value)
		{
			_buffer[0] = type;
			fixed (byte* b = &_buffer[1])
				*((int*)b) = value;
            bw.Write(_buffer, 0, 5);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void WriteDouble(FastBinaryWriter bw, byte type, double value)
		{
			_buffer[0] = type;
			fixed (byte* b = &_buffer[1])
				*((double*)b) = value;
            bw.Write(_buffer, 0, 9);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void WriteFloat(FastBinaryWriter bw, byte type, float value)
		{
			_buffer[0] = type;
			fixed (byte* b = &_buffer[1])
				*((float*)b) = value;
            bw.Write(_buffer, 0, 5);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void WriteLong(FastBinaryWriter bw, byte type, long value)
		{
			_buffer[0] = type;
			fixed (byte* b = &_buffer[1])
				*((long*)b) = value;
            bw.Write(_buffer, 0, 9);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void WriteULong(FastBinaryWriter bw, byte type, ulong value)
		{
			_buffer[0] = type;
			fixed (byte* b = &_buffer[1])
				*((ulong*)b) = value;
            bw.Write(_buffer, 0, 9);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void WriteUInt(FastBinaryWriter bw, byte type, uint value)
		{
			_buffer[0] = type;
			fixed (byte* b = &_buffer[1])
				*((uint*)b) = value;
            bw.Write(_buffer, 0, 5);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void WriteShort(FastBinaryWriter bw, byte type, short value)
		{
			_buffer[0] = type;
			fixed (byte* b = &_buffer[1])
				*((short*)b) = value;
            bw.Write(_buffer, 0, 3);
		}
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe void WriteChar(FastBinaryWriter bw, byte type, char value)
        {
            _buffer[0] = type;
            fixed (byte* b = &_buffer[1])
                *((char*)b) = value;
            bw.Write(_buffer, 0, 3);
        }
    }

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
        public abstract void WriteValue(FastBinaryWriter writer, T value);
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract T ReadValue(FastBinaryReader reader, byte type);
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract T ReadValue(FastBinaryReader reader);
    }
}
