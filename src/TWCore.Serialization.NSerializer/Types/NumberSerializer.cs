using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.NSerializer
{
    public partial class SerializersTable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(decimal value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.DecimalDefault);
                return;
            }
            if (_decimalCache.TryGetValue(value, out var objIdx))
            {
                WriteDefInt(DataBytesDefinition.RefDecimal, objIdx);
                return;
            }
            WriteByte(DataBytesDefinition.Decimal);
            WriteDecimal(value);
            _decimalCache.Set(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double value)
        {
            if (Math.Abs(value - default(double)) < 0.0000000000001)
            {
                WriteByte(DataBytesDefinition.DoubleDefault);
                return;
            }
            if (_doubleCache.TryGetValue(value, out var objIdx))
            {
                WriteDefInt(DataBytesDefinition.RefDouble, objIdx);
                return;
            }
            WriteDefDouble(DataBytesDefinition.Double, value);
            _doubleCache.Set(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float value)
        {
            if (Math.Abs(value - default(float)) < 0.0000000000001)
            {
                WriteByte(DataBytesDefinition.FloatDefault);
                return;
            }
            if (_floatCache.TryGetValue(value, out var objIdx))
            {
                WriteDefInt(DataBytesDefinition.RefFloat, objIdx);
                return;
            }
            WriteDefFloat(DataBytesDefinition.Float, value);
            _floatCache.Set(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.LongDefault);
                return;
            }
            if (_longCache.TryGetValue(value, out var objIdx))
            {
                WriteDefInt(DataBytesDefinition.RefLong, objIdx);
                return;
            }
            WriteDefLong(DataBytesDefinition.Long, value);
            _longCache.Set(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.ULongDefault);
                return;
            }
            if (_uLongCache.TryGetValue(value, out var objIdx))
            {
                WriteDefInt(DataBytesDefinition.RefULong, objIdx);
                return;
            }
            WriteDefULong(DataBytesDefinition.ULong, value);
            _uLongCache.Set(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.IntDefault);
                return;
            }
            WriteDefInt(DataBytesDefinition.Int, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.UIntDefault);
                return;
            }
            WriteDefUInt(DataBytesDefinition.UInt, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.ShortDefault);
                return;
            }
            WriteDefShort(DataBytesDefinition.Short, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.UShortDefault);
                return;
            }
            WriteDefUshort(DataBytesDefinition.UShort, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.ByteDefault);
                return;
            }
            WriteDefByte(DataBytesDefinition.Byte, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.SByteDefault);
                return;
            }
            if (value == -1)
            {
                WriteByte(DataBytesDefinition.SByteMinusOne);
                return;
            }
            WriteByte(DataBytesDefinition.SByte);
            WriteSByte(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(decimal? value)
        {
            if (value == null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double? value)
        {
            if (value == null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float? value)
        {
            if (value == null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long? value)
        {
            if (value == null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong? value)
        {
            if (value == null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int? value)
        {
            if (value == null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint? value)
        {
            if (value == null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short? value)
        {
            if (value == null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort? value)
        {
            if (value == null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte? value)
        {
            if (value == null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte? value)
        {
            if (value == null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
    }



    public partial class DeserializersTable
    {
        [DeserializerMethod(DataBytesDefinition.Decimal, DataBytesDefinition.DecimalDefault, DataBytesDefinition.RefDecimal, ReturnType = typeof(decimal))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal StreamReadDecimal(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.Decimal:
                    var v1 = StreamReadDecimal();
                    _decimalCache.Set(v1);
                    return v1;
                case DataBytesDefinition.DecimalDefault:
                    return default;
                case DataBytesDefinition.RefDecimal:
                    return _decimalCache.Get(StreamReadInt());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.Double, DataBytesDefinition.DoubleDefault, DataBytesDefinition.RefDouble, ReturnType = typeof(double))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double StreamReadDouble(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.Double:
                    var v2 = StreamReadDouble();
                    _doubleCache.Set(v2);
                    return v2;
                case DataBytesDefinition.DoubleDefault:
                    return default;
                case DataBytesDefinition.RefDouble:
                    return _doubleCache.Get(StreamReadInt());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.Float, DataBytesDefinition.FloatDefault, DataBytesDefinition.RefFloat, ReturnType = typeof(float))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float StreamReadFloat(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.Float:
                    var v3 = StreamReadFloat();
                    _floatCache.Set(v3);
                    return v3;
                case DataBytesDefinition.FloatDefault:
                    return default;
                case DataBytesDefinition.RefFloat:
                    return _floatCache.Get(StreamReadInt());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.Long, DataBytesDefinition.LongDefault, DataBytesDefinition.RefLong, ReturnType = typeof(long))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long StreamReadLong(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.LongDefault:
                    return default;
                case DataBytesDefinition.Long:
                    var v4 = StreamReadLong();
                    _longCache.Set(v4);
                    return v4;
                case DataBytesDefinition.RefLong:
                    return _longCache.Get(StreamReadInt());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.ULong, DataBytesDefinition.ULongDefault, DataBytesDefinition.RefULong, ReturnType = typeof(ulong))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong StreamReadULong(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.ULongDefault:
                    return default;
                case DataBytesDefinition.ULong:
                    var v5 = StreamReadULong();
                    _uLongCache.Set(v5);
                    return v5;
                case DataBytesDefinition.RefULong:
                    return _uLongCache.Get(StreamReadInt());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.Int, DataBytesDefinition.IntDefault, ReturnType = typeof(int))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int StreamReadInt(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.IntDefault:
                    return default;
                case DataBytesDefinition.Int:
                    return StreamReadInt();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.UInt, DataBytesDefinition.UIntDefault, ReturnType = typeof(uint))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint StreamReadUInt(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.UIntDefault:
                    return default;
                case DataBytesDefinition.UInt:
                    return StreamReadUInt();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.Short, DataBytesDefinition.ShortDefault, ReturnType = typeof(short))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short StreamReadShort(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.ShortDefault:
                    return default;
                case DataBytesDefinition.Short:
                    return StreamReadShort();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.UShort, DataBytesDefinition.UShortDefault, ReturnType = typeof(ushort))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort StreamReadUShort(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.UShortDefault:
                    return default;
                case DataBytesDefinition.UShort:
                    return StreamReadUShort();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.Byte, DataBytesDefinition.ByteDefault, ReturnType = typeof(byte))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte StreamReadByte(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.ByteDefault:
                    return default;
                case DataBytesDefinition.Byte:
                    return StreamReadByte();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.SByte, DataBytesDefinition.SByteDefault, DataBytesDefinition.SByteMinusOne, ReturnType = typeof(sbyte))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte StreamReadSByte(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.SByteDefault:
                    return default;
                case DataBytesDefinition.SByte:
                    return StreamReadSByte();
                case DataBytesDefinition.SByteMinusOne:
                    return -1;
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}