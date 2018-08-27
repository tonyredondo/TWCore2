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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TWCore.Compression
{
    /// <summary>
    /// Class to manage the compressors
    /// </summary>
    public static class CompressorManager
    {
        /// <summary>
        /// List of registered compressors
        /// </summary>
        public static readonly NonBlocking.ConcurrentDictionary<string, ICompressor> Compressors = new NonBlocking.ConcurrentDictionary<string, ICompressor>(StringComparer.OrdinalIgnoreCase);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static CompressorManager()
        {
            Register(new BrotliCompressor());
            Register(new GZipCompressor());
            Register(new DeflateCompressor());
        }

        #region Register and Deregister
        /// <summary>
        /// Register a new compressor
        /// </summary>
        /// <param name="compressor">Compressor to register</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Register(ICompressor compressor)
        {
            if (compressor != null)
                Compressors.TryAdd(compressor.EncodingType, compressor);
        }
        /// <summary>
        /// Deregister a compressor from the list
        /// </summary>
        /// <param name="compressor">Compressor to deregister</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deregister(ICompressor compressor)
        {
            if (compressor != null)
                Compressors.TryRemove(compressor.EncodingType, out compressor);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get a compressor by encoding type
        /// </summary>
        /// <param name="encodingType">Encoding type</param>
        /// <returns>Compressor instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICompressor GetByEncodingType(string encodingType)
            => Compressors.TryGetValue(encodingType, out var compressor) ? compressor : null;
        /// <summary>
        /// Get a compressor by file extension
        /// </summary>
        /// <param name="fileExtension">File extension</param>
        /// <returns>Compressor instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICompressor GetByFileExtension(string fileExtension)
            => Compressors.Values.FirstOrDefault((c, fExt) => c.FileExtension.Equals(fExt, StringComparison.OrdinalIgnoreCase), fileExtension);
        /// <summary>
        /// Get a compressor by type
        /// </summary>
        /// <typeparam name="T">ICompressor type</typeparam>
        /// <returns>ICompressor instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICompressor Get<T>() where T : ICompressor
            => Compressors.Values.FirstOrDefault((v, tType) => v.GetType() == tType, typeof(T));
        /// <summary>
        /// Gets the compressors list
        /// </summary>
        /// <returns>Registered compressors list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<ICompressor> ToList()
            => Compressors.Values.ToList();
        #endregion
    }
}
