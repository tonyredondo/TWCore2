using TWCore.Diagnostics.Trace;
using TWCore.Diagnostics.Trace.Storages;
using TWCore.Serialization;
using System.Linq;

namespace TWCore
{
    /// <summary>
    /// ITraceEngine extension methods
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Adds a simple file trace storage instance to the trace engine
        /// </summary>
        /// <param name="traceEngine">Trace engine</param>
        /// <param name="basePath">Trace items base path</param>
        /// <param name="serializer">Serializer used for writing the trace item data</param>
        public static void AddSimpleFileStorage(this ITraceEngine traceEngine, string basePath, ISerializer serializer = null)
        {
            if (traceEngine?.Storages?.GetAllStorages()?.Any(s => s is SimpleFileTraceStorage) == false)
                traceEngine.Storages.Add(new SimpleFileTraceStorage(basePath, serializer ?? SerializerManager.DefaultBinarySerializer));
        }
    }
}