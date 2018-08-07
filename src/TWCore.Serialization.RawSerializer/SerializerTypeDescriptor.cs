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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using TWCore.Reflection;

namespace TWCore.Serialization.RawSerializer
{
    public delegate void SerializeActionDelegate(object obj, SerializersTable table);

    public struct SerializerTypeDescriptor
    {
        public Type Type;
        public PropertyInfo[] RuntimeProperties;
        public Type IListType;
        public Type IDictionaryType;
        //
        public string[] Properties;
        public bool IsArray;
        public bool IsList;
        public bool IsDictionary;
        public bool IsNSerializable;
        public byte[] Definition;
        public SerializeActionDelegate SerializeAction;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializerTypeDescriptor(Type type)
        {
            Type = type;
            var ifaces = type.GetInterfaces();
            IListType = ifaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
            IDictionaryType = ifaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            var isIList = IListType != null;
            var isIDictionary = IDictionaryType != null;
            RuntimeProperties = type.GetRuntimeProperties().OrderBy(p => p.Name).Where(prop =>
            {
                if (prop.IsSpecialName || !prop.CanRead || !prop.CanWrite) return false;
                if (prop.GetAttribute<NonSerializeAttribute>() != null) return false;
                if (prop.GetIndexParameters().Length > 0) return false;
                if (isIList && prop.Name == "Capacity") return false;
                return true;
            }).ToArray();

            //
            Properties = RuntimeProperties.Select(p => p.Name).ToArray();
            IsNSerializable = ifaces.Any(i => i == typeof(IRawSerializable));
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
            //
            var typeName = type.GetTypeName();
            var defText = typeName + ";" + (IsArray ? "1" : "0") + ";" + (IsList ? "1" : "0") + ";" + (IsDictionary ? "1" : "0") + ";" + Properties.Join(";");
            var defBytesLength = Encoding.UTF8.GetByteCount(defText);
            var defBytes = new byte[defBytesLength + 5];
            defBytes[0] = DataBytesDefinition.TypeStart;
            defBytes[1] = (byte)defBytesLength;
            defBytes[2] = (byte)(defBytesLength >> 8);
            defBytes[3] = (byte)(defBytesLength >> 16);
            defBytes[4] = (byte)(defBytesLength >> 24);
            Encoding.UTF8.GetBytes(defText, 0, defText.Length, defBytes, 5);
            Definition = defBytes;
            SerializeAction = null;

            //
            var obj = Expression.Parameter(typeof(object), "obj");
            var serTable = Expression.Parameter(typeof(SerializersTable), "table");

            var instance = Expression.Parameter(type, "instance");
            var serExpression = Expression.Block(new[] { instance },
                Expression.Assign(instance, Expression.Convert(obj, type)),
                GetSerializerExpression(instance, serTable));

            var lambda = Expression.Lambda<SerializeActionDelegate>(serExpression, type.Name + "_Serializer", new[] { obj, serTable });
            SerializeAction = lambda.Compile();
        }

        public Expression GetSerializerExpression(Expression instance, ParameterExpression serTable)
        {
            //
            var serExpressions = new List<Expression>();
            var varExpressions = new List<ParameterExpression>();
            //

            if (IsArray)
            {
                var elementType = Type.GetElementType();

                var arrLength = Expression.Parameter(typeof(int), "length");
                varExpressions.Add(arrLength);
                serExpressions.Add(Expression.Assign(arrLength, Expression.Call(instance, SerializersTable.ArrayLengthGetMethod)));
                serExpressions.Add(Expression.Call(serTable, SerializersTable.WriteIntMethodInfo, arrLength));

                var forIdx = Expression.Parameter(typeof(int), "i");
                varExpressions.Add(forIdx);
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));

                var getValueExpression = Expression.ArrayIndex(instance, Expression.PostIncrementAssign(forIdx));

