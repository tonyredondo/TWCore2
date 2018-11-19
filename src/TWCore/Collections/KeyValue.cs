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
    public class KeyValue<TKey, TValue> : IKeyItem<TKey>, IXmlSerializable
    {
        static readonly bool isKeyAnAttribute;
        static XmlSerializer keySerializer = null;
        static readonly bool isValueAnAttribute;
        static XmlSerializer valueSerializer = null;

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
        static KeyValue()
        {
            isKeyAnAttribute = typeof(TKey) == typeof(string) || typeof(TKey) == typeof(bool) ||
                typeof(TKey) == typeof(char) || typeof(TKey) == typeof(DateTime) ||
                typeof(TKey) == typeof(DateTimeOffset) || typeof(TKey) == typeof(TimeSpan) ||
                typeof(TKey) == typeof(Guid) || typeof(TKey).IsEnum ||
                typeof(TKey) == typeof(byte) ||
                typeof(TKey) == typeof(sbyte) || typeof(TKey) == typeof(short) ||
                typeof(TKey) == typeof(ushort) || typeof(TKey) == typeof(int) ||
                typeof(TKey) == typeof(uint) || typeof(TKey) == typeof(long) ||
                typeof(TKey) == typeof(ulong) || typeof(TKey) == typeof(float) ||
                typeof(TKey) == typeof(double) || typeof(TKey) == typeof(decimal);

            isValueAnAttribute = typeof(TValue) == typeof(string) || typeof(TValue) == typeof(bool) ||
                typeof(TValue) == typeof(char) || typeof(TValue) == typeof(DateTime) ||
                typeof(TValue) == typeof(DateTimeOffset) || typeof(TValue) == typeof(TimeSpan) ||
                typeof(TValue) == typeof(Guid) || typeof(TValue).IsEnum ||
                typeof(TValue) == typeof(byte) ||
                typeof(TValue) == typeof(sbyte) || typeof(TValue) == typeof(short) ||
                typeof(TValue) == typeof(ushort) || typeof(TValue) == typeof(int) ||
                typeof(TValue) == typeof(uint) || typeof(TValue) == typeof(long) ||
                typeof(TValue) == typeof(ulong) || typeof(TValue) == typeof(float) ||
                typeof(TValue) == typeof(double) || typeof(TValue) == typeof(decimal);

            if (!isKeyAnAttribute)
                keySerializer = new XmlSerializer(typeof(KeyBox));
            if (!isValueAnAttribute)
                valueSerializer = new XmlSerializer(typeof(ValueBox));
        }
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
        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (isKeyAnAttribute)
            {
                var key = reader["Key"];
                if (!(key is null))
                {
                    Key = key.ParseTo<TKey>(default);
                    if (typeof(TKey) == typeof(string) && ((object)Key) is null)
                        Key = (TKey)(object)string.Empty;
                }
            }
            else
            {
                reader.ReadToDescendant("Key");
                var kBox = (KeyBox)valueSerializer.Deserialize(reader);
                Key = kBox.Content;
            }

            if (isValueAnAttribute)
            {
                var value = reader["Value"];
                if (!(value is null))
                {
                    Value = value.ParseTo<TValue>(default);
                    if (typeof(TValue) == typeof(string) && ((object)Value) is null)
                        Value = (TValue)(object)string.Empty;
                }
            }
            else
            {
                reader.ReadToDescendant("Value");
                var vBox = (ValueBox)valueSerializer.Deserialize(reader);
                Value = vBox.Content;
            }
            reader.Read();
        }
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (isKeyAnAttribute)
            {
                var key = Convert(Key);
                if (!(key is null))
                    writer.WriteAttributeString("Key", key);
            }
            else
            {
                var kBox = new KeyBox(Key);
                keySerializer.Serialize(writer, kBox);
            }

            if (isValueAnAttribute)
            {
                var value = Convert(Value);
                if (!(value is null))
                    writer.WriteAttributeString("Value", value);
            }
            else
            {
                var vBox = new ValueBox(Value);
                valueSerializer.Serialize(writer, vBox);
            }
        }

        private string Convert<T>(T value)
        {
            if (typeof(T) == typeof(bool))
                return XmlConvert.ToString((bool)(object)value);
            if (typeof(T) == typeof(char))
                return XmlConvert.ToString((char)(object)value);
            if (typeof(T) == typeof(sbyte))
                return XmlConvert.ToString((sbyte)(object)value);
            if (typeof(T) == typeof(byte))
                return XmlConvert.ToString((byte)(object)value);
            if (typeof(T) == typeof(short))
                return XmlConvert.ToString((short)(object)value);
            if (typeof(T) == typeof(ushort))
                return XmlConvert.ToString((ushort)(object)value);
            if (typeof(T) == typeof(int))
                return XmlConvert.ToString((int)(object)value);
            if (typeof(T) == typeof(uint))
                return XmlConvert.ToString((uint)(object)value);
            if (typeof(T) == typeof(long))
                return XmlConvert.ToString((long)(object)value);
            if (typeof(T) == typeof(ulong))
                return XmlConvert.ToString((ulong)(object)value);
            if (typeof(T) == typeof(float))
                return XmlConvert.ToString((float)(object)value);
            if (typeof(T) == typeof(double))
                return XmlConvert.ToString((double)(object)value);
            if (typeof(T) == typeof(decimal))
                return XmlConvert.ToString((decimal)(object)value);
            if (typeof(T) == typeof(DateTime))
                return XmlConvert.ToString((DateTime)(object)value, XmlDateTimeSerializationMode.RoundtripKind);
            if (typeof(T) == typeof(Guid))
                return XmlConvert.ToString((Guid)(object)value);
            return value?.ToString();
        }

        #region Boxes
        /// <summary>
        /// Value Box
        /// </summary>
        [XmlRoot("Value")]
        public struct ValueBox
        {
            /// <summary>
            /// Content Value
            /// </summary>
            [XmlElement]
            public TValue Content { get; set; }
            /// <summary>
            /// Value Box
            /// </summary>
            /// <param name="content">Content value</param>
            public ValueBox(TValue content) { Content = content; }
        }
        /// <summary>
        /// Key Box
        /// </summary>
        [XmlRoot("Key")]
        public struct KeyBox
        {
            /// <summary>
            /// Content Key
            /// </summary>
            [XmlElement]
            public TKey Content { get; set; }
            /// <summary>
            /// Key Box
            /// </summary>
            /// <param name="content">Content key</param>
            public KeyBox(TKey content) { Content = content; }
        }
        #endregion

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
        public KeyValue(string key, string value) : base(key, value) { }
        #endregion
    }
}
