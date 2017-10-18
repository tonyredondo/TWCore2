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
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Serialization;
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace TWCore.Net.RPC.Client.Transports.TW
{
    /// <inheritdoc />
    /// <summary>
    /// TW RPC Transport client factory
    /// </summary>
    public class TWTransportClientFactory : TransportClientFactoryBase
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
            var port = parameters["Port"].ParseTo(0);
            var timeout = parameters["Timeout"].ParseTo(10000);
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
            var lclient = new TWTransportClient(host, port, socketsPerClient, serializer)
            {
                InvokeMethodTimeout = timeout
            };
            ITransportClient client = lclient;
            Core.Log.LibDebug("Creating a new TWTransportClient with parameters:");
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
