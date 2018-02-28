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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToConstant.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Serialization
{
    /// <inheritdoc />
    /// <summary>
    /// Xml Serializer
    /// </summary>
    public sealed class XmlTextSerializer : TextSerializer
    {
        private static readonly NonBlocking.ConcurrentDictionary<(Encoding, bool, bool), XmlWriterSettings> SettingsCache = new NonBlocking.ConcurrentDictionary<(Encoding, bool, bool), XmlWriterSettings>();
        private static readonly string[] SExtensions = { ".xml" };
        private static readonly string[] SMimeTypes = { SerializerMimeTypes.Xml, "text/xml" };

        #region Default Values
        /// <summary>
        /// Default encoding to use when serializing or deserializing.
        /// </summary>
        public static Encoding DefaultEncoding = new UTF8Encoding(false);
        /// <summary>
        /// Default value to indicate if the serialized xml should have Indent
        /// </summary>
        public static bool DefaultIndent = true;
        /// <summary>
        /// Default value to indicate if the serializer needs to include the Xml declaration line.
        /// </summary>
        public static bool DefaultOmitXmlDeclaration = false;
        /// <summary>
        /// Default value to indicate if the serializer removes the default xml namespaces
        /// </summary>
        public static bool DefaultRemoveNamespaces = false;
        #endregion

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Supported file extensions
        /// </summary>
        public override string[] Extensions => SExtensions;
        /// <inheritdoc />
        /// <summary>
        /// Supported mime types
        /// </summary>
        public override string[] MimeTypes => SMimeTypes;
        /// <summary>
        /// Indicates if the serialized xml should have Indent
        /// </summary>
        public bool Indent { get; set; } = DefaultIndent;
        /// <summary>
        /// Indicates if the serializer needs to include the Xml declaration line.
        /// </summary>
        public bool OmitXmlDeclaration { get; set; } = DefaultOmitXmlDeclaration;
        /// <summary>
        /// Indicates if the serializer removes the default xml namespaces
        /// </summary>
        public bool RemoveNamespaces { get; set; } = DefaultRemoveNamespaces;
        /// <summary>
        /// Namespaces
        /// </summary>
        public Dictionary<string, string> Namespaces { get; set; } = new Dictionary<string, string>();
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Xml Serializer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlTextSerializer()
        {
            Encoding = DefaultEncoding;
        }
        #endregion

        private static readonly NonBlocking.ConcurrentDictionary<string, XmlSerializer> CacheSerializer = new NonBlocking.ConcurrentDictionary<string, XmlSerializer>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static XmlSerializer CreateSerializer(Type type, Type[] extraTypes)
        {
            var key = string.Format("{0}[{1}]", type?.FullName, extraTypes?.Select(i => i?.FullName).RemoveNulls().Join(","));
            return CacheSerializer.GetOrAdd(key, mKey => new XmlSerializer(type, extraTypes));
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the object instance deserialized from a stream
        /// </summary>
        /// <param name="stream">Deserialized stream value</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override object OnDeserialize(Stream stream, Type itemType)
        {
            var extraTypes = new HashSet<Type>();
            extraTypes.UnionWith(SerializerManager.DefaultKnownTypes);
            extraTypes.UnionWith(KnownTypes);
            var xser = CreateSerializer(itemType, extraTypes.ToArray());
            using (var xreader = XmlReader.Create(stream))
                return xser.Deserialize(xreader);
        }
        /// <inheritdoc />
        /// <summary>
        /// Serialize an object and writes it to the stream
        /// </summary>
        /// <param name="stream">Destination stream</param>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Deserialized byte array value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnSerialize(Stream stream, object item, Type itemType)
        {
            var extraTypes = new HashSet<Type>();
            SerializerManager.FillExtraTypes(extraTypes, item, itemType);
            extraTypes.UnionWith(SerializerManager.DefaultKnownTypes);
            extraTypes.UnionWith(KnownTypes);
            var xser = CreateSerializer(itemType, extraTypes.ToArray());
            var settings = SettingsCache.GetOrAdd((Encoding, Indent, OmitXmlDeclaration), tpl => new XmlWriterSettings
            {
                Encoding = tpl.Item1,
                Indent = tpl.Item2,
                OmitXmlDeclaration = tpl.Item3
            });
            var xns = new XmlSerializerNamespaces();
            if (RemoveNamespaces)
                xns.Add(string.Empty, string.Empty);
            foreach (var nspace in Namespaces)
                xns.Add(nspace.Key, nspace.Value);

            using (var xwriter = XmlWriter.Create(stream, settings))
                xser.Serialize(xwriter, item, xns);
        }

        /// <inheritdoc />
        /// <summary>
        /// Make a deep clone of the object
        /// </summary>
        /// <returns>A brand new object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ISerializer DeepClone()
        {
            var nSerializer = new XmlTextSerializer
            {
                Compressor = Compressor,
                Encoding = Encoding,
                UseFileExtensions = UseFileExtensions,
                Indent = Indent,
                OmitXmlDeclaration = OmitXmlDeclaration,
                RemoveNamespaces = RemoveNamespaces
            };
            nSerializer.KnownTypes.UnionWith(KnownTypes);
            return nSerializer;
        }
    }

    /// <summary>
    /// Xml Serializer extensions
    /// </summary>
    public static class XmlTextSerializerExtensions
    {
        /// <summary>
        /// Serializer used by the extensions
        /// </summary>
        public static XmlTextSerializer Serializer { get; } = new XmlTextSerializer();

        /// <summary>
        /// Serialize object to xml
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <returns>Serialized xml value</returns>
        public static string SerializeToXml<T>(this T item) => Serializer.SerializeToString(item);
        /// <summary>
        /// Deserialize xml value to an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Serialized xml value</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromXml<T>(this string value) => Serializer.DeserializeFromString<T>(value);
        /// <summary>
        /// Serialize object to xml
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <returns>Serialized xml value</returns>
        public static SubArray<byte> SerializeToXmlBytes<T>(this T item) => Serializer.Serialize(item);
        /// <summary>
        /// Deserialize xml value to an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Serialized xml value</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromXmlBytes<T>(this byte[] value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Deserialize xml value to an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Serialized xml value</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromXmlBytes<T>(this SubArray<byte> value) => Serializer.Deserialize<T>(value);

        /// <summary>
        /// Serialize object to xml and write it into the stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="stream">Destination stream</param>
        public static void SerializeToXml<T>(this T item, Stream stream) => Serializer.Serialize(item, stream);
        /// <summary>
        /// Deserialize a stream content in xml and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromXml<T>(this Stream stream) => Serializer.Deserialize<T>(stream);
        /// <summary>
        /// Serialize object to xml and write it into a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="filePath">Destination File path</param>
        public static void SerializeToXmlFile<T>(this T item, string filePath) => Serializer.SerializeToFile(item, filePath);
        /// <summary>
        /// Deserialize a file content in xml and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filePath">File source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromXmlFile<T>(this string filePath) => Serializer.DeserializeFromFile<T>(filePath);
    }
}