                var loop = Expression.Loop(
                            Expression.IfThenElse(
                                Expression.LessThan(forIdx, arrLength),
                                WriteExpression(elementType, getValueExpression, serTable),
                                Expression.Break(breakLabel)), breakLabel);
                serExpressions.Add(loop);
            }
            else if (IsList)
            {
                var argTypes = IListType.GenericTypeArguments;
                var iLength = Expression.Parameter(typeof(int), "length");
                varExpressions.Add(iLength);
                serExpressions.Add(Expression.Assign(iLength, Expression.Call(instance, SerializersTable.ListCountGetMethod)));
                serExpressions.Add(Expression.Call(serTable, SerializersTable.WriteIntMethodInfo, iLength));

                var forIdx = Expression.Parameter(typeof(int), "i");
                varExpressions.Add(forIdx);
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));

                var getValueExpression = Expression.MakeIndex(instance, SerializersTable.ListIndexProperty, new[] { Expression.PostIncrementAssign(forIdx) });

                var loop = Expression.Loop(
                            Expression.IfThenElse(
                                Expression.LessThan(forIdx, iLength),
                                WriteExpression(argTypes[0], getValueExpression, serTable),
                                Expression.Break(breakLabel)), breakLabel);
                serExpressions.Add(loop);
            }
            else if (IsDictionary)
            {
                var argTypes = IDictionaryType.GenericTypeArguments;
                var iLength = Expression.Parameter(typeof(int), "length");
                varExpressions.Add(iLength);
                serExpressions.Add(Expression.Assign(iLength, Expression.Call(instance, SerializersTable.ListCountGetMethod)));
                serExpressions.Add(Expression.Call(serTable, SerializersTable.WriteIntMethodInfo, iLength));

                var enumerator = Expression.Parameter(typeof(IDictionaryEnumerator), "enumerator");
                varExpressions.Add(enumerator);
                serExpressions.Add(Expression.Assign(enumerator, Expression.Call(instance, SerializersTable.DictionaryGetEnumeratorMethod)));

                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                var getKeyExpression = Expression.Convert(Expression.Call(enumerator, SerializersTable.DictionaryEnumeratorKeyMethod), argTypes[0]);
                var getValueExpression = Expression.Convert(Expression.Call(enumerator, SerializersTable.DictionaryEnumeratorValueMethod), argTypes[1]);

                var loop = Expression.Loop(
                            Expression.IfThenElse(
                                Expression.Call(enumerator, SerializersTable.EnumeratorMoveNextMethod),
                                Expression.Block(
                                    WriteExpression(argTypes[0], getKeyExpression, serTable),
                                    WriteExpression(argTypes[1], getValueExpression, serTable)
                                ),
                                Expression.Break(breakLabel)), breakLabel);

                serExpressions.Add(loop);
            }
            //
            if (RuntimeProperties.Length > 0)
            {
                foreach (var prop in RuntimeProperties)
                {
                    var getExpression = Expression.Property(instance, prop);
                    serExpressions.Add(WriteExpression(prop.PropertyType, getExpression, serTable));
                }
            }

            var expressionBlock = Expression.Block(varExpressions, serExpressions).Reduce();
            return expressionBlock;
        }

        public Expression WriteExpression(Type type, Expression getExpression, ParameterExpression serTable)
        {
            if (type == typeof(int))
                return SerializersTable.WriteIntExpression(getExpression, serTable);
            if (type == typeof(int?))
                return SerializersTable.WriteNulleableIntExpression(getExpression, serTable);
            if (type == typeof(bool))
                return SerializersTable.WriteBooleanExpression(getExpression, serTable);
            if (type == typeof(bool?))
                return SerializersTable.WriteNulleableBooleanExpression(getExpression, serTable);
            if (SerializersTable.WriteValues.TryGetValue(type, out var wMethodTuple))
                return Expression.Call(serTable, wMethodTuple.Method, getExpression);
            if (type.IsEnum)
                return Expression.Call(serTable, SerializersTable.WriteValues[typeof(Enum)].Method, Expression.Convert(getExpression, typeof(Enum)));
            if (type == typeof(IEnumerable) || type == typeof(IEnumerable<>) || type.ReflectedType == typeof(IEnumerable) || type.ReflectedType == typeof(IEnumerable<>) ||
                type.ReflectedType == typeof(Enumerable) || type.FullName.IndexOf("System.Linq", StringComparison.Ordinal) > -1 ||
                type.IsAssignableFrom(typeof(IEnumerable)))
                return Expression.Call(serTable, SerializersTable.InternalWriteObjectValueMInfo, getExpression);
            if (type.IsAbstract || type.IsInterface || type == typeof(object))
                return Expression.Call(serTable, SerializersTable.InternalWriteObjectValueMInfo, getExpression);
            if (type.IsSealed)
                return Expression.Call(serTable, SerializersTable.InternalSimpleWriteObjectValueMInfo, getExpression);

            return Expression.Call(serTable, SerializersTable.InternalSimpleWriteObjectValueMInfo, getExpression);
        }
    }
}
