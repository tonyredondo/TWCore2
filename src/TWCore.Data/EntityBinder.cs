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
            foreach (var (propName, prop) in entityInfo.GetColumnNamesByPattern(pattern))
            {
                if (!columnIndex.TryGetValue(propName, out var idx)) continue;
                if (idx >= rowValues.Length || idx < 0)
                {
                    Core.Log.Warning($"The value for the property: {propName} on the entity: {typeof(T).Name} could'nt be found on index: {idx}. Please check if there are duplicate column names in the query.");
                    continue;
                }
                var defaultValue = prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null;
                var value = rowValues[idx];
                if (value is null)
                {
                    prop.SetValue(entity, defaultValue);
                    continue;
                }

                var valueType = value?.GetType();
                var propertyType = prop.PropertyUnderlayingType;

                if (propertyType == valueType)
                    prop.SetValue(entity, value);
                else
                {
                    var result = defaultValue;

                    if (propertyType == typeof(Guid) && valueType == typeof(string))
                        result = new Guid((string)value);
                    else if (propertyType.IsEnum && (valueType == typeof(int) || valueType == typeof(long) || valueType == typeof(string) || valueType == typeof(byte) || valueType == typeof(short)))
                        result = Enum.Parse(propertyType, value.ToString());
                    else if (valueConverter != null && valueConverter.Convert(value, valueType, prop.PropertyType, out var valueConverterResult))
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
            private static readonly ConcurrentDictionary<string, (string, FastPropertyInfo)[]> EntityInfoPropertyPatterns = new ConcurrentDictionary<string, (string, FastPropertyInfo)[]>();
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
            /// <param name="pattern"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public (string, FastPropertyInfo)[] GetColumnNamesByPattern(string pattern)
            {
                return EntityInfoPropertyPatterns.GetOrAdd(pattern ?? string.Empty, mPattern =>
                {
                    if (mPattern == string.Empty)
                        return Properties.Select(p => (p.Name, p)).ToArray();
                    return Properties.Select(p => (pattern.Replace("%", p.Name), p)).ToArray();
                });
            }
        }
        #endregion
    }
}
