using System.Runtime.CompilerServices;
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Serialization;
using TWCore.Services;

namespace TWCore.Net.RPC.Server.Transports
{
    /// <inheritdoc />
    /// <summary>
    /// Messaging Transport server factory
    /// </summary>
    public class MessagingTransportServerFactory : TransportServerFactoryBase
    {
        /// <inheritdoc />
        /// <summary>
        /// Create a new Transport from a KeyValueCollection parameters
        /// </summary>
        /// <param name="parameters">Parameters to create the transport</param>
        /// <returns>Transport instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ITransportServer CreateTransport(KeyValueCollection parameters)
        {
            var queueServerName = parameters["QueueServerName"];
            var enableGetDescriptors = parameters["EnableGetDescriptors"].ParseTo(true);
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
            Core.Log.LibDebug("\tQueueServerName: {0}", queueServerName.IsNotNullOrWhitespace() ? 
                queueServerName : 
                "Using Default QueueServer from configuration file.");
            Core.Log.LibDebug("\tEnableGetDescriptors: {0}", enableGetDescriptors);
            if (serializerMimeType != null)
            {
                Core.Log.LibDebug("\tSerializer: {0}", serializer);
                if (serializer?.Compressor != null)
                    Core.Log.LibDebug("\tCompressorEncoding: {0}", compressorEncoding);
            }
            var queueServer = queueServerName.IsNotNullOrWhitespace()
                ? Core.Services.GetQueueServer(queueServerName)
                : Core.Services.GetQueueServer();
            return new MessagingTransportServer(queueServer, serializer);
        }
    }
}