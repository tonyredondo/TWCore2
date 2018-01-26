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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TWCore.Compression;
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Serialization
{
    /// <inheritdoc />
    /// <summary>
    /// Serializer interface
    /// </summary>
    public interface ISerializer : IDeepCloneable<ISerializer>
    {
        /// <summary>
        /// Serializer Type
        /// </summary>
        SerializerType SerializerType { get; }
        /// <summary>
        /// Supported mime types
        /// </summary>
        string[] MimeTypes { get; }
        /// <summary>
        /// Supported file extensions
        /// </summary>
        string[] Extensions { get; }
        /// <summary>
        /// Known types to add to the serializer
        /// </summary>
        HashSet<Type> KnownTypes { get; }
        /// <summary>
        /// true if the serializer extension and/or compressor extension would be appended to the filePath; otherwise, false.
        /// </summary>
        bool UseFileExtensions { get; set; }
        /// <summary>
        /// Compresor used on Serialization and Deserialization of byte arrays, streams and files
        /// </summary>
        /// <remarks>This compressor is ignored on TextSerializer</remarks>
        ICompressor Compressor { get; set; }

        //+++

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="item">Object to serialize</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Serialized byte array</returns>
        SubArray<byte> Serialize(object item, Type itemType);
        /// <summary>
        /// Serialize an object to a stream
        /// </summary>
        /// <param name="item">Object to serialize</param>
        /// <param name="itemType">Object type</param>
        /// <param name="stream">Stream data destination</param>
        void Serialize(object item, Type itemType, Stream stream);
        /// <summary>
        /// Serialize an object to a filepath
        /// </summary>
        /// <param name="item">Object to serialize</param>
        /// <param name="itemType">Object type</param>
        /// <param name="filePath">File path to write the results of the serialization</param>
        void SerializeToFile(object item, Type itemType, string filePath);
        /// <summary>
        /// Serialize an object to a filepath
        /// </summary>
        /// <param name="item">Object to serialize</param>
        /// <param name="itemType">Object type</param>
        /// <param name="filePath">File path to write the results of the serialization</param>
        Task SerializeToFileAsync(object item, Type itemType, string filePath);
        
        //+++
        /// <summary>
        /// Get the Serialized Object from the instance
        /// </summary>
        /// <param name="item">Object instance</param>
        /// <returns>SerializedObject instance</returns>
        SerializedObject GetSerializedObject(object item);
        /// <summary>
        /// Get the Serialized Object from the instance
        /// </summary>
        /// <param name="item">Object instance</param>
        /// <returns>SerializedObject instance</returns>
        SerializedObject GetSerializedObject<T>(T item);

        //+++

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object to serialize</param>
        /// <returns>Serialized byte array</returns>
        SubArray<byte> Serialize<T>(T item);
        /// <summary>
        /// Serialize an object to a stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object to serialize</param>
        /// <param name="stream">Stream data destination</param>
        void Serialize<T>(T item, Stream stream);
        /// <summary>
        /// Serialize an object to a filepath
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object to serialize</param>
        /// <param name="filePath">File path to write the results of the serialization</param>
        void SerializeToFile<T>(T item, string filePath);
        /// <summary>
        /// Serialize an object to a filepath
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object to serialize</param>
        /// <param name="filePath">File path to write the results of the serialization</param>
        Task SerializeToFileAsync<T>(T item, string filePath);
        //+++

        /// <summary>
        /// Deserialize a byte array value to an item type
        /// </summary>
        /// <param name="value">Value to deserialize</param>
        /// <param name="valueType">Value type</param>
        /// <returns>Deserialized object</returns>
        object Deserialize(SubArray<byte> value, Type valueType);
        /// <summary>
        /// Deserialize a stream content to a object
        /// </summary>
        /// <param name="stream">Stream data source</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Deserialized object</returns>
        object Deserialize(Stream stream, Type itemType);
        /// <summary>
        /// Deserialize a file to an object instance
        /// </summary>
        /// <param name="itemType">Object type</param>
        /// <param name="filePath">File path to read the content to deserialize</param>
        /// <returns>Deserialized object</returns>
        object DeserializeFromFile(Type itemType, string filePath);
        //+++

        /// <summary>
        /// Deserialize a byte array value to an item type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Byte array to deserialize</param>
        /// <returns>Deserialized object</returns>
        T Deserialize<T>(SubArray<byte> value);
        /// <summary>
        /// Deserialize a stream content to a object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream data source</param>
        /// <returns>Deserialized object</returns>
        T Deserialize<T>(Stream stream);
        /// <summary>
        /// Deserialize a file to an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filePath">File path to read the content to deserialize</param>
        /// <returns>Deserialized object</returns>
        T DeserializeFromFile<T>(string filePath);
    }
}
