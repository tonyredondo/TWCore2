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
    public struct EnumSerializer : ITypeSerializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter writer, int value)
        {
            switch (value)
            {
                case -1:
                    writer.Write(DataBytesDefinition.EnumSByteMinusOne);
                    return;
                case 0:
                    writer.Write(DataBytesDefinition.EnumByteDefault);
                    return;
                case 1:
                    writer.Write(DataBytesDefinition.EnumByte1);
                    return;
                case 2:
                    writer.Write(DataBytesDefinition.EnumByte2);
                    return;
                case 3:
                    writer.Write(DataBytesDefinition.EnumByte3);
                    return;
                case 4:
                    writer.Write(DataBytesDefinition.EnumByte4);
                    return;
                case 5:
                    writer.Write(DataBytesDefinition.EnumByte5);
                    return;
                case 6:
                    writer.Write(DataBytesDefinition.EnumByte6);
                    return;
                case 7:
                    writer.Write(DataBytesDefinition.EnumByte7);
                    return;
                case 8:
                    writer.Write(DataBytesDefinition.EnumByte8);
                    return;
                case 9:
                    writer.Write(DataBytesDefinition.EnumByte9);
                    return;
                case 10:
                    writer.Write(DataBytesDefinition.EnumByte10);
                    return;
                case 11:
                    writer.Write(DataBytesDefinition.EnumByte11);
                    return;
                case 12:
                    writer.Write(DataBytesDefinition.EnumByte12);
                    return;
                case 13:
                    writer.Write(DataBytesDefinition.EnumByte13);
                    return;
                case 14:
                    writer.Write(DataBytesDefinition.EnumByte14);
                    return;
                case 15:
                    writer.Write(DataBytesDefinition.EnumByte15);
                    return;
                case 16:
                    writer.Write(DataBytesDefinition.EnumByte16);
                    return;
                default:
                    if (value <= byte.MaxValue)
                        WriteHelper.WriteByte(writer, DataBytesDefinition.EnumByte, (byte)value);
                    else if (value <= ushort.MaxValue)
                        WriteHelper.WriteUshort(writer, DataBytesDefinition.EnumUShort, (ushort)value);
                    else
                        WriteHelper.WriteInt(writer, DataBytesDefinition.EnumInt, value);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter writer, int? value)
        {
            if (value == null) writer.Write(DataBytesDefinition.ValueNull);
            else Write(writer, value.Value);
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