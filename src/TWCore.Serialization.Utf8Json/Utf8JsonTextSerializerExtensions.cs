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

using System.IO;
using TWCore.Serialization.Utf8Json;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace TWCore.Serialization
{
    /// <summary>
    /// Ut8Json Serializer extensions
    /// </summary>
    public static class Utf8JsonTextSerializerExtensions
    {
        /// <summary>
        /// Serializer used by the extensions
        /// </summary>
        public static Utf8JsonTextSerializer Serializer { get; } = new Utf8JsonTextSerializer();


        /// <summary>
        /// Serialize object to json
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <returns>Serialized json value</returns>
        public static string SerializeToUtf8Json<T>(this T item) => Serializer.SerializeToString(item);
        /// <summary>
        /// Deserialize json value to an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Serialized json value</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromUtf8Json<T>(this string value) => Serializer.DeserializeFromString<T>(value);
        /// <summary>
        /// Serialize object to json
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <returns>Serialized json value</returns>
        public static MultiArray<byte> SerializeToUtf8JsonBytes<T>(this T item) => Serializer.Serialize(item);
        /// <summary>
        /// Deserialize json value to an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Serialized json value</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromUtf8JsonBytes<T>(this byte[] value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Deserialize json value to an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Serialized json value</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromUtf8JsonBytes<T>(this MultiArray<byte> value) => Serializer.Deserialize<T>(value);


        /// <summary>
        /// Serialize object to json and write it into the stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="stream">Destination stream</param>
        public static void SerializeToUtf8Json<T>(this T item, Stream stream) => Serializer.Serialize(item, stream);
        /// <summary>
        /// Deserialize a stream content in json and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromUtf8Json<T>(this Stream stream) => Serializer.Deserialize<T>(stream);
        /// <summary>
        /// Serialize object to json and write it into a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="filePath">Destination File path</param>
        public static void SerializeToUtf8JsonFile<T>(this T item, string filePath) => Serializer.SerializeToFile(item, filePath);
        /// <summary>
        /// Deserialize a file content in json and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filePath">File source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromUtf8JsonFile<T>(this string filePath) => Serializer.DeserializeFromFile<T>(filePath);
    }
}
