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
using TWCore.Serialization.RawSerializer;

namespace TWCore.Serialization
{
    /// <summary>
    /// RawBinary serializer extensions
    /// </summary>
    public static class RawBinarySerializerExtensions
    {
        /// <summary>
        /// Serializer used by the extensions
        /// </summary>
        public static RawBinarySerializer Serializer { get; } = new RawBinarySerializer();

        /// <summary>
        /// Serialize object using RawBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <returns>WBinary serialized object</returns>
        public static SubArray<byte> SerializeToRawBinary<T>(this T item) => Serializer.Serialize(item);
        /// <summary>
        /// Deserialize an object using the RawBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">RawBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromRawBinary<T>(this byte[] value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Deserialize an object using the RawBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">RawBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromRawBinary<T>(this SubArray<byte> value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Serialize object using RawBinary and write it into the stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="stream">Destination stream</param>
        public static void SerializeToRawBinary<T>(this T item, Stream stream) => Serializer.Serialize(item, stream);
        /// <summary>
        /// Deserialize a stream content using RawBinary and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromRawBinary<T>(this Stream stream) => Serializer.Deserialize<T>(stream);
        /// <summary>
        /// Serialize object using RawBinary and write it into a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="filePath">Destination File path</param>
        public static void SerializeToRawBinaryFile<T>(this T item, string filePath) => Serializer.SerializeToFile(item, filePath);
        /// <summary>
        /// Deserialize a file content using RawBinary and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filePath">File source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromRawBinaryFile<T>(this string filePath) => Serializer.DeserializeFromFile<T>(filePath);
    }
}