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

using TWCore.Settings;
// ReSharper disable MemberCanBeProtected.Global

namespace TWCore.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Entity Dal Settings
    /// </summary>
    public abstract class EntityDalSettings : SettingsBase
    {
        /// <summary>
        /// Command timeout
        /// </summary>
        public int CommandTimeout { get; set; } = 120;
        /// <summary>
        /// Cache for columns by name or query in seconds. Default Value 300 sec = 5 min
        /// </summary>
        public int ColumnsByNameOrQueryCacheInSec { get; set; } = 300;
        /// <summary>
        /// Connection String
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Data access type
        /// </summary>
        public DataAccessType QueryType { get; set; } = DataAccessType.Query;
    }
}
