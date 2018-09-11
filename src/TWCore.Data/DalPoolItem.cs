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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TWCore.Data.Schema;

namespace TWCore.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Dal Pool Item
    /// </summary>
    public class DalPoolItem : IDataAccess
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal readonly ObjectPool<IDataAccess> Pool;

        /// <summary>
        /// Dal Pool item
        /// </summary>
		/// <param name="pool">ObjectPool instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DalPoolItem(ObjectPool<IDataAccess> pool)
        {
			Pool = pool;
        }

		#region Private Methods
		private void DataAccess_OnError(object sender, EventArgs<Exception> e)
            => OnError?.Invoke(sender, e);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IDataAccess GetDataAccess()
		{
			var dAccess = Pool.New();
			dAccess.OnError += DataAccess_OnError;
			return dAccess;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void StoreDataAccess(IDataAccess daccess)
		{
			daccess.OnError -= DataAccess_OnError;
			Pool.Store(daccess);
		}
        #endregion

        #region IDataAccess
        /// <summary>
        /// Fires when an error occurs in the execution of a command.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> OnError;

        /// <summary>
        /// Execute a command on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ExecuteNonQuery(string nameOrQuery, IDictionary<string, object> parameters = null)
		{
			var dAccess = GetDataAccess();
            var res = dAccess.ExecuteNonQuery(nameOrQuery, parameters);
			StoreDataAccess(dAccess);
			return res;
		}
        /// <summary>
        /// Execute a command on the data source and returns the number of rows.
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ExecuteNonQuery(string nameOrQuery, object parameters)
		{
			var dAccess = GetDataAccess();
            var res = dAccess.ExecuteNonQuery(nameOrQuery, parameters);
			StoreDataAccess(dAccess);
			return res;
		}
        /// <summary>
        /// Select a single element from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>Single entity from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T SelectElement<T>(string nameOrQuery, FillDataDelegate<T> fillMethod = null)
		{
			var dAccess = GetDataAccess();
			var res = dAccess.SelectElement(nameOrQuery, fillMethod);
			StoreDataAccess(dAccess);
			return res;
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
		public T SelectElement<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod = null)
		{
			var dAccess = GetDataAccess();
            var res = dAccess.SelectElement(nameOrQuery, parameters, fillMethod);
			StoreDataAccess(dAccess);
			return res;
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
		public T SelectElement<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod = null)
		{
			var dAccess = GetDataAccess();
            var res = dAccess.SelectElement(nameOrQuery, parameters, fillMethod);
			StoreDataAccess(dAccess);
			return res;
		}
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
		{
			var dAccess = GetDataAccess();
            var res = dAccess.SelectElement(nameOrQuery, fillMethod, out returnValue);
			StoreDataAccess(dAccess);
			return res;
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
		public T SelectElement<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod, out object returnValue)
		{
			var dAccess = GetDataAccess();
            var res = dAccess.SelectElement(nameOrQuery, parameters, fillMethod, out returnValue);
			StoreDataAccess(dAccess);
			return res;
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
		public T SelectElement<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod, out object returnValue)
		{
			var dAccess = GetDataAccess();
            var res = dAccess.SelectElement(nameOrQuery, parameters, fillMethod, out returnValue);
			StoreDataAccess(dAccess);
			return res;
		}
        /// <summary>
        /// Selects a collection of elements from the data source
        /// </summary>
        /// <typeparam name="T">Type of entity to be selected</typeparam>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="fillMethod">Entity fill delegate</param>
        /// <returns>IEnumerable of entity type with the results from the data source</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerable<T> SelectElements<T>(string nameOrQuery, FillDataDelegate<T> fillMethod = null)
		{
			var dAccess = GetDataAccess();
            var res = dAccess.SelectElements(nameOrQuery, fillMethod);
			StoreDataAccess(dAccess);
			return res;
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
		public IEnumerable<T> SelectElements<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod = null)
		{
			var dAccess = GetDataAccess();
            var res = dAccess.SelectElements(nameOrQuery, parameters, fillMethod);
			StoreDataAccess(dAccess);
			return res;
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
		public IEnumerable<T> SelectElements<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod = null)
		{
			var dAccess = GetDataAccess();
            var res = dAccess.SelectElements(nameOrQuery, parameters, fillMethod);
			StoreDataAccess(dAccess);
			return res;
		}
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
		{
			var dAccess = GetDataAccess();
            var res = dAccess.SelectElements(nameOrQuery, fillMethod, out returnValue);
			StoreDataAccess(dAccess);
			return res;
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
		public IEnumerable<T> SelectElements<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod, out object returnValue)
		{
			var dAccess = GetDataAccess();
            var res = dAccess.SelectElements(nameOrQuery, parameters, fillMethod, out returnValue);
			StoreDataAccess(dAccess);
			return res;
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
		public IEnumerable<T> SelectElements<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod, out object returnValue)
		{
			var dAccess = GetDataAccess();
            var res = dAccess.SelectElements(nameOrQuery, parameters, fillMethod, out returnValue);
			StoreDataAccess(dAccess);
			return res;
		}
        /// <summary>
        /// Select a single row field from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T SelectScalar<T>(string nameOrQuery, IDictionary<string, object> parameters = null)
		{
			var dAccess = GetDataAccess();
            var res = dAccess.SelectScalar<T>(nameOrQuery, parameters);
			StoreDataAccess(dAccess);
			return res;
		}
        /// <summary>
        /// Select a single row field from the data source
        /// </summary>
        /// <param name="nameOrQuery">Procedure name or sql query</param>
        /// <param name="parameters">Inputs and outputs parameters</param>
        /// <returns>Number of rows</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T SelectScalar<T>(string nameOrQuery, object parameters)
		{
			var dAccess = GetDataAccess();
            var res = dAccess.SelectScalar<T>(nameOrQuery, parameters);
			StoreDataAccess(dAccess);
			return res;
		}
        /// <summary>
        /// Get Database Schema
        /// </summary>
        /// <returns>DataTable with all schema</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CatalogSchema GetSchema()
        {
            var dAccess = GetDataAccess();
            var res = dAccess.GetSchema();
            StoreDataAccess(dAccess);
            return res;
        }
        #endregion
    }
}
