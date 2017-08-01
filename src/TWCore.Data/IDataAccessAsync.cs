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
using System.Threading.Tasks;

namespace TWCore.Data
{
    /// <summary>
    /// Data access async connection interface definition
    /// </summary>
    public interface IDataAccessAsync
    {
        /// <summary>
        /// Fires when an error occurs in the execution of a command.
        /// </summary>
        event EventHandler<EventArgs<Exception>> OnError;

        #region SelectElementsAsync
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        Task<IEnumerable<T>> SelectElementsAsync<T>(string nameOrQuery, FillDataDelegate<T> fillMethod = null);
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        Task<IEnumerable<T>> SelectElementsAsync<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod = null);
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        Task<IEnumerable<T>> SelectElementsAsync<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod = null);
        #endregion

        #region SelectElementAsync
        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>Single entity from the data source</returns>
        Task<T> SelectElementAsync<T>(string nameOrQuery, FillDataDelegate<T> fillMethod = null);
        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>Single entity from the data source</returns>
        Task<T> SelectElementAsync<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod = null);
        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>Single entity from the data source</returns>
        Task<T> SelectElementAsync<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod = null);
        #endregion

        #region ExecuteNonQueryAsync
        /// <summary>
        /// Execute a command on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        Task<int> ExecuteNonQueryAsync(string nameOrQuery, IDictionary<string, object> parameters = null);
        /// <summary>
        /// Execute a command on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        Task<int> ExecuteNonQueryAsync(string nameOrQuery, object parameters);
        #endregion

        #region SelectScalarAsync
        /// <summary>
        /// Select a single row field from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        Task<T> SelectScalarAsync<T>(string nameOrQuery, IDictionary<string, object> parameters = null);
        /// <summary>
        /// Select a single row field from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        Task<T> SelectScalarAsync<T>(string nameOrQuery, object parameters);
        #endregion
    }
}
