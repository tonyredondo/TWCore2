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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using TWCore.Collections;
using TWCore.Reflection;
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Data
{
    /// <summary>
    /// Manages the binding from a object array to a entity object.
    /// </summary>
    public class EntityBinder
    {
        private static volatile int HasInvalidCast;

        #region Statics
        /// <summary>
        /// Prepare Type for entity binder
        /// </summary>
        /// <returns>EntityInfo instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityInfo<T> PrepareEntity<T>() => EntityInfo<T>.Instance;
        #endregion

        #region Property
        /// <summary>
        /// Contains the binding from properties names to the object index on the data source
        /// </summary>
        public Dictionary<string, int> ColumnIndex { get; set; } = new Dictionary<string, int>();
        /// <summary>
        /// Concurrent list with all casting errors
        /// </summary>
        private static ConcurrentDictionary<(Type, Type), InvalidCast> InvalidCastList { get; } = new ConcurrentDictionary<(Type, Type), InvalidCast>();
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
            var columnIndex = ColumnIndex;
            if (columnIndex.Count != rowValues.Length)
                throw new ArgumentException("The row values length is different from the expected.");
            if (typeof(T) == typeof(DictionaryObject))
            {
                var dicData = columnIndex.Select((c, rValues) => new KeyValuePair<string, object>(c.Key, rValues[c.Value]), rowValues).ToDictionary();
                return (T)(object)new DictionaryObject(dicData);
            }
            var valueConverter = ValueConverter;
            var entityInfo = EntityInfo<T>.Instance;
            var entity = (T)entityInfo.Activator();
            foreach (var dataPattern in entityInfo.GetColumnNamesByPattern(pattern))
            {
                if (!columnIndex.TryGetValue(dataPattern.Name, out var idx)) continue;
                if (idx >= rowValues.Length || idx < 0)
                {
                    Core.Log.Warning($"The value for the property: {dataPattern.Name} on the entity: {typeof(T).Name} could'nt be found on index: {idx}. Please check if there are duplicate column names in the query.");
                    continue;
                }
                var value = rowValues[idx];
                if (value is null)
                {
                    dataPattern.Property.SetValue(entity, dataPattern.DefaultValue);
                    continue;
                }

                var valueType = value?.GetType();
                var propertyType = dataPattern.Property.PropertyUnderlayingType;

                if (propertyType == valueType)
                    dataPattern.Property.SetValue(entity, value);
                else
                {
                    var result = dataPattern.DefaultValue;

                    if (dataPattern.IsGuid && valueType == typeof(string))
                        result = new Guid((string)value);
                    else if (dataPattern.IsEnum && (valueType == typeof(int) || valueType == typeof(long) || valueType == typeof(string) || valueType == typeof(byte) || valueType == typeof(short)))
                        result = Enum.Parse(propertyType, value.ToString());
                    else if (valueConverter != null && valueConverter.Convert(value, valueType, dataPattern.Property.PropertyType, dataPattern.DefaultValue, out var valueConverterResult))
                        result = valueConverterResult;
                    else if (HasInvalidCast == 0 || !InvalidCastList.TryGetValue((valueType, propertyType), out _))
                    {
                        try
                        {
                            result = Convert.ChangeType(value, propertyType);
                        }
                        catch (InvalidCastException exCast)
                        {
                            Core.Log.Write(exCast);
                            Interlocked.Increment(ref HasInvalidCast);
                            InvalidCastList.TryAdd((valueType, propertyType), new InvalidCast(valueType, propertyType));
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                    }

                    dataPattern.Property.SetValue(entity, result);
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
            return default;
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
        /// Entity Creator delegate
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="rowValues">Row values</param>
        /// <param name="columnIndex">Column index names</param>
        /// <param name="valueConverter">Value converter</param>
        /// <returns>Entity instance</returns>
        public delegate T EntityCreatorDelegate<T>(object[] rowValues, Dictionary<string, int> columnIndex, IEntityValueConverter valueConverter);

        /// <summary>
        /// Entity info
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        public class EntityInfo<T>
        {
            private static readonly ConcurrentDictionary<string, DataPerPattern[]> EntityInfoPropertyPatterns = new ConcurrentDictionary<string, DataPerPattern[]>();
            
            /// <summary>
            /// Singleton instance
            /// </summary>
            public static readonly EntityInfo<T> Instance = new EntityInfo<T>();


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
            private EntityInfo()
            {
                var type = typeof(T);
                Activator = Factory.Accessors.CreateActivator(type);
                Properties = type.GetRuntimeProperties().Where(p => p.CanWrite).Select(p => p.GetFastPropertyInfo()).ToArray();
            }

            /// <summary>
            /// Gets the columns names list using a pattern 
            /// </summary>
            /// <param name="pattern">String pattern</param>
            /// <returns>Data struct</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal DataPerPattern[] GetColumnNamesByPattern(string pattern)
            {
                return EntityInfoPropertyPatterns.GetOrAdd(pattern ?? string.Empty, mPattern =>
                {
                    return Properties.Select(prop =>
                    {
                        var name = mPattern == string.Empty ? prop.Name : mPattern.Replace("%", prop.Name);
                        var defaultValue = prop.PropertyType.IsValueType ? System.Activator.CreateInstance(prop.PropertyType) : null;
                        var isGuid = prop.PropertyUnderlayingType == typeof(Guid);
                        var isEnum = prop.PropertyUnderlayingType.IsEnum;

                        return new DataPerPattern(name, prop, defaultValue, isGuid, isEnum);
                    }).ToArray();
                });
            }

            internal readonly struct DataPerPattern
            {
                public readonly string Name;
                public readonly FastPropertyInfo Property;
                public readonly object DefaultValue;
                public readonly bool IsGuid;
                public readonly bool IsEnum;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal DataPerPattern(string name, FastPropertyInfo property, object defaultValue, bool isGuid, bool isEnum)
                {
                    Name = name;
                    Property = property;
                    DefaultValue = defaultValue;
                    IsGuid = isGuid;
                    IsEnum = isEnum;
                }
            }
        }
        #endregion
    }
}
