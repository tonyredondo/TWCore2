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
    public class CharSerializer : TypeSerializer
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
        public void Write(BinaryWriter writer, char value)
            => writer.Write(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(BinaryWriter writer, char? value)
        {
            if (value == null) 
                writer.Write(DataBytesDefinition.ValueNull);
            else
            {
                writer.Write(DataBytesDefinition.Char);
                writer.Write(value.Value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char Read(BinaryReader reader)
            => ReadNullable(reader) ?? default;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char? ReadNullable(BinaryReader reader)
        {
            var value = reader.ReadByte();
            switch(value)
            {
                case DataBytesDefinition.ValueNull:
                    return null;
                case DataBytesDefinition.Char:
                    return reader.ReadChar();
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}