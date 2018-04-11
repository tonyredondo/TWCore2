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
    public struct SerializedObjectSerializer : ITypeSerializer
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
        public void Write(BinaryWriter writer, SerializedObject value)
        {
            if (value == null)
                writer.Write(DataBytesDefinition.SerializedObjectNull);
            else
            {
                writer.Write(DataBytesDefinition.SerializedObject);
                value.WriteTo(writer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedObject Read(BinaryReader reader)
        {
            var type = reader.ReadByte();
            switch(type)
            {
                case DataBytesDefinition.SerializedObjectNull:
                    return null;
                case DataBytesDefinition.SerializedObject:
                    return SerializedObject.FromStream(reader);
            }
            throw new InvalidOperationException("Invalid type value.");
        }
    }
}