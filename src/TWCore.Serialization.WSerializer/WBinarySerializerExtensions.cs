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

using System.IO;
using TWCore.Serialization.WSerializer;

namespace TWCore.Serialization
{
    /// <summary>
    /// W serializer extensions
    /// </summary>
    public static class WBinarySerializerExtensions
    {
        /// <summary>
        /// Serializer used by the extensions
        /// </summary>
        public static WBinarySerializer Serializer { get; } = new WBinarySerializer();

        /// <summary>
        /// Serialize object using WBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <returns>WBinary serialized object</returns>
        public static SubArray<byte> SerializeToWBinary<T>(this T item) => Serializer.Serialize<T>(item);
        /// <summary>
        /// Deserialize an object using the WBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">TBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromWBinary<T>(this byte[] value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Deserialize an object using the WBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">WBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromWBinary<T>(this SubArray<byte> value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Serialize object using WBinary and write it into the stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="stream">Destination stream</param>
        public static void SerializeToWBinary<T>(this T item, Stream stream) => Serializer.Serialize<T>(item, stream);
        /// <summary>
        /// Deserialize a stream content using WBinary and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromWBinary<T>(this Stream stream) => Serializer.Deserialize<T>(stream);
        /// <summary>
        /// Serialize object using WBinary and write it into a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="filePath">Destination File path</param>
        public static void SerializeToWBinaryFile<T>(this T item, string filePath) => Serializer.SerializeToFile<T>(item, filePath);
        /// <summary>
        /// Deserialize a file content using WBinary and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filePath">File source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromWBinaryFile<T>(this string filePath) => Serializer.DeserializeFromFile<T>(filePath);
    }
}
