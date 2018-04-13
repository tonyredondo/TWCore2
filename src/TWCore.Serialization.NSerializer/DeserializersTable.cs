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
using System.IO;
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.NSerializer
{
    public partial class DeserializersTable
    {
        private static readonly byte[] EmptyBytes = new byte[0];
        private readonly byte[] _buffer = new byte[16];
        private readonly DeserializerCache<DateTimeOffset> _dateTimeOffsetCache = new DeserializerCache<DateTimeOffset>();
        private readonly DeserializerCache<DateTime> _dateTimeCache = new DeserializerCache<DateTime>();
        private readonly DeserializerCache<Guid> _guidCache = new DeserializerCache<Guid>();
        private readonly DeserializerCache<decimal> _decimalCache = new DeserializerCache<decimal>();
        private readonly DeserializerCache<double> _doubleCache = new DeserializerCache<double>();
        private readonly DeserializerCache<float> _floatCache = new DeserializerCache<float>();
        private readonly DeserializerCache<long> _longCache = new DeserializerCache<long>();
        private readonly DeserializerCache<ulong> _uLongCache = new DeserializerCache<ulong>();
        private readonly DeserializerStringCache _stringCache8 = new DeserializerStringCache();
        private readonly DeserializerStringCache _stringCache16 = new DeserializerStringCache();
        private readonly DeserializerStringCache _stringCache32 = new DeserializerStringCache();
        private readonly DeserializerStringCache _stringCache = new DeserializerStringCache();
        private readonly DeserializerCache<TimeSpan> _timespanCache = new DeserializerCache<TimeSpan>();


        protected Stream Stream;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Deserialize(Stream stream)
        {
            Stream = stream;



            _dateTimeOffsetCache.Clear();
            _dateTimeCache.Clear();
            _guidCache.Clear();
            _decimalCache.Clear();
            _doubleCache.Clear();
            _floatCache.Clear();
            _longCache.Clear();
            _uLongCache.Clear();
            _stringCache8.Clear();
            _stringCache16.Clear();
            _stringCache32.Clear();
            _stringCache.Clear();
            _timespanCache.Clear();
            Stream = null;
            return null;
        }


        #region Private Read Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected byte ReadByte()
        {
            Stream.Read(_buffer, 0, 1);
            return _buffer[0];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ushort ReadUShort()
        {
            Stream.Read(_buffer, 0, 2);
            return BitConverter.ToUInt16(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int ReadInt()
        {
            Stream.Read(_buffer, 0, 4);
            return BitConverter.ToInt32(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected double ReadDouble()
        {
            Stream.Read(_buffer, 0, 8);
            return BitConverter.ToDouble(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float ReadFloat()
        {
            Stream.Read(_buffer, 0, 4);
            return BitConverter.ToSingle(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected long ReadLong()
        {
            Stream.Read(_buffer, 0, 8);
            return BitConverter.ToInt64(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ulong ReadULong()
        {
            Stream.Read(_buffer, 0, 8);
            return BitConverter.ToUInt64(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected uint ReadUInt()
        {
            Stream.Read(_buffer, 0, 4);
            return BitConverter.ToUInt32(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected short ReadShort()
        {
            Stream.Read(_buffer, 0, 2);
            return BitConverter.ToInt16(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected char ReadChar()
        {
            Stream.Read(_buffer, 0, 2);
            return BitConverter.ToChar(_buffer, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected decimal ReadDecimal()
        {
            Stream.Read(_buffer, 0, 8);
            var val = BitConverter.ToDouble(_buffer, 0);
            return new decimal(val);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sbyte ReadSByte()
        {
            Stream.Read(_buffer, 0, 1);
            return (sbyte)_buffer[0];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Guid ReadGuid()
        {
            Stream.Read(_buffer, 0, 16);
            return new Guid(_buffer);
        }
        #endregion

    }
}