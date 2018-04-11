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
    public class GuidSerializer : ITypeSerializer
    {
        private SerializerCache<Guid> _cache;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
            => _cache = new SerializerCache<Guid>();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
            => _cache.Clear();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter writer, Guid value)
        {
            if (value == default)
            {
                writer.Write(DataBytesDefinition.GuidDefault);
                return;
            }
            var objIdx = _cache.SerializerGet(value);
            if (objIdx > -1)
            {
                if (objIdx <= byte.MaxValue)
                    WriteHelper.WriteByte(writer, DataBytesDefinition.RefGuidByte, (byte)objIdx);
                else
                    WriteHelper.WriteUshort(writer, DataBytesDefinition.RefGuidUShort, (ushort)objIdx);
            }
            else
            {
                writer.Write(DataBytesDefinition.Guid);
                var bytes = value.ToByteArray();
                writer.Write(bytes, 0, bytes.Length);
                _cache.SerializerSet(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter writer, Guid? value)
        {
            if (value == null) writer.Write(DataBytesDefinition.ValueNull);
            else Write(writer, value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid Read(BinaryReader reader)
            => ReadNullable(reader) ?? default;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid? ReadNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.GuidDefault:
                    return default(Guid);
                case DataBytesDefinition.RefGuidByte:
                    return _cache.DeserializerGet(reader.ReadByte());
                case DataBytesDefinition.RefGuidUShort:
                    return _cache.DeserializerGet(reader.ReadUInt16());
                case DataBytesDefinition.Guid:
                    var bytes = reader.ReadBytes(16);
                    var guidValue = new Guid(bytes);
                    _cache.DeserializerSet(guidValue);
                    return guidValue;
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}