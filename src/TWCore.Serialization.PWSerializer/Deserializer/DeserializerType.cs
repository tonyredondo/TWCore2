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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TWCore.Serialization.PWSerializer.Deserializer
{
    internal class DeserializerType
    {
        private static readonly ConcurrentDictionary<Type, DeserializerTypeInfo> DeserializationTypeInfo = new ConcurrentDictionary<Type, DeserializerTypeInfo>();
        private object _tmpDictionary;

        public Type Type;
        public DeserializerTypeInfo TypeInfo;
        public object Value;
        public IList ListValue;
        public IDictionary DictionaryValue;
        public int ListIndex;
        public int ItemDictionaryIdx;
        public string[] Properties;
        public Type[] PropertiesType;
        public int PropertiesLength;
        public int CurrentPropertyIndex;
        public byte Operation;
        public Type CurrentType;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Properties = null;
            PropertiesLength = 0;
            CurrentPropertyIndex = 0;
            Operation = 0;
            Value = null;
            ListValue = null;
            DictionaryValue = null;
            Type = null;
            TypeInfo = null;
            ListIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetProperties(Type type, string[] properties, Dictionary<Type, Type[]> propertiesTypes, int length)
        {
            Type = type;
            TypeInfo = GetDeserializationTypeInfo(type);
            CurrentType = type;
            if (properties.Length == 0 && length == -1)
                Value = null;
            else
                Value = TypeInfo.CreateInstance(length);

            if (TypeInfo.IsIList)
                ListValue = (IList)Value;
            else if (TypeInfo.IsIDictionary)
                DictionaryValue = (IDictionary)Value;

            if (properties.Length <= 0) return;

            Properties = properties;
            PropertiesLength = properties.Length;
            if (!propertiesTypes.TryGetValue(type, out PropertiesType))
            {
                PropertiesType = properties.Select(p => TypeInfo.Properties[p].Property.PropertyType).ToArray();
                propertiesTypes[type] = PropertiesType;
            }
            CurrentType = PropertiesType[0];
            CurrentPropertyIndex = 0;
            Operation = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ListStart()
        {
            Operation = 1;
            CurrentType = TypeInfo.InnerTypes[0];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ListEnd()
        {
            Operation = 0;
            CurrentType = Type;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DictionaryStart()
        {
            ItemDictionaryIdx = 0;
            Operation = 2;
            CurrentType = TypeInfo.InnerTypes[0];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DictionaryEnd()
        {
            Operation = 0;
            CurrentType = Type;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddValue(object lastObject)
        {
            switch (Operation)
            {
                case 0:
                    var propName = Properties[CurrentPropertyIndex++];
                    if (TypeInfo.Properties.TryGetValue(propName, out var fPropInfo))
                    {
                        var fastProp = fPropInfo.Property;
                        var propType = fastProp.PropertyType;
                        if (lastObject == null)
                            fastProp.SetValue(Value, null);
                        else if (lastObject.GetType() == propType)
                            fastProp.SetValue(Value, lastObject);
                        else if (fPropInfo.IsEnum)
                            fastProp.SetValue(Value, Enum.ToObject(propType, lastObject));
                        else
                            fastProp.SetValue(Value, DataTypeHelper.Change(lastObject, fPropInfo.UnderlyingType));
                    }
                    CurrentType = CurrentPropertyIndex < PropertiesLength ? PropertiesType[CurrentPropertyIndex] : Type;
                    break;
                case 1:
                    if (TypeInfo.IsArray)
                        ListValue[ListIndex++] = DataTypeHelper.Change(lastObject, TypeInfo.InnerTypes[0]);
                    else
                        ListValue.Add(DataTypeHelper.Change(lastObject, TypeInfo.InnerTypes[0]));
                    break;
                case 2:
                    if (ItemDictionaryIdx == 0)
                    {
                        _tmpDictionary = lastObject;
                        ItemDictionaryIdx++;
                        CurrentType = TypeInfo.InnerTypes[1];
                    }
                    else
                    {
                        var dKey = DataTypeHelper.Change(_tmpDictionary, TypeInfo.InnerTypes[0]);
                        var dValue = DataTypeHelper.Change(lastObject, TypeInfo.InnerTypes[1]);
                        DictionaryValue.Add(dKey, dValue);
                        _tmpDictionary = null;
                        ItemDictionaryIdx = 0;
                        CurrentType = TypeInfo.InnerTypes[0];
                    }
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DeserializerTypeInfo GetDeserializationTypeInfo(Type type)
        {
            if (type == null) return null;
            return DeserializationTypeInfo.GetOrAdd(type, valueType =>
            {
                if (valueType == null) return null;
                var typeInfo = valueType.GetTypeInfo();
                var isGenericType = typeInfo.IsGenericType;

                var tinfo = new DeserializerTypeInfo
                {
                    Type = valueType,
                    Properties = valueType.GetRuntimeProperties().Where(p =>
                    {
                        var ok = !p.IsSpecialName && p.CanRead && p.CanWrite;
                        if (!ok) return false;
                        return p.GetIndexParameters().Length <= 0;
                    }).Select(p => new DeserializerTypeInfo.FPropertyInfo
                    {
                        Property = p.GetFastPropertyInfo(),
                        IsEnum = p.PropertyType.GetTypeInfo().IsEnum,
                        UnderlyingType = p.PropertyType.GetUnderlyingType()
                    }).ToDictionary(k => k.Property.Name, v => v)
                };
                var constructor = typeInfo.DeclaredConstructors.First();
                tinfo.Activator = Factory.Accessors.CreateActivator(constructor);
                tinfo.ActivatorParametersTypes = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
                tinfo.IsArray = valueType.IsArray;

                //
                var ifaces = typeInfo.ImplementedInterfaces.ToArray();


                var ilist = ifaces.FirstOrDefault(i => i == typeof(IList) || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)));
                if (ilist != null)
                {
                    Type innerType = null;
                    if (type.IsArray)
                        innerType = type.GetElementType();
                    else
                    {
                        var gargs = ilist.GenericTypeArguments;
                        if (gargs.Length == 0)
                            gargs = type.GenericTypeArguments;
                        if (gargs.Length > 0)
                            innerType = gargs[0];
                        else
                        {
                            var iListType = typeInfo.ImplementedInterfaces.FirstOrDefault(m => (m.GetTypeInfo().IsGenericType && m.GetGenericTypeDefinition() == typeof(IList<>)));
                            if (iListType != null && iListType.GenericTypeArguments.Length > 0)
                                innerType = iListType.GenericTypeArguments[0];
                        }
                    }
                    tinfo.IsIList = true;
                    tinfo.InnerTypes = new[] { innerType };
                }

                var idictio = ifaces.FirstOrDefault(i => i == typeof(IDictionary) || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)));
                if (idictio != null)
                {
                    tinfo.IsIDictionary = true;
                    tinfo.InnerTypes = idictio.GenericTypeArguments;
                }

                if (!tinfo.IsIList && !tinfo.IsIDictionary && isGenericType)
                    tinfo.InnerTypes = typeInfo.GenericTypeArguments;

                if (tinfo.IsIList && type.GenericTypeArguments.Length == 1 && type.GetGenericTypeDefinition() == typeof(List<>))
                    tinfo.IsTypeList = true;
                else if (tinfo.IsIDictionary && type.GenericTypeArguments.Length == 2 && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    tinfo.IsTypeDictionary = true;

                return tinfo;
            });
        }
    }
}
