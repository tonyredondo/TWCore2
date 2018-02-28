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

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace TWCore.Serialization
{
    /// <inheritdoc />
    /// <summary>
    /// Json Serializer
    /// </summary>
    public sealed class JsonTextSerializer : TextSerializer
    {
        private static readonly NonBlocking.ConcurrentDictionary<(bool, bool, TypeNameHandling, bool, bool, bool), JsonSerializer> SerializerSettings = new NonBlocking.ConcurrentDictionary<(bool, bool, TypeNameHandling, bool, bool, bool), JsonSerializer>();
        private static readonly string[] SExtensions = { ".json" };
        private static readonly string[] SMimeTypes = { SerializerMimeTypes.Json, "text/json" };

        #region Default Values
        /// <summary>
        /// Default encoding to use when serializing or deserializing.
        /// </summary>
        public static Encoding DefaultEncoding = new UTF8Encoding(false);
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
        /// Gets or sets if the serialized json result should be indented
        /// </summary>
        public bool Indent { get; set; }
        /// <summary>
        /// Gets or sets if the properties should be serialized in CammelCase, false if is PascalCase
        /// </summary>
        public bool UseCamelCase { get; set; }
        /// <summary>
        /// Ignore Null Values
        /// </summary>
        public bool IgnoreNullValues { get; set; } = false;
        /// <summary>
        /// Gets or sets the name handling for types
        /// </summary>
		public TypeNameHandling NameHandling { get; set; } = TypeNameHandling.Auto;
		/// <summary>
		/// Gets or sets the setting for serializing Enums as integer or as string.
		/// </summary>
		public bool EnumsAsStrings { get; set; }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Json Serializer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonTextSerializer()
        {
            Encoding = DefaultEncoding;
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private JsonSerializer GetSerializer()
        {
            return SerializerSettings.GetOrAdd((Indent, UseCamelCase, NameHandling, IgnoreNullValues, EnumsAsStrings, true), tpl =>
            {
                var ser = new JsonSerializerSettings
                {
                    Formatting = tpl.Item1 ? Formatting.Indented : Formatting.None,
                    ContractResolver = tpl.Item2 ? new CamelCasePropertyNamesContractResolver() : new DefaultContractResolver(),
                    TypeNameHandling = tpl.Item3,
                    NullValueHandling = tpl.Item4 ? NullValueHandling.Ignore : NullValueHandling.Include,
                    CheckAdditionalContent = tpl.Item6
                };
                if (tpl.Item5)
                    ser.Converters = new JsonConverter[] { new Newtonsoft.Json.Converters.StringEnumConverter() };
                return JsonSerializer.Create(ser);
            });
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
            var ser = GetSerializer();
            using (var sr = new StreamReader(stream, Encoding, true, 1024, true))
                return ser.Deserialize(sr, itemType);
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
            var ser = GetSerializer();
            using (var sw = new StreamWriter(stream, Encoding, 1024, true))
                ser.Serialize(sw, item, itemType);
        }

        /// <inheritdoc />
        /// <summary>
        /// Make a deep clone of the object
        /// </summary>
        /// <returns>A brand new object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ISerializer DeepClone()
        {
            var nSerializer = new JsonTextSerializer
            {
                Compressor = Compressor,
                UseFileExtensions = UseFileExtensions,
                Encoding = Encoding,
                Indent = Indent,
                NameHandling = NameHandling,
                UseCamelCase = UseCamelCase,
				EnumsAsStrings = EnumsAsStrings
            };
            nSerializer.KnownTypes.UnionWith(KnownTypes);
            return nSerializer;
        }
    }

    /// <summary>
    /// Json Serializer extensions
    /// </summary>
    public static class JsonTextSerializerExtensions
    {
        /// <summary>
        /// Serializer used by the extensions
        /// </summary>
        public static JsonTextSerializer Serializer { get; } = new JsonTextSerializer();


        /// <summary>
        /// Serialize object to json
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <returns>Serialized json value</returns>
        public static string SerializeToJson<T>(this T item) => Serializer.SerializeToString(item);
        /// <summary>
        /// Deserialize json value to an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Serialized json value</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromJson<T>(this string value) => Serializer.DeserializeFromString<T>(value);
        /// <summary>
        /// Serialize object to json
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <returns>Serialized json value</returns>
        public static SubArray<byte> SerializeToJsonBytes<T>(this T item) => Serializer.Serialize(item);
        /// <summary>
        /// Deserialize json value to an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Serialized json value</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromJsonBytes<T>(this byte[] value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Deserialize json value to an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Serialized json value</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromJsonBytes<T>(this SubArray<byte> value) => Serializer.Deserialize<T>(value);


        /// <summary>
        /// Serialize object to json and write it into the stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="stream">Destination stream</param>
        public static void SerializeToJson<T>(this T item, Stream stream) => Serializer.Serialize(item, stream);
        /// <summary>
        /// Deserialize a stream content in json and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromJson<T>(this Stream stream) => Serializer.Deserialize<T>(stream);
        /// <summary>
        /// Serialize object to json and write it into a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="filePath">Destination File path</param>
        public static void SerializeToJsonFile<T>(this T item, string filePath) => Serializer.SerializeToFile(item, filePath);
        /// <summary>
        /// Deserialize a file content in json and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filePath">File source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromJsonFile<T>(this string filePath) => Serializer.DeserializeFromFile<T>(filePath);
    }
}
