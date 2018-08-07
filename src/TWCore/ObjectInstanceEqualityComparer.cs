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
using NonBlocking;
using TWCore.Serialization;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBeProtected.Local
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable UnusedMember.Local
// ReSharper disable NotAccessedField.Local
// ReSharper disable CoVariantArrayConversion
// ReSharper disable MemberCanBePrivate.Local
#pragma warning disable 414

namespace TWCore
{
    /// <summary>
    /// Object instance Equality comparer
    /// </summary>
    public class ObjectInstanceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ObjectInstanceEqualityComparer Instance = new ObjectInstanceEqualityComparer();
        private static readonly ConcurrentDictionary<Type, ObjectHashDescriptor> HashDescriptors = new ConcurrentDictionary<Type, ObjectHashDescriptor>();
        private delegate bool EqualsDelegate(object x, object y);
        private delegate int HashCodeDelegate(object obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new bool Equals(object x, object y)
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(object obj)
        {
            if (obj == null) return -1;
            var oType = obj.GetType();
            if (oType.IsValueType || oType == typeof(string))
                return obj.GetHashCode();
            var desc = HashDescriptors.GetOrAdd(oType, type => new ObjectHashDescriptor(type));
            return desc.HashCodeDelegate(obj);
        }

        #region Nested Type
        private class ObjectHashDescriptor
        {
            private static readonly MethodInfo ListCountGetMethod = typeof(ICollection).GetProperty("Count").GetMethod;
            private static readonly PropertyInfo ListIndexProperty = typeof(IList).GetProperty("Item");
            //
            private static readonly MethodInfo ArrayLengthGetMethod = typeof(Array).GetProperty("Length").GetMethod;
            //
            private static readonly MethodInfo DictionaryGetEnumeratorMethod = typeof(IDictionary).GetMethod("GetEnumerator");
            private static readonly MethodInfo EnumeratorMoveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");
            private static readonly MethodInfo DictionaryEnumeratorKeyMethod = typeof(IDictionaryEnumerator).GetProperty("Key").GetMethod;
            private static readonly MethodInfo DictionaryEnumeratorValueMethod = typeof(IDictionaryEnumerator).GetProperty("Value").GetMethod;
            //
            public Type Type;
            public PropertyInfo[] RuntimeProperties;
            public FieldInfo[] RuntimeFields;
            public Type IListType;
            public Type IDictionaryType;
            //
            public string[] Properties;
            public bool IsArray;
            public bool IsList;
            public bool IsDictionary;
            public HashCodeDelegate HashCodeDelegate;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ObjectHashDescriptor(Type type)
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
                RuntimeFields = type.GetRuntimeFields().OrderBy(f => f.Name).Where(field =>
                {
                    if (field.IsSpecialName || field.IsStatic || field.IsLiteral ||
                        field.IsPrivate || field.IsInitOnly || field.IsNotSerialized || field.IsPinvokeImpl) return false;
                    if (field.GetAttribute<NonSerializeAttribute>() != null) return false;
                    return true;
                }).ToArray();
                //
                Properties = RuntimeProperties.Select(p => p.Name).ToArray();
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
                var obj = Expression.Parameter(typeof(object), "obj");

                var instance = Expression.Parameter(type, "instance");
                var expression = Expression.Block(new[] { instance },
                    Expression.Assign(instance, Expression.Convert(obj, type)),
                    GetExpression(type, instance));

                var name = type.Name;
                if (IsArray)
                    name += "_Array";
                else if (isIList)
                    name += "_" + type.GenericTypeArguments[0].Name;
                else if (isIDictionary)
                    name += "_" + type.GenericTypeArguments[0].Name + "_" + type.GenericTypeArguments[1].Name;

                var lambda = Expression.Lambda<HashCodeDelegate>(expression, name + "_HashCode", new[] { obj });
                HashCodeDelegate = lambda.Compile();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expression GetExpression(Type type, Expression instance)
            {
                var serExpressions = new List<Expression>();
                var varExpressions = new List<ParameterExpression>();
                //
                var hash = Expression.Parameter(typeof(int), "hash");
                var returnTarget = Expression.Label(typeof(int), "ReturnTarget");
                varExpressions.Add(hash);

                //
                if (IsArray)
                {
                    var elementType = Type.GetElementType();

                    var arrLength = Expression.Parameter(typeof(int), "length");
                    varExpressions.Add(arrLength);
                    serExpressions.Add(Expression.Assign(arrLength, Expression.Call(instance, ArrayLengthGetMethod)));

                    var forIdx = Expression.Parameter(typeof(int), "i");
                    varExpressions.Add(forIdx);
                    var breakLabel = Expression.Label(typeof(void), "exitLoop");
                    serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));

                    var getValueExpression = Expression.ArrayIndex(instance, forIdx);

                    var loop = Expression.Loop(
                        Expression.IfThenElse(
                            Expression.LessThan(forIdx, arrLength),
                            Expression.Block(
                                elementType.IsValueType ?
                                    (Expression) Expression.AddAssign(hash, HashExpression(elementType, getValueExpression)) :
                                    Expression.IfThen(Expression.ReferenceNotEqual(getValueExpression, Expression.Constant(null)), Expression.AddAssign(hash, HashExpression(elementType, getValueExpression))),
                                Expression.PostIncrementAssign(forIdx)
                            ),
                            Expression.Break(breakLabel)), breakLabel);
                    serExpressions.Add(loop);
                }
                else if (IsList)
                {
                    var argTypes = IListType.GenericTypeArguments;

                    var iLength = Expression.Parameter(typeof(int), "length");
                    varExpressions.Add(iLength);
                    serExpressions.Add(Expression.Assign(iLength, Expression.Call(instance, ListCountGetMethod)));

                    var forIdx = Expression.Parameter(typeof(int), "i");
                    varExpressions.Add(forIdx);
                    var breakLabel = Expression.Label(typeof(void), "exitLoop");
                    serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));

