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
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
// ReSharper disable InconsistentNaming

namespace TWCore.Security
{
    /// <inheritdoc />
    /// <summary>
    /// SHA384 Hash implementation
    /// </summary>
    public class SHA384Hash : HashBase
    {
        private readonly SHA384 _hashAlgo;
        /// <inheritdoc />
        /// <summary>
        /// Hash algorithm used
        /// </summary>
        public override string Algorithm { get; } = "SHA384";
        /// <inheritdoc />
        /// <summary>
        /// MD5 Hash implementation
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SHA384Hash() => _hashAlgo = SHA384.Create();
        /// <inheritdoc />
        /// <summary>
        /// Gets the hash bytes from a bytes array
        /// </summary>
        /// <param name="value">Bytes array to calculate the hash.</param>
        /// <returns>Hash bytes array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte[] GetHashValue(byte[] value) => _hashAlgo.ComputeHash(value);
    }

    /// <summary>
    /// SHA384 Hash extensions
    /// </summary>
    public static class SHA384HashExtensions
    {
        /// <summary>
        /// Hash algorithm used by the extensions
        /// </summary>
        public static IHash Hash { get; } = new SHA384Hash();

        /// <summary>
        /// Get the string hash for the byte array
        /// </summary>
        /// <param name="bytes">Byte array</param>
        /// <returns>String hash</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetHashSHA384(this byte[] bytes) => Hash.Get(bytes);
        /// <summary>
        /// Get the string hash for the object
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>String hash</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetHashSHA384(this object obj) => Hash.Get(obj);
        /// <summary>
        /// Get the string hash for the string
        /// </summary>
        /// <param name="text">String value</param>
        /// <returns>String hash</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetHashSHA384(this string text) => Hash.Get(text);

        /// <summary>
        /// Get the byte array hash for the byte array
        /// </summary>
        /// <param name="bytes">Byte array</param>
        /// <returns>Byte array hash</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] GetHashSHA384Bytes(this byte[] bytes) => Hash.GetBytes(bytes);
        /// <summary>
        /// Get the byte array hash for the object
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Byte array hash</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] GetHashSHA384Bytes(this object obj) => Hash.GetBytes(obj);
        /// <summary>
        /// Get the byte array hash for the string
        /// </summary>
        /// <param name="text">String value</param>
        /// <returns>Byte array hash</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] GetHashSHA384Bytes(this string text) => Hash.GetBytes(text);

        /// <summary>
        /// Get the Guid hash for the byte array
        /// </summary>
        /// <param name="bytes">Byte array</param>
        /// <returns>Guid hash</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid GetHashSHA384Guid(this byte[] bytes) => Hash.GetGuid(bytes);
        /// <summary>
        /// Get the Guid hash for the object
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Guid hash</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid GetHashSHA384Guid(this object obj) => Hash.GetGuid(obj);
        /// <summary>
        /// Get the Guid hash for the string
        /// </summary>
        /// <param name="text">String value</param>
        /// <returns>Guid hash</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid GetHashSHA384Guid(this string text) => Hash.GetGuid(text);
    }

}
