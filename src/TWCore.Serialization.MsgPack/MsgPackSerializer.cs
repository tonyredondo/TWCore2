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
        private static readonly MethodInfo DeserializeMethod;
        private static readonly MethodInfo SerializeMethod;

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
                var data = (byte[])SerializeMethod.MakeGenericMethod(itemType).Invoke(null, new[] { item });
                stream.WriteBytes(data);
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
