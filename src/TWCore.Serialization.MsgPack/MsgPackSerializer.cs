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
using System.Reflection;
using System.Runtime.CompilerServices;
using MessagePack;
using NonBlocking;

namespace TWCore.Serialization.MsgPack
{
    /// <inheritdoc />
    /// <summary>
    /// MsgPack Serializer
    /// </summary>
    public class MsgPackSerializer : BinarySerializer
    {
        private static readonly string[] SExtensions = { ".msgpack" };
        private static readonly string[] SMimeTypes = { "application/msgpack", "application/x-msgpack" };
        private static MethodInfo SerializeMethod;
        private static MethodInfo DeserializeMethod;
        private static ConcurrentDictionary<Type, MethodInfo> SerializeMethods = new ConcurrentDictionary<Type, MethodInfo>();
        private static ConcurrentDictionary<Type, MethodInfo> DeserializeMethods = new ConcurrentDictionary<Type, MethodInfo>();

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

        static MsgPackSerializer()
        {
            SerializeMethod = typeof(MessagePackSerializer).GetMethods().FirstOf(method =>
            {
                if (method.Name != "Serialize") return false;
                if (method.GetParameters().Length != 3) return false;
                return true;
            });
            DeserializeMethod = typeof(MessagePackSerializer).GetMethods().FirstOf(method =>
            {
                if (method.Name != "Deserialize") return false;
                if (method.GetParameters().Length != 3) return false;
                return true;
            });
        }

        /// <summary>
        /// On deserialize
        /// </summary>
        /// <param name="stream">Stream source</param>
        /// <param name="itemType">Item type</param>
        /// <returns>Deserialized item</returns>
        /// <exception cref="Exception">On target invocation in the reflection</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override object OnDeserialize(Stream stream, Type itemType)
        {
            using(var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                ms.Position = 0;
                try
                {
                    var deserialize = DeserializeMethods.GetOrAdd(itemType, type => DeserializeMethod.MakeGenericMethod(type));
                    return deserialize.Invoke(null, new object[] { stream, MessagePack.Resolvers.ContractlessStandardResolver.Instance, false });
                }
                catch(TargetInvocationException tie)
                {
                    throw tie.InnerException;
                }
            }
        }
        
        /// <summary>
        /// On Serialize
        /// </summary>
        /// <param name="stream">Stream destination</param>
        /// <param name="item">Item to serialize</param>
        /// <param name="itemType">Item type to serialize</param>
        /// <exception cref="Exception">On target invocation in the reflection</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnSerialize(Stream stream, object item, Type itemType)
        {
            try
            {
                var serialize = SerializeMethods.GetOrAdd(itemType, type => SerializeMethod.MakeGenericMethod(type));
                serialize.Invoke(null, new[] { stream, item, MessagePack.Resolvers.ContractlessStandardResolver.Instance });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
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
            var nSerializer = new MsgPackSerializer
            {
                Compressor = Compressor,
                UseFileExtensions = UseFileExtensions
            };
            return nSerializer;
        }
    }
}
