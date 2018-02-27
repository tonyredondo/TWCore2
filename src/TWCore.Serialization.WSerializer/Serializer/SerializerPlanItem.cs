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
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TWCore.Reflection;

// ReSharper disable NotAccessedField.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable SuggestBaseTypeForParameter

namespace TWCore.Serialization.WSerializer.Serializer
{
    internal abstract class SerializerPlanItem
    {
        #region Static
        private static readonly NonBlocking.ConcurrentDictionary<Type, string> AllTypeNames = new NonBlocking.ConcurrentDictionary<Type, string>();
        private static readonly NonBlocking.ConcurrentDictionary<Type, Tuple<string, string, string>> AllTypeNamesTuples = new NonBlocking.ConcurrentDictionary<Type, Tuple<string, string, string>>();
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
                var typeInfo = gType.GetTypeInfo();
                var assembly = typeInfo.Assembly;
                var assemblyName = new AssemblyName(assembly.FullName);
                var mType = typeInfo.IsGenericType ? 
                    string.Format("{0}.{1}[{2}]", typeInfo.Namespace, typeInfo.Name, typeInfo.GenericTypeArguments.Select(a => "[" + GetTypeName(a) + "]").ToArray().Join(",")) : 
                    type.FullName;
                if (mType != null && assemblyName.Name != "mscorlib" && assemblyName.Name != "System.Private.CoreLib")
                    mType += "," + assemblyName.Name;
                return mType;
            });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tuple<string, string, string> GetTypeNameTuple(Type type)
        {
            return AllTypeNamesTuples.GetOrAdd(type, gType =>
            {
                var typeInfo = gType.GetTypeInfo();
                var assembly = typeInfo.Assembly;
                var assemblyName = new AssemblyName(assembly.FullName);
                string mNamespace;
                string mType;
                string asmName = null;

                if (typeInfo.IsGenericType)
                {
                    mNamespace = typeInfo.Namespace;
                    mType = typeInfo.GenericTypeArguments.Length > 0 ? 
                        string.Format("{0}[{1}]", typeInfo.Name, typeInfo.GenericTypeArguments.Select(a => "[" + GetTypeName(a) + "]").ToArray().Join(",")) : 
                        typeInfo.Name;
                }
                else
                {
                    mNamespace = type.Namespace;
                    mType = type.Name;
                }
                if (mType != null && assemblyName.Name != "mscorlib" && assemblyName.Name != "System.Private.CoreLib")
                    asmName = assemblyName.Name;

                if (type.DeclaringType != null)
                {
                    mType = type.DeclaringType.Name + "+" + mType;
                }
                return Tuple.Create(mNamespace, mType, asmName);
            });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetDataType(Type type)
        {
            if (type == typeof(string))
                return DataType.TypeNameString;
            if (type == typeof(int))
                return DataType.TypeNameInt;
            if (type == typeof(Guid))
                return DataType.TypeNameGuid;
            if (type == typeof(DateTime))
                return DataType.TypeNameDateTime;
            if (type == typeof(TimeSpan))
                return DataType.TypeNameTimeSpan;
            if (type == typeof(decimal))
                return DataType.TypeNameDecimal;
            if (type == typeof(bool))
                return DataType.TypeNameBool;
            if (type == typeof(double))
                return DataType.TypeNameDouble;
            if (type == typeof(float))
                return DataType.TypeNameFloat;
            if (type == typeof(ulong))
                return DataType.TypeNameULong;
            if (type == typeof(long))
                return DataType.TypeNameLong;
            if (type == typeof(uint))
                return DataType.TypeNameUInt;
            if (type == typeof(ushort))
                return DataType.TypeNameUShort;
            if (type == typeof(short))
                return DataType.TypeNameShort;
            if (type == typeof(sbyte))
                return DataType.TypeNameSByte;
            if (type == typeof(byte))
                return DataType.TypeNameByte;
            if (type == typeof(char))
                return DataType.TypeNameChar;
            return type == typeof(DateTimeOffset) ? DataType.TypeNameDateTimeOffset : DataType.Unknown;
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
            public string TypeNamespace;
            public string TypeName;
            public string TypeAssembly;
            public int Quantity;
            public Type[] Types;
            public byte[] DTypes;
            public string[] TypeNamespaces;
            public string[] TypeNames;
            public string[] TypeAssemblies;
            public bool IsArray;
            public string[] Properties;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TypeStart(Type type, TypeInfo typeInfo)
            {
                PlanType = SerializerPlanItemType.TypeStart;
                Type = type;
                IsArray = type.IsArray;
                if (typeInfo.IsGenericType)
                {
                    var gtype = typeInfo.GetGenericTypeDefinition();
                    var ta = GetTypeNameTuple(gtype);
                    TypeNamespace = ta.Item1;
                    TypeName = ta.Item2;
                    TypeAssembly = ta.Item3;

                    var types = typeInfo.GenericTypeArguments;
                    Quantity = types.Length;
                    Types = types;
                    DTypes = new byte[Quantity];
                    TypeNamespaces = new string[Quantity];
                    TypeNames = new string[Quantity];
                    TypeAssemblies = new string[Quantity];
                    for (var i = 0; i < Quantity; i++)
                    {
                        DTypes[i] = GetDataType(Types[i]);
                        var takey = GetTypeNameTuple(Types[i]);
                        TypeNamespaces[i] = takey.Item1;
                        TypeNames[i] = takey.Item2;
                        TypeAssemblies[i] = takey.Item3;
                    }
                }
                else
                {
                    var ta = GetTypeNameTuple(type);
                    TypeNamespace = ta.Item1;
                    TypeName = ta.Item2;
                    TypeAssembly = ta.Item3;
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

        #region KeyValueStart
        public class KeyValueStart : SerializerPlanItem
        {
            public Type KeyType;
            public FastPropertyInfo Key;
            public Type KeySerializerType;
            public bool KeyIsNullable;

            public Type ValueType;
            public FastPropertyInfo Value;
            public Type ValueSerializerType;
            public bool ValueIsNullable;

            public byte KeyDType;
            public string KeyTypeNamespace;
            public string KeyTypeName;
            public string KeyTypeAssembly;
            
            public byte ValueDType;
            public string ValueTypeNamespace;
            public string ValueTypeName;
            public string ValueTypeAssembly;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyValueStart(Type type, Type keyType, Type keySerializerType, bool keyIsNullable, Type valueType, Type valueSerializerType, bool valueIsNullable)
            {
                PlanType = SerializerPlanItemType.KeyValueStart;
                Type = type;
                var props = type.GetProperties();

                KeyType = keyType;
                KeySerializerType = keySerializerType;
                KeyIsNullable = keyIsNullable;
                Key = props.First(p => p.Name == "Key").GetFastPropertyInfo();
                KeyDType = GetDataType(KeyType);
                var takey = GetTypeNameTuple(KeyType);
                KeyTypeNamespace = takey.Item1;
                KeyTypeName = takey.Item2;
                KeyTypeAssembly = takey.Item3;

                ValueType = valueType;
                ValueSerializerType = valueSerializerType;
                ValueIsNullable = valueIsNullable;
                Value = props.First(p => p.Name == "Value").GetFastPropertyInfo();
                ValueDType = GetDataType(ValueType);
                var tavalue = GetTypeNameTuple(ValueType);
                ValueTypeNamespace = tavalue.Item1;
                ValueTypeName = tavalue.Item2;
                ValueTypeAssembly = tavalue.Item3;
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
                DefaultValue = isNullable ? null : Type.GetTypeInfo().IsValueType ? Activator.CreateInstance(Type) : null;
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
                DefaultValue = Type.GetTypeInfo().IsValueType ? Activator.CreateInstance(Type) : null;
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
            public RuntimeValue(Type type, Type serializer, object value)
            {
                PlanType = SerializerPlanItemType.RuntimeValue;
                Type = type;
                SerializerType = serializer;
                Value = value;
            }
        }
        #endregion

        #region TupleStart
        public class TupleStart : SerializerPlanItem
        {
            public int Quantity;
            public Type[] Types;
            public FastPropertyInfo[] Props;
            public Type[] SerializerTypes;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TupleStart(Type tupleType, Type[] types, Type[] serializerTypes)
            {
                PlanType = SerializerPlanItemType.TupleStart;
                Type = tupleType;
                Quantity = types.Length;
                Types = types;
                SerializerTypes = serializerTypes;
                Props = new FastPropertyInfo[Quantity];
                for (var i = 0; i < Quantity; i++)
                    Props[i] = tupleType.GetRuntimeProperty("Item" + (i + 1)).GetFastPropertyInfo();
            }
        }
        #endregion

        #endregion
    }
}
