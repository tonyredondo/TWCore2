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
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
// ReSharper disable InconsistentNaming
#pragma warning disable 1591

namespace TWCore.Serialization.PWSerializer.Types
{
    /// <inheritdoc cref="ITypeSerializer" />
    /// <summary>
    /// String type serializer
    /// </summary>
	public class StringSerializer : ITypeSerializer<string>
    {
        private static readonly Encoding DefaultUTF8Encoding = new UTF8Encoding(false);

        public static readonly HashSet<byte> ReadTypes = new HashSet<byte>(new[]
        {
            DataType.StringNull, DataType.StringEmpty,
            DataType.StringLengthByte1, DataType.StringLengthByte2, DataType.StringLengthByte3, DataType.StringLengthByte4, DataType.StringLengthByte5, DataType.StringLengthByte6,
            DataType.StringLengthByte7, DataType.StringLengthByte8, DataType.StringLengthByte9, DataType.StringLengthByte10, DataType.StringLengthByte11, DataType.StringLengthByte12,
            DataType.StringLengthByte13, DataType.StringLengthByte14, DataType.StringLengthByte15, DataType.StringLengthByte16,
            DataType.StringLengthByte17, DataType.StringLengthByte18, DataType.StringLengthByte19, DataType.StringLengthByte20,
            DataType.StringLengthByte, DataType.StringLengthUShort, DataType.StringLengthInt,
            DataType.RefStringByte, DataType.RefStringUShort,

            DataType.RefString32Byte, DataType.RefString32Byte0, DataType.RefString32Byte1, DataType.RefString32Byte2, DataType.RefString32Byte3, DataType.RefString32Byte4, DataType.RefString32Byte5,
            DataType.RefString32Byte6, DataType.RefString32Byte7, DataType.RefString32Byte8, DataType.RefString32Byte9, DataType.RefString32Byte10, DataType.RefString32Byte11, DataType.RefString32Byte12,
            DataType.RefString32Byte13, DataType.RefString32Byte14, DataType.RefString32Byte15, DataType.RefString32UShort,

            DataType.RefString16Byte, DataType.RefString16Byte0, DataType.RefString16Byte1, DataType.RefString16Byte2, DataType.RefString16Byte3, DataType.RefString16Byte4, DataType.RefString16Byte5,
            DataType.RefString16Byte6, DataType.RefString16Byte7, DataType.RefString16Byte8, DataType.RefString16Byte9, DataType.RefString16Byte10, DataType.RefString16Byte11, DataType.RefString16Byte12,
            DataType.RefString16Byte13, DataType.RefString16Byte14, DataType.RefString16Byte15, DataType.RefString16UShort,

            DataType.RefString8Byte,    DataType.RefString8Byte0,   DataType.RefString8Byte1,   DataType.RefString8Byte2, DataType.RefString8Byte3,     DataType.RefString8Byte4,   DataType.RefString8Byte5,
            DataType.RefString8Byte6,   DataType.RefString8Byte7,   DataType.RefString8Byte8,   DataType.RefString8Byte9, DataType.RefString8Byte10,    DataType.RefString8Byte11,  DataType.RefString8Byte12,
            DataType.RefString8Byte13,  DataType.RefString8Byte14,  DataType.RefString8Byte15,  DataType.RefString8UShort
        });

        private SerializerCache<string> _cache;
        private SerializerCache<string> _cache8;
        private SerializerCache<string> _cache16;
        private SerializerCache<string> _cache32;

