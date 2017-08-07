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
using System.Threading.Tasks;

namespace TWCore.Compression
{
    /// <summary>
    /// Defines the Compressors interface
    /// </summary>
    public interface ICompressor
    {
        /// <summary>
        /// Compressor encoding type
        /// </summary>
        string EncodingType { get; }
        /// <summary>
        /// Compressor file extension
        /// </summary>
        string FileExtension { get; }

        #region Compress methods
        /// <summary>
        /// Compress a stream into another stream
        /// </summary>
        /// <param name="source">Stream source</param>
        /// <param name="destination">Stream destination</param>
        void Compress(Stream source, Stream destination);
        /// <summary>
        /// Compress a byte array
        /// </summary>
        /// <param name="source">Byte array source</param>
        /// <returns>Compressed byte array</returns>
        SubArray<byte> Compress(SubArray<byte> source);
        /// <summary>
        /// Compress a stream into another stream
        /// </summary>
        /// <param name="source">Stream source</param>
        /// <param name="destination">Stream destination</param>
        /// <returns>Task of the method execution</returns>
        Task CompressAsync(Stream source, Stream destination);
        /// <summary>
        /// Compress a byte array
        /// </summary>
        /// <param name="source">Byte array source</param>
        /// <returns>Compressed byte array</returns>
        ValueTask<SubArray<byte>> CompressAsync(SubArray<byte> source);
        #endregion

        #region Decompress methods
        /// <summary>
        /// Decompress a stream into another stream
        /// </summary>
        /// <param name="source">Compressed stream source</param>
        /// <param name="destination">Stream destination</param>
        void Decompress(Stream source, Stream destination);
        /// <summary>
        /// Decompress a byte array
        /// </summary>
        /// <param name="source">Compressed byte array source</param>
        /// <returns>Decompressed byte array</returns>
        SubArray<byte> Decompress(SubArray<byte> source);
        /// <summary>
        /// Decompress a stream into another stream
        /// </summary>
        /// <param name="source">Compressed stream source</param>
        /// <param name="destination">Stream destination</param>
        /// <returns>Task of the method execution</returns>
        Task DecompressAsync(Stream source, Stream destination);
        /// <summary>
        /// Decompress a byte array
        /// </summary>
        /// <param name="source">Compressed byte array source</param>
        /// <returns>Decompressed byte array</returns>
        ValueTask<SubArray<byte>> DecompressAsync(SubArray<byte> source);
        #endregion
    }
}
