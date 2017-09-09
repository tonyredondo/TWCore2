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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TWCore.Cache;
using TWCore.Net.Multicast;
using TWCore.Net.RPC.Server;
using TWCore.Services.Configuration;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Default Cache Service
    /// </summary>
    public class DefaultCacheService : CacheService
    {
        private ServerOptions _serverOptions;

        /// <inheritdoc />
        /// <summary>
        /// Gets the cache storage manager
        /// </summary>
        /// <returns>StorageManager instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override StorageManager GetManager()
        {
            if (_serverOptions == null)
                _serverOptions = Core.Services.GetDefaultCacheServerOptions();
            Ensure.ReferenceNotNull(_serverOptions, "The Cache server configuration couldn't be loaded. Please check your configuration files.");
            Ensure.ReferenceNotNull(_serverOptions.StorageStack, "The Cache server was loaded but the StorageStack is not defined. Please check your configuration files.");
            return _serverOptions.StorageStack.GetStorageManager();
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the cache server transports
        /// </summary>
        /// <returns>ITransportServer[] instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ITransportServer[] GetTransports()
        {
            if (_serverOptions == null)
                _serverOptions = Core.Services.GetDefaultCacheServerOptions();
            Ensure.ReferenceNotNull(_serverOptions, "The Cache server configuration couldn't be loaded. Please check your configuration files.");
            Ensure.ReferenceNotNull(_serverOptions.Transports, "The Cache server configuration was loaded but there is not TransportServers defined. Please check your configuration files.");
            var lstTransports = new List<ITransportServer>();
            foreach(var transportItem in _serverOptions.Transports)
            {
                if (!transportItem.Enabled) continue;
                var parameters = new Dictionary<string, object>
                {
                    ["Transport.TypeFactory"] = transportItem.TypeFactory,
                };
                if (transportItem.Parameters != null)
                {
                    foreach (var paramItem in transportItem.Parameters)
                    {
                        if (!parameters.ContainsKey(paramItem.Key))
                            parameters[paramItem.Key] = paramItem.Value;
                    }
                }
                var transport = transportItem.CreateInstance<ITransportServer>();
                parameters["Transport.Name"] = transport.Name;
                lstTransports.Add(transport);
                DiscoveryService.RegisterService(DiscoveryService.AppCategory, nameof(DefaultCacheService), $"{Core.ApplicationName} Cache Service", parameters);
            }
            return lstTransports.ToArray();
        }
    }
}
