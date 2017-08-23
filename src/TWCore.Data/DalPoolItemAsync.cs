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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TWCore.Data
{
    /// <summary>
    /// Dal Pool Item
    /// </summary>
    public class DalPoolItemAsync : IDataAccessAsync
    {
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		internal ObjectPool<IDataAccessAsync> Pool;

		/// <summary>
		/// Dal Pool item
		/// </summary>
		/// <param name="pool">ObjectPool instance</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DalPoolItemAsync(ObjectPool<IDataAccessAsync> pool)
        {
			Pool = pool;
        }

		#region Private Methods
		private void DataAccess_OnError(object sender, EventArgs<Exception> e)
			=> OnError?.Invoke(sender, e);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IDataAccessAsync GetDataAccess()
		{
			var dAccess = Pool.New();
			dAccess.OnError += DataAccess_OnError;
			return dAccess;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void StoreDataAccess(IDataAccessAsync daccess)
		{
			daccess.OnError -= DataAccess_OnError;
			Pool.Store(daccess);
		}
		#endregion

		#region IDataAccess
		public event EventHandler<EventArgs<Exception>> OnError;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<IEnumerable<T>> SelectElementsAsync<T>(string nameOrQuery, FillDataDelegate<T> fillMethod = null)
		{
			var dAccess = GetDataAccess();
			var res = await dAccess.SelectElementsAsync(nameOrQuery, fillMethod).ConfigureAwait(false);
			StoreDataAccess(dAccess);
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<IEnumerable<T>> SelectElementsAsync<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod = null)
		{
			var dAccess = GetDataAccess();
			var res = await dAccess.SelectElementsAsync(nameOrQuery, parameters, fillMethod).ConfigureAwait(false);
			StoreDataAccess(dAccess);
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<IEnumerable<T>> SelectElementsAsync<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod = null)
		{
			var dAccess = GetDataAccess();
			var res = await dAccess.SelectElementsAsync(nameOrQuery, parameters, fillMethod).ConfigureAwait(false);
			StoreDataAccess(dAccess);
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<T> SelectElementAsync<T>(string nameOrQuery, FillDataDelegate<T> fillMethod = null)
		{
			var dAccess = GetDataAccess();
			var res = await dAccess.SelectElementAsync(nameOrQuery, fillMethod).ConfigureAwait(false);
			StoreDataAccess(dAccess);
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<T> SelectElementAsync<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod = null)
		{
			var dAccess = GetDataAccess();
			var res = await dAccess.SelectElementAsync(nameOrQuery, parameters, fillMethod).ConfigureAwait(false);
			StoreDataAccess(dAccess);
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<T> SelectElementAsync<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod = null)
		{
			var dAccess = GetDataAccess();
			var res = await dAccess.SelectElementAsync(nameOrQuery, parameters, fillMethod).ConfigureAwait(false);
			StoreDataAccess(dAccess);
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<int> ExecuteNonQueryAsync(string nameOrQuery, IDictionary<string, object> parameters = null)
		{
			var dAccess = GetDataAccess();
			var res = await dAccess.ExecuteNonQueryAsync(nameOrQuery, parameters).ConfigureAwait(false);
			StoreDataAccess(dAccess);
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<int> ExecuteNonQueryAsync(string nameOrQuery, object parameters)
		{
			var dAccess = GetDataAccess();
			var res = await dAccess.ExecuteNonQueryAsync(nameOrQuery, parameters).ConfigureAwait(false);
			StoreDataAccess(dAccess);
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<T> SelectScalarAsync<T>(string nameOrQuery, IDictionary<string, object> parameters = null)
		{
			var dAccess = GetDataAccess();
			var res = await dAccess.SelectScalarAsync<T>(nameOrQuery, parameters).ConfigureAwait(false);
			StoreDataAccess(dAccess);
			return res;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public async Task<T> SelectScalarAsync<T>(string nameOrQuery, object parameters)
		{
			var dAccess = GetDataAccess();
			var res = await dAccess.SelectScalarAsync<T>(nameOrQuery, parameters).ConfigureAwait(false);
			StoreDataAccess(dAccess);
			return res;
		}
        #endregion

    }
}
