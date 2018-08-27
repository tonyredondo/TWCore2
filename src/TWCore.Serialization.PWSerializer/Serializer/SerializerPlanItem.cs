﻿/*
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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TWCore.Reflection;
// ReSharper disable NotAccessedField.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace TWCore.Serialization.PWSerializer.Serializer
{
    internal abstract class SerializerPlanItem
    {
        #region Static
        private static readonly string[] IgnoredAssemblies = { "System.Private.CoreLib", "mscorlib", "NETStandard" };
        private static readonly NonBlocking.ConcurrentDictionary<Type, string> AllTypeNames = new NonBlocking.ConcurrentDictionary<Type, string>();
        private static readonly NonBlocking.ConcurrentDictionary<Type, (string @assembly, string @namespace, string @type)> AllTypeNamesArray = new NonBlocking.ConcurrentDictionary<Type, (string @assembly, string @namespace, string @type)>();
        #endregion

        #region Fields and Properties
        public byte PlanType;
        public Type Type;
        public byte[] ValueBytes;
        #endregion

        #region Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTypeName(Type type)
        {
            return AllTypeNames.GetOrAdd(type, gType =>
            {
                var assembly = gType.Assembly;
                var assemblyName = new AssemblyName(assembly.FullName);
                var mType = gType.IsGenericType ?
                    string.Format("{0}.{1}[{2}]", gType.Namespace, gType.Name, gType.GenericTypeArguments.Select(a => "[" + GetTypeName(a) + "]").ToArray().Join(",")) :
                    type.FullName;
                if (mType != null && Array.IndexOf(IgnoredAssemblies, assemblyName.Name) < 0)
                    mType += "," + assemblyName.Name;
                return mType;
            });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (string @assembly, string @namespace, string @type) GetTypeNameTuple(Type type)
        {
            return AllTypeNamesArray.GetOrAdd(type, gType =>
            {
                var assembly = gType.Assembly;
                var assemblyName = new AssemblyName(assembly.FullName);
                string mNamespace;
                string mType;
                string asmName = null;

                if (gType.IsGenericType)
                {
                    mNamespace = gType.Namespace;
                    mType = gType.GenericTypeArguments.Length > 0 ?
                        string.Format("{0}[{1}]", gType.Name, gType.GenericTypeArguments.Select(a => "[" + GetTypeName(a) + "]").ToArray().Join(",")) :
                        gType.Name;
                }
                else
                {
                    mNamespace = type.Namespace;
                    mType = type.Name;
                }

                if (mType != null && Array.IndexOf(IgnoredAssemblies, assemblyName.Name) < 0)
                    asmName = assemblyName.Name;

                if (type.DeclaringType != null)
                    mType = type.DeclaringType.Name + "+" + mType;

                return (asmName, mNamespace, mType);
            });
        }
        #endregion


        #region Nested Classes

        #region WriteByte
        public class WriteBytes : SerializerPlanItem
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public WriteBytes(byte[] values)
            {
                PlanType = SerializerPlanItemType.WriteBytes;
                ValueBytes = values;
            }
        }
        #endregion

        #region TypeStart
        public class TypeStart : SerializerPlanItem
        {
            public bool IsArray;
            public bool IsIList;
            public bool IsIDictionary;
            public string[] Properties;

            public readonly string AssemblyName;
            public readonly string NamespaceName;
            public readonly string TypeName;
            public readonly int Quantity;
            public readonly Type[] Types;
            public readonly string[] AssembliesNames;
            public readonly string[] NamespacesNames;
            public readonly string[] TypesNames;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TypeStart(Type type)
            {
                PlanType = SerializerPlanItemType.TypeStart;
                Type = type;
                IsArray = type.IsArray;

                if (type.IsGenericType)
                {
                    var gtype = type.GetGenericTypeDefinition();
                    (AssemblyName, NamespaceName, TypeName) = GetTypeNameTuple(gtype);

                    var types = type.GenericTypeArguments;
                    Quantity = types.Length;
                    Types = types;
                    AssembliesNames = new string[Quantity];
                    NamespacesNames = new string[Quantity];
                    TypesNames = new string[Quantity];
                    for (var i = 0; i < Quantity; i++)
                        (AssembliesNames[i], NamespacesNames[i], TypesNames[i]) = GetTypeNameTuple(Types[i]);
                }
                else
                {
                    (AssemblyName, NamespaceName, TypeName) = GetTypeNameTuple(type);
                }
            }
        }
        #endregion

        #region ListStart
        public class ListStart : SerializerPlanItem
        {
            public Type InnerType;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ListStart(Type listType, Type innerType)
            {
                PlanType = SerializerPlanItemType.ListStart;
                Type = listType;
                InnerType = innerType;
            }
        }
        #endregion

        #region DictionaryStart
        public class DictionaryStart : SerializerPlanItem
        {
            public Type KeyType;
            public Type KeySerializerType;
            public bool KeyIsNullable;

            public Type ValueType;
            public Type ValueSerializerType;
            public bool ValueIsNullable;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DictionaryStart(Type type, Type keyType, Type keySerializerType, bool keyIsNullable, Type valueType, Type valueSerializerType, bool valueIsNullable)
            {
                PlanType = SerializerPlanItemType.DictionaryStart;
                Type = type;

                KeyType = keyType;
                KeySerializerType = keySerializerType;
                KeyIsNullable = keyIsNullable;

                ValueType = valueType;
                ValueSerializerType = valueSerializerType;
                ValueIsNullable = valueIsNullable;
            }
        }
        #endregion


        #region PropertyValue
        public class PropertyValue : SerializerPlanItem
        {
            public string Name;
            public FastPropertyInfo Property;
            public Type SerializerType;
            public object DefaultValue;
            public bool IsNullable;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public PropertyValue(PropertyInfo pInfo, Type serializer, bool isNullable)
            {
                PlanType = SerializerPlanItemType.PropertyValue;
                Type = pInfo.PropertyType;
                DefaultValue = isNullable ? null : Type.IsValueType ? Activator.CreateInstance(Type) : null;
                IsNullable = isNullable;
                Property = pInfo.GetFastPropertyInfo();
                Name = pInfo.Name;
                SerializerType = serializer;
            }
        }
        #endregion

        #region PropertyReference
        public class PropertyReference : SerializerPlanItem
        {
            public string Name;
            public FastPropertyInfo Property;
            public object DefaultValue;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public PropertyReference(PropertyInfo pInfo)
            {
                PlanType = SerializerPlanItemType.PropertyReference;
                Type = pInfo.PropertyType;
                DefaultValue = Type.IsValueType ? Activator.CreateInstance(Type) : null;
                Property = pInfo.GetFastPropertyInfo();
                Name = pInfo.Name;
            }
        }
        #endregion

        #region Value
        public class ValueItem : SerializerPlanItem
        {
            public Type SerializerType;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueItem(Type type, Type serializer)
            {
                PlanType = SerializerPlanItemType.Value;
                Type = type;
                SerializerType = serializer;
            }
        }
        #endregion

        #region RuntimeValue
        public class RuntimeValue : SerializerPlanItem
        {
            public Type SerializerType;
            public object Value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public RuntimeValue()
            {
                PlanType = SerializerPlanItemType.RuntimeValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Init(Type type, Type serializer, object value)
            {
                Type = type;
                SerializerType = serializer;
                Value = value;
            }
        }
        #endregion

        #endregion
    }
}
