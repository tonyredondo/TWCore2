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
                WriteByte(DataBytesDefinition.NumberDefault);
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
                WriteByte(DataBytesDefinition.NumberDefault);
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
                WriteByte(DataBytesDefinition.NumberDefault);
                return;
            }
            WriteDefInt(DataBytesDefinition.Int, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.NumberDefault);
                return;
            }
            WriteDefUInt(DataBytesDefinition.UInt, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.NumberDefault);
                return;
            }
            WriteDefShort(DataBytesDefinition.Short, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.NumberDefault);
                return;
            }
            WriteDefUshort(DataBytesDefinition.UShort, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.NumberDefault);
                return;
            }
            WriteDefByte(DataBytesDefinition.Byte, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.NumberDefault);
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
        private DeserializerCache<decimal> _decimalCache;
        private DeserializerCache<double> _doubleCache;
        private DeserializerCache<float> _floatCache;
        private DeserializerCache<long> _longCache;
        private DeserializerCache<ulong> _uLongCache;

        /// <inheritdoc />
        /// <summary>
        /// Type serializer initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitNumber()
        {
            _decimalCache = new DeserializerCache<decimal>();
            _doubleCache = new DeserializerCache<double>();
            _floatCache = new DeserializerCache<float>();
            _longCache = new DeserializerCache<long>();
            _uLongCache = new DeserializerCache<ulong>();
        }
        /// <inheritdoc />
        /// <summary>
        /// Clear serializer cache
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearNumber()
        {
            _decimalCache?.Clear();
            _doubleCache?.Clear();
            _floatCache?.Clear();
            _longCache?.Clear();
            _uLongCache?.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal ReadDecimal(BinaryReader reader)
            => ReadDecimalNullable(reader) ?? default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble(BinaryReader reader)
            => ReadDoubleNullable(reader) ?? default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat(BinaryReader reader)
            => ReadFloatNullable(reader) ?? default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadLong(BinaryReader reader)
            => ReadLongNullable(reader) ?? default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUlong(BinaryReader reader)
            => ReadUlongNullable(reader) ?? default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt(BinaryReader reader)
            => ReadIntNullable(reader) ?? default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUint(BinaryReader reader)
            => ReadUintNullable(reader) ?? default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadShort(BinaryReader reader)
            => ReadShortNullable(reader) ?? default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUshort(BinaryReader reader)
            => ReadUshortNullable(reader) ?? default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte(BinaryReader reader)
            => ReadByteNullable(reader) ?? default;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSbyte(BinaryReader reader)
            => ReadSbyteNullable(reader) ?? default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal? ReadDecimalNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.NumberDefault:
                    return 0;
                case DataBytesDefinition.Decimal:
                    var v1 = reader.ReadDecimal();
                    _decimalCache.Set(v1);
                    return v1;
                case DataBytesDefinition.DecimalDefault:
                    return default(decimal);
                case DataBytesDefinition.RefDecimal:
                    return _decimalCache.Get(reader.ReadInt32());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? ReadDoubleNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.NumberDefault:
                    return 0;
                case DataBytesDefinition.Double:
                    var v2 = reader.ReadDouble();
                    _doubleCache.Set(v2);
                    return v2;
                case DataBytesDefinition.DoubleDefault:
                    return default(double);
                case DataBytesDefinition.RefDouble:
                    return _doubleCache.Get(reader.ReadInt32());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float? ReadFloatNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.NumberDefault:
                    return 0;
                case DataBytesDefinition.Float:
                    var v3 = reader.ReadSingle();
                    _floatCache.Set(v3);
                    return v3;
                case DataBytesDefinition.FloatDefault:
                    return default(float);
                case DataBytesDefinition.RefFloat:
                    return _floatCache.Get(reader.ReadInt32());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long? ReadLongNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.NumberDefault:
                    return 0;
                case DataBytesDefinition.Long:
                    var v4 = reader.ReadInt64();
                    _longCache.Set(v4);
                    return v4;
                case DataBytesDefinition.RefLong:
                    return _longCache.Get(reader.ReadInt32());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong? ReadUlongNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.NumberDefault:
                    return 0;
                case DataBytesDefinition.ULong:
                    var v5 = reader.ReadUInt64();
                    _uLongCache.Set(v5);
                    return v5;
                case DataBytesDefinition.RefULong:
                    return _uLongCache.Get(reader.ReadInt32());
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int? ReadIntNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.NumberDefault:
                    return 0;
                case DataBytesDefinition.Int:
                    return reader.ReadInt32();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint? ReadUintNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.NumberDefault:
                    return 0;
                case DataBytesDefinition.UInt:
                    return reader.ReadUInt32();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short? ReadShortNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.NumberDefault:
                    return 0;
                case DataBytesDefinition.Short:
                    return reader.ReadInt16();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort? ReadUshortNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.NumberDefault:
                    return 0;
                case DataBytesDefinition.UShort:
                    return reader.ReadUInt16();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte? ReadByteNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.NumberDefault:
                    return 0;
                case DataBytesDefinition.Byte:
                    return reader.ReadByte();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte? ReadSbyteNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.NumberDefault:
                    return 0;
                case DataBytesDefinition.SByte:
                    return reader.ReadSByte();
                case DataBytesDefinition.SByteMinusOne:
                    return -1;
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}