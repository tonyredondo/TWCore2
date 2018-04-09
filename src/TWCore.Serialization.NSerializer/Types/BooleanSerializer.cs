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
    public struct BooleanSerializer : ITypeSerializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter writer, bool value)
        {
            writer.Write(value? DataBytesDefinition.BoolTrue : DataBytesDefinition.BoolFalse);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNullable(BinaryWriter writer, bool? value)
        {
            if (value == null) writer.Write(DataBytesDefinition.ValueNull);
            else Write(writer, value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Read(BinaryReader reader)
        {
            return reader.ReadByte() == DataBytesDefinition.BoolTrue;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool? ReadNullable(BinaryReader reader)
        {
            var boolValue = reader.ReadByte();
            if (boolValue == DataBytesDefinition.ValueNull) return null;
            return boolValue == DataBytesDefinition.BoolTrue;
        }
    }
}