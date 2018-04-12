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
    public partial class SerializersTable
    {
        private SerializerCache<DateTime> _dateTimeCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitDateTime()
            => _dateTimeCache = new SerializerCache<DateTime>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearDateTime()
            => _dateTimeCache.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTime value)
        {
            if (value == default)
            {
                _stream.WriteByte(DataBytesDefinition.DateTimeDefault);
                return;
            }
            if (_dateTimeCache.TryGetValue(value, out var objIdx))
            {
                WriteInt(DataBytesDefinition.RefDateTime, objIdx);
                return;
            }
            var longBinary = value.ToBinary();
            WriteLong(DataBytesDefinition.DateTime, longBinary);
            _dateTimeCache.Set(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTime? value)
        {
            if (value == null) _stream.WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
    }


    public partial class DeserializersTable
    {
        private DeserializerCache<DateTime> _dateTimeCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitDateTime()
            => _dateTimeCache = new DeserializerCache<DateTime>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearDateTime()
            => _dateTimeCache.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime ReadDateTime(BinaryReader reader)
            => ReadDateTimeNullable(reader) ?? default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime? ReadDateTimeNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.DateTimeDefault:
                    return default(DateTime);
                case DataBytesDefinition.RefDateTime:
                    return _dateTimeCache.Get(reader.ReadInt32());
                case DataBytesDefinition.DateTime:
                    var longBinary = reader.ReadInt64();
                    var cValue = DateTime.FromBinary(longBinary);
                    _dateTimeCache.Set(cValue);
                    return cValue;
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}