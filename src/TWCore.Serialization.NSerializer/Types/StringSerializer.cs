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
                Stream.WriteByte(DataBytesDefinition.StringNull);
                return;
            }
            if (value == string.Empty)
            {
                Stream.WriteByte(DataBytesDefinition.StringEmpty);
                return;
            }

            var vLength = value.Length;
            if (vLength > 2)
            {
                if (vLength <= 8)
                {
                    if (_stringCache8.TryGetValue(value, out var objIdx))
                    {
                        WriteDefInt(DataBytesDefinition.RefString8, objIdx);
                        return;
                    }
                    _stringCache8.Set(value);
                }
                else if (vLength <= 16)
                {
                    if (_stringCache16.TryGetValue(value, out var objIdx))
                    {
                        WriteDefInt(DataBytesDefinition.RefString16, objIdx);
                        return;
                    }
                    _stringCache16.Set(value);
                }
                else if (vLength <= 32)
                {
                    if (_stringCache32.TryGetValue(value, out var objIdx))
                    {
                        WriteDefInt(DataBytesDefinition.RefString32, objIdx);
                        return;
                    }
                    _stringCache32.Set(value);
                }
                else if (vLength <= 64)
                {
                    if (_stringCache64.TryGetValue(value, out var objIdx))
                    {
                        WriteDefInt(DataBytesDefinition.RefString64, objIdx);
                        return;
                    }
                    _stringCache64.Set(value);
                }
                else
                {
                    if (_stringCache.TryGetValue(value, out var objIdx))
                    {
                        WriteDefInt(DataBytesDefinition.RefString, objIdx);
                        return;
                    }
                    _stringCache.Set(value);
                }
            }

            var length = Encoding.UTF8.GetByteCount(value);
            var bytes = new byte[length + 5];
            bytes[0] = DataBytesDefinition.StringLength;
            bytes[1] = (byte)length;
            bytes[2] = (byte)(length >> 8);
            bytes[3] = (byte)(length >> 16);
            bytes[4] = (byte)(length >> 24);
            Encoding.UTF8.GetBytes(value, 0, value.Length, bytes, 5);
            Stream.Write(bytes, 0, bytes.Length);
        }
    }

    public partial class DeserializersTable
    {
        private DeserializerStringCache _stringCache8;
        private DeserializerStringCache _stringCache16;
        private DeserializerStringCache _stringCache32;
        private DeserializerStringCache _stringCache64;
        private DeserializerStringCache _stringCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitString()
        {
            _stringCache8 = new DeserializerStringCache();
            _stringCache16 = new DeserializerStringCache();
            _stringCache32 = new DeserializerStringCache();
            _stringCache64 = new DeserializerStringCache();
            _stringCache = new DeserializerStringCache();
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
            int length;
            switch (type)
            {
                case DataBytesDefinition.StringNull:
                    return null;
                case DataBytesDefinition.StringEmpty:
                    return string.Empty;
                case DataBytesDefinition.RefString:
                    return _stringCache.Get(reader.ReadInt32());
                case DataBytesDefinition.RefString8:
                    return _stringCache8.Get(reader.ReadInt32());
                case DataBytesDefinition.RefString16:
                    return _stringCache16.Get(reader.ReadInt32());
                case DataBytesDefinition.RefString32:
                    return _stringCache32.Get(reader.ReadInt32());
                case DataBytesDefinition.RefString64:
                    return _stringCache64.Get(reader.ReadInt32());
                case DataBytesDefinition.StringLength:
                    length = reader.ReadInt32();
                    break;
                default:
                    throw new InvalidOperationException("Invalid type value.");
            }

            var bytes = new byte[length];
            reader.Read(bytes, 0, length);
            var strValue = Encoding.UTF8.GetString(bytes, 0, length);
            var sLength = strValue.Length;

            if (sLength <= 2) return strValue;
            if (sLength <= 8)
                _stringCache8.Set(strValue);
            else if (sLength <= 16)
                _stringCache16.Set(strValue);
            else if (sLength <= 32)
                _stringCache32.Set(strValue);
            else if (sLength <= 64)
                _stringCache64.Set(strValue);
            else
                _stringCache.Set(strValue);
            return strValue;
        }
    }
}