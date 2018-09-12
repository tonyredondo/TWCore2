﻿using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
#pragma warning disable 1591

namespace TWCore.Serialization.RawSerializer
{
    public partial class SerializersTable
    {
        #region Expressions - Int
        private static readonly MethodInfo IntValueProperty = typeof(int?).GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetMethod;

        internal static Expression WriteIntExpression(Expression value, ParameterExpression serTable)
        {
            var intParam = Expression.Parameter(typeof(int));
            var block = Expression.Block(new[] { intParam },
                Expression.Assign(intParam, value),
                Expression.IfThenElse(
                    Expression.Equal(intParam, Expression.Constant(default(int))),
                    Expression.Call(serTable, WriteByteMethodInfo, Expression.Constant(DataBytesDefinition.IntDefault)),
                    Expression.Call(serTable, WriteDefIntMInfo, Expression.Constant(DataBytesDefinition.Int), intParam)));
            return block.Reduce();
        }
        internal static Expression WriteNulleableIntExpression(Expression value, ParameterExpression serTable)
        {
            var intParam = Expression.Parameter(typeof(int?));
            var block = Expression.Block(new[] { intParam },
                Expression.Assign(intParam, value),
                Expression.IfThenElse(
                    Expression.Equal(intParam, Expression.Constant(null, typeof(int?))),
                    Expression.Call(serTable, WriteByteMethodInfo, Expression.Constant(DataBytesDefinition.ValueNull)),
                    WriteIntExpression(Expression.Call(intParam, IntValueProperty), serTable)));
            return block.Reduce();
        }
        #endregion



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(decimal value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.DecimalDefault);
            else
                WriteDefDecimal(DataBytesDefinition.Decimal, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.DoubleDefault);
            else
                WriteDefDouble(DataBytesDefinition.Double, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.FloatDefault);
            else
                WriteDefFloat(DataBytesDefinition.Float, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.LongDefault);
            else
                WriteDefLong(DataBytesDefinition.Long, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.ULongDefault);
            else
                WriteDefULong(DataBytesDefinition.ULong, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.IntDefault);
            else
                WriteDefInt(DataBytesDefinition.Int, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.UIntDefault);
            else
                WriteDefUInt(DataBytesDefinition.UInt, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.ShortDefault);
            else
                WriteDefShort(DataBytesDefinition.Short, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.UShortDefault);
            else
                WriteDefUshort(DataBytesDefinition.UShort, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.ByteDefault);
            else
                WriteDefByte(DataBytesDefinition.Byte, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.SByteDefault);
            else if (value == -1)
                WriteByte(DataBytesDefinition.SByteMinusOne);
            else
            {
                WriteByte(DataBytesDefinition.SByte);
                WriteSByte(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(decimal? value)
        {
            if (value is null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double? value)
        {
            if (value is null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float? value)
        {
            if (value is null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long? value)
        {
            if (value is null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong? value)
        {
            if (value is null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int? value)
        {
            if (value is null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint? value)
        {
            if (value is null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short? value)
        {
            if (value is null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort? value)
        {
            if (value is null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte? value)
        {
            if (value is null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte? value)
        {
            if (value is null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
    }



    public partial class DeserializersTable
    {
        [DeserializerMethod(DataBytesDefinition.Decimal, DataBytesDefinition.DecimalDefault, ReturnType = typeof(decimal))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal StreamReadDecimal(byte type)
        {
            if (type == DataBytesDefinition.Decimal)
                return StreamReadDecimal();
            if (type == DataBytesDefinition.DecimalDefault)
                return default;
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(ReturnType = typeof(decimal?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal? StreamReadDecimalNullable(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            return StreamReadDecimal(type);
        }

        [DeserializerMethod(DataBytesDefinition.Double, DataBytesDefinition.DoubleDefault, ReturnType = typeof(double))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double StreamReadDouble(byte type)
        {
            if (type == DataBytesDefinition.Double)
                return StreamReadDouble();
            if (type == DataBytesDefinition.DoubleDefault)
                return default;
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(ReturnType = typeof(double?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? StreamReadDoubleNullable(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            return StreamReadDouble(type);
        }

        [DeserializerMethod(DataBytesDefinition.Float, DataBytesDefinition.FloatDefault, ReturnType = typeof(float))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float StreamReadFloat(byte type)
        {
            if (type == DataBytesDefinition.Float)
                return StreamReadFloat();
            if (type == DataBytesDefinition.FloatDefault)
                return default;
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(ReturnType = typeof(float?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float? StreamReadFloatNullable(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            return StreamReadFloat(type);
        }

        [DeserializerMethod(DataBytesDefinition.Long, DataBytesDefinition.LongDefault, ReturnType = typeof(long))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long StreamReadLong(byte type)
        {
            if (type == DataBytesDefinition.Long)
                return StreamReadLong();
            if (type == DataBytesDefinition.LongDefault)
                return default;
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(ReturnType = typeof(long?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long? StreamReadLongNullable(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            return StreamReadLong(type);
        }

        [DeserializerMethod(DataBytesDefinition.ULong, DataBytesDefinition.ULongDefault, ReturnType = typeof(ulong))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong StreamReadULong(byte type)
        {
            if (type == DataBytesDefinition.ULong)
                return StreamReadULong();
            if (type == DataBytesDefinition.ULongDefault)
                return default;
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(ReturnType = typeof(ulong?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong? StreamReadULongNullable(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            return StreamReadULong(type);
        }

        [DeserializerMethod(DataBytesDefinition.Int, DataBytesDefinition.IntDefault, ReturnType = typeof(int))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int StreamReadInt(byte type)
        {
            if (type == DataBytesDefinition.Int)
                return StreamReadInt();
            if (type == DataBytesDefinition.IntDefault)
                return default;
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(ReturnType = typeof(int?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int? StreamReadIntNullable(byte type)
        {
            if (type == DataBytesDefinition.ValueNull)
                return null;
            return StreamReadInt(type);
        }

        [DeserializerMethod(DataBytesDefinition.UInt, DataBytesDefinition.UIntDefault, ReturnType = typeof(uint))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint StreamReadUInt(byte type)
        {
            if (type == DataBytesDefinition.UInt)
                return StreamReadUInt();
            if (type == DataBytesDefinition.UIntDefault)
                return default;
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(ReturnType = typeof(uint?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint? StreamReadUIntNullable(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            return StreamReadUInt(type);
        }

        [DeserializerMethod(DataBytesDefinition.Short, DataBytesDefinition.ShortDefault, ReturnType = typeof(short))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short StreamReadShort(byte type)
        {
            if (type == DataBytesDefinition.Short)
                return StreamReadShort();
            if (type == DataBytesDefinition.ShortDefault)
                return default;
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(ReturnType = typeof(short?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short? StreamReadShortNullable(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            return StreamReadShort(type);
        }

        [DeserializerMethod(DataBytesDefinition.UShort, DataBytesDefinition.UShortDefault, ReturnType = typeof(ushort))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort StreamReadUShort(byte type)
        {
            if (type == DataBytesDefinition.UShort)
                return StreamReadUShort();
            if (type == DataBytesDefinition.UShortDefault)
                return default;
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(ReturnType = typeof(ushort?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort? StreamReadUShortNullable(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            return StreamReadUShort(type);
        }

        [DeserializerMethod(DataBytesDefinition.Byte, DataBytesDefinition.ByteDefault, ReturnType = typeof(byte))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte StreamReadByte(byte type)
        {
            if (type == DataBytesDefinition.Byte)
                return StreamReadByte();
            if (type == DataBytesDefinition.ByteDefault)
                return default;
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(ReturnType = typeof(byte?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte? StreamReadByteNullable(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            return StreamReadByte(type);
        }

        [DeserializerMethod(DataBytesDefinition.SByte, DataBytesDefinition.SByteDefault, DataBytesDefinition.SByteMinusOne, ReturnType = typeof(sbyte))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte StreamReadSByte(byte type)
        {
            if (type == DataBytesDefinition.SByte)
                return StreamReadSByte();
            if (type == DataBytesDefinition.SByteDefault)
                return default;
            if (type == DataBytesDefinition.SByteMinusOne)
                return -1;
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(ReturnType = typeof(sbyte?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte? StreamReadSByteNullable(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            return StreamReadSByte(type);
        }
    }
}