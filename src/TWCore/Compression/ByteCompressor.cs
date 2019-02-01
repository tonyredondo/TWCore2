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

namespace TWCore.Compression
{
    /// <inheritdoc />
    /// <summary>
    /// Byte Compressor base class
    /// </summary>
    public class ByteCompressor<T> : ICompressor
        where T : struct, IByteCompressorImpl
    {
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
            var compressValue = _implementation.Compress(source.ReadAllBytes());
            compressValue.CopyTo(destination);
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
            var compressValue = _implementation.Compress(await source.ReadAllBytesAsync().ConfigureAwait(false));
            await compressValue.CopyToAsync(destination).ConfigureAwait(false);
        }
        /// <summary>
        /// Compress a byte array
        /// </summary>
        /// <param name="source">Byte array source</param>
        /// <returns>Compressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray<byte> Compress(MultiArray<byte> source)
            => _implementation.Compress(source);
        /// <inheritdoc />
        /// <summary>
        /// Compress a byte array
        /// </summary>
        /// <param name="source">Byte array source</param>
        /// <returns>Compressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<MultiArray<byte>> CompressAsync(MultiArray<byte> source) 
            => Task.FromResult(_implementation.Compress(source));
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
            var decompress = _implementation.Decompress(source.ReadAllBytes());
            decompress.CopyTo(destination);
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
            var decompress = _implementation.Decompress(await source.ReadAllBytesAsync().ConfigureAwait(false));
            await decompress.CopyToAsync(destination).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Decompress a byte array
        /// </summary>
        /// <param name="source">Compressed byte array source</param>
        /// <returns>Decompressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray<byte> Decompress(MultiArray<byte> source)
            => _implementation.Decompress(source);
        /// <inheritdoc />
        /// <summary>
        /// Decompress a byte array
        /// </summary>
        /// <param name="source">Compressed byte array source</param>
        /// <returns>Decompressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<MultiArray<byte>> DecompressAsync(MultiArray<byte> source)
            => Task.FromResult(_implementation.Decompress(source));
        #endregion
    }
}
