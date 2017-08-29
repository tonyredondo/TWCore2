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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Data.Schema;
using TWCore.Security;

namespace TWCore.Data
{
    /// <summary>
    /// Entity Dal base class
    /// </summary>
    public abstract class EntityDal : IEntityDal
    {
        static ConcurrentDictionary<string, ObjectPool<IDataAccess>> Pools = new ConcurrentDictionary<string, ObjectPool<IDataAccess>>();
		static ConcurrentDictionary<string, ObjectPool<IDataAccessAsync>> AsyncPools = new ConcurrentDictionary<string, ObjectPool<IDataAccessAsync>>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        EntityDalSettings _settings = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        DalPoolItem _poolItem = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        DalPoolItemAsync _poolAsyncItem = null;

        #region Properties
        /// <summary>
        /// Entity Settings
        /// </summary>
        public EntityDalSettings Settings
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_settings == null)
                    _settings = OnGetSettings();
                return _settings;
            }
        }
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
            var _pool = Pools.GetOrAdd(poolKey, key => new ObjectPool<IDataAccess>(pool => OnGetDataAccess(Settings)));
			var _poolAsync = AsyncPools.GetOrAdd(poolKey, key => new ObjectPool<IDataAccessAsync>(pool => OnGetDataAccessAsync(Settings)));
            _poolItem = new DalPoolItem(_pool);
            _poolAsyncItem = new DalPoolItemAsync(_poolAsync);
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
