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
            var intValue = 0;
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.EnumSByteMinusOne:
                    intValue = -1;
                    break;
                case DataBytesDefinition.EnumByteDefault:
                    intValue = default;
                    break;
                case DataBytesDefinition.EnumByte1:
                    intValue = 1;
                    break;
                case DataBytesDefinition.EnumByte2:
                    intValue = 2;
                    break;
                case DataBytesDefinition.EnumByte3:
                    intValue = 3;
                    break;
                case DataBytesDefinition.EnumByte4:
                    intValue = 4;
                    break;
                case DataBytesDefinition.EnumByte5:
                    intValue = 5;
                    break;
                case DataBytesDefinition.EnumByte6:
                    intValue = 6;
                    break;
                case DataBytesDefinition.EnumByte7:
                    intValue = 7;
                    break;
                case DataBytesDefinition.EnumByte8:
                    intValue = 8;
                    break;
                case DataBytesDefinition.EnumByte9:
                    intValue = 9;
                    break;
                case DataBytesDefinition.EnumByte10:
                    intValue = 10;
                    break;
                case DataBytesDefinition.EnumByte11:
                    intValue = 11;
                    break;
                case DataBytesDefinition.EnumByte12:
                    intValue = 12;
                    break;
                case DataBytesDefinition.EnumByte13:
                    intValue = 13;
                    break;
                case DataBytesDefinition.EnumByte14:
                    intValue = 14;
                    break;
                case DataBytesDefinition.EnumByte15:
                    intValue = 15;
                    break;
                case DataBytesDefinition.EnumByte16:
                    intValue = 16;
                    break;
                case DataBytesDefinition.EnumByte:
                    intValue = reader.ReadByte();
                    break;
                case DataBytesDefinition.EnumUShort:
                    intValue = reader.ReadUInt16();
                    break;
                case DataBytesDefinition.EnumInt:
                    intValue = reader.ReadInt32();
                    break;
            }
            return intValue;
        }
    }
}