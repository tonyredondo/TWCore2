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
    /// Faster Property info getter and setter
    /// </summary>
    public class FastPropertyInfo
    {
        private static readonly NonBlocking.ConcurrentDictionary<PropertyInfo, FastPropertyInfo> Infos = new NonBlocking.ConcurrentDictionary<PropertyInfo, FastPropertyInfo>();
        private static readonly NonBlocking.ConcurrentDictionary<PropertyInfo, GetAccessorDelegate> GetAccessorCache = new NonBlocking.ConcurrentDictionary<PropertyInfo, GetAccessorDelegate>();
        private static readonly NonBlocking.ConcurrentDictionary<PropertyInfo, SetAccessorDelegate> SetAccessorCache = new NonBlocking.ConcurrentDictionary<PropertyInfo, SetAccessorDelegate>();

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
		/// Property Type Info
		/// </summary>
		public readonly TypeInfo PropertyTypeInfo;
		/// <summary>
		/// Property Underlaying Type
		/// </summary>
		public readonly Type PropertyUnderlayingType;
		/// <summary>
		/// Property Underlaying TypeInfo
		/// </summary>
		public readonly Type PropertyUnderlayingTypeInfo;

        /// <summary>
        /// Faster Property info getter and setter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private FastPropertyInfo(PropertyInfo prop)
        {
            Name = prop.Name;
			GetValue = GetAccessorCache.GetOrAdd(prop, GetDelegate);
			SetValue = SetAccessorCache.GetOrAdd(prop, SetDelegate);
            PropertyType = prop.PropertyType;
			PropertyTypeInfo = PropertyType.GetTypeInfo();
			PropertyUnderlayingType = PropertyType.GetUnderlyingType();
			PropertyUnderlayingTypeInfo = PropertyUnderlayingType.GetTypeInfo();
        }

        /// <summary>
        /// Get the fast property info from a reflection PropertyInfo
        /// </summary>
        /// <param name="prop">Property Info</param>
        /// <returns>Fast property info instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FastPropertyInfo Get(PropertyInfo prop)
            => Infos.GetOrAdd(prop, p => new FastPropertyInfo(p));

        private static GetAccessorDelegate GetDelegate(PropertyInfo prop)
			=> prop.CanRead ? Factory.Accessors.BuildGetAccessor(prop) : EmptyGet;

        private static SetAccessorDelegate SetDelegate(PropertyInfo prop)
			=> prop.CanWrite ? Factory.Accessors.BuildSetAccessor(prop) : EmptySet;

        private static object EmptyGet(object value) => null;
        private static void EmptySet(object value, object arg) { }
    }
}
