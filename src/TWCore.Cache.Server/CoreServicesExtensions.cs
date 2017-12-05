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
using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Serialization;
using TWCore.Services.Configuration;
using TWCore.Settings;
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedParameter.Global

namespace TWCore.Services
{
    /// <summary>
    /// Core Services Extensions
    /// </summary>
    public static class CoreServicesExtensions
    {
        private static CacheConfiguration _cacheConfiguration;
        private static ServerOptions _serverOptions;
        private static bool _init;

		#region Init
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Init()
        {
            if (_init) return;
            _init = true;

            var cacheSettings = Core.GetSettings<CacheConfigurationSettings>();
            if (string.IsNullOrEmpty(cacheSettings.ConfigFile)) return;

            //Cache configuration
            var cachesConfigFile = cacheSettings.ConfigFile;
            cachesConfigFile = cachesConfigFile.Replace("{EnvironmentName}", Core.EnvironmentName);
            cachesConfigFile = cachesConfigFile.Replace("{MachineName}", Core.MachineName);
            cachesConfigFile = cachesConfigFile.Replace("{ApplicationName}", Core.ApplicationName);
            Core.Log.InfoBasic("Loading cache server configuration: {0}", cachesConfigFile);

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

            _cacheConfiguration = serverCacheSettings.Caches?.FirstOrDefault(c => c.Name == (cacheSettings.ServerName ?? Core.ApplicationName));
            _serverOptions =
                    _cacheConfiguration?.ServerOptionsList?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true) ??
                    _cacheConfiguration?.ServerOptionsList?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true) ??
                    _cacheConfiguration?.ServerOptionsList?.FirstOrDefault(c => c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true) ??
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
        /// <summary>
        /// Sets the default cache server configuration
        /// </summary>
        /// <param name="services">CoreServices instance</param>
        /// <param name="configuration">Cache Configuration instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetDefaultCacheServerConfiguration(this CoreServices services, CacheConfiguration configuration)
        {
            _init = true;
            _cacheConfiguration = configuration;
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
        /// <summary>
        /// Sets the default cache server options
        /// </summary>
        /// <param name="services">CoreServices instance</param>
        /// <param name="options">Cache Server Options instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetDefaultCacheServerOptions(this CoreServices services, ServerOptions options)
        {
            _init = true;
            _serverOptions = options;
        }
        #endregion

        #region Nested Settings Type
        [SettingsContainer("Core.Services.Cache")]
        private class CacheConfigurationSettings : SettingsBase
        {
            public string ConfigFile { get; set; }
            public string ServerName { get; set; }
        }
        #endregion
    }
}
