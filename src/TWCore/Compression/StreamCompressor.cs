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
    /// Stream Compressor class
    /// </summary>
    public class StreamCompressor<T> : ICompressor
        where T : struct, IStreamCompressorImpl
    {
        private static readonly ReferencePool<RecycleMemoryStream> _recMemStream = ReferencePool<RecycleMemoryStream>.Shared;
        private readonly T _implementation = default;

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Compressor encoding type
        /// </summary>
        public string EncodingType => _implementation.EncodingType;
        /// <inheritdoc />
        /// <summary>
        /// Compressor file extension
        /// </summary>
        public string FileExtension => _implementation.FileExtension;
        #endregion

        #region Compression Methods
        /// <inheritdoc />
        /// <summary>
        /// Compress a stream into another stream
        /// </summary>
        /// <param name="source">Stream source</param>
        /// <param name="destination">Stream destination</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Compress(Stream source, Stream destination)
        {
            using (var compressed = _implementation.GetCompressionStream(destination))
                source.CopyTo(compressed);
        }
        /// <inheritdoc />
        /// <summary>
        /// Compress a stream into another stream
        /// </summary>
        /// <param name="source">Stream source</param>
        /// <param name="destination">Stream destination</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task CompressAsync(Stream source, Stream destination)
        {
            using (var compressed = _implementation.GetCompressionStream(destination))
            {
                await source.CopyToAsync(compressed).ConfigureAwait(false);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Compress a byte array
        /// </summary>
        /// <param name="source">Byte array source</param>
        /// <returns>Compressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray<byte> Compress(MultiArray<byte> source)
        {
            var msDes = _recMemStream.New();
            using (var msOrg = source.AsReadOnlyStream())
                Compress(msOrg, msDes);
            var value = msDes.GetMultiArray();
            msDes.Reset();
            _recMemStream.Store(msDes);
            return value;
        }
        /// <inheritdoc />
        /// <summary>
        /// Compress a byte array
        /// </summary>
        /// <param name="source">Byte array source</param>
        /// <returns>Compressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<MultiArray<byte>> CompressAsync(MultiArray<byte> source)
        {
            var msDes = _recMemStream.New();
            await CompressAsync(source.AsReadOnlyStream(), msDes).ConfigureAwait(false);
            var value = msDes.GetMultiArray();
            msDes.Reset();
            _recMemStream.Store(msDes);
            return value;
        }
        #endregion

        #region Decompression Methods
        /// <inheritdoc />
        /// <summary>
        /// Decompress a stream into another stream
        /// </summary>
        /// <param name="source">Compressed stream source</param>
        /// <param name="destination">Stream destination</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Decompress(Stream source, Stream destination)
        {
            using (var decompressed = _implementation.GetDecompressionStream(source))
                decompressed.CopyTo(destination);
        }
        /// <inheritdoc />
        /// <summary>
        /// Decompress a stream into another stream
        /// </summary>
        /// <param name="source">Compressed stream source</param>
        /// <param name="destination">Stream destination</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task DecompressAsync(Stream source, Stream destination)
        {
            using (var decompressed = _implementation.GetDecompressionStream(source))
            {
                await decompressed.CopyToAsync(destination).ConfigureAwait(false);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Decompress a byte array
        /// </summary>
        /// <param name="source">Compressed byte array source</param>
        /// <returns>Decompressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray<byte> Decompress(MultiArray<byte> source)
        {
            var msDes = _recMemStream.New();
            using (var msOrg = source.AsReadOnlyStream())
                Decompress(msOrg, msDes);
            var value = msDes.GetMultiArray();
            msDes.Reset();
            _recMemStream.Store(msDes);
            return value;
        }
        /// <inheritdoc />
        /// <summary>
        /// Decompress a byte array
        /// </summary>
        /// <param name="source">Compressed byte array source</param>
        /// <returns>Decompressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<MultiArray<byte>> DecompressAsync(MultiArray<byte> source)
        {
            var msDes = _recMemStream.New();
            using (var msOrg = source.AsReadOnlyStream())
                await DecompressAsync(msOrg, msDes).ConfigureAwait(false);
            var value = msDes.GetMultiArray();
            msDes.Reset();
            _recMemStream.Store(msDes);
            return value;
        }
        #endregion
    }
}
