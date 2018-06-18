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
    public abstract class ByteCompressor : ICompressor
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
        /// <inheritdoc />
        /// <summary>
        /// Compress a stream into another stream
        /// </summary>
        /// <param name="source">Stream source</param>
        /// <param name="destination">Stream destination</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Compress(Stream source, Stream destination)
        {
            using (var ms = Compress(source.ReadAllBytes()).ToMemoryStream())
                ms.CopyTo(destination);
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
            using (var ms = Compress(await source.ReadAllBytesAsync().ConfigureAwait(false)).ToMemoryStream())
                await ms.CopyToAsync(destination).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Compress a byte array
        /// </summary>
        /// <param name="source">Byte array source</param>
        /// <returns>Compressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract SubArray<byte> Compress(SubArray<byte> source);
        /// <inheritdoc />
        /// <summary>
        /// Compress a byte array
        /// </summary>
        /// <param name="source">Byte array source</param>
        /// <returns>Compressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual Task<SubArray<byte>> CompressAsync(SubArray<byte> source) 
            => Task.FromResult(Compress(source));
        #endregion

        #region Decompression Methods
        /// <inheritdoc />
        /// <summary>
        /// Decompress a stream into another stream
        /// </summary>
        /// <param name="source">Compressed stream source</param>
        /// <param name="destination">Stream destination</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Decompress(Stream source, Stream destination)
        {
            using (var ms = Decompress(source.ReadAllBytes()).ToMemoryStream())
                ms.CopyTo(destination);
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
            using (var ms = Decompress(await source.ReadAllBytesAsync().ConfigureAwait(false)).ToMemoryStream())
                await ms.CopyToAsync(destination).ConfigureAwait(false);
        }
        /// <inheritdoc />
        /// <summary>
        /// Decompress a byte array
        /// </summary>
        /// <param name="source">Compressed byte array source</param>
        /// <returns>Decompressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract SubArray<byte> Decompress(SubArray<byte> source);
        /// <inheritdoc />
        /// <summary>
        /// Decompress a byte array
        /// </summary>
        /// <param name="source">Compressed byte array source</param>
        /// <returns>Decompressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual Task<SubArray<byte>> DecompressAsync(SubArray<byte> source)
            => Task.FromResult(Decompress(source));
        #endregion
    }
}
