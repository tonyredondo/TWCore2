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

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TWCore.Collections
{
	/// <summary>
	/// Key/Value Item
	/// </summary>
	/// <typeparam name="TKey">Key Type</typeparam>
	/// <typeparam name="TValue">Value Type</typeparam>
	[XmlRoot("KeyValue"), DataContract]
	[Serializable]
	public class KeyValue<TKey, TValue> : IKeyItem<TKey>
	{
		#region Properties
		/// <summary>
		/// Key
		/// </summary>
		[XmlAttribute, DataMember]
		public TKey Key { get; set; }
		/// <summary>
		/// Value
		/// </summary>
		[XmlAttribute, DataMember]
		public TValue Value { get; set; }
		#endregion

		#region .ctors
		/// <summary>
		/// Key/Value Item
		/// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValue() { }
		/// <summary>
		/// Key/Value Item
		/// </summary>
		/// <param name="key">Key</param>
		/// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValue(TKey key, TValue value)
		{
			Key = key;
			Value = value;
		}
		/// <summary>
		/// Key/Value Item
		/// </summary>
		/// <param name="item">KeyValuePair item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValue(KeyValuePair<TKey, TValue> item)
		{
			Key = item.Key;
			Value = item.Value;
		}
		#endregion

		#region Public Method
		/// <summary>
		/// Get Key Value Pair
		/// </summary>
		/// <returns>The key value pair.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<TKey, TValue> ToKeyValuePair()
			=> new KeyValuePair<TKey, TValue>(Key, Value);
		#endregion
	}

    /// <summary>
    /// Key/Value Item
    /// </summary>
    [XmlRoot("KeyValue"), DataContract]
	[Serializable]
    public class KeyValue : KeyValue<string, string>
    {
        #region .ctors
        /// <summary>
        /// Key/Value Item
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValue() { }
        /// <summary>
        /// Key/Value Item
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValue(string key, string value) : base(key, value) {}
        #endregion
    }
}
