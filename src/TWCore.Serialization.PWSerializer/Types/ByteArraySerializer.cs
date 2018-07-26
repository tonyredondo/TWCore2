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
using System.IO;
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.PWSerializer.Types
{
    /// <inheritdoc cref="ITypeSerializer" />
    /// <summary>
    /// Byte array optimized serializer
    /// </summary>
    public class ByteArraySerializer : ITypeSerializer<byte[]>
    {
        public static readonly HashSet<byte> ReadTypes = new HashSet<byte>(new []
        {
            DataType.ByteArrayNull, DataType.ByteArrayEmpty, DataType.ByteArrayLengthByte, DataType.ByteArrayLengthUShort, DataType.ByteArrayLengthInt,
        });

        /// <inheritdoc />
        /// <summary>
        /// Type serializer initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
        }
        /// <inheritdoc />
        /// <summary>
        /// Clear serializer cache
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets if the type serializer can write the type
        /// </summary>
        /// <param name="type">Type of the value to write</param>
        /// <returns>true if the type serializer can write the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanWrite(Type type)
            => type == typeof(byte[]);
        /// <inheritdoc />
        /// <summary>
        /// Gets if the type serializer can read the data type
        /// </summary>
        /// <param name="type">DataType value</param>
        /// <returns>true if the type serializer can read the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanRead(byte type)
            => ReadTypes.Contains(type);
        /// <inheritdoc />
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter writer, object value)
            => WriteValue(writer, (byte[])value);
        /// <inheritdoc />
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Read(BinaryReader reader, byte type)
            => ReadValue(reader, type);

        /// <inheritdoc />
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(BinaryWriter writer, byte[] value)
        {
			if (value == null)
			{
				writer.Write(DataType.ByteArrayNull);
				return;
			}
            if (value.Length == 0)
            {
                writer.Write(DataType.ByteArrayEmpty);
                return;
            }
            #region Write Array
            var length = value.Length;
            if (length <= byte.MaxValue)
                WriteHelper.WriteByte(writer, DataType.ByteArrayLengthByte, (byte)length);
            else if (length <= ushort.MaxValue)
                WriteHelper.WriteUshort(writer, DataType.ByteArrayLengthUShort, (ushort)length);
            else
                WriteHelper.WriteInt(writer, DataType.ByteArrayLengthInt, length);
            writer.Write(value);
            #endregion
        }
        /// <inheritdoc />
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadValue(BinaryReader reader, byte type)
        {
            var length = 0;
            switch (type)
            {
                case DataType.ByteArrayNull:
                    return null;
                case DataType.ByteArrayEmpty:
                    return Array.Empty<byte>();
                case DataType.ByteArrayLengthByte:
                    length = reader.ReadByte();
                    break;
                case DataType.ByteArrayLengthUShort:
                    length = reader.ReadUInt16();
                    break;
                case DataType.ByteArrayLengthInt:
                    length = reader.ReadInt32();
                    break;
            }
            var cValue = reader.ReadBytes(length);
            return cValue;
        }
        /// <inheritdoc />
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadValue(BinaryReader reader)
            => ReadValue(reader, reader.ReadByte());
    }
}
