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
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
// ReSharper disable UnusedMember.Global

namespace TWCore.Serialization
{
    /// <inheritdoc />
    /// <summary>
    /// Binary Formatter serializer
    /// </summary>
    [Obsolete]
    public class BinaryFormatterSerializer : BinarySerializer
    {
        /// <inheritdoc />
        /// <summary>
        /// Supported file extensions
        /// </summary>
        public override string[] Extensions { get; } = { ".bin" };
        /// <inheritdoc />
        /// <summary>
        /// Supported mime types
        /// </summary>
        public override string[] MimeTypes { get; } = { "application/binary-formatter" };
        /// <summary>
        /// Internal type serialization binder
        /// </summary>
        public SerializationBinder Binder { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Gets the object instance deserialized from a stream
        /// </summary>
        /// <param name="stream">Deserialized stream value</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete]
        protected override object OnDeserialize(Stream stream, Type itemType)
        {
            var bformatter = new BinaryFormatter();
            if (Binder != null)
                bformatter.Binder = Binder;
            return bformatter.Deserialize(stream);
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
        [Obsolete]
        protected override void OnSerialize(Stream stream, object item, Type itemType)
        {
            var bformatter = new BinaryFormatter();
            if (Binder != null)
                bformatter.Binder = Binder;
            bformatter.Serialize(stream, item);
        }
        /// <inheritdoc />
        /// <summary>
        /// Make a deep clone of the object
        /// </summary>
        /// <returns>A brand new object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ISerializer DeepClone()
        {
            var nSerializer = new BinaryFormatterSerializer
            {
                Compressor = Compressor?.DeepClone(),
                Binder = Binder,
                UseFileExtensions = UseFileExtensions
            };
            nSerializer.KnownTypes.UnionWith(KnownTypes);
            return nSerializer;
        }
    }

    /// <summary>
    /// BinaryFormatter serializer extensions
    /// </summary>
    public static class BinaryFormatterSerializerExtensions
    {
        /// <summary>
        /// Serializer used by the extensions
        /// </summary>
        public static BinaryFormatterSerializer Serializer { get; } = new BinaryFormatterSerializer();

        /// <summary>
        /// Serialize object to binary formatter
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <returns>Serialized binary formatter value</returns>
        public static MultiArray<byte> SerializeToBinFormatter<T>(this T item) => Serializer.Serialize(item);
        /// <summary>
        /// Deserialize a binary formatter value to an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Serialized binary formatter value</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromBinFormatter<T>(this MultiArray<byte> value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Deserialize a binary formatter value to an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Serialized binary formatter value</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromBinFormatter<T>(this byte[] value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Serialize object to a binary formatter value and write it into the stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="stream">Destination stream</param>
        public static void SerializeToBinFormatter<T>(this T item, Stream stream) => Serializer.Serialize(item, stream);
        /// <summary>
        /// Deserialize a stream content in binary formatter and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromBinFormatter<T>(this Stream stream) => Serializer.Deserialize<T>(stream);
        /// <summary>
        /// Serialize object to a binary formatter and write it into a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="filePath">Destination File path</param>
        public static void SerializeToBinFormatterFile<T>(this T item, string filePath) => Serializer.SerializeToFile(item, filePath);
        /// <summary>
        /// Deserialize a file content in binary formatter and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filePath">File source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromBinFormatterFile<T>(this string filePath) => Serializer.DeserializeFromFile<T>(filePath);
    }
}