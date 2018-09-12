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
        private static CacheSettings _serverCacheSettings;
        private static CacheConfiguration _defaultCacheConfiguration;
        private static ServerOptions _defaultServerOptions;
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
            cachesConfigFile = Factory.ResolveLowLowFilePath(cachesConfigFile);
            Core.Log.InfoBasic("Loading cache server configuration: {0}", cachesConfigFile);

            try
            {
                var value = cachesConfigFile.ReadTextFromFile();
                value = Core.ReplaceSettingsTemplate(value);
                value = Core.ReplaceEnvironmentTemplate(value);
                var serializer = SerializerManager.GetByFileName<ITextSerializer>(cachesConfigFile);
                _serverCacheSettings = serializer.DeserializeFromString<CacheSettings>(value);
            }
            catch (Exception ex)
            {
                throw new Exception($"The Cache config file: {cachesConfigFile} can't be deserialized.", ex);
            }

            if (_serverCacheSettings is null)
            {
                Core.Log.Warning("The Cache configuration file is null or empty.");
                return;
            }

            _defaultCacheConfiguration = _serverCacheSettings.Caches?.FirstOrDefault((c, cSettings) => c.Name == (cSettings.ServerName ?? Core.ApplicationName), cacheSettings);
            _defaultServerOptions = _defaultCacheConfiguration?.ServerOptionsList?.FirstOf(
                c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true,
                c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true,
                c => c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true,
                c => c.EnvironmentName.IsNullOrWhitespace());
        }
        #endregion

        #region CacheServerConfiguration
        /// <summary>
        /// Gets the cache server configuration loaded from config
        /// </summary>
        /// <param name="services">CoreServices instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CacheConfiguration GetDefaultCacheServerConfiguration(this CoreServices services)
        {
            Init();
            return _defaultCacheConfiguration;
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
            _defaultCacheConfiguration = configuration;
        }
        /// <summary>
        /// Gets the cache server configuration loaded from config
        /// </summary>
        /// <param name="services">CoreServices instance</param>
        /// <param name="name">Name of cache configuration</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CacheConfiguration GetCacheServerConfiguration(this CoreServices services, string name)
        {
            Init();
            return _serverCacheSettings.Caches?.FirstOrDefault((c, mName) => c.Name == mName, name);
        }
        #endregion

        #region CacheServerOptions
        /// <summary>
        /// Gets the cache server options loaded from config
        /// </summary>
        /// <param name="services">CoreServices instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ServerOptions GetDefaultCacheServerOptions(this CoreServices services)
        {
            Init();
            return _defaultServerOptions;
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
            _defaultServerOptions = options;
        }
        /// <summary>
        /// Gets the cache server options loaded from config
        /// </summary>
        /// <param name="services">CoreServices instance</param>
        /// <param name="name">Name of cache configuration</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ServerOptions GetCacheServerOptions(this CoreServices services, string name)
        {
            Init();
            var cConfig = _serverCacheSettings.Caches?.FirstOrDefault((c, mName) => c.Name == mName, name);
            return cConfig?.ServerOptionsList?.FirstOf(
                c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true,
                c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true,
                c => c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true,
                c => c.EnvironmentName.IsNullOrWhitespace());
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
