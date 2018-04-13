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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(TimeSpan value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.TimeSpanDefault);
                return;
            }
            if (_timespanCache.TryGetValue(value, out var objIdx))
            {
                WriteDefInt(DataBytesDefinition.RefTimeSpan, objIdx);
                return;
            }
            var longBinary = value.Ticks;
            WriteDefLong(DataBytesDefinition.TimeSpan, longBinary);
            _timespanCache.Set(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(TimeSpan? value)
        {
            if (value == null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
    }

    public partial class DeserializersTable
    {
        private DeserializerCache<TimeSpan> _timespanCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitTimeSpan()
            => _timespanCache = new DeserializerCache<TimeSpan>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearTimeSpan()
            => _timespanCache.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan ReadTimeSpan(BinaryReader reader)
            => ReadTimeSpanNullable(reader) ?? default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan? ReadTimeSpanNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.TimeSpanDefault:
                    return default(TimeSpan);
                case DataBytesDefinition.RefTimeSpan:
                    return _timespanCache.Get(reader.ReadInt32());
                case DataBytesDefinition.TimeSpan:
                    var longBinary = reader.ReadInt64();
                    var cValue = TimeSpan.FromTicks(longBinary);
                    _timespanCache.Set(cValue);
                    return cValue;
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}