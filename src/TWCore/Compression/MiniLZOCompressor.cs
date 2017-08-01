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

using System.Runtime.CompilerServices;

namespace TWCore.Compression
{
    /// <summary>
    /// Implements a MiniLZO Compresor
    /// </summary>
    public class MiniLZOCompressor : ByteCompressor
    {
        /// <summary>
        /// Compressor encoding type
        /// </summary>
        public override string EncodingType { get; } = "lzo";
        /// <summary>
        /// Compressor file extension
        /// </summary>
        public override string FileExtension { get; } = ".lzo";
        /// <summary>
        /// Compress a byte array
        /// </summary>
        /// <param name="source">Byte array source</param>
        /// <returns>Compressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override SubArray<byte> Compress(SubArray<byte> source) => MiniLZO.Compress(source);
        /// <summary>
        /// Decompress a byte array
        /// </summary>
        /// <param name="source">Compressed byte array source</param>
        /// <returns>Decompressed byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override SubArray<byte> Decompress(SubArray<byte> source) => MiniLZO.Decompress(source);
    }
}
