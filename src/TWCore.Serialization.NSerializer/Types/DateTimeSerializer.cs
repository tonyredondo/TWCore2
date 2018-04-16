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
        public void WriteValue(DateTime value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.DateTimeDefault);
                return;
            }
            if (_dateTimeCache.TryGetValue(value, out var objIdx))
            {
                WriteDefInt(DataBytesDefinition.RefDateTime, objIdx);
                return;
            }
            var longBinary = value.ToBinary();
            WriteDefLong(DataBytesDefinition.DateTime, longBinary);
            _dateTimeCache.Set(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTime? value)
        {
            if (value == null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
    }




    public partial class DeserializersTable
    {
        [DeserializerMethod(DataBytesDefinition.DateTimeDefault, DataBytesDefinition.RefDateTime, DataBytesDefinition.DateTime, ReturnType = typeof(DateTime))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime ReadDateTime(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.DateTimeDefault:
                    return default;
                case DataBytesDefinition.RefDateTime:
                    return _dateTimeCache.Get(StreamReadInt());
                case DataBytesDefinition.DateTime:
                    var longBinary = StreamReadLong();
                    var cValue = DateTime.FromBinary(longBinary);
                    _dateTimeCache.Set(cValue);
                    return cValue;
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}