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
using System.IO;
using System.Runtime.CompilerServices;
// ReSharper disable UnusedMemberInSuper.Global

namespace TWCore.Serialization.PWSerializer
{
    /// <summary>
    /// Definition for a type serializer
    /// </summary>
    public abstract class TypeSerializer
    {
        private readonly byte[] _buffer = new byte[2];

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
			bw.Write(type);
			bw.Write(value);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void WriteInt(BinaryWriter bw, byte type, int value)
		{
			bw.Write(type);
			bw.Write(value);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void WriteDouble(BinaryWriter bw, byte type, double value)
		{
			bw.Write(type);
			bw.Write(value);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void WriteFloat(BinaryWriter bw, byte type, float value)
		{
			bw.Write(type);
			bw.Write(value);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void WriteLong(BinaryWriter bw, byte type, long value)
		{
			bw.Write(type);
			bw.Write(value);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void WriteULong(BinaryWriter bw, byte type, ulong value)
		{
			bw.Write(type);
			bw.Write(value);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void WriteUInt(BinaryWriter bw, byte type, uint value)
		{
			bw.Write(type);
			bw.Write(value);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void WriteShort(BinaryWriter bw, byte type, short value)
		{
			bw.Write(type);
			bw.Write(value);
		}
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void WriteChar(BinaryWriter bw, byte type, char value)
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
