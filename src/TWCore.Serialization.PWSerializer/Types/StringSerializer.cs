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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TWCore.IO;
// ReSharper disable InconsistentNaming

namespace TWCore.Serialization.PWSerializer.Types
{
    /// <summary>
    /// String type serializer
    /// </summary>
	public class StringSerializer : TypeSerializer<string>
    {
        private static readonly Encoding DefaultUTF8Encoding = new UTF8Encoding(false);

        public static HashSet<byte> ReadTypes = new HashSet<byte>(new[]
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

        private SerializerMode _mode;
        private SerializerCache<string> _cache;
        private SerializerCache<string> Cache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _cache ?? (_cache = new SerializerCache<string>(_mode)); }
        }
        private SerializerCache<string> _cache8;
        private SerializerCache<string> Cache8
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _cache8 ?? (_cache8 = new SerializerCache<string>(_mode)); }
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

        /// <summary>
        /// Type serializer initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Init(SerializerMode mode)
        {
            _mode = mode;
            Cache?.Clear(mode);
            Cache8?.Clear(mode);
            Cache16?.Clear(mode);
            Cache32?.Clear(mode);
            _useCache = (mode != SerializerMode.NoCached);
        }
        /// <summary>
        /// Gets if the type serializer can write the type
        /// </summary>
        /// <param name="type">Type of the value to write</param>
        /// <returns>true if the type serializer can write the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanWrite(Type type)
            => type == typeof(string);
        /// <summary>
        /// Gets if the type serializer can read the data type
        /// </summary>
        /// <param name="type">DataType value</param>
        /// <returns>true if the type serializer can read the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanRead(byte type)
            => ReadTypes.Contains(type);
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(FastBinaryWriter writer, object value)
            => WriteValue(writer, (string)value);
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteValue(FastBinaryWriter writer, string value)
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
            var cache8 = false;
            var cache16 = false;
            var cache32 = false;
            if (_useCache)
            {
                var vLength = value.Length;
                if (vLength <= 8)
                {
                    cache8 = true;
                    var objIdx = Cache8.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        #region Write Reference
                        switch (objIdx)
                        {
                            case 0:
                                writer.Write(DataType.RefString8Byte0);
                                return;
                            case 1:
                                writer.Write(DataType.RefString8Byte1);
                                return;
                            case 2:
                                writer.Write(DataType.RefString8Byte2);
                                return;
                            case 3:
                                writer.Write(DataType.RefString8Byte3);
                                return;
                            case 4:
                                writer.Write(DataType.RefString8Byte4);
                                return;
                            case 5:
                                writer.Write(DataType.RefString8Byte5);
                                return;
                            case 6:
                                writer.Write(DataType.RefString8Byte6);
                                return;
                            case 7:
                                writer.Write(DataType.RefString8Byte7);
                                return;
                            case 8:
                                writer.Write(DataType.RefString8Byte8);
                                return;
                            case 9:
                                writer.Write(DataType.RefString8Byte9);
                                return;
                            case 10:
                                writer.Write(DataType.RefString8Byte10);
                                return;
                            case 11:
                                writer.Write(DataType.RefString8Byte11);
                                return;
                            case 12:
                                writer.Write(DataType.RefString8Byte12);
                                return;
                            case 13:
                                writer.Write(DataType.RefString8Byte13);
                                return;
                            case 14:
                                writer.Write(DataType.RefString8Byte14);
                                return;
                            case 15:
                                writer.Write(DataType.RefString8Byte15);
                                return;
                            default:
                                if (objIdx <= byte.MaxValue)
                                    WriteByte(writer, DataType.RefString8Byte, (byte)objIdx);
                                else
                                    WriteUshort(writer, DataType.RefString8UShort, (ushort)objIdx);
                                return;
                        }
                        #endregion
                    }
                }
                else if (vLength <= 16)
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

            var bytes = Encoding.GetBytes(value);
            var length = bytes.Length;

            #region Write Length
            switch (length)
            {
                case 1:
                    writer.Write(DataType.StringLengthByte1);
                    break;
                case 2:
                    writer.Write(DataType.StringLengthByte2);
                    break;
                case 3:
                    writer.Write(DataType.StringLengthByte3);
                    break;
                case 4:
                    writer.Write(DataType.StringLengthByte4);
                    break;
                case 5:
                    writer.Write(DataType.StringLengthByte5);
                    break;
                case 6:
                    writer.Write(DataType.StringLengthByte6);
                    break;
                case 7:
                    writer.Write(DataType.StringLengthByte7);
                    break;
                case 8:
                    writer.Write(DataType.StringLengthByte8);
                    break;
                case 9:
                    writer.Write(DataType.StringLengthByte9);
                    break;
                case 10:
                    writer.Write(DataType.StringLengthByte10);
                    break;
                case 11:
                    writer.Write(DataType.StringLengthByte11);
                    break;
                case 12:
                    writer.Write(DataType.StringLengthByte12);
                    break;
                case 13:
                    writer.Write(DataType.StringLengthByte13);
                    break;
                case 14:
                    writer.Write(DataType.StringLengthByte14);
                    break;
                case 15:
                    writer.Write(DataType.StringLengthByte15);
                    break;
                case 16:
                    writer.Write(DataType.StringLengthByte16);
                    break;
                case 17:
                    writer.Write(DataType.StringLengthByte17);
                    break;
                case 18:
                    writer.Write(DataType.StringLengthByte18);
                    break;
                case 19:
                    writer.Write(DataType.StringLengthByte19);
                    break;
                case 20:
                    writer.Write(DataType.StringLengthByte20);
                    break;
                default:
                    if (length <= byte.MaxValue)
                        WriteByte(writer, DataType.StringLengthByte, (byte)length);
                    else if (length <= ushort.MaxValue)
                        WriteUshort(writer, DataType.StringLengthUShort, (ushort)length);
                    else
                        WriteInt(writer, DataType.StringLengthUShort, length);
                    break;
            }
            #endregion

            writer.Write(bytes, 0, length);

            if (!_useCache || length <= 2) return;
            if (cache8)
                Cache8.SerializerSet(value);
            else if (cache16)
                Cache16.SerializerSet(value);
            else if (cache32)
                Cache32.SerializerSet(value);
            else
                Cache.SerializerSet(value);
        }
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Read(FastBinaryReader reader, byte type)
            => ReadValue(reader, type);
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ReadValue(FastBinaryReader reader, byte type)
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
                case DataType.RefString16UShort:
                    return Cache16.DeserializerGet(reader.ReadUInt16());

                case DataType.RefString8Byte:
                    return Cache8.DeserializerGet(reader.ReadByte());
                case DataType.RefString8Byte0:
                    return Cache8.DeserializerGet(0);
                case DataType.RefString8Byte1:
                    return Cache8.DeserializerGet(1);
                case DataType.RefString8Byte2:
                    return Cache8.DeserializerGet(2);
                case DataType.RefString8Byte3:
                    return Cache8.DeserializerGet(3);
                case DataType.RefString8Byte4:
                    return Cache8.DeserializerGet(4);
                case DataType.RefString8Byte5:
                    return Cache8.DeserializerGet(5);
                case DataType.RefString8Byte6:
                    return Cache8.DeserializerGet(6);
                case DataType.RefString8Byte7:
                    return Cache8.DeserializerGet(7);
                case DataType.RefString8Byte8:
                    return Cache8.DeserializerGet(8);
                case DataType.RefString8Byte9:
                    return Cache8.DeserializerGet(9);
                case DataType.RefString8Byte10:
                    return Cache8.DeserializerGet(10);
                case DataType.RefString8Byte11:
                    return Cache8.DeserializerGet(11);
                case DataType.RefString8Byte12:
                    return Cache8.DeserializerGet(12);
                case DataType.RefString8Byte13:
                    return Cache8.DeserializerGet(13);
                case DataType.RefString8Byte14:
                    return Cache8.DeserializerGet(14);
                case DataType.RefString8Byte15:
                    return Cache8.DeserializerGet(15);
                case DataType.RefString8UShort:
                    return Cache8.DeserializerGet(reader.ReadUInt16());
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

            var bytes = reader.ReadBytes(length);
            var strValue = Encoding.GetString(bytes);

            if (!_useCache || length <= 2) return strValue;
            var sLength = strValue.Length;
            if (sLength <= 8)
                Cache8.DeserializerSet(strValue);
            else if (sLength <= 16)
                Cache16.DeserializerSet(strValue);
            else if (sLength <= 32)
                Cache32.DeserializerSet(strValue);
            else
                Cache.DeserializerSet(strValue);
            return strValue;
        }
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ReadValue(FastBinaryReader reader)
            => ReadValue(reader, reader.ReadByte());
    }
}
