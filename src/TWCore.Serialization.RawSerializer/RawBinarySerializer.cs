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
        private static SerializersTable _serializer;
        [ThreadStatic]
        private static DeserializersTable _deserializer;

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
            var des = _deserializer;
            if (des is null)
            {
                des = new DeserializersTable();
                _deserializer = des;
            }
            if (itemType == typeof(GenericObject))
                return des.GenericObjectDeserialize(stream);
            return des.Deserialize(stream);
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
            var ser = _serializer;
            if (ser is null)
            {
                ser = new SerializersTable();
                _serializer = ser;
            }
            ser.Serialize(stream, item, itemType);
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
            var ser = _serializer;
            if (ser is null)
            {
                ser = new SerializersTable();
                _serializer = ser;
            }
            using (var stream = new RecycleMemoryStream())
            {
                if (Compressor is null)
                {
                    ser.Serialize(stream, item);
                    return stream.GetMultiArray();
                }
                using (var ms = new RecycleMemoryStream())
                {
                    ser.Serialize(ms, item);
                    ms.Position = 0;
                    Compressor.Compress(ms, stream);
                }
                return stream.GetMultiArray();
            }
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
            var ser = _serializer;
            if (ser is null)
            {
                ser = new SerializersTable();
                _serializer = ser;
            }
            if (Compressor is null)
            {
                ser.Serialize(stream, item);
                return;
            }
            using (var ms = new RecycleMemoryStream())
            {
                ser.Serialize(ms, item);
                ms.Position = 0;
                Compressor.Compress(ms, stream);
            }
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
