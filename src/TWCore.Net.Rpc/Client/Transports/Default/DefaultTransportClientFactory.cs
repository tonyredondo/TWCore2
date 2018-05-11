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

using System.Runtime.CompilerServices;
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Serialization;

namespace TWCore.Net.RPC.Client.Transports.Default
{
    /// <inheritdoc />
    /// <summary>
    /// Default RPC Transport client factory
    /// </summary>
    public class DefaultTransportClientFactory : TransportClientFactoryBase
    {
        /// <inheritdoc />
        /// <summary>
        /// Create a new Transport from a KeyValueCollection parameters
        /// </summary>
        /// <param name="parameters">Parameters to create the transport</param>
        /// <returns>Transport instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ITransportClient CreateTransport(KeyValueCollection parameters)
        {
            var host = parameters["Host"];
            if (host == null || host == Factory.SkipInstanceValue)
            {
                Core.Log.Warning("Skipping transport instance by Host value.");
                return null;
            }
            var strPort = parameters["Port"];
            if (strPort == null || strPort == Factory.SkipInstanceValue)
            {
                Core.Log.Warning("Skipping transport instance by Port value.");
                return null;
            }
            var port = strPort.ParseTo(0);

            var timeout = parameters["Timeout"].ParseTo(20000);
            var serializerMimeType = parameters["SerializerMimeType"];
            var socketsPerClient = parameters["SocketsPerClient"].ParseTo<byte>(1);
            var compressorEncoding = parameters["CompressorEncoding"];
            var serializer = SerializerManager.GetByMimeType(serializerMimeType);
            if (compressorEncoding.IsNotNullOrEmpty())
            {
                var compressor = CompressorManager.GetByEncodingType(compressorEncoding);
                if (compressor != null)
                    serializer.Compressor = compressor;
            }
            var lclient = new DefaultTransportClient(host, port, socketsPerClient, serializer)
            {
                InvokeMethodTimeout = timeout
            };
            ITransportClient client = lclient;
            Core.Log.LibDebug("Creating a new DefaultTransportClient with parameters:");
            Core.Log.LibDebug("\tHost: {0}", host);
            Core.Log.LibDebug("\tPort: {0}", port);
            Core.Log.LibDebug("\tSocketsPerClient: {0}", socketsPerClient);
            if (serializerMimeType == null)
                return client;
            Core.Log.LibDebug("\tSerializer: {0}", serializer);
            if (serializer?.Compressor != null)
                Core.Log.LibDebug("\tCompressorEncoding: {0}", compressorEncoding);
            return client;
        }
    }
}
