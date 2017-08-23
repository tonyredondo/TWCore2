/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.WSerializer.Types.ValueTypes
{
    /// <summary>
    /// Enum value type serializer
    /// </summary>
	public class EnumSerializer : TypeSerializer<int>
    {
        public static HashSet<byte> ReadTypes = new HashSet<byte>(new []
        {
            DataType.EnumByte, DataType.EnumByteDefault, DataType.EnumByte1, DataType.EnumByte2, DataType.EnumByte3, DataType.EnumByte4, DataType.EnumByte5, DataType.EnumByte6, DataType.EnumByte7,
            DataType.EnumByte8, DataType.EnumByte9, DataType.EnumByte10, DataType.EnumByte11, DataType.EnumByte12, DataType.EnumByte13, DataType.EnumByte14, DataType.EnumByte15,
            DataType.EnumByte16, DataType.EnumUShort, DataType.EnumInt, DataType.EnumSByteMinusOne
        });
        /// <summary>
        /// Gets if the type serializer can write the type
        /// </summary>
        /// <param name="type">Type of the value to write</param>
        /// <returns>true if the type serializer can write the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanWrite(Type type) 
            => type.GetTypeInfo().IsEnum;
        /// <summary>
        /// Gets if the type serializer can read the data type
        /// </summary>
        /// <param name="type">DataType value</param>
        /// <returns>true if the type serializer can read the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanRead(byte type) 
            => ReadTypes.Contains(type);
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(BinaryWriter writer, object value)
        {
            var iValue = Factory.Converter.ToInt(value);
            switch(iValue)
            {
                case -1:
                    writer.Write(DataType.EnumSByteMinusOne);
                    return;
                case 0:
                    writer.Write(DataType.EnumByteDefault);
                    return;
                case 1:
                    writer.Write(DataType.EnumByte1);
                    return;
                case 2:
                    writer.Write(DataType.EnumByte2);
                    return;
                case 3:
                    writer.Write(DataType.EnumByte3);
                    return;
                case 4:
                    writer.Write(DataType.EnumByte4);
                    return;
                case 5:
                    writer.Write(DataType.EnumByte5);
                    return;
                case 6:
                    writer.Write(DataType.EnumByte6);
                    return;
                case 7:
                    writer.Write(DataType.EnumByte7);
                    return;
                case 8:
                    writer.Write(DataType.EnumByte8);
                    return;
                case 9:
                    writer.Write(DataType.EnumByte9);
                    return;
                case 10:
                    writer.Write(DataType.EnumByte10);
                    return;
                case 11:
                    writer.Write(DataType.EnumByte11);
                    return;
                case 12:
                    writer.Write(DataType.EnumByte12);
                    return;
                case 13:
                    writer.Write(DataType.EnumByte13);
                    return;
                case 14:
                    writer.Write(DataType.EnumByte14);
                    return;
                case 15:
                    writer.Write(DataType.EnumByte15);
                    return;
                case 16:
                    writer.Write(DataType.EnumByte16);
                    return;
                default:
                    if (iValue <= byte.MaxValue)
                        WriteByte(writer, DataType.EnumByte, (byte) iValue);
                    else if (iValue <= ushort.MaxValue)
                        WriteUshort(writer, DataType.EnumUShort, (ushort) iValue);
                    else
                        WriteInt(writer, DataType.EnumInt, iValue);
                    break;
            }
        }
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteValue(BinaryWriter writer, int value)
        {
            switch (value)
            {
                case -1:
                    writer.Write(DataType.EnumSByteMinusOne);
                    return;
                case 0:
                    writer.Write(DataType.EnumByteDefault);
                    return;
                case 1:
                    writer.Write(DataType.EnumByte1);
                    return;
                case 2:
                    writer.Write(DataType.EnumByte2);
                    return;
                case 3:
                    writer.Write(DataType.EnumByte3);
                    return;
                case 4:
                    writer.Write(DataType.EnumByte4);
                    return;
                case 5:
                    writer.Write(DataType.EnumByte5);
                    return;
                case 6:
                    writer.Write(DataType.EnumByte6);
                    return;
                case 7:
                    writer.Write(DataType.EnumByte7);
                    return;
                case 8:
                    writer.Write(DataType.EnumByte8);
                    return;
                case 9:
                    writer.Write(DataType.EnumByte9);
                    return;
                case 10:
                    writer.Write(DataType.EnumByte10);
                    return;
                case 11:
                    writer.Write(DataType.EnumByte11);
                    return;
                case 12:
                    writer.Write(DataType.EnumByte12);
                    return;
                case 13:
                    writer.Write(DataType.EnumByte13);
                    return;
                case 14:
                    writer.Write(DataType.EnumByte14);
                    return;
                case 15:
                    writer.Write(DataType.EnumByte15);
                    return;
                case 16:
                    writer.Write(DataType.EnumByte16);
                    return;
                default:
                    if (value <= byte.MaxValue)
                        WriteByte(writer, DataType.EnumByte, (byte)value);
                    else if (value <= ushort.MaxValue)
                        WriteUshort(writer, DataType.EnumUShort, (ushort)value);
                    else
                        WriteInt(writer, DataType.EnumInt, value);
                    break;
            }
        }

        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Read(BinaryReader reader, byte type)
        {
            var intValue = 0;
            switch (type)
            {
                case DataType.EnumSByteMinusOne:
                    intValue = -1;
                    break;
                case DataType.EnumByteDefault:
                    intValue = default(int);
                    break;
                case DataType.EnumByte1:
                    intValue = 1;
                    break;
                case DataType.EnumByte2:
                    intValue = 2;
                    break;
                case DataType.EnumByte3:
                    intValue = 3;
                    break;
                case DataType.EnumByte4:
                    intValue = 4;
                    break;
                case DataType.EnumByte5:
                    intValue = 5;
                    break;
                case DataType.EnumByte6:
                    intValue = 6;
                    break;
                case DataType.EnumByte7:
                    intValue = 7;
                    break;
                case DataType.EnumByte8:
                    intValue = 8;
                    break;
                case DataType.EnumByte9:
                    intValue = 9;
                    break;
                case DataType.EnumByte10:
                    intValue = 10;
                    break;
                case DataType.EnumByte11:
                    intValue = 11;
                    break;
                case DataType.EnumByte12:
                    intValue = 12;
                    break;
                case DataType.EnumByte13:
                    intValue = 13;
                    break;
                case DataType.EnumByte14:
                    intValue = 14;
                    break;
                case DataType.EnumByte15:
                    intValue = 15;
                    break;
                case DataType.EnumByte16:
                    intValue = 16;
                    break;
                case DataType.EnumByte:
                    intValue = reader.ReadByte();
                    break;
                case DataType.EnumUShort:
                    intValue = reader.ReadUInt16();
                    break;
                case DataType.EnumInt:
                    intValue = reader.ReadInt32();
                    break;
            }
            //return declaredType.GetTypeInfo().IsEnum ? Enum.ToObject(declaredType, intValue) : intValue;
            return intValue;
        }
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadValue(BinaryReader reader, byte type)
        {
            var intValue = 0;
            switch (type)
            {
                case DataType.EnumSByteMinusOne:
                    intValue = -1;
                    break;
                case DataType.EnumByteDefault:
                    intValue = default(int);
                    break;
                case DataType.EnumByte1:
                    intValue = 1;
                    break;
                case DataType.EnumByte2:
                    intValue = 2;
                    break;
                case DataType.EnumByte3:
                    intValue = 3;
                    break;
                case DataType.EnumByte4:
                    intValue = 4;
                    break;
                case DataType.EnumByte5:
                    intValue = 5;
                    break;
                case DataType.EnumByte6:
                    intValue = 6;
                    break;
                case DataType.EnumByte7:
                    intValue = 7;
                    break;
                case DataType.EnumByte8:
                    intValue = 8;
                    break;
                case DataType.EnumByte9:
                    intValue = 9;
                    break;
                case DataType.EnumByte10:
                    intValue = 10;
                    break;
                case DataType.EnumByte11:
                    intValue = 11;
                    break;
                case DataType.EnumByte12:
                    intValue = 12;
                    break;
                case DataType.EnumByte13:
                    intValue = 13;
                    break;
                case DataType.EnumByte14:
                    intValue = 14;
                    break;
                case DataType.EnumByte15:
                    intValue = 15;
                    break;
                case DataType.EnumByte16:
                    intValue = 16;
                    break;
                case DataType.EnumByte:
                    intValue = reader.ReadByte();
                    break;
                case DataType.EnumUShort:
                    intValue = reader.ReadUInt16();
                    break;
                case DataType.EnumInt:
                    intValue = reader.ReadInt32();
                    break;
            }
            return intValue;
        }
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadValue(BinaryReader reader)
            => ReadValue(reader, reader.ReadByte());
    }
}
