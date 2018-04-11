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

namespace TWCore.Serialization.NSerializer.Types
{
    public class DateTimeOffsetSerializer : ITypeSerializer
    {
        private SerializerCache<DateTimeOffset> _cache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
            => _cache = new SerializerCache<DateTimeOffset>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
            => _cache.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter writer, DateTimeOffset value)
        {
            if (value == default)
            {
                writer.Write(DataBytesDefinition.DateTimeOffsetDefault);
                return;
            }
            var objIdx = _cache.SerializerGet(value);
            if (objIdx > -1)
            {
                if (objIdx <= byte.MaxValue)
                    WriteHelper.WriteByte(writer, DataBytesDefinition.RefDateTimeOffsetByte, (byte)objIdx);
                else
                    WriteHelper.WriteUshort(writer, DataBytesDefinition.RefDateTimeOffsetUShort, (ushort)objIdx);
            }
            else
            {
                var longBinary = value.ToFileTime();
                WriteHelper.WriteLong(writer, DataBytesDefinition.DateTimeOffset, longBinary);
                _cache.SerializerSet(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter writer, DateTimeOffset? value)
        {
            if (value == null) writer.Write(DataBytesDefinition.ValueNull);
            else Write(writer, value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset Read(BinaryReader reader)
            => ReadNullable(reader) ?? default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset? ReadNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.DateTimeOffsetDefault:
                    return default(DateTimeOffset);
                case DataBytesDefinition.RefDateTimeOffsetByte:
                    return _cache.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefDateTimeOffsetUShort:
                    return _cache.DeserializerGet(reader.ReadUInt16());
                case DataBytesDefinition.DateTimeOffset:
                    var longBinary = reader.ReadInt64();
                    var cValue = DateTimeOffset.FromFileTime(longBinary);
                    _cache.DeserializerSet(cValue);
                    return cValue;
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }

}