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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Data.Schema;
using TWCore.Data.Schema.Generator;
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable ParameterTypeCanBeEnumerable.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global

namespace TWCore.Data
{
    /// <summary>
    /// Data access connection base class
    /// </summary>
    // ReSharper disable once InheritdocConsiderUsage
    public abstract class DataAccessBase : IDataAccess, IDataAccessAsync, IDataAccessDynamicGenerator
    {
        private static readonly TimeoutDictionary<string, Dictionary<string, int>> ColumnsByNameOrQuery = new TimeoutDictionary<string, Dictionary<string, int>>();
        private static readonly DataAccessSettings Settings = Core.GetSettings<DataAccessSettings>();
        private string _connectionString;
        private string _counterCategory;

        #region Properties
        /// <summary>
        /// Data access command type
        /// </summary>
        public DataAccessType AccessType { get; protected set; }
        /// <summary>
        /// Parameters Binder
        /// </summary>
        public IParametersBinder ParametersBinder { get; set; } = new DefaultParametersBinder();
        /// <summary>
        /// Gets or Sets the value converter for the entity binder
        /// </summary>
        public IEntityValueConverter EntityValueConverter { get; set; } = new DefaultEntityValueConverter();
        /// <summary>
        /// Return parameter parameter name
        /// </summary>
        public string ReturnParameterName { get; set; } = "@ReturnValue";
        /// <summary>
        /// Command timeout
        /// </summary>
        public int CommandTimeout { get; set; } = Settings.CommandTimeout;
        /// <summary>;
        /// Data access connection string
        /// </summary>
        public string ConnectionString
        {
            get => _connectionString;
            set
            {
                _connectionString = Core.ReplaceEncriptionTemplate(value);
            }
        }
        /// <summary>
        /// String to append before every parameter
        /// </summary>
        public string ParametersPrefix { get; } = "@";
        /// <summary>
        /// Cache for columns by name or query in seconds
        /// </summary>
        public int ColumnsByNameOrQueryCacheInSec { get; set; } = Settings.ColumnsByNameOrQueryCacheInSec;
        #endregion

        #region Events
        /// <summary>
        /// Fires when an error occurs in the execution of a command.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> OnError;
        #endregion

        #region .ctor
        /// <summary>
        /// Data access connection base class
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected DataAccessBase()
        {
            _counterCategory = GetType().Name + " DataAccess";
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Gets the database connection object
        /// </summary>
        /// <returns>A DbConnection object</returns>
        protected abstract DbConnection GetConnection();
        /// <summary>
        /// Gets the database command object
        /// </summary>
        /// <returns>A DbCommand object</returns>
        protected abstract DbCommand GetCommand();
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
            if (propertyInfos is null) return null;
            var dctParameters = new Dictionary<string, object>();
            foreach (var prop in propertyInfos)
            {
                var pKey = prop.Name;
                var pValue = prop.GetValue(parameters);
                dctParameters[pKey] = pValue;
            }
            return dctParameters;
        }
        /// <summary>
        /// Extract or get cached version of the columns names from a DBDataReader result
        /// </summary>
        /// <param name="nameOrQuery">Name of the SP or Sql query</param>
        /// <param name="reader">DbDataReader instance</param>
        /// <returns>Dictionary of the column names</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual Dictionary<string, int> ExtractColumnNames(string nameOrQuery, DbDataReader reader)
        {
            if (ColumnsByNameOrQueryCacheInSec <= 0) return InternalExtractColumnNames(nameOrQuery, reader);

            if (ColumnsByNameOrQuery.TryGetValue(nameOrQuery, out var result))
            {
                if (result.Count == reader.FieldCount)
                    return result;
                else
                    ColumnsByNameOrQuery.TryRemove(nameOrQuery, out result);
            }
            return ColumnsByNameOrQuery.GetOrAdd(nameOrQuery, k => (InternalExtractColumnNames(nameOrQuery, reader), TimeSpan.FromSeconds(ColumnsByNameOrQueryCacheInSec)));
        }
        /// <summary>
        /// Extract the columns names from a DBDataReader result
        /// </summary>
        /// <param name="nameOrQuery">Name of the SP or Sql query</param>
        /// <param name="reader">DbDataReader instance</param>
        /// <returns>Dictionary of the column names</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual Dictionary<string, int> InternalExtractColumnNames(string nameOrQuery, DbDataReader reader)
        {
            var dct = new Dictionary<string, int>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                if (!dct.ContainsKey(name))
                    dct[name] = i;
                else
                {
                    var oIdx = dct[name];
                    var nIdx = i;
                    Core.Log.Error($"The column name '{name}' for the query '{nameOrQuery}' is already on the collection. [FirstIndex={oIdx}, CurrentIndex={nIdx}]");
                }
            }
            return dct;
        }
        #endregion

        //Sync Version

