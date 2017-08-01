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
using TWCore.Services.Configuration;

namespace TWCore.Services
{
    /// <summary>
    /// Core Services Extensions
    /// </summary>
    public static class CoreServicesExtensions
    {
        const string CacheServerConfiguration = "CacheServerConfiguration";
        const string CacheServerOptions = "CacheServerOptions";

        #region CacheServerConfiguration
        /// <summary>
        /// Gets the cache server configuration loaded from config
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CacheConfiguration GetCacheServerConfiguration(this CoreServices services)
        {
            if (services.ServicesData.TryGetValue(CacheServerConfiguration, out var obj))
                return (CacheConfiguration)obj;
            else
                return null;
        }
        /// <summary>
        /// Sets the cache server configuration loaded from config
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetCacheServerConfiguration(this CoreServices services, CacheConfiguration value)
        {
            services.ServicesData[CacheServerConfiguration] = value;
        }
        #endregion

        #region CacheServerOptions
        /// <summary>
        /// Gets the cache server options loaded from config
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ServerOptions GetCacheServerOptions(this CoreServices services)
        {
            if (services.ServicesData.TryGetValue(CacheServerOptions, out var obj))
                return (ServerOptions)obj;
            else
                return null;
        }
        /// <summary>
        /// Sets the cache server options loaded from config
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetCacheServerOptions(this CoreServices services, ServerOptions value)
        {
            services.ServicesData[CacheServerOptions] = value;
        }
        #endregion
    }
}
