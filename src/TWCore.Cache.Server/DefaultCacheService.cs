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

using System.Linq;
using System.Runtime.CompilerServices;
using TWCore.Cache;
using TWCore.Net.RPC.Server;
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
        /// <inheritdoc />
        /// <summary>
        /// Gets the cache storage manager
        /// </summary>
        /// <returns>StorageManager instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override StorageManager GetManager()
            => Core.Services.GetDefaultCacheServerOptions().StorageStack.GetStorageManager();
        /// <inheritdoc />
        /// <summary>
        /// Gets the cache server transports
        /// </summary>
        /// <returns>ITransportServer[] instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ITransportServer[] GetTransports()
            => Core.Services.GetDefaultCacheServerOptions().Transports.Select(t => t.CreateInstance<ITransportServer>()).ToArray();
    }
}
