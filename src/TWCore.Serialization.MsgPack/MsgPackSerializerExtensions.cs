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
using TWCore.Serialization.MsgPack;
// ReSharper disable CheckNamespace

namespace TWCore.Serialization
{
    /// <summary>
    /// MsgPack extensions
    /// </summary>
    public static class MsgPackSerializerExtensions
    {
        /// <summary>
        /// Serializer used by the extensions
        /// </summary>
        public static MsgPackSerializer Serializer { get; } = new MsgPackSerializer();

        /// <summary>
        /// Serialize object using MsgPack serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <returns>TBinary serialized object</returns>
        public static SubArray<byte> SerializeToMsgPack<T>(this T item) => Serializer.Serialize(item);
        /// <summary>
        /// Deserialize an object using the MsgPack serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">TBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromMsgPack<T>(this byte[] value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Deserialize an object using the MsgPack serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">TBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromMsgPack<T>(this SubArray<byte> value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Deserialize an object using the MsgPack serializer
        /// </summary>
        /// <param name="value">TBinary serialized object</param>
        /// <param name="type">Object type</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromMsgPack(this byte[] value, Type type) => Serializer.Deserialize(value, type);
        /// <summary>
        /// Deserialize an object using the MsgPack serializer
        /// </summary>
        /// <param name="value">TBinary serialized object</param>
        /// <param name="type">Object type</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromMsgPack(this SubArray<byte> value, Type type) => Serializer.Deserialize(value, type);
        /// <summary>
        /// Serialize object using MsgPack and write it into the stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="stream">Destination stream</param>
        public static void SerializeToMsgPack<T>(this T item, Stream stream) => Serializer.Serialize(item, stream);
        /// <summary>
        /// Deserialize a stream content using MsgPack and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromMsgPack<T>(this Stream stream) => Serializer.Deserialize<T>(stream);
        /// <summary>
        /// Deserialize an object using the MsgPack serializer
        /// </summary>
        /// <param name="stream">Stream source with the serialized data</param>
        /// <param name="type">Object type</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromMsgPack(this Stream stream, Type type) => Serializer.Deserialize(stream, type);
        /// <summary>
        /// Serialize object using MsgPack and write it into a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="filePath">Destination File path</param>
        public static void SerializeToMsgPackFile<T>(this T item, string filePath) => Serializer.SerializeToFile(item, filePath);
        /// <summary>
        /// Deserialize a file content using MsgPack and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filePath">File source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromMsgPackFile<T>(this string filePath) => Serializer.DeserializeFromFile<T>(filePath);
        /// <summary>
        /// Deserialize an object using the MsgPack serializer
        /// </summary>
        /// <param name="filePath">File source with the serialized data</param>
        /// <param name="type">Object type</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromMsgPackFile(this string filePath, Type type) => Serializer.DeserializeFromFile(type, filePath);
    }
}
