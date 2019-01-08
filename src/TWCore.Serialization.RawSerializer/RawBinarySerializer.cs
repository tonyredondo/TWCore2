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
using TWCore.IO;
// ReSharper disable UnusedMember.Global

namespace TWCore.Serialization.RawSerializer
{
    /// <summary>
    /// RawBinary Serializer
    /// </summary>
    public class RawBinarySerializer : BinarySerializer
    {
        private static readonly string[] SExtensions = { ".rawbin" };
        private static readonly string[] SMimeTypes = { SerializerMimeTypes.NBinary };
        [ThreadStatic] 
        private static SerTools _tools;
        
        #region Nested Type
        private class SerTools
        {
            public readonly SerializersTable Serializer;
            public readonly DeserializersTable Deserializer;
            public readonly RecycleMemoryStream SerStream;
            public readonly RecycleMemoryStream ComStream;
            public SerTools()
            {
                Serializer = new SerializersTable();
                Deserializer = new DeserializersTable();
                SerStream = new RecycleMemoryStream();
                ComStream = new RecycleMemoryStream();
            }
        }
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

        #endregion

        /// <summary>
        /// Gets the object instance deserialized from a stream
        /// </summary>
        /// <param name="stream">Deserialized stream value</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override object OnDeserialize(Stream stream, Type itemType)
        {
            var tools = _tools;
            if (tools is null)
            {
                tools = new SerTools();
                _tools = tools;
            }
            if (itemType == typeof(GenericObject))
                return tools.Deserializer.GenericObjectDeserialize(stream);
            return tools.Deserializer.Deserialize(stream);
        }

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
            var tools = _tools;
            if (tools is null)
            {
                tools = new SerTools();
                _tools = tools;
            }
            tools.Serializer.Serialize(stream, item, itemType);
        }

        /// <inheritdoc />
        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object to serialize</param>
        /// <returns>Serialized byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MultiArray<byte> Serialize<T>(T item)
        {
            var tools = _tools;
            if (tools is null)
            {
                tools = new SerTools();
                _tools = tools;
            }
            else
            {
                tools.SerStream.Reset();                 
            }
            if (Compressor is null)
            {
                tools.Serializer.Serialize(tools.SerStream, item);
                return tools.SerStream.GetMultiArray();
            }
            tools.ComStream.Reset();
            tools.Serializer.Serialize(tools.SerStream, item);
            tools.SerStream.Position = 0;
            Compressor.Compress(tools.SerStream, tools.ComStream);
            return tools.ComStream.GetMultiArray();
        }
        /// <inheritdoc />
        /// <summary>
        /// Serialize an object to a stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object to serialize</param>
        /// <param name="stream">Stream data destination</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Serialize<T>(T item, Stream stream)
        {
            var tools = _tools;
            if (tools is null)
            {
                tools = new SerTools();
                _tools = tools;
            }
            if (Compressor is null)
            {
                tools.Serializer.Serialize(stream, item);
                return;
            }
            tools.SerStream.Reset();
            tools.Serializer.Serialize(tools.SerStream, item);
            tools.SerStream.Position = 0;
            Compressor.Compress(tools.SerStream, stream);
        }
        /// <inheritdoc />
        /// <summary>
        /// Deserialize a stream content to a object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream data source</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override T Deserialize<T>(Stream stream)
        {
            var tools = _tools;
            if (tools is null)
            {
                tools = new SerTools();
                _tools = tools;
            }
            if (Compressor is null)
            {
                if (typeof(T) == typeof(GenericObject))
                    return (T) tools.Deserializer.GenericObjectDeserialize(stream);
                return (T) tools.Deserializer.Deserialize(stream);
            }
            tools.ComStream.Reset();
            Compressor.Decompress(stream, tools.ComStream);
            tools.ComStream.Position = 0;
            if (typeof(T) == typeof(GenericObject))
                return (T) tools.Deserializer.GenericObjectDeserialize(tools.ComStream);
            return (T) tools.Deserializer.Deserialize(tools.ComStream);
        }
        /// <inheritdoc />
        /// <summary>
        /// Deserialize a byte array value to an item type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Byte array to deserialize</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override T Deserialize<T>(MultiArray<byte> value)
        {
            var tools = _tools;
            if (tools is null)
            {
                tools = new SerTools();
                _tools = tools;
            }
            if (Compressor is null)
            {
                using (var stream = value.AsReadOnlyStream())
                    return (T) tools.Deserializer.Deserialize(stream);
            }
            tools.ComStream.Reset();
            using(var stream = value.AsReadOnlyStream())
                Compressor.Decompress(stream, tools.ComStream);
            tools.ComStream.Position = 0;
            return (T) tools.Deserializer.Deserialize(tools.ComStream);
        }
        /// <inheritdoc />
        /// <summary>
        /// Make a deep clone of the object
        /// </summary>
        /// <returns>A brand new object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ISerializer DeepClone()
        {
            var nSerializer = new RawBinarySerializer
            {
                Compressor = Compressor,
                UseFileExtensions = UseFileExtensions
            };
            return nSerializer;
        }
    }
}
