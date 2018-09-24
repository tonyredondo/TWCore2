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
#pragma warning disable 1591

namespace TWCore.Serialization.RawSerializer
{
    public partial class SerializersTable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(string value)
        {
            if (value is null)
            {
                WriteByte(DataBytesDefinition.StringNull);
                return;
            }
            if (value == string.Empty)
            {
                WriteByte(DataBytesDefinition.StringEmpty);
                return;
            }

#if NETSTANDARD2_0
            var length = Encoding.UTF8.GetByteCount(value);

            var bufferLength = length + 5;
            var buffer = ArrayPool<byte>.Shared.Rent(bufferLength);
            var bufferSpan = buffer.AsSpan(0, bufferLength);
            buffer[0] = DataBytesDefinition.StringLength;
            BitConverter.GetBytes(length).CopyTo(bufferSpan.Slice(1, 4));
            Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 5);
            Stream.Write(buffer, 0, bufferLength);
            ArrayPool<byte>.Shared.Return(buffer);
#else
            var length = Encoding.UTF8.GetByteCount(value);
            if (length <= 16384)
            {
                Span<byte> bufferSpan = stackalloc byte[length + 5];
                bufferSpan[0] = DataBytesDefinition.StringLength;
                BitConverter.TryWriteBytes(bufferSpan.Slice(1, 4), length);
                Encoding.UTF8.GetBytes(value, bufferSpan.Slice(5));
                Stream.Write(bufferSpan);
            }
            else
            {
                var buffer = ArrayPool<byte>.Shared.Rent(length + 5);
                var bufferSpan = buffer.AsSpan(0, length + 5);
                buffer[0] = DataBytesDefinition.StringLength;
                BitConverter.TryWriteBytes(bufferSpan.Slice(1, 4), length);
                Encoding.UTF8.GetBytes(value, bufferSpan.Slice(5));
                Stream.Write(bufferSpan);
                ArrayPool<byte>.Shared.Return(buffer);
            }
#endif
        }
    }



    public partial class DeserializersTable
    {
        [DeserializerMethod(
            DataBytesDefinition.StringNull,
            DataBytesDefinition.StringEmpty,
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
                case DataBytesDefinition.StringLength:
                    length = StreamReadInt();
                    break;
                default:
                    throw new InvalidOperationException("Invalid type value.");
            }

            string strValue = null;

#if NETSTANDARD2_0
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                Stream.ReadExact(buffer, 0, length);
                strValue = Encoding.UTF8.GetString(buffer, 0, length);
            }
            catch
            {
                throw;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
#else
            if (length <= 16384)
            {
                Span<byte> bufferSpan = stackalloc byte[length];
                Stream.Fill(bufferSpan);
                strValue = Encoding.UTF8.GetString(bufferSpan);
            }
            else
            {
                var buffer = ArrayPool<byte>.Shared.Rent(length);
                try
                {
                    Stream.ReadExact(buffer, 0, length);
                    strValue = Encoding.UTF8.GetString(buffer, 0, length);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
#endif

            return strValue;
        }
    }
}