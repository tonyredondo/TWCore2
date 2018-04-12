using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.NSerializer
{
    public partial class SerializersTable
    {
        private SerializerCache<decimal> _decimalCache;
        private SerializerCache<double> _doubleCache;
        private SerializerCache<float> _floatCache;
        private SerializerCache<long> _longCache;
        private SerializerCache<ulong> _uLongCache;

        /// <inheritdoc />
        /// <summary>
        /// Type serializer initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitNumber()
        {
            _decimalCache = new SerializerCache<decimal>();
            _doubleCache = new SerializerCache<double>();
            _floatCache = new SerializerCache<float>();
            _longCache = new SerializerCache<long>();
            _uLongCache = new SerializerCache<ulong>();
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
        public void WriteValue(decimal value)
        {
            if (value == default)
            {
                _stream.WriteByte(DataBytesDefinition.DecimalDefault);
                return;
            }
            if (_decimalCache.SerializerTryGetValue(value, out var objIdx))
            {
                WriteInt(DataBytesDefinition.RefDecimal, objIdx);
                return;
            }
            _stream.WriteByte(DataBytesDefinition.Decimal);
            WriteDecimal(value);
            _decimalCache.SerializerSet(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double value)
        {
            if (Math.Abs(value - default(double)) < 0.0000000000001)
            {
                _stream.WriteByte(DataBytesDefinition.DoubleDefault);
                return;
            }
            if (_doubleCache.SerializerTryGetValue(value, out var objIdx))
            {
                WriteInt(DataBytesDefinition.RefDouble, objIdx);
                return;
            }
            WriteDouble(DataBytesDefinition.Double, value);
            _doubleCache.SerializerSet(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float value)
        {
            if (Math.Abs(value - default(float)) < 0.0000000000001)
            {
                _stream.WriteByte(DataBytesDefinition.FloatDefault);
                return;
            }
            if (_floatCache.SerializerTryGetValue(value, out var objIdx))
            {
                WriteInt(DataBytesDefinition.RefFloat, objIdx);
                return;
            }
            WriteFloat(DataBytesDefinition.Float, value);
            _floatCache.SerializerSet(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long value)
        {
            if (value == default)
            {
                _stream.WriteByte(DataBytesDefinition.NumberDefault);
                return;
            }
            if (_longCache.SerializerTryGetValue(value, out var objIdx))
            {
                WriteInt(DataBytesDefinition.RefLong, objIdx);
                return;
            }
            WriteLong(DataBytesDefinition.Long, value);
            _longCache.SerializerSet(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong value)
        {
            if (value == default)
            {
                _stream.WriteByte(DataBytesDefinition.NumberDefault);
                return;
            }
            if (_uLongCache.SerializerTryGetValue(value, out var objIdx))
            {
                WriteInt(DataBytesDefinition.RefULong, objIdx);
                return;
            }
            WriteULong(DataBytesDefinition.ULong, value);
            _uLongCache.SerializerSet(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int value)
        {
            if (value == default)
            {
                _stream.WriteByte(DataBytesDefinition.NumberDefault);
                return;
            }
            WriteInt(DataBytesDefinition.Int, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint value)
        {
            if (value == default)
            {
                _stream.WriteByte(DataBytesDefinition.NumberDefault);
                return;
            }
            WriteUInt(DataBytesDefinition.UInt, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short value)
        {
            if (value == default)
            {
                _stream.WriteByte(DataBytesDefinition.NumberDefault);
                return;
            }
            WriteShort(DataBytesDefinition.Short, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort value)
        {
            if (value == default)
            {
                _stream.WriteByte(DataBytesDefinition.NumberDefault);
                return;
            }
            WriteUshort(DataBytesDefinition.UShort, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte value)
        {
            if (value == default)
            {
                _stream.WriteByte(DataBytesDefinition.NumberDefault);
                return;
            }
            WriteByte(DataBytesDefinition.Byte, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte value)
        {
            if (value == default)
            {
                _stream.WriteByte(DataBytesDefinition.NumberDefault);
                return;
            }
            if (value == -1)
            {
                _stream.WriteByte(DataBytesDefinition.SByteMinusOne);
                return;
            }
            _stream.WriteByte(DataBytesDefinition.SByte);
            WriteSByte(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(decimal? value)
        {
            if (value == null) _stream.WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double? value)
        {
            if (value == null) _stream.WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float? value)
        {
            if (value == null) _stream.WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long? value)
        {
            if (value == null) _stream.WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong? value)
        {
            if (value == null) _stream.WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int? value)
        {
            if (value == null) _stream.WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint? value)
        {
            if (value == null) _stream.WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short? value)
        {
            if (value == null) _stream.WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort? value)
        {
            if (value == null) _stream.WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte? value)
        {
            if (value == null) _stream.WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte? value)
        {
            if (value == null) _stream.WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
    }

    public partial class DeserializersTable
    {
        private SerializerCache<decimal> _decimalCache;
        private SerializerCache<double> _doubleCache;
        private SerializerCache<float> _floatCache;
        private SerializerCache<long> _longCache;
        private SerializerCache<ulong> _uLongCache;

        /// <inheritdoc />
        /// <summary>
        /// Type serializer initialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitNumber()
        {
            _decimalCache = new SerializerCache<decimal>();
            _doubleCache = new SerializerCache<double>();
            _floatCache = new SerializerCache<float>();
            _longCache = new SerializerCache<long>();
            _uLongCache = new SerializerCache<ulong>();
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
                    _decimalCache.DeserializerSet(v1);
                    return v1;
                case DataBytesDefinition.DecimalDefault:
                    return default(decimal);
                case DataBytesDefinition.RefDecimal:
                    return _decimalCache.DeserializerGet(reader.ReadInt32());
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
                    _doubleCache.DeserializerSet(v2);
                    return v2;
                case DataBytesDefinition.DoubleDefault:
                    return default(double);
                case DataBytesDefinition.RefDouble:
                    return _doubleCache.DeserializerGet(reader.ReadInt32());
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
                    _floatCache.DeserializerSet(v3);
                    return v3;
                case DataBytesDefinition.FloatDefault:
                    return default(float);
                case DataBytesDefinition.RefFloat:
                    return _floatCache.DeserializerGet(reader.ReadInt32());
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
                    _longCache.DeserializerSet(v4);
                    return v4;
                case DataBytesDefinition.RefLong:
                    return _longCache.DeserializerGet(reader.ReadInt32());
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
                    _uLongCache.DeserializerSet(v5);
                    return v5;
                case DataBytesDefinition.RefULong:
                    return _uLongCache.DeserializerGet(reader.ReadInt32());
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