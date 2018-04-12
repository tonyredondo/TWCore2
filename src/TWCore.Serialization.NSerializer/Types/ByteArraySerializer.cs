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

namespace TWCore.Serialization.NSerializer
{
    public partial class SerializersTable
    {
        private static readonly byte[] EmptyBytes = new byte[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte[] value)
        {
            if (value == null)
            {
                WriteByte(DataBytesDefinition.ByteArrayNull);
                return;
            }
            var length = value.Length;
            if (length == 0)
            {
                WriteByte(DataBytesDefinition.ByteArrayEmpty);
                return;
            }
            WriteDefInt(DataBytesDefinition.ByteArrayLength, length);
            Stream.Write(value, 0, length);
        }
    }


    public partial class DeserializersTable
    {
        private static readonly byte[] EmptyBytes = new byte[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadByteArray(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ByteArrayNull:
                    return null;
                case DataBytesDefinition.ByteArrayEmpty:
                    return EmptyBytes;
                case DataBytesDefinition.ByteArrayLength:
                    return reader.ReadBytes(reader.ReadInt32());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}