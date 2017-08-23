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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TWCore.Data
{
    /// <summary>
    /// Dal Pool Item
    /// </summary>
    public class DalPoolItemAsync : IDataAccessAsync, IDisposable
    {
        IDataAccessAsync _dataAccess;
        EntityDalAsync _entityDal;
        internal bool Recycle { get; set; } = true;

        /// <summary>
        /// Dal Pool item
        /// </summary>
        /// <param name="dataAccess">DataAccess instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DalPoolItemAsync(EntityDalAsync entityDal, IDataAccessAsync dataAccess)
        {
            _entityDal = entityDal;
            _dataAccess = dataAccess;
            _dataAccess.OnError += DataAccess_OnError;
        }

        private void DataAccess_OnError(object sender, EventArgs<Exception> e)
            => OnError?.Invoke(sender, e);

        #region IDataAccess
        public event EventHandler<EventArgs<Exception>> OnError;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<IEnumerable<T>> SelectElementsAsync<T>(string nameOrQuery, FillDataDelegate<T> fillMethod = null)
            => _dataAccess.SelectElementsAsync(nameOrQuery, fillMethod);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<IEnumerable<T>> SelectElementsAsync<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod = null)
            => _dataAccess.SelectElementsAsync(nameOrQuery, parameters, fillMethod);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<IEnumerable<T>> SelectElementsAsync<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod = null)
            => _dataAccess.SelectElementsAsync(nameOrQuery, parameters, fillMethod);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> SelectElementAsync<T>(string nameOrQuery, FillDataDelegate<T> fillMethod = null)
            => _dataAccess.SelectElementAsync(nameOrQuery, fillMethod);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> SelectElementAsync<T>(string nameOrQuery, IDictionary<string, object> parameters, FillDataDelegate<T> fillMethod = null)
            => _dataAccess.SelectElementAsync(nameOrQuery, parameters, fillMethod);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> SelectElementAsync<T>(string nameOrQuery, object parameters, FillDataDelegate<T> fillMethod = null)
            => _dataAccess.SelectElementAsync(nameOrQuery, parameters, fillMethod);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> ExecuteNonQueryAsync(string nameOrQuery, IDictionary<string, object> parameters = null)
            => _dataAccess.ExecuteNonQueryAsync(nameOrQuery, parameters);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> ExecuteNonQueryAsync(string nameOrQuery, object parameters)
            => _dataAccess.ExecuteNonQueryAsync(nameOrQuery, parameters);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> SelectScalarAsync<T>(string nameOrQuery, IDictionary<string, object> parameters = null)
            => _dataAccess.SelectScalarAsync<T>(nameOrQuery, parameters);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> SelectScalarAsync<T>(string nameOrQuery, object parameters)
            => _dataAccess.SelectScalarAsync<T>(nameOrQuery, parameters);
        #endregion

        #region IDisposable Support
        internal bool disposedValue = false; // Para detectar llamadas redundantes
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (Recycle)
                    _entityDal?._pool?.Store(this);
                disposedValue = true;
            }
        }
        ~DalPoolItemAsync()
        {
            // No cambie este código. Coloque el código de limpieza en el anterior Dispose(colocación de bool).
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
