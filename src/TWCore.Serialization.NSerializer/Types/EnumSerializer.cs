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
    public class EnumSerializer : TypeSerializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Init()
        {
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Clear()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Stream stream, int value)
        {
            switch (value)
            {
                case -1:
                    stream.WriteByte(DataBytesDefinition.EnumSByteMinusOne);
                    return;
                case 0:
                    stream.WriteByte(DataBytesDefinition.EnumByteDefault);
                    return;
                case 1:
                    stream.WriteByte(DataBytesDefinition.EnumByte1);
                    return;
                case 2:
                    stream.WriteByte(DataBytesDefinition.EnumByte2);
                    return;
                case 3:
                    stream.WriteByte(DataBytesDefinition.EnumByte3);
                    return;
                case 4:
                    stream.WriteByte(DataBytesDefinition.EnumByte4);
                    return;
                case 5:
                    stream.WriteByte(DataBytesDefinition.EnumByte5);
                    return;
                case 6:
                    stream.WriteByte(DataBytesDefinition.EnumByte6);
                    return;
                case 7:
                    stream.WriteByte(DataBytesDefinition.EnumByte7);
                    return;
                case 8:
                    stream.WriteByte(DataBytesDefinition.EnumByte8);
                    return;
                case 9:
                    stream.WriteByte(DataBytesDefinition.EnumByte9);
                    return;
                case 10:
                    stream.WriteByte(DataBytesDefinition.EnumByte10);
                    return;
                case 11:
                    stream.WriteByte(DataBytesDefinition.EnumByte11);
                    return;
                case 12:
                    stream.WriteByte(DataBytesDefinition.EnumByte12);
                    return;
                case 13:
                    stream.WriteByte(DataBytesDefinition.EnumByte13);
                    return;
                case 14:
                    stream.WriteByte(DataBytesDefinition.EnumByte14);
                    return;
                case 15:
                    stream.WriteByte(DataBytesDefinition.EnumByte15);
                    return;
                case 16:
                    stream.WriteByte(DataBytesDefinition.EnumByte16);
                    return;
                default:
                    if (value <= byte.MaxValue)
                        WriteByte(stream, DataBytesDefinition.EnumByte, (byte)value);
                    else if (value <= ushort.MaxValue)
                        WriteUshort(stream, DataBytesDefinition.EnumUShort, (ushort)value);
                    else
                        WriteInt(stream, DataBytesDefinition.EnumInt, value);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Stream stream, int? value)
        {
            if (value == null) stream.WriteByte(DataBytesDefinition.ValueNull);
            else Write(stream, value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(BinaryReader reader)
            => ReadNullable(reader) ?? 0;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int? ReadNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.EnumSByteMinusOne:
                    return -1;
                case DataBytesDefinition.EnumByteDefault:
                    return default(int);
                case DataBytesDefinition.EnumByte1:
                    return 1;
                case DataBytesDefinition.EnumByte2:
                    return 2;
                case DataBytesDefinition.EnumByte3:
                    return 3;
                case DataBytesDefinition.EnumByte4:
                    return 4;
                case DataBytesDefinition.EnumByte5:
                    return 5;
                case DataBytesDefinition.EnumByte6:
                    return 6;
                case DataBytesDefinition.EnumByte7:
                    return 7;
                case DataBytesDefinition.EnumByte8:
                    return 8;
                case DataBytesDefinition.EnumByte9:
                    return 9;
                case DataBytesDefinition.EnumByte10:
                    return 10;
                case DataBytesDefinition.EnumByte11:
                    return 11;
                case DataBytesDefinition.EnumByte12:
                    return 12;
                case DataBytesDefinition.EnumByte13:
                    return 13;
                case DataBytesDefinition.EnumByte14:
                    return 14;
                case DataBytesDefinition.EnumByte15:
                    return 15;
                case DataBytesDefinition.EnumByte16:
                    return 16;
                case DataBytesDefinition.EnumByte:
                    return reader.ReadByte();
                case DataBytesDefinition.EnumUShort:
                    return reader.ReadUInt16();
                case DataBytesDefinition.EnumInt:
                    return reader.ReadInt32();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}