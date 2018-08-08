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
// ReSharper disable UnusedMember.Global

namespace TWCore.Serialization.NSerializer
{
    /// <summary>
    /// NBinary Serializer
    /// </summary>
    public class NBinarySerializer : BinarySerializer
    {
        private static readonly string[] SExtensions = { ".nbin" };
        private static readonly string[] SMimeTypes = { SerializerMimeTypes.NBinary };
        private static readonly ReferencePool<SerializersTable> SerPool = new ReferencePool<SerializersTable>(1);
        private static readonly ReferencePool<DeserializersTable> DeserPool = new ReferencePool<DeserializersTable>(1);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override object OnDeserialize(Stream stream, Type itemType)
        {
            var des = DeserPool.New();
            var value = des.Deserialize(stream);
            DeserPool.Store(des);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnSerialize(Stream stream, object item, Type itemType)
        {
            var ser = SerPool.New();
            ser.Serialize(stream, item, itemType);
            SerPool.Store(ser);
        }

        /// <inheritdoc />
        /// <summary>
        /// Make a deep clone of the object
        /// </summary>
        /// <returns>A brand new object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ISerializer DeepClone()
        {
            var nSerializer = new NBinarySerializer
            {
                Compressor = Compressor,
                UseFileExtensions = UseFileExtensions,
                EnableCache = EnableCache,
                CacheTimeout = CacheTimeout
            };
            return nSerializer;
        }
    }
}
