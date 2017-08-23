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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.WSerializer.Deserializer
{
    internal class DeserializerTypeItem
    {
        public Type Type;
        public DeserializerTypeInfo TypeInfo;
        public byte Last;
        public string LastPropertyName;
        public object Value;
        public List<object> Items = new List<object>();
        public IList ValueIList;
        public readonly string[] Properties;
        public int PropertyIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DeserializerTypeItem(Type type)
        {
            Type = type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DeserializerTypeItem(DeserializerTypeDefinition typeDefinition)
        {
            Type = typeDefinition.Type;
            TypeInfo = typeDefinition.TypeInfo;
            if (!Type.IsArray && TypeInfo.ActivatorParametersTypes.Length == 0)
            {
                Value = TypeInfo.Activator();
                if (TypeInfo.IsIList)
                    ValueIList = (IList)Value;
            }
            Properties = typeDefinition.Properties;
            PropertyIndex = 0;
        }
    }
}
