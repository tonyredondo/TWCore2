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
using System.Threading.Tasks;
using TWCore.Cache;
using TWCore.Diagnostics.Status;
using TWCore.Net.RPC.Server;
using TWCore.Net.RPC.Server.Transports;

// ReSharper disable CheckNamespace

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Cache RPC Service
    /// </summary>
    [StatusName("Cache Service")]
    public abstract class CacheService : RPCService
    {
        #region Properties
        /// <summary>
        /// Gets the RPC Server transports
        /// </summary>
        public ITransportServer[] Transports { get; private set; }
        /// <summary>
        /// Gets the cache storage manager
        /// </summary>
        [StatusReference]
        public StorageManager Manager { get; private set; }
        #endregion

        #region Overrides
        /// <inheritdoc />
        /// <summary>
        /// Gets the RPCServer 
        /// </summary>
        /// <returns>RPCServer instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override async Task<RPCServer> GetRPCServerAsync()
        {
            Transports = GetTransports();
            Manager = GetManager();
            Manager.Init();
            var loopCount = 0;
            while (!Manager.IsReady() && loopCount < 30)
            {
                Core.Log.InfoBasic("Waiting for StorageManager Ready state...");
                loopCount++;
                await Task.Delay(1000).ConfigureAwait(false);
            }
            if (!Manager.IsReady())
                Core.Log.Warning("The StorageManager is not on Ready state, some data couldn't be found during this state.");

            if (Transports?.Any() != true)
                throw new NullReferenceException("The server needs to define at least one server transport.");
            
            RPCServer server;
            if (Transports.Length == 1)
                server = new RPCServer(Transports[0]);
            else
            {
                var transport = new TransportServerCollection();
                foreach (var item in Transports)
                    transport.Add(item);
                server = new RPCServer(transport);
            }
            server.SetCounterInfo("Cache Server", Diagnostics.Counters.CounterLevel.Framework, Diagnostics.Counters.CounterKind.Cache);
            server.AddService(typeof(IStorage), Manager);
            server.AddService(typeof(IStorageWithExtensionExecution), Manager);
            return server;
        }
        /// <inheritdoc />
        /// <summary>
        /// On Service Dispose
        /// </summary>
        protected override void OnDispose()
        {
            Manager.Dispose();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Gets the cache storage manager
        /// </summary>
        /// <returns>StorageManager instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract StorageManager GetManager();
        /// <summary>
        /// Gets the cache server transports
        /// </summary>
        /// <returns>ITransportServer[] instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract ITransportServer[] GetTransports();
        #endregion
    }
}
