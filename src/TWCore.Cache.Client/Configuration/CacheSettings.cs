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

using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Net.RPC.Client.Transports;
using TWCore.Serialization;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

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
        public async Task<CacheClientPoolAsync> GetCacheClientAsync(string name)
        {
            if (Caches?.Contains(name) != true) return null;
            var cacheConfig = Caches[name];
            var cConfig = cacheConfig.ClientOptionsList?.FirstOf(
                c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true,
                c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true,
                c => c.EnvironmentName.IsNullOrWhitespace());

            if (cConfig?.Pool == null) return null;
            var pingDelay = cConfig.Pool.PingDelay.ParseTo(5000);
            var pingDelayOnError = cConfig.Pool.PingDelayOnError.ParseTo(30000);
            var readMode = cConfig.Pool.ReadMode;
            var writeMode = cConfig.Pool.WriteMode;
            var selectionOrder = cConfig.Pool.SelectionOrder;
            var indexOrder = cConfig.Pool.IndexOrder;
            var forceNetworkItem = cConfig.Pool.ForceAtLeastOneNetworkItemEnabled;
            ISerializer serializer;
            if (cConfig.Pool.SerializerMimeType.IsNotNullOrEmpty())
            {
                serializer = SerializerManager.GetByMimeType(cConfig.Pool.SerializerMimeType);
                Ensure.ReferenceNotNull(serializer, $"The Serializer \"{cConfig.Pool.SerializerMimeType}\" couldn't be loaded.");
                if (cConfig.Pool.CompressorEncoding.IsNotNullOrEmpty())
                    serializer.Compressor = CompressorManager.GetByEncodingType(cConfig.Pool.CompressorEncoding);
            }
            else
                serializer = SerializerManager.DefaultBinarySerializer;
            var ccp = new CacheClientPoolAsync(pingDelay, pingDelayOnError, readMode, writeMode, selectionOrder, indexOrder)
            {
                Serializer = serializer,
                ForceAtLeastOneNetworkItemEnabled = forceNetworkItem
            };

            if (cConfig.Pool.Items?.Any() != true) return ccp;
            
            var idx = 0;
            foreach (var pitem in cConfig.Pool.Items)
            {
                idx++;
                if (!pitem.Enabled)
                    continue;

                var objType = pitem.CreateInstance<object>();
                switch (objType)
                {
                    case ITransportClient transport:
                    {
                        var hostParam = pitem.Parameters?.FirstOrDefault(p => p.Key == "Host");
                        var portParam = pitem.Parameters?.FirstOrDefault(p => p.Key == "Port");

                        var proxy = await CacheClientProxy.GetClientAsync(transport).ConfigureAwait(false);
                        var cppName = Core.EnvironmentName + "." + Core.MachineName + "." + name + ".Storage(" + transport.GetType().Name + "-" + hostParam?.Value + "-" + portParam?.Value + ")." + idx;
                        ccp.Add(cppName, (IStorageAsync)proxy, pitem.Mode);
                        break;
                    }
                    case StorageBase sto:
                    {
                        var cppName = Core.EnvironmentName + "." + Core.MachineName + "." + name + ".Storage(" + sto.Type + ")." + idx;
                        ccp.Add(cppName, sto);
                        break;
                    }
                }
            }
            return ccp;
        }
    }
}
