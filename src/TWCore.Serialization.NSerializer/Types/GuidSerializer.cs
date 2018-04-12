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
        private SerializerCache<Guid> _guidCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitGuid()
            => _guidCache = new SerializerCache<Guid>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearGuid()
            => _guidCache.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Guid value)
        {
            if (value == default)
            {
                _stream.WriteByte(DataBytesDefinition.GuidDefault);
                return;
            }
            if (_guidCache.SerializerTryGetValue(value, out var objIdx))
            {
                WriteInt(DataBytesDefinition.RefGuid, objIdx);
                return;
            }
            _stream.WriteByte(DataBytesDefinition.Guid);
            var bytes = value.ToByteArray();
            _stream.Write(bytes, 0, bytes.Length);
            _guidCache.SerializerSet(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValue(Guid? value)
        {
            if (value == null) _stream.WriteByte(DataBytesDefinition.ValueNull);
            else WriteValue(value.Value);
        }
    }

    public partial class DeserializersTable
    {
        private DeserializerCache<Guid> _guidCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitGuid()
            => _guidCache = new DeserializerCache<Guid>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearGuid()
            => _guidCache.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid ReadGuid(BinaryReader reader)
            => ReadGuidNullable(reader) ?? default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid? ReadGuidNullable(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.GuidDefault:
                    return default(Guid);
                case DataBytesDefinition.RefGuid:
                    return _guidCache.DeserializerGet(reader.ReadInt32());
                case DataBytesDefinition.Guid:
                    var bytes = reader.ReadBytes(16);
                    var guidValue = new Guid(bytes);
                    _guidCache.DeserializerSet(guidValue);
                    return guidValue;
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}