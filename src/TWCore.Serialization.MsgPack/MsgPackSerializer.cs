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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MP = ProGaudi.MsgPack.Light;

namespace TWCore.Serialization
{
    /// <summary>
    /// MsgPack Serializer
    /// </summary>
    public class MsgPackSerializer : BinarySerializer
    {
        static readonly string[] _extensions = new string[] { ".msgpack" };
        static readonly string[] _mimeTypes = new string[] { "application/msgpack", "application/x-msgpack" };
        static readonly MethodInfo DeserializeMethod;
        static readonly MethodInfo SerializeMethod;

        #region Properties
        /// <summary>
        /// Supported file extensions
        /// </summary>
        public override string[] Extensions => _extensions;
        /// <summary>
        /// Supported mime types
        /// </summary>
        public override string[] MimeTypes => _mimeTypes;
        #endregion

        static MsgPackSerializer()
        {
            var methods = typeof(MP.MsgPackSerializer).GetMethods();
            DeserializeMethod = methods.First(m => m.Name == "Deserialize" && m.GetParameters().Length == 1);
            SerializeMethod = methods.First(m => m.Name == "Serialize" && m.GetParameters().Length == 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override object OnDeserialize(Stream stream, Type itemType)
        {
            using(var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                ms.Position = 0;
                try
                {
                    return DeserializeMethod.MakeGenericMethod(itemType).Invoke(null, new object[] { ms });
                }
                catch(TargetInvocationException tie)
                {
                    throw tie.InnerException;
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnSerialize(Stream stream, object item, Type itemType)
        {
            try
            {
                var data = (byte[])SerializeMethod.MakeGenericMethod(itemType).Invoke(null, new object[] { item });
                stream.WriteBytes(data);
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        /// <summary>
        /// Make a deep clone of the object
        /// </summary>
        /// <returns>A brand new object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ISerializer DeepClone()
        {
            var nSerializer = new MsgPackSerializer
            {
                Compressor = Compressor,
                UseFileExtensions = UseFileExtensions,
            };
            return nSerializer;
        }
    }

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
        public static SubArray<byte> SerializeToMsgPack<T>(this T item) => Serializer.Serialize<T>(item);
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
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">TBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromMsgPack(this byte[] value, Type type) => Serializer.Deserialize(value, type);
        /// <summary>
        /// Deserialize an object using the MsgPack serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">TBinary serialized object</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromMsgPack(this SubArray<byte> value, Type type) => Serializer.Deserialize(value, type);
        /// <summary>
        /// Serialize object using MsgPack and write it into the stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="stream">Destination stream</param>
        public static void SerializeToMsgPack<T>(this T item, Stream stream) => Serializer.Serialize<T>(item, stream);
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
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromMsgPack(this Stream stream, Type type) => Serializer.Deserialize(stream, type);
        /// <summary>
        /// Serialize object using MsgPack and write it into a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="filePath">Destination File path</param>
        public static void SerializeToMsgPackFile<T>(this T item, string filePath) => Serializer.SerializeToFile<T>(item, filePath);
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
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filePath">File source with the serialized data</param>
        /// <returns>Object instance</returns>
        public static object DeserializeFromMsgPackFile(this string filePath, Type type) => Serializer.DeserializeFromFile(type, filePath);
    }
}
