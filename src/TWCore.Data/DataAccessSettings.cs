﻿/*
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

using TWCore.Settings;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace TWCore.Data
{
    /// <inheritdoc />
    /// <summary>
    /// Default configuration values for SqlDataAccess classes
    /// </summary>
    public class DataAccessSettings : SettingsBase
    {
        /// <summary>
        /// Command timeout
        /// </summary>
        public int CommandTimeout { get; set; } = 120;
        /// <summary>
        /// Cache for columns by name or query in seconds. Default Value 300 sec = 5 min
        /// </summary>
        public int ColumnsByNameOrQueryCacheInSec { get; set; } = 300;
    }
}
