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
using System.Linq;
using System.Reflection;
// ReSharper disable UnusedMember.Global
// ReSharper disable SwitchStatementMissingSomeCases
// ReSharper disable LoopCanBeConvertedToQuery

namespace TWCore.Object.Descriptor
{
    /// <summary>
    /// Object descriptor
    /// </summary>
    public class Descriptor
    {
        private static readonly string[] IgnoredMethods = { "ToString", "Equals", "GetHashCode", "GetType" };
        private int _depth;

        #region Public Methods
        /// <summary>
        /// Get object description
        /// </summary>
        /// <param name="value">Object to describe</param>
        /// <returns>Object description</returns>
        public ObjectDescription GetDescription(object value)
            => GetDescription(value, null);

        /// <summary>
        /// Get object description
        /// </summary>
        /// <param name="value">Object to describe</param>
        /// <param name="valueType">Value type</param>
        /// <returns>Object description</returns>
        public ObjectDescription GetDescription(object value, Type valueType)
        {
            _depth = 0;
            var oDesc = new ObjectDescription();
            valueType = valueType ?? value?.GetType();
            oDesc.Type = valueType?.AssemblyQualifiedName;
            oDesc.Value = value != null ? ExtractValue(value, valueType) : null;
            return oDesc;
        }
        #endregion

        #region Private Methods

        private ValueType GetValueType(object value, Type valueType)
        {
            if (valueType.IsEnum)
                return ValueType.Enum;
            if (valueType == typeof(string))
                return ValueType.String;
            if (valueType == typeof(bool))
                return ValueType.Bool;
            if (valueType == typeof(DateTime))
                return ValueType.Date;
            if (valueType == typeof(int))
                return ValueType.Number;
            if (valueType == typeof(long))
                return ValueType.Number;
            if (valueType == typeof(float))
                return ValueType.Number;
            if (valueType == typeof(decimal))
                return ValueType.Number;
            if (valueType == typeof(short))
                return ValueType.Number;
            if (valueType == typeof(ushort))
                return ValueType.Number;
            if (valueType == typeof(byte))
                return ValueType.Number;
            if (valueType == typeof(double))
                return ValueType.Number;
            if (valueType == typeof(TimeSpan))
                return ValueType.Time;
            if (valueType == typeof(Guid))
                return ValueType.Guid;
            if (valueType == typeof(MethodInfo))
                return ValueType.Method;
            if (valueType == typeof(RuntimeMethodHandle))
                return ValueType.Method;
            if (valueType == typeof(RuntimeTypeHandle))
                return ValueType.Type;
            if (value is MethodInfo)
                return ValueType.Method;
            if (value is Type)
                return ValueType.Type;
            if (valueType == typeof(Type))
                return ValueType.Type;
            if (valueType.GetTypeInfo().IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                return GetValueType(value, Nullable.GetUnderlyingType(valueType));
            if (valueType.GetInterfaces().Any(i =>
                (i == typeof(IEnumerable) || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))))
                return ValueType.Enumerable;
            return ValueType.Complex;
        } 
        
        private Value ExtractValue(object value, Type valueType)
        {
            if (_depth > 15)
                return null;
            _depth++;
            var oValue = new Value
            {
                ValueType = valueType.FullName,
                ValueString = value?.ToString() ?? "(null)",
                Type = GetValueType(value, valueType)
            };

            switch (oValue.Type)
            {
                case ValueType.Enumerable:
                {
                    var members = new List<Member>();
                    var idx = 0;
                    if (value != null)
                    {
                        foreach (var item in (IEnumerable)value)
                        {
                            var oMember = new Member
                            {
                                Name = "[{0}]".ApplyFormat(idx),
                                Type = MemberType.EnumerableItem,
                                Object = item,
                                Value = ExtractValue(item, item?.GetType() ?? valueType.GetGenericArguments()[0])
                            };
                            members.Add(oMember);
                            idx++;
                        }
                    }
                    oValue.Members = members.ToArray();
                    break;
                }
                case ValueType.Type:
                {
                    var members = new List<Member>();
                    var vType = (Type)value;
                    if (vType == null) break;
                    var props = vType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => !p.IsSpecialName && p.GetIndexParameters().Length == 0);
                    foreach (var prop in props)
                    {
                        var oMember = new Member
                        {
                            Name = prop.Name,
                            Type = MemberType.Property,
                            Object = prop,
                            Value = new Value
                            {
                                Type = ValueType.String,
                                ValueString = prop.PropertyType.Name,
                                ValueType = prop.PropertyType.FullName
                            }
                        };
                        members.Add(oMember);
                    }

                    var fields = vType.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => !f.IsStatic);
                    foreach (var field in fields)
                    {
                        var oMember = new Member
                        {
                            Name = field.Name,
                            Type = MemberType.Field,
                            Object = field,
                            Value = new Value
                            {
                                Type = ValueType.String,
                                ValueString = field.FieldType.Name,
                                ValueType = field.FieldType.FullName
                            }
                        };
                        members.Add(oMember);
                    }

                    var methods = vType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => !m.IsSpecialName);
                    foreach (var method in methods)
                    {
                        if (IgnoredMethods.Contains(method.Name))
                            continue;
                        var oMember = new Member
                        {
                            Name = method.Name,
                            Type = MemberType.Method,
                            Object = method,
                            Value = new Value
                            {
                                Type = ValueType.Method,
                                ValueString = method.ToString(),
                                ValueType = method.ReturnType?.FullName
                            }
                        };
                        members.Add(oMember);
                    }

                    oValue.Members = members.ToArray();
                    break;
                }
                case ValueType.Complex:
                {
                    var members = new List<Member>();
                    foreach (var prop in valueType.GetProperties().Where(p => p.CanRead && !p.IsSpecialName && p.GetIndexParameters().Length == 0))
                    {
                        var oMember = new Member
                        {
                            Name = prop.Name,
                            Type = MemberType.Property
                        };
                        object innerValue;
                        try
                        {
                            if (prop.GetGetMethod().IsStatic)
                                //innerValue = prop.GetValue(null);
                                continue;
                            else
                                innerValue = value != null ? prop.GetFastPropertyInfo().GetValue(value) : null;
                        }
                        catch (Exception e)
                        {
                            innerValue = e.Message;
                            Core.Log.Write(e);
                        }
                        oMember.Object = innerValue;
                        oMember.Value = ExtractValue(innerValue, innerValue?.GetType() ?? prop.PropertyType);
                        members.Add(oMember);
                    }
                    foreach (var field in valueType.GetFields().Where(f => !f.IsStatic))
                    {
                        var oMember = new Member
                        {
                            Name = field.Name,
                            Type = MemberType.Field
                        };
                        object innerValue;
                        try
                        {
                            if (field.IsStatic)
                                //innerValue = field.GetValue(null);
                                continue;
                            else
                                innerValue = value != null ? field.GetValue(value) : null;
                        }
                        catch (Exception e)
                        {
                            innerValue = e.Message;
                            Core.Log.Write(e);
                        }
                        oMember.Object = innerValue;
                        oMember.Value = ExtractValue(innerValue, innerValue?.GetType() ?? field.FieldType);
                        members.Add(oMember);
                    }
                    foreach (var method in valueType.GetMethods().Where(m => !m.IsSpecialName))
                    {
                        if (IgnoredMethods.Contains(method.Name))
                            continue;
                        var oMember = new Member
                        {
                            Name = method.Name,
                            Type = MemberType.Method,
                            Object = method,
                            Value = ExtractValue(method, method.GetType())
                        };
                        members.Add(oMember);
                    }
                    oValue.Members = members.ToArray();
                    break;
                }
            }

            _depth--;
            return oValue;
        }
        #endregion
    }
}
