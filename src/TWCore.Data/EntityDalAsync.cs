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
using System.Runtime.CompilerServices;

namespace TWCore.Data
{
    /// <summary>
    /// Entity Dal Async base class
    /// </summary>
    public abstract class EntityDalAsync : IEntityDalAsync
    {
        EntityDalSettings _settings = null;
        internal ObjectPool<DalPoolItemAsync> _pool = null;

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
        #endregion

        public EntityDalAsync()
        {
            _pool = new ObjectPool<DalPoolItemAsync>(pool =>
            {
                return new DalPoolItemAsync(this, OnGetDataAccess(Settings))
                {
                    Recycle = true
                };
            }, item => item.disposedValue = false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DalPoolItemAsync GetDataAccess()
            => _pool.New();


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
        protected abstract IDataAccessAsync OnGetDataAccess(EntityDalSettings settings);
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // Para detectar llamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (_pool != null)
                {
                    foreach (var pItem in _pool.GetCurrentObjects())
                    {
                        pItem.Recycle = false;
                        pItem.Dispose();
                    }
                    _pool.Clear();
                    _pool = null;
                }
                disposedValue = true;
            }
        }

        ~EntityDalAsync()
        {
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
