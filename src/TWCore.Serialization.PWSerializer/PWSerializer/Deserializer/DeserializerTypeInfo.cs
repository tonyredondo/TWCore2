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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TWCore.Reflection;

namespace TWCore.Serialization.PWSerializer.Deserializer
{
    internal class DeserializerTypeInfo
    {
        public Type Type;
        public Type[] InnerTypes;
        public Type[] ActivatorParametersTypes;
        public Dictionary<string, FPropertyInfo> Properties;
        public ActivatorDelegate Activator;
        public bool IsArray;
        public bool IsIList;
        public bool IsIDictionary;
        public bool IsTypeList;
        public bool IsTypeDictionary;

        internal class FPropertyInfo
        {
            public FastPropertyInfo Property;
            public bool IsEnum;
            public Type UnderlyingType;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object CreateInstance(int length)
        {
            if (ActivatorParametersTypes.Length == 0)
                return Activator();
            else if (IsArray)
                return Activator(length);
            return null;
        }
    }
}
