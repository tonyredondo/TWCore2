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
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.WSerializer.Types.ValueTypes
{
    /// <summary>
    /// Int value type serializer
    /// </summary>
	public class NumberSerializer : TypeSerializer<int>
    {
        public static HashSet<byte> ReadTypes = new HashSet<byte>(new[]
        {
            DataType.Decimal, DataType.DecimalDefault, DataType.RefDecimalByte, DataType.RefDecimalUShort,
            DataType.Double, DataType.DoubleDefault, DataType.RefDoubleByte, DataType.RefDoubleUShort,
            DataType.Float, DataType.FloatDefault, DataType.RefFloatByte, DataType.RefFloatUShort,
            DataType.Long, DataType.RefLongByte, DataType.RefLongUShort,
            DataType.ULong, DataType.RefULongByte, DataType.RefULongUShort,
            DataType.Int, DataType.RefIntByte, DataType.RefIntUShort,
            DataType.UInt, DataType.RefUIntByte, DataType.RefUIntUShort,
            DataType.Short, DataType.RefShortByte,
            DataType.UShort, DataType.RefUShortByte,
            DataType.SByte, DataType.SByteMinusOne,
            DataType.Byte, DataType.ByteDefault,
            DataType.Byte1, DataType.Byte2, DataType.Byte3, DataType.Byte4, DataType.Byte5, DataType.Byte6, DataType.Byte7, DataType.Byte8,
            DataType.Byte9, DataType.Byte10, DataType.Byte11, DataType.Byte12, DataType.Byte13, DataType.Byte14, DataType.Byte15, DataType.Byte16
        });

        SerializerMode _mode;

        #region Decimal Cache
        SerializerCache<decimal> decimalCache;
        SerializerCache<decimal> DecimalCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return decimalCache ?? (decimalCache = new SerializerCache<decimal>(_mode)); }
        }
        #endregion

        #region Double Cache
        SerializerCache<double> doubleCache;
        SerializerCache<double> DoubleCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return doubleCache ?? (doubleCache = new SerializerCache<double>(_mode)); }
        }
        #endregion

        #region Float Cache
        SerializerCache<float> floatCache;
        SerializerCache<float> FloatCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return floatCache ?? (floatCache = new SerializerCache<float>(_mode)); }
        }
        #endregion

        #region Long Cache
        SerializerCache<long> longCache;
        SerializerCache<long> LongCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return longCache ?? (longCache = new SerializerCache<long>(_mode)); }
        }
        #endregion

        #region ULong Cache
        SerializerCache<ulong> uLongCache;
        SerializerCache<ulong> ULongCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return uLongCache ?? (uLongCache = new SerializerCache<ulong>(_mode)); }
        }
        #endregion

        #region Int Cache
        SerializerCache<int> intCache;
        SerializerCache<int> IntCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return intCache ?? (intCache = new SerializerCache<int>(_mode)); }
        }
        #endregion

        #region UInt Cache
        SerializerCache<uint> uIntCache;
        SerializerCache<uint> UIntCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return uIntCache ?? (uIntCache = new SerializerCache<uint>(_mode)); }
        }
        #endregion

        #region Short Cache
        SerializerCache<short> shortCache;
        SerializerCache<short> ShortCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return shortCache ?? (shortCache = new SerializerCache<short>(_mode)); }
        }
        #endregion

        #region UShort Cache
        SerializerCache<ushort> uShortCache;
        SerializerCache<ushort> UShortCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return uShortCache ?? (uShortCache = new SerializerCache<ushort>(_mode)); }
        }
        #endregion

        /// <summary>
        /// Type serializer initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Init(SerializerMode mode)
        {
            _mode = mode;
            if (decimalCache != null) decimalCache = new SerializerCache<decimal>(mode);
            if (doubleCache != null) doubleCache = new SerializerCache<double>(mode);
            if (floatCache != null) floatCache = new SerializerCache<float>(mode);
            if (longCache != null) longCache = new SerializerCache<long>(mode);
            if (uLongCache != null) uLongCache = new SerializerCache<ulong>(mode);
            if (intCache != null) intCache = new SerializerCache<int>(mode);
            if (uIntCache != null) uIntCache = new SerializerCache<uint>(mode);
            if (shortCache != null) shortCache = new SerializerCache<short>(mode);
            if (uShortCache != null) uShortCache = new SerializerCache<ushort>(mode);
        }
        /// <summary>
        /// Gets if the type serializer can write the type
        /// </summary>
        /// <param name="type">Type of the value to write</param>
        /// <returns>true if the type serializer can write the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanWrite(Type type)
            =>
            type == typeof(decimal) || type == typeof(double) || type == typeof(float) ||
            type == typeof(long) || type == typeof(ulong) ||
            type == typeof(int) || type == typeof(uint) ||
            type == typeof(short) || type == typeof(ushort) ||
            type == typeof(byte) || type == typeof(sbyte);
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
            var valueType = value?.GetType();
            var decType = DataTypeHelper.GetDecreaseDataType(value, valueType);
            var objIdx = 0;
            switch (decType)
            {
                case DataType.Decimal:
                    #region Decimal Type
                    var v1 = Factory.Converter.ToDecimal(value);
                    if (v1 == default(decimal))
                    {
                        writer.Write(DataType.DecimalDefault);
                        return;
                    }
                    objIdx = DecimalCache.SerializerGet(v1);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteByte(writer, DataType.RefDecimalByte, (byte)objIdx);
                        else
                            WriteUshort(writer, DataType.RefDecimalUShort, (ushort)objIdx);
                    }
                    else
                    {
                        writer.Write(DataType.Decimal);
                        writer.Write(v1);
                        DecimalCache.SerializerSet(v1);
                    }
                    #endregion
                    return;
                case DataType.Double:
                    #region Double Type
                    var v2 = Factory.Converter.ToDouble(value);
                    if (Math.Abs(v2 - default(double)) < 0.0000000000001)
                    {
                        writer.Write(DataType.DoubleDefault);
                        return;
                    }
                    objIdx = DoubleCache.SerializerGet(v2);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteByte(writer, DataType.RefDoubleByte, (byte)objIdx);
                        else
                            WriteUshort(writer, DataType.RefDoubleUShort, (ushort)objIdx);
                    }
                    else
                    {
                        WriteDouble(writer, DataType.Double, v2);
                        DoubleCache.SerializerSet(v2);
                    }
                    #endregion
                    return;
                case DataType.Float:
                    #region Float Type
                    var v3 = Factory.Converter.ToFloat(value);
                    if (Math.Abs(v3 - default(float)) < 0.0000000000001)
                    {
                        writer.Write(DataType.FloatDefault);
                        return;
                    }
                    objIdx = FloatCache.SerializerGet(v3);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteByte(writer, DataType.RefFloatByte, (byte)objIdx);
                        else
                            WriteUshort(writer, DataType.RefFloatUShort, (ushort)objIdx);
                    }
                    else
                    {
                        WriteFloat(writer, DataType.Float, v3);
                        FloatCache.SerializerSet(v3);
                    }
                    #endregion
                    return;
                case DataType.Long:
                    #region Long Type
                    var v4 = Factory.Converter.ToLong(value);
                    objIdx = LongCache.SerializerGet(v4);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteByte(writer, DataType.RefLongByte, (byte)objIdx);
                        else
                            WriteUshort(writer, DataType.RefLongUShort, (ushort)objIdx);
                    }
                    else
                    {
                        WriteLong(writer, DataType.Long, v4);
                        LongCache.SerializerSet(v4);
                    }
                    #endregion
                    return;
                case DataType.ULong:
                    #region ULong Type
                    var v5 = Factory.Converter.ToULong(value);
                    objIdx = ULongCache.SerializerGet(v5);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteByte(writer, DataType.RefULongByte, (byte)objIdx);
                        else
                            WriteUshort(writer, DataType.RefULongUShort, (ushort)objIdx);
                    }
                    else
                    {
                        WriteULong(writer, DataType.ULong, v5);
                        ULongCache.SerializerSet(v5);
                    }
                    #endregion
                    return;
                case DataType.Int:
                    #region Int Type
                    var v6 = Factory.Converter.ToInt(value);
                    objIdx = IntCache.SerializerGet(v6);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteByte(writer, DataType.RefIntByte, (byte)objIdx);
                        else
                            WriteUshort(writer, DataType.RefIntUShort, (ushort)objIdx);
                    }
                    else
                    {
                        WriteInt(writer, DataType.Int, v6);
                        IntCache.SerializerSet(v6);
                    }
                    #endregion
                    return;
                case DataType.UInt:
                    #region UInt Type
                    var v7 = Factory.Converter.ToUInt(value);
                    objIdx = UIntCache.SerializerGet(v7);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteByte(writer, DataType.RefUIntByte, (byte)objIdx);
                        else
                            WriteUshort(writer, DataType.RefUIntUShort, (ushort)objIdx);
                    }
                    else
                    {
                        WriteUInt(writer, DataType.UInt, v7);
                        UIntCache.SerializerSet(v7);
                    }
                    #endregion
                    return;
                case DataType.Short:
                    #region Short Type
                    var v8 = Factory.Converter.ToShort(value);
                    objIdx = ShortCache.SerializerGet(v8);
                    if (objIdx > -1 && objIdx <= byte.MaxValue)
                        WriteByte(writer, DataType.RefShortByte, (byte)objIdx);
                    else
                    {
                        WriteShort(writer, DataType.Short, v8);
                        ShortCache.SerializerSet(v8);
                    }
                    #endregion
                    return;
                case DataType.UShort:
                    #region UShort Type
                    var v9 = Factory.Converter.ToUShort(value);
                    objIdx = UShortCache.SerializerGet(v9);
                    if (objIdx > -1 && objIdx <= byte.MaxValue)
                        WriteByte(writer, DataType.RefUShortByte, (byte)objIdx);
                    else
                    {
                        WriteUshort(writer, DataType.UShort, v9);
                        UShortCache.SerializerSet(v9);
                    }
                    #endregion
                    return;
                case DataType.Byte:
                    #region Byte Type
                    var v10 = Factory.Converter.ToByte(value);
                    switch (v10)
                    {
                        case 0:
                            writer.Write(DataType.ByteDefault);
                            return;
                        case 1:
                            writer.Write(DataType.Byte1);
                            return;
                        case 2:
                            writer.Write(DataType.Byte2);
                            return;
                        case 3:
                            writer.Write(DataType.Byte3);
                            return;
                        case 4:
                            writer.Write(DataType.Byte4);
                            return;
                        case 5:
                            writer.Write(DataType.Byte5);
                            return;
                        case 6:
                            writer.Write(DataType.Byte6);
                            return;
                        case 7:
                            writer.Write(DataType.Byte7);
                            return;
                        case 8:
                            writer.Write(DataType.Byte8);
                            return;
                        case 9:
                            writer.Write(DataType.Byte9);
                            return;
                        case 10:
                            writer.Write(DataType.Byte10);
                            return;
                        case 11:
                            writer.Write(DataType.Byte11);
                            return;
                        case 12:
                            writer.Write(DataType.Byte12);
                            return;
                        case 13:
                            writer.Write(DataType.Byte13);
                            return;
                        case 14:
                            writer.Write(DataType.Byte14);
                            return;
                        case 15:
                            writer.Write(DataType.Byte15);
                            return;
                        case 16:
                            writer.Write(DataType.Byte16);
                            return;
                        default:
                            WriteByte(writer, DataType.Byte, v10);
                            return;
                    }
                #endregion
                case DataType.SByte:
                    #region SByte Type
                    var sByte = Factory.Converter.ToSByte(value);
                    switch (sByte)
                    {
                        case -1:
                            writer.Write(DataType.SByteMinusOne);
                            return;
                        default:
                            writer.Write(DataType.SByte);
                            writer.Write(sByte);
                            return;
                    }
                    #endregion
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
                    writer.Write(DataType.SByteMinusOne);
                    return;
                case 0:
                    writer.Write(DataType.ByteDefault);
                    return;
                case 1:
                    writer.Write(DataType.Byte1);
                    return;
                case 2:
                    writer.Write(DataType.Byte2);
                    return;
                case 3:
                    writer.Write(DataType.Byte3);
                    return;
                case 4:
                    writer.Write(DataType.Byte4);
                    return;
                case 5:
                    writer.Write(DataType.Byte5);
                    return;
                case 6:
                    writer.Write(DataType.Byte6);
                    return;
                case 7:
                    writer.Write(DataType.Byte7);
                    return;
                case 8:
                    writer.Write(DataType.Byte8);
                    return;
                case 9:
                    writer.Write(DataType.Byte9);
                    return;
                case 10:
                    writer.Write(DataType.Byte10);
                    return;
                case 11:
                    writer.Write(DataType.Byte11);
                    return;
                case 12:
                    writer.Write(DataType.Byte12);
                    return;
                case 13:
                    writer.Write(DataType.Byte13);
                    return;
                case 14:
                    writer.Write(DataType.Byte14);
                    return;
                case 15:
                    writer.Write(DataType.Byte15);
                    return;
                case 16:
                    writer.Write(DataType.Byte16);
                    return;
            }

            var decType = DataTypeHelper.GetDecreaseDataType(value);
            var objIdx = 0;
            switch (decType)
            {
                case DataType.Int:
                    #region Int Type
                    objIdx = IntCache.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteByte(writer, DataType.RefIntByte, (byte)objIdx);
                        else
                            WriteUshort(writer, DataType.RefIntUShort, (ushort)objIdx);
                    }
                    else
                    {
                        WriteInt(writer, DataType.Int, value);
                        IntCache.SerializerSet(value);
                    }
                    #endregion
                    return;
                case DataType.Short:
                    #region Short Type
                    var v8 = Factory.Converter.ToShort(value);
                    objIdx = ShortCache.SerializerGet(v8);
                    if (objIdx > -1 && objIdx <= byte.MaxValue)
                        WriteByte(writer, DataType.RefShortByte, (byte)objIdx);
                    else
                    {
                        WriteShort(writer, DataType.Short, v8);
                        ShortCache.SerializerSet(v8);
                    }
                    #endregion
                    return;
                case DataType.UShort:
                    #region UShort Type
                    var v9 = Factory.Converter.ToUShort(value);
                    objIdx = UShortCache.SerializerGet(v9);
                    if (objIdx > -1 && objIdx <= byte.MaxValue)
                        WriteByte(writer, DataType.RefUShortByte, (byte)objIdx);
                    else
                    {
                        WriteUshort(writer, DataType.UShort, v9);
                        UShortCache.SerializerSet(v9);
                    }
                    #endregion
                    return;
                case DataType.Byte:
                    #region Byte Type
                    WriteByte(writer, DataType.Byte, Factory.Converter.ToByte(value));
                    #endregion
                    return;
                case DataType.SByte:
                    #region SByte Type
                    writer.Write(DataType.SByte);
                    writer.Write(Factory.Converter.ToSByte(value));
                    #endregion
                    return;
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
            switch (type)
            {
                #region Decimal
                case DataType.Decimal:
                    var v1 = reader.ReadDecimal();
                    DecimalCache.DeserializerSet(v1);
                    return v1;
                case DataType.DecimalDefault:
                    return default(decimal);
                case DataType.RefDecimalByte:
                    return DecimalCache.DeserializerGet(reader.ReadByte());
                case DataType.RefDecimalUShort:
                    return DecimalCache.DeserializerGet(reader.ReadUInt16());
                #endregion

                #region Double
                case DataType.Double:
                    var v2 = reader.ReadDouble();
                    DoubleCache.DeserializerSet(v2);
                    return v2;
                case DataType.DoubleDefault:
                    return default(double);
                case DataType.RefDoubleByte:
                    return DoubleCache.DeserializerGet(reader.ReadByte());
                case DataType.RefDoubleUShort:
                    return DoubleCache.DeserializerGet(reader.ReadUInt16());
                #endregion

                #region Float
                case DataType.Float:
                    var v3 = reader.ReadSingle();
                    FloatCache.DeserializerSet(v3);
                    return v3;
                case DataType.FloatDefault:
                    return default(float);
                case DataType.RefFloatByte:
                    return FloatCache.DeserializerGet(reader.ReadByte());
                case DataType.RefFloatUShort:
                    return FloatCache.DeserializerGet(reader.ReadUInt16());
                #endregion

                #region Long
                case DataType.Long:
                    var v4 = reader.ReadInt64();
                    LongCache.DeserializerSet(v4);
                    return v4;
                case DataType.RefLongByte:
                    return LongCache.DeserializerGet(reader.ReadByte());
                case DataType.RefLongUShort:
                    return LongCache.DeserializerGet(reader.ReadUInt16());
                #endregion

                #region ULong
                case DataType.ULong:
                    var v5 = reader.ReadUInt64();
                    ULongCache.DeserializerSet(v5);
                    return v5;
                case DataType.RefULongByte:
                    return ULongCache.DeserializerGet(reader.ReadByte());
                case DataType.RefULongUShort:
                    return ULongCache.DeserializerGet(reader.ReadUInt16());
                #endregion

                #region Int
                case DataType.Int:
                    var v6 = reader.ReadInt32();
                    IntCache.DeserializerSet(v6);
                    return v6;
                case DataType.RefIntByte:
                    return IntCache.DeserializerGet(reader.ReadByte());
                case DataType.RefIntUShort:
                    return IntCache.DeserializerGet(reader.ReadUInt16());
                #endregion

                #region UInt
                case DataType.UInt:
                    var v7 = reader.ReadUInt32();
                    UIntCache.DeserializerSet(v7);
                    return v7;
                case DataType.RefUIntByte:
                    return UIntCache.DeserializerGet(reader.ReadByte());
                case DataType.RefUIntUShort:
                    return UIntCache.DeserializerGet(reader.ReadUInt16());
                #endregion

                #region Short
                case DataType.Short:
                    var v8 = reader.ReadInt16();
                    ShortCache.DeserializerSet(v8);
                    return v8;
                case DataType.RefShortByte:
                    return ShortCache.DeserializerGet(reader.ReadByte());
                #endregion

                #region UShort
                case DataType.UShort:
                    var v9 = reader.ReadUInt16();
                    UShortCache.DeserializerSet(v9);
                    return v9;
                case DataType.RefUShortByte:
                    return UShortCache.DeserializerGet(reader.ReadByte());
                #endregion

                #region Byte
                case DataType.Byte:
                    return reader.ReadByte();
                case DataType.ByteDefault:
                    return (byte)0;
                case DataType.Byte1:
                    return (byte)1;
                case DataType.Byte2:
                    return (byte)2;
                case DataType.Byte3:
                    return (byte)3;
                case DataType.Byte4:
                    return (byte)4;
                case DataType.Byte5:
                    return (byte)5;
                case DataType.Byte6:
                    return (byte)6;
                case DataType.Byte7:
                    return (byte)7;
                case DataType.Byte8:
                    return (byte)8;
                case DataType.Byte9:
                    return (byte)9;
                case DataType.Byte10:
                    return (byte)10;
                case DataType.Byte11:
                    return (byte)11;
                case DataType.Byte12:
                    return (byte)12;
                case DataType.Byte13:
                    return (byte)13;
                case DataType.Byte14:
                    return (byte)14;
                case DataType.Byte15:
                    return (byte)15;
                case DataType.Byte16:
                    return (byte)16;
                #endregion

                //SByte
                case DataType.SByte:
                    return reader.ReadSByte();

                case DataType.SByteMinusOne:
                    return -1;
            }
            return 0;
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
            switch (type)
            {
                #region Int
                case DataType.Int:
                    var v6 = reader.ReadInt32();
                    IntCache.DeserializerSet(v6);
                    return v6;
                case DataType.RefIntByte:
                    return IntCache.DeserializerGet(reader.ReadByte());
                case DataType.RefIntUShort:
                    return IntCache.DeserializerGet(reader.ReadUInt16());
                #endregion

                #region Short
                case DataType.Short:
                    var v8 = reader.ReadInt16();
                    ShortCache.DeserializerSet(v8);
                    return v8;
                case DataType.RefShortByte:
                    return ShortCache.DeserializerGet(reader.ReadByte());
                #endregion

                #region UShort
                case DataType.UShort:
                    var v9 = reader.ReadUInt16();
                    UShortCache.DeserializerSet(v9);
                    return v9;
                case DataType.RefUShortByte:
                    return UShortCache.DeserializerGet(reader.ReadByte());
                #endregion

                #region Byte
                case DataType.Byte:
                    return reader.ReadByte();
                case DataType.ByteDefault:
                    return 0;
                case DataType.Byte1:
                    return 1;
                case DataType.Byte2:
                    return 2;
                case DataType.Byte3:
                    return 3;
                case DataType.Byte4:
                    return 4;
                case DataType.Byte5:
                    return 5;
                case DataType.Byte6:
                    return 6;
                case DataType.Byte7:
                    return 7;
                case DataType.Byte8:
                    return 8;
                case DataType.Byte9:
                    return 9;
                case DataType.Byte10:
                    return 10;
                case DataType.Byte11:
                    return 11;
                case DataType.Byte12:
                    return 12;
                case DataType.Byte13:
                    return 13;
                case DataType.Byte14:
                    return 14;
                case DataType.Byte15:
                    return 15;
                case DataType.Byte16:
                    return 16;
                #endregion

                //SByte
                case DataType.SByte:
                    return reader.ReadSByte();
                case DataType.SByteMinusOne:
                    return -1;
            }
            return 0;
        }
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int ReadValue(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                #region Int
                case DataType.Int:
                    var v6 = reader.ReadInt32();
                    IntCache.DeserializerSet(v6);
                    return v6;
                case DataType.RefIntByte:
                    return IntCache.DeserializerGet(reader.ReadByte());
                case DataType.RefIntUShort:
                    return IntCache.DeserializerGet(reader.ReadUInt16());
                #endregion

                #region Short
                case DataType.Short:
                    var v8 = reader.ReadInt16();
                    ShortCache.DeserializerSet(v8);
                    return v8;
                case DataType.RefShortByte:
                    return ShortCache.DeserializerGet(reader.ReadByte());
                #endregion

                #region UShort
                case DataType.UShort:
                    var v9 = reader.ReadUInt16();
                    UShortCache.DeserializerSet(v9);
                    return v9;
                case DataType.RefUShortByte:
                    return UShortCache.DeserializerGet(reader.ReadByte());
                #endregion

                #region Byte
                case DataType.Byte:
                    return reader.ReadByte();
                case DataType.ByteDefault:
                    return 0;
                case DataType.Byte1:
                    return 1;
                case DataType.Byte2:
                    return 2;
                case DataType.Byte3:
                    return 3;
                case DataType.Byte4:
                    return 4;
                case DataType.Byte5:
                    return 5;
                case DataType.Byte6:
                    return 6;
                case DataType.Byte7:
                    return 7;
                case DataType.Byte8:
                    return 8;
                case DataType.Byte9:
                    return 9;
                case DataType.Byte10:
                    return 10;
                case DataType.Byte11:
                    return 11;
                case DataType.Byte12:
                    return 12;
                case DataType.Byte13:
                    return 13;
                case DataType.Byte14:
                    return 14;
                case DataType.Byte15:
                    return 15;
                case DataType.Byte16:
                    return 16;
                #endregion

                //SByte
                case DataType.SByte:
                    return reader.ReadSByte();
                case DataType.SByteMinusOne:
                    return -1;
            }
            return 0;
        }
    }
}

