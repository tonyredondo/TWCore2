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
using System.Text;
using TWCore.Cache;
using TWCore.Net.RPC.Server;
using TWCore.Serialization;
using TWCore.Services.Configuration;

namespace TWCore.Services
{
    /// <summary>
    /// Cache RPC Service
    /// </summary>
    public abstract class CacheService : RPCService, ICoreStart
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
            RPCServer server;
            if (Transports?.Any() == true)
            {
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
            else
				throw new NullReferenceException("The server needs to define at least one server transport.");
        }
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

        #region ICoreStart
        static bool _init = false;
        /// <summary>
        /// Core Init
        /// </summary>
        /// <param name="factories"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CoreInit(Factories factories)
        {
            if (!_init)
            {
                _init = true;

                //Cache configuration
                var cachesConfigFile = Core.Settings["Core.Services.Cache.ConfigFile"];
                cachesConfigFile = cachesConfigFile?.Replace("{EnvironmentName}", Core.EnvironmentName);
                cachesConfigFile = cachesConfigFile?.Replace("{MachineName}", Core.MachineName);
                cachesConfigFile = cachesConfigFile?.Replace("{ApplicationName}", Core.ApplicationName);
                if (cachesConfigFile.IsNotNullOrWhitespace())
                {
                    Core.Log.InfoBasic("Loading cache server options configuration: {0}", cachesConfigFile);
                    CacheSettings serverCacheSettings;
                    try
                    {
                        var value = cachesConfigFile.ReadTextFromFile(new UTF8Encoding(false));
                        value = Core.ReplaceSettingsTemplate(value);
                        var serializer = SerializerManager.GetByFileName<ITextSerializer>(cachesConfigFile);
                        serverCacheSettings = serializer.DeserializeFromString<CacheSettings>(value);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"The Cache config file: {cachesConfigFile} can't be deserialized.", ex);
                    }
                    if (Core.Settings["Core.Services.Cache.ServerName"].IsNotNullOrWhitespace())
                    {
                        var serverConfiguration = serverCacheSettings?.Caches?.FirstOrDefault(c => c.Name == Core.Settings["Core.Services.Cache.ServerName"]);
                        Core.Services.SetCacheServerConfiguration(serverConfiguration);
                        Core.Services.SetCacheServerOptions(serverConfiguration?.ServerOptionsList?.FirstOrDefault(c => 
                            c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && 
                            c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true) ??
                                serverConfiguration?.ServerOptionsList?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true) ??
                                serverConfiguration?.ServerOptionsList?.FirstOrDefault(c => c.EnvironmentName.IsNullOrWhitespace()));
                    }
                }
            }
        }
        #endregion
    }
}
