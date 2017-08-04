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
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Net;
using TWCore.Net.RPC.Client;
using TWCore.Serialization;

namespace TWCore.Cache.Client.Configuration
{
    /// <summary>
    /// Cache Servers Settings
    /// </summary>
    [DataContract]
    public class CacheSettings
    {
        /// <summary>
        /// Cache settings
        /// </summary>
        [XmlElement("Cache"), DataMember]
        public NameCollection<CacheConfiguration> Caches { get; set; } = new NameCollection<CacheConfiguration>(false);

        /// <summary>
        /// Gets the cache client connection pool
        /// </summary>
        /// <param name="name">Cache name</param>
        /// <returns>Cache client pool</returns>
        public CacheClientPool GetCacheClient(string name)
        {
            if (Caches?.Contains(name) == true)
            {
                var cacheConfig = Caches[name];
                var cConfig = cacheConfig.ClientOptionsList?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true) ??
                    cacheConfig.ClientOptionsList?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true) ??
                    cacheConfig.ClientOptionsList?.FirstOrDefault(c => c.EnvironmentName.IsNullOrWhitespace());

                if (cConfig?.Pool != null)
                {
                    var pingDelay = cConfig.Pool.PingDelay.ParseTo(5000);
                    var pingDelayOnError = cConfig.Pool.PingDelayOnError.ParseTo(30000);
                    var readMode = cConfig.Pool.ReadMode;
                    var writeMode = cConfig.Pool.WriteMode;
                    var selectionOrder = cConfig.Pool.SelectionOrder;
                    var indexOrder = cConfig.Pool.IndexOrder;
                    var forceNetworkItem = cConfig.Pool.ForceAtLeastOneNetworkItemEnabled;
                    ISerializer serializer = null;
                    if (cConfig.Pool.SerializerMimeType.IsNotNullOrEmpty())
                    {
                        serializer = SerializerManager.GetByMimeType(cConfig.Pool.SerializerMimeType);
                        if (cConfig.Pool.CompressorEncoding.IsNotNullOrEmpty())
                            serializer.Compressor = CompressorManager.GetByEncodingType(cConfig.Pool.CompressorEncoding);
                    }
                    else
                        serializer = SerializerManager.DefaultBinarySerializer;
                    var ccp = new CacheClientPool(pingDelay, pingDelayOnError, readMode, writeMode, selectionOrder, indexOrder);
                    ccp.Serializer = serializer;
                    ccp.ForceAtLeastOneNetworkItemEnabled = forceNetworkItem;

                    if (cConfig.Pool.Items?.Any() == true)
                    {
                        var idx = 0;
                        foreach (var pitem in cConfig.Pool.Items)
                        {
                            idx++;
                            if (!pitem.Enabled)
                                continue;

                            var hostParam = pitem.Parameters?.FirstOrDefault(p => p.Key == "Host");
                            var portParam = pitem.Parameters?.FirstOrDefault(p => p.Key == "Port");

                            if (!pitem.InMemoryStorage)
                            {
                                if (cacheConfig.UseSharedMemoryOnLocal)
                                {
                                    if (hostParam != null)
                                    {
                                        if (IpHelper.IsLocalhost(hostParam.Value))
                                        {
                                            var type = "TWCore.Net.RPC.Client.Configuration.TwoWay.SharedMemoryTwoWayTransportClientFactory, TWCore.Desktop.Net.RPC.Client.TwoWay";
                                            var _type = Core.GetType(type, false);
                                            if (_type != null)
                                            {
                                                //TODO: 
                                                Core.Log.Warning("The UseSharedMemoryOnLocal Flag was activated, this is an alpha feature and is not to be used on production environment.");

                                                pitem.TypeFactory = type;
                                                pitem.Parameters = new KeyValueCollection
                                                {
                                                    new KeyValue<string, string> { Key = "MappedFileName", Value = cacheConfig.Name }
                                                };
                                                pitem.Name = "SharedMemoryTwoWayTransportClient";
                                            }
                                            else
                                            {
                                                Core.Log.Warning("The Flag UseSharedMemoryOnLocal was activated, but the type {0} couldn't be loaded, check if the assymbly exist on your bin folder.", type);
                                            }
                                        }
                                    }
                                }
                                var transport = pitem.CreateInstance<ITransportClient>();
                                var proxy = CacheClientProxy.GetClient(transport);
                                var cppName = Core.EnvironmentName + "." + Core.MachineName + "." + name + ".PoolItem(" + transport.GetType().Name + "-" + hostParam?.Value + "-" + portParam?.Value + ")." + idx;
                                ccp.Add(cppName, proxy, pitem.Mode, false);
                            }
                            else
                            {
                                var sto = pitem.CreateInstance<StorageBase>();
                                var cppName = Core.EnvironmentName + "." + Core.MachineName + "." + name + ".MStorage." + idx;
                                ccp.Add(cppName, sto, StorageItemMode.ReadAndWrite, true);
                            }
                        }
                    }
                    return ccp;
                }

            }

