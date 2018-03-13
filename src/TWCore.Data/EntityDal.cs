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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Data.Schema;
using TWCore.Security;
// ReSharper disable MemberCanBeProtected.Global

namespace TWCore.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Entity Dal base class
    /// </summary>
    public abstract class EntityDal : IEntityDal
    {
        private static readonly NonBlocking.ConcurrentDictionary<string, ObjectPool<IDataAccess>> Pools = new NonBlocking.ConcurrentDictionary<string, ObjectPool<IDataAccess>>();
        private static readonly NonBlocking.ConcurrentDictionary<string, ObjectPool<IDataAccessAsync>> AsyncPools = new NonBlocking.ConcurrentDictionary<string, ObjectPool<IDataAccessAsync>>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EntityDalSettings _settings;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly DalPoolItem _poolItem;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly DalPoolItemAsync _poolAsyncItem;

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Entity Settings
        /// </summary>
        public EntityDalSettings Settings
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _settings ?? (_settings = OnGetSettings());
        }
        /// <inheritdoc />
        /// <summary>
        /// Data Access Pool Item
        /// </summary>
        public IDataAccess Data => _poolItem;
        /// <summary>
        /// DataAsync Access Pool Item
        /// </summary>
        public IDataAccessAsync DataAsync => _poolAsyncItem;
        #endregion

        #region .ctor
        protected EntityDal()
        {
            var poolKey = Settings.GetHashSHA1();
            var pool = Pools.GetOrAdd(poolKey, key => new ObjectPool<IDataAccess>(pl => OnGetDataAccess(Settings)));
			var poolAsync = AsyncPools.GetOrAdd(poolKey, key => new ObjectPool<IDataAccessAsync>(pl => OnGetDataAccessAsync(Settings)));
            _poolItem = new DalPoolItem(pool);
            _poolAsyncItem = new DalPoolItemAsync(poolAsync);
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// On Get Settings for Entity Dal. Load using: Core.GetSettings
        /// </summary>
        /// <returns>Return EntityDal Settings</returns>
        protected abstract EntityDalSettings OnGetSettings();
        /// <summary>
        /// On Get DataAccess instance.
        /// </summary>
        /// <param name="settings">EntityDal settings</param>
        /// <returns></returns>
        protected abstract IDataAccess OnGetDataAccess(EntityDalSettings settings);
        /// <summary>
        /// On Get DataAccess instance.
        /// </summary>
        /// <param name="settings">EntityDal settings</param>
        /// <returns></returns>
        protected abstract IDataAccessAsync OnGetDataAccessAsync(EntityDalSettings settings);
        #endregion

        #region GetSchema
        /// <summary>
        /// Get Database Schema
        /// </summary>
        /// <returns>Schema</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CatalogSchema GetSchema() => Data.GetSchema();
        /// <summary>
        /// Get Database Schema
        /// </summary>
        /// <returns>Schema</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<CatalogSchema> GetSchemaAsync() => DataAsync.GetSchemaAsync();
        #endregion
    }
}
