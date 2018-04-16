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
    public struct DeserializerTypeDescriptor
    {
        public Type Type;
        public ActivatorDelegate Activator;
        public Dictionary<string, FastPropertyInfo> Properties;
        public bool IsNSerializable;
        public DeserializerMetaDataOfType Metadata;
        public DeserializeDelegate DeserializeFunc;
        public Expression<DeserializeDelegate> Lambda;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DeserializerTypeDescriptor(Type type)
        {
            Type = type;
            var ifaces = type.GetInterfaces();
            var isIList = ifaces.Any(i => i == typeof(IList) || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)));
            var isIDictionary = ifaces.Any(i => i == typeof(IDictionary) || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)));
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

                var forIdx = Expression.Parameter(typeof(int), "i");
                varExpressions.Add(forIdx);
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));
                var loop = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(forIdx, capacity),
                        Expression.Assign(Expression.ArrayIndex(value, Expression.PostIncrementAssign(forIdx)),
                            Expression.Convert(Expression.Call(table, "InnerReadValue", Type.EmptyTypes, Expression.Call(table, "StreamReadByte", Type.EmptyTypes)), elementType)),
                        Expression.Break(breakLabel)), breakLabel);
                serExpressions.Add(loop);
            }
            else if (isIList)
            {
                serExpressions.Add(Expression.Assign(capacity, Expression.Call(table, DeserializersTable.StreamReadIntMethod)));
                serExpressions.Add(Expression.Assign(value, Expression.New(type)));
                serExpressions.Add(Expression.Call(objectCache, "Set", Type.EmptyTypes, value));
                //serExpressions.Add(Expression.Call(coreLogProp, infoBasicMethod, Expression.Call(capacity, "ToString", Type.EmptyTypes)));

                var addMethod = type.GetMethod("Add");
                var addParameters = addMethod.GetParameters();

                var forIdx = Expression.Parameter(typeof(int), "i");
                varExpressions.Add(forIdx);
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));
                var loop = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(forIdx, capacity),
                        Expression.Block(
                            Expression.Call(value, addMethod, Expression.Convert(Expression.Call(table, "InnerReadValue", Type.EmptyTypes, Expression.Call(table, "StreamReadByte", Type.EmptyTypes)), addParameters[0].ParameterType)),
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

                var addMethod = type.GetMethod("Add");
                var addParameters = addMethod.GetParameters();

                var forIdx = Expression.Parameter(typeof(int), "i");
                varExpressions.Add(forIdx);
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));
                var loop = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(forIdx, capacity),
                        Expression.Block(
                            Expression.Call(value, addMethod,
                                Expression.Convert(Expression.Call(table, "ReadValue", Type.EmptyTypes, Expression.Call(table, "StreamReadByte", Type.EmptyTypes)), addParameters[0].ParameterType),
                                Expression.Convert(Expression.Call(table, "ReadValue", Type.EmptyTypes, Expression.Call(table, "StreamReadByte", Type.EmptyTypes)), addParameters[1].ParameterType)),
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
                    serExpressions.Add(Expression.Call(value, setMethod, Expression.Convert(Expression.Call(table, "InnerReadValue", Type.EmptyTypes, Expression.Call(table, "StreamReadByte", Type.EmptyTypes)), prop.PropertyType)));
            }

            serExpressions.Add(Expression.Call(table, "StreamReadByte", Type.EmptyTypes));
            //serExpressions.Add(Expression.Call(coreLogProp, infoBasicMethod, Expression.Constant("END - " + type.FullName)));

            serExpressions.Add(Expression.Return(returnTarget, value, typeof(object)));
            serExpressions.Add(Expression.Label(returnTarget, value));

            var block = Expression.Block(varExpressions, serExpressions).Reduce();
            var lambda = Expression.Lambda<DeserializeDelegate>(block, type.Name + "_DeserializeFunc", new[] { table });
            Lambda = lambda;
            DeserializeFunc = lambda.Compile();
        }

        public delegate object DeserializeDelegate(DeserializersTable table);
    }
}