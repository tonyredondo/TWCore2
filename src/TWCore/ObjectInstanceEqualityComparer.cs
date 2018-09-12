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
        private delegate bool EqualsDelegate(object x, object y);
        private delegate int HashCodeDelegate(object obj);
        //
        /// <summary>
        /// Object instance equality comparer default instance
        /// </summary>
        public static readonly ObjectInstanceEqualityComparer Instance = new ObjectInstanceEqualityComparer();
        //
        private static readonly ConcurrentDictionary<Type, HashCodeDelegate> HashCodeDelegates = new ConcurrentDictionary<Type, HashCodeDelegate>();
        private static readonly ConcurrentDictionary<Type, EqualsDelegate> EqualsDelegates = new ConcurrentDictionary<Type, EqualsDelegate>();
        //
        private static readonly MethodInfo ListCountGetMethod = typeof(ICollection).GetProperty("Count").GetMethod;
        private static readonly PropertyInfo ListIndexProperty = typeof(IList).GetProperty("Item");
        private static readonly MethodInfo ArrayLengthGetMethod = typeof(Array).GetProperty("Length").GetMethod;
        private static readonly MethodInfo DictionaryGetEnumeratorMethod = typeof(IDictionary).GetMethod("GetEnumerator");
        private static readonly MethodInfo DictionaryContainsMethod = typeof(IDictionary).GetMethod("Contains");
        private static readonly PropertyInfo DictionaryIndexProperty = typeof(IDictionary).GetProperty("Item");
        private static readonly MethodInfo EnumeratorMoveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");
        private static readonly MethodInfo DictionaryEnumeratorKeyMethod = typeof(IDictionaryEnumerator).GetProperty("Key").GetMethod;
        private static readonly MethodInfo DictionaryEnumeratorValueMethod = typeof(IDictionaryEnumerator).GetProperty("Value").GetMethod;

        /// <summary>
        /// Get if two objects are equal
        /// </summary>
        /// <param name="x">First object</param>
        /// <param name="y">Second object</param>
        /// <returns>True if first and second object are the same</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new bool Equals(object x, object y) => InnerEquals(x, y);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool InnerEquals(object x, object y)
        {
            if (x == null && y == null) return true;
            if (x != null && y == null) return false;
            if (x == null) return false;
            var xType = x.GetType();
            var yType = y.GetType();
            if (xType != yType) return false;
            if (xType.IsValueType || xType == typeof(string))
                return x == y;
            var equalsDelegate = EqualsDelegates.GetOrAdd(xType, type => CreateEquals(type));
            return equalsDelegate(x, y);
        }
        
        /// <summary>
        /// Gets the hashcode of an object
        /// </summary>
        /// <param name="obj">Object to calculate the hash code</param>
        /// <returns>Hash code</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(object obj)
        {
            if (obj == null) return -1;
            var oType = obj.GetType();
            if (oType.IsValueType || oType == typeof(string))
                return obj.GetHashCode();
            var hashDelegate = HashCodeDelegates.GetOrAdd(oType, type => CreateHashCode(type));
            return hashDelegate(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static HashCodeDelegate CreateHashCode(Type type)
        {
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
            var runtimeFields = type.GetRuntimeFields().OrderBy(f => f.Name).Where(field =>
            {
                if (field.IsSpecialName || field.IsStatic || field.IsLiteral ||
                    field.IsPrivate || field.IsInitOnly || field.IsNotSerialized || field.IsPinvokeImpl) return false;
                if (field.GetAttribute<NonSerializeAttribute>() != null) return false;
                return true;
            }).ToArray();
            var isArray = type.IsArray;
            var isList = false;
            var isDictionary = false;
            if (!isArray)
            {
                isDictionary = isIDictionary;
                isList = !isDictionary && isIList;
            }
            //
            var serExpressions = new List<Expression>();
            var varExpressions = new List<ParameterExpression>();
            
            var obj = Expression.Parameter(typeof(object), "obj");
            
            var instance = Expression.Parameter(type, "instance");
            varExpressions.Add(instance);
            serExpressions.Add(Expression.Assign(instance, Expression.Convert(obj, type)));
            //
            var hash = Expression.Parameter(typeof(int), "hash");
            var returnTarget = Expression.Label(typeof(int), "ReturnTarget");
            varExpressions.Add(hash);
            //
            if (isArray)
            {
                var elementType = type.GetElementType();

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
                                (Expression) Expression.AddAssign(hash, HashExpression(getValueExpression)) :
                                Expression.IfThen(Expression.ReferenceNotEqual(getValueExpression, Expression.Constant(null)), Expression.AddAssign(hash, HashExpression(getValueExpression))),
                            Expression.PostIncrementAssign(forIdx)
                        ),
                        Expression.Break(breakLabel)), breakLabel);
                serExpressions.Add(loop);
            }
            else if (isList)
            {
                var argTypes = iListType.GenericTypeArguments;

                var iLength = Expression.Parameter(typeof(int), "length");
                varExpressions.Add(iLength);
                serExpressions.Add(Expression.Assign(iLength, Expression.Call(instance, ListCountGetMethod)));

                var forIdx = Expression.Parameter(typeof(int), "i");
                varExpressions.Add(forIdx);
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));

                var getValueExpression = Expression.Convert(Expression.MakeIndex(instance, ListIndexProperty, new[] { forIdx }), argTypes[0]);

                var loop = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(forIdx, iLength),
                        Expression.Block(
                            argTypes[0].IsValueType ?
                                (Expression)Expression.AddAssign(hash, HashExpression(getValueExpression)) :
                                Expression.IfThen(Expression.ReferenceNotEqual(getValueExpression, Expression.Constant(null)), Expression.AddAssign(hash, HashExpression(getValueExpression))),
                            Expression.PostIncrementAssign(forIdx)
                        ),
                        Expression.Break(breakLabel)), breakLabel);
                serExpressions.Add(loop);
            }
            else if (isDictionary)
            {
                var argTypes = iDictionaryType.GenericTypeArguments;
                var iLength = Expression.Parameter(typeof(int), "length");
                varExpressions.Add(iLength);
                serExpressions.Add(Expression.Assign(iLength, Expression.Call(instance, ListCountGetMethod)));

                var enumerator = Expression.Parameter(typeof(IDictionaryEnumerator), "enumerator");
                varExpressions.Add(enumerator);
                serExpressions.Add(Expression.Assign(enumerator, Expression.Call(instance, DictionaryGetEnumeratorMethod)));

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
                                (Expression)Expression.AddAssign(hash, HashExpression(getKeyExpression)) :
                                Expression.IfThen(Expression.ReferenceNotEqual(getKeyExpression, Expression.Constant(null)), Expression.AddAssign(hash, HashExpression(getKeyExpression))),
                            valueElementType.IsValueType ?
                                (Expression)Expression.AddAssign(hash, HashExpression(getValueExpression)) :
                                Expression.IfThen(Expression.ReferenceNotEqual(getValueExpression, Expression.Constant(null)), Expression.AddAssign(hash, HashExpression(getValueExpression)))
                        ),
                        Expression.Break(breakLabel)), breakLabel);

                serExpressions.Add(loop);
            }
            //
            if (runtimeFields.Length > 0)
            {
                foreach (var field in runtimeFields)
                {
                    var fieldExp = Expression.Field(instance, field);
                    serExpressions.Add(
                        field.FieldType.IsValueType ?
                            (Expression)Expression.AddAssign(hash, HashExpression(fieldExp)) :
                            Expression.IfThen(Expression.ReferenceNotEqual(fieldExp, Expression.Constant(null)), Expression.AddAssign(hash, HashExpression(fieldExp))));
                }
            }
            //
            if (runtimeProperties.Length > 0)
            {
                foreach (var prop in runtimeProperties)
                {
                    var propExp = Expression.Property(instance, prop);
                    serExpressions.Add(
                        prop.PropertyType.IsValueType ?
                            (Expression)Expression.AddAssign(hash, HashExpression(propExp)) :
                            Expression.IfThen(Expression.ReferenceNotEqual(propExp, Expression.Constant(null)), Expression.AddAssign(hash, HashExpression(propExp))));
                }
            }

            serExpressions.Add(Expression.Return(returnTarget, hash, typeof(int)));
            serExpressions.Add(Expression.Label(returnTarget, hash));

            //
            var name = type.Name;
            if (isArray)
                name += "_Array";
            else if (isIList && type.GenericTypeArguments.Length > 0)
                name += "_" + type.GenericTypeArguments[0].Name;
            else if (isIDictionary && type.GenericTypeArguments.Length > 1)
                name += "_" + type.GenericTypeArguments[0].Name + "_" + type.GenericTypeArguments[1].Name;

            var expressionBlock = Expression.Block(varExpressions, serExpressions).Reduce();
            var lambda = Expression.Lambda<HashCodeDelegate>(expressionBlock, name + "_HashCode", new[] { obj });
            return lambda.Compile();
            
            Expression HashExpression(Expression gvExpression)
            {
                return Expression.ExclusiveOr(Expression.Convert(Expression.Call(gvExpression, "GetHashCode", Type.EmptyTypes), typeof(int)),
                    Expression.Constant(31));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static EqualsDelegate CreateEquals(Type type)
        {
            var ifaces = type.GetInterfaces();
            var iListType = ifaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
            var iDictionaryType = ifaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            var isIList = iListType != null;
            var isIDictionary = iDictionaryType != null;
            var runtimeProperties = type.GetRuntimeProperties().OrderBy(p =>
            {
                if (p.PropertyType.IsValueType) return 0;
                if (p.PropertyType == typeof(string)) return 0;
                return 1;
            }).ThenBy(p => p.Name).Where(prop =>
            {
                if (prop.IsSpecialName || !prop.CanRead || !prop.CanWrite) return false;
                if (prop.GetAttribute<NonSerializeAttribute>() != null) return false;
                if (prop.GetIndexParameters().Length > 0) return false;
                if (isIList && prop.Name == "Capacity") return false;
                return true;
            }).ToArray();
            var runtimeFields = type.GetRuntimeFields().OrderBy(f => f.Name).Where(field =>
            {
                if (field.IsSpecialName || field.IsStatic || field.IsLiteral ||
                    field.IsPrivate || field.IsInitOnly || field.IsNotSerialized || field.IsPinvokeImpl) return false;
                if (field.GetAttribute<NonSerializeAttribute>() != null) return false;
                return true;
            }).ToArray();
            var isArray = type.IsArray;
            var isList = false;
            var isDictionary = false;
            if (!isArray)
            {
                isDictionary = isIDictionary;
                isList = !isDictionary && isIList;
            }
            //
            var serExpressions = new List<Expression>();
            var varExpressions = new List<ParameterExpression>();
            
            var obj1 = Expression.Parameter(typeof(object), "obj1");
            var obj2 = Expression.Parameter(typeof(object), "obj2");
            //
            var instX = Expression.Parameter(type, "instX");
            var instY = Expression.Parameter(type, "instY");
            varExpressions.Add(instX);
            varExpressions.Add(instY);
            serExpressions.Add(Expression.Assign(instX, Expression.Convert(obj1, type)));
            serExpressions.Add(Expression.Assign(instY, Expression.Convert(obj2, type)));
            //
            var rtn = Expression.Parameter(typeof(bool), "rtn");
            var returnTarget = Expression.Label(typeof(bool), "ReturnTarget");
            varExpressions.Add(rtn);
            //

            
            if (isArray)
            {
                var elementType = type.GetElementType();

                var arrXLength = Expression.Parameter(typeof(int), "arrXLength");
                varExpressions.Add(arrXLength);
                serExpressions.Add(Expression.Assign(arrXLength, Expression.Call(instX, ArrayLengthGetMethod)));

                var arrYLength = Expression.Parameter(typeof(int), "arrYLength");
                varExpressions.Add(arrYLength);
                serExpressions.Add(Expression.Assign(arrYLength, Expression.Call(instY, ArrayLengthGetMethod)));

                serExpressions.Add(CheckIfNotEqualAndReturn(typeof(int), arrXLength, arrYLength, returnTarget));
                
                var forIdx = Expression.Parameter(typeof(int), "i");
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                varExpressions.Add(forIdx);
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));

                var instXGetValueExpression = Expression.ArrayIndex(instX, forIdx);
                var instYGetValueExpression = Expression.ArrayIndex(instY, forIdx);

                var loop = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(forIdx, arrXLength),
                        Expression.Block(
                            CheckIfNotEqualAndReturn(elementType, instXGetValueExpression, instYGetValueExpression, returnTarget),
                            Expression.PostIncrementAssign(forIdx)
                        ),
                        Expression.Break(breakLabel)), breakLabel);
                serExpressions.Add(loop);
            }
            else if (isList)
            {
                var argTypes = iListType.GenericTypeArguments;

                var xLength = Expression.Parameter(typeof(int), "xLength");
                varExpressions.Add(xLength);
                serExpressions.Add(Expression.Assign(xLength, Expression.Call(instX, ListCountGetMethod)));

                var yLength = Expression.Parameter(typeof(int), "yLength");
                varExpressions.Add(yLength);
                serExpressions.Add(Expression.Assign(yLength, Expression.Call(instY, ListCountGetMethod)));

                serExpressions.Add(CheckIfNotEqualAndReturn(typeof(int), xLength, yLength, returnTarget));

                var forIdx = Expression.Parameter(typeof(int), "i");
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                varExpressions.Add(forIdx);
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));

                var instXGetValueExpression = Expression.Convert(Expression.MakeIndex(instX, ListIndexProperty, new[] { forIdx }), argTypes[0]);
                var instYGetValueExpression = Expression.Convert(Expression.MakeIndex(instY, ListIndexProperty, new[] { forIdx }), argTypes[0]);

                var loop = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(forIdx, xLength),
                        Expression.Block(
                            CheckIfNotEqualAndReturn(argTypes[0], instXGetValueExpression, instYGetValueExpression, returnTarget),
                            Expression.PostIncrementAssign(forIdx)
                        ),
                        Expression.Break(breakLabel)), breakLabel);
                serExpressions.Add(loop);
            }
            else if (isDictionary)
            {
                var argTypes = iDictionaryType.GenericTypeArguments;
                
                var xLength = Expression.Parameter(typeof(int), "length");
                varExpressions.Add(xLength);
                serExpressions.Add(Expression.Assign(xLength, Expression.Call(instX, ListCountGetMethod)));
                
                var yLength = Expression.Parameter(typeof(int), "length");
                varExpressions.Add(yLength);
                serExpressions.Add(Expression.Assign(yLength, Expression.Call(instY, ListCountGetMethod)));
                
                serExpressions.Add(CheckIfNotEqualAndReturn(typeof(int), xLength, yLength, returnTarget));

                var enumerator = Expression.Parameter(typeof(IDictionaryEnumerator), "enumerator");
                varExpressions.Add(enumerator);
                serExpressions.Add(Expression.Assign(enumerator, Expression.Call(instX, DictionaryGetEnumeratorMethod)));

                var keyElementType = argTypes[0];
                var valueElementType = argTypes[1];

                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                var getKeyExpression = Expression.Convert(Expression.Call(enumerator, DictionaryEnumeratorKeyMethod), keyElementType);
                var getValueExpression = Expression.Convert(Expression.Call(enumerator, DictionaryEnumeratorValueMethod), valueElementType);
                var containsExpression = Expression.Call(instY, DictionaryContainsMethod, getKeyExpression);
                var indexValueExpression = Expression.Convert(Expression.MakeIndex(instY, DictionaryIndexProperty, new[] { getKeyExpression }), valueElementType);
                
                var loop = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Call(enumerator, EnumeratorMoveNextMethod),
                        Expression.IfThenElse(containsExpression, 
                            CheckIfNotEqualAndReturn(valueElementType, indexValueExpression, getValueExpression, returnTarget),
                            Expression.Return(returnTarget, Expression.Constant(false), typeof(bool))),
                        Expression.Break(breakLabel)), breakLabel);

                serExpressions.Add(loop);
            }
            //
            if (runtimeFields.Length > 0)
            {
                foreach (var field in runtimeFields)
                {
                    var xField = Expression.Field(instX, field);
                    var yField = Expression.Field(instY, field);
                    serExpressions.Add(CheckIfNotEqualAndReturn(field.FieldType, xField, yField, returnTarget));
                }
            }
            //
            if (runtimeProperties.Length > 0)
            {
                foreach (var prop in runtimeProperties)
                {
                    var xProp = Expression.Property(instX, prop);
                    var yProp = Expression.Property(instY, prop);
                    serExpressions.Add(CheckIfNotEqualAndReturn(prop.PropertyType, xProp, yProp, returnTarget));
                }
            }

            
            //
            serExpressions.Add(Expression.Assign(rtn, Expression.Constant(true)));
            serExpressions.Add(Expression.Return(returnTarget, rtn, typeof(bool)));
            serExpressions.Add(Expression.Label(returnTarget, rtn));
            //
            var name = type.Name;
            if (isArray)
                name += "_Array";
            else if (isIList && type.GenericTypeArguments.Length > 0)
                name += "_" + type.GenericTypeArguments[0].Name;
            else if (isIDictionary && type.GenericTypeArguments.Length > 1)
                name += "_" + type.GenericTypeArguments[0].Name + "_" + type.GenericTypeArguments[1].Name;

            var expressionBlock = Expression.Block(varExpressions, serExpressions).Reduce();
            var lambda = Expression.Lambda<EqualsDelegate>(expressionBlock, name + "_Equals", new[] { obj1, obj2 });
            return lambda.Compile();


            Expression CheckIfNotEqualAndReturn(Type elmType, Expression item1Exp, Expression item2Exp, LabelTarget rtnTarget)
            {
                return Expression.IfThen(
                    GetIfNotExpression(elmType, item1Exp, item2Exp), 
                    Expression.Return(rtnTarget, Expression.Constant(false), typeof(bool)));
            }
            Expression GetIfNotExpression(Type elmType, Expression item1Exp, Expression item2Exp)
            {
                if (elmType.IsValueType || elmType == typeof(string))
                    return Expression.NotEqual(item1Exp, item2Exp);
                return Expression.Call(typeof(ObjectInstanceEqualityComparer), "InnerEquals", Type.EmptyTypes, item1Exp, item2Exp);
            }
        }
    }
}
