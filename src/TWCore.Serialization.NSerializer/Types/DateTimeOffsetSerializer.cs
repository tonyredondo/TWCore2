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
        public void WriteValue(DateTimeOffset value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.DateTimeOffsetDefault);
                return;
            }
            if (_dateTimeOffsetCache.TryGetValue(value, out var objIdx))
            {
                WriteDefInt(DataBytesDefinition.RefDateTimeOffset, objIdx);
                return;
            }
            var longBinary = value.ToFileTime();
            WriteDefLong(DataBytesDefinition.DateTimeOffset, longBinary);
            _dateTimeOffsetCache.Set(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTimeOffset? value)
        {
            if (value == null)
                WriteByte(DataBytesDefinition.ValueNull);
            else
                WriteValue(value.Value);
        }
    }




    public partial class DeserializersTable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset ReadDateTimeOffset(byte type)
            => ReadDateTimeOffsetNullable(type) ?? default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset? ReadDateTimeOffsetNullable(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.DateTimeOffsetDefault:
                    return default(DateTimeOffset);
                case DataBytesDefinition.RefDateTimeOffset:
                    return _dateTimeOffsetCache.Get(ReadInt());
                case DataBytesDefinition.DateTimeOffset:
                    var longBinary = ReadLong();
                    var cValue = DateTimeOffset.FromFileTime(longBinary);
                    _dateTimeOffsetCache.Set(cValue);
                    return cValue;
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }

}