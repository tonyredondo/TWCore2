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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TWCore.Reflection;
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Data
{
    /// <summary>
    /// Manages the binding from a object array to a entity object.
    /// </summary>
    public class EntityBinder
    {
        #region Statics
        /// <summary>
        /// Properties of entities type
        /// </summary>
        public static ConcurrentDictionary<Type, EntityInfo> Entities { get; } = new ConcurrentDictionary<Type, EntityInfo>();
        /// <summary>
        /// Types overwrite definition
        /// </summary>
        public static ConcurrentDictionary<Type, Type> TypesOverwrite { get; } = new ConcurrentDictionary<Type, Type>();
        /// <summary>
        /// Prepare Type for entity binder
        /// </summary>
        /// <param name="type">Type of entity</param>
        /// <returns>EntityInfo instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityInfo PrepareEntity(Type type) => Entities.GetOrAdd(type, EntityInfo.CreateEntityInfo);
        #endregion

        #region Property
        /// <summary>
        /// Contains the binding from properties names to the object index on the data source
        /// </summary>
        public Dictionary<string, int> ColumnIndex { get; set; } = new Dictionary<string, int>();
        /// <summary>
        /// Concurrent list with all casting errors
        /// </summary>
        private static ConcurrentBag<InvalidCast> InvalidCastList { get; } = new ConcurrentBag<InvalidCast>();
        /// <summary>
        /// Value converter from the data source to the entity properties.
        /// </summary>
        public IEntityValueConverter ValueConverter { get; set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Manages the binding from a object array to a entity object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityBinder() { }
        /// <summary>
        /// Manages the binding from a object array to a entity object.
        /// </summary>
        /// <param name="valueConverter">Entity value converter</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityBinder(IEntityValueConverter valueConverter)
        {
            ValueConverter = valueConverter;
        }
        #endregion

        /// <summary>
        /// Bind an object values array to an object entity
        /// </summary>
        /// <typeparam name="T">Type of object entity</typeparam>
        /// <param name="rowValues">Object values array for a single row</param>
        /// <param name="pattern">Pattern to do the binding</param>
        /// <returns>Object entity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Bind<T>(object[] rowValues, string pattern = null)
        {
            var type = typeof(T);
            if (TypesOverwrite.TryGetValue(type, out var rType))
                type = rType;

            if (ColumnIndex.Count != rowValues.Length)
                throw new ArgumentException("The row values length is different from the expected.");

            if (type == typeof(DictionaryObject))
            {
                var dicData = ColumnIndex.Select(c => new KeyValuePair<string, object>(c.Key, rowValues[c.Value])).ToDictionary();
                return (T)(object)new DictionaryObject(dicData);
            }
            
            var entityInfo = PrepareEntity(type);
            var entity = (T)entityInfo.Activator();
            foreach (var prop in entityInfo.Properties)
            {
                var propName = prop.Name;
                if (pattern.IsNotNullOrEmpty())
                    propName = pattern.Replace("%", propName);

                if (!ColumnIndex.ContainsKey(propName)) continue;
                    
                var idx = ColumnIndex[propName];
                if (idx >= rowValues.Length || idx < 0)
                {
                    Core.Log.Warning($"The value for the property: {propName} on the entity: {type.Name} could'nt be found on index: {idx}. Please check if there are duplicate column names in the query.");
                    continue;
                }
                var value = rowValues[idx];
                var valueType = value?.GetType();
                var propertyType = prop.PropertyUnderlayingType;
                var propertyTypeInfo = prop.PropertyUnderlayingTypeInfo;
                var defaultValue = prop.PropertyTypeInfo.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null;

                if (value == null)
                    prop.SetValue(entity, defaultValue);
                else if (propertyType == valueType)
                    prop.SetValue(entity, value);
                else
                {
                    var result = defaultValue;

                    if (propertyType == typeof(Guid) && valueType == typeof(string))
                        result = new Guid((string)value);
                    else if (propertyTypeInfo.IsEnum &&
                             (valueType == typeof(int) || valueType == typeof(long) || valueType == typeof(string) || valueType == typeof(byte) || valueType == typeof(short)))
                        result = Enum.Parse(propertyType, value.ToString());
                    else if (ValueConverter != null && ValueConverter.Convert(value, valueType, prop.PropertyType, out var valueConverterResult))
                        result = valueConverterResult;
                    else if (!InvalidCastList.Any(i => i.ValueType == valueType && i.PropertyType == propertyType))
                    {
                        try
                        {
                            result = Convert.ChangeType(value, propertyType);
                        }
                        catch (InvalidCastException exCast)
                        {
                            Core.Log.Write(exCast);
                            InvalidCastList.Add(new InvalidCast(valueType, propertyType));
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                    }

                    prop.SetValue(entity, result);
                }
            }
            return entity;
        }

        /// <summary>
        /// Gets the column value from a row
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="rowValues">Object values array for a single row</param>
        /// <param name="columnName">Column name to get the value</param>
        /// <returns>Column value from the row</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue<T>(object[] rowValues, string columnName)
        {
            if (ColumnIndex.TryGetValue(columnName, out var index) && rowValues.Length > index)
                return (T)rowValues[index];
            return default(T);
        }

        #region Nested Classes
        /// <summary>
        /// Invalid cast item
        /// </summary>
        private class InvalidCast
        {
            public readonly Type ValueType;
            public readonly Type PropertyType;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public InvalidCast(Type valueType, Type propertyType)
            {
                ValueType = valueType;
                PropertyType = propertyType;
            }
        }

        /// <summary>
        /// Entity info
        /// </summary>
        public class EntityInfo
        {
            /// <summary>
            /// Entity Activator
            /// </summary>
            public readonly ActivatorDelegate Activator;
            /// <summary>
            /// Fast Property information
            /// </summary>
            public readonly FastPropertyInfo[] Properties;

            /// <summary>
            /// Entity info
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private EntityInfo(Type type)
            {
                Activator = Factory.Accessors.CreateActivator(type.GetTypeInfo().DeclaredConstructors.First());
                Properties = type.GetRuntimeProperties().Where(p => p.CanWrite).Select(p => p.GetFastPropertyInfo()).ToArray();
            }

            /// <summary>
            /// Create a new entity info
            /// </summary>
            /// <param name="type">Type of entity</param>
            /// <returns>EntityInfo</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static EntityInfo CreateEntityInfo(Type type) => new EntityInfo(type);
        }
        #endregion
    }
}