        #region SelectElements<T>
        /// <inheritdoc />
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
        /// <inheritdoc />
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
        /// <inheritdoc />
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
            => SelectElements(nameOrQuery, parameters, fillMethod, out object _);
        /// <inheritdoc />
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
        /// <inheritdoc />
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
        /// <inheritdoc />
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
                var sw = Stopwatch.StartNew();
                var response = OnSelectElements(nameOrQuery, parameters, fillMethod, out returnValue);
                sw.Stop();
                Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                FireOnError(ex);
                throw;
            }
        }
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
        protected virtual IEnumerable<T> OnSelectElements<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod, out object returnValue)
        {
            using (var connection = GetConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = GetCommand())
                {
                    #region Sets Connection Object
                    command.Connection = connection;
                    command.CommandTimeout = CommandTimeout;
                    switch (AccessType)
                    {
                        case DataAccessType.Query:
                            command.CommandType = CommandType.Text;
                            break;
                        case DataAccessType.StoredProcedure:
                            command.CommandType = CommandType.StoredProcedure;
                            break;
                    }
                    command.CommandText = nameOrQuery;
                    #endregion

                    #region Bind parameters
                    //Bind parameters
                    ParametersBinder.BindParameters(command, parameters, ParametersPrefix);
                    //Bind return parameter
                    DbParameter returnValueParam = null;
                    if (ReturnParameterName.IsNotNullOrEmpty())
                    {
                        returnValueParam = command.CreateParameter();
                        returnValueParam.ParameterName = ReturnParameterName;
                        returnValueParam.Value = DBNull.Value;
                        returnValueParam.Direction = ParameterDirection.ReturnValue;
                        command.Parameters.Add(returnValueParam);
                    }
                    #endregion

                    #region Sets EntityBinder and FillMethod
                    var entityBinder = new EntityBinder(EntityValueConverter);
                    //Task.Run(() => EntityBinder.PrepareEntity(typeof(T)));
                    Task.Run(() => EntityBinder.PrepareEntity<T>());
                    if (fillMethod is null)
                        fillMethod = DefaultFillMethod<T>.Instance;
                    #endregion

                    try
                    {
                        #region Command Execution
                        var lstRows = new List<EntityDataRow<T>>();
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var cIndex = ExtractColumnNames(nameOrQuery, reader);
                                var indexNumber = cIndex.Count;
                                entityBinder.ColumnIndex = cIndex;
                                do
                                {
                                    var columns = new object[indexNumber];
                                    reader.GetValues(columns);
                                    lstRows.Add(new EntityDataRow<T>(columns, entityBinder, fillMethod));
                                } while (reader.Read());
                            }
                        }
                        returnValue = returnValueParam?.Value;
                        ParametersBinder.RetrieveOutputParameters(command, parameters, ParametersPrefix);
                        connection.Close();
                        #endregion

                        //Returns the IEnumerable of T with the late fillMethod call.
                        return lstRows.Select(row => row.Entity);
                    }
                    catch
                    {
                        connection.Close();
                        throw;
                    }
                }
            }
        }

        #endregion

        #region SelectElement
        /// <inheritdoc />
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
        /// <inheritdoc />
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
        /// <inheritdoc />
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
            => SelectElement(nameOrQuery, parameters, fillMethod, out object _);
        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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
                var sw = Stopwatch.StartNew();
                var response = OnSelectElement(nameOrQuery, parameters, fillMethod, out returnValue);
                sw.Stop();
                Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                FireOnError(ex);
                throw;
            }
        }
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
        protected virtual T OnSelectElement<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod, out object returnValue)
        {
            using (var connection = GetConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = GetCommand())
                {
                    #region Sets Connection Object
                    command.Connection = connection;
                    command.CommandTimeout = CommandTimeout;
                    switch (AccessType)
                    {
                        case DataAccessType.Query:
                            command.CommandType = CommandType.Text;
                            break;
                        case DataAccessType.StoredProcedure:
                            command.CommandType = CommandType.StoredProcedure;
                            break;
                    }
                    command.CommandText = nameOrQuery;
                    #endregion

                    #region Bind parameters
                    //Bind parameters
                    ParametersBinder.BindParameters(command, parameters, ParametersPrefix);
                    //Bind return parameter
                    DbParameter returnValueParam = null;
                    if (ReturnParameterName.IsNotNullOrEmpty())
                    {
                        returnValueParam = command.CreateParameter();
                        returnValueParam.ParameterName = ReturnParameterName;
                        returnValueParam.Value = DBNull.Value;
                        returnValueParam.Direction = ParameterDirection.ReturnValue;
                        command.Parameters.Add(returnValueParam);
                    }
                    #endregion

                    #region Sets EntityBinder and FillMethod
                    var entityBinder = new EntityBinder(EntityValueConverter);
                    //Task.Run(() => EntityBinder.PrepareEntity(typeof(T)));
                    Task.Run(() => EntityBinder.PrepareEntity<T>());
                    if (fillMethod is null)
                        fillMethod = DefaultFillMethod<T>.Instance;
                    #endregion

                    try
                    {
                        #region Command Execution
                        var row = new EntityDataRow<T>(null, entityBinder, fillMethod);
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var cIndex = ExtractColumnNames(nameOrQuery, reader);
                                entityBinder.ColumnIndex = cIndex;
                                var rowValues = new object[cIndex.Count];
                                reader.GetValues(rowValues);
                                row.SetRowValues(rowValues);
                            }
                        }
                        returnValue = returnValueParam?.Value;
                        ParametersBinder.RetrieveOutputParameters(command, parameters, ParametersPrefix);
                        connection.Close();
                        #endregion

                        return row.Entity;
                    }
                    catch
                    {
                        connection.Close();
                        throw;
                    }
                }
            }
        }

        #endregion

        #region ExecuteNonQuery
        /// <inheritdoc />
        /// <summary>
        /// Execute a command on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ExecuteNonQuery(string nameOrQuery, object parameters)
            => ExecuteNonQuery(nameOrQuery, GetCommandParameters(parameters));
        /// <inheritdoc />
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
                var sw = Stopwatch.StartNew();
                var response = OnExecuteNonQuery(nameOrQuery, parameters);
                sw.Stop();
                Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                FireOnError(ex);
                throw;
            }
        }
        /// <summary>
        /// Execute a command on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual int OnExecuteNonQuery(string nameOrQuery, IDictionary<string, object> parameters = null)
        {
            using (var connection = GetConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = GetCommand())
                {
                    #region Sets Connection Object
                    command.Connection = connection;
                    command.CommandTimeout = CommandTimeout;
                    switch (AccessType)
                    {
                        case DataAccessType.Query:
                            command.CommandType = CommandType.Text;
                            break;
                        case DataAccessType.StoredProcedure:
                            command.CommandType = CommandType.StoredProcedure;
                            break;
                    }
                    command.CommandText = nameOrQuery;
                    #endregion

                    #region Bind parameters
                    //Bind parameters
                    ParametersBinder.BindParameters(command, parameters, ParametersPrefix);
                    #endregion

                    try
                    {
                        #region Command Execution
                        connection.Open();
                        var resInt = command.ExecuteNonQuery();
                        ParametersBinder.RetrieveOutputParameters(command, parameters, ParametersPrefix);
                        connection.Close();
                        #endregion

                        //Returns the IEnumerable of T with the late fillMethod call.
                        return resInt;
                    }
                    catch
                    {
                        connection.Close();
                        throw;
                    }
                }
            }
        }
        #endregion

        #region ExecuteNonQuery with array
        /// <summary>
        /// Execute a command multiple times over an array of parameters on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parametersArray">Inputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteNonQuery(string nameOrQuery, object[] parametersArray)
            => ExecuteNonQuery(nameOrQuery, parametersArray.Select(GetCommandParameters));
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
                var sw = Stopwatch.StartNew();
                OnExecuteNonQuery(nameOrQuery, parametersIEnumerable);
                sw.Stop();
                Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                FireOnError(ex);
                throw;
            }
        }
        /// <summary>
        /// Execute a command multiple times over an array of parameters on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parametersIEnumerable">Inputsparameters</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnExecuteNonQuery(string nameOrQuery, IEnumerable<IDictionary<string, object>> parametersIEnumerable = null)
        {
            using (var connection = GetConnection())
            {
                connection.ConnectionString = ConnectionString;
                try
                {
                    connection.Open();

                    using (var command = GetCommand())
                    {
                        var transaction = connection.BeginTransaction();

                        #region Sets Connection Object
                        command.Connection = connection;
                        command.Transaction = transaction;
                        command.CommandTimeout = CommandTimeout;
                        switch (AccessType)
                        {
                            case DataAccessType.Query:
                                command.CommandType = CommandType.Text;
                                break;
                            case DataAccessType.StoredProcedure:
                                command.CommandType = CommandType.StoredProcedure;
                                break;
                        }
                        command.CommandText = nameOrQuery;
                        #endregion

                        if (parametersIEnumerable != null)
                        {
                            foreach (var parameters in parametersIEnumerable)
                            {
                                #region Bind parameters
                                //Bind parameters
                                ParametersBinder.BindParameters(command, parameters, ParametersPrefix);
                                #endregion

                                #region Command Execution
                                command.ExecuteNonQuery();
                                #endregion
                            }
                        }

                        try
                        {
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                            transaction.Rollback();
                            throw;
                        }
                    }
                    connection.Close();
                }
                catch
                {
                    connection.Close();
                    throw;
                }
            }
        }
        #endregion

        #region SelectScalar
        /// <inheritdoc />
        /// <summary>
        /// Select a single row field from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T SelectScalar<T>(string nameOrQuery, object parameters)
            => SelectScalar<T>(nameOrQuery, GetCommandParameters(parameters));
        /// <inheritdoc />
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
                var sw = Stopwatch.StartNew();
                var response = OnSelectScalar<T>(nameOrQuery, parameters);
                sw.Stop();
                Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                FireOnError(ex);
                throw;
            }
        }
        /// <summary>
        /// Select a single row field from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual T OnSelectScalar<T>(string nameOrQuery, IDictionary<string, object> parameters = null)
        {
            using (var connection = GetConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = GetCommand())
                {
                    #region Sets Connection Object
                    command.Connection = connection;
                    command.CommandTimeout = CommandTimeout;
                    switch (AccessType)
                    {
                        case DataAccessType.Query:
                            command.CommandType = CommandType.Text;
                            break;
                        case DataAccessType.StoredProcedure:
                            command.CommandType = CommandType.StoredProcedure;
                            break;
                    }
                    command.CommandText = nameOrQuery;
                    #endregion

                    #region Bind parameters
                    //Bind parameters
                    ParametersBinder.BindParameters(command, parameters, ParametersPrefix);
                    #endregion

                    try
                    {
                        #region Command Execution
                        connection.Open();
                        var value = command.ExecuteScalar();
                        ParametersBinder.RetrieveOutputParameters(command, parameters, ParametersPrefix);
                        connection.Close();
                        #endregion

                        var valueType = value?.GetType();
                        var propertyType = typeof(T).GetUnderlyingType();
                        var defaultValue = default(T);

                        object result = defaultValue;

                        if (propertyType == typeof(Guid) && valueType == typeof(string))
                            result = new Guid((string)value);
                        else if (propertyType.IsEnum &&
                            (valueType == typeof(int) || valueType == typeof(long) || valueType == typeof(string) || valueType == typeof(byte)))
                            result = Enum.Parse(propertyType, value.ToString());
                        else if (EntityValueConverter != null && EntityValueConverter.Convert(value, valueType, typeof(T), defaultValue, out var valueConverterResult))
                            result = valueConverterResult;
                        else
                        {
                            try
                            {
                                result = Convert.ChangeType(value, propertyType);
                            }
                            catch (Exception ex)
                            {
                                Core.Log.Write(ex);
                            }
                        }
                        return (T)result;
                    }
                    catch
                    {
                        connection.Close();
                        throw;
                    }
                }
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
            => SelectElements(nameOrQuery, null, fillMethod1, fillMethod2);
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
            => SelectElements(nameOrQuery, GetCommandParameters(parameters), fillMethod1, fillMethod2);
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
            => SelectElements(nameOrQuery, parameters, fillMethod1, fillMethod2, out object _);
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
                var sw = Stopwatch.StartNew();
                var rs1 = new ResultSet<T1>(fillMethod1);
                var rs2 = new ResultSet<T2>(fillMethod2);
                OnSelectElements(nameOrQuery, parameters, new IResultSet[] { rs1, rs2 }, out returnValue);
                sw.Stop();
                Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
                return (rs1.Result, rs2.Result);
            }
            catch (Exception ex)
            {
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
            => SelectElements(nameOrQuery, null, fillMethod1, fillMethod2, fillMethod3);
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
            => SelectElements(nameOrQuery, GetCommandParameters(parameters), fillMethod1, fillMethod2, fillMethod3);
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
            => SelectElements(nameOrQuery, parameters, fillMethod1, fillMethod2, fillMethod3, out object _);
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
                var sw = Stopwatch.StartNew();
                var rs1 = new ResultSet<T1>(fillMethod1);
                var rs2 = new ResultSet<T2>(fillMethod2);
                var rs3 = new ResultSet<T3>(fillMethod3);
                OnSelectElements(nameOrQuery, parameters, new IResultSet[] { rs1, rs2, rs3 }, out returnValue);
                sw.Stop();
                Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
                return (rs1.Result, rs2.Result, rs3.Result);
            }
            catch (Exception ex)
            {
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
        {
            var sw = Stopwatch.StartNew();
            OnSelectElements(nameOrQuery, parameters, resultSets, out returnValue);
            sw.Stop();
            Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
        }
        /// <summary>
        /// Selects a all Result sets with a collection of elements from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="resultSets">Array of IResultSetItem instances to fill from the data source</param>
        /// <param name="returnValue">Return value from the data source</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnSelectElements(string nameOrQuery, IDictionary<string, object> parameters, IResultSet[] resultSets, out object returnValue)
        {
            returnValue = null;
            if (resultSets is null) return;
            using (var connection = GetConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = GetCommand())
                {
                    #region Sets Connection Object
                    command.Connection = connection;
                    command.CommandTimeout = CommandTimeout;
                    switch (AccessType)
                    {
                        case DataAccessType.Query:
                            command.CommandType = CommandType.Text;
                            break;
                        case DataAccessType.StoredProcedure:
                            command.CommandType = CommandType.StoredProcedure;
                            break;
                    }
                    command.CommandText = nameOrQuery;
                    #endregion

                    #region Bind parameters
                    //Bind parameters
                    ParametersBinder.BindParameters(command, parameters, ParametersPrefix);
                    //Bind return parameter
                    DbParameter returnValueParam = null;
                    if (ReturnParameterName.IsNotNullOrEmpty())
                    {
                        returnValueParam = command.CreateParameter();
                        returnValueParam.ParameterName = ReturnParameterName;
                        returnValueParam.Value = DBNull.Value;
                        returnValueParam.Direction = ParameterDirection.ReturnValue;
                        command.Parameters.Add(returnValueParam);
                    }
                    #endregion

                    #region Sets EntityBinder and FillMethod
                    resultSets.Each(r => r.PrepareSet(EntityValueConverter));
                    #endregion

                    try
                    {
                        #region Command Execution
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            var resultSetIndex = 0;

                            do
                            {
                                var resultset = resultSets[resultSetIndex];
                                if (reader.Read())
                                {
                                    var dct = new Dictionary<string, int>();
                                    for (var i = 0; i < reader.FieldCount; i++)
                                    {
                                        var name = reader.GetName(i);
                                        if (!dct.ContainsKey(name))
                                            dct[name] = i;
                                        else
                                        {
                                            var oIdx = dct[name];
                                            var nIdx = i;
                                            Core.Log.Error($"The column name '{name}' for the query '{nameOrQuery}' is already on the collection. [ResulsetIndex={resultSetIndex}, FirstIndex={oIdx}, CurrentIndex={nIdx}]");
                                        }
                                    }
                                    var indexNumber = dct.Count;
                                    resultset.SetColumnsOnBinder(dct);

                                    do
                                    {
                                        var columns = new object[indexNumber];
                                        reader.GetValues(columns);
                                        resultset.AddRow(columns);
                                    } while (reader.Read());
                                }
                                resultSetIndex++;
                            } while (reader.NextResult() && resultSets.Length > resultSetIndex);
                        }
                        returnValue = returnValueParam?.Value;
                        ParametersBinder.RetrieveOutputParameters(command, parameters, ParametersPrefix);
                        connection.Close();
                        #endregion
                    }
                    catch
                    {
                        connection.Close();
                        throw;
                    }
                }
            }
        }
        #endregion

        //Async Version

        #region SelectElementsAsync<T>
        /// <inheritdoc />
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
        /// <inheritdoc />
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
        /// <inheritdoc />
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
                var sw = Stopwatch.StartNew();
                var response = await OnSelectElementsAsync(nameOrQuery, parameters, fillMethod).ConfigureAwait(false);
                sw.Stop();
                Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                FireOnError(ex);
                throw;
            }
        }
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual async Task<IEnumerable<T>> OnSelectElementsAsync<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod)
        {
            using (var connection = GetConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = GetCommand())
                {
                    #region Sets Connection Object
                    command.Connection = connection;
                    command.CommandTimeout = CommandTimeout;
                    switch (AccessType)
                    {
                        case DataAccessType.Query:
                            command.CommandType = CommandType.Text;
                            break;
                        case DataAccessType.StoredProcedure:
                            command.CommandType = CommandType.StoredProcedure;
                            break;
                    }
                    command.CommandText = nameOrQuery;
                    #endregion

                    #region Bind parameters
                    //Bind parameters
                    ParametersBinder.BindParameters(command, parameters, ParametersPrefix);
                    #endregion

                    #region Sets EntityBinder and FillMethod
                    var entityBinder = new EntityBinder(EntityValueConverter);
                    //EntityBinder.PrepareEntity(typeof(T));
                    EntityBinder.PrepareEntity<T>();
                    if (fillMethod is null)
                        fillMethod = DefaultFillMethod<T>.Instance;
                    #endregion

                    try
                    {
                        #region Command Execution
                        var lstRows = new List<EntityDataRow<T>>();
                        await connection.OpenAsync().ConfigureAwait(false);
                        using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            if (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                var cIndex = ExtractColumnNames(nameOrQuery, reader);
                                var indexNumber = cIndex.Count;
                                entityBinder.ColumnIndex = cIndex;
                                do
                                {
                                    var columns = new object[indexNumber];
                                    reader.GetValues(columns);
                                    lstRows.Add(new EntityDataRow<T>(columns, entityBinder, fillMethod));
                                } while (await reader.ReadAsync().ConfigureAwait(false));
                            }
                        }
                        ParametersBinder.RetrieveOutputParameters(command, parameters, ParametersPrefix);
                        connection.Close();
                        #endregion

                        //Returns the IEnumerable of T with the late fillMethod call.
                        return lstRows.Select(row => row.Entity);
                    }
                    catch
                    {
                        connection.Close();
                        throw;
                    }
                }
            }
        }
        #endregion

        #region SelectElementAsync
        /// <inheritdoc />
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
        /// <inheritdoc />
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
        /// <inheritdoc />
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
                var sw = Stopwatch.StartNew();
                var response = await OnSelectElementAsync(nameOrQuery, parameters, fillMethod).ConfigureAwait(false);
                sw.Stop();
                Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                FireOnError(ex);
                throw;
            }
        }
        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>Single entity from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual async Task<T> OnSelectElementAsync<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod)
        {
            using (var connection = GetConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = GetCommand())
                {
                    #region Sets Connection Object
                    command.Connection = connection;
                    command.CommandTimeout = CommandTimeout;
                    switch (AccessType)
                    {
                        case DataAccessType.Query:
                            command.CommandType = CommandType.Text;
                            break;
                        case DataAccessType.StoredProcedure:
                            command.CommandType = CommandType.StoredProcedure;
                            break;
                    }
                    command.CommandText = nameOrQuery;
                    #endregion

                    #region Bind parameters
                    //Bind parameters
                    ParametersBinder.BindParameters(command, parameters, ParametersPrefix);
                    #endregion

                    #region Sets EntityBinder and FillMethod
                    var entityBinder = new EntityBinder(EntityValueConverter);
                    //EntityBinder.PrepareEntity(typeof(T));
                    EntityBinder.PrepareEntity<T>();
                    if (fillMethod is null)
                        fillMethod = DefaultFillMethod<T>.Instance;
                    #endregion

                    try
                    {
                        #region Command Execution
                        var row = new EntityDataRow<T>(null, entityBinder, fillMethod);
                        await connection.OpenAsync().ConfigureAwait(false);
                        using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            if (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                var cIndex = ExtractColumnNames(nameOrQuery, reader);
                                entityBinder.ColumnIndex = cIndex;
                                var rowValues = new object[cIndex.Count];
                                reader.GetValues(rowValues);
                                row.SetRowValues(rowValues);
                            }
                        }
                        ParametersBinder.RetrieveOutputParameters(command, parameters, ParametersPrefix);
                        connection.Close();
                        #endregion

                        return row.Entity;
                    }
                    catch
                    {
                        connection.Close();
                        throw;
                    }
                }
            }
        }
        #endregion

        #region ExecuteNonQueryAsync
        /// <inheritdoc />
        /// <summary>
        /// Execute a command on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> ExecuteNonQueryAsync(string nameOrQuery, object parameters)
            => ExecuteNonQueryAsync(nameOrQuery, GetCommandParameters(parameters));
        /// <inheritdoc />
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
                var sw = Stopwatch.StartNew();
                var response = await OnExecuteNonQueryAsync(nameOrQuery, parameters).ConfigureAwait(false);
                sw.Stop();
                Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                FireOnError(ex);
                throw;
            }
        }
        /// <summary>
        /// Execute a command on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual async Task<int> OnExecuteNonQueryAsync(string nameOrQuery, IDictionary<string, object> parameters = null)
        {
            using (var connection = GetConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = GetCommand())
                {
                    #region Sets Connection Object
                    command.Connection = connection;
                    command.CommandTimeout = CommandTimeout;
                    switch (AccessType)
                    {
                        case DataAccessType.Query:
                            command.CommandType = CommandType.Text;
                            break;
                        case DataAccessType.StoredProcedure:
                            command.CommandType = CommandType.StoredProcedure;
                            break;
                    }
                    command.CommandText = nameOrQuery;
                    #endregion

                    #region Bind parameters
                    //Bind parameters
                    ParametersBinder.BindParameters(command, parameters, ParametersPrefix);
                    #endregion

                    try
                    {
                        #region Command Execution
                        await connection.OpenAsync().ConfigureAwait(false);
                        var resInt = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        ParametersBinder.RetrieveOutputParameters(command, parameters, ParametersPrefix);
                        connection.Close();
                        #endregion

                        //Returns the IEnumerable of T with the late fillMethod call.
                        return resInt;
                    }
                    catch
                    {
                        connection.Close();
                        throw;
                    }
                }
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
            => ExecuteNonQueryAsync(nameOrQuery, parametersArray.Select(GetCommandParameters));
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
                var sw = Stopwatch.StartNew();
                await OnExecuteNonQueryAsync(nameOrQuery, parametersIEnumerable).ConfigureAwait(false);
                sw.Stop();
                Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                FireOnError(ex);
                throw;
            }
        }
        /// <summary>
        /// Execute a command multiple times over an array of parameters on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parametersIEnumerable">Inputsparameters</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual async Task OnExecuteNonQueryAsync(string nameOrQuery, IEnumerable<IDictionary<string, object>> parametersIEnumerable = null)
        {
            using (var connection = GetConnection())
            {
                connection.ConnectionString = ConnectionString;
                try
                {
                    await connection.OpenAsync().ConfigureAwait(false);

                    using (var command = GetCommand())
                    {
                        var transaction = connection.BeginTransaction();

                        #region Sets Connection Object
                        command.Connection = connection;
                        command.Transaction = transaction;
                        command.CommandTimeout = CommandTimeout;
                        switch (AccessType)
                        {
                            case DataAccessType.Query:
                                command.CommandType = CommandType.Text;
                                break;
                            case DataAccessType.StoredProcedure:
                                command.CommandType = CommandType.StoredProcedure;
                                break;
                        }
                        command.CommandText = nameOrQuery;
                        #endregion

                        if (parametersIEnumerable != null)
                        {
                            foreach (var parameters in parametersIEnumerable)
                            {
                                #region Bind parameters

                                //Bind parameters
                                ParametersBinder.BindParameters(command, parameters, ParametersPrefix);

                                #endregion

                                #region Command Execution

                                await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                                #endregion
                            }
                        }

                        try
                        {
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                            transaction.Rollback();
                            throw;
                        }
                    }
                    connection.Close();
                }
                catch
                {
                    connection.Close();
                    throw;
                }
            }
        }
        #endregion

        #region SelectScalarAsync
        /// <inheritdoc />
        /// <summary>
        /// Select a single row field from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> SelectScalarAsync<T>(string nameOrQuery, object parameters)
            => SelectScalarAsync<T>(nameOrQuery, GetCommandParameters(parameters));
        /// <inheritdoc />
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
                var sw = Stopwatch.StartNew();
                var response = await OnSelectScalarAsync<T>(nameOrQuery, parameters).ConfigureAwait(false);
                sw.Stop();
                Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                FireOnError(ex);
                throw;
            }
        }
        /// <summary>
        /// Select a single row field from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual async Task<T> OnSelectScalarAsync<T>(string nameOrQuery, IDictionary<string, object> parameters = null)
        {
            using (var connection = GetConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = GetCommand())
                {
                    #region Sets Connection Object
                    command.Connection = connection;
                    command.CommandTimeout = CommandTimeout;
                    switch (AccessType)
                    {
                        case DataAccessType.Query:
                            command.CommandType = CommandType.Text;
                            break;
                        case DataAccessType.StoredProcedure:
                            command.CommandType = CommandType.StoredProcedure;
                            break;
                    }
                    command.CommandText = nameOrQuery;
                    #endregion

                    #region Bind parameters
                    //Bind parameters
                    ParametersBinder.BindParameters(command, parameters, ParametersPrefix);
                    #endregion

                    try
                    {
                        #region Command Execution
                        await connection.OpenAsync().ConfigureAwait(false);
                        var value = await command.ExecuteScalarAsync().ConfigureAwait(false);
                        ParametersBinder.RetrieveOutputParameters(command, parameters, ParametersPrefix);
                        connection.Close();
                        #endregion

                        var valueType = value?.GetType();
                        var propertyType = typeof(T).GetUnderlyingType();
                        var defaultValue = default(T);

                        object result = defaultValue;

                        if (propertyType == typeof(Guid) && valueType == typeof(string))
                            result = new Guid((string)value);
                        else if (propertyType.IsEnum &&
                            (valueType == typeof(int) || valueType == typeof(long) || valueType == typeof(string) || valueType == typeof(byte)))
                            result = Enum.Parse(propertyType, value.ToString());
                        else if (EntityValueConverter != null && EntityValueConverter.Convert(value, valueType, propertyType, defaultValue, out var valueConverterResult))
                            result = valueConverterResult;
                        else
                        {
                            try
                            {
                                result = Convert.ChangeType(value, propertyType);
                            }
                            catch (Exception ex)
                            {
                                Core.Log.Write(ex);
                            }
                        }
                        return (T)result;
                    }
                    catch
                    {
                        connection.Close();
                        throw;
                    }
                }
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
            => SelectElementsAsync(nameOrQuery, null, fillMethod1, fillMethod2);
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
            => SelectElementsAsync(nameOrQuery, GetCommandParameters(parameters), fillMethod1, fillMethod2);
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
                var sw = Stopwatch.StartNew();
                var rs1 = new ResultSet<T1>(fillMethod1);
                var rs2 = new ResultSet<T2>(fillMethod2);
                await OnSelectElementsAsync(nameOrQuery, parameters, new IResultSet[] { rs1, rs2 }).ConfigureAwait(false);
                sw.Stop();
                Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
                return (rs1.Result, rs2.Result);
            }
            catch (Exception ex)
            {
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
            => SelectElementsAsync(nameOrQuery, null, fillMethod1, fillMethod2, fillMethod3);
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
            => SelectElementsAsync(nameOrQuery, GetCommandParameters(parameters), fillMethod1, fillMethod2, fillMethod3);
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
                var sw = Stopwatch.StartNew();
                var rs1 = new ResultSet<T1>(fillMethod1);
                var rs2 = new ResultSet<T2>(fillMethod2);
                var rs3 = new ResultSet<T3>(fillMethod3);
                await OnSelectElementsAsync(nameOrQuery, parameters, new IResultSet[] { rs1, rs2, rs3 }).ConfigureAwait(false);
                sw.Stop();
                Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
                return (rs1.Result, rs2.Result, rs3.Result);
            }
            catch (Exception ex)
            {
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
        public async Task SelectResultSetsElementsAsync(string nameOrQuery, IDictionary<string, object> parameters, IResultSet[] resultSets)
        {
            var sw = Stopwatch.StartNew();
            await OnSelectElementsAsync(nameOrQuery, parameters, resultSets).ConfigureAwait(false);
            sw.Stop();
            Core.Counters.GetDoubleCounter(_counterCategory, nameOrQuery, Diagnostics.Counters.CounterType.Average, Diagnostics.Counters.CounterLevel.Framework).Add(sw.Elapsed.TotalMilliseconds);
        }
        /// <summary>
        /// Selects a all Result sets with a collection of elements from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="resultSets">Array of IResultSetItem instances to fill from the data source</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual async Task OnSelectElementsAsync(string nameOrQuery, IDictionary<string, object> parameters, IResultSet[] resultSets)
        {
            if (resultSets is null) return;
            using (var connection = GetConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = GetCommand())
                {
                    #region Sets Connection Object
                    command.Connection = connection;
                    command.CommandTimeout = CommandTimeout;
                    switch (AccessType)
                    {
                        case DataAccessType.Query:
                            command.CommandType = CommandType.Text;
                            break;
                        case DataAccessType.StoredProcedure:
                            command.CommandType = CommandType.StoredProcedure;
                            break;
                    }
                    command.CommandText = nameOrQuery;
                    #endregion

                    #region Bind parameters
                    //Bind parameters
                    ParametersBinder.BindParameters(command, parameters, ParametersPrefix);
                    #endregion

                    #region Sets EntityBinder and FillMethod
                    resultSets.Each(r => r.PrepareSet(EntityValueConverter));
                    #endregion

                    try
                    {
                        #region Command Execution
                        await connection.OpenAsync().ConfigureAwait(false);
                        using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            var resultSetIndex = 0;

                            do
                            {
                                var resultset = resultSets[resultSetIndex];

                                if (await reader.ReadAsync().ConfigureAwait(false))
                                {
                                    var dct = new Dictionary<string, int>();
                                    for (var i = 0; i < reader.FieldCount; i++)
                                    {
                                        var name = reader.GetName(i);
                                        if (!dct.ContainsKey(name))
                                            dct[name] = i;
                                        else
                                        {
                                            var oIdx = dct[name];
                                            var nIdx = i;
                                            Core.Log.Error($"The column name '{name}' for the query '{nameOrQuery}' is already on the collection. [ResulsetIndex={resultSetIndex}, FirstIndex={oIdx}, CurrentIndex={nIdx}]");
                                        }
                                    }
                                    var indexNumber = dct.Count;
                                    resultset.SetColumnsOnBinder(dct);
                                    do
                                    {
                                        var columns = new object[indexNumber];
                                        reader.GetValues(columns);
                                        resultset.AddRow(columns);
                                    } while (await reader.ReadAsync().ConfigureAwait(false));
                                }
                                resultSetIndex++;
                            } while (await reader.NextResultAsync().ConfigureAwait(false) && resultSets.Length > resultSetIndex);
                        }
                        ParametersBinder.RetrieveOutputParameters(command, parameters, ParametersPrefix);
                        connection.Close();
                        #endregion
                    }
                    catch
                    {
                        connection.Close();
                        throw;
                    }
                }
            }
        }
        #endregion


        #region GetSchema
        /// <inheritdoc />
        /// <summary>
        /// Get Database Schema
        /// </summary>
        /// <returns>DataTable with all schema</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CatalogSchema GetSchema()
        {
            using (var connection = GetConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
                var dset = OnGetSchema(connection);
                connection.Close();
                return dset;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Get Database Schema
        /// </summary>
        /// <returns>DataTable with all schema</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<CatalogSchema> GetSchemaAsync()
        {
            using (var connection = GetConnection())
            {
                connection.ConnectionString = ConnectionString;
                await connection.OpenAsync().ConfigureAwait(false);
                var dset = OnGetSchema(connection);
                connection.Close();
                return dset;
            }
        }

        /// <summary>
        /// On GetSchema
        /// </summary>
        /// <param name="connection">Opened connection</param>
        /// <returns>Schema instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual CatalogSchema OnGetSchema(DbConnection connection)
        {
            return null;
        }
        #endregion

        #region IDataAccessDynamicGenerator
        /// <inheritdoc />
        /// <summary>
        /// Get the Select Base Sql from a GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>Select base sql query</returns>
        public virtual string GetSelectFromContainer(GeneratorSelectionContainer container)
        {
            throw new NotSupportedException("The DynamicQuery feature is not supported by this provider.");
        }
        /// <inheritdoc />
        /// <summary>
        /// Get the Where from Sql from GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>The where list.</returns>
        public virtual List<(string, string)> GetWhereFromContainer(GeneratorSelectionContainer container)
        {
            throw new NotSupportedException("The DynamicQuery feature is not supported by this provider.");
        }
        /// <inheritdoc />
        /// <summary>
        /// Get the Insert sql from a GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>Sql query</returns>
        public virtual string GetInsertFromContainer(GeneratorSelectionContainer container)
        {
            throw new NotSupportedException("The DynamicQuery feature is not supported by this provider.");
        }
        /// <inheritdoc />
        /// <summary>
        /// Get the Update sql from a GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>Sql query</returns>
        public virtual string GetUpdateFromContainer(GeneratorSelectionContainer container)
        {
            throw new NotSupportedException("The DynamicQuery feature is not supported by this provider.");
        }
        /// <inheritdoc />
        /// <summary>
        /// Get the Delete sql from a GeneratorSelectionContainer instance
        /// </summary>
        /// <param name="container">Container object</param>
        /// <returns>Sql query</returns>
        public virtual string GetDeleteFromContainer(GeneratorSelectionContainer container)
        {
            throw new NotSupportedException("The DynamicQuery feature is not supported by this provider.");
        }
        #endregion


        #region Nested Class
        /// <summary>
        /// Default Fill Method for an entity
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        public static class DefaultFillMethod<T>
        {
            /// <summary>
            /// Default fill method delegate
            /// </summary>
            public static FillDataDelegate<T> Instance = (e, o) => e.Bind<T>(o);
        }
        #endregion
    }
}
