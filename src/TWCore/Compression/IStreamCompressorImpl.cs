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
using System.Runtime.CompilerServices;

namespace TWCore.Compression
{
    /// <summary>
    /// Stream compressor implementation
    /// </summary>
    public interface IStreamCompressorImpl
    {
        /// <summary>
        /// Compressor encoding type
        /// </summary>
        string EncodingType { get; }
        /// <summary>
        /// Compressor file extension
        /// </summary>
        string FileExtension { get; }
        /// <summary>
        /// Gets a new compression stream wrapper for the stream source
        /// </summary>
        /// <param name="source">Stream source</param>
        /// <returns>Stream compression wrapper</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Stream GetCompressionStream(Stream source);
        /// <summary>
        /// Gets a new decompression stream wrapper for the stream source
        /// </summary>
        /// <param name="source">Stream source</param>
        /// <returns>Stream decompression wrapper</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Stream GetDecompressionStream(Stream source);
    }
}