                    var addMethod = type.GetMethod("Add", argTypes);

                    var getValueExpression = Expression.MakeIndex(instance, ListIndexProperty, new[] { forIdx });

                    var loop = Expression.Loop(
                        Expression.IfThenElse(
                            Expression.LessThan(forIdx, iLength),
                            Expression.Block(
                                argTypes[0].IsValueType ?
                                    (Expression)Expression.AddAssign(hash, HashExpression(argTypes[0], getValueExpression)) :
                                    Expression.IfThen(Expression.ReferenceNotEqual(getValueExpression, Expression.Constant(null)), Expression.AddAssign(hash, HashExpression(argTypes[0], getValueExpression))),
                                Expression.PostIncrementAssign(forIdx)
                            ),
                            Expression.Break(breakLabel)), breakLabel);
                    serExpressions.Add(loop);
                }
                else if (IsDictionary)
                {
                    var argTypes = IDictionaryType.GenericTypeArguments;
                    var iLength = Expression.Parameter(typeof(int), "length");
                    varExpressions.Add(iLength);
                    serExpressions.Add(Expression.Assign(iLength, Expression.Call(instance, ListCountGetMethod)));

                    var enumerator = Expression.Parameter(typeof(IDictionaryEnumerator), "enumerator");
                    varExpressions.Add(enumerator);
                    serExpressions.Add(Expression.Assign(enumerator, Expression.Call(instance, DictionaryGetEnumeratorMethod)));

                    var addMethod = type.GetMethod("Add", argTypes);
                    var keyElementType = argTypes[0];
                    var valueElementType = argTypes[1];

                    var breakLabel = Expression.Label(typeof(void), "exitLoop");
                    var getKeyExpression = Expression.Convert(Expression.Call(enumerator, DictionaryEnumeratorKeyMethod), keyElementType);
                    var getValueExpression = Expression.Convert(Expression.Call(enumerator, DictionaryEnumeratorValueMethod), valueElementType);

                    var loop = Expression.Loop(
                        Expression.IfThenElse(
                            Expression.Call(enumerator, EnumeratorMoveNextMethod),
                            Expression.Block(
                                keyElementType.IsValueType ?
                                    (Expression)Expression.AddAssign(hash, HashExpression(keyElementType, getKeyExpression)) :
                                    Expression.IfThen(Expression.ReferenceNotEqual(getKeyExpression, Expression.Constant(null)), Expression.AddAssign(hash, HashExpression(keyElementType, getKeyExpression))),
                                valueElementType.IsValueType ?
                                    (Expression)Expression.AddAssign(hash, HashExpression(valueElementType, getValueExpression)) :
                                    Expression.IfThen(Expression.ReferenceNotEqual(getValueExpression, Expression.Constant(null)), Expression.AddAssign(hash, HashExpression(valueElementType, getValueExpression)))
                            ),
                            Expression.Break(breakLabel)), breakLabel);

                    serExpressions.Add(loop);
                }
                //
                if (RuntimeFields.Length > 0)
                {
                    foreach (var field in RuntimeFields)
                    {
                        var fieldExp = Expression.Field(instance, field);
                        serExpressions.Add(
                            field.FieldType.IsValueType ?
                                (Expression)Expression.AddAssign(hash, HashExpression(field.FieldType, fieldExp)) :
                                Expression.IfThen(Expression.ReferenceNotEqual(fieldExp, Expression.Constant(null)), Expression.AddAssign(hash, HashExpression(field.FieldType, fieldExp))));
                    }
                }
                //
                if (RuntimeProperties.Length > 0)
                {
                    foreach (var prop in RuntimeProperties)
                    {
                        var propExp = Expression.Property(instance, prop);
                        serExpressions.Add(
                            prop.PropertyType.IsValueType ?
                                (Expression)Expression.AddAssign(hash, HashExpression(prop.PropertyType, propExp)) :
                                Expression.IfThen(Expression.ReferenceNotEqual(propExp, Expression.Constant(null)), Expression.AddAssign(hash, HashExpression(prop.PropertyType, propExp))));
                    }
                }

                serExpressions.Add(Expression.Return(returnTarget, hash, typeof(int)));
                serExpressions.Add(Expression.Label(returnTarget, hash));

                var block = Expression.Block(varExpressions, serExpressions).Reduce();

                return block;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expression HashExpression(Type type, Expression getValueExpression)
            {
                return Expression.Convert(Expression.Call(getValueExpression, "GetHashCode", Type.EmptyTypes), typeof(int));
            }

        }
        #endregion
    }
}
