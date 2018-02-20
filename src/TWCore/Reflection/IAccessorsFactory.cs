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
    /// <summary>
    /// Activator delegate
    /// </summary>
    /// <param name="args">Arguments</param>
    /// <returns>New object instance</returns>
    public delegate object ActivatorDelegate(params object[] args);
    /// <summary>
    /// Method caller delegate
    /// </summary>
    /// <param name="instance">Object instance</param>
    /// <param name="args">Arguments to call the method</param>
    /// <returns>Return value</returns>
    public delegate object MethodAccessorDelegate(object instance, params object[] args);
    /// <summary>
    /// Get accessor delegate
    /// </summary>
    /// <param name="instance">Object instance</param>
    /// <returns>Get value</returns>
    public delegate object GetAccessorDelegate(object instance);
    /// <summary>
    /// Set accessor delegate
    /// </summary>
    /// <param name="instance">Object instance</param>
    /// <param name="value">Value to be setted</param>
    public delegate void SetAccessorDelegate(object instance, object value);

    /// <summary>
    /// Activator Interface
    /// </summary>
    public interface IAccessorsFactory
    {
        /// <summary>
        /// Create an activator delegate to a constructor info, faster than Activator.CreateInstance
        /// </summary>
        /// <param name="ctor">Constructor</param>
        /// <returns>Activator delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ActivatorDelegate CreateActivator(ConstructorInfo ctor);
        /// <summary>
        /// Create an activator delegate to the default constructor info.
        /// </summary>
        /// <param name="type">Type to create the default constructor activator</param>
        /// <returns>Activator delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ActivatorDelegate CreateActivator(Type type);
        /// <summary>
        /// Build a get accessor from a property info
        /// </summary>
        /// <param name="property">Property info</param>
        /// <returns>Delegate to the get accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        GetAccessorDelegate BuildGetAccessor(PropertyInfo property);
        /// <summary>
        /// Build a set accessor from a property info
        /// </summary>
        /// <param name="property">Property info</param>
        /// <returns>Delegate to the set accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        SetAccessorDelegate BuildSetAccessor(PropertyInfo property);
        /// <summary>
        /// Create an accessor delegte for a MethodInfo
        /// </summary>
        /// <param name="method">Method info instance</param>
        /// <returns>Accessor delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        MethodAccessorDelegate BuildMethodAccessor(MethodInfo method);
    }
}
