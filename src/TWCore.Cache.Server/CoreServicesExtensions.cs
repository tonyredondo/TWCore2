﻿/*
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
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Serialization;
using TWCore.Services.Configuration;
using TWCore.Settings;

namespace TWCore.Services
{
    /// <summary>
    /// Core Services Extensions
    /// </summary>
    public static class CoreServicesExtensions
    {
        static CacheConfiguration _cacheConfiguration = null;
        static ServerOptions _serverOptions = null;
        static bool _init = false;

        #region Init
        static void Init()
        {
            if (_init) return;
            _init = true;

            var cacheSettings = Core.GetSettings<CacheConfigurationSettings>();
            if (string.IsNullOrEmpty(cacheSettings.ConfigFile)) return;
            if (string.IsNullOrEmpty(cacheSettings.ServerName)) return;

            //Cache configuration
            var cachesConfigFile = cacheSettings.ConfigFile;
            cachesConfigFile = cachesConfigFile.Replace("{EnvironmentName}", Core.EnvironmentName);
            cachesConfigFile = cachesConfigFile.Replace("{MachineName}", Core.MachineName);
            cachesConfigFile = cachesConfigFile.Replace("{ApplicationName}", Core.ApplicationName);
            Core.Log.InfoBasic("Loading cache server options configuration: {0}", cachesConfigFile);

            CacheSettings serverCacheSettings;
            try
            {
                var value = cachesConfigFile.ReadTextFromFile();
                value = Core.ReplaceSettingsTemplate(value);
                var serializer = SerializerManager.GetByFileName<ITextSerializer>(cachesConfigFile);
                serverCacheSettings = serializer.DeserializeFromString<CacheSettings>(value);
            }
            catch (Exception ex)
            {
                throw new Exception($"The Cache config file: {cachesConfigFile} can't be deserialized.", ex);
            }

            if (serverCacheSettings == null)
            {
                Core.Log.Warning("The Cache configuration file is null or empty.");
                return;
            }

            _cacheConfiguration = serverCacheSettings?.Caches?.FirstOrDefault(c => c.Name == cacheSettings.ServerName);
            _serverOptions =
                    _cacheConfiguration?.ServerOptionsList?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true) ??
                    _cacheConfiguration?.ServerOptionsList?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true) ??
                    _cacheConfiguration?.ServerOptionsList?.FirstOrDefault(c => c.EnvironmentName.IsNullOrWhitespace());
        }
        #endregion

        #region CacheServerConfiguration
        /// <summary>
        /// Gets the cache server configuration loaded from config
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CacheConfiguration GetDefaultCacheServerConfiguration(this CoreServices services)
        {
            Init();
            return _cacheConfiguration;
        }
        #endregion

        #region CacheServerOptions
        /// <summary>
        /// Gets the cache server options loaded from config
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ServerOptions GetDefaultCacheServerOptions(this CoreServices services)
        {
            Init();
            return _serverOptions;
        }
        #endregion

        #region Nested Settings Type
        [SettingsContainer("Core.Services.Cache")]
        class CacheConfigurationSettings : SettingsBase
        {
            public string ConfigFile { get; set; }
            public string ServerName { get; set; }
        }
        #endregion
    }
}
