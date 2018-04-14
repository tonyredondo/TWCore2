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
        [DeserializerMethod(DataBytesDefinition.Decimal, DataBytesDefinition.DecimalDefault, DataBytesDefinition.RefDecimal)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal ReadDecimal(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.Decimal:
                    var v1 = ReadDecimal();
                    _decimalCache.Set(v1);
                    return v1;
                case DataBytesDefinition.DecimalDefault:
                    return default;
                case DataBytesDefinition.RefDecimal:
                    return _decimalCache.Get(ReadInt());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.Double, DataBytesDefinition.DoubleDefault, DataBytesDefinition.RefDouble)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.Double:
                    var v2 = ReadDouble();
                    _doubleCache.Set(v2);
                    return v2;
                case DataBytesDefinition.DoubleDefault:
                    return default;
                case DataBytesDefinition.RefDouble:
                    return _doubleCache.Get(ReadInt());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.Float, DataBytesDefinition.FloatDefault, DataBytesDefinition.RefFloat)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.Float:
                    var v3 = ReadFloat();
                    _floatCache.Set(v3);
                    return v3;
                case DataBytesDefinition.FloatDefault:
                    return default;
                case DataBytesDefinition.RefFloat:
                    return _floatCache.Get(ReadInt());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.Long, DataBytesDefinition.LongDefault, DataBytesDefinition.RefLong)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadLong(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.LongDefault:
                    return default;
                case DataBytesDefinition.Long:
                    var v4 = ReadLong();
                    _longCache.Set(v4);
                    return v4;
                case DataBytesDefinition.RefLong:
                    return _longCache.Get(ReadInt());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.ULong, DataBytesDefinition.ULongDefault, DataBytesDefinition.RefULong)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadULong(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.ULongDefault:
                    return default;
                case DataBytesDefinition.ULong:
                    var v5 = ReadULong();
                    _uLongCache.Set(v5);
                    return v5;
                case DataBytesDefinition.RefULong:
                    return _uLongCache.Get(ReadInt());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.Int, DataBytesDefinition.IntDefault)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.IntDefault:
                    return default;
                case DataBytesDefinition.Int:
                    return ReadInt();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.UInt, DataBytesDefinition.UIntDefault)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.UIntDefault:
                    return default;
                case DataBytesDefinition.UInt:
                    return ReadUInt();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.Short, DataBytesDefinition.ShortDefault)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadShort(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.ShortDefault:
                    return default;
                case DataBytesDefinition.Short:
                    return ReadShort();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.UShort, DataBytesDefinition.UShortDefault)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUShort(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.UShortDefault:
                    return default;
                case DataBytesDefinition.UShort:
                    return ReadUShort();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.Byte, DataBytesDefinition.ByteDefault)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.ByteDefault:
                    return default;
                case DataBytesDefinition.Byte:
                    return ReadByte();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [DeserializerMethod(DataBytesDefinition.SByte, DataBytesDefinition.SByteDefault, DataBytesDefinition.SByteMinusOne)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByte(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.SByteDefault:
                    return default;
                case DataBytesDefinition.SByte:
                    return ReadSByte();
                case DataBytesDefinition.SByteMinusOne:
                    return -1;
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}