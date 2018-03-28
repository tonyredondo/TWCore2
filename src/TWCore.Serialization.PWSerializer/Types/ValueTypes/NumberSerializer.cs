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
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.PWSerializer.Types.ValueTypes
{
    /// <inheritdoc cref="ITypeSerializer" />
    /// <summary>
    /// Int value type serializer
    /// </summary>
	public class NumberSerializer : ITypeSerializer<int>
    {
        public static readonly HashSet<byte> ReadTypes = new HashSet<byte>(new[]
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
            DataType.Byte9, DataType.Byte10, DataType.Byte11, DataType.Byte12, DataType.Byte13, DataType.Byte14, DataType.Byte15, DataType.Byte16,
            DataType.Byte17, DataType.Byte18, DataType.Byte19, DataType.Byte20
        });

        #region Decimal Cache
        private SerializerCache<decimal> _decimalCache;
        private SerializerCache<decimal> DecimalCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _decimalCache ?? (_decimalCache = new SerializerCache<decimal>()); }
        }
        #endregion

        #region Double Cache
        private SerializerCache<double> _doubleCache;
        private SerializerCache<double> DoubleCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _doubleCache ?? (_doubleCache = new SerializerCache<double>()); }
        }
        #endregion

        #region Float Cache
        private SerializerCache<float> _floatCache;
        private SerializerCache<float> FloatCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _floatCache ?? (_floatCache = new SerializerCache<float>()); }
        }
        #endregion

        #region Long Cache
        private SerializerCache<long> _longCache;
        private SerializerCache<long> LongCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _longCache ?? (_longCache = new SerializerCache<long>()); }
        }
        #endregion

        #region ULong Cache
        private SerializerCache<ulong> _uLongCache;
        private SerializerCache<ulong> ULongCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _uLongCache ?? (_uLongCache = new SerializerCache<ulong>()); }
        }
        #endregion

        #region Int Cache
        private SerializerCache<int> _intCache;
        private SerializerCache<int> IntCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _intCache ?? (_intCache = new SerializerCache<int>()); }
        }
        #endregion

        #region UInt Cache
        private SerializerCache<uint> _uIntCache;
        private SerializerCache<uint> UIntCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _uIntCache ?? (_uIntCache = new SerializerCache<uint>()); }
        }
        #endregion

        #region Short Cache
        private SerializerCache<short> _shortCache;
        private SerializerCache<short> ShortCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _shortCache ?? (_shortCache = new SerializerCache<short>()); }
        }
        #endregion

        #region UShort Cache
        private SerializerCache<ushort> _uShortCache;
        private SerializerCache<ushort> UShortCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _uShortCache ?? (_uShortCache = new SerializerCache<ushort>()); }
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Type serializer initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
        }
        /// <inheritdoc />
        /// <summary>
        /// Clear serializer cache
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _decimalCache?.Clear();
            _doubleCache?.Clear();
            _floatCache?.Clear();
            _longCache?.Clear();
            _uLongCache?.Clear();
            _intCache?.Clear();
            _uIntCache?.Clear();
            _shortCache?.Clear();
            _uShortCache?.Clear();
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets if the type serializer can write the type
        /// </summary>
        /// <param name="type">Type of the value to write</param>
        /// <returns>true if the type serializer can write the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanWrite(Type type)
            =>
            type == typeof(decimal) || type == typeof(double) || type == typeof(float) ||
            type == typeof(long) || type == typeof(ulong) ||
            type == typeof(int) || type == typeof(uint) ||
            type == typeof(short) || type == typeof(ushort) ||
            type == typeof(byte) || type == typeof(sbyte);
        /// <inheritdoc />
        /// <summary>
        /// Gets if the type serializer can read the data type
        /// </summary>
        /// <param name="type">DataType value</param>
        /// <returns>true if the type serializer can read the type; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanRead(byte type)
            => ReadTypes.Contains(type);
        /// <inheritdoc />
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter writer, object value)
        {
            var valueType = value?.GetType();
            var decType = DataTypeHelper.GetDecreaseDataType(value, valueType);
            var convValue = (IConvertible) value ?? 0;
            int objIdx;
            switch (decType)
            {
                case DataType.Decimal:
                    #region Decimal Type
                    var v1 = convValue.ToDecimal(null);
                    if (v1 == default(decimal))
                    {
                        writer.Write(DataType.DecimalDefault);
                        return;
                    }
                    objIdx = DecimalCache.SerializerGet(v1);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteHelper.WriteByte(writer, DataType.RefDecimalByte, (byte)objIdx);
                        else
                            WriteHelper.WriteUshort(writer, DataType.RefDecimalUShort, (ushort)objIdx);
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
                    var v2 = convValue.ToDouble(null);
                    if (Math.Abs(v2 - default(double)) < 0.0000000000001)
                    {
                        writer.Write(DataType.DoubleDefault);
                        return;
                    }
                    objIdx = DoubleCache.SerializerGet(v2);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteHelper.WriteByte(writer, DataType.RefDoubleByte, (byte)objIdx);
                        else
                            WriteHelper.WriteUshort(writer, DataType.RefDoubleUShort, (ushort)objIdx);
                    }
                    else
                    {
                        WriteHelper.WriteDouble(writer, DataType.Double, v2);
                        DoubleCache.SerializerSet(v2);
                    }
                    #endregion
                    return;
                case DataType.Float:
                    #region Float Type
                    var v3 = convValue.ToSingle(null);
                    if (Math.Abs(v3 - default(float)) < 0.0000000000001)
                    {
                        writer.Write(DataType.FloatDefault);
                        return;
                    }
                    objIdx = FloatCache.SerializerGet(v3);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteHelper.WriteByte(writer, DataType.RefFloatByte, (byte)objIdx);
                        else
                            WriteHelper.WriteUshort(writer, DataType.RefFloatUShort, (ushort)objIdx);
                    }
                    else
                    {
                        WriteHelper.WriteFloat(writer, DataType.Float, v3);
                        FloatCache.SerializerSet(v3);
                    }
                    #endregion
                    return;
                case DataType.Long:
                    #region Long Type
                    var v4 = convValue.ToInt64(null);
                    objIdx = LongCache.SerializerGet(v4);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteHelper.WriteByte(writer, DataType.RefLongByte, (byte)objIdx);
                        else
                            WriteHelper.WriteUshort(writer, DataType.RefLongUShort, (ushort)objIdx);
                    }
                    else
                    {
                        WriteHelper.WriteLong(writer, DataType.Long, v4);
                        LongCache.SerializerSet(v4);
                    }
                    #endregion
                    return;
                case DataType.ULong:
                    #region ULong Type
                    var v5 = convValue.ToUInt64(null);
                    objIdx = ULongCache.SerializerGet(v5);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteHelper.WriteByte(writer, DataType.RefULongByte, (byte)objIdx);
                        else
                            WriteHelper.WriteUshort(writer, DataType.RefULongUShort, (ushort)objIdx);
                    }
                    else
                    {
                        WriteHelper.WriteULong(writer, DataType.ULong, v5);
                        ULongCache.SerializerSet(v5);
                    }
                    #endregion
                    return;
                case DataType.Int:
                    #region Int Type
                    var v6 = convValue.ToInt32(null);
                    objIdx = IntCache.SerializerGet(v6);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteHelper.WriteByte(writer, DataType.RefIntByte, (byte)objIdx);
                        else
                            WriteHelper.WriteUshort(writer, DataType.RefIntUShort, (ushort)objIdx);
                    }
                    else
                    {
                        WriteHelper.WriteInt(writer, DataType.Int, v6);
                        IntCache.SerializerSet(v6);
                    }
                    #endregion
                    return;
                case DataType.UInt:
                    #region UInt Type
                    var v7 = convValue.ToUInt32(null);
                    objIdx = UIntCache.SerializerGet(v7);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteHelper.WriteByte(writer, DataType.RefUIntByte, (byte)objIdx);
                        else
                            WriteHelper.WriteUshort(writer, DataType.RefUIntUShort, (ushort)objIdx);
                    }
                    else
                    {
                        WriteHelper.WriteUInt(writer, DataType.UInt, v7);
                        UIntCache.SerializerSet(v7);
                    }
                    #endregion
                    return;
                case DataType.Short:
                    #region Short Type
                    var v8 = convValue.ToInt16(null);
                    objIdx = ShortCache.SerializerGet(v8);
                    if (objIdx > -1 && objIdx <= byte.MaxValue)
                        WriteHelper.WriteByte(writer, DataType.RefShortByte, (byte)objIdx);
                    else
                    {
                        WriteHelper.WriteShort(writer, DataType.Short, v8);
                        ShortCache.SerializerSet(v8);
                    }
                    #endregion
                    return;
                case DataType.UShort:
                    #region UShort Type
                    var v9 = convValue.ToUInt16(null);
                    objIdx = UShortCache.SerializerGet(v9);
                    if (objIdx > -1 && objIdx <= byte.MaxValue)
                        WriteHelper.WriteByte(writer, DataType.RefUShortByte, (byte)objIdx);
                    else
                    {
                        WriteHelper.WriteUshort(writer, DataType.UShort, v9);
                        UShortCache.SerializerSet(v9);
                    }
                    #endregion
                    return;
                case DataType.Byte:
                    #region Byte Type
                    var v10 = convValue.ToByte(null);
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
                        case 17:
                            writer.Write(DataType.Byte17);
                            return;
                        case 18:
                            writer.Write(DataType.Byte18);
                            return;
                        case 19:
                            writer.Write(DataType.Byte19);
                            return;
                        case 20:
                            writer.Write(DataType.Byte20);
                            return;
                        default:
                            WriteHelper.WriteByte(writer, DataType.Byte, v10);
                            return;
                    }
                #endregion
                case DataType.SByte:
                    #region SByte Type
                    var sByte = convValue.ToSByte(null);
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
        /// <inheritdoc />
        /// <summary>
        /// Writes the serialized value to the binary stream.
        /// </summary>
        /// <param name="writer">Binary writer of the stream</param>
        /// <param name="value">Object value to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(BinaryWriter writer, int value)
        {
            #region Static values
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
                case 17:
                    writer.Write(DataType.Byte17);
                    return;
                case 18:
                    writer.Write(DataType.Byte18);
                    return;
                case 19:
                    writer.Write(DataType.Byte19);
                    return;
                case 20:
                    writer.Write(DataType.Byte20);
                    return;
            }
            #endregion

            var decType = DataTypeHelper.GetDecreaseDataType(value);
            int objIdx;
            switch (decType)
            {
                case DataType.Int:
                    #region Int Type
                    objIdx = IntCache.SerializerGet(value);
                    if (objIdx > -1)
                    {
                        if (objIdx <= byte.MaxValue)
                            WriteHelper.WriteByte(writer, DataType.RefIntByte, (byte)objIdx);
                        else
                            WriteHelper.WriteUshort(writer, DataType.RefIntUShort, (ushort)objIdx);
                    }
                    else
                    {
                        WriteHelper.WriteInt(writer, DataType.Int, value);
                        IntCache.SerializerSet(value);
                    }
                    #endregion
                    return;
                case DataType.Short:
                    #region Short Type
                    var v8 = (short)value;
                    objIdx = ShortCache.SerializerGet(v8);
                    if (objIdx > -1 && objIdx <= byte.MaxValue)
                        WriteHelper.WriteByte(writer, DataType.RefShortByte, (byte)objIdx);
                    else
                    {
                        WriteHelper.WriteShort(writer, DataType.Short, v8);
                        ShortCache.SerializerSet(v8);
                    }
                    #endregion
                    return;
                case DataType.UShort:
                    #region UShort Type
                    var v9 = (ushort)value;
                    objIdx = UShortCache.SerializerGet(v9);
                    if (objIdx > -1 && objIdx <= byte.MaxValue)
                        WriteHelper.WriteByte(writer, DataType.RefUShortByte, (byte)objIdx);
                    else
                    {
                        WriteHelper.WriteUshort(writer, DataType.UShort, v9);
                        UShortCache.SerializerSet(v9);
                    }
                    #endregion
                    return;
                case DataType.Byte:
                    #region Byte Type
                    WriteHelper.WriteByte(writer, DataType.Byte, (byte)value);
                    #endregion
                    return;
                case DataType.SByte:
                    #region SByte Type
                    writer.Write(DataType.SByte);
                    writer.Write((sbyte)value);
                    #endregion
                    return;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Read(BinaryReader reader, byte type)
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
                case DataType.Byte17:
                    return 17;
                case DataType.Byte18:
                    return 18;
                case DataType.Byte19:
                    return 19;
                case DataType.Byte20:
                    return 20;
                #endregion

                //SByte
                case DataType.SByte:
                    return reader.ReadSByte();

                case DataType.SByteMinusOne:
                    return -1;
            }
            return 0;
        }

        /// <inheritdoc />
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <param name="type">DataType</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadValue(BinaryReader reader, byte type)
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
                case DataType.Byte17:
                    return 17;
                case DataType.Byte18:
                    return 18;
                case DataType.Byte19:
                    return 19;
                case DataType.Byte20:
                    return 20;
                #endregion

                //SByte
                case DataType.SByte:
                    return reader.ReadSByte();
                case DataType.SByteMinusOne:
                    return -1;
            }
            return 0;
        }
        /// <inheritdoc />
        /// <summary>
        /// Reads a value from the serialized stream.
        /// </summary>
        /// <param name="reader">Binary reader of the stream</param>
        /// <returns>Object instance of the value deserialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadValue(BinaryReader reader)
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
                case DataType.Byte17:
                    return 17;
                case DataType.Byte18:
                    return 18;
                case DataType.Byte19:
                    return 19;
                case DataType.Byte20:
                    return 20;
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

