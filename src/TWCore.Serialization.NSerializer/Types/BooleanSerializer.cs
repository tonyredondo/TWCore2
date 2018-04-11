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
    public class BooleanSerializer : TypeSerializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Init()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Clear()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Stream stream, bool value)
        {
            if (value)
                stream.WriteByte(DataBytesDefinition.BoolTrue);
            else
                stream.WriteByte(DataBytesDefinition.BoolFalse);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Stream stream, bool? value)
        {
            if (value == null)
                stream.WriteByte(DataBytesDefinition.ValueNull);
            else if (value.Value)
                stream.WriteByte(DataBytesDefinition.BoolTrue);
            else
                stream.WriteByte(DataBytesDefinition.BoolFalse);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Read(BinaryReader reader)
        {
            var value = reader.ReadByte();
            if (value == DataBytesDefinition.BoolTrue)
                return true;
            if (value == DataBytesDefinition.BoolFalse)
                return false;
            throw new InvalidOperationException("Invalid type value.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool? ReadNullable(BinaryReader reader)
        {
            var value = reader.ReadByte();
            if (value == DataBytesDefinition.ValueNull)
                return null;
            if (value == DataBytesDefinition.BoolTrue)
                return true;
            if (value == DataBytesDefinition.BoolFalse)
                return false;
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}