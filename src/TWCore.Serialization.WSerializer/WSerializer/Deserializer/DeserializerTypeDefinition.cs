/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.WSerializer.Deserializer
{
    internal class DeserializerTypeDefinition
    {
        public readonly Type Type;
        public readonly DeserializerTypeInfo TypeInfo;
        public readonly string[] Properties;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DeserializerTypeDefinition(Type type, DeserializerTypeInfo typeInfo, string[] properties)
        {
            Type = type;
            TypeInfo = typeInfo;
            Properties = properties;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => Type.Equals(obj);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Type.GetHashCode();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => Type.ToString();
    }
}
