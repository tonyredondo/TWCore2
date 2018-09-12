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
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TWCore.Collections;
using TWCore.Serialization;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBeProtected.Global

namespace TWCore.Security
{
    /// <inheritdoc />
    /// <summary>
    /// Abstract Hash base
    /// </summary>
    public abstract class HashBase : IHash
    {
        private static readonly ConcurrentDictionary<string, string> StringHashCache = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
        private static readonly LRU2QCollection<string, MultiArray<byte>> StringBytesHashCache = new LRU2QCollection<string, MultiArray<byte>>(250);
        private static readonly LRU2QCollection<string, Guid> StringGuidHashCache = new LRU2QCollection<string, Guid>(250);
        private readonly string _instanceName;

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Hash algorithm used
        /// </summary>
        public abstract string Algorithm { get; }
        /// <summary>
        /// Serializer to calculate hash on objects
        /// </summary>
        public ISerializer Serializer { get; set; } = SerializerManager.DefaultBinarySerializer ?? SerializerManager.Serializers.FirstOrDefault();
        /// <summary>
        /// Gets or sets the string encoding used to convert to byte array
        /// </summary>
        public Encoding Encoding { get; set; } = new UTF8Encoding(false);
        #endregion

        /// <summary>
        /// Gets the hash bytes from a bytes array
        /// </summary>
        /// <param name="value">Bytes array to calculate the hash.</param>
        /// <returns>Hash bytes array.</returns>
        public abstract MultiArray<byte> GetHashValue(MultiArray<byte> value);

        #region .ctor
        /// <summary>
        /// Abstract Hash base
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected HashBase()
        {
            _instanceName = GetType().Name;
            HashManager.Register(this);
        }
        #endregion

        #region Methods
        /// <inheritdoc />
        /// <summary>
        /// Gets the hash bytes from a bytes array
        /// </summary>
        /// <param name="bytes">Bytes array to calculate the hash.</param>
        /// <returns>Hash bytes array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray<byte> GetBytes(MultiArray<byte> bytes)
        {
            return GetHashValue(bytes);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the string hash value from a bytes array
        /// </summary>
        /// <param name="bytes">Bytes array to calculate the hash.</param>
        /// <returns>String value with the hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Get(MultiArray<byte> bytes)
        {
            var data = GetBytes(bytes);
            var sb = new StringBuilder();
            for (var i = 0; i < data.Count; i++)
                sb.Append(data[i].ToString("x2"));
            return sb.ToString();
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the guid hash value from a bytes array
        /// </summary>
        /// <param name="bytes">Bytes array to calculate the hash.</param>
        /// <returns>Guid value with the hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Guid GetGuid(MultiArray<byte> bytes) 
		{
			return new Guid(GetBytes(bytes).Slice(0, 16).AsSpan());
		}

        /// <inheritdoc />
        /// <summary>
        /// Gets the hash bytes from an object
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>Hash bytes array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray<byte> GetBytes(object obj) => GetBytes(GetSerializer().Serialize(obj, obj.GetType()));
        /// <inheritdoc />
        /// <summary>
        /// Gets the hash string value from an object
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>String value with the hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Get(object obj) => Get(GetSerializer().Serialize(obj, obj.GetType()));
        /// <inheritdoc />
        /// <summary>
        /// Gets the guid hash value from an object
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>Guid value with the hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid GetGuid(object obj) => GetGuid(GetSerializer().Serialize(obj, obj.GetType()));

        /// <inheritdoc />
        /// <summary>
        /// Gets the hash bytes from a string value
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>Hash bytes array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray<byte> GetBytes(string obj) => StringBytesHashCache.GetOrAdd(_instanceName + obj, key => GetBytes(Encoding.GetBytes(obj)));
        /// <inheritdoc />
        /// <summary>
        /// Gets the hash string value from a string value
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>String value with the hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Get(string obj) => StringHashCache.GetOrAdd(_instanceName + obj, key => Get(Encoding.GetBytes(obj)));
        /// <inheritdoc />
        /// <summary>
        /// Gets the guid hash value from a string value
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>Guid value with the hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Guid GetGuid(string obj) => StringGuidHashCache.GetOrAdd(_instanceName + obj, key => GetGuid(Encoding.GetBytes(obj)));
        #endregion

        #region Private Method
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ISerializer GetSerializer()
        {
            return Serializer ?? SerializerManager.DefaultBinarySerializer ?? SerializerManager.Serializers.FirstOrDefault() ??
                throw new NullReferenceException("The aren't any default Serializer loaded.");
        }
        #endregion
    }
}
