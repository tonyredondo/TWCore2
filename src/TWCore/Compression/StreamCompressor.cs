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
using System.Threading.Tasks;
using TWCore.IO;
// ReSharper disable MemberCanBeProtected.Global

namespace TWCore.Compression
{
    /// <inheritdoc />
    /// <summary>
    /// Stream Compressor base class
    /// </summary>
    public abstract class StreamCompressor : ICompressor
    {
        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Compressor encoding type
        /// </summary>
        public abstract string EncodingType { get; }
        /// <inheritdoc />
        /// <summary>
        /// Compressor file extension
        /// </summary>
        public abstract string FileExtension { get; }
        #endregion

        #region Compression Methods
        /// <summary>
        /// Gets a new compression stream wrapper for the stream source
        /// </summary>
        /// <param name="source">Stream source</param>
        /// <returns>Stream compression wrapper</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Stream GetCompressionStream(Stream source);

        /// <inheritdoc />
        /// <summary>
        /// Compress a stream into another stream
        /// </summary>
        /// <param name="source">Stream source</param>
        /// <param name="destination">Stream destination</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Compress(Stream source, Stream destination)
        {
            using (var compressed = GetCompressionStream(destination))
                source.CopyTo(compressed);
        }
        /// <inheritdoc />
        /// <summary>
        /// Compress a stream into another stream
        /// </summary>
        /// <param name="source">Stream source</param>
        /// <param name="destination">Stream destination</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual async Task CompressAsync(Stream source, Stream destination)
        {
            using (var compressed = GetCompressionStream(destination))
                await source.CopyToAsync(compressed).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Compress a byte array
        /// </summary>
        /// <param name="source">Byte array source</param>
        /// <returns>Compressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual MultiArray<byte> Compress(MultiArray<byte> source)
        {
            using (var msDes = new RecycleMemoryStream())
            {
                using (var msOrg = source.AsReadOnlyStream())
                    Compress(msOrg, msDes);
                return msDes.GetMultiArray();
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Compress a byte array
        /// </summary>
        /// <param name="source">Byte array source</param>
        /// <returns>Compressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual async Task<MultiArray<byte>> CompressAsync(MultiArray<byte> source)
        {
            using (var msDes = new RecycleMemoryStream())
            {
                await CompressAsync(source.AsReadOnlyStream(), msDes).ConfigureAwait(false);
                return msDes.GetMultiArray();
            }
        }
        #endregion

        #region Decompression Methods
        /// <summary>
        /// Get a new decompression stream wrapper for the stream source
        /// </summary>
        /// <param name="source">Stream source</param>
        /// <returns>Stream decompression wrapper</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Stream GetDecompressionStream(Stream source);
        /// <inheritdoc />
        /// <summary>
        /// Decompress a stream into another stream
        /// </summary>
        /// <param name="source">Compressed stream source</param>
        /// <param name="destination">Stream destination</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Decompress(Stream source, Stream destination)
        {
            using (var decompressed = GetDecompressionStream(source))
                decompressed.CopyTo(destination);
        }
        /// <inheritdoc />
        /// <summary>
        /// Decompress a stream into another stream
        /// </summary>
        /// <param name="source">Compressed stream source</param>
        /// <param name="destination">Stream destination</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual async Task DecompressAsync(Stream source, Stream destination)
        {
            using (var decompressed = GetDecompressionStream(source))
                await decompressed.CopyToAsync(destination).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Decompress a byte array
        /// </summary>
        /// <param name="source">Compressed byte array source</param>
        /// <returns>Decompressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual MultiArray<byte> Decompress(MultiArray<byte> source)
        {
            using (var msDes = new RecycleMemoryStream())
            {
                using (var msOrg = source.AsReadOnlyStream())
                    Decompress(msOrg, msDes);
                return msDes.GetMultiArray();
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Decompress a byte array
        /// </summary>
        /// <param name="source">Compressed byte array source</param>
        /// <returns>Decompressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual async Task<MultiArray<byte>> DecompressAsync(MultiArray<byte> source)
        {
            using (var msDes = new RecycleMemoryStream())
            {
                using (var msOrg = source.AsReadOnlyStream())
                    await DecompressAsync(msOrg, msDes).ConfigureAwait(false);
                return msDes.GetMultiArray();
            }
        }
        #endregion
    }
}
