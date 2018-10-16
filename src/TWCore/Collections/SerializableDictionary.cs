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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
// ReSharper disable SuggestBaseTypeForParameter

namespace TWCore.Collections
{
    /// <summary>
    /// Dictionary with xml serialization support
    /// </summary>
    /// <typeparam name="TKey">Type of the Key</typeparam>
    /// <typeparam name="TValue">Type of the Value</typeparam>
    [XmlRoot("Dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        #region .ctor
        /// <summary>
        /// Initializes a new instance of the SerializableDictionary class 
        /// that is empty, has the default initial capacity, and uses the default equality 
        /// comparer for the key type.
        /// </summary>
        public SerializableDictionary() { }
        /// <summary>
        /// Initializes a new instance of the SerializableDictionary class 
        /// that contains elements copied from the specified IDictionary 
        /// and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary">The IDictionary whose elements are copied to the new SerializableDictionary.</param>
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
#if !COMPATIBILITY
        /// <summary>
        /// Initializes a new instance of the SerializableDictionary class 
        /// that contains elements copied from the specified IEnumerable 
        /// and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="collection">The IEnumerable whose elements are copied to the new SerializableDictionary.</param>
        public SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection) { }
#endif
        /// <summary>
        /// Initializes a new instance of the SerializableDictionary class 
        /// that is empty, has the default initial capacity, and uses the specified IEqualityComparer.
        /// </summary>
        /// <param name="comparer">The IEqualityComparer implementation to use when comparing keys, or null to use the default IEqualityComparer for the type of the key.</param>
        public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        /// <summary>
        /// Initializes a new instance of the SerializableDictionary class 
        /// that is empty, has the specified initial capacity, and uses the default equality 
        /// comparer for the key type.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the SerializableDictionary can contain.</param>
        public SerializableDictionary(int capacity) : base(capacity) { }
        /// <summary>
        /// Initializes a new instance of the SerializableDictionary class 
        /// that contains elements copied from the specified IDictionary
        /// and uses the specified IEqualityComparer.
        /// </summary>
        /// <param name="dictionary">The IDictionary whose elements are copied to the new SerializableDictionary.</param>
        /// <param name="comparer">The IEqualityComparer implementation to use when comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1 for the type of the key.</param>
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
#if !COMPATIBILITY

        /// <summary>
        /// Initializes a new instance of the SerializableDictionary class 
        /// that contains elements copied from the specified IEnumerable
        /// and uses the specified IEqualityComparer.
        /// </summary>
        /// <param name="collection">The IEnumerable whose elements are copied to the new SerializableDictionary.</param>
        /// <param name="comparer">The IEqualityComparer implementation to use when comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1 for the type of the key.</param>
        public SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(collection, comparer) { }
#endif

        /// <summary>
        /// Initializes a new instance of the SerializableDictionary class 
        /// that is empty, has the specified initial capacity, and uses the specified System.Collections.Generic.IEqualityComparer`1.
        /// </summary>
        /// <param name="capacity">T    he initial number of elements that the SerializableDictionary can contain.</param>
        /// <param name="comparer">The IEqualityComparer implementation to use when comparing keys, or null to use the default IEqualityComparer for the type of the key.</param>
        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }
        /// <summary>
        /// Initializes a new instance of the SerializableDictionary class with serialized data.
        /// </summary>
        /// <param name="info">A System.Runtime.Serialization.SerializationInfo object containing the information required to serialize the SerializableDictionary.</param>
        /// <param name="context">A System.Runtime.Serialization.StreamingContext structure containing the source and destination of the serialized stream associated with the SerializableDictionary.</param>
        protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion

        #region IXmlSerializable Members

        XmlSchema IXmlSerializable.GetSchema() => null;

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("Item");

                reader.ReadStartElement("Key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("Value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("Item");

                writer.WriteStartElement("Key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("Value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        #endregion
    }
}
