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
            if (ColumnIndex.Count != rowValues.Length)
                throw new ArgumentException("The row values length is different from the expected.");
            if (typeof(T) == typeof(DictionaryObject))
            {
                var dicData = ColumnIndex.Select((c, rValues) => new KeyValuePair<string, object>(c.Key, rValues[c.Value]), rowValues).ToDictionary();
                return (T)(object)new DictionaryObject(dicData);
            }

            var entityInfo = EntityInfo<T>.Instance;
            var entityCreator = entityInfo.GetEntityCreator(pattern);

            var entity = (T)entityInfo.Activator();
            foreach (var (propName, prop) in entityInfo.GetColumnNamesByPattern(pattern))
            {
                if (!ColumnIndex.TryGetValue(propName, out var idx)) continue;
                if (idx >= rowValues.Length || idx < 0)
                {
                    Core.Log.Warning($"The value for the property: {propName} on the entity: {typeof(T).Name} could'nt be found on index: {idx}. Please check if there are duplicate column names in the query.");
                    continue;
                }
                var value = rowValues[idx];
                var valueType = value?.GetType();
                var propertyType = prop.PropertyUnderlayingType;
                var propertyTypeInfo = prop.PropertyUnderlayingTypeInfo;
                var defaultValue = prop.PropertyTypeInfo.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null;

                if (value is null)
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
                    else if (!InvalidCastList.Any((i, vTuple) => i.ValueType == vTuple.valueType && i.PropertyType == vTuple.propertyType, (valueType, propertyType)))
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
            return default;
        }

        #region Nested Classes
        /// <summary>
        /// Invalid cast item
        /// </summary>
        public class InvalidCast
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
        /// <param name="invalidCastList">Invalid cast list</param>
        /// <param name="valueConverter">Value converter</param>
        /// <returns>Entity instance</returns>
        public delegate T EntityCreatorDelegate<T>(object[] rowValues, Dictionary<string, int> columnIndex, ConcurrentBag<InvalidCast> invalidCastList, IEntityValueConverter valueConverter);

        /// <summary>
        /// Entity info
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        public class EntityInfo<T>
        {
            private static readonly ConcurrentDictionary<string, EntityCreatorDelegate<T>> CreatorDelegates = new ConcurrentDictionary<string, EntityCreatorDelegate<T>>();

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

            /// <summary>
            /// Gets the entity creator delegate
            /// </summary>
            /// <param name="pattern">Pattern of the entity creator</param>
            /// <returns>Entity creator delegate</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityCreatorDelegate<T> GetEntityCreator(string pattern)
                => CreatorDelegates.GetOrAdd(pattern ?? string.Empty, CreateEntityCreator);

            private EntityCreatorDelegate<T> CreateEntityCreator(string pattern)
            {
                var type = typeof(T);
                var properties = type.GetRuntimeProperties().Where(p => p.CanWrite);


                //***************************************************************************************************** Expression
                var serExpressions = new List<Expression>();
                var varExpressions = new List<ParameterExpression>();

                //In Parameters
                var rowValues = Expression.Parameter(typeof(object[]), "rowValues");
                var columnIndex = Expression.Parameter(typeof(Dictionary<string, int>), "columnIndex");
                var invalidCastList = Expression.Parameter(typeof(ConcurrentBag<InvalidCast>), "invalidCastList");
                var valueConverter = Expression.Parameter(typeof(IEntityValueConverter), "valueConverter");
                
                //Return value
                var returnTarget = Expression.Label(type, "ReturnTarget");
                var returnValue = Expression.Parameter(type, "returnValue");
                varExpressions.Add(returnValue);
                //***************************************************************************************************** 

                serExpressions.Add(Expression.Assign(returnValue, Expression.New(type)));
                var idxValue = Expression.Variable(typeof(int), "idx");
                varExpressions.Add(idxValue);
                foreach (var prop in properties)
                {
                    var propName = string.IsNullOrEmpty(pattern) ? prop.Name : pattern.Replace("%", prop.Name);
                    var tryGetValue = Expression.Call(columnIndex, "TryGetValue", null, Expression.Constant(propName), idxValue);
                    var idxIf = Expression.And(Expression.GreaterThanOrEqual(idxValue, Expression.Constant(0)), Expression.LessThan(idxValue,  Expression.Property(rowValues, "Length")));

                    //
                    var value = Expression.Variable(typeof(object), "value");
                    var valueType = Expression.Variable(typeof(Type), "valueType");

                    var propertyType = prop.PropertyType.GetUnderlyingType();

                    //
                    var valueConverterResult = Expression.Variable(typeof(object), "valueConverterResult");
                    var valueConverterExpression = Expression.IfThenElse(

                        Expression.And(
                            Expression.ReferenceNotEqual(valueConverter, Expression.Constant(null)),
                            Expression.Call(valueConverter, "Convert", null, value, valueType, Expression.Constant(prop.PropertyType), valueConverterResult)
                        ),
                        Expression.Assign(Expression.Property(returnValue, prop), Expression.Convert(valueConverterResult, prop.PropertyType)),
                        Expression.Empty()
                    );


                    //
                    Expression expType = Expression.Empty();
                    if (propertyType == typeof(Guid))
                    {
                        expType = Expression.IfThenElse(Expression.TypeEqual(valueType, typeof(string)),
                            Expression.Assign(Expression.Property(returnValue, prop), Expression.New(typeof(Guid).GetConstructor(new[] { typeof(string) }), Expression.Convert(value, typeof(string)))),
                            valueConverterExpression);
                    }


                    //
                    serExpressions.Add(
                        Expression.IfThen(Expression.And(tryGetValue, idxIf), Expression.Block(new[] { value, valueType },
                            
                                Expression.Assign(value, Expression.ArrayIndex(rowValues, idxValue)),
                                Expression.IfThenElse(Expression.ReferenceEqual(value, Expression.Constant(null)),
                                    Expression.Assign(Expression.Property(returnValue, prop), Expression.Default(prop.PropertyType)),
                                    Expression.Block(
                                        Expression.Assign(valueType, Expression.Call(value, "GetType", null)),
                                        Expression.IfThenElse(Expression.TypeEqual(valueType, propertyType),
                                            Expression.Assign(Expression.Property(returnValue, prop), Expression.Convert(value, prop.PropertyType)),
                                            expType
                                        )
                                    )
                                )

                            )
                        )
                    );
                }

                //***************************************************************************************************** 
                serExpressions.Add(Expression.Return(returnTarget, returnValue, type));
                serExpressions.Add(Expression.Label(returnTarget, returnValue));

                var block = Expression.Block(varExpressions, serExpressions).Reduce();
                var lambda = Expression.Lambda<EntityCreatorDelegate<T>>(block, type.Name + "_EntityBinder", new[] { rowValues, columnIndex, invalidCastList, valueConverter });
                return lambda.Compile();
            }
        }
        #endregion
    }
}
