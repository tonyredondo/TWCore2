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

using System.Runtime.CompilerServices;

namespace TWCore.Data
{
    /// <summary>
    /// Entity Dal base class
    /// </summary>
    public abstract class EntityDal : IEntityDal
    {
        EntityDalSettings _settings = null;
        IDataAccess _data = null;

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
        /// Data Access
        /// </summary>
        public IDataAccess Data
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_data == null)
                    _data = OnGetDataAccess(Settings);
                return _data;
            }
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
        #endregion
    }
}