        /// <inheritdoc />
        /// <summary>
        /// Type serializer initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            _cache = new SerializerCache<string>();
            _cache8 = new SerializerCache<string>();
            _cache16 = new SerializerCache<string>();
            _cache32 = new SerializerCache<string>();
        }
        /// <inheritdoc />
        /// <summary>
        /// Clear serializer cache
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _cache.Clear();
            _cache8.Clear();
            _cache16.Clear();
            _cache32.Clear();
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets if the type serializer can write the type
        /// </summary>
        /// <param name="type">Type of the value to write</param>
        /// <returns>true if the type serializer can write the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanWrite(Type type)
            => type == typeof(string);
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
            => WriteValue(writer, (string)value);
        /// <inheritdoc />
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(BinaryWriter writer, string value)
        {
            if (value == null)
            {
                writer.Write(DataType.StringNull);
                return;
            }
            if (value == string.Empty)
            {
                writer.Write(DataType.StringEmpty);
                return;
            }

            var vLength = value.Length;

            if (vLength > 2)
            {
                if (vLength <= 8)
                {
                    var objIdx = _cache8.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        if (objIdx < 16)
                            writer.Write((byte)(DataType.RefString8Byte0 + objIdx));
                        else if (objIdx <= byte.MaxValue)
                            WriteHelper.WriteByte(writer, DataType.RefString8Byte, (byte)objIdx);
                        else
                            WriteHelper.WriteUshort(writer, DataType.RefString8UShort, (ushort)objIdx);
                        return;
                    }
                    _cache8.SerializerSet(value);
                }
                else if (vLength <= 16)
                {
                    var objIdx = _cache16.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        if (objIdx < 16)
                            writer.Write((byte)(DataType.RefString16Byte0 + objIdx));
                        else if (objIdx <= byte.MaxValue)
                            WriteHelper.WriteByte(writer, DataType.RefString16Byte, (byte)objIdx);
                        else
                            WriteHelper.WriteUshort(writer, DataType.RefString16UShort, (ushort)objIdx);
                        return;
                    }
                    _cache16.SerializerSet(value);
                }
                else if (vLength <= 32)
                {
                    var objIdx = _cache32.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        if (objIdx < 16)
                            writer.Write((byte)(DataType.RefString32Byte0 + objIdx));
                        else if (objIdx <= byte.MaxValue)
                            WriteHelper.WriteByte(writer, DataType.RefString32Byte, (byte)objIdx);
                        else
                            WriteHelper.WriteUshort(writer, DataType.RefString32UShort, (ushort)objIdx);
                        return;
                    }
                    _cache32.SerializerSet(value);
                }
                else
                {
                    var objIdx = _cache.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteHelper.WriteByte(writer, DataType.RefStringByte, (byte)objIdx);
                        else
                            WriteHelper.WriteUshort(writer, DataType.RefStringUShort, (ushort)objIdx);
                        return;
                    }
                    _cache.SerializerSet(value);
                }
            }

            var length = DefaultUTF8Encoding.GetByteCount(value);
            int bytesLength;
            byte[] bytes;

            if (length < 21)
            {
                bytesLength = length + 1;
                bytes = ArrayPool<byte>.Shared.Rent(bytesLength);
                bytes[0] = (byte)(DataType.StringLengthByte1 + (length - 1));
                DefaultUTF8Encoding.GetBytes(value, 0, value.Length, bytes, 1);
            }
            else if (length <= byte.MaxValue)
            {
                bytesLength = length + 2;
                bytes = ArrayPool<byte>.Shared.Rent(bytesLength);
                bytes[0] = DataType.StringLengthByte;
                bytes[1] = (byte)length;
                DefaultUTF8Encoding.GetBytes(value, 0, value.Length, bytes, 2);
            }
            else if (length <= ushort.MaxValue)
            {
                bytesLength = length + 3;
                bytes = ArrayPool<byte>.Shared.Rent(bytesLength);
                bytes[0] = DataType.StringLengthUShort;
                bytes[1] = (byte)(ushort)length;
                bytes[2] = (byte)(((ushort)length) >> 8);
                DefaultUTF8Encoding.GetBytes(value, 0, value.Length, bytes, 3);
            }
            else
            {
                bytesLength = length + 5;
                bytes = ArrayPool<byte>.Shared.Rent(bytesLength);
                bytes[0] = DataType.StringLengthInt;
                bytes[1] = (byte)length;
                bytes[2] = (byte)(length >> 8);
                bytes[3] = (byte)(length >> 16);
                bytes[4] = (byte)(length >> 24);
                DefaultUTF8Encoding.GetBytes(value, 0, value.Length, bytes, 5);
            }

            writer.Write(bytes, 0, bytesLength);
            ArrayPool<byte>.Shared.Return(bytes);
        }
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
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadValue(BinaryReader reader, byte type)
        {
            switch (type)
            {
                case DataType.StringNull:
                    return null;
                case DataType.StringEmpty:
                    return string.Empty;
                case DataType.RefStringByte:
                    return _cache.DeserializerGet(reader.ReadByte());
                case DataType.RefStringUShort:
                    return _cache.DeserializerGet(reader.ReadUInt16());

                case DataType.RefString32Byte:
                    return _cache32.DeserializerGet(reader.ReadByte());
                case DataType.RefString32Byte0:
                    return _cache32.DeserializerGet(0);
                case DataType.RefString32Byte1:
                    return _cache32.DeserializerGet(1);
                case DataType.RefString32Byte2:
                    return _cache32.DeserializerGet(2);
                case DataType.RefString32Byte3:
                    return _cache32.DeserializerGet(3);
                case DataType.RefString32Byte4:
                    return _cache32.DeserializerGet(4);
                case DataType.RefString32Byte5:
                    return _cache32.DeserializerGet(5);
                case DataType.RefString32Byte6:
                    return _cache32.DeserializerGet(6);
                case DataType.RefString32Byte7:
                    return _cache32.DeserializerGet(7);
                case DataType.RefString32Byte8:
                    return _cache32.DeserializerGet(8);
                case DataType.RefString32Byte9:
                    return _cache32.DeserializerGet(9);
                case DataType.RefString32Byte10:
                    return _cache32.DeserializerGet(10);
                case DataType.RefString32Byte11:
                    return _cache32.DeserializerGet(11);
                case DataType.RefString32Byte12:
                    return _cache32.DeserializerGet(12);
                case DataType.RefString32Byte13:
                    return _cache32.DeserializerGet(13);
                case DataType.RefString32Byte14:
                    return _cache32.DeserializerGet(14);
                case DataType.RefString32Byte15:
                    return _cache32.DeserializerGet(15);
                case DataType.RefString32UShort:
                    return _cache32.DeserializerGet(reader.ReadUInt16());

                case DataType.RefString16Byte:
                    return _cache16.DeserializerGet(reader.ReadByte());
                case DataType.RefString16Byte0:
                    return _cache16.DeserializerGet(0);
                case DataType.RefString16Byte1:
                    return _cache16.DeserializerGet(1);
                case DataType.RefString16Byte2:
                    return _cache16.DeserializerGet(2);
                case DataType.RefString16Byte3:
                    return _cache16.DeserializerGet(3);
                case DataType.RefString16Byte4:
                    return _cache16.DeserializerGet(4);
                case DataType.RefString16Byte5:
                    return _cache16.DeserializerGet(5);
                case DataType.RefString16Byte6:
                    return _cache16.DeserializerGet(6);
                case DataType.RefString16Byte7:
                    return _cache16.DeserializerGet(7);
                case DataType.RefString16Byte8:
                    return _cache16.DeserializerGet(8);
                case DataType.RefString16Byte9:
                    return _cache16.DeserializerGet(9);
                case DataType.RefString16Byte10:
                    return _cache16.DeserializerGet(10);
                case DataType.RefString16Byte11:
                    return _cache16.DeserializerGet(11);
                case DataType.RefString16Byte12:
                    return _cache16.DeserializerGet(12);
                case DataType.RefString16Byte13:
                    return _cache16.DeserializerGet(13);
                case DataType.RefString16Byte14:
                    return _cache16.DeserializerGet(14);
                case DataType.RefString16Byte15:
                    return _cache16.DeserializerGet(15);
                case DataType.RefString16UShort:
                    return _cache16.DeserializerGet(reader.ReadUInt16());

                case DataType.RefString8Byte:
                    return _cache8.DeserializerGet(reader.ReadByte());
                case DataType.RefString8Byte0:
                    return _cache8.DeserializerGet(0);
                case DataType.RefString8Byte1:
                    return _cache8.DeserializerGet(1);
                case DataType.RefString8Byte2:
                    return _cache8.DeserializerGet(2);
                case DataType.RefString8Byte3:
                    return _cache8.DeserializerGet(3);
                case DataType.RefString8Byte4:
                    return _cache8.DeserializerGet(4);
                case DataType.RefString8Byte5:
                    return _cache8.DeserializerGet(5);
                case DataType.RefString8Byte6:
                    return _cache8.DeserializerGet(6);
                case DataType.RefString8Byte7:
                    return _cache8.DeserializerGet(7);
                case DataType.RefString8Byte8:
                    return _cache8.DeserializerGet(8);
                case DataType.RefString8Byte9:
                    return _cache8.DeserializerGet(9);
                case DataType.RefString8Byte10:
                    return _cache8.DeserializerGet(10);
                case DataType.RefString8Byte11:
                    return _cache8.DeserializerGet(11);
                case DataType.RefString8Byte12:
                    return _cache8.DeserializerGet(12);
                case DataType.RefString8Byte13:
                    return _cache8.DeserializerGet(13);
                case DataType.RefString8Byte14:
                    return _cache8.DeserializerGet(14);
                case DataType.RefString8Byte15:
                    return _cache8.DeserializerGet(15);
                case DataType.RefString8UShort:
                    return _cache8.DeserializerGet(reader.ReadUInt16());
            }

            var length = 0;
            switch (type)
            {
                case DataType.StringLengthByte:
                    length = reader.ReadByte();
                    break;
                case DataType.StringLengthByte1:
                    length = 1;
                    break;
                case DataType.StringLengthByte2:
                    length = 2;
                    break;
                case DataType.StringLengthByte3:
                    length = 3;
                    break;
                case DataType.StringLengthByte4:
                    length = 4;
                    break;
                case DataType.StringLengthByte5:
                    length = 5;
                    break;
                case DataType.StringLengthByte6:
                    length = 6;
                    break;
                case DataType.StringLengthByte7:
                    length = 7;
                    break;
                case DataType.StringLengthByte8:
                    length = 8;
                    break;
                case DataType.StringLengthByte9:
                    length = 9;
                    break;
                case DataType.StringLengthByte10:
                    length = 10;
                    break;
                case DataType.StringLengthByte11:
                    length = 11;
                    break;
                case DataType.StringLengthByte12:
                    length = 12;
                    break;
                case DataType.StringLengthByte13:
                    length = 13;
                    break;
                case DataType.StringLengthByte14:
                    length = 14;
                    break;
                case DataType.StringLengthByte15:
                    length = 15;
                    break;
                case DataType.StringLengthByte16:
                    length = 16;
                    break;
                case DataType.StringLengthByte17:
                    length = 17;
                    break;
                case DataType.StringLengthByte18:
                    length = 18;
                    break;
                case DataType.StringLengthByte19:
                    length = 19;
                    break;
                case DataType.StringLengthByte20:
                    length = 20;
                    break;
                case DataType.StringLengthUShort:
                    length = reader.ReadUInt16();
                    break;
                case DataType.StringLengthInt:
                    length = reader.ReadInt32();
                    break;
            }
            if (length == 0) return null;

            var bytes = ArrayPool<byte>.Shared.Rent(length);
            reader.Read(bytes, 0, length);
            var strValue = DefaultUTF8Encoding.GetString(bytes, 0, length);
            ArrayPool<byte>.Shared.Return(bytes);
            var sLength = strValue.Length;

            if (sLength <= 2) return strValue;
            if (sLength <= 8)
                _cache8.DeserializerSet(strValue);
            else if (sLength <= 16)
                _cache16.DeserializerSet(strValue);
            else if (sLength <= 32)
                _cache32.DeserializerSet(strValue);
            else
                _cache.DeserializerSet(strValue);
            return strValue;
        }
        /// <inheritdoc />
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadValue(BinaryReader reader)
            => ReadValue(reader, reader.ReadByte());
    }
}
