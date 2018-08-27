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
using TWCore.Serialization.NSerializer;

namespace TWCore.Serialization
{
    /// <summary>
    /// NBinary serializer extensions
    /// </summary>
    public static class NBinarySerializerExtensions
    {
        /// <summary>
        /// Serializer used by the extensions
        /// </summary>
        public static NBinarySerializer Serializer { get; } = new NBinarySerializer();

        /// <summary>
        /// Serialize object using NBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <returns>WBinary serialized object</returns>
        public static MultiArray<byte> SerializeToNBinary<T>(this T item) => Serializer.Serialize(item);
        /// <summary>
        /// Deserialize an object using the NBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">NBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromNBinary<T>(this byte[] value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Deserialize an object using the NBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">NBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromNBinary<T>(this MultiArray<byte> value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Serialize object using NBinary and write it into the stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="stream">Destination stream</param>
        public static void SerializeToNBinary<T>(this T item, Stream stream) => Serializer.Serialize(item, stream);
        /// <summary>
        /// Deserialize a stream content using NBinary and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromNBinary<T>(this Stream stream) => Serializer.Deserialize<T>(stream);
        /// <summary>
        /// Serialize object using NBinary and write it into a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="filePath">Destination File path</param>
        public static void SerializeToNBinaryFile<T>(this T item, string filePath) => Serializer.SerializeToFile(item, filePath);
        /// <summary>
        /// Deserialize a file content using NBinary and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filePath">File source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromNBinaryFile<T>(this string filePath) => Serializer.DeserializeFromFile<T>(filePath);
    }
}