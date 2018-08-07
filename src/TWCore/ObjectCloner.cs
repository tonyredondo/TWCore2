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
    /// Object Cloner
    /// </summary>
    public static class ObjectCloner
    {
        private static readonly ConcurrentDictionary<Type, ObjectDescriptor> Descriptors = new ConcurrentDictionary<Type, ObjectDescriptor>();
        private delegate object CopyActionDelegate(object obj);

        /// <summary>
        /// Clone a object and copy Properties and Fields
        /// </summary>
        /// <param name="value">Source object</param>
        /// <returns>Destination object</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Clone(object value)
        {
            return InnerClone(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object InnerClone(object value)
        {
            if (value == null) return null;
            var vType = value.GetType();
            if (vType.IsValueType) return value;
            var descriptor = Descriptors.GetOrAdd(vType, type => new ObjectDescriptor(type));
            return descriptor.CopyAction(value);
        }
        
        #region Nested Type
        private class ObjectDescriptor
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
            public CopyActionDelegate CopyAction;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ObjectDescriptor(Type type)
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
                if (isIList)
                    name += "_" + type.GenericTypeArguments[0].Name;
                else if (isIDictionary)
                    name += "_" + type.GenericTypeArguments[0].Name + "_" + type.GenericTypeArguments[1].Name;                    
                
                var lambda = Expression.Lambda<CopyActionDelegate>(expression, name + "_Clone", new[] { obj });
                CopyAction = lambda.Compile();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expression GetExpression(Type type, Expression instance)
            {
                var serExpressions = new List<Expression>();
                var varExpressions = new List<ParameterExpression>();
                //
                var value = Expression.Parameter(type, "value");
                var returnTarget = Expression.Label(typeof(object), "ReturnTarget");

                varExpressions.Add(value);
                //
                if (IsArray)
                {
                    var elementType = Type.GetElementType();

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
                else if (IsList)
                {
                    var argTypes = IListType.GenericTypeArguments;

                    var iLength = Expression.Parameter(typeof(int), "length");
                    varExpressions.Add(iLength);
                    serExpressions.Add(Expression.Assign(iLength, Expression.Call(instance, ListCountGetMethod)));
                    serExpressions.Add(Expression.Assign(value, Expression.New(type)));

                    var forIdx = Expression.Parameter(typeof(int), "i");
                    varExpressions.Add(forIdx);
                    var breakLabel = Expression.Label(typeof(void), "exitLoop");
                    serExpressions.Add(Expression.Assign(forIdx, Expression.Constant(0)));
                    
                    var addMethod = type.GetMethod("Add", argTypes);

                    //var setValueExpression = Expression.MakeIndex(value, ListIndexProperty, new[] { forIdx });
                    var getValueExpression = Expression.MakeIndex(instance, ListIndexProperty, new[] { Expression.PostIncrementAssign(forIdx) });

                    var loop = Expression.Loop(
                        Expression.IfThenElse(
                            Expression.LessThan(forIdx, iLength),
                            Expression.Call(value, addMethod, CopyExpression(argTypes[0], getValueExpression)),
                            //Expression.Assign(setValueExpression, CopyExpression(argTypes[0], getValueExpression)),
                            Expression.Break(breakLabel)), breakLabel);
                    serExpressions.Add(loop);
                }
                else if (IsDictionary)
                {
                    var argTypes = IDictionaryType.GenericTypeArguments;
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
                if (RuntimeFields.Length > 0)
                {
                    foreach (var field in RuntimeFields)
                    {
                        serExpressions.Add(Expression.Assign(Expression.Field(value, field), CopyExpression(field.FieldType, Expression.Field(instance, field))));
                    }
                }
                //
                if (RuntimeProperties.Length > 0)
                {
                    foreach (var prop in RuntimeProperties)
                    {
                        var setExpression = Expression.Property(value, prop);
                        var getExpression = Expression.Property(instance, prop);
                        serExpressions.Add(Expression.Assign(setExpression, CopyExpression(prop.PropertyType, getExpression)));
                    }
                }
                
                serExpressions.Add(Expression.Return(returnTarget, value, typeof(object)));
                serExpressions.Add(Expression.Label(returnTarget, value));

                var block = Expression.Block(varExpressions, serExpressions).Reduce();
                
                return block;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Expression CopyExpression(Type type, Expression getValueExpression)
            {
                if (type.IsValueType || type == typeof(string))
                    return getValueExpression;
                return Expression.Convert(Expression.Call(typeof(ObjectCloner), "InnerClone", Type.EmptyTypes, getValueExpression), type);
            }
        }
        #endregion
    }
}
