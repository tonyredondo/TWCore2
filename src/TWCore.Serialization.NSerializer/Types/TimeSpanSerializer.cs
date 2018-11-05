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
#pragma warning disable 1591

namespace TWCore.Serialization.NSerializer
{
    public partial class SerializersTable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(TimeSpan value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.TimeSpanDefault);
            else if (_timespanCache.TryGetValue(value, out var objIdx))
                WriteDefInt(DataBytesDefinition.RefTimeSpan, objIdx);
            else
            {
                var longBinary = value.Ticks;
                WriteDefLong(DataBytesDefinition.TimeSpan, longBinary);
                _timespanCache.Set(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(TimeSpan? value)
        {
            if (value is null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
    }



    public partial class DeserializersTable
    {
        [DeserializerMethod(DataBytesDefinition.TimeSpanDefault, DataBytesDefinition.RefTimeSpan, DataBytesDefinition.TimeSpan, ReturnType = typeof(TimeSpan))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan ReadTimeSpan(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.TimeSpanDefault:
                    return default;
                case DataBytesDefinition.RefTimeSpan:
                    return _timespanCache.Get(StreamReadInt());
                case DataBytesDefinition.TimeSpan:
                    var longBinary = StreamReadLong();
                    var cValue = TimeSpan.FromTicks(longBinary);
                    _timespanCache.Set(cValue);
                    return cValue;
            }
            throw new InvalidOperationException($"Invalid type value. [{type}]");
        }

        [DeserializerMethod(ReturnType = typeof(TimeSpan?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan? ReadTimeSpanNullable(byte type)
        {
            if (type == DataBytesDefinition.ValueNull) return null;
            return ReadTimeSpan(type);
        }
    }
}