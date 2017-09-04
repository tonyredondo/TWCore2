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
using TWCore.Compression;

namespace TWCore
{
    /// <summary>
    /// Extensions for bytes
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Gets a GZip byte array from the source byte array
        /// </summary>
        /// <param name="value">Source byte array</param>
        /// <returns>GZipped byte array</returns>
        public static SubArray<byte> ToGzip(this byte[] value)
        {
            if (value.IsGzip())
                return value;
            var gzipCompressor = CompressorManager.Get<GZipCompressor>() ?? new GZipCompressor();
            return gzipCompressor.Compress(value);
        }
        /// <summary>
        /// Gets a GZip byte array from the source byte array
        /// </summary>
        /// <param name="value">Source byte array</param>
        /// <returns>GZipped byte array</returns>
        public static SubArray<byte> ToGzip(this SubArray<byte> value)
        {
            if (value.IsGzip())
                return value;
            var gzipCompressor = CompressorManager.Get<GZipCompressor>() ?? new GZipCompressor();
            return gzipCompressor.Compress(value);
        }

        /// <summary>
        /// Gets a Gzip decompressed byte array from a GZipped byte array
        /// </summary>
        /// <param name="value">GZipped byte array</param>
        /// <returns>Decompressed array</returns>
        public static SubArray<byte> FromGzip(this byte[] value)
        {
            if (!value.IsGzip())
                return value;
            var gzipCompressor = CompressorManager.Get<GZipCompressor>() ?? new GZipCompressor();
            return gzipCompressor.Decompress(value);
        }
        /// <summary>
        /// Gets a Gzip decompressed byte array from a GZipped byte array
        /// </summary>
        /// <param name="value">GZipped byte array</param>
        /// <returns>Decompressed array</returns>
        public static SubArray<byte> FromGzip(this SubArray<byte> value)
        {
            if (!value.IsGzip())
                return value;
            var gzipCompressor = CompressorManager.Get<GZipCompressor>() ?? new GZipCompressor();
            return gzipCompressor.Decompress(value);
        }

        /// <summary>
        /// Gets a Deflated byte array from the source byte array
        /// </summary>
        /// <param name="value">Source byte array</param>
        /// <returns>Deflated byte array</returns>
        public static SubArray<byte> ToDeflate(this byte[] value)
            => ToDeflate((SubArray<byte>)value);
        /// <summary>
        /// Gets a Deflated byte array from the source byte array
        /// </summary>
        /// <param name="value">Source byte array</param>
        /// <returns>Deflated byte array</returns>
        public static SubArray<byte> ToDeflate(this SubArray<byte> value)
        {
            var deflateCompressor = CompressorManager.Get<DeflateCompressor>() ?? new DeflateCompressor();
            return deflateCompressor.Compress(value);
        }

        /// <summary>
        /// Gets a Deflate decompressed byte array from a Deflated byte array
        /// </summary>
        /// <param name="value">Deflated byte array</param>
        /// <returns>Decompressed array</returns>
        public static SubArray<byte> FromDeflate(this byte[] value)
            => FromDeflate((SubArray<byte>)value);
        /// <summary>
        /// Gets a Deflate decompressed byte array from a Deflated byte array
        /// </summary>
        /// <param name="value">Deflated byte array</param>
        /// <returns>Decompressed array</returns>
        public static SubArray<byte> FromDeflate(this SubArray<byte> value)
        {
            var deflateCompressor = CompressorManager.Get<DeflateCompressor>() ?? new DeflateCompressor();
            return deflateCompressor.Decompress(value);
        }

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="value">The bytes to write to the file.</param>
        public static void WriteToFile(this byte[] value, string path) 
            => File.WriteAllBytes(path, value);
        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="value">The bytes to write to the file.</param>
        public static void WriteToFile(this SubArray<byte> value, string path)
        {
            using (var fs = File.OpenWrite(path))
                fs.Write(value.Array, value.Offset, value.Count);
        }

        /// <summary>
        /// Gets if a byte array has the Gzip magic number
        /// </summary>
        /// <param name="value">Byte array</param>
        /// <returns>true if have the gzip magic number; otherwise, false.</returns>
        public static bool IsGzip(this byte[] value)
        {
            if (value == null) return false;
            if (value.Length > 2 == true)
                return (value[0] == 0x1f && value[1] == 0x8b);
            return false;
        }
        /// <summary>
        /// Gets if a byte array has the Gzip magic number
        /// </summary>
        /// <param name="value">Byte array</param>
        /// <returns>true if have the gzip magic number; otherwise, false.</returns>
        public static bool IsGzip(this SubArray<byte> value)
        {
            if (value.Count > 2 == true)
                return (value[0] == 0x1f && value[1] == 0x8b);
            return false;
        }
    }
}
