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
using TWCore.Cache;
using TWCore.Net.RPC.Server;
using TWCore.Net.RPC.Server.Transports;

// ReSharper disable CheckNamespace

namespace TWCore.Services
{
    /// <inheritdoc />
    /// <summary>
    /// Cache RPC Service
    /// </summary>
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
        public StorageManager Manager { get; private set; }
        #endregion

        #region Overrides
        /// <inheritdoc />
        /// <summary>
        /// Gets the RPCServer 
        /// </summary>
        /// <returns>RPCServer instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override RPCServer GetRPCServer()
        {
            Transports = GetTransports();
            Manager = GetManager();
            Manager.Init();
            var loopCount = 0;
            while (!Manager.IsReady() && loopCount < 30)
            {
                Core.Log.InfoBasic("Waiting for StorageManager Ready state...");
                loopCount++;
                Factory.Thread.Sleep(2000);
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
                Transports.Each(transport.Add);
                server = new RPCServer(transport);
            }
            server.AddService(typeof(IStorage), Manager);
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
