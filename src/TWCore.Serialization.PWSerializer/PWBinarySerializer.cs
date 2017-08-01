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
using System.Runtime.CompilerServices;

namespace TWCore.Serialization
{
    /// <summary>
    /// W Binary Serializer
    /// </summary>
    public class PWBinarySerializer : BinarySerializer
    {
        #region Properties
        /// <summary>
        /// Supported file extensions
        /// </summary>
        public override string[] Extensions { get; } = new string[] { ".pwbin" };
        /// <summary>
        /// Supported mime types
        /// </summary>
        public override string[] MimeTypes { get; } = new string[] { SerializerMimeTypes.PWBinary };
        /// <summary>
        /// Serialization mode
        /// </summary>
        public PWSerializer.SerializerMode SerializerMode { get; set; } = PWSerializer.SerializerMode.Cached2048;
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override object OnDeserialize(Stream stream, Type itemType)
        {
            var ser = new PWSerializer.PWSerializer(SerializerMode);
            return ser.Deserialize(stream, itemType);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnSerialize(Stream stream, object item, Type itemType)
        {
            var ser = new PWSerializer.PWSerializer(SerializerMode);
            ser.Serialize(stream, item);
        }

        /// <summary>
        /// Make a deep clone of the object
        /// </summary>
        /// <returns>A brand new object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ISerializer DeepClone()
        {
            var nSerializer = new PWBinarySerializer
            {
                Compressor = Compressor,
                UseFileExtensions = UseFileExtensions,
                SerializerMode = SerializerMode,
            };
            return nSerializer;
        }
    }

    /// <summary>
    /// W serializer extensions
    /// </summary>
    public static class PWBinarySerializerExtensions
    {
        /// <summary>
        /// Serializer used by the extensions
        /// </summary>
        public static PWBinarySerializer Serializer { get; } = new PWBinarySerializer();

        /// <summary>
        /// Serialize object using WBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <returns>TBinary serialized object</returns>
        public static SubArray<byte> SerializeToPWBinary<T>(this T item) => Serializer.Serialize<T>(item);
        /// <summary>
        /// Deserialize an object using the WBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">TBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromPWBinary<T>(this byte[] value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Deserialize an object using the WBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">TBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromPWBinary<T>(this SubArray<byte> value) => Serializer.Deserialize<T>(value);
        /// <summary>
        /// Deserialize an object using the WBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">TBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromPWBinary(this byte[] value, Type type) => Serializer.Deserialize(value, type);
        /// <summary>
        /// Deserialize an object using the WBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">TBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromPWBinary(this SubArray<byte> value, Type type) => Serializer.Deserialize(value, type);
        /// <summary>
        /// Serialize object using WBinary and write it into the stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="stream">Destination stream</param>
        public static void SerializeToPWBinary<T>(this T item, Stream stream) => Serializer.Serialize<T>(item, stream);
        /// <summary>
        /// Deserialize a stream content using WBinary and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromPWBinary<T>(this Stream stream) => Serializer.Deserialize<T>(stream);
        /// <summary>
        /// Deserialize an object using the WBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromPWBinary(this Stream stream, Type type) => Serializer.Deserialize(stream, type);
        /// <summary>
        /// Serialize object using WBinary and write it into a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="filePath">Destination File path</param>
        public static void SerializeToPWBinaryFile<T>(this T item, string filePath) => Serializer.SerializeToFile<T>(item, filePath);
        /// <summary>
        /// Deserialize a file content using WBinary and returns an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filePath">File source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static T DeserializeFromPWBinaryFile<T>(this string filePath) => Serializer.DeserializeFromFile<T>(filePath);
        /// <summary>
        /// Deserialize an object using the WBinary serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filePath">File source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromPWBinaryFile(this string filePath, Type type) => Serializer.DeserializeFromFile(type, filePath);
    }
}
