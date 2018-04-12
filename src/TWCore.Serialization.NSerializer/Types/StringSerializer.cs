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

namespace TWCore.Serialization.NSerializer
{
    public partial class SerializersTable
    {
        private SerializerStringCache _stringCache8;
        private SerializerStringCache _stringCache16;
        private SerializerStringCache _stringCache32;
        private SerializerStringCache _stringCache64;
        private SerializerStringCache _stringCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitString()
        {
            _stringCache8 = new SerializerStringCache();
            _stringCache16 = new SerializerStringCache();
            _stringCache32 = new SerializerStringCache();
            _stringCache64 = new SerializerStringCache();
            _stringCache = new SerializerStringCache();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearString()
        {
            _stringCache8.Clear();
            _stringCache16.Clear();
            _stringCache32.Clear();
            _stringCache64.Clear();
            _stringCache.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(string value)
        {
            if (value == null)
            {
                _stream.WriteByte(DataBytesDefinition.StringNull);
                return;
            }
            if (value == string.Empty)
            {
                _stream.WriteByte(DataBytesDefinition.StringEmpty);
                return;
            }

            var vLength = value.Length;
            if (vLength > 2)
            {
                if (vLength <= 8)
                {
                    var objIdx = _stringCache8.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteByte(DataBytesDefinition.RefString8Byte, (byte)objIdx);
                        else
                            WriteUshort(DataBytesDefinition.RefString8UShort, (ushort)objIdx);
                        return;
                    }
                    _stringCache8.SerializerSet(value);
                }
                else if (vLength <= 16)
                {
                    var objIdx = _stringCache16.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteByte(DataBytesDefinition.RefString16Byte, (byte)objIdx);
                        else
                            WriteUshort(DataBytesDefinition.RefString16UShort, (ushort)objIdx);
                        return;
                    }
                    _stringCache16.SerializerSet(value);
                }
                else if (vLength <= 32)
                {
                    var objIdx = _stringCache32.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteByte(DataBytesDefinition.RefString32Byte, (byte)objIdx);
                        else
                            WriteUshort(DataBytesDefinition.RefString32UShort, (ushort)objIdx);
                        return;
                    }
                    _stringCache32.SerializerSet(value);
                }
                else if (vLength <= 64)
                {
                    var objIdx = _stringCache64.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteByte(DataBytesDefinition.RefString64Byte, (byte)objIdx);
                        else
                            WriteUshort(DataBytesDefinition.RefString64UShort, (ushort)objIdx);
                        return;
                    }
                    _stringCache64.SerializerSet(value);
                }
                else
                {
                    var objIdx = _stringCache.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteByte(DataBytesDefinition.RefStringByte, (byte)objIdx);
                        else
                            WriteUshort(DataBytesDefinition.RefStringUShort, (ushort)objIdx);
                        return;
                    }
                    _stringCache.SerializerSet(value);
                }
            }

            var length = Encoding.Unicode.GetByteCount(value);
            int bytesLength;
            byte[] bytes;

            if (length < 21)
            {
                bytesLength = length + 1;
                bytes = new byte[bytesLength];
                bytes[0] = (byte)(DataBytesDefinition.StringLengthByte1 + (length - 1));
                Encoding.UTF8.GetBytes(value, 0, value.Length, bytes, 1);
            }
            else if (length <= byte.MaxValue)
            {
                bytesLength = length + 2;
                bytes = new byte[bytesLength];
                bytes[0] = DataBytesDefinition.StringLengthByte;
                bytes[1] = (byte)length;
                Encoding.UTF8.GetBytes(value, 0, value.Length, bytes, 2);
            }
            else if (length <= ushort.MaxValue)
            {
                bytesLength = length + 3;
                bytes = new byte[bytesLength];
                bytes[0] = DataBytesDefinition.StringLengthUShort;
                bytes[1] = (byte)(ushort)length;
                bytes[2] = (byte)(((ushort)length) >> 8);
                Encoding.UTF8.GetBytes(value, 0, value.Length, bytes, 3);
            }
            else
            {
                bytesLength = length + 5;
                bytes = new byte[bytesLength];
                bytes[0] = DataBytesDefinition.StringLengthInt;
                bytes[1] = (byte)length;
                bytes[2] = (byte)(length >> 8);
                bytes[3] = (byte)(length >> 16);
                bytes[4] = (byte)(length >> 24);
                Encoding.UTF8.GetBytes(value, 0, value.Length, bytes, 5);
            }
            _stream.Write(bytes, 0, bytesLength);
        }
    }

    public partial class DeserializersTable
    {
        private SerializerStringCache _stringCache8;
        private SerializerStringCache _stringCache16;
        private SerializerStringCache _stringCache32;
        private SerializerStringCache _stringCache64;
        private SerializerStringCache _stringCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitString()
        {
            _stringCache8 = new SerializerStringCache();
            _stringCache16 = new SerializerStringCache();
            _stringCache32 = new SerializerStringCache();
            _stringCache64 = new SerializerStringCache();
            _stringCache = new SerializerStringCache();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearString()
        {
            _stringCache8.Clear();
            _stringCache16.Clear();
            _stringCache32.Clear();
            _stringCache64.Clear();
            _stringCache.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString(BinaryReader reader)
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
                    return _stringCache.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefStringUShort:
                    return _stringCache.DeserializerGet(reader.ReadUInt16());

                case DataBytesDefinition.RefString8Byte:
                    return _stringCache8.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefString8UShort:
                    return _stringCache8.DeserializerGet(reader.ReadUInt16());

                case DataBytesDefinition.RefString16Byte:
                    return _stringCache16.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefString16UShort:
                    return _stringCache16.DeserializerGet(reader.ReadUInt16());

                case DataBytesDefinition.RefString32Byte:
                    return _stringCache32.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefString32UShort:
                    return _stringCache32.DeserializerGet(reader.ReadUInt16());

                case DataBytesDefinition.RefString64Byte:
                    return _stringCache64.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefString64UShort:
                    return _stringCache64.DeserializerGet(reader.ReadUInt16());

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
                _stringCache16.DeserializerSet(strValue);
            else
                _stringCache.DeserializerSet(strValue);
            return strValue;
        }
    }
}