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

namespace TWCore.Serialization.RawSerializer
{
    public partial class SerializersTable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTimeOffset value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.DateTimeOffsetDefault);
            else
                WriteDefLong(DataBytesDefinition.DateTimeOffset, value.ToFileTime());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(DateTimeOffset? value)
        {
            if (value is null)
                WriteByte(DataBytesDefinition.ValueNull);
            else if (value == default(DateTimeOffset))
                WriteByte(DataBytesDefinition.DateTimeOffsetDefault);
            else
                WriteDefLong(DataBytesDefinition.DateTimeOffset, value.Value.ToFileTime());
        }
    }




    public partial class DeserializersTable
    {
        [DeserializerMethod(DataBytesDefinition.DateTimeOffsetDefault, DataBytesDefinition.DateTimeOffset, ReturnType = typeof(DateTimeOffset))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset ReadDateTimeOffset(byte type)
        {
            if (type == DataBytesDefinition.DateTimeOffsetDefault)
                return default;
            if (type == DataBytesDefinition.DateTimeOffset)
                return DateTimeOffset.FromFileTime(StreamReadLong());
            ThrowInvalidOperationException(type);
            return default;
        }

        [DeserializerMethod(ReturnType = typeof(DateTimeOffset?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset? ReadDateTimeOffsetNullable(byte type)
        {
            if (type == DataBytesDefinition.ValueNull)
                return null;
            if (type == DataBytesDefinition.DateTimeOffsetDefault)
                return default;
            if (type == DataBytesDefinition.DateTimeOffset)
                return DateTimeOffset.FromFileTime(StreamReadLong());
            ThrowInvalidOperationException(type);
            return default;
        }
    }

}