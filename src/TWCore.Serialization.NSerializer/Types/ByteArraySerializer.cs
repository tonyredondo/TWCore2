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
using System.IO;
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.NSerializer.Types
{
    public class ByteArraySerializer : TypeSerializer
    {
        private static readonly byte[] EmptyBytes = new byte[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Init()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Clear()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Stream stream, byte[] value)
        {
            if (value == null)
            {
                stream.WriteByte(DataBytesDefinition.ByteArrayNull);
                return;
            }
            if (value.Length == 0)
            {
                stream.WriteByte(DataBytesDefinition.ByteArrayEmpty);
                return;
            }

            #region Write Array
            var length = value.Length;
            if (length <= byte.MaxValue)
                WriteByte(stream, DataBytesDefinition.ByteArrayLengthByte, (byte)length);
            else if (length <= ushort.MaxValue)
                WriteUshort(stream, DataBytesDefinition.ByteArrayLengthUShort, (ushort)length);
            else
                WriteInt(stream, DataBytesDefinition.ByteArrayLengthInt, length);
            stream.Write(value, 0, value.Length);
            #endregion
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Read(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ByteArrayNull:
                    return null;
                case DataBytesDefinition.ByteArrayEmpty:
                    return EmptyBytes;
                case DataBytesDefinition.ByteArrayLengthByte:
                    return reader.ReadBytes(reader.ReadByte());
                case DataBytesDefinition.ByteArrayLengthUShort:
                    return reader.ReadBytes(reader.ReadUInt16());
                case DataBytesDefinition.ByteArrayLengthInt:
                    return reader.ReadBytes(reader.ReadInt32());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}