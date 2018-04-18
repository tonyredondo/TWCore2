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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
// ReSharper disable UnusedMember.Local

namespace TWCore.Reflection
{
    /// <inheritdoc />
    /// <summary>
    /// Activator Interface
    /// </summary>
    public class CompleteAccessorsFactory : IAccessorsFactory
    {
        private static MethodInfo _cTypeMInfo;
        private static MethodInfo ChangeTypeMethodInfo 
            => _cTypeMInfo ?? (_cTypeMInfo = typeof(CompleteAccessorsFactory).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).First(n => n.Name == "ChangeType"));


        /// <inheritdoc />
        /// <summary>
        /// Create an activator delegate to a constructor info, faster than Activator.CreateInstance
        /// </summary>
        /// <param name="ctor">Constructor</param>
        /// <returns>Activator delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ActivatorDelegate CreateActivator(ConstructorInfo ctor)
        {
            Ensure.ArgumentNotNull(ctor);
            var ctorParams = ctor.GetParameters();

            var paramExp = Expression.Parameter(typeof(object[]), "args");
            var expArr = new Expression[ctorParams.Length];
            for (var i = 0; i < ctorParams.Length; i++)
            {
                var ctorType = ctorParams[i].ParameterType;
                var argExp = Expression.ArrayIndex(paramExp, Expression.Constant(i));
                var argExpConverted = Expression.Convert(Expression.Call(ChangeTypeMethodInfo, argExp, Expression.Constant(ctorType)), ctorType);
                expArr[i] = argExpConverted;
            }
            var newExp = Expression.New(ctor, expArr);
            var lambda = Expression.Lambda<ActivatorDelegate>(newExp, "New+" + ctor.Name, new []{ paramExp });
            return lambda.Compile();
        }
        /// <inheritdoc />
        /// <summary>
        /// Create an activator delegate to the default constructor info.
        /// </summary>
        /// <param name="type">Type to create the default constructor activator</param>
        /// <returns>Activator delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ActivatorDelegate CreateActivator(Type type)
        {
            Ensure.ArgumentNotNull(type);
            var ctors = type.GetConstructors();
            var ctor = ctors.FirstOrDefault(c => c.GetParameters().Length == 0) ?? ctors[0];
            return CreateActivator(ctor);
        }
        /// <inheritdoc />
        /// <summary>
        /// Build a get accessor from a property info
        /// </summary>
        /// <param name="property">Property info</param>
        /// <returns>Delegate to the get accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GetAccessorDelegate BuildGetAccessor(PropertyInfo property)
        {
            var method = property.GetMethod;
            var obj = Expression.Parameter(typeof(object), "obj");
            var instance = Expression.Convert(obj, method.DeclaringType);
            var call = Expression.Call(instance, method);
            var result = Expression.Convert(call, typeof(object));
            var expr = Expression.Lambda<GetAccessorDelegate>(result, "Get+" +  property.Name, new[] { obj });
            return expr.Compile();
        }
        /// <inheritdoc />
        /// <summary>
        /// Build a set accessor from a property info
        /// </summary>
        /// <param name="property">Property info</param>
        /// <returns>Delegate to the set accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SetAccessorDelegate BuildSetAccessor(PropertyInfo property)
        {
            var method = property.SetMethod;
            var obj = Expression.Parameter(typeof(object), "obj");
            var value = Expression.Parameter(typeof(object), "value");
            var instance = Expression.Convert(obj, method.DeclaringType);
            var castedValue = Expression.Convert(value, method.GetParameters()[0].ParameterType);
            var call = Expression.Call(instance, method, castedValue);
            var expr = Expression.Lambda<SetAccessorDelegate>(call, "Set+" + property.Name, new []{ obj, value });
            return expr.Compile();
        }
        /// <inheritdoc />
        /// <summary>
        /// Create an accessor delegte for a MethodInfo
        /// </summary>
        /// <param name="method">Method info instance</param>
        /// <returns>Accessor delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MethodAccessorDelegate BuildMethodAccessor(MethodInfo method)
        {
            var obj = Expression.Parameter(typeof(object), "o");
            var castedObject = Expression.Convert(obj, method.DeclaringType);

            var parameters = method.GetParameters();
            var paramExp = Expression.Parameter(typeof(object[]), "args");
            var expArr = new Expression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                var pType = p.ParameterType;
                Expression argExp = Expression.ArrayIndex(paramExp, Expression.Constant(i));
                if (p.HasDefaultValue)
                    argExp = Expression.Condition(Expression.Equal(argExp, Expression.Constant(null, typeof(object))),
                        Expression.Constant(p.RawDefaultValue, pType), Expression.Convert(argExp, pType));
                else if (pType != typeof(object))
                {
                    argExp = Expression.Convert(Expression.Call(ChangeTypeMethodInfo, argExp, Expression.Constant(pType)), pType);
                }
                else
                {
                    argExp = Expression.Convert(argExp, pType);
                }
                expArr[i] = argExp;
            }
            Expression callExpression = Expression.Call(castedObject, method, expArr);
            if (method.ReturnType != typeof(void))
                callExpression = Expression.Convert(callExpression, typeof(object));
            else
                callExpression = Expression.Block(callExpression, Expression.Constant(null, typeof(object)));
            return Expression.Lambda<MethodAccessorDelegate>(callExpression, "Call+" + method.Name, new[] { obj, paramExp }).Compile();
        }

        private static object ChangeType(object value, Type conversionType)
        {
            if (!conversionType.GetTypeInfo().IsGenericType ||
                conversionType.GetGenericTypeDefinition() != typeof(Nullable<>))
                return Convert.ChangeType(value, conversionType);
            if (value == null) return null;
            conversionType = Nullable.GetUnderlyingType(conversionType);
            return Convert.ChangeType(value, conversionType);
        }
    }
}