            return null;
        }
        /// <summary>
        /// Gets the cache client connection pool
        /// </summary>
        /// <param name="name">Cache name</param>
        /// <returns>Cache client pool</returns>
        public async Task<CacheClientPoolAsync> GetCacheClientAsync(string name)
        {
            if (Caches?.Contains(name) == true)
            {
                var cacheConfig = Caches[name];
                var cConfig = cacheConfig.ClientOptionsList?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true) ??
                    cacheConfig.ClientOptionsList?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true) ??
                    cacheConfig.ClientOptionsList?.FirstOrDefault(c => c.EnvironmentName.IsNullOrWhitespace());

                if (cConfig?.Pool != null)
                {
                    var pingDelay = cConfig.Pool.PingDelay.ParseTo(5000);
                    var pingDelayOnError = cConfig.Pool.PingDelayOnError.ParseTo(30000);
                    var readMode = cConfig.Pool.ReadMode;
                    var writeMode = cConfig.Pool.WriteMode;
                    var selectionOrder = cConfig.Pool.SelectionOrder;
                    var indexOrder = cConfig.Pool.IndexOrder;
                    var forceNetworkItem = cConfig.Pool.ForceAtLeastOneNetworkItemEnabled;
                    ISerializer serializer = null;
                    if (cConfig.Pool.SerializerMimeType.IsNotNullOrEmpty())
                    {
                        serializer = SerializerManager.GetByMimeType(cConfig.Pool.SerializerMimeType);
                        if (cConfig.Pool.CompressorEncoding.IsNotNullOrEmpty())
                            serializer.Compressor = CompressorManager.GetByEncodingType(cConfig.Pool.CompressorEncoding);
                    }
                    else
                        serializer = SerializerManager.DefaultBinarySerializer;
                    var ccp = new CacheClientPoolAsync(pingDelay, pingDelayOnError, readMode, writeMode, selectionOrder, indexOrder);
                    ccp.Serializer = serializer;
                    ccp.ForceAtLeastOneNetworkItemEnabled = forceNetworkItem;

                    if (cConfig.Pool.Items?.Any() == true)
                    {
                        var idx = 0;
                        foreach (var pitem in cConfig.Pool.Items)
                        {
                            idx++;
                            if (!pitem.Enabled)
                                continue;

                            var hostParam = pitem.Parameters?.FirstOrDefault(p => p.Key == "Host");
                            var portParam = pitem.Parameters?.FirstOrDefault(p => p.Key == "Port");
                            if (!pitem.InMemoryStorage)
                            {
                                if (cacheConfig.UseSharedMemoryOnLocal)
                                {
                                    if (hostParam != null)
                                    {
                                        if (IpHelper.IsLocalhost(hostParam.Value))
                                        {
                                            var type = "TWCore.Net.RPC.Client.Configuration.TwoWay.SharedMemoryTwoWayTransportClientFactory, TWCore.Desktop.Net.RPC.Client.TwoWay";
                                            var _type = Core.GetType(type, false);
                                            if (_type != null)
                                            {
                                                //TODO: 
                                                Core.Log.Warning("The UseSharedMemoryOnLocal Flag was activated, this is an alpha feature and is not to be used on production environment.");

                                                pitem.TypeFactory = type;
                                                pitem.Parameters = new KeyValueCollection
                                                {
                                                    new KeyValue<string, string> { Key = "MappedFileName", Value = cacheConfig.Name }
                                                };
                                                pitem.Name = "SharedMemoryTwoWayTransportClient";
                                            }
                                            else
                                            {
                                                Core.Log.Warning("The Flag UseSharedMemoryOnLocal was activated, but the type {0} couldn't be loaded, check if the assymbly exist on your bin folder.", type);
                                            }
                                        }
                                    }
                                }
                                var transport = pitem.CreateInstance<ITransportClient>();
                                var proxy = await CacheClientProxy.GetClientAsync(transport).ConfigureAwait(false);
                                var cppName = Core.EnvironmentName + "." + Core.MachineName + "." + name + ".PoolItem(" + transport.GetType().Name + "-" + hostParam?.Value + "-" + portParam?.Value + ")." + idx;
                                ccp.Add(cppName, (IStorageAsync)proxy, pitem.Mode, false);
                            }
                            else
                            {
                                var sto = pitem.CreateInstance<StorageBase>();
                                var cppName = Core.EnvironmentName + "." + Core.MachineName + "." + name + ".MStorage." + idx;
                                ccp.Add(cppName, sto, StorageItemMode.ReadAndWrite, true);
                            }
                        }
                    }
                    return ccp;
                }
            }
            return null;
        }
    }
}
