/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TWCore.Reflection
{
    /// <summary>
    /// Faster Property info getter and setter
    /// </summary>
    public class FastPropertyInfo
    {
        static ConcurrentDictionary<PropertyInfo, FastPropertyInfo> Infos = new ConcurrentDictionary<PropertyInfo, FastPropertyInfo>();
        static ConcurrentDictionary<PropertyInfo, GetAccessorDelegate> GetAccessorCache = new ConcurrentDictionary<PropertyInfo, GetAccessorDelegate>();
        static ConcurrentDictionary<PropertyInfo, SetAccessorDelegate> SetAccessorCache = new ConcurrentDictionary<PropertyInfo, SetAccessorDelegate>();

        /// <summary>
        /// Getter delegate
        /// </summary>
        public readonly GetAccessorDelegate GetValue;
        /// <summary>
        /// Setter delegate
        /// </summary>
        public readonly SetAccessorDelegate SetValue;
        /// <summary>
        /// Property Name
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Property Type
        /// </summary>
        public readonly Type PropertyType;

        /// <summary>
        /// Faster Property info getter and setter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        FastPropertyInfo(PropertyInfo prop)
        {
            Name = prop.Name;
            GetValue = GetAccessorCache.GetOrAdd(prop, p => p.CanRead ? Factory.Accessors.BuildGetAccessor(p) : o => null);
            SetValue = SetAccessorCache.GetOrAdd(prop, p => p.CanWrite ? Factory.Accessors.BuildSetAccessor(p) : (a, b) => { });
            PropertyType = prop.PropertyType;
        }

        /// <summary>
        /// Get the fast property info from a reflection PropertyInfo
        /// </summary>
        /// <param name="prop">Property Info</param>
        /// <returns>Fast property info instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FastPropertyInfo Get(PropertyInfo prop)
            => Infos.GetOrAdd(prop, p => new FastPropertyInfo(p));
    }
}
