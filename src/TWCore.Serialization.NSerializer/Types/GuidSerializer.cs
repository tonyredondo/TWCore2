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
        public void WriteValue(Guid value)
        {
            if (value == default)
                WriteByte(DataBytesDefinition.GuidDefault);
            else if (_guidCache.TryGetValue(value, out var objIdx))
                WriteDefInt(DataBytesDefinition.RefGuid, objIdx);
            else
            {
                Span<byte> bytes = stackalloc byte[17];
                bytes[0] = DataBytesDefinition.Guid;
                value.TryWriteBytes(bytes.Slice(1));
                Stream.Write(bytes);
                _guidCache.Set(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Guid? value)
        {
            if (value is null)
                WriteByte(DataBytesDefinition.ValueNull);
            else
                WriteValue(value.Value);
        }
    }




    public partial class DeserializersTable
    {
        [DeserializerMethod(DataBytesDefinition.GuidDefault, DataBytesDefinition.RefGuid, DataBytesDefinition.Guid, ReturnType = typeof(Guid))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid StreamReadGuid(byte type)
        {
            if (type == DataBytesDefinition.GuidDefault)
                return default;
            if (type == DataBytesDefinition.RefGuid)
                return _guidCache.Get(StreamReadInt());
            if (type == DataBytesDefinition.Guid)
            {
                var guidValue = StreamReadGuid();
                _guidCache.Set(guidValue);
                return guidValue;
            }
            throw new InvalidOperationException("Invalid type value.");
        }

        [DeserializerMethod(ReturnType = typeof(Guid?))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid? StreamReadGuidNullable(byte type)
        {
            if (type == DataBytesDefinition.ValueNull)
                return null;
            return StreamReadGuid(type);
        }
    }
}