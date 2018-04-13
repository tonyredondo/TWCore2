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
        public void WriteValue(Guid value)
        {
            if (value == default)
            {
                WriteByte(DataBytesDefinition.GuidDefault);
                return;
            }
            if (_guidCache.TryGetValue(value, out var objIdx))
            {
                WriteDefInt(DataBytesDefinition.RefGuid, objIdx);
                return;
            }
            WriteByte(DataBytesDefinition.Guid);
            var bytes = value.ToByteArray();
            Stream.Write(bytes, 0, bytes.Length);
            _guidCache.Set(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Guid? value)
        {
            if (value == null) WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
    }




    public partial class DeserializersTable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid ReadGuid(byte type)
            => ReadGuidNullable(type) ?? default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid? ReadGuidNullable(byte type)
        {
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.GuidDefault:
                    return default(Guid);
                case DataBytesDefinition.RefGuid:
                    return _guidCache.Get(ReadInt());
                case DataBytesDefinition.Guid:
                    var guidValue = ReadGuid();
                    _guidCache.Set(guidValue);
                    return guidValue;
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}