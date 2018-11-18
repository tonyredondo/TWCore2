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
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Collections
{
	/// <inheritdoc />
	/// <summary>
	/// Key/Value Item
	/// </summary>
	/// <typeparam name="TKey">Key Type</typeparam>
	/// <typeparam name="TValue">Value Type</typeparam>
	[XmlRoot("KeyValue"), DataContract]
	[Serializable]
	public class KeyValue<TKey, TValue> : IKeyItem<TKey> //, IXmlSerializable
	{
		//static bool isKeyAnAttribute;
		//static XmlSerializer keySerializer = null;
		//static bool isValueAnAttribute;
		//static XmlSerializer valueSerializer = null;

		#region Properties
		/// <inheritdoc />
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
		//static KeyValue()
		//{
		//	isKeyAnAttribute = typeof(TKey) == typeof(string) || typeof(TKey) == typeof(bool) ||
		//		typeof(TKey) == typeof(char) || typeof(TKey) == typeof(DateTime) ||
		//		typeof(TKey) == typeof(DateTimeOffset) || typeof(TKey) == typeof(TimeSpan) ||
		//		typeof(TKey) == typeof(Guid) || typeof(TKey).IsEnum ||
		//		typeof(TKey) == typeof(byte) ||
		//		typeof(TKey) == typeof(sbyte) || typeof(TKey) == typeof(short) ||
		//		typeof(TKey) == typeof(ushort) || typeof(TKey) == typeof(int) ||
		//		typeof(TKey) == typeof(uint) || typeof(TKey) == typeof(long) ||
		//		typeof(TKey) == typeof(ulong) || typeof(TKey) == typeof(float) ||
		//		typeof(TKey) == typeof(double) || typeof(TKey) == typeof(decimal);

		//	isValueAnAttribute = typeof(TValue) == typeof(string) || typeof(TValue) == typeof(bool) ||
		//		typeof(TValue) == typeof(char) || typeof(TValue) == typeof(DateTime) ||
		//		typeof(TValue) == typeof(DateTimeOffset) || typeof(TValue) == typeof(TimeSpan) ||
		//		typeof(TValue) == typeof(Guid) || typeof(TValue).IsEnum ||
		//		typeof(TValue) == typeof(byte) ||
		//		typeof(TValue) == typeof(sbyte) || typeof(TValue) == typeof(short) ||
		//		typeof(TValue) == typeof(ushort) || typeof(TValue) == typeof(int) ||
		//		typeof(TValue) == typeof(uint) || typeof(TValue) == typeof(long) ||
		//		typeof(TValue) == typeof(ulong) || typeof(TValue) == typeof(float) ||
		//		typeof(TValue) == typeof(double) || typeof(TValue) == typeof(decimal);

		//	keySerializer = new XmlSerializer(typeof(TKey));
		//	valueSerializer = new XmlSerializer(typeof(TValue));
		//}
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

		#region IXmlSerializable
		//System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		//{
		//	return null;
		//}
		//void IXmlSerializable.ReadXml(XmlReader reader)
		//{
		//	bool wasEmpty = reader.IsEmptyElement;
		//	reader.Read();

		//	if (wasEmpty)
		//		return;

		//	if (isKeyAnAttribute)
		//	{
		//		Key = reader["Key"].ParseTo<TKey>(default);
		//	}
		//	else
		//	{
		//		reader.ReadStartElement("Key");
		//		Key = (TKey)keySerializer.Deserialize(reader);
		//		reader.ReadEndElement();
		//	}

		//	if (isValueAnAttribute)
		//	{
		//		Value = reader["Value"].ParseTo<TValue>(default);
		//	}
		//	else
		//	{
		//		reader.ReadStartElement("Key");
		//		Value = (TValue)valueSerializer.Deserialize(reader);
		//		reader.ReadEndElement();
		//	}
		//}
		//void IXmlSerializable.WriteXml(XmlWriter writer)
		//{
		//	if (isKeyAnAttribute)
		//	{
		//		writer.WriteAttributeString("Key", Key.ToString());
		//	}
		//	else
		//	{
		//		writer.WriteStartElement("Key");
		//		keySerializer.Serialize(writer, Key);
		//		writer.WriteEndElement();
		//	}

		//	if (isValueAnAttribute)
		//	{
		//		writer.WriteAttributeString("Value", Value.ToString());
		//	}
		//	else
		//	{
		//		writer.WriteStartElement("Value");
		//		valueSerializer.Serialize(writer, Value, null, null, "Value");
		//		writer.WriteEndElement();
		//	}
		//}
		#endregion
	}

    /// <inheritdoc />
    /// <summary>
    /// Key/Value Item
    /// </summary>
    [XmlRoot("KeyValue"), DataContract]
	[Serializable]
    public class KeyValue : KeyValue<string, string>
    {
        #region .ctors
        /// <inheritdoc />
        /// <summary>
        /// Key/Value Item
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValue() { }
        /// <inheritdoc />
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
