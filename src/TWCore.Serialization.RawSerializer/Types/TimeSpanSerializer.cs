﻿/*
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
        public void WriteValue(TimeSpan value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.TimeSpanDefault);
            else
                WriteDefLong(DataBytesDefinition.TimeSpan, value.Ticks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(TimeSpan? value)
        {
            if (value is null)
                WriteByte(DataBytesDefinition.ValueNull);
            else if (value == default(TimeSpan))
                WriteByte(DataBytesDefinition.TimeSpanDefault);
            else
                WriteDefLong(DataBytesDefinition.TimeSpan, value.Value.Ticks);
        }
    }



    public partial class DeserializersTable
    {
        [DeserializerMethod(DataBytesDefinition.TimeSpanDefault, DataBytesDefinition.TimeSpan, ReturnType = typeof(TimeSpan))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan ReadTimeSpan(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.TimeSpanDefault:
                    return default;
                case DataBytesDefinition.TimeSpan:
                    return TimeSpan.FromTicks(StreamReadLong());
            }
            ThrowInvalidOperationException(type);
            return default;
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