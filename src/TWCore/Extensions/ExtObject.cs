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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TWCore.Reflection;
// ReSharper disable CheckNamespace

namespace TWCore
{
    /// <summary>
    /// Extensions for the Object class
    /// </summary>
    public static partial class Extensions
    {
        private static readonly Dictionary<string, GetAccessorDelegate> GetterCache = new Dictionary<string, GetAccessorDelegate>();
        private static readonly Dictionary<string, SetAccessorDelegate> SetterCache = new Dictionary<string, SetAccessorDelegate>();

        /// <summary>
        /// Object Deep copy/clone using serialization
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="source">Source object</param>
        /// <returns>Cloned object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DeepClone<T>(this T source)
        {
            if (source == null) return default(T);
            if (source.GetType().IsValueType) return default(T);
            return (T)ObjectCloner.Clone(source);
        }
        
        /// <summary>
        /// Create a new Object and copy fields and properties (by reflection)
        /// </summary>
        /// <typeparam name="T">New Object type</typeparam>
        /// <param name="source">Source object</param>
        /// <returns>New Object with properties and fields set</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CreateObjectOfAndCopyDataFrom<T>(this object source)
        {
            var value = Activator.CreateInstance(typeof(T));
            value.CopyValuesFrom(source);
            return (T)value;
        }

        #region Get and Set Methods
        /// <summary>
        /// Get object property or field Type
        /// </summary>
        /// <param name="source">Object source</param>
        /// <param name="name">Property or field name</param>
        /// <returns>Property or field value</returns>
        public static Type GetMemberObjectType(this object source, string name)
        {
            if (source == null) return null;
            var sourceType = source.GetType();
            if (source is IDictionary sourceDictio)
                return sourceDictio.Contains(name) ? sourceDictio[name]?.GetType() : null;

            var prop = sourceType.GetRuntimeProperty(name);
            if (prop != null && prop.CanRead)
                return prop.PropertyType;

            var field = sourceType.GetRuntimeField(name);
            return field?.FieldType;
        }
        /// <summary>
        /// Get object property or field value
        /// </summary>
        /// <param name="source">Object source</param>
        /// <param name="name">Property or field name</param>
        /// <returns>Property or field value</returns>
        public static object GetMemberObjectValue(this object source, string name)
        {
            if (source == null) return null;
            var sourceType = source.GetType();
            GetAccessorDelegate getter = null;
            var key = string.Format("{0}.{1}", sourceType.AssemblyQualifiedName, name);

            if (GetterCache.ContainsKey(key))
                getter = GetterCache[key];
            else
            {
                var sDictionary = source as IDictionary;
                if (sDictionary != null)
                    return sDictionary.Contains(name) ? sDictionary[name] : null;

                var prop = sourceType.GetRuntimeProperty(name);
                if (prop != null && prop.CanRead)
                    getter = Factory.Accessors.BuildGetAccessor(prop);

                if (getter == null)
                {
                    var field = sourceType.GetRuntimeField(name);
                    if (field != null)
                        getter = obj => field.GetValue(obj);
                }

                GetterCache[key] = getter;
            }
            return getter?.Invoke(source);
        }

        /// <summary>
        /// Get object property or field value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="source">Object source</param>
        /// <param name="name">Property or field name</param>
        /// <returns>Property or field value</returns>
        public static T GetMemberValue<T>(this object source, string name)
        {
            var objRes = GetMemberObjectValue(source, name);
            if (objRes == (object) default(T) || objRes == null) return default(T);
            try
            {
                return (T)Convert.ChangeType(objRes, typeof(T));
            }
            catch
            {
                return (T)objRes;
            }
        }
        /// <summary>
        /// Set object property or field value
        /// </summary>
        /// <param name="source">Object source</param>
        /// <param name="name">Property or field name</param>
        /// <param name="value">Property or field value</param>
        public static void SetMemberObjectValue(this object source, string name, object value)
        {
            if (source == null) return;
            var sourceType = source.GetType();
            var sDictio = source as IDictionary;
            if (sDictio != null)
            {
                sDictio[name] = value;
                return;
            }

            SetAccessorDelegate setter = null;
            var key = string.Format("{0}.{1}", sourceType.AssemblyQualifiedName, name);

            if (SetterCache.ContainsKey(key))
                setter = SetterCache[key];
            else
            {
                var prop = sourceType.GetRuntimeProperty(name);
                if (prop != null && prop.CanWrite)
                    setter = Factory.Accessors.BuildSetAccessor(prop);
                if (setter == null)
                {
                    var field = sourceType.GetRuntimeField(name);
                    if (field != null)
                        setter = (s, v) => field.SetValue(s, v);
                }
                SetterCache[key] = setter;
            }
            setter?.Invoke(source, value);
        }
        #endregion

