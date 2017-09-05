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
using System.IO;
using TWCore.Serialization.PWSerializer;
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace TWCore.Serialization
{

    /// <summary>
    /// PW serializer extensions
    /// </summary>
    public static class PWBinarySerializerExtensions
    {
        /// <summary>
        /// Serializer used by the extensions
        /// </summary>
        public static PWBinarySerializer Serializer { get; } = new PWBinarySerializer();

        /// <summary>
        /// Serialize object using PWBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <returns>PWBinary serialized object</returns>
        public static SubArray<byte> SerializeToPWBinary<T>(this T item) => Serializer.Serialize(item);
        /// <summary>
        /// Deserialize an object using the PWBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">PWBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromPWBinary<T>(this byte[] value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Deserialize an object using the PWBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">PWBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromPWBinary<T>(this SubArray<byte> value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Deserialize an object using the PWBinary serializer
        /// </summary>
        /// <param name="value">PWBinary serialized object</param>
        /// <param name="type">Object type</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromPWBinary(this byte[] value, Type type) => Serializer.Deserialize(value, type);
        /// <summary>
        /// Deserialize an object using the PWBinary serializer
        /// </summary>
        /// <param name="value">PWBinary serialized object</param>
        /// <param name="type">Object type</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromPWBinary(this SubArray<byte> value, Type type) => Serializer.Deserialize(value, type);
        /// <summary>
        /// Serialize object using PWBinary and write it into the stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="stream">Destination stream</param>
        public static void SerializeToPWBinary<T>(this T item, Stream stream) => Serializer.Serialize(item, stream);
        /// <summary>
        /// Deserialize a stream content using PWBinary and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromPWBinary<T>(this Stream stream) => Serializer.Deserialize<T>(stream);
        /// <summary>
        /// Deserialize an object using the PWBinary serializer
        /// </summary>
        /// <param name="stream">Stream source with the serialized data</param>
        /// <param name="type">Object type</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromPWBinary(this Stream stream, Type type) => Serializer.Deserialize(stream, type);
        /// <summary>
        /// Serialize object using PWBinary and write it into a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="filePath">Destination File path</param>
        public static void SerializeToPWBinaryFile<T>(this T item, string filePath) => Serializer.SerializeToFile(item, filePath);
        /// <summary>
        /// Deserialize a file content using PWBinary and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filePath">File source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromPWBinaryFile<T>(this string filePath) => Serializer.DeserializeFromFile<T>(filePath);
        /// <summary>
        /// Deserialize an object using the PWBinary serializer
        /// </summary>
        /// <param name="filePath">File source with the serialized data</param>
        /// <param name="type">Object type</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromPWBinaryFile(this string filePath, Type type) => Serializer.DeserializeFromFile(type, filePath);
    }
}
