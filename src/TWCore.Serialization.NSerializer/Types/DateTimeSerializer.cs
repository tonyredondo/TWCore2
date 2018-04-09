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
    public class DateTimeSerializer : ITypeSerializer
    {
        private SerializerCache<DateTime> _cache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            _cache = new SerializerCache<DateTime>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _cache.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter writer, DateTime value)
        {
            if (value == default)
            {
                writer.Write(DataBytesDefinition.DateTimeDefault);
                return;
            }
            var objIdx = _cache.SerializerGet(value);
            if (objIdx > -1)
            {
                if (objIdx <= byte.MaxValue)
                    WriteHelper.WriteByte(writer, DataBytesDefinition.RefDateTimeByte, (byte)objIdx);
                else
                    WriteHelper.WriteUshort(writer, DataBytesDefinition.RefDateTimeUShort, (ushort)objIdx);
            }
            else
            {
                var longBinary = value.ToBinary();
                WriteHelper.WriteLong(writer, DataBytesDefinition.DateTime, longBinary);
                _cache.SerializerSet(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNullable(BinaryWriter writer, DateTime? value)
        {
            if (value == null) writer.Write(DataBytesDefinition.ValueNull);
            else Write(writer, value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime Read(BinaryReader reader)
        {
            return ReadNullable(reader) ?? default;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime? ReadNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            var objIdx = -1;
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.DateTimeDefault:
                    return default(DateTime);
                case DataBytesDefinition.RefDateTimeByte:
                    objIdx = reader.ReadByte();
                    break;
                case DataBytesDefinition.RefDateTimeUShort:
                    objIdx = reader.ReadUInt16();
                    break;
            }

            if (objIdx > -1)
                return _cache.DeserializerGet(objIdx);

            var longBinary = reader.ReadInt64();
            var cValue = DateTime.FromBinary(longBinary);
            _cache.DeserializerSet(cValue);
            return cValue;
        }
    }
}