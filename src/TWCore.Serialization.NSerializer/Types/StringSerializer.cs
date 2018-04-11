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

namespace TWCore.Serialization.NSerializer.Types
{
    public class StringSerializer : ITypeSerializer
    {
        private SerializerCache<string> _cache16;
        private SerializerCache<string> _cache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            _cache16 = new SerializerCache<string>();
            _cache = new SerializerCache<string>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _cache16.Clear();
            _cache.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter writer, string value)
        {
            if (value == null)
            {
                writer.Write(DataBytesDefinition.StringNull);
                return;
            }
            if (value == string.Empty)
            {
                writer.Write(DataBytesDefinition.StringEmpty);
                return;
            }

            var vLength = value.Length;
            if (vLength > 2)
            {
                if (vLength <= 16)
                {
                    var objIdx = _cache16.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        if (objIdx < 16)
                            writer.Write((byte)(DataBytesDefinition.RefString16Byte0 + objIdx));
                        else if (objIdx <= byte.MaxValue)
                            WriteHelper.WriteByte(writer, DataBytesDefinition.RefString16Byte, (byte)objIdx);
                        else
                            WriteHelper.WriteUshort(writer, DataBytesDefinition.RefString16UShort, (ushort)objIdx);
                        return;
                    }
                    _cache16.SerializerSet(value);
                }
                else
                {
                    var objIdx = _cache.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        if (objIdx < 16)
                            writer.Write((byte)(DataBytesDefinition.RefStringByte0 + objIdx));
                        else if (objIdx <= byte.MaxValue)
                            WriteHelper.WriteByte(writer, DataBytesDefinition.RefStringByte, (byte)objIdx);
                        else
                            WriteHelper.WriteUshort(writer, DataBytesDefinition.RefStringUShort, (ushort)objIdx);
                        return;
                    }
                    _cache.SerializerSet(value);
                }
            }

            var length = Encoding.UTF8.GetByteCount(value);
            int bytesLength;
            byte[] bytes;

            if (length < 21)
            {
                bytesLength = length + 1;
                bytes = ArrayPool<byte>.Shared.Rent(bytesLength);
                bytes[0] = (byte)(DataBytesDefinition.StringLengthByte1 + (length - 1));
                Encoding.UTF8.GetBytes(value, 0, value.Length, bytes, 1);
            }
            else if (length <= byte.MaxValue)
            {
                bytesLength = length + 2;
                bytes = ArrayPool<byte>.Shared.Rent(bytesLength);
                bytes[0] = DataBytesDefinition.StringLengthByte;
                bytes[1] = (byte)length;
                Encoding.UTF8.GetBytes(value, 0, value.Length, bytes, 2);
            }
            else if (length <= ushort.MaxValue)
            {
                bytesLength = length + 3;
                bytes = ArrayPool<byte>.Shared.Rent(bytesLength);
                bytes[0] = DataBytesDefinition.StringLengthUShort;
                bytes[1] = (byte)(ushort)length;
                bytes[2] = (byte)(((ushort)length) >> 8);
                Encoding.UTF8.GetBytes(value, 0, value.Length, bytes, 3);
            }
            else
            {
                bytesLength = length + 5;
                bytes = ArrayPool<byte>.Shared.Rent(bytesLength);
                bytes[0] = DataBytesDefinition.StringLengthInt;
                bytes[1] = (byte)length;
                bytes[2] = (byte)(length >> 8);
                bytes[3] = (byte)(length >> 16);
                bytes[4] = (byte)(length >> 24);
                Encoding.UTF8.GetBytes(value, 0, value.Length, bytes, 5);
            }
            writer.Write(bytes, 0, bytesLength);
            ArrayPool<byte>.Shared.Return(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Read(BinaryReader reader)
        {
            var type = reader.ReadByte();
            int? length = null;
            switch (type)
            {
                case DataBytesDefinition.StringNull:
                    return null;
                case DataBytesDefinition.StringEmpty:
                    return string.Empty;
                case DataBytesDefinition.RefStringByte:
                    return _cache.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefStringByte0:
                    return _cache.DeserializerGet(0);
                case DataBytesDefinition.RefStringByte1:
                    return _cache.DeserializerGet(1);
                case DataBytesDefinition.RefStringByte2:
                    return _cache.DeserializerGet(2);
                case DataBytesDefinition.RefStringByte3:
                    return _cache.DeserializerGet(3);
                case DataBytesDefinition.RefStringByte4:
                    return _cache.DeserializerGet(4);
                case DataBytesDefinition.RefStringByte5:
                    return _cache.DeserializerGet(5);
                case DataBytesDefinition.RefStringByte6:
                    return _cache.DeserializerGet(6);
                case DataBytesDefinition.RefStringByte7:
                    return _cache.DeserializerGet(7);
                case DataBytesDefinition.RefStringByte8:
                    return _cache.DeserializerGet(8);
                case DataBytesDefinition.RefStringByte9:
                    return _cache.DeserializerGet(9);
                case DataBytesDefinition.RefStringByte10:
                    return _cache.DeserializerGet(10);
                case DataBytesDefinition.RefStringByte11:
                    return _cache.DeserializerGet(11);
                case DataBytesDefinition.RefStringByte12:
                    return _cache.DeserializerGet(12);
                case DataBytesDefinition.RefStringByte13:
                    return _cache.DeserializerGet(13);
                case DataBytesDefinition.RefStringByte14:
                    return _cache.DeserializerGet(14);
                case DataBytesDefinition.RefStringByte15:
                    return _cache.DeserializerGet(15);
                case DataBytesDefinition.RefStringUShort:
                    return _cache.DeserializerGet(reader.ReadUInt16());

                case DataBytesDefinition.RefString16Byte:
                    return _cache16.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefString16Byte0:
                    return _cache16.DeserializerGet(0);
                case DataBytesDefinition.RefString16Byte1:
                    return _cache16.DeserializerGet(1);
                case DataBytesDefinition.RefString16Byte2:
                    return _cache16.DeserializerGet(2);
                case DataBytesDefinition.RefString16Byte3:
                    return _cache16.DeserializerGet(3);
                case DataBytesDefinition.RefString16Byte4:
                    return _cache16.DeserializerGet(4);
                case DataBytesDefinition.RefString16Byte5:
                    return _cache16.DeserializerGet(5);
                case DataBytesDefinition.RefString16Byte6:
                    return _cache16.DeserializerGet(6);
                case DataBytesDefinition.RefString16Byte7:
                    return _cache16.DeserializerGet(7);
                case DataBytesDefinition.RefString16Byte8:
                    return _cache16.DeserializerGet(8);
                case DataBytesDefinition.RefString16Byte9:
                    return _cache16.DeserializerGet(9);
                case DataBytesDefinition.RefString16Byte10:
                    return _cache16.DeserializerGet(10);
                case DataBytesDefinition.RefString16Byte11:
                    return _cache16.DeserializerGet(11);
                case DataBytesDefinition.RefString16Byte12:
                    return _cache16.DeserializerGet(12);
                case DataBytesDefinition.RefString16Byte13:
                    return _cache16.DeserializerGet(13);
                case DataBytesDefinition.RefString16Byte14:
                    return _cache16.DeserializerGet(14);
                case DataBytesDefinition.RefString16Byte15:
                    return _cache16.DeserializerGet(15);
                case DataBytesDefinition.RefString16UShort:
                    return _cache16.DeserializerGet(reader.ReadUInt16());

                case DataBytesDefinition.StringLengthByte:
                    length = reader.ReadByte();
                    break;
                case DataBytesDefinition.StringLengthByte1:
                    length = 1;
                    break;
                case DataBytesDefinition.StringLengthByte2:
                    length = 2;
                    break;
                case DataBytesDefinition.StringLengthByte3:
                    length = 3;
                    break;
                case DataBytesDefinition.StringLengthByte4:
                    length = 4;
                    break;
                case DataBytesDefinition.StringLengthByte5:
                    length = 5;
                    break;
                case DataBytesDefinition.StringLengthByte6:
                    length = 6;
                    break;
                case DataBytesDefinition.StringLengthByte7:
                    length = 7;
                    break;
                case DataBytesDefinition.StringLengthByte8:
                    length = 8;
                    break;
                case DataBytesDefinition.StringLengthByte9:
                    length = 9;
                    break;
                case DataBytesDefinition.StringLengthByte10:
                    length = 10;
                    break;
                case DataBytesDefinition.StringLengthByte11:
                    length = 11;
                    break;
                case DataBytesDefinition.StringLengthByte12:
                    length = 12;
                    break;
                case DataBytesDefinition.StringLengthByte13:
                    length = 13;
                    break;
                case DataBytesDefinition.StringLengthByte14:
                    length = 14;
                    break;
                case DataBytesDefinition.StringLengthByte15:
                    length = 15;
                    break;
                case DataBytesDefinition.StringLengthByte16:
                    length = 16;
                    break;
                case DataBytesDefinition.StringLengthByte17:
                    length = 17;
                    break;
                case DataBytesDefinition.StringLengthByte18:
                    length = 18;
                    break;
                case DataBytesDefinition.StringLengthByte19:
                    length = 19;
                    break;
                case DataBytesDefinition.StringLengthByte20:
                    length = 20;
                    break;
                case DataBytesDefinition.StringLengthUShort:
                    length = reader.ReadUInt16();
                    break;
                case DataBytesDefinition.StringLengthInt:
                    length = reader.ReadInt32();
                    break;
            }
            if (length == null) throw new InvalidOperationException("Invalid type value.");

            var bytes = ArrayPool<byte>.Shared.Rent(length.Value);
            reader.Read(bytes, 0, length.Value);
            var strValue = Encoding.UTF8.GetString(bytes, 0, length.Value);
            ArrayPool<byte>.Shared.Return(bytes);
            var sLength = strValue.Length;

            if (sLength <= 2) return strValue;
            if (sLength <= 16)
                _cache16.DeserializerSet(strValue);
            else
                _cache.DeserializerSet(strValue);
            return strValue;
        }
    }
}