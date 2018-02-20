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
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TWCore.Reflection
{
    /// <inheritdoc />
    /// <summary>
    /// Activator Interface
    /// </summary>
    public class DefaultAccessorsFactory : IAccessorsFactory
    {
        /// <inheritdoc />
        /// <summary>
        /// Create an activator delegate to a constructor info, faster than Activator.CreateInstance
        /// </summary>
        /// <param name="ctor">Constructor</param>
        /// <returns>Activator delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ActivatorDelegate CreateActivator(ConstructorInfo ctor)
        {
            var type = ctor.DeclaringType;
            return args => args.Length == 0 ? Activator.CreateInstance(type) : Activator.CreateInstance(type, args);
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
            return args => args.Length == 0 ? Activator.CreateInstance(type) : Activator.CreateInstance(type, args);
        }
        /// <inheritdoc />
        /// <summary>
        /// Build a get accessor from a property info
        /// </summary>
        /// <param name="property">Property info</param>
        /// <returns>Delegate to the get accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GetAccessorDelegate BuildGetAccessor(PropertyInfo property)
            => obj => property?.GetMethod.Invoke(obj, null);
        /// <inheritdoc />
        /// <summary>
        /// Build a set accessor from a property info
        /// </summary>
        /// <param name="property">Property info</param>
        /// <returns>Delegate to the set accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SetAccessorDelegate BuildSetAccessor(PropertyInfo property)
            => (obj, value) => property?.SetMethod.Invoke(obj, new[] { value });
        /// <inheritdoc />
        /// <summary>
        /// Create an accessor delegte for a MethodInfo
        /// </summary>
        /// <param name="method">Method info instance</param>
        /// <returns>Accessor delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MethodAccessorDelegate BuildMethodAccessor(MethodInfo method)
            => method.Invoke;
    }
}
