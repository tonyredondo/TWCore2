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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
#pragma warning disable 1591

namespace TWCore.Serialization.RawSerializer
{
    public delegate void SerializeActionDelegate(object obj, SerializersTable table);

    public class SerializerTypeDescriptor
    {
        private static readonly MethodInfo RawSerializableSerializeMInfo = typeof(IRawSerializable).GetMethod("Serialize", BindingFlags.Public | BindingFlags.Instance);

        public readonly bool IsRawSerializable;
        public readonly byte[] Definition;
        public readonly Type Type;
        public readonly LambdaExpression InnerWriterLambda;
        public readonly SerializeActionDelegate SerializeAction;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializerTypeDescriptor(Type type, HashSet<Type> previousTypes = null)
        {
            Type = type;
            if (previousTypes == null)
                previousTypes = new HashSet<Type>();
            var ifaces = type.GetInterfaces();
            var iListType = ifaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
            var iDictionaryType = ifaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            var isIList = iListType != null;
            var isIDictionary = iDictionaryType != null;
            var runtimeProperties = type.GetRuntimeProperties().OrderBy(p => p.Name).Where(prop =>
            {
                if (prop.IsSpecialName || !prop.CanRead || !prop.CanWrite) return false;
                if (prop.GetAttribute<NonSerializeAttribute>() != null) return false;
                if (prop.GetIndexParameters().Length > 0) return false;
                if (isIList && prop.Name == "Capacity") return false;
                return true;
            }).ToArray();

            //
            var properties = runtimeProperties.Select(p => p.Name).ToArray();
            IsRawSerializable = ifaces.Any(i => i == typeof(IRawSerializable));
            var isArray = type.IsArray;
            var isList = false;
            if (!isArray)
                isList = !isIDictionary && isIList;
            //
            var typeName = type.GetTypeName();
            var defText = typeName + ";" + (isArray ? "1" : "0") + ";" + (isList ? "1" : "0") + ";" + (isIDictionary ? "1" : "0") + ";" + properties.Join(";");
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
            var serExpressions = new List<Expression>();
            var varExpressions = new List<ParameterExpression>();

            //
            var obj = Expression.Parameter(typeof(object), "obj");
            var serTable = Expression.Parameter(typeof(SerializersTable), "table");

            var instance = Expression.Parameter(type, "instance");
            varExpressions.Add(instance);
            serExpressions.Add(Expression.Assign(instance, Expression.Convert(obj, type)));
            //
            if (isArray)
            {
                var elementType = type.GetElementType();

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
            else if (isList)
            {
                var argTypes = iListType.GenericTypeArguments;
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
            else if (isIDictionary)
            {
                var argTypes = iDictionaryType.GenericTypeArguments;
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
            if (runtimeProperties.Length > 0)
            {
                foreach (var prop in runtimeProperties)
                {
                    var getExpression = Expression.Property(instance, prop);
                    serExpressions.Add(WriteExpression(prop.PropertyType, getExpression, serTable));
                }
            }
            //
            var serExpression = Expression.Block(varExpressions, serExpressions).Reduce();

            var innerValue = Expression.Parameter(type, "innerValue");
            var innerSerExpression = Expression.Block(varExpressions, new[] { Expression.Assign(instance, innerValue) }.Concat(serExpressions.Skip(1))).Reduce();
            InnerWriterLambda = Expression.Lambda(innerSerExpression, type.Name + "Writer", new[] { innerValue, serTable });

            var serializationLambda = Expression.Lambda<SerializeActionDelegate>(serExpression, type.Name + "_Serializer", new[] { obj, serTable });
            SerializeAction = serializationLambda.Compile();

			Expression WriteExpression(Type itemType, Expression itemGetExpression, ParameterExpression serTableExpression)
			{
				if (itemType == typeof(int))
					return SerializersTable.WriteIntExpression(itemGetExpression, serTableExpression);
				if (itemType == typeof(int?))
					return SerializersTable.WriteNulleableIntExpression(itemGetExpression, serTableExpression);
				if (itemType == typeof(bool))
					return SerializersTable.WriteBooleanExpression(itemGetExpression, serTableExpression);
				if (itemType == typeof(bool?))
					return SerializersTable.WriteNulleableBooleanExpression(itemGetExpression, serTableExpression);
				if (SerializersTable.WriteValues.TryGetValue(itemType, out var wMethodTuple))
					return Expression.Call(serTableExpression, wMethodTuple.Method, itemGetExpression);
				if (itemType.IsEnum)
					return Expression.Call(serTableExpression, SerializersTable.WriteValues[typeof(Enum)].Method, Expression.Convert(itemGetExpression, typeof(Enum)));
                if (itemType.IsGenericType && itemType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var nullParam = Expression.Parameter(itemType);
                    var nullBlock = Expression.Block(new[] { nullParam },
                            Expression.Assign(nullParam, itemGetExpression),
                            Expression.IfThenElse(
                                Expression.Equal(nullParam, Expression.Constant(null, itemType)),
                                Expression.Call(serTable, SerializersTable.WriteByteMethodInfo, Expression.Constant(DataBytesDefinition.ValueNull)),
                                WriteExpression(itemType.GenericTypeArguments[0], nullParam, serTableExpression)
                            )
                        );
                    return nullBlock;
                }
                if (itemType == typeof(IEnumerable) || itemType == typeof(IEnumerable<>) || itemType.ReflectedType == typeof(IEnumerable) || itemType.ReflectedType == typeof(IEnumerable<>) ||
					itemType.ReflectedType == typeof(Enumerable) || itemType.FullName.IndexOf("System.Linq", StringComparison.Ordinal) > -1 ||
					itemType.IsAssignableFrom(typeof(IEnumerable)))
					return Expression.Call(serTableExpression, SerializersTable.InternalWriteObjectValueMInfo, itemGetExpression);
				if (itemType.IsAbstract || itemType.IsInterface || itemType == typeof(object))
					return Expression.Call(serTableExpression, SerializersTable.InternalWriteObjectValueMInfo, itemGetExpression);
				if (itemType.IsSealed)
					return WriteSealedExpression(itemType, itemGetExpression, serTableExpression);

				return WriteMixedExpression(itemType, itemGetExpression, serTableExpression);

				//return Expression.Call(serTableExpression, SerializersTable.InternalSimpleWriteObjectValueMInfo, itemGetExpression);
			}

			Expression WriteSealedExpression(Type itemType, Expression itemGetExpression, ParameterExpression serTableExpression)
			{
				if (itemType == type)
					return Expression.Call(serTableExpression, SerializersTable.InternalSimpleWriteObjectValueMInfo, itemGetExpression);
                if (!previousTypes.Add(itemType))
					return Expression.Call(serTableExpression, SerializersTable.InternalSimpleWriteObjectValueMInfo, itemGetExpression);

                var innerDescriptor = SerializersTable.Descriptors.GetOrAdd(itemType, (tp, ptypes) => new SerializerTypeDescriptor(tp, ptypes), previousTypes);

                var innerVarExpressions = new List<ParameterExpression>();
				var innerSerExpressions = new List<Expression>();

				var returnLabel = Expression.Label(typeof(void), "return");

				var value = Expression.Parameter(itemType, "value");
				innerVarExpressions.Add(value);
				innerSerExpressions.Add(Expression.Assign(value, Expression.Convert(itemGetExpression, itemType)));

				#region Null Check
				innerSerExpressions.Add(
					Expression.IfThen(Expression.ReferenceEqual(value, Expression.Constant(null)),
						Expression.Block(
							Expression.Call(serTable, SerializersTable.WriteByteMethodInfo, Expression.Constant(DataBytesDefinition.ValueNull)),
							Expression.Return(returnLabel)
						)
					)
				);
				#endregion

				#region Object Cache Check
				var objCacheExp = Expression.Field(serTableExpression, "_objectCache");
				var objIdxExp = Expression.Variable(typeof(int), "objIdx");

				innerVarExpressions.Add(objIdxExp);

				innerSerExpressions.Add(
					Expression.IfThenElse(Expression.Call(objCacheExp, SerializersTable.TryGetValueObjectSerializerCacheMethod, value, objIdxExp),
						Expression.Block(
							Expression.Call(serTable, SerializersTable.WriteRefObjectMInfo, objIdxExp),
							Expression.Return(returnLabel)
						),
						Expression.Call(objCacheExp, SerializersTable.SetObjectSerializerCacheMethod, value)
					)
				);
				#endregion

				#region Type Cache Check
				var typeCacheExp = Expression.Field(serTableExpression, "_typeCache");
				var typeIdxExp = Expression.Variable(typeof(int), "tIdx");

				innerVarExpressions.Add(typeIdxExp);

				var descDefinition = typeof(SerializerTypeDescriptor<>).MakeGenericType(itemType).GetField("Definition", BindingFlags.Public | BindingFlags.Static);

				innerSerExpressions.Add(
					Expression.IfThenElse(Expression.Call(typeCacheExp, SerializersTable.TryGetValueTypeSerializerCacheMethod, Expression.Constant(itemType, typeof(Type)), typeIdxExp),
						Expression.Call(serTable, SerializersTable.WriteRefTypeMInfo, typeIdxExp),
						Expression.Block(
							Expression.Call(serTable, SerializersTable.WriteBytesMethodInfo, Expression.Field(null, descDefinition)),
							Expression.Call(typeCacheExp, SerializersTable.SetTypeSerializerCacheMethod, Expression.Constant(itemType, typeof(Type)))
						)
					)
				);

				#endregion

				#region Serializer Call
				if (innerDescriptor.IsRawSerializable)
				{
					innerSerExpressions.Add(Expression.Call(Expression.Convert(value, typeof(IRawSerializable)), RawSerializableSerializeMInfo, serTableExpression));
				}
				else
				{
					innerSerExpressions.Add(Expression.Invoke(innerDescriptor.InnerWriterLambda, value, serTable));
				}
				#endregion

				innerSerExpressions.Add(Expression.Call(serTable, SerializersTable.WriteByteMethodInfo, Expression.Constant(DataBytesDefinition.TypeEnd)));
				innerSerExpressions.Add(Expression.Label(returnLabel));
				var innerExpression = Expression.Block(innerVarExpressions, innerSerExpressions).Reduce();

                previousTypes.Remove(itemType);
				return innerExpression;
			}

			Expression WriteMixedExpression(Type itemType, Expression itemGetExpression, ParameterExpression serTableExpression)
			{
				if (itemType == type)
					return Expression.Call(serTableExpression, SerializersTable.InternalSimpleWriteObjectValueMInfo, itemGetExpression);
                if (!previousTypes.Add(itemType))
					return Expression.Call(serTableExpression, SerializersTable.InternalSimpleWriteObjectValueMInfo, itemGetExpression);

                var innerDescriptor = SerializersTable.Descriptors.GetOrAdd(itemType, (tp, ptypes) => new SerializerTypeDescriptor(tp, ptypes), previousTypes);

                var innerVarExpressions = new List<ParameterExpression>();
				var innerSerExpressions = new List<Expression>();

				var returnLabel = Expression.Label(typeof(void), "return");
				var objValue = Expression.Parameter(typeof(object), "objValue");

				innerVarExpressions.Add(objValue);
				innerSerExpressions.Add(Expression.Assign(objValue, itemGetExpression));

				#region Null Check
				innerSerExpressions.Add(
					Expression.IfThen(Expression.ReferenceEqual(objValue, Expression.Constant(null)),
						Expression.Block(
							Expression.Call(serTable, SerializersTable.WriteByteMethodInfo, Expression.Constant(DataBytesDefinition.ValueNull)),
							Expression.Return(returnLabel)
						)
					)
				);
				#endregion

				#region Object Cache Check
				var objCacheExp = Expression.Field(serTableExpression, "_objectCache");
				var objIdxExp = Expression.Variable(typeof(int), "objIdx");

				innerVarExpressions.Add(objIdxExp);

				innerSerExpressions.Add(
					Expression.IfThenElse(Expression.Call(objCacheExp, SerializersTable.TryGetValueObjectSerializerCacheMethod, objValue, objIdxExp),
						Expression.Block(
							Expression.Call(serTable, SerializersTable.WriteRefObjectMInfo, objIdxExp),
							Expression.Return(returnLabel)
						),
						Expression.Call(objCacheExp, SerializersTable.SetObjectSerializerCacheMethod, objValue)
					)
				);
				#endregion


				var valueType = Expression.Parameter(typeof(Type), "valueType");
				innerVarExpressions.Add(valueType);
				innerSerExpressions.Add(Expression.Assign(valueType, Expression.Call(objValue, SerializersTable.GetTypeMethodInfo)));

				innerSerExpressions.Add(
					Expression.IfThenElse(Expression.TypeIs(objValue, itemType),
						WriteShortSealedExpression(itemType, objValue, serTableExpression, returnLabel, innerDescriptor),
						Expression.Call(serTableExpression, SerializersTable.InternalMixedWriteObjectValueMInfo, objValue, valueType)
					)
				);

				innerSerExpressions.Add(Expression.Call(serTable, SerializersTable.WriteByteMethodInfo, Expression.Constant(DataBytesDefinition.TypeEnd)));
				innerSerExpressions.Add(Expression.Label(returnLabel));
				var innerExpression = Expression.Block(innerVarExpressions, innerSerExpressions).Reduce();

                previousTypes.Remove(itemType);
				return innerExpression;
			}
			Expression WriteShortSealedExpression(Type itemType, Expression itemGetExpression, ParameterExpression serTableExpression, LabelTarget returnLabel, SerializerTypeDescriptor innerDescriptor)
			{
				var innerVarExpressions = new List<ParameterExpression>();
				var innerSerExpressions = new List<Expression>();


				var value = Expression.Parameter(itemType, "value");
				innerVarExpressions.Add(value);
				innerSerExpressions.Add(Expression.Assign(value, Expression.Convert(itemGetExpression, itemType)));

				#region Type Cache Check
				var typeCacheExp = Expression.Field(serTableExpression, "_typeCache");
				var typeIdxExp = Expression.Variable(typeof(int), "tIdx");

				innerVarExpressions.Add(typeIdxExp);

				var descDefinition = typeof(SerializerTypeDescriptor<>).MakeGenericType(itemType).GetField("Definition", BindingFlags.Public | BindingFlags.Static);

				innerSerExpressions.Add(
					Expression.IfThenElse(Expression.Call(typeCacheExp, SerializersTable.TryGetValueTypeSerializerCacheMethod, Expression.Constant(itemType, typeof(Type)), typeIdxExp),
						Expression.Call(serTable, SerializersTable.WriteRefTypeMInfo, typeIdxExp),
						Expression.Block(
							Expression.Call(serTable, SerializersTable.WriteBytesMethodInfo, Expression.Field(null, descDefinition)),
							Expression.Call(typeCacheExp, SerializersTable.SetTypeSerializerCacheMethod, Expression.Constant(itemType, typeof(Type)))
						)
					)
				);

				#endregion

				#region Serializer Call
				if (innerDescriptor.IsRawSerializable)
				{
					innerSerExpressions.Add(Expression.Call(Expression.Convert(value, typeof(IRawSerializable)), RawSerializableSerializeMInfo, serTableExpression));
				}
				else
				{
					innerSerExpressions.Add(Expression.Invoke(innerDescriptor.InnerWriterLambda, value, serTable));
				}
				#endregion

				var innerExpression = Expression.Block(innerVarExpressions, innerSerExpressions).Reduce();

				return innerExpression;
			}

		}
	}


    public static class SerializerTypeDescriptor<T>
    {
        public static byte[] Definition;
        public static SerializerTypeDescriptor Descriptor;

        static SerializerTypeDescriptor()
        {
            Descriptor = SerializersTable.Descriptors.GetOrAdd(typeof(T), t => new SerializerTypeDescriptor(t));
            Definition = Descriptor.Definition;
        }
    }


}
