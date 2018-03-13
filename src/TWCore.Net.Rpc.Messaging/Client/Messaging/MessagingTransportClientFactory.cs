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
using TWCore.Services;
// ReSharper disable CheckNamespace

namespace TWCore.Net.RPC.Client.Transports
{
    /// <inheritdoc />
    /// <summary>
    /// Messaging Transport client factory
    /// </summary>
    public class MessagingTransportClientFactory : TransportClientFactoryBase
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
            var queueClientName = parameters["QueueClientName"];
            var serializerMimeType = parameters["SerializerMimeType"];
            var compressorEncoding = parameters["CompressorEncoding"];
            var serializer = SerializerManager.GetByMimeType(serializerMimeType);
            if (serializer != null && compressorEncoding.IsNotNullOrEmpty())
            {
                var compressor = CompressorManager.GetByEncodingType(compressorEncoding);
                if (compressor != null)
                    serializer.Compressor = compressor;
            }
            Core.Log.LibDebug("Creating a new MessagingTransportServer with the parameters:");
            Core.Log.LibDebug("\tQueueClientName: {0}", queueClientName);
            if (serializerMimeType != null)
            {
                Core.Log.LibDebug("\tSerializer: {0}", serializer);
                if (serializer?.Compressor != null)
                    Core.Log.LibDebug("\tCompressorEncoding: {0}", compressorEncoding);
            }
            Ensure.ArgumentNotNull(queueClientName, "The QueueClientName parameter in the MessagingTransportClientFactory can't be null.");
            var queueClient = Core.Services.GetQueueClient(queueClientName);
            return new MessagingTransportClient(queueClient, serializer);
        }
    }
}