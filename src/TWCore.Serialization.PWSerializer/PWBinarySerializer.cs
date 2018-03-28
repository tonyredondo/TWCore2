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
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace TWCore.Serialization.PWSerializer
{
    /// <inheritdoc />
    /// <summary>
    /// PW Binary Serializer
    /// </summary>
    public class PWBinarySerializer : BinarySerializer
    {
        private static readonly string[] sExtensions = { ".pwbin" };
        private static readonly string[] sMimeTypes = { SerializerMimeTypes.PWBinary };
        private static readonly ReferencePool<PWSerializerCore> _pool = ReferencePool<PWSerializerCore>.Shared;

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Supported file extensions
        /// </summary>
        public override string[] Extensions => sExtensions;
        /// <inheritdoc />
        /// <summary>
        /// Supported mime types
        /// </summary>
        public override string[] MimeTypes => sMimeTypes;
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override object OnDeserialize(Stream stream, Type itemType)
        {
            var ser = _pool.New();
            var obj = ser.Deserialize(stream, itemType);
            _pool.Store(ser);
            return obj;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnSerialize(Stream stream, object item, Type itemType)
        {
            var ser = _pool.New();
            ser.Serialize(stream, item);
            _pool.Store(ser);
        }

        /// <inheritdoc />
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
            };
            return nSerializer;
        }
    }
}
