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
using System.Runtime.CompilerServices;
using System.Text;

namespace TWCore.Serialization.NSerializer
{
    public partial class SerializersTable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(string value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.StringNull);
                return;
            }
            if (value == string.Empty)
            {
                WriteByte(DataBytesDefinition.StringEmpty);
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
            if (length < 1024)
            {
                Span<byte> span = stackalloc byte[length + 5];
                span[0] = DataBytesDefinition.StringLength;
                span[1] = (byte)length;
                span[2] = (byte)(length >> 8);
                span[3] = (byte)(length >> 16);
                span[4] = (byte)(length >> 24);
                Encoding.UTF8.GetBytes(value, span.Slice(5, length));
                Stream.Write(span);
            }
            else
            {
                using (var buffer = MemoryPool<byte>.Shared.Rent(minBufferSize: length + 5))
                {
                    var span = buffer.Memory.Span.Slice(0, length + 5);
                    span[0] = DataBytesDefinition.StringLength;
                    span[1] = (byte) length;
                    span[2] = (byte) (length >> 8);
                    span[3] = (byte) (length >> 16);
                    span[4] = (byte) (length >> 24);
                    Encoding.UTF8.GetBytes(value, span.Slice(5, length));
                    Stream.Write(span);
                }
            }
        }
    }



    public partial class DeserializersTable
    {
        [DeserializerMethod(
            DataBytesDefinition.StringNull, 
            DataBytesDefinition.StringEmpty, 
            DataBytesDefinition.RefString, 
            DataBytesDefinition.RefString8, 
            DataBytesDefinition.RefString16, 
            DataBytesDefinition.RefString32, 
            DataBytesDefinition.StringLength, ReturnType = typeof(string))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString(byte type)
        {
            int length;
            switch (type)
            {
                case DataBytesDefinition.StringNull:
                    return null;
                case DataBytesDefinition.StringEmpty:
                    return string.Empty;
                case DataBytesDefinition.RefString:
                    return _stringCache.Get(StreamReadInt());
                case DataBytesDefinition.RefString8:
                    return _stringCache8.Get(StreamReadInt());
                case DataBytesDefinition.RefString16:
                    return _stringCache16.Get(StreamReadInt());
                case DataBytesDefinition.RefString32:
                    return _stringCache32.Get(StreamReadInt());
                case DataBytesDefinition.StringLength:
                    length = StreamReadInt();
                    break;
                default:
                    throw new InvalidOperationException("Invalid type value.");
            }

            string strValue;
            if (length < 1024)
            {
                Span<byte> bytes = stackalloc byte[length];
                Stream.Read(bytes);
                strValue = Encoding.UTF8.GetString(bytes);
            }
            else
            {
                using (var buffer = MemoryPool<byte>.Shared.Rent(minBufferSize: length))
                {
                    Span<byte> bytes = buffer.Memory.Span.Slice(0, length);
                    Stream.Read(bytes);
                    strValue = Encoding.UTF8.GetString(bytes);
                }
            }

            var sLength = strValue.Length;

            if (sLength <= 2) return strValue;
            if (sLength <= 8)
                _stringCache8.Set(strValue);
            else if (sLength <= 16)
                _stringCache16.Set(strValue);
            else if (sLength <= 32)
                _stringCache32.Set(strValue);
            else
                _stringCache.Set(strValue);
            return strValue;
        }
    }
}