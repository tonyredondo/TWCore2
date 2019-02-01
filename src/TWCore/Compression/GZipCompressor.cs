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

using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace TWCore.Compression
{
    /// <inheritdoc />
    /// <summary>
    /// Implements a Gzip Compresor
    /// </summary>
    public class GZipCompressor : StreamCompressor<GZipCompressor.Impl>
    {
        /// <inheritdoc />
        public readonly struct Impl : IStreamCompressorImpl
        {
            /// <inheritdoc />
            public string EncodingType => "gzip";
            /// <inheritdoc />
            public string FileExtension => ".gz";

            /// <inheritdoc />
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Stream GetCompressionStream(Stream source)
                => new GZipStream(source, CompressionLevel.Fastest, true);

            /// <inheritdoc />
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Stream GetDecompressionStream(Stream source)
                => new GZipStream(source, CompressionMode.Decompress, true);
        }
    }
}
