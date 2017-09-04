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
// ReSharper disable InconsistentNaming

namespace TWCore.Net.RPC.Server.Transports
{
    /// <summary>
    /// TW RPC Transport server factory
    /// </summary>
    public class TWTransportServerFactory : TransportServerFactoryBase
    {
        /// <summary>
        /// Create a new Transport from a KeyValueCollection parameters
        /// </summary>
        /// <param name="parameters">Parameters to create the transport</param>
        /// <returns>Transport instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ITransportServer CreateTransport(KeyValueCollection parameters)
        {
            var port = parameters["Port"].ParseTo(0);
            var serializerMimeType = parameters["SerializerMimeType"];
            var compressorEncoding = parameters["CompressorEncoding"];
            var disconnectedSessionTimeoutInMinutes = parameters["DisconnectedSessionTimeoutInMinutes"].ParseTo(30);
            var serializer = SerializerManager.GetByMimeType(serializerMimeType);
            if (compressorEncoding.IsNotNullOrEmpty())
            {
                var compressor = CompressorManager.GetByEncodingType(compressorEncoding);
                if (compressor != null)
                    serializer.Compressor = compressor;
            }
            var twServer = new TWTransportServer(port, serializer);
            if (disconnectedSessionTimeoutInMinutes > 0)
                twServer.DisconnectedSessionTimeoutInMinutes = disconnectedSessionTimeoutInMinutes;
            Core.Log.LibDebug("TWTransportServer created with parameters:");
            Core.Log.LibDebug("Port: {0}", port);
            if (serializerMimeType != null)
            {
                Core.Log.LibDebug("SerializerMimeType: {0}", serializerMimeType);
                if (serializer?.Compressor != null)
                    Core.Log.LibDebug("CompressorEncoding: {0}", compressorEncoding);
            }
            Core.Log.LibDebug("DisconnectedSessionTimeoutInMinutes: {0}", disconnectedSessionTimeoutInMinutes);
            return twServer;
        }
    }
}
