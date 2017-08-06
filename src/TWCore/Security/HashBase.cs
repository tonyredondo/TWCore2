﻿/*
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

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TWCore.Collections;
using TWCore.Serialization;

namespace TWCore.Security
{
    /// <summary>
    /// Abstract Hash base
    /// </summary>
    public abstract class HashBase : IHash
    {
        string _instanceName;
        readonly static ConcurrentDictionary<string, string> StringHashCache = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
        readonly static LRU2QCollection<string, byte[]> StringBytesHashCache = new LRU2QCollection<string, byte[]>(1000);
        readonly static LRU2QCollection<string, Guid> StringGuidHashCache = new LRU2QCollection<string, Guid>(1000);

        #region Properties
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
        public abstract byte[] GetHashValue(byte[] value);

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
        /// <summary>
        /// Gets the hash bytes from a bytes array
        /// </summary>
        /// <param name="bytes">Bytes array to calculate the hash.</param>
        /// <returns>Hash bytes array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual byte[] GetBytes(byte[] bytes)
        {
            return GetHashValue(bytes);
        }
        /// <summary>
        /// Gets the string hash value from a bytes array
        /// </summary>
        /// <param name="bytes">Bytes array to calculate the hash.</param>
        /// <returns>String value with the hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string Get(byte[] bytes)
        {
            byte[] data = GetBytes(bytes);
            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                sb.Append(data[i].ToString("x2"));
            return sb.ToString();
        }
        /// <summary>
        /// Gets the guid hash value from a bytes array
        /// </summary>
        /// <param name="bytes">Bytes array to calculate the hash.</param>
        /// <returns>Guid value with the hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual Guid GetGuid(byte[] bytes) 
		{
			var bHash = GetBytes(bytes);
			var newGuid = new byte[16];
			Buffer.BlockCopy(bHash, 0, newGuid, 0, 16);
			return new Guid(newGuid);
		}

        /// <summary>
        /// Gets the hash bytes from an object
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>Hash bytes array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual byte[] GetBytes(object obj) => GetBytes((Serializer ?? SerializerManager.DefaultBinarySerializer ?? SerializerManager.Serializers.FirstOrDefault()).Serialize(obj, obj.GetType()));
        /// <summary>
        /// Gets the hash string value from an object
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>String value with the hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string Get(object obj) => Get((Serializer ?? SerializerManager.DefaultBinarySerializer ?? SerializerManager.Serializers.FirstOrDefault()).Serialize(obj, obj.GetType()));
        /// <summary>
        /// Gets the guid hash value from an object
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>Guid value with the hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual Guid GetGuid(object obj) => GetGuid((Serializer ?? SerializerManager.DefaultBinarySerializer ?? SerializerManager.Serializers.FirstOrDefault()).Serialize(obj, obj.GetType()));

        /// <summary>
        /// Gets the hash bytes from a string value
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>Hash bytes array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual byte[] GetBytes(string obj) => StringBytesHashCache.GetOrAdd(_instanceName + obj, key => GetBytes(Encoding.GetBytes(obj)));
        /// <summary>
        /// Gets the hash string value from a string value
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>String value with the hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string Get(string obj) => StringHashCache.GetOrAdd(_instanceName + obj, key => Get(Encoding.GetBytes(obj)));
        /// <summary>
        /// Gets the guid hash value from a string value
        /// </summary>
        /// <param name="obj">Object to get the hash.</param>
        /// <returns>Guid value with the hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual Guid GetGuid(string obj) => StringGuidHashCache.GetOrAdd(_instanceName + obj, key => GetGuid(Encoding.GetBytes(obj)));

        #endregion
    }
}
