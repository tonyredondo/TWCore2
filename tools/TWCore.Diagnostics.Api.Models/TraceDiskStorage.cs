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

// ReSharper disable UnusedMember.Global

using System;
using System.IO;
using System.Threading.Tasks;
using TWCore.Diagnostics.Api.Models.Trace;

namespace TWCore.Diagnostics
{
    /// <summary>
    /// Trace disk storage
    /// </summary>
    public static class TraceDiskStorage
    {
        private static readonly DiagnosticsSettings Settings = Core.GetSettings<DiagnosticsSettings>();

        /// <summary>
        /// Store trace data to disk
        /// </summary>
        /// <param name="traceItem">Trace item</param>
        /// <param name="traceData">Trace data</param>
        /// <param name="extension">Trace extension</param>
        public static async Task StoreAsync(NodeTraceItem traceItem, Stream traceData, string extension)
        {
            var filePath = GetTracePath(traceItem, extension, true);
            using (var fStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                await traceData.CopyToAsync(fStream).ConfigureAwait(false);
            Core.Log.InfoBasic("The TraceData '{0}' was stored", filePath);
        }
        /// <summary>
        /// Get trace data from disk
        /// </summary>
        /// <param name="traceItem">Trace item</param>
        /// <param name="extension">Trace extension</param>
        /// <returns>Trace data</returns>
        public static async Task<MultiArray<byte>> GetAsync(NodeTraceItem traceItem, string extension)
        {
            var filePath = GetTracePath(traceItem, extension, false);
            using (var fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var data = await fStream.ReadAllBytesAsync().ConfigureAwait(false);
                Core.Log.InfoBasic("The TraceData '{0}' was loaded", filePath);
                return data;
            }
        }

        private static string GetTracePath(NodeTraceItem traceItem, string extension, bool createIfNotExists)
        {
            if (Settings.TracesFolderPath == null)
                throw new Exception("The TraceFolderPath diagnostics settings is not set.");
            if (Settings.TracesFolderPath == string.Empty)
                Settings.TracesFolderPath = Environment.CurrentDirectory;
            if (traceItem == null)
                throw new Exception("The TraceItem is null");

            var folderPath = Path.Combine(Settings.TracesFolderPath, traceItem.Environment, traceItem.Timestamp.ToString("yyyy-MM-dd"), traceItem.Group);
            folderPath = folderPath.RemovePathInvalidChars();

            if (!Directory.Exists(folderPath))
            {
                if (createIfNotExists)
                    Directory.CreateDirectory(folderPath);
                else
                    throw new Exception($"Trace data folder '{folderPath}' doesn't exist.");
            }
            var idValue = string.Empty;
            if (traceItem.TraceId != Guid.Empty)
                idValue = $"[{traceItem.TraceId.ToString()}]";
            else
                idValue = $"[{traceItem.Timestamp.TimeOfDay.ToString()}]";

            var fileName = traceItem.Name + idValue + extension.ToLowerInvariant();
            fileName = fileName.RemoveFileNameInvalidChars();
            var filePath = Path.Combine(folderPath, fileName);

            if (!createIfNotExists)
            {
                if (!File.Exists(filePath))
                {
                    fileName = traceItem.Name + extension.ToLowerInvariant();
                    fileName = fileName.RemoveFileNameInvalidChars();
                    filePath = Path.Combine(folderPath, fileName);
                    return filePath;
                }
            }

            return filePath;
        }
    }
}