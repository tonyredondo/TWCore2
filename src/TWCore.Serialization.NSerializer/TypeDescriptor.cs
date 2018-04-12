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
using System.Text;
using TWCore.Reflection;

namespace TWCore.Serialization.NSerializer
{
    public delegate void SerializeActionDelegate(object obj, SerializersTable table);

    public struct TypeDescriptor
    {
        public string TypeName;
        public ActivatorDelegate Activator;
        public string[] Properties;
        public List<FastPropertyInfo> FastProperties;
        public bool IsArray;
        public bool IsList;
        public bool IsDictionary;
        public bool IsNSerializable;
        public byte[] Definition;
        public SerializeActionDelegate SerializeAction;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeDescriptor(Type type)
        {
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
            var typeName = type.GetTypeName();

            var serExpressions = new List<Expression>();
            var varExpressions = new List<ParameterExpression>();

            //
            var obj = Expression.Parameter(typeof(object), "obj");
            var serTable = Expression.Parameter(typeof(SerializersTable), "table");

            var instance = Expression.Parameter(type, "instance");
            varExpressions.Add(instance);
            serExpressions.Add(Expression.Assign(instance, Expression.Convert(obj, type)));

            //
            TypeName = typeName;
            Activator = Factory.Accessors.CreateActivator(type);
            Properties = runtimeProperties.Select(p => p.Name).ToArray();
            FastProperties = new List<FastPropertyInfo>();
            IsNSerializable = ifaces.Any(i => i == typeof(INSerializable));

            //
            if (runtimeProperties.Length > 0)
            {
                var propByte = Expression.Constant(DataBytesDefinition.PropertiesStart, typeof(byte));
                var propLength = Expression.Constant(runtimeProperties.Length, typeof(int));
                serExpressions.Add(Expression.Call(serTable, SerializersTable.WriteDefIntMInfo, propByte, propLength));

                foreach (var prop in runtimeProperties)
                {
                    FastProperties.Add(prop.GetFastPropertyInfo());

                    var getMethod = prop.GetMethod;
                    var getExpression = Expression.Call(instance, getMethod);
                    if (SerializersTable.WriteValues.TryGetValue(prop.PropertyType, out var wMethodTuple))
                        serExpressions.Add(Expression.Call(serTable, wMethodTuple.Method, getExpression));
                    else
                    {
                        serExpressions.Add(Expression.Call(serTable, SerializersTable.InternalWriteObjectValueMInfo, getExpression));
                    }
                }
            }
            //
            IsArray = type.IsArray;
            if (!IsArray)
            {
                IsDictionary = isIDictionary;
                IsList = !IsDictionary && isIList;
            }
            else
            {
                IsList = false;
                IsDictionary = false;
            }

            var defText = typeName + ";" + Properties.Join(";");
            var defBytesLength = Encoding.UTF8.GetByteCount(defText);
            var defBytes = new byte[defBytesLength + 5];
            defBytes[0] = DataBytesDefinition.TypeStart;
            defBytes[1] = (byte)defBytesLength;
            defBytes[2] = (byte)(defBytesLength >> 8);
            defBytes[3] = (byte)(defBytesLength >> 16);
            defBytes[4] = (byte)(defBytesLength >> 24);
            Encoding.UTF8.GetBytes(defText, 0, defText.Length, defBytes, 5);
            Definition = defBytes;

            if (IsArray)
            {
                var arrLength = Expression.Parameter(typeof(int), "length");
                varExpressions.Add(arrLength);
                serExpressions.Add(Expression.Assign(arrLength, Expression.Call(instance, SerializersTable.ArrayLengthGetMethod)));

                var arrByte = Expression.Constant(DataBytesDefinition.ArrayStart, typeof(byte));
                serExpressions.Add(Expression.Call(serTable, SerializersTable.WriteDefIntMInfo, arrByte, arrLength));

                var forIdx = Expression.Parameter(typeof(int), "i");
                varExpressions.Add(forIdx);
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));
                var loop = Expression.Loop(
                            Expression.IfThenElse(
                                Expression.LessThan(forIdx, arrLength),
                                Expression.Call(serTable, SerializersTable.InternalWriteObjectValueMInfo, Expression.ArrayIndex(instance, Expression.PostIncrementAssign(forIdx))),
                                Expression.Break(breakLabel)), breakLabel);
                serExpressions.Add(loop);
            }
            else if (IsList)
            {
                var iLength = Expression.Parameter(typeof(int), "length");
                varExpressions.Add(iLength);
                serExpressions.Add(Expression.Assign(iLength, Expression.Call(instance, SerializersTable.IListLengthGetMethod)));

                var iByte = Expression.Constant(DataBytesDefinition.ListStart, typeof(byte));
                serExpressions.Add(Expression.Call(serTable, SerializersTable.WriteDefIntMInfo, iByte, iLength));

                var forIdx = Expression.Parameter(typeof(int), "i");
                varExpressions.Add(forIdx);
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));
                var loop = Expression.Loop(
                            Expression.IfThenElse(
                                Expression.LessThan(forIdx, iLength),
                                Expression.Call(serTable, SerializersTable.InternalWriteObjectValueMInfo, Expression.MakeIndex(instance, SerializersTable.IListIndexProperty, new[] { Expression.PostIncrementAssign(forIdx) })),
                                Expression.Break(breakLabel)), breakLabel);
                serExpressions.Add(loop);
            }
            else if (IsDictionary)
            {

            }

            var expressionBlock = Expression.Block(varExpressions, serExpressions);
            var lambda = Expression.Lambda<SerializeActionDelegate>(expressionBlock, type.Name + "SerializeAction", new[] { obj, serTable });
            SerializeAction = lambda.Compile();
        }
    }
}