        #region Copy Methods
        /// <summary>
        /// Copy Properties and fields values from source object to a target object
        /// </summary>
        /// <typeparam name="T">Type of target object</typeparam>
        /// <param name="target">Target object</param>
        /// <param name="source">Source object</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyValuesFrom<T>(this T target, object source)
        {
            if (source?.GetType().GetInterfaces().Any(i => (i == typeof(IList) || (i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))) == true)
            {
                var iTarget = target as IList;
                var iSource = (IList)source;
                if (iTarget == null) return;
                Type innerType = null;
                if (typeof(T).IsArray)
                    innerType = typeof(T).GetElementType();
                else
                {
                    var gargs = iTarget.GetType().GenericTypeArguments;
                    if (gargs.Length == 0)
                        gargs = typeof(T).GenericTypeArguments;
                    if (gargs.Length > 0)
                        innerType = gargs[0];
                    else
                    {
                        var iListType = iTarget.GetType().GetTypeInfo().ImplementedInterfaces.FirstOrDefault(m => (m.GetTypeInfo().IsGenericType && m.GetGenericTypeDefinition() == typeof(IList<>)));
                        if (iListType?.GenericTypeArguments.Any() == true)
                            innerType = iListType.GenericTypeArguments[0];
                    }
                }
                foreach (var item in iSource)
                {
                    var nItem = Activator.CreateInstance(item?.GetType() ?? innerType);
                    nItem.CopyValuesFrom(item);
                    iTarget.Add(nItem);
                }
            }
            else if (source != null)
            {
                var sourceType = source.GetType();
                var targetType = target.GetType();


                var sourceProperties = sourceType.GetRuntimeProperties().Where(p => !p.IsSpecialName && p.CanRead);
                foreach (var sProp in sourceProperties)
                {
                    var sFastProp = sProp.GetFastPropertyInfo();
                    var tProp = targetType.GetRuntimeProperty(sProp.Name);
                    if (tProp == null || tProp.IsSpecialName || !tProp.CanWrite) continue;
                    var tFastProp = tProp.GetFastPropertyInfo();
                    var sPropValue = sFastProp.GetValue(source);
                    var sPropTypeInfo = sFastProp.PropertyType.GetTypeInfo();
                    var tPropTypeInfo = tFastProp.PropertyType.GetTypeInfo();

                    if (tPropTypeInfo.IsEnum)
                        tFastProp.SetValue(target, Enum.ToObject(tFastProp.PropertyType, sPropValue));
                    else if (sPropTypeInfo.IsValueType || sFastProp.PropertyType == typeof(string) || sPropValue is string)
                        tFastProp.SetValue(target, sPropValue);
                    else
                    {
                        if (sPropValue == null)
                            tFastProp.SetValue(target, null);
                        else
                        {
                            var tPropValue = Activator.CreateInstance(tFastProp.PropertyType);
                            tPropValue.CopyValuesFrom(sPropValue);
                            tFastProp.SetValue(target, tPropValue);
                        }
                    }
                }

                var sourceFields = sourceType.GetRuntimeFields();
                foreach (var sField in sourceFields)
                {
                    var tField = targetType.GetRuntimeField(sField.Name);
                    if (tField == null) continue;
                    var sFieldValue = sField.GetValue(source);
                    var sFieldTypeInfo = sField.FieldType.GetTypeInfo();
                    var tFieldTypeInfo = tField.FieldType.GetTypeInfo();

                    if (tFieldTypeInfo.IsEnum)
                        tField.SetValue(target, Enum.ToObject(tField.FieldType, sFieldValue));
                    else if (sFieldTypeInfo.IsValueType || sField.FieldType == typeof(string) || sFieldValue is string)
                        tField.SetValue(target, sFieldValue);
                    else
                    {
                        if (sFieldValue == null)
                            tField.SetValue(target, null);
                        else
                        {
                            var tFieldValue = Activator.CreateInstance(sField.FieldType);
                            tFieldValue.CopyValuesFrom(sFieldValue);
                            tField.SetValue(target, tFieldValue);
                        }
                    }
                }
            }
        }
        #endregion

        #region Copy From/To Dictionary
        /// <summary>
        /// Get a dictionary with all properties values
        /// </summary>
        /// <param name="source">Object source</param>
        /// <returns>Dictionary containing all property values</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<string, object> GetDictionaryValues(this object source)
            => source?.GetType().GetRuntimeProperties().Where(p => p.CanRead).ToDictionary(k => k.Name, v => v.GetValue(source));
        /// <summary>
        /// Set a dictionary values to the object properties
        /// </summary>
        /// <param name="destination">Object destination</param>
        /// <param name="source">Dictionary with all property values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetDictionaryValues(this object destination, Dictionary<string, object> source)
        {
            if (source == null || destination == null) return;
            foreach (var item in source)
            {
                var prop = destination.GetType().GetRuntimeProperty(item.Key);
                if (prop.CanWrite)
                    prop.SetValue(destination, item.Value);
            }
        }
        #endregion
    }
}
