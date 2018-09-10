﻿/*
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
using System.Text;
using System.Buffers;
// ReSharper disable InconsistentNaming
#pragma warning disable 1591

namespace TWCore.Serialization.WSerializer.Types
{
    /// <inheritdoc />
    /// <summary>
    /// String type serializer
    /// </summary>
	public class StringSerializer : TypeSerializer<string>
    {
        private static readonly Encoding DefaultUTF8Encoding = new UTF8Encoding(false);

        public static readonly HashSet<byte> ReadTypes = new HashSet<byte>(new []
        {
            DataType.StringNull, DataType.StringEmpty,
            DataType.StringLengthByte1, DataType.StringLengthByte2, DataType.StringLengthByte3, DataType.StringLengthByte4, DataType.StringLengthByte5, DataType.StringLengthByte6,
            DataType.StringLengthByte7, DataType.StringLengthByte8, DataType.StringLengthByte9, DataType.StringLengthByte10, DataType.StringLengthByte11, DataType.StringLengthByte12,
            DataType.StringLengthByte13, DataType.StringLengthByte14, DataType.StringLengthByte15, DataType.StringLengthByte16,
            DataType.StringLengthByte, DataType.StringLengthUShort, DataType.StringLengthInt,
            DataType.RefStringByte, DataType.RefStringUShort,

            DataType.RefString32Byte, DataType.RefString32Byte0, DataType.RefString32Byte1, DataType.RefString32Byte2, DataType.RefString32Byte3, DataType.RefString32Byte4, DataType.RefString32Byte5,
            DataType.RefString32Byte6, DataType.RefString32Byte7, DataType.RefString32Byte8, DataType.RefString32Byte9, DataType.RefString32Byte10, DataType.RefString32Byte11, DataType.RefString32Byte12,
            DataType.RefString32Byte13, DataType.RefString32Byte14, DataType.RefString32Byte15, DataType.RefString32Byte16, DataType.RefString32UShort,
            DataType.RefString16Byte, DataType.RefString16Byte0, DataType.RefString16Byte1, DataType.RefString16Byte2, DataType.RefString16Byte3, DataType.RefString16Byte4, DataType.RefString16Byte5,
            DataType.RefString16Byte6, DataType.RefString16Byte7, DataType.RefString16Byte8, DataType.RefString16Byte9, DataType.RefString16Byte10, DataType.RefString16Byte11, DataType.RefString16Byte12,
            DataType.RefString16Byte13, DataType.RefString16Byte14, DataType.RefString16Byte15, DataType.RefString16Byte16, DataType.RefString16UShort
        });

        private SerializerMode _mode;
        private SerializerCache<string> _cache;
        private SerializerCache<string> Cache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _cache ?? (_cache = new SerializerCache<string>(_mode)); }
        }
        private SerializerCache<string> _cache16;
        private SerializerCache<string> Cache16
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _cache16 ?? (_cache16 = new SerializerCache<string>(_mode)); }
        }
        private SerializerCache<string> _cache32;
        private SerializerCache<string> Cache32
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _cache32 ?? (_cache32 = new SerializerCache<string>(_mode)); }
        }
        private bool _useCache;
        public Encoding Encoding { get; set; } = DefaultUTF8Encoding;

        /// <inheritdoc />
        /// <summary>
        /// Type serializer initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Init(SerializerMode mode)
        {
            _mode = mode;
            Cache?.Clear(mode);
            Cache16?.Clear(mode);
            Cache32?.Clear(mode);
            _useCache = (mode != SerializerMode.NoCached);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets if the type serializer can write the type
        /// </summary>
        /// <param name="type">Type of the value to write</param>
        /// <returns>true if the type serializer can write the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanWrite(Type type)
            => type == typeof(string);
        /// <inheritdoc />
        /// <summary>
        /// Gets if the type serializer can read the data type
        /// </summary>
        /// <param name="type">DataType value</param>
        /// <returns>true if the type serializer can read the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanRead(byte type) 
            => ReadTypes.Contains(type);
        /// <inheritdoc />
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(BinaryWriter writer, object value)
            => WriteValue(writer, (string)value);
        /// <inheritdoc />
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteValue(BinaryWriter writer, string value)
        {
            switch (value)
            {
                case null:
                    writer.Write(DataType.StringNull);
                    return;
                case "":
                    writer.Write(DataType.StringEmpty);
                    return;
            }
            var cache16 = false;
            var cache32 = false;
            if (_useCache)
            {
                var vLength = value.Length;
                if (vLength <= 16)
                {
                    cache16 = true;
                    var objIdx = Cache16.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        #region Write Reference
                        switch (objIdx)
                        {
                            case 0:
                                writer.Write(DataType.RefString16Byte0);
                                return;
                            case 1:
                                writer.Write(DataType.RefString16Byte1);
                                return;
                            case 2:
                                writer.Write(DataType.RefString16Byte2);
                                return;
                            case 3:
                                writer.Write(DataType.RefString16Byte3);
                                return;
                            case 4:
                                writer.Write(DataType.RefString16Byte4);
                                return;
                            case 5:
                                writer.Write(DataType.RefString16Byte5);
                                return;
                            case 6:
                                writer.Write(DataType.RefString16Byte6);
                                return;
                            case 7:
                                writer.Write(DataType.RefString16Byte7);
                                return;
                            case 8:
                                writer.Write(DataType.RefString16Byte8);
                                return;
                            case 9:
                                writer.Write(DataType.RefString16Byte9);
                                return;
                            case 10:
                                writer.Write(DataType.RefString16Byte10);
                                return;
                            case 11:
                                writer.Write(DataType.RefString16Byte11);
                                return;
                            case 12:
                                writer.Write(DataType.RefString16Byte12);
                                return;
                            case 13:
                                writer.Write(DataType.RefString16Byte13);
                                return;
                            case 14:
                                writer.Write(DataType.RefString16Byte14);
                                return;
                            case 15:
                                writer.Write(DataType.RefString16Byte15);
                                return;
                            case 16:
                                writer.Write(DataType.RefString16Byte16);
                                return;
                            default:
                                if (objIdx <= byte.MaxValue)
                                    WriteByte(writer, DataType.RefString16Byte, (byte)objIdx);
                                else
                                    WriteUshort(writer, DataType.RefString16UShort, (ushort)objIdx);
                                return;
                        }
                        #endregion
                    }
                }
                else if (vLength <= 32)
                {
                    cache32 = true;
                    var objIdx = Cache32.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        #region Write Reference
                        switch (objIdx)
                        {
                            case 0:
                                writer.Write(DataType.RefString32Byte0);
                                return;
                            case 1:
                                writer.Write(DataType.RefString32Byte1);
                                return;
                            case 2:
                                writer.Write(DataType.RefString32Byte2);
                                return;
                            case 3:
                                writer.Write(DataType.RefString32Byte3);
                                return;
                            case 4:
                                writer.Write(DataType.RefString32Byte4);
                                return;
                            case 5:
                                writer.Write(DataType.RefString32Byte5);
                                return;
                            case 6:
                                writer.Write(DataType.RefString32Byte6);
                                return;
                            case 7:
                                writer.Write(DataType.RefString32Byte7);
                                return;
                            case 8:
                                writer.Write(DataType.RefString32Byte8);
                                return;
                            case 9:
                                writer.Write(DataType.RefString32Byte9);
                                return;
                            case 10:
                                writer.Write(DataType.RefString32Byte10);
                                return;
                            case 11:
                                writer.Write(DataType.RefString32Byte11);
                                return;
                            case 12:
                                writer.Write(DataType.RefString32Byte12);
                                return;
                            case 13:
                                writer.Write(DataType.RefString32Byte13);
                                return;
                            case 14:
                                writer.Write(DataType.RefString32Byte14);
                                return;
                            case 15:
                                writer.Write(DataType.RefString32Byte15);
                                return;
                            case 16:
                                writer.Write(DataType.RefString32Byte16);
                                return;
                            default:
                                if (objIdx <= byte.MaxValue)
                                    WriteByte(writer, DataType.RefString32Byte, (byte)objIdx);
                                else
                                    WriteUshort(writer, DataType.RefString32UShort, (ushort)objIdx);
                                return;
                        }
                        #endregion
                    }
                }
                else
                {
                    var objIdx = Cache.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteByte(writer, DataType.RefStringByte, (byte)objIdx);
                        else
                            WriteUshort(writer, DataType.RefStringUShort, (ushort)objIdx);
                        return;
                    }
                }
            }

            var length = Encoding.GetByteCount(value);
            int bytesLength;
            byte[] bytes;

            if (length < 17)
            {
                bytesLength = length + 1;
                bytes = ArrayPool<byte>.Shared.Rent(bytesLength);
                bytes[0] = (byte)(DataType.StringLengthByte1 + (length - 1));
                Encoding.GetBytes(value, 0, value.Length, bytes, 1);
            }
            else if (length <= byte.MaxValue)
            {
                bytesLength = length + 2;
                bytes = ArrayPool<byte>.Shared.Rent(bytesLength);
                bytes[0] = DataType.StringLengthByte;
                bytes[1] = (byte)length;
                Encoding.GetBytes(value, 0, value.Length, bytes, 2);
            }
            else if (length <= ushort.MaxValue)
            {
                bytesLength = length + 3;
                bytes = ArrayPool<byte>.Shared.Rent(bytesLength);
                bytes[0] = DataType.StringLengthUShort;
                bytes[1] = (byte)(ushort)length;
                bytes[2] = (byte)(((ushort)length) >> 8);
                Encoding.GetBytes(value, 0, value.Length, bytes, 3);
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
                Encoding.GetBytes(value, 0, value.Length, bytes, 5);
            }

            writer.Write(bytes, 0, bytesLength);
            ArrayPool<byte>.Shared.Return(bytes);

            if (!_useCache)
                return;
            if (length < 2)
                return;
            if (length == 2 && Cache.Count > byte.MaxValue)
                return;
            if (length > 2 && Cache.Count > ushort.MaxValue)
                return;

            if (cache16)
                Cache16.SerializerSet(value);
            else if (cache32)
                Cache32.SerializerSet(value);
            else
                Cache.SerializerSet(value);
        }
        /// <inheritdoc />
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Read(BinaryReader reader, byte type)
            => ReadValue(reader, type);
        /// <inheritdoc />
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ReadValue(BinaryReader reader, byte type)
        {
            switch (type)
            {
                case DataType.StringNull:
                    return null;
                case DataType.StringEmpty:
                    return string.Empty;
                case DataType.RefStringByte:
                    return Cache.DeserializerGet(reader.ReadByte());
                case DataType.RefStringUShort:
                    return Cache.DeserializerGet(reader.ReadUInt16());

                case DataType.RefString32Byte:
                    return Cache32.DeserializerGet(reader.ReadByte());
                case DataType.RefString32Byte0:
                    return Cache32.DeserializerGet(0);
                case DataType.RefString32Byte1:
                    return Cache32.DeserializerGet(1);
                case DataType.RefString32Byte2:
                    return Cache32.DeserializerGet(2);
                case DataType.RefString32Byte3:
                    return Cache32.DeserializerGet(3);
                case DataType.RefString32Byte4:
                    return Cache32.DeserializerGet(4);
                case DataType.RefString32Byte5:
                    return Cache32.DeserializerGet(5);
                case DataType.RefString32Byte6:
                    return Cache32.DeserializerGet(6);
                case DataType.RefString32Byte7:
                    return Cache32.DeserializerGet(7);
                case DataType.RefString32Byte8:
                    return Cache32.DeserializerGet(8);
                case DataType.RefString32Byte9:
                    return Cache32.DeserializerGet(9);
                case DataType.RefString32Byte10:
                    return Cache32.DeserializerGet(10);
                case DataType.RefString32Byte11:
                    return Cache32.DeserializerGet(11);
                case DataType.RefString32Byte12:
                    return Cache32.DeserializerGet(12);
                case DataType.RefString32Byte13:
                    return Cache32.DeserializerGet(13);
                case DataType.RefString32Byte14:
                    return Cache32.DeserializerGet(14);
                case DataType.RefString32Byte15:
                    return Cache32.DeserializerGet(15);
                case DataType.RefString32Byte16:
                    return Cache32.DeserializerGet(16);
                case DataType.RefString32UShort:
                    return Cache32.DeserializerGet(reader.ReadUInt16());

                case DataType.RefString16Byte:
                    return Cache16.DeserializerGet(reader.ReadByte());
                case DataType.RefString16Byte0:
                    return Cache16.DeserializerGet(0);
                case DataType.RefString16Byte1:
                    return Cache16.DeserializerGet(1);
                case DataType.RefString16Byte2:
                    return Cache16.DeserializerGet(2);
                case DataType.RefString16Byte3:
                    return Cache16.DeserializerGet(3);
                case DataType.RefString16Byte4:
                    return Cache16.DeserializerGet(4);
                case DataType.RefString16Byte5:
                    return Cache16.DeserializerGet(5);
                case DataType.RefString16Byte6:
                    return Cache16.DeserializerGet(6);
                case DataType.RefString16Byte7:
                    return Cache16.DeserializerGet(7);
                case DataType.RefString16Byte8:
                    return Cache16.DeserializerGet(8);
                case DataType.RefString16Byte9:
                    return Cache16.DeserializerGet(9);
                case DataType.RefString16Byte10:
                    return Cache16.DeserializerGet(10);
                case DataType.RefString16Byte11:
                    return Cache16.DeserializerGet(11);
                case DataType.RefString16Byte12:
                    return Cache16.DeserializerGet(12);
                case DataType.RefString16Byte13:
                    return Cache16.DeserializerGet(13);
                case DataType.RefString16Byte14:
                    return Cache16.DeserializerGet(14);
                case DataType.RefString16Byte15:
                    return Cache16.DeserializerGet(15);
                case DataType.RefString16Byte16:
                    return Cache16.DeserializerGet(16);
                case DataType.RefString16UShort:
                    return Cache16.DeserializerGet(reader.ReadUInt16());
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
            var strValue = Encoding.GetString(bytes, 0, length);
            ArrayPool<byte>.Shared.Return(bytes);

            if (!_useCache)
                return strValue;
            if (length < 2)
                return strValue;
            if (length == 2 && Cache.Count > byte.MaxValue)
                return strValue;
            if (length > 2 && Cache.Count > ushort.MaxValue)
                return strValue;

            var vLength = strValue.Length;

            if (vLength <= 16)
                Cache16.DeserializerSet(strValue);
            else if (vLength <= 32)
                Cache32.DeserializerSet(strValue);
            else
                Cache.DeserializerSet(strValue);

            return strValue;
        }
        /// <inheritdoc />
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ReadValue(BinaryReader reader)
            => ReadValue(reader, reader.ReadByte());
    }
}
