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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TWCore.Reflection;
// ReSharper disable CheckNamespace

namespace TWCore
{
    /// <summary>
    /// Type extensions
    /// </summary>
    public static partial class Extensions
    {
        private static readonly string[] IgnoredAssemblies = { ", System.Private.CoreLib", ", mscorlib", ", NETStandard" };
        private static readonly Regex AssemblyVersionRegex =
            new Regex(", Version=[a-zA-Z0-9.]*, Culture=[a-zA-Z0-9]*, PublicKeyToken=[a-zA-Z0-9]*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly NonBlocking.ConcurrentDictionary<Type, string> TypesNameCache = new NonBlocking.ConcurrentDictionary<Type, string>();

        /// <summary>
        /// Gets the UnderlyingType behind a Base type in case a nullable type.
        /// </summary>
        /// <param name="type">Base type</param>
        /// <returns>Underlying type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetUnderlyingType(this Type type)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);
            return type;
        }
        /// <summary>
        /// Gets a MethodInfo of a Method name from a Type
        /// </summary>
        /// <param name="type">Type where is the method</param>
        /// <param name="name">Method name</param>
        /// <returns>Method info object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetMethod(this Type type, string name)
            => type.GetTypeInfo().GetDeclaredMethod(name);

        /// <summary>
        /// Gets the implemented interfaces of a Type
        /// </summary>
        /// <param name="type">Type to look at</param>
        /// <returns>IEnumerable with the implemented interfaces</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Type> GetInterfaces(this Type type)
            => type.GetTypeInfo().ImplementedInterfaces;

        /// <summary>
        /// Gets the declared properties of an object
        /// </summary>
        /// <param name="type">Type to get the declared properties</param>
        /// <returns>Declared properties IEnumerable</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<PropertyInfo> GetProperties(this Type type)
            => type.GetTypeInfo().DeclaredProperties;
        /// <summary>
        /// Gets the fast property info getter and setter delegates
        /// </summary>
        /// <param name="prop">Property info</param>
        /// <returns>FastPropertyInfo instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FastPropertyInfo GetFastPropertyInfo(this PropertyInfo prop)
            => FastPropertyInfo.Get(prop);

        /// <summary>
        /// Gets the generic method
        /// </summary>
        /// <param name="type">Source Type</param>
        /// <param name="name">Method name</param>
        /// <param name="parameterTypes">Parameters type</param>
        /// <returns>Method info</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetGenericMethod(this Type type, string name, params Type[] parameterTypes)
        {
            var methods = type.GetRuntimeMethods().Where((method, mName) => method.Name == mName, name);
            foreach (var method in methods)
            {
                if (method.HasParameters(parameterTypes))
                    return method;
            }
            return null;
        }
        /// <summary>
        /// Gets the method accessor
        /// </summary>
        /// <param name="method">Source MethodInfo</param>
        /// <returns>Method accessor delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodAccessorDelegate GetMethodAccessor(this MethodInfo method)
            => Factory.Accessors.BuildMethodAccessor(method);
        /// <summary>
        /// Gets if the method infor has the requiered parameters
        /// </summary>
        /// <param name="method">Method info</param>
        /// <param name="parameterTypes">Parameters type</param>
        /// <returns>True if the method has the parameters; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasParameters(this MethodInfo method, params Type[] parameterTypes)
        {
            var methodParameters = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
            if (methodParameters.Length != parameterTypes.Length)
                return false;
            for (var i = 0; i < methodParameters.Length; i++)
                if (methodParameters[i].ToString() != parameterTypes[i].ToString())
                    return false;
            return true;
        }
        /// <summary>
        /// Gets all interfaces
        /// </summary>
        /// <param name="target">Source type</param>
        /// <returns>IEnumerable of types</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Type> AllInterfaces(this Type target)
        {
            foreach (var IF in target.GetInterfaces())
            {
                yield return IF;
                foreach (var childIf in IF.AllInterfaces())
                    yield return childIf;
            }
        }
        /// <summary>
        /// Gets all methods
        /// </summary>
        /// <param name="target">Source type</param>
        /// <returns>IEnumerable of method info</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<MethodInfo> AllMethods(this Type target)
        {
            var allTypes = target.AllInterfaces().ToList();
            allTypes.Add(target);

            return from type in allTypes
                   from method in type.GetTypeInfo().DeclaredMethods
                   where method.IsSpecialName == false
                   select method;
        }
        /// <summary>
        /// Get Type name (short version for AssemblyQualifiedName)
        /// </summary>
        /// <param name="type">Source type</param>
        /// <returns></returns>
        public static string GetTypeName(this Type type)
        {
            Ensure.ArgumentNotNull(type, "The type can't be null.");
            return TypesNameCache.GetOrAdd(type, mType =>
            {
                var aqn = mType.AssemblyQualifiedName;
                var asn = AssemblyVersionRegex.Replace(aqn, string.Empty);
                foreach (var iasm in IgnoredAssemblies)
                    asn = asn.Replace(iasm, string.Empty);
                return asn;
            });
        }
    }
}
