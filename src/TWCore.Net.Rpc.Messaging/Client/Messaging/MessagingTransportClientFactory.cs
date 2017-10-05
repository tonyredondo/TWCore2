using System.Runtime.CompilerServices;
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Serialization;
using TWCore.Services;

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