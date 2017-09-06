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

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Compression;

namespace TWCore.Net
{
    /// <inheritdoc />
    /// <summary>
    /// Decompression handler for a System.Net.Http delegating handler
    /// </summary>
    public class DecompressionHandler : DelegatingHandler
    {
        /// <inheritdoc />
        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>Returns System.Threading.Tasks.Task`1. The task object representing the asynchronous operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.Content?.Headers?.ContentEncoding.IsNotNullOrEmpty() != true) return response;
            var encoding = response.Content.Headers.ContentEncoding.First();
            var compressor = CompressorManager.GetByEncodingType(encoding);
            if (compressor != null)
                response.Content = await DecompressContentAsync(response.Content, compressor).ConfigureAwait(false);
            return response;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async Task<HttpContent> DecompressContentAsync(HttpContent compressedContent, ICompressor compressor)
        {
            using (compressedContent)
            {
                Core.Log.LibVerbose("Decompressing a http client stream");
                var sourceStream = await compressedContent.ReadAsStreamAsync().ConfigureAwait(false);
                var destinationStream = new MemoryStream();
                compressor.Decompress(sourceStream, destinationStream);
                destinationStream.Position = 0;
                var newContent = new StreamContent(destinationStream);
                // copy content type so we know how to load correct formatter
                newContent.Headers.ContentType = compressedContent.Headers.ContentType;
                Core.Log.LibVerbose("Stream decompressed");
                return newContent;
            }
        }
    }
}
