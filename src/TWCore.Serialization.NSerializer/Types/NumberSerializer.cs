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
        private SerializerCache<int> _intCache;
        private SerializerCache<uint> _uIntCache;
        private SerializerCache<short> _shortCache;
        private SerializerCache<ushort> _uShortCache;

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
            _intCache = new SerializerCache<int>();
            _uIntCache = new SerializerCache<uint>();
            _shortCache = new SerializerCache<short>();
            _uShortCache = new SerializerCache<ushort>();
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
            _intCache?.Clear();
            _uIntCache?.Clear();
            _shortCache?.Clear();
            _uShortCache?.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(decimal value)
        {
            if (value == default)
            {
                _stream.WriteByte(DataBytesDefinition.DecimalDefault);
                return;
            }
            var objIdx = _decimalCache.SerializerGet(value);
            if (objIdx > -1)
            {
                if (objIdx <= byte.MaxValue)
                    WriteByte(DataBytesDefinition.RefDecimalByte, (byte)objIdx);
                else
                    WriteUshort(DataBytesDefinition.RefDecimalUShort, (ushort)objIdx);
            }
            else
            {
                _stream.WriteByte(DataBytesDefinition.Decimal);
                WriteDecimal(value);
                _decimalCache.SerializerSet(value);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(double value)
        {
            if (Math.Abs(value - default(double)) < 0.0000000000001)
            {
                _stream.WriteByte(DataBytesDefinition.DoubleDefault);
                return;
            }
            var objIdx = _doubleCache.SerializerGet(value);
            if (objIdx > -1)
            {
                if (objIdx <= byte.MaxValue)
                    WriteByte(DataBytesDefinition.RefDoubleByte, (byte)objIdx);
                else
                    WriteUshort(DataBytesDefinition.RefDoubleUShort, (ushort)objIdx);
            }
            else
            {
                WriteDouble(DataBytesDefinition.Double, value);
                _doubleCache.SerializerSet(value);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(float value)
        {
            if (Math.Abs(value - default(float)) < 0.0000000000001)
            {
                _stream.WriteByte(DataBytesDefinition.FloatDefault);
                return;
            }
            var objIdx = _floatCache.SerializerGet(value);
            if (objIdx > -1)
            {
                if (objIdx <= byte.MaxValue)
                    WriteByte(DataBytesDefinition.RefFloatByte, (byte)objIdx);
                else
                    WriteUshort(DataBytesDefinition.RefFloatUShort, (ushort)objIdx);
            }
            else
            {
                WriteFloat(DataBytesDefinition.Float, value);
                _floatCache.SerializerSet(value);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(long value)
        {
            #region Static Values
            switch (value)
            {
                case -1:
                    _stream.WriteByte(DataBytesDefinition.SByteMinusOne);
                    return;
                case 0:
                    _stream.WriteByte(DataBytesDefinition.ByteDefault);
                    return;
                case 1:
                    _stream.WriteByte(DataBytesDefinition.Byte1);
                    return;
                case 2:
                    _stream.WriteByte(DataBytesDefinition.Byte2);
                    return;
                case 3:
                    _stream.WriteByte(DataBytesDefinition.Byte3);
                    return;
                case 4:
                    _stream.WriteByte(DataBytesDefinition.Byte4);
                    return;
                case 5:
                    _stream.WriteByte(DataBytesDefinition.Byte5);
                    return;
                case 6:
                    _stream.WriteByte(DataBytesDefinition.Byte6);
                    return;
                case 7:
                    _stream.WriteByte(DataBytesDefinition.Byte7);
                    return;
                case 8:
                    _stream.WriteByte(DataBytesDefinition.Byte8);
                    return;
                case 9:
                    _stream.WriteByte(DataBytesDefinition.Byte9);
                    return;
                case 10:
                    _stream.WriteByte(DataBytesDefinition.Byte10);
                    return;
                case 11:
                    _stream.WriteByte(DataBytesDefinition.Byte11);
                    return;
                case 12:
                    _stream.WriteByte(DataBytesDefinition.Byte12);
                    return;
                case 13:
                    _stream.WriteByte(DataBytesDefinition.Byte13);
                    return;
                case 14:
                    _stream.WriteByte(DataBytesDefinition.Byte14);
                    return;
                case 15:
                    _stream.WriteByte(DataBytesDefinition.Byte15);
                    return;
                case 16:
                    _stream.WriteByte(DataBytesDefinition.Byte16);
                    return;
                case 17:
                    _stream.WriteByte(DataBytesDefinition.Byte17);
                    return;
                case 18:
                    _stream.WriteByte(DataBytesDefinition.Byte18);
                    return;
                case 19:
                    _stream.WriteByte(DataBytesDefinition.Byte19);
                    return;
                case 20:
                    _stream.WriteByte(DataBytesDefinition.Byte20);
                    return;
            }
            #endregion
            var objIdx = _longCache.SerializerGet(value);
            if (objIdx > -1)
            {
                if (objIdx <= byte.MaxValue)
                    WriteByte(DataBytesDefinition.RefLongByte, (byte)objIdx);
                else
                    WriteUshort(DataBytesDefinition.RefLongUShort, (ushort)objIdx);
            }
            else
            {
                WriteLong(DataBytesDefinition.Long, value);
                _longCache.SerializerSet(value);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ulong value)
        {
            #region Static Values
            switch (value)
            {
                case 0:
                    _stream.WriteByte(DataBytesDefinition.ByteDefault);
                    return;
                case 1:
                    _stream.WriteByte(DataBytesDefinition.Byte1);
                    return;
                case 2:
                    _stream.WriteByte(DataBytesDefinition.Byte2);
                    return;
                case 3:
                    _stream.WriteByte(DataBytesDefinition.Byte3);
                    return;
                case 4:
                    _stream.WriteByte(DataBytesDefinition.Byte4);
                    return;
                case 5:
                    _stream.WriteByte(DataBytesDefinition.Byte5);
                    return;
                case 6:
                    _stream.WriteByte(DataBytesDefinition.Byte6);
                    return;
                case 7:
                    _stream.WriteByte(DataBytesDefinition.Byte7);
                    return;
                case 8:
                    _stream.WriteByte(DataBytesDefinition.Byte8);
                    return;
                case 9:
                    _stream.WriteByte(DataBytesDefinition.Byte9);
                    return;
                case 10:
                    _stream.WriteByte(DataBytesDefinition.Byte10);
                    return;
                case 11:
                    _stream.WriteByte(DataBytesDefinition.Byte11);
                    return;
                case 12:
                    _stream.WriteByte(DataBytesDefinition.Byte12);
                    return;
                case 13:
                    _stream.WriteByte(DataBytesDefinition.Byte13);
                    return;
                case 14:
                    _stream.WriteByte(DataBytesDefinition.Byte14);
                    return;
                case 15:
                    _stream.WriteByte(DataBytesDefinition.Byte15);
                    return;
                case 16:
                    _stream.WriteByte(DataBytesDefinition.Byte16);
                    return;
                case 17:
                    _stream.WriteByte(DataBytesDefinition.Byte17);
                    return;
                case 18:
                    _stream.WriteByte(DataBytesDefinition.Byte18);
                    return;
                case 19:
                    _stream.WriteByte(DataBytesDefinition.Byte19);
                    return;
                case 20:
                    _stream.WriteByte(DataBytesDefinition.Byte20);
                    return;
            }
            #endregion
            var objIdx = _uLongCache.SerializerGet(value);
            if (objIdx > -1)
            {
                if (objIdx <= byte.MaxValue)
                    WriteByte(DataBytesDefinition.RefULongByte, (byte)objIdx);
                else
                    WriteUshort(DataBytesDefinition.RefULongUShort, (ushort)objIdx);
            }
            else
            {
                WriteULong(DataBytesDefinition.ULong, value);
                _uLongCache.SerializerSet(value);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(int value)
        {
            #region Static Values
            switch (value)
            {
                case -1:
                    _stream.WriteByte(DataBytesDefinition.SByteMinusOne);
                    return;
                case 0:
                    _stream.WriteByte(DataBytesDefinition.ByteDefault);
                    return;
                case 1:
                    _stream.WriteByte(DataBytesDefinition.Byte1);
                    return;
                case 2:
                    _stream.WriteByte(DataBytesDefinition.Byte2);
                    return;
                case 3:
                    _stream.WriteByte(DataBytesDefinition.Byte3);
                    return;
                case 4:
                    _stream.WriteByte(DataBytesDefinition.Byte4);
                    return;
                case 5:
                    _stream.WriteByte(DataBytesDefinition.Byte5);
                    return;
                case 6:
                    _stream.WriteByte(DataBytesDefinition.Byte6);
                    return;
                case 7:
                    _stream.WriteByte(DataBytesDefinition.Byte7);
                    return;
                case 8:
                    _stream.WriteByte(DataBytesDefinition.Byte8);
                    return;
                case 9:
                    _stream.WriteByte(DataBytesDefinition.Byte9);
                    return;
                case 10:
                    _stream.WriteByte(DataBytesDefinition.Byte10);
                    return;
                case 11:
                    _stream.WriteByte(DataBytesDefinition.Byte11);
                    return;
                case 12:
                    _stream.WriteByte(DataBytesDefinition.Byte12);
                    return;
                case 13:
                    _stream.WriteByte(DataBytesDefinition.Byte13);
                    return;
                case 14:
                    _stream.WriteByte(DataBytesDefinition.Byte14);
                    return;
                case 15:
                    _stream.WriteByte(DataBytesDefinition.Byte15);
                    return;
                case 16:
                    _stream.WriteByte(DataBytesDefinition.Byte16);
                    return;
                case 17:
                    _stream.WriteByte(DataBytesDefinition.Byte17);
                    return;
                case 18:
                    _stream.WriteByte(DataBytesDefinition.Byte18);
                    return;
                case 19:
                    _stream.WriteByte(DataBytesDefinition.Byte19);
                    return;
                case 20:
                    _stream.WriteByte(DataBytesDefinition.Byte20);
                    return;
            }
            #endregion

            var objIdx = _intCache.SerializerGet(value);
            if (objIdx > -1)
            {
                if (objIdx <= byte.MaxValue)
                    WriteByte(DataBytesDefinition.RefIntByte, (byte)objIdx);
                else
                    WriteUshort(DataBytesDefinition.RefIntUShort, (ushort)objIdx);
            }
            else
            {
                WriteInt(DataBytesDefinition.Int, value);
                _intCache.SerializerSet(value);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(uint value)
        {
            #region Static Values
            switch (value)
            {
                case 0:
                    _stream.WriteByte(DataBytesDefinition.ByteDefault);
                    return;
                case 1:
                    _stream.WriteByte(DataBytesDefinition.Byte1);
                    return;
                case 2:
                    _stream.WriteByte(DataBytesDefinition.Byte2);
                    return;
                case 3:
                    _stream.WriteByte(DataBytesDefinition.Byte3);
                    return;
                case 4:
                    _stream.WriteByte(DataBytesDefinition.Byte4);
                    return;
                case 5:
                    _stream.WriteByte(DataBytesDefinition.Byte5);
                    return;
                case 6:
                    _stream.WriteByte(DataBytesDefinition.Byte6);
                    return;
                case 7:
                    _stream.WriteByte(DataBytesDefinition.Byte7);
                    return;
                case 8:
                    _stream.WriteByte(DataBytesDefinition.Byte8);
                    return;
                case 9:
                    _stream.WriteByte(DataBytesDefinition.Byte9);
                    return;
                case 10:
                    _stream.WriteByte(DataBytesDefinition.Byte10);
                    return;
                case 11:
                    _stream.WriteByte(DataBytesDefinition.Byte11);
                    return;
                case 12:
                    _stream.WriteByte(DataBytesDefinition.Byte12);
                    return;
                case 13:
                    _stream.WriteByte(DataBytesDefinition.Byte13);
                    return;
                case 14:
                    _stream.WriteByte(DataBytesDefinition.Byte14);
                    return;
                case 15:
                    _stream.WriteByte(DataBytesDefinition.Byte15);
                    return;
                case 16:
                    _stream.WriteByte(DataBytesDefinition.Byte16);
                    return;
                case 17:
                    _stream.WriteByte(DataBytesDefinition.Byte17);
                    return;
                case 18:
                    _stream.WriteByte(DataBytesDefinition.Byte18);
                    return;
                case 19:
                    _stream.WriteByte(DataBytesDefinition.Byte19);
                    return;
                case 20:
                    _stream.WriteByte(DataBytesDefinition.Byte20);
                    return;
            }
            #endregion

            var objIdx = _uIntCache.SerializerGet(value);
            if (objIdx > -1)
            {
                if (objIdx <= byte.MaxValue)
                    WriteByte(DataBytesDefinition.RefUIntByte, (byte)objIdx);
                else
                    WriteUshort(DataBytesDefinition.RefUIntUShort, (ushort)objIdx);
            }
            else
            {
                WriteUInt(DataBytesDefinition.UInt, value);
                _uIntCache.SerializerSet(value);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(short value)
        {
            #region Static Values
            switch (value)
            {
                case -1:
                    _stream.WriteByte(DataBytesDefinition.SByteMinusOne);
                    return;
                case 0:
                    _stream.WriteByte(DataBytesDefinition.ByteDefault);
                    return;
                case 1:
                    _stream.WriteByte(DataBytesDefinition.Byte1);
                    return;
                case 2:
                    _stream.WriteByte(DataBytesDefinition.Byte2);
                    return;
                case 3:
                    _stream.WriteByte(DataBytesDefinition.Byte3);
                    return;
                case 4:
                    _stream.WriteByte(DataBytesDefinition.Byte4);
                    return;
                case 5:
                    _stream.WriteByte(DataBytesDefinition.Byte5);
                    return;
                case 6:
                    _stream.WriteByte(DataBytesDefinition.Byte6);
                    return;
                case 7:
                    _stream.WriteByte(DataBytesDefinition.Byte7);
                    return;
                case 8:
                    _stream.WriteByte(DataBytesDefinition.Byte8);
                    return;
                case 9:
                    _stream.WriteByte(DataBytesDefinition.Byte9);
                    return;
                case 10:
                    _stream.WriteByte(DataBytesDefinition.Byte10);
                    return;
                case 11:
                    _stream.WriteByte(DataBytesDefinition.Byte11);
                    return;
                case 12:
                    _stream.WriteByte(DataBytesDefinition.Byte12);
                    return;
                case 13:
                    _stream.WriteByte(DataBytesDefinition.Byte13);
                    return;
                case 14:
                    _stream.WriteByte(DataBytesDefinition.Byte14);
                    return;
                case 15:
                    _stream.WriteByte(DataBytesDefinition.Byte15);
                    return;
                case 16:
                    _stream.WriteByte(DataBytesDefinition.Byte16);
                    return;
                case 17:
                    _stream.WriteByte(DataBytesDefinition.Byte17);
                    return;
                case 18:
                    _stream.WriteByte(DataBytesDefinition.Byte18);
                    return;
                case 19:
                    _stream.WriteByte(DataBytesDefinition.Byte19);
                    return;
                case 20:
                    _stream.WriteByte(DataBytesDefinition.Byte20);
                    return;
            }
            #endregion

            var objIdx = _shortCache.SerializerGet(value);
            if (objIdx > -1 && objIdx <= byte.MaxValue)
                WriteByte(DataBytesDefinition.RefShortByte, (byte)objIdx);
            else
            {
                WriteShort(DataBytesDefinition.Short, value);
                _shortCache.SerializerSet(value);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(ushort value)
        {
            #region Static Values
            switch (value)
            {
                case 0:
                    _stream.WriteByte(DataBytesDefinition.ByteDefault);
                    return;
                case 1:
                    _stream.WriteByte(DataBytesDefinition.Byte1);
                    return;
                case 2:
                    _stream.WriteByte(DataBytesDefinition.Byte2);
                    return;
                case 3:
                    _stream.WriteByte(DataBytesDefinition.Byte3);
                    return;
                case 4:
                    _stream.WriteByte(DataBytesDefinition.Byte4);
                    return;
                case 5:
                    _stream.WriteByte(DataBytesDefinition.Byte5);
                    return;
                case 6:
                    _stream.WriteByte(DataBytesDefinition.Byte6);
                    return;
                case 7:
                    _stream.WriteByte(DataBytesDefinition.Byte7);
                    return;
                case 8:
                    _stream.WriteByte(DataBytesDefinition.Byte8);
                    return;
                case 9:
                    _stream.WriteByte(DataBytesDefinition.Byte9);
                    return;
                case 10:
                    _stream.WriteByte(DataBytesDefinition.Byte10);
                    return;
                case 11:
                    _stream.WriteByte(DataBytesDefinition.Byte11);
                    return;
                case 12:
                    _stream.WriteByte(DataBytesDefinition.Byte12);
                    return;
                case 13:
                    _stream.WriteByte(DataBytesDefinition.Byte13);
                    return;
                case 14:
                    _stream.WriteByte(DataBytesDefinition.Byte14);
                    return;
                case 15:
                    _stream.WriteByte(DataBytesDefinition.Byte15);
                    return;
                case 16:
                    _stream.WriteByte(DataBytesDefinition.Byte16);
                    return;
                case 17:
                    _stream.WriteByte(DataBytesDefinition.Byte17);
                    return;
                case 18:
                    _stream.WriteByte(DataBytesDefinition.Byte18);
                    return;
                case 19:
                    _stream.WriteByte(DataBytesDefinition.Byte19);
                    return;
                case 20:
                    _stream.WriteByte(DataBytesDefinition.Byte20);
                    return;
            }
            #endregion

            var objIdx = _uShortCache.SerializerGet(value);
            if (objIdx > -1 && objIdx <= byte.MaxValue)
                WriteByte(DataBytesDefinition.RefUShortByte, (byte)objIdx);
            else
            {
                WriteUshort(DataBytesDefinition.UShort, value);
                _uShortCache.SerializerSet(value);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(byte value)
        {
            #region Static Values
            switch (value)
            {
                case 0:
                    _stream.WriteByte(DataBytesDefinition.ByteDefault);
                    return;
                case 1:
                    _stream.WriteByte(DataBytesDefinition.Byte1);
                    return;
                case 2:
                    _stream.WriteByte(DataBytesDefinition.Byte2);
                    return;
                case 3:
                    _stream.WriteByte(DataBytesDefinition.Byte3);
                    return;
                case 4:
                    _stream.WriteByte(DataBytesDefinition.Byte4);
                    return;
                case 5:
                    _stream.WriteByte(DataBytesDefinition.Byte5);
                    return;
                case 6:
                    _stream.WriteByte(DataBytesDefinition.Byte6);
                    return;
                case 7:
                    _stream.WriteByte(DataBytesDefinition.Byte7);
                    return;
                case 8:
                    _stream.WriteByte(DataBytesDefinition.Byte8);
                    return;
                case 9:
                    _stream.WriteByte(DataBytesDefinition.Byte9);
                    return;
                case 10:
                    _stream.WriteByte(DataBytesDefinition.Byte10);
                    return;
                case 11:
                    _stream.WriteByte(DataBytesDefinition.Byte11);
                    return;
                case 12:
                    _stream.WriteByte(DataBytesDefinition.Byte12);
                    return;
                case 13:
                    _stream.WriteByte(DataBytesDefinition.Byte13);
                    return;
                case 14:
                    _stream.WriteByte(DataBytesDefinition.Byte14);
                    return;
                case 15:
                    _stream.WriteByte(DataBytesDefinition.Byte15);
                    return;
                case 16:
                    _stream.WriteByte(DataBytesDefinition.Byte16);
                    return;
                case 17:
                    _stream.WriteByte(DataBytesDefinition.Byte17);
                    return;
                case 18:
                    _stream.WriteByte(DataBytesDefinition.Byte18);
                    return;
                case 19:
                    _stream.WriteByte(DataBytesDefinition.Byte19);
                    return;
                case 20:
                    _stream.WriteByte(DataBytesDefinition.Byte20);
                    return;
            }
            #endregion

            WriteByte(DataBytesDefinition.Byte, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(sbyte value)
        {
            #region Static Values
            switch (value)
            {
                case -1:
                    _stream.WriteByte(DataBytesDefinition.SByteMinusOne);
                    return;
                case 0:
                    _stream.WriteByte(DataBytesDefinition.ByteDefault);
                    return;
                case 1:
                    _stream.WriteByte(DataBytesDefinition.Byte1);
                    return;
                case 2:
                    _stream.WriteByte(DataBytesDefinition.Byte2);
                    return;
                case 3:
                    _stream.WriteByte(DataBytesDefinition.Byte3);
                    return;
                case 4:
                    _stream.WriteByte(DataBytesDefinition.Byte4);
                    return;
                case 5:
                    _stream.WriteByte(DataBytesDefinition.Byte5);
                    return;
                case 6:
                    _stream.WriteByte(DataBytesDefinition.Byte6);
                    return;
                case 7:
                    _stream.WriteByte(DataBytesDefinition.Byte7);
                    return;
                case 8:
                    _stream.WriteByte(DataBytesDefinition.Byte8);
                    return;
                case 9:
                    _stream.WriteByte(DataBytesDefinition.Byte9);
                    return;
                case 10:
                    _stream.WriteByte(DataBytesDefinition.Byte10);
                    return;
                case 11:
                    _stream.WriteByte(DataBytesDefinition.Byte11);
                    return;
                case 12:
                    _stream.WriteByte(DataBytesDefinition.Byte12);
                    return;
                case 13:
                    _stream.WriteByte(DataBytesDefinition.Byte13);
                    return;
                case 14:
                    _stream.WriteByte(DataBytesDefinition.Byte14);
                    return;
                case 15:
                    _stream.WriteByte(DataBytesDefinition.Byte15);
                    return;
                case 16:
                    _stream.WriteByte(DataBytesDefinition.Byte16);
                    return;
                case 17:
                    _stream.WriteByte(DataBytesDefinition.Byte17);
                    return;
                case 18:
                    _stream.WriteByte(DataBytesDefinition.Byte18);
                    return;
                case 19:
                    _stream.WriteByte(DataBytesDefinition.Byte19);
                    return;
                case 20:
                    _stream.WriteByte(DataBytesDefinition.Byte20);
                    return;
            }
            #endregion

            switch (value)
            {
                default:
                    _stream.WriteByte(DataBytesDefinition.SByte);
                    WriteSByte(value);
                    return;
            }
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
        private SerializerCache<int> _intCache;
        private SerializerCache<uint> _uIntCache;
        private SerializerCache<short> _shortCache;
        private SerializerCache<ushort> _uShortCache;

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
            _intCache = new SerializerCache<int>();
            _uIntCache = new SerializerCache<uint>();
            _shortCache = new SerializerCache<short>();
            _uShortCache = new SerializerCache<ushort>();
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
            _intCache?.Clear();
            _uIntCache?.Clear();
            _shortCache?.Clear();
            _uShortCache?.Clear();
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
                case DataBytesDefinition.ByteDefault:
                    return 0;
                case DataBytesDefinition.Byte1:
                    return 1;
                case DataBytesDefinition.Byte2:
                    return 2;
                case DataBytesDefinition.Byte3:
                    return 3;
                case DataBytesDefinition.Byte4:
                    return 4;
                case DataBytesDefinition.Byte5:
                    return 5;
                case DataBytesDefinition.Byte6:
                    return 6;
                case DataBytesDefinition.Byte7:
                    return 7;
                case DataBytesDefinition.Byte8:
                    return 8;
                case DataBytesDefinition.Byte9:
                    return 9;
                case DataBytesDefinition.Byte10:
                    return 10;
                case DataBytesDefinition.Byte11:
                    return 11;
                case DataBytesDefinition.Byte12:
                    return 12;
                case DataBytesDefinition.Byte13:
                    return 13;
                case DataBytesDefinition.Byte14:
                    return 14;
                case DataBytesDefinition.Byte15:
                    return 15;
                case DataBytesDefinition.Byte16:
                    return 16;
                case DataBytesDefinition.Byte17:
                    return 17;
                case DataBytesDefinition.Byte18:
                    return 18;
                case DataBytesDefinition.Byte19:
                    return 19;
                case DataBytesDefinition.Byte20:
                    return 20;
                case DataBytesDefinition.Decimal:
                    var v1 = reader.ReadDecimal();
                    _decimalCache.DeserializerSet(v1);
                    return v1;
                case DataBytesDefinition.DecimalDefault:
                    return default(decimal);
                case DataBytesDefinition.RefDecimalByte:
                    return _decimalCache.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefDecimalUShort:
                    return _decimalCache.DeserializerGet(reader.ReadUInt16());
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
                case DataBytesDefinition.ByteDefault:
                    return 0;
                case DataBytesDefinition.Byte1:
                    return 1;
                case DataBytesDefinition.Byte2:
                    return 2;
                case DataBytesDefinition.Byte3:
                    return 3;
                case DataBytesDefinition.Byte4:
                    return 4;
                case DataBytesDefinition.Byte5:
                    return 5;
                case DataBytesDefinition.Byte6:
                    return 6;
                case DataBytesDefinition.Byte7:
                    return 7;
                case DataBytesDefinition.Byte8:
                    return 8;
                case DataBytesDefinition.Byte9:
                    return 9;
                case DataBytesDefinition.Byte10:
                    return 10;
                case DataBytesDefinition.Byte11:
                    return 11;
                case DataBytesDefinition.Byte12:
                    return 12;
                case DataBytesDefinition.Byte13:
                    return 13;
                case DataBytesDefinition.Byte14:
                    return 14;
                case DataBytesDefinition.Byte15:
                    return 15;
                case DataBytesDefinition.Byte16:
                    return 16;
                case DataBytesDefinition.Byte17:
                    return 17;
                case DataBytesDefinition.Byte18:
                    return 18;
                case DataBytesDefinition.Byte19:
                    return 19;
                case DataBytesDefinition.Byte20:
                    return 20;
                case DataBytesDefinition.Double:
                    var v2 = reader.ReadDouble();
                    _doubleCache.DeserializerSet(v2);
                    return v2;
                case DataBytesDefinition.DoubleDefault:
                    return default(double);
                case DataBytesDefinition.RefDoubleByte:
                    return _doubleCache.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefDoubleUShort:
                    return _doubleCache.DeserializerGet(reader.ReadUInt16());
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
                case DataBytesDefinition.ByteDefault:
                    return 0;
                case DataBytesDefinition.Byte1:
                    return 1;
                case DataBytesDefinition.Byte2:
                    return 2;
                case DataBytesDefinition.Byte3:
                    return 3;
                case DataBytesDefinition.Byte4:
                    return 4;
                case DataBytesDefinition.Byte5:
                    return 5;
                case DataBytesDefinition.Byte6:
                    return 6;
                case DataBytesDefinition.Byte7:
                    return 7;
                case DataBytesDefinition.Byte8:
                    return 8;
                case DataBytesDefinition.Byte9:
                    return 9;
                case DataBytesDefinition.Byte10:
                    return 10;
                case DataBytesDefinition.Byte11:
                    return 11;
                case DataBytesDefinition.Byte12:
                    return 12;
                case DataBytesDefinition.Byte13:
                    return 13;
                case DataBytesDefinition.Byte14:
                    return 14;
                case DataBytesDefinition.Byte15:
                    return 15;
                case DataBytesDefinition.Byte16:
                    return 16;
                case DataBytesDefinition.Byte17:
                    return 17;
                case DataBytesDefinition.Byte18:
                    return 18;
                case DataBytesDefinition.Byte19:
                    return 19;
                case DataBytesDefinition.Byte20:
                    return 20;
                case DataBytesDefinition.Float:
                    var v3 = reader.ReadSingle();
                    _floatCache.DeserializerSet(v3);
                    return v3;
                case DataBytesDefinition.FloatDefault:
                    return default(float);
                case DataBytesDefinition.RefFloatByte:
                    return _floatCache.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefFloatUShort:
                    return _floatCache.DeserializerGet(reader.ReadUInt16());
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
                case DataBytesDefinition.ByteDefault:
                    return 0;
                case DataBytesDefinition.Byte1:
                    return 1;
                case DataBytesDefinition.Byte2:
                    return 2;
                case DataBytesDefinition.Byte3:
                    return 3;
                case DataBytesDefinition.Byte4:
                    return 4;
                case DataBytesDefinition.Byte5:
                    return 5;
                case DataBytesDefinition.Byte6:
                    return 6;
                case DataBytesDefinition.Byte7:
                    return 7;
                case DataBytesDefinition.Byte8:
                    return 8;
                case DataBytesDefinition.Byte9:
                    return 9;
                case DataBytesDefinition.Byte10:
                    return 10;
                case DataBytesDefinition.Byte11:
                    return 11;
                case DataBytesDefinition.Byte12:
                    return 12;
                case DataBytesDefinition.Byte13:
                    return 13;
                case DataBytesDefinition.Byte14:
                    return 14;
                case DataBytesDefinition.Byte15:
                    return 15;
                case DataBytesDefinition.Byte16:
                    return 16;
                case DataBytesDefinition.Byte17:
                    return 17;
                case DataBytesDefinition.Byte18:
                    return 18;
                case DataBytesDefinition.Byte19:
                    return 19;
                case DataBytesDefinition.Byte20:
                    return 20;
                case DataBytesDefinition.Long:
                    var v4 = reader.ReadInt64();
                    _longCache.DeserializerSet(v4);
                    return v4;
                case DataBytesDefinition.RefLongByte:
                    return _longCache.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefLongUShort:
                    return _longCache.DeserializerGet(reader.ReadUInt16());
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
                case DataBytesDefinition.ByteDefault:
                    return 0;
                case DataBytesDefinition.Byte1:
                    return 1;
                case DataBytesDefinition.Byte2:
                    return 2;
                case DataBytesDefinition.Byte3:
                    return 3;
                case DataBytesDefinition.Byte4:
                    return 4;
                case DataBytesDefinition.Byte5:
                    return 5;
                case DataBytesDefinition.Byte6:
                    return 6;
                case DataBytesDefinition.Byte7:
                    return 7;
                case DataBytesDefinition.Byte8:
                    return 8;
                case DataBytesDefinition.Byte9:
                    return 9;
                case DataBytesDefinition.Byte10:
                    return 10;
                case DataBytesDefinition.Byte11:
                    return 11;
                case DataBytesDefinition.Byte12:
                    return 12;
                case DataBytesDefinition.Byte13:
                    return 13;
                case DataBytesDefinition.Byte14:
                    return 14;
                case DataBytesDefinition.Byte15:
                    return 15;
                case DataBytesDefinition.Byte16:
                    return 16;
                case DataBytesDefinition.Byte17:
                    return 17;
                case DataBytesDefinition.Byte18:
                    return 18;
                case DataBytesDefinition.Byte19:
                    return 19;
                case DataBytesDefinition.Byte20:
                    return 20;
                case DataBytesDefinition.ULong:
                    var v5 = reader.ReadUInt64();
                    _uLongCache.DeserializerSet(v5);
                    return v5;
                case DataBytesDefinition.RefULongByte:
                    return _uLongCache.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefULongUShort:
                    return _uLongCache.DeserializerGet(reader.ReadUInt16());
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
                case DataBytesDefinition.ByteDefault:
                    return 0;
                case DataBytesDefinition.Byte1:
                    return 1;
                case DataBytesDefinition.Byte2:
                    return 2;
                case DataBytesDefinition.Byte3:
                    return 3;
                case DataBytesDefinition.Byte4:
                    return 4;
                case DataBytesDefinition.Byte5:
                    return 5;
                case DataBytesDefinition.Byte6:
                    return 6;
                case DataBytesDefinition.Byte7:
                    return 7;
                case DataBytesDefinition.Byte8:
                    return 8;
                case DataBytesDefinition.Byte9:
                    return 9;
                case DataBytesDefinition.Byte10:
                    return 10;
                case DataBytesDefinition.Byte11:
                    return 11;
                case DataBytesDefinition.Byte12:
                    return 12;
                case DataBytesDefinition.Byte13:
                    return 13;
                case DataBytesDefinition.Byte14:
                    return 14;
                case DataBytesDefinition.Byte15:
                    return 15;
                case DataBytesDefinition.Byte16:
                    return 16;
                case DataBytesDefinition.Byte17:
                    return 17;
                case DataBytesDefinition.Byte18:
                    return 18;
                case DataBytesDefinition.Byte19:
                    return 19;
                case DataBytesDefinition.Byte20:
                    return 20;
                case DataBytesDefinition.Int:
                    var v6 = reader.ReadInt32();
                    _intCache.DeserializerSet(v6);
                    return v6;
                case DataBytesDefinition.RefIntByte:
                    return _intCache.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefIntUShort:
                    return _intCache.DeserializerGet(reader.ReadUInt16());
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
                case DataBytesDefinition.ByteDefault:
                    return 0;
                case DataBytesDefinition.Byte1:
                    return 1;
                case DataBytesDefinition.Byte2:
                    return 2;
                case DataBytesDefinition.Byte3:
                    return 3;
                case DataBytesDefinition.Byte4:
                    return 4;
                case DataBytesDefinition.Byte5:
                    return 5;
                case DataBytesDefinition.Byte6:
                    return 6;
                case DataBytesDefinition.Byte7:
                    return 7;
                case DataBytesDefinition.Byte8:
                    return 8;
                case DataBytesDefinition.Byte9:
                    return 9;
                case DataBytesDefinition.Byte10:
                    return 10;
                case DataBytesDefinition.Byte11:
                    return 11;
                case DataBytesDefinition.Byte12:
                    return 12;
                case DataBytesDefinition.Byte13:
                    return 13;
                case DataBytesDefinition.Byte14:
                    return 14;
                case DataBytesDefinition.Byte15:
                    return 15;
                case DataBytesDefinition.Byte16:
                    return 16;
                case DataBytesDefinition.Byte17:
                    return 17;
                case DataBytesDefinition.Byte18:
                    return 18;
                case DataBytesDefinition.Byte19:
                    return 19;
                case DataBytesDefinition.Byte20:
                    return 20;
                case DataBytesDefinition.UInt:
                    var v7 = reader.ReadUInt32();
                    _uIntCache.DeserializerSet(v7);
                    return v7;
                case DataBytesDefinition.RefUIntByte:
                    return _uIntCache.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefUIntUShort:
                    return _uIntCache.DeserializerGet(reader.ReadUInt16());
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
                case DataBytesDefinition.ByteDefault:
                    return 0;
                case DataBytesDefinition.Byte1:
                    return 1;
                case DataBytesDefinition.Byte2:
                    return 2;
                case DataBytesDefinition.Byte3:
                    return 3;
                case DataBytesDefinition.Byte4:
                    return 4;
                case DataBytesDefinition.Byte5:
                    return 5;
                case DataBytesDefinition.Byte6:
                    return 6;
                case DataBytesDefinition.Byte7:
                    return 7;
                case DataBytesDefinition.Byte8:
                    return 8;
                case DataBytesDefinition.Byte9:
                    return 9;
                case DataBytesDefinition.Byte10:
                    return 10;
                case DataBytesDefinition.Byte11:
                    return 11;
                case DataBytesDefinition.Byte12:
                    return 12;
                case DataBytesDefinition.Byte13:
                    return 13;
                case DataBytesDefinition.Byte14:
                    return 14;
                case DataBytesDefinition.Byte15:
                    return 15;
                case DataBytesDefinition.Byte16:
                    return 16;
                case DataBytesDefinition.Byte17:
                    return 17;
                case DataBytesDefinition.Byte18:
                    return 18;
                case DataBytesDefinition.Byte19:
                    return 19;
                case DataBytesDefinition.Byte20:
                    return 20;
                case DataBytesDefinition.Short:
                    var v8 = reader.ReadInt16();
                    _shortCache.DeserializerSet(v8);
                    return v8;
                case DataBytesDefinition.RefShortByte:
                    return _shortCache.DeserializerGet(reader.ReadByte());
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
                case DataBytesDefinition.ByteDefault:
                    return 0;
                case DataBytesDefinition.Byte1:
                    return 1;
                case DataBytesDefinition.Byte2:
                    return 2;
                case DataBytesDefinition.Byte3:
                    return 3;
                case DataBytesDefinition.Byte4:
                    return 4;
                case DataBytesDefinition.Byte5:
                    return 5;
                case DataBytesDefinition.Byte6:
                    return 6;
                case DataBytesDefinition.Byte7:
                    return 7;
                case DataBytesDefinition.Byte8:
                    return 8;
                case DataBytesDefinition.Byte9:
                    return 9;
                case DataBytesDefinition.Byte10:
                    return 10;
                case DataBytesDefinition.Byte11:
                    return 11;
                case DataBytesDefinition.Byte12:
                    return 12;
                case DataBytesDefinition.Byte13:
                    return 13;
                case DataBytesDefinition.Byte14:
                    return 14;
                case DataBytesDefinition.Byte15:
                    return 15;
                case DataBytesDefinition.Byte16:
                    return 16;
                case DataBytesDefinition.Byte17:
                    return 17;
                case DataBytesDefinition.Byte18:
                    return 18;
                case DataBytesDefinition.Byte19:
                    return 19;
                case DataBytesDefinition.Byte20:
                    return 20;
                case DataBytesDefinition.UShort:
                    var v9 = reader.ReadUInt16();
                    _uShortCache.DeserializerSet(v9);
                    return v9;
                case DataBytesDefinition.RefUShortByte:
                    return _uShortCache.DeserializerGet(reader.ReadByte());
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
                case DataBytesDefinition.ByteDefault:
                    return 0;
                case DataBytesDefinition.Byte1:
                    return 1;
                case DataBytesDefinition.Byte2:
                    return 2;
                case DataBytesDefinition.Byte3:
                    return 3;
                case DataBytesDefinition.Byte4:
                    return 4;
                case DataBytesDefinition.Byte5:
                    return 5;
                case DataBytesDefinition.Byte6:
                    return 6;
                case DataBytesDefinition.Byte7:
                    return 7;
                case DataBytesDefinition.Byte8:
                    return 8;
                case DataBytesDefinition.Byte9:
                    return 9;
                case DataBytesDefinition.Byte10:
                    return 10;
                case DataBytesDefinition.Byte11:
                    return 11;
                case DataBytesDefinition.Byte12:
                    return 12;
                case DataBytesDefinition.Byte13:
                    return 13;
                case DataBytesDefinition.Byte14:
                    return 14;
                case DataBytesDefinition.Byte15:
                    return 15;
                case DataBytesDefinition.Byte16:
                    return 16;
                case DataBytesDefinition.Byte17:
                    return 17;
                case DataBytesDefinition.Byte18:
                    return 18;
                case DataBytesDefinition.Byte19:
                    return 19;
                case DataBytesDefinition.Byte20:
                    return 20;
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
                case DataBytesDefinition.ByteDefault:
                    return 0;
                case DataBytesDefinition.Byte1:
                    return 1;
                case DataBytesDefinition.Byte2:
                    return 2;
                case DataBytesDefinition.Byte3:
                    return 3;
                case DataBytesDefinition.Byte4:
                    return 4;
                case DataBytesDefinition.Byte5:
                    return 5;
                case DataBytesDefinition.Byte6:
                    return 6;
                case DataBytesDefinition.Byte7:
                    return 7;
                case DataBytesDefinition.Byte8:
                    return 8;
                case DataBytesDefinition.Byte9:
                    return 9;
                case DataBytesDefinition.Byte10:
                    return 10;
                case DataBytesDefinition.Byte11:
                    return 11;
                case DataBytesDefinition.Byte12:
                    return 12;
                case DataBytesDefinition.Byte13:
                    return 13;
                case DataBytesDefinition.Byte14:
                    return 14;
                case DataBytesDefinition.Byte15:
                    return 15;
                case DataBytesDefinition.Byte16:
                    return 16;
                case DataBytesDefinition.Byte17:
                    return 17;
                case DataBytesDefinition.Byte18:
                    return 18;
                case DataBytesDefinition.Byte19:
                    return 19;
                case DataBytesDefinition.Byte20:
                    return 20;
                case DataBytesDefinition.SByte:
                    return reader.ReadSByte();
                case DataBytesDefinition.SByteMinusOne:
                    return -1;
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}