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
        public void Write(BinaryWriter writer, byte[] value)
        {
            if (value == null)
            {
                writer.Write(DataBytesDefinition.ByteArrayNull);
                return;
            }
            if (value.Length == 0)
            {
                writer.Write(DataBytesDefinition.ByteArrayEmpty);
                return;
            }

            #region Write Array
            var length = value.Length;
            if (length <= byte.MaxValue)
                WriteByte(writer, DataBytesDefinition.ByteArrayLengthByte, (byte)length);
            else if (length <= ushort.MaxValue)
                WriteUshort(writer, DataBytesDefinition.ByteArrayLengthUShort, (ushort)length);
            else
                WriteInt(writer, DataBytesDefinition.ByteArrayLengthInt, length);
            writer.Write(value);
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