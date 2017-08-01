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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TWCore.Collections;

namespace TWCore.Data
{
    /// <summary>
    /// Data access connection base class
    /// </summary>
    public abstract class DataAccessBase : IDataAccess, IDataAccessAsync
    {
        /// <summary>
        /// Fires when an error occurs in the execution of a command.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> OnError;
        /// <summary>
        /// Cache timeout per command
        /// </summary>
        public TimeSpan CacheTimeout { get; set; }

        #region .ctor
        /// <summary>
        /// Data access connection base class
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected DataAccessBase()
        {
            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(CacheTimeout), CacheTimeout);
            });
        }
        #endregion

        #region Fire Event
        /// <summary>
        /// Fires the OnError event
        /// </summary>
        /// <param name="ex">Exception object as argument of the event</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void FireOnError(Exception ex)
            => OnError?.Invoke(this, new EventArgs<Exception>(ex));
        #endregion

        #region Protected Methods
        /// <summary>
        /// Gets the IDictionary parameters object from the properties of an object.
        /// </summary>
        /// <param name="parameters">Object with the parameters to the data source</param>
        /// <returns>IDictionary with the parameters</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual IDictionary<string, object> GetCommandParameters(object parameters)
        {
            var propertyInfos = parameters?.GetType().GetRuntimeProperties().Where(p => p.CanRead && !p.GetCustomAttributes<IgnoreParameterAttribute>().Any());
            if (propertyInfos != null)
            {
                var dctParameters = new Dictionary<string, object>();
                foreach (var prop in propertyInfos)
                {
                    var pKey = prop.Name;
                    var pValue = prop.GetValue(parameters);
                    dctParameters[pKey] = pValue;
                }
                return dctParameters;
            }
            return null;
        }
        #endregion

        #region Cache Methods
        static TimeoutDictionary<string, object> Caches = new TimeoutDictionary<string, object>();
        /// <summary>
        /// Gets the cache key for the name and input parameters
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>Cache key string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetCacheKey<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod)
        {
            StringBuilder sb = new StringBuilder(nameOrQuery + ";");
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> item in parameters)
                {
                    if (item.Value != null)
                        sb.Append($"{item.Key}:{item.Value};");
                    else
                        sb.Append($"{item.Key};");
                }
            }
            sb.Append($";{fillMethod?.GetType().FullName}");
            return sb.ToString();
        }
        /// <summary>
        /// Cache value
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        protected class CacheValue<T>
        {
            /// <summary>
            /// Response collection from a SelectElements method
            /// </summary>
            public IEnumerable<T> ResponseCollection;
            /// <summary>
            /// Response from a SelectElement
            /// </summary>
            public T Response;
            /// <summary>
            /// Return value object
            /// </summary>
            public object ReturnValue;

            public CacheValue(IEnumerable<T> responseCollection, object returnValue)
            {
                ResponseCollection = responseCollection;
                ReturnValue = returnValue;
            }
            public CacheValue(T response, object returnValue)
            {
                Response = response;
                ReturnValue = returnValue;
            }
        }
        #endregion

        //Sync Version

        #region SelectElements<T>
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> SelectElements<T>(string nameOrQuery, FillDataDelegate<T> fillMethod = null)
            => SelectElements(nameOrQuery, null, fillMethod);
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> SelectElements<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod = null)
            => SelectElements(nameOrQuery, GetCommandParameters(parameters), fillMethod);
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> SelectElements<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod = null)
            => SelectElements(nameOrQuery, parameters, fillMethod, out object returnValue);
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <param name="returnValue">Return value from the data source</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> SelectElements<T>(string nameOrQuery, FillDataDelegate<T> fillMethod, out object returnValue)
            => SelectElements(nameOrQuery, null, fillMethod, out returnValue);
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <param name="returnValue">Return value from the data source</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> SelectElements<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod, out object returnValue)
            => SelectElements(nameOrQuery, GetCommandParameters(parameters), fillMethod, out returnValue);
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <param name="returnValue">Return value from the data source</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> SelectElements<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod, out object returnValue)
        {
            returnValue = null;
            try
            {
                Core.Log.LibVerbose("Selecting elements from the data source using: {0}", nameOrQuery);
                if (CacheTimeout == TimeSpan.MinValue || CacheTimeout.TotalMilliseconds == 0)
                    return OnSelectElements(nameOrQuery, parameters, fillMethod, out returnValue);
                else
                {
                    var key = GetCacheKey(nameOrQuery, parameters, fillMethod);
                    if (Caches.TryGetValue(key, out object cacheValue))
                    {
                        Core.Log.LibVerbose("Elements found in the Cache", nameOrQuery);
                        var value = (CacheValue<T>)cacheValue;
                        returnValue = value.ReturnValue;
                        return value.ResponseCollection;
                    }
                    else
                    {
                        var col = OnSelectElements(nameOrQuery, parameters, fillMethod, out returnValue);
                        Caches.TryAdd(key, new CacheValue<T>(col, returnValue), CacheTimeout);
                        return col;
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                FireOnError(ex);
                throw;
            }
        }
        #endregion

        #region SelectElement
        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>Single entity from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T SelectElement<T>(string nameOrQuery, FillDataDelegate<T> fillMethod = null)
            => SelectElement(nameOrQuery, null, fillMethod);
        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>Single entity from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T SelectElement<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod = null)
            => SelectElement(nameOrQuery, GetCommandParameters(parameters), fillMethod);
        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>Single entity from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T SelectElement<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod = null)
            => SelectElement(nameOrQuery, parameters, fillMethod, out object returnValue);
        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <param name="returnValue">Return value from the data source</param>
        /// <returns>Single entity from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T SelectElement<T>(string nameOrQuery, FillDataDelegate<T> fillMethod, out object returnValue)
            => SelectElement(nameOrQuery, null, fillMethod, out returnValue);

        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <param name="returnValue">Return value from the data source</param>
        /// <returns>Single entity from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T SelectElement<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod, out object returnValue)
            => SelectElement(nameOrQuery, GetCommandParameters(parameters), fillMethod, out returnValue);

        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <param name="returnValue">Return value from the data source</param>
        /// <returns>Single entity from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T SelectElement<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod, out object returnValue)
        {
            returnValue = null;
            try
            {
                Core.Log.LibVerbose("Select an element from the data source using: {0}", nameOrQuery);
                if (CacheTimeout == TimeSpan.MinValue || CacheTimeout.TotalMilliseconds == 0)
                    return OnSelectElement(nameOrQuery, parameters, fillMethod, out returnValue);
                else
                {
                    var key = GetCacheKey(nameOrQuery, parameters, fillMethod);
                    if (Caches.TryGetValue(key, out object cacheValue))
                    {
                        Core.Log.LibVerbose("Elements found in the Cache", nameOrQuery);
                        var value = (CacheValue<T>)cacheValue;
                        returnValue = value.ReturnValue;
                        return value.Response;
                    }
                    else
                    {
                        var item = OnSelectElement(nameOrQuery, parameters, fillMethod, out returnValue);
                        Caches.TryAdd(key, new CacheValue<T>(item, returnValue), CacheTimeout);
                        return item;
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                FireOnError(ex);
                throw;
            }
        }
        #endregion

        #region ExecuteNonQuery
        /// <summary>
        /// Execute a command on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ExecuteNonQuery(string nameOrQuery, object parameters)
            => ExecuteNonQuery(nameOrQuery, GetCommandParameters(parameters));
        /// <summary>
        /// Execute a command on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ExecuteNonQuery(string nameOrQuery, IDictionary<string, object> parameters = null)
        {
            try
            {
                Core.Log.LibVerbose("Executing a non query sentence on the data source using: {0}", nameOrQuery);
                return OnExecuteNonQuery(nameOrQuery, parameters);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                FireOnError(ex);
                throw;
            }
        }
        #endregion

        #region ExecuteNonQuery with array
        /// <summary>
        /// Execute a command multiple times over an array of parameters on the data source and returns the number of rows.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parametersArray">Inputs parameters</param>
        /// <returns>Number of rows</returns>
        public void ExecuteNonQuery(string nameOrQuery, object[] parametersArray)
            => ExecuteNonQuery(nameOrQuery, parametersArray.Select(parameters => GetCommandParameters(parameters)));
        /// <summary>
        /// Execute a command multiple times over an array of parameters on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parametersIEnumerable">Input parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteNonQuery(string nameOrQuery, IEnumerable<IDictionary<string, object>> parametersIEnumerable = null)
        {
            try
            {
                Core.Log.LibVerbose("Executing a non query sentence with IEnumerable of parameters on the data source using: {0}", nameOrQuery);
                OnExecuteNonQuery(nameOrQuery, parametersIEnumerable);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                FireOnError(ex);
                throw;
            }
        }
        #endregion

        #region SelectScalar
        /// <summary>
        /// Select a single row field from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T SelectScalar<T>(string nameOrQuery, object parameters)
            => SelectScalar<T>(nameOrQuery, GetCommandParameters(parameters));
        /// <summary>
        /// Select a single row field from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T SelectScalar<T>(string nameOrQuery, IDictionary<string, object> parameters = null)
        {
            try
            {
                Core.Log.LibVerbose("Selecting an escalar value from the data source using: {0}", nameOrQuery);
                return OnSelectScalar<T>(nameOrQuery, parameters);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                FireOnError(ex);
                throw;
            }
        }
        #endregion

        #region SelectElements<T1, T2>
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>) SelectElements<T1, T2>(string nameOrQuery)
            => SelectElements<T1, T2>(nameOrQuery, null, null, null);
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>) SelectElements<T1, T2>(string nameOrQuery, FillDataDelegate<T1> fillMethod1)
            => SelectElements<T1, T2>(nameOrQuery, null, fillMethod1, null);
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>) SelectElements<T1, T2>(string nameOrQuery, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2)
            => SelectElements<T1, T2>(nameOrQuery, null, fillMethod1, fillMethod2);
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>) SelectElements<T1, T2>(string nameOrQuery, object parameters)
            => SelectElements<T1, T2>(nameOrQuery, GetCommandParameters(parameters), null, null);
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>) SelectElements<T1, T2>(string nameOrQuery, object parameters, FillDataDelegate<T1> fillMethod1)
            => SelectElements<T1, T2>(nameOrQuery, GetCommandParameters(parameters), fillMethod1, null);
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>) SelectElements<T1, T2>(string nameOrQuery, object parameters, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2)
            => SelectElements<T1, T2>(nameOrQuery, GetCommandParameters(parameters), fillMethod1, fillMethod2);
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>) SelectElements<T1, T2>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2)
            => SelectElements<T1, T2>(nameOrQuery, parameters, fillMethod1, fillMethod2, out object returnValue);
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <param name="returnValue">Return value from the data source</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>) SelectElements<T1, T2>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2, out object returnValue)
        {
            returnValue = null;
            try
            {
                Core.Log.LibVerbose("Selecting two resultsets elements from the data source using: {0}", nameOrQuery);
                if (CacheTimeout == TimeSpan.MinValue || CacheTimeout.TotalMilliseconds == 0)
                {
                    var rs1 = new ResultSet<T1>(fillMethod1);
                    var rs2 = new ResultSet<T2>(fillMethod2);
                    OnSelectElements(nameOrQuery, parameters, new IResultSet[] { rs1, rs2 }, out returnValue);
                    return (rs1.Result, rs2.Result);
                }
                else
                {
                    var key1 = GetCacheKey(nameOrQuery, parameters, fillMethod1);
                    var key2 = GetCacheKey(nameOrQuery, parameters, fillMethod2);
                    if (Caches.TryGetValue(key1, out object cacheValue1) && Caches.TryGetValue(key2, out object cacheValue2))
                    {
                        Core.Log.LibVerbose("Elements found in the Cache", nameOrQuery);
                        var value1 = (CacheValue<T1>)cacheValue1;
                        var value2 = (CacheValue<T2>)cacheValue2;
                        returnValue = value1.ReturnValue;
                        return (value1.ResponseCollection, value2.ResponseCollection);
                    }
                    else
                    {
                        var rs1 = new ResultSet<T1>(fillMethod1);
                        var rs2 = new ResultSet<T2>(fillMethod2);
                        OnSelectElements(nameOrQuery, parameters, new IResultSet[] { rs1, rs2 }, out returnValue);

                        Caches.TryAdd(key1, new CacheValue<T1>(rs1.Result, returnValue), CacheTimeout);
                        Caches.TryAdd(key2, new CacheValue<T2>(rs2.Result, returnValue), CacheTimeout);
                        return (rs1.Result, rs2.Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                FireOnError(ex);
                throw;
            }
        }
        #endregion

        #region SelectElements<T1, T2, T3>
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>) SelectElements<T1, T2, T3>(string nameOrQuery)
            => SelectElements<T1, T2, T3>(nameOrQuery, null, null, null, null);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>) SelectElements<T1, T2, T3>(string nameOrQuery, FillDataDelegate<T1> fillMethod1)
            => SelectElements<T1, T2, T3>(nameOrQuery, null, fillMethod1, null, null);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>) SelectElements<T1, T2, T3>(string nameOrQuery, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2)
            => SelectElements<T1, T2, T3>(nameOrQuery, null, fillMethod1, fillMethod2, null);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <param name="fillMethod3">Fill delegate for third entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>) SelectElements<T1, T2, T3>(string nameOrQuery, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2, FillDataDelegate<T3> fillMethod3)
            => SelectElements<T1, T2, T3>(nameOrQuery, null, fillMethod1, fillMethod2, fillMethod3);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>) SelectElements<T1, T2, T3>(string nameOrQuery, object parameters)
            => SelectElements<T1, T2, T3>(nameOrQuery, GetCommandParameters(parameters), null, null, null);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>) SelectElements<T1, T2, T3>(string nameOrQuery, object parameters, FillDataDelegate<T1> fillMethod1)
            => SelectElements<T1, T2, T3>(nameOrQuery, GetCommandParameters(parameters), fillMethod1, null, null);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>) SelectElements<T1, T2, T3>(string nameOrQuery, object parameters, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2)
            => SelectElements<T1, T2, T3>(nameOrQuery, GetCommandParameters(parameters), fillMethod1, fillMethod2, null);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <param name="fillMethod3">Fill delegate for third entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>) SelectElements<T1, T2, T3>(string nameOrQuery, object parameters, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2, FillDataDelegate<T3> fillMethod3)
            => SelectElements<T1, T2, T3>(nameOrQuery, GetCommandParameters(parameters), fillMethod1, fillMethod2, fillMethod3);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <param name="fillMethod3">Fill delegate for third entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>) SelectElements<T1, T2, T3>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2, FillDataDelegate<T3> fillMethod3)
            => SelectElements<T1, T2, T3>(nameOrQuery, parameters, fillMethod1, fillMethod2, fillMethod3, out object returnValue);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <param name="fillMethod3">Fill delegate for third entity</param>
        /// <param name="returnValue">Return value from the data source</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>) SelectElements<T1, T2, T3>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2, FillDataDelegate<T3> fillMethod3, out object returnValue)
        {
            returnValue = null;
            try
            {
                Core.Log.LibVerbose("Selecting two resultsets elements from the data source using: {0}", nameOrQuery);
                if (CacheTimeout == TimeSpan.MinValue || CacheTimeout.TotalMilliseconds == 0)
                {
                    var rs1 = new ResultSet<T1>(fillMethod1);
                    var rs2 = new ResultSet<T2>(fillMethod2);
                    var rs3 = new ResultSet<T3>(fillMethod3);
                    OnSelectElements(nameOrQuery, parameters, new IResultSet[] { rs1, rs2, rs3 }, out returnValue);
                    return (rs1.Result, rs2.Result, rs3.Result);
                }
                else
                {
                    var key1 = GetCacheKey(nameOrQuery, parameters, fillMethod1);
                    var key2 = GetCacheKey(nameOrQuery, parameters, fillMethod2);
                    var key3 = GetCacheKey(nameOrQuery, parameters, fillMethod3);
                    if (Caches.TryGetValue(key1, out object cacheValue1) && Caches.TryGetValue(key2, out object cacheValue2) && Caches.TryGetValue(key3, out object cacheValue3))
                    {
                        Core.Log.LibVerbose("Elements found in the Cache", nameOrQuery);
                        var value1 = (CacheValue<T1>)cacheValue1;
                        var value2 = (CacheValue<T2>)cacheValue2;
                        var value3 = (CacheValue<T3>)cacheValue3;
                        returnValue = value1.ReturnValue;
                        return (value1.ResponseCollection, value2.ResponseCollection, value3.ResponseCollection);
                    }
                    else
                    {
                        var rs1 = new ResultSet<T1>(fillMethod1);
                        var rs2 = new ResultSet<T2>(fillMethod2);
                        var rs3 = new ResultSet<T3>(fillMethod3);
                        OnSelectElements(nameOrQuery, parameters, new IResultSet[] { rs1, rs2, rs3 }, out returnValue);

                        Caches.TryAdd(key1, new CacheValue<T1>(rs1.Result, returnValue), CacheTimeout);
                        Caches.TryAdd(key2, new CacheValue<T2>(rs2.Result, returnValue), CacheTimeout);
                        return (rs1.Result, rs2.Result, rs3.Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                FireOnError(ex);
                throw;
            }
        }
        #endregion

        #region SelectResultSetsElements 
        /// <summary>
        /// Selects a all Result sets with a collection of elements from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="resultSets">Array of IResultSetItem instances to fill from the data source</param>
        /// <param name="returnValue">Return value from the data source</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SelectResultSetsElements(string nameOrQuery, IDictionary<string, object> parameters, IResultSet[] resultSets, out object returnValue)
            => OnSelectElements(nameOrQuery, parameters, resultSets, out returnValue);
        #endregion

        #region Abstracts Methods
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <param name="returnValue">Return value from the data source</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract IEnumerable<T> OnSelectElements<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod, out object returnValue);
        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <param name="returnValue">Return value from the data source</param>
        /// <returns>Single entity from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract T OnSelectElement<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod, out object returnValue);
        /// <summary>
        /// Execute a command on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract int OnExecuteNonQuery(string nameOrQuery, IDictionary<string, object> parameters = null);
        /// <summary>
        /// Execute a command multiple times over an array of parameters on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parametersIEnumerable">Inputs parameters</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void OnExecuteNonQuery(string nameOrQuery, IEnumerable<IDictionary<string, object>> parametersIEnumerable = null);
        /// <summary>
        /// Select a single row field from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract T OnSelectScalar<T>(string nameOrQuery, IDictionary<string, object> parameters = null);
        /// <summary>
        /// Selects a all Result sets with a collection of elements from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="resultSets">Array of IResultSetItem instances to fill from the data source</param>
        /// <param name="returnValue">Return value from the data source</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void OnSelectElements(string nameOrQuery, IDictionary<string, object> parameters, IResultSet[] resultSets, out object returnValue);
        #endregion


        //Async Version

        #region SelectElementsAsync<T>
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<IEnumerable<T>> SelectElementsAsync<T>(string nameOrQuery, FillDataDelegate<T> fillMethod = null)
            => SelectElementsAsync(nameOrQuery, null, fillMethod);
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<IEnumerable<T>> SelectElementsAsync<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod = null)
            => SelectElementsAsync(nameOrQuery, GetCommandParameters(parameters), fillMethod);
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<IEnumerable<T>> SelectElementsAsync<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod)
        {
            try
            {
                Core.Log.LibVerbose("Selecting elements from the data source using: {0}", nameOrQuery);
                if (CacheTimeout == TimeSpan.MinValue || CacheTimeout.TotalMilliseconds == 0)
                    return await OnSelectElementsAsync(nameOrQuery, parameters, fillMethod).ConfigureAwait(false);
                else
                {
                    var key = GetCacheKey(nameOrQuery, parameters, fillMethod);
                    if (Caches.TryGetValue(key, out object cacheValue))
                    {
                        Core.Log.LibVerbose("Elements found in the Cache", nameOrQuery);
                        var value = (CacheValue<T>)cacheValue;
                        return value.ResponseCollection;
                    }
                    else
                    {
                        var col = await OnSelectElementsAsync(nameOrQuery, parameters, fillMethod).ConfigureAwait(false);
                        Caches.TryAdd(key, new CacheValue<T>(col, null), CacheTimeout);
                        return col;
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                FireOnError(ex);
                throw;
            }
        }
        #endregion

        #region SelectElementAsync
        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>Single entity from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> SelectElementAsync<T>(string nameOrQuery, FillDataDelegate<T> fillMethod = null)
            => SelectElementAsync(nameOrQuery, null, fillMethod);
        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>Single entity from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> SelectElementAsync<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod = null)
            => SelectElementAsync(nameOrQuery, GetCommandParameters(parameters), fillMethod);
        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>Single entity from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> SelectElementAsync<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod)
        {
            try
            {
                Core.Log.LibVerbose("Select an element from the data source using: {0}", nameOrQuery);
                if (CacheTimeout == TimeSpan.MinValue || CacheTimeout.TotalMilliseconds == 0)
                    return await OnSelectElementAsync(nameOrQuery, parameters, fillMethod);
                else
                {
                    var key = GetCacheKey(nameOrQuery, parameters, fillMethod);
                    if (Caches.TryGetValue(key, out object cacheValue))
                    {
                        Core.Log.LibVerbose("Elements found in the Cache", nameOrQuery);
                        var value = (CacheValue<T>)cacheValue;
                        return value.Response;
                    }
                    else
                    {
                        var item = await OnSelectElementAsync(nameOrQuery, parameters, fillMethod).ConfigureAwait(false);
                        Caches.TryAdd(key, new CacheValue<T>(item, null), CacheTimeout);
                        return item;
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                FireOnError(ex);
                throw;
            }
        }
        #endregion

        #region ExecuteNonQueryAsync
        /// <summary>
        /// Execute a command on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> ExecuteNonQueryAsync(string nameOrQuery, object parameters)
            => ExecuteNonQueryAsync(nameOrQuery, GetCommandParameters(parameters));
        /// <summary>
        /// Execute a command on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<int> ExecuteNonQueryAsync(string nameOrQuery, IDictionary<string, object> parameters = null)
        {
            try
            {
                Core.Log.LibVerbose("Executing a non query sentence on the data source using: {0}", nameOrQuery);
                return await OnExecuteNonQueryAsync(nameOrQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                FireOnError(ex);
                throw;
            }
        }
        #endregion

        #region ExecuteNonQueryAsync with array
        /// <summary>
        /// Execute a command multiple times over an array of parameters on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parametersArray">Inputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task ExecuteNonQueryAsync(string nameOrQuery, object[] parametersArray)
            => ExecuteNonQueryAsync(nameOrQuery, parametersArray.Select(parameters => GetCommandParameters(parameters)));
        /// <summary>
        /// Execute a command multiple times over an array of parameters on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parametersIEnumerable">Input parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task ExecuteNonQueryAsync(string nameOrQuery, IEnumerable<IDictionary<string, object>> parametersIEnumerable = null)
        {
            try
            {
                Core.Log.LibVerbose("Executing a non query sentence with IEnumerable of parameters on the data source using: {0}", nameOrQuery);
                await OnExecuteNonQueryAsync(nameOrQuery, parametersIEnumerable).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                FireOnError(ex);
                throw;
            }
        }
        #endregion

        #region SelectScalarAsync
        /// <summary>
        /// Select a single row field from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> SelectScalarAsync<T>(string nameOrQuery, object parameters)
            => SelectScalarAsync<T>(nameOrQuery, GetCommandParameters(parameters));
        /// <summary>
        /// Select a single row field from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> SelectScalarAsync<T>(string nameOrQuery, IDictionary<string, object> parameters = null)
        {
            try
            {
                Core.Log.LibVerbose("Selecting an escalar value from the data source using: {0}", nameOrQuery);
                return await OnSelectScalarAsync<T>(nameOrQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                FireOnError(ex);
                throw;
            }
        }
        #endregion

        #region SelectElementsAsync<T1, T2>
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(IEnumerable<T1>, IEnumerable<T2>)> SelectElementsAsync<T1, T2>(string nameOrQuery)
            => SelectElementsAsync<T1, T2>(nameOrQuery, null, null, null);
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(IEnumerable<T1>, IEnumerable<T2>)> SelectElementsAsync<T1, T2>(string nameOrQuery, FillDataDelegate<T1> fillMethod1)
            => SelectElementsAsync<T1, T2>(nameOrQuery, null, fillMethod1, null);
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(IEnumerable<T1>, IEnumerable<T2>)> SelectElementsAsync<T1, T2>(string nameOrQuery, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2)
            => SelectElementsAsync<T1, T2>(nameOrQuery, null, fillMethod1, fillMethod2);
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(IEnumerable<T1>, IEnumerable<T2>)> SelectElementsAsync<T1, T2>(string nameOrQuery, object parameters)
            => SelectElementsAsync<T1, T2>(nameOrQuery, GetCommandParameters(parameters), null, null);
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(IEnumerable<T1>, IEnumerable<T2>)> SelectElementsAsync<T1, T2>(string nameOrQuery, object parameters, FillDataDelegate<T1> fillMethod1)
            => SelectElementsAsync<T1, T2>(nameOrQuery, GetCommandParameters(parameters), fillMethod1, null);
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(IEnumerable<T1>, IEnumerable<T2>)> SelectElementsAsync<T1, T2>(string nameOrQuery, object parameters, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2)
            => SelectElementsAsync<T1, T2>(nameOrQuery, GetCommandParameters(parameters), fillMethod1, fillMethod2);
        /// <summary>
        /// Selects a two result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<(IEnumerable<T1>, IEnumerable<T2>)> SelectElementsAsync<T1, T2>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2)
        {
            try
            {
                Core.Log.LibVerbose("Selecting two resultsets elements from the data source using: {0}", nameOrQuery);
                if (CacheTimeout == TimeSpan.MinValue || CacheTimeout.TotalMilliseconds == 0)
                {
                    var rs1 = new ResultSet<T1>(fillMethod1);
                    var rs2 = new ResultSet<T2>(fillMethod2);
                    await OnSelectElementsAsync(nameOrQuery, parameters, new IResultSet[] { rs1, rs2 }).ConfigureAwait(false);
                    return (rs1.Result, rs2.Result);
                }
                else
                {
                    var key1 = GetCacheKey(nameOrQuery, parameters, fillMethod1);
                    var key2 = GetCacheKey(nameOrQuery, parameters, fillMethod2);
                    if (Caches.TryGetValue(key1, out object cacheValue1) && Caches.TryGetValue(key2, out object cacheValue2))
                    {
                        Core.Log.LibVerbose("Elements found in the Cache", nameOrQuery);
                        var value1 = (CacheValue<T1>)cacheValue1;
                        var value2 = (CacheValue<T2>)cacheValue2;
                        return (value1.ResponseCollection, value2.ResponseCollection);
                    }
                    else
                    {
                        var rs1 = new ResultSet<T1>(fillMethod1);
                        var rs2 = new ResultSet<T2>(fillMethod2);
                        await OnSelectElementsAsync(nameOrQuery, parameters, new IResultSet[] { rs1, rs2 }).ConfigureAwait(false);

                        Caches.TryAdd(key1, new CacheValue<T1>(rs1.Result, null), CacheTimeout);
                        Caches.TryAdd(key2, new CacheValue<T2>(rs2.Result, null), CacheTimeout);
                        return (rs1.Result, rs2.Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                FireOnError(ex);
                throw;
            }
        }
        #endregion

        #region SelectElementsAsync<T1, T2, T3>
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> SelectElementsAsync<T1, T2, T3>(string nameOrQuery)
            => SelectElementsAsync<T1, T2, T3>(nameOrQuery, null, null, null, null);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> SelectElementsAsync<T1, T2, T3>(string nameOrQuery, FillDataDelegate<T1> fillMethod1)
            => SelectElementsAsync<T1, T2, T3>(nameOrQuery, null, fillMethod1, null, null);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> SelectElementsAsync<T1, T2, T3>(string nameOrQuery, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2)
            => SelectElementsAsync<T1, T2, T3>(nameOrQuery, null, fillMethod1, fillMethod2, null);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <param name="fillMethod3">Fill delegate for third entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> SelectElementsAsync<T1, T2, T3>(string nameOrQuery, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2, FillDataDelegate<T3> fillMethod3)
            => SelectElementsAsync<T1, T2, T3>(nameOrQuery, null, fillMethod1, fillMethod2, fillMethod3);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> SelectElementsAsync<T1, T2, T3>(string nameOrQuery, object parameters)
            => SelectElementsAsync<T1, T2, T3>(nameOrQuery, GetCommandParameters(parameters), null, null, null);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> SelectElementsAsync<T1, T2, T3>(string nameOrQuery, object parameters, FillDataDelegate<T1> fillMethod1)
            => SelectElementsAsync<T1, T2, T3>(nameOrQuery, GetCommandParameters(parameters), fillMethod1, null, null);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> SelectElementsAsync<T1, T2, T3>(string nameOrQuery, object parameters, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2)
            => SelectElementsAsync<T1, T2, T3>(nameOrQuery, GetCommandParameters(parameters), fillMethod1, fillMethod2, null);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <param name="fillMethod3">Fill delegate for third entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> SelectElementsAsync<T1, T2, T3>(string nameOrQuery, object parameters, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2, FillDataDelegate<T3> fillMethod3)
            => SelectElementsAsync<T1, T2, T3>(nameOrQuery, GetCommandParameters(parameters), fillMethod1, fillMethod2, fillMethod3);
        /// <summary>
        /// Selects a three result sets with a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T1">Type of the first entity to be selected</typeparam>
        /// <typeparam name="T2">Type of the second entity to be selected</typeparam>
        /// <typeparam name="T3">Type of the third entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod1">Fill delegate for first entity</param>
        /// <param name="fillMethod2">Fill delegate for second entity</param>
        /// <param name="fillMethod3">Fill delegate for third entity</param>
        /// <returns>A Tuple with IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> SelectElementsAsync<T1, T2, T3>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T1> fillMethod1, FillDataDelegate<T2> fillMethod2, FillDataDelegate<T3> fillMethod3)
        {
            try
            {
                Core.Log.LibVerbose("Selecting two resultsets elements from the data source using: {0}", nameOrQuery);
                if (CacheTimeout == TimeSpan.MinValue || CacheTimeout.TotalMilliseconds == 0)
                {
                    var rs1 = new ResultSet<T1>(fillMethod1);
                    var rs2 = new ResultSet<T2>(fillMethod2);
                    var rs3 = new ResultSet<T3>(fillMethod3);
                    await OnSelectElementsAsync(nameOrQuery, parameters, new IResultSet[] { rs1, rs2, rs3 }).ConfigureAwait(false);
                    return (rs1.Result, rs2.Result, rs3.Result);
                }
                else
                {
                    var key1 = GetCacheKey(nameOrQuery, parameters, fillMethod1);
                    var key2 = GetCacheKey(nameOrQuery, parameters, fillMethod2);
                    var key3 = GetCacheKey(nameOrQuery, parameters, fillMethod3);
                    if (Caches.TryGetValue(key1, out object cacheValue1) && Caches.TryGetValue(key2, out object cacheValue2) && Caches.TryGetValue(key3, out object cacheValue3))
                    {
                        Core.Log.LibVerbose("Elements found in the Cache", nameOrQuery);
                        var value1 = (CacheValue<T1>)cacheValue1;
                        var value2 = (CacheValue<T2>)cacheValue2;
                        var value3 = (CacheValue<T3>)cacheValue3;
                        return (value1.ResponseCollection, value2.ResponseCollection, value3.ResponseCollection);
                    }
                    else
                    {
                        var rs1 = new ResultSet<T1>(fillMethod1);
                        var rs2 = new ResultSet<T2>(fillMethod2);
                        var rs3 = new ResultSet<T3>(fillMethod3);
                        await OnSelectElementsAsync(nameOrQuery, parameters, new IResultSet[] { rs1, rs2, rs3 }).ConfigureAwait(false);

                        Caches.TryAdd(key1, new CacheValue<T1>(rs1.Result, null), CacheTimeout);
                        Caches.TryAdd(key2, new CacheValue<T2>(rs2.Result, null), CacheTimeout);
                        return (rs1.Result, rs2.Result, rs3.Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                FireOnError(ex);
                throw;
            }
        }
        #endregion

        #region SelectResultSetsElementsAsync
        /// <summary>
        /// Selects a all Result sets with a collection of elements from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="resultSets">Array of IResultSetItem instances to fill from the data source</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task SelectResultSetsElementsAsync(string nameOrQuery, IDictionary<string, object> parameters, IResultSet[] resultSets)
            => OnSelectElementsAsync(nameOrQuery, parameters, resultSets);
        #endregion

        #region Abstract Async Methods
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<IEnumerable<T>> OnSelectElementsAsync<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod);
        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>Single entity from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<T> OnSelectElementAsync<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod);
        /// <summary>
        /// Execute a command on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<int> OnExecuteNonQueryAsync(string nameOrQuery, IDictionary<string, object> parameters = null);
        /// <summary>
        /// Execute a command multiple times over an array of parameters on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parametersIEnumerable">Inputs parameters</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task OnExecuteNonQueryAsync(string nameOrQuery, IEnumerable<IDictionary<string, object>> parametersIEnumerable = null);
        /// <summary>
        /// Select a single row field from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task<T> OnSelectScalarAsync<T>(string nameOrQuery, IDictionary<string, object> parameters = null);
        /// <summary>
        /// Selects a all Result sets with a collection of elements from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="resultSets">Array of IResultSetItem instances to fill from the data source</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract Task OnSelectElementsAsync(string nameOrQuery, IDictionary<string, object> parameters, IResultSet[] resultSets);
        #endregion
    }
}
