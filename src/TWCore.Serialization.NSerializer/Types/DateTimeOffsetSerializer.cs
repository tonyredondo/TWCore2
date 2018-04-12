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
        private SerializerCache<DateTimeOffset> _dateTimeOffsetCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitDateTimeOffset()
            => _dateTimeOffsetCache = new SerializerCache<DateTimeOffset>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearDateTimeOffset()
            => _dateTimeOffsetCache.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTimeOffset value)
        {
            if (value == default)
            {
                _stream.WriteByte(DataBytesDefinition.DateTimeOffsetDefault);
                return;
            }
            var objIdx = _dateTimeOffsetCache.SerializerGet(value);
            if (objIdx > -1)
            {
                if (objIdx <= byte.MaxValue)
                    WriteByte(DataBytesDefinition.RefDateTimeOffsetByte, (byte)objIdx);
                else
                    WriteUshort(DataBytesDefinition.RefDateTimeOffsetUShort, (ushort)objIdx);
            }
            else
            {
                var longBinary = value.ToFileTime();
                WriteLong(DataBytesDefinition.DateTimeOffset, longBinary);
                _dateTimeOffsetCache.SerializerSet(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTimeOffset? value)
        {
            if (value == null)
                _stream.WriteByte(DataBytesDefinition.ValueNull);
            else
                WriteValue(value.Value);
        }
    }


    public partial class DeserializersTable
    {
        private SerializerCache<DateTimeOffset> _dateTimeOffsetCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitDateTimeOffset()
            => _dateTimeOffsetCache = new SerializerCache<DateTimeOffset>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearDateTimeOffset()
            => _dateTimeOffsetCache.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset ReadDateTimeOffset(BinaryReader reader)
            => ReadDateTimeOffsetNullable(reader) ?? default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset? ReadDateTimeOffsetNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.DateTimeOffsetDefault:
                    return default(DateTimeOffset);
                case DataBytesDefinition.RefDateTimeOffsetByte:
                    return _dateTimeOffsetCache.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefDateTimeOffsetUShort:
                    return _dateTimeOffsetCache.DeserializerGet(reader.ReadUInt16());
                case DataBytesDefinition.DateTimeOffset:
                    var longBinary = reader.ReadInt64();
                    var cValue = DateTimeOffset.FromFileTime(longBinary);
                    _dateTimeOffsetCache.DeserializerSet(cValue);
                    return cValue;
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }

}