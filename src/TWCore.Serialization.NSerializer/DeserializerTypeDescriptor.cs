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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using TWCore.Reflection;

namespace TWCore.Serialization.NSerializer
{
    public delegate object DeserializeDelegate(DeserializersTable table);

    public struct DeserializerTypeDescriptor
    {
        public Type Type;
        public ActivatorDelegate Activator;
        public Dictionary<string, FastPropertyInfo> Properties;
        public bool IsNSerializable;
        public DeserializerMetaDataOfType Metadata;
        public DeserializeDelegate DeserializeFunc;
        //public Expression<DeserializeDelegate> Lambda;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DeserializerTypeDescriptor(Type type)
        {
            Type = type;
            var ifaces = type.GetInterfaces();
            var iListType = ifaces.FirstOrDefault(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
            var isIList = iListType != null;
            var iDictionaryType = ifaces.FirstOrDefault(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            var isIDictionary = iDictionaryType != null;
            var runtimeProperties = type.GetRuntimeProperties().OrderBy(p => p.Name).Where(prop =>
            {
                if (prop.IsSpecialName || !prop.CanRead || !prop.CanWrite) return false;
                if (prop.GetAttribute<NonSerializeAttribute>() != null) return false;
                if (prop.GetIndexParameters().Length > 0) return false;
                if (isIList && prop.Name == "Capacity") return false;
                return true;
            }).ToArray();
            Activator = Factory.Accessors.CreateActivator(type);
            IsNSerializable = ifaces.Any(i => i == typeof(INSerializable));
            Properties = new Dictionary<string, FastPropertyInfo>();
            var propNames = new string[runtimeProperties.Length];
            var idx = 0;
            foreach (var prop in runtimeProperties)
            {
                Properties[prop.Name] = prop.GetFastPropertyInfo();
                propNames[idx++] = prop.Name;
            }
            var isArray = type.IsArray;
            if (isArray)
            {
                isIList = false;
                isIDictionary = false;
            }
            Metadata = new DeserializerMetaDataOfType(type, isArray, isIList, isIDictionary, propNames);

            //*** Expressions
            var ctors = type.GetConstructors();
            var ctor = ctors.FirstOrDefault(c => c.GetParameters().Length == 0) ?? ctors[0];

            var serExpressions = new List<Expression>();

            //var infoBasicMethod = typeof(Diagnostics.Log.ILogEngine).GetMethod("InfoBasic", new[] {typeof(string)});
            //var coreLogProp = Expression.Property(null, typeof(Core), "Log");
            //serExpressions.Add(Expression.Call(coreLogProp, infoBasicMethod, Expression.Constant("START - " + type.FullName)));

            var varExpressions = new List<ParameterExpression>();
            //
            var table = Expression.Parameter(typeof(DeserializersTable), "table");
            var returnTarget = Expression.Label(typeof(object), "ReturnTarget");

            var value = Expression.Parameter(type, "value");
            var capacity = Expression.Parameter(typeof(int), "capacity");
            varExpressions.Add(value);
            varExpressions.Add(capacity);

            var objectCache = Expression.Field(table, "ObjectCache");

            if (isArray)
            {
                serExpressions.Add(Expression.Assign(capacity, Expression.Call(table, DeserializersTable.StreamReadIntMethod)));
                serExpressions.Add(Expression.Assign(value, Expression.New(ctor, capacity)));
                serExpressions.Add(Expression.Call(objectCache, "Set", Type.EmptyTypes, value));

                var elementType = type.GetElementType();
                //var methodName = "ReadValue";
                var methodName = "InnerReadValue";
                if (DeserializersTable.ReadValuesFromType.TryGetValue(elementType, out var propMethod))
                    methodName = propMethod.Name;
                else if (elementType.IsEnum)
                    methodName = DeserializersTable.ReadValuesFromType[typeof(Enum)].Name;
                else if (elementType == typeof(object))
                    methodName = "ReadValue";

                var forIdx = Expression.Parameter(typeof(int), "i");
                varExpressions.Add(forIdx);
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));
                var loop = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(forIdx, capacity),
                        Expression.Assign(Expression.ArrayAccess(value, Expression.PostIncrementAssign(forIdx)),
                            Expression.Convert(Expression.Call(table, methodName, Type.EmptyTypes, Expression.Call(table, "StreamReadByte", Type.EmptyTypes)), elementType)),
                        Expression.Break(breakLabel)), breakLabel);
                serExpressions.Add(loop);
            }
            else if (isIList)
            {
                serExpressions.Add(Expression.Assign(capacity, Expression.Call(table, DeserializersTable.StreamReadIntMethod)));
                serExpressions.Add(Expression.Assign(value, Expression.New(type)));
                serExpressions.Add(Expression.Call(objectCache, "Set", Type.EmptyTypes, value));
                //serExpressions.Add(Expression.Call(coreLogProp, infoBasicMethod, Expression.Call(capacity, "ToString", Type.EmptyTypes)));

                var addMethod = type.GetMethod("Add", iListType.GenericTypeArguments);
                var elementType = iListType.GenericTypeArguments[0];
                //var methodName = "ReadValue";
                var methodName = "InnerReadValue";
                if (DeserializersTable.ReadValuesFromType.TryGetValue(elementType, out var propMethod))
                    methodName = propMethod.Name;
                else if (elementType.IsEnum)
                    methodName = DeserializersTable.ReadValuesFromType[typeof(Enum)].Name;
                else if (elementType == typeof(object))
                    methodName = "ReadValue";

                var forIdx = Expression.Parameter(typeof(int), "i");
                varExpressions.Add(forIdx);
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));
                var loop = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(forIdx, capacity),
                        Expression.Block(
                            Expression.Call(value, addMethod, Expression.Convert(Expression.Call(table, methodName, Type.EmptyTypes, Expression.Call(table, "StreamReadByte", Type.EmptyTypes)), elementType)),
                            Expression.PostIncrementAssign(forIdx)
                        ),
                        Expression.Break(breakLabel)), breakLabel);
                serExpressions.Add(loop);
            }
            else if (isIDictionary)
            {
                serExpressions.Add(Expression.Assign(capacity, Expression.Call(table, DeserializersTable.StreamReadIntMethod)));
                serExpressions.Add(Expression.Assign(value, Expression.New(type)));
                serExpressions.Add(Expression.Call(objectCache, "Set", Type.EmptyTypes, value));

                var addMethod = type.GetMethod("Add", iDictionaryType.GenericTypeArguments);
                var keyElementType = iDictionaryType.GenericTypeArguments[0];
                var valueElementType = iDictionaryType.GenericTypeArguments[1];

                var keyMethodName = "InnerReadValue";
                if (DeserializersTable.ReadValuesFromType.TryGetValue(keyElementType, out var keyPropMethod))
                    keyMethodName = keyPropMethod.Name;
                else if (keyElementType.IsEnum)
                    keyMethodName = DeserializersTable.ReadValuesFromType[typeof(Enum)].Name;
                else if (keyElementType == typeof(object))
                    keyMethodName = "ReadValue";

                var valueMethodName = "InnerReadValue";
                if (DeserializersTable.ReadValuesFromType.TryGetValue(valueElementType, out var valuePropMethod))
                    valueMethodName = valuePropMethod.Name;
                else if (keyElementType.IsEnum)
                    valueMethodName = DeserializersTable.ReadValuesFromType[typeof(Enum)].Name;
                else if (keyElementType == typeof(object))
                    valueMethodName = "ReadValue";

                var forIdx = Expression.Parameter(typeof(int), "i");
                varExpressions.Add(forIdx);
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));
                var loop = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(forIdx, capacity),
                        Expression.Block(
                            Expression.Call(value, addMethod,
                                Expression.Convert(Expression.Call(table, keyMethodName, Type.EmptyTypes, Expression.Call(table, "StreamReadByte", Type.EmptyTypes)), keyElementType),
                                Expression.Convert(Expression.Call(table, valueMethodName, Type.EmptyTypes, Expression.Call(table, "StreamReadByte", Type.EmptyTypes)), valueElementType)),
                            Expression.PostIncrementAssign(forIdx)
                        ),
                        Expression.Break(breakLabel)), breakLabel);
                serExpressions.Add(loop);
            }
            else
            {
                serExpressions.Add(Expression.Assign(value, Expression.New(type)));
                serExpressions.Add(Expression.Call(objectCache, "Set", Type.EmptyTypes, value));
            }
            foreach (var prop in runtimeProperties)
            {
                var setMethod = prop.SetMethod;
                if (DeserializersTable.ReadValuesFromType.TryGetValue(prop.PropertyType, out var propMethod))
                    serExpressions.Add(Expression.Call(value, setMethod, Expression.Call(table, propMethod, Expression.Call(table, "StreamReadByte", Type.EmptyTypes))));
                else if (prop.PropertyType.IsEnum)
                    serExpressions.Add(Expression.Call(value, setMethod, Expression.Convert(Expression.Call(table, DeserializersTable.ReadValuesFromType[typeof(Enum)], Expression.Call(table, "StreamReadByte", Type.EmptyTypes)), prop.PropertyType)));
                else
                    serExpressions.Add(Expression.Call(value, setMethod, Expression.Convert(Expression.Call(table, "ReadValue", Type.EmptyTypes, Expression.Call(table, "StreamReadByte", Type.EmptyTypes)), prop.PropertyType)));
            }

            serExpressions.Add(Expression.Call(table, "StreamReadByte", Type.EmptyTypes));
            //serExpressions.Add(Expression.Call(coreLogProp, infoBasicMethod, Expression.Constant("END - " + type.FullName)));

            serExpressions.Add(Expression.Return(returnTarget, value, typeof(object)));
            serExpressions.Add(Expression.Label(returnTarget, value));

            var block = Expression.Block(varExpressions, serExpressions).Reduce();
            var lambda = Expression.Lambda<DeserializeDelegate>(block, type.Name + "_Deserializer", new[] { table });
            //Lambda = lambda;
            DeserializeFunc = lambda.Compile();
        }
    }
}