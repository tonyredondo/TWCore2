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
    public class TimeSpanSerializer : TypeSerializer
    {
        private SerializerCache<TimeSpan> _cache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Init()
            => _cache = new SerializerCache<TimeSpan>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Clear()
            => _cache.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Stream stream, TimeSpan value)
        {
            if (value == default)
            {
                stream.WriteByte(DataBytesDefinition.TimeSpanDefault);
                return;
            }

            var objIdx = _cache.SerializerGet(value);
            if (objIdx > -1)
            {
                if (objIdx <= byte.MaxValue)
                    WriteByte(stream, DataBytesDefinition.RefTimeSpanByte, (byte)objIdx);
                else
                    WriteUshort(stream, DataBytesDefinition.RefTimeSpanUShort, (ushort)objIdx);
            }
            else
            {
                var longBinary = value.Ticks;
                WriteLong(stream, DataBytesDefinition.TimeSpan, longBinary);
                _cache.SerializerSet(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Stream stream, TimeSpan? value)
        {
            if (value == null) stream.WriteByte(DataBytesDefinition.ValueNull);
            else Write(stream, value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan Read(BinaryReader reader)
            => ReadNullable(reader) ?? default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan? ReadNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.TimeSpanDefault:
                    return default(TimeSpan);
                case DataBytesDefinition.RefTimeSpanByte:
                    return _cache.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefTimeSpanUShort:
                    return _cache.DeserializerGet(reader.ReadUInt16());
                case DataBytesDefinition.TimeSpan:
                    var longBinary = reader.ReadInt64();
                    var cValue = TimeSpan.FromTicks(longBinary);
                    _cache.DeserializerSet(cValue);
                    return cValue;
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}