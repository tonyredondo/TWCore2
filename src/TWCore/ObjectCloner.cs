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
    /// Object Cloner
    /// </summary>
    public static class ObjectCloner
    {
        private static readonly ConcurrentDictionary<Type, CloneDelegate> Descriptors = new ConcurrentDictionary<Type, CloneDelegate>();
        private delegate object CloneDelegate(object obj);
        //
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
        
        /// <summary>
        /// Clone a object and copy Properties and Fields
        /// </summary>
        /// <param name="value">Source object</param>
        /// <returns>Destination object</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Clone(object value)
        {
            if (value is null) return null;
            var vType = value.GetType();
            if (vType.IsValueType || vType == typeof(string)) return value;
            var cloneDelegate = Descriptors.GetOrAdd(vType, type => CreateCopyDelegate(type));
            return cloneDelegate(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CloneDelegate CreateCopyDelegate(Type type)
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
#if NET7_0_OR_GREATER
                if (field.IsSpecialName || field.IsStatic || field.IsLiteral || 
                    field.IsPrivate || field.IsInitOnly || field.IsPinvokeImpl) return false;
#else
                if (field.IsSpecialName || field.IsStatic || field.IsLiteral || 
                    field.IsPrivate || field.IsInitOnly || field.IsNotSerialized || field.IsPinvokeImpl) return false;
#endif
                if (field.GetAttribute<NonSerializeAttribute>() != null) return false;
                return true;
            }).ToArray();
            //
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

            
            var value = Expression.Parameter(type, "value");
            var returnTarget = Expression.Label(typeof(object), "ReturnTarget");

            varExpressions.Add(value);
            //
            if (isArray)
            {
                var elementType = type.GetElementType();

                var arrLength = Expression.Parameter(typeof(int), "length");
                varExpressions.Add(arrLength);
                serExpressions.Add(Expression.Assign(arrLength, Expression.Call(instance, ArrayLengthGetMethod)));
                serExpressions.Add(Expression.Assign(value, Expression.NewArrayBounds(elementType, arrLength)));
                
                var forIdx = Expression.Parameter(typeof(int), "i");
                varExpressions.Add(forIdx);
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));

                var setValueExpression = Expression.ArrayAccess(value, forIdx);
                var getValueExpression = Expression.ArrayIndex(instance, Expression.PostIncrementAssign(forIdx));

                var loop = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(forIdx, arrLength),
                        Expression.Assign(setValueExpression, CopyExpression(elementType, getValueExpression)),
                        Expression.Break(breakLabel)), breakLabel);
                serExpressions.Add(loop);
            }
            else if (isList)
            {
                var argTypes = iListType.GenericTypeArguments;

                var iLength = Expression.Parameter(typeof(int), "length");
                varExpressions.Add(iLength);
                serExpressions.Add(Expression.Assign(iLength, Expression.Call(instance, ListCountGetMethod)));
                serExpressions.Add(Expression.Assign(value, Expression.New(type)));

                var forIdx = Expression.Parameter(typeof(int), "i");
                varExpressions.Add(forIdx);
                var breakLabel = Expression.Label(typeof(void), "exitLoop");
                serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));
                
                var addMethod = type.GetMethod("Add", argTypes);

                var getValueExpression = Expression.Convert(Expression.MakeIndex(instance, ListIndexProperty, new[] { Expression.PostIncrementAssign(forIdx) }), argTypes[0]);

                var loop = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(forIdx, iLength),
                        Expression.Call(value, addMethod, CopyExpression(argTypes[0], getValueExpression)),
                        Expression.Break(breakLabel)), breakLabel);
                serExpressions.Add(loop);
            }
            else if (isDictionary)
            {
                var argTypes = iDictionaryType.GenericTypeArguments;
                var iLength = Expression.Parameter(typeof(int), "length");
                varExpressions.Add(iLength);
                serExpressions.Add(Expression.Assign(iLength, Expression.Call(instance, ListCountGetMethod)));
                serExpressions.Add(Expression.Assign(value, Expression.New(type)));

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
                        Expression.Call(value, addMethod, CopyExpression(keyElementType, getKeyExpression), CopyExpression(valueElementType, getValueExpression)),
                        Expression.Break(breakLabel)), breakLabel);

                serExpressions.Add(loop);
            }
            else
            {
                serExpressions.Add(Expression.Assign(value, Expression.New(type)));
            }
            //
            if (runtimeFields.Length > 0)
            {
                foreach (var field in runtimeFields)
                {
                    serExpressions.Add(Expression.Assign(Expression.Field(value, field), CopyExpression(field.FieldType, Expression.Field(instance, field))));
                }
            }
            //
            if (runtimeProperties.Length > 0)
            {
                foreach (var prop in runtimeProperties)
                {
                    var setExpression = Expression.Property(value, prop);
                    var getExpression = Expression.Property(instance, prop);
                    serExpressions.Add(Expression.Assign(setExpression, CopyExpression(prop.PropertyType, getExpression)));
                }
            }
            
            serExpressions.Add(Expression.Return(returnTarget, value, typeof(object)));
            serExpressions.Add(Expression.Label(returnTarget, value));

            
            //
            var name = type.Name;
            if (isArray)
                name += "_Array";
            else if (isIList && type.GenericTypeArguments.Length > 0)
                name += "_" + type.GenericTypeArguments[0].Name;
            else if (isIDictionary && type.GenericTypeArguments.Length > 1)
                name += "_" + type.GenericTypeArguments[0].Name + "_" + type.GenericTypeArguments[1].Name;

            var expressionBlock = Expression.Block(varExpressions, serExpressions).Reduce();
            var lambda = Expression.Lambda<CloneDelegate>(expressionBlock, name + "_Clone", new[] { obj });
            return lambda.Compile();
            
            Expression CopyExpression(Type expType, Expression getExp)
            {
                if (expType.IsValueType || expType == typeof(string))
                    return getExp;
                return Expression.Convert(Expression.Call(typeof(ObjectCloner), "Clone", Type.EmptyTypes, getExp), expType);
            }
        }
        
    }
}
