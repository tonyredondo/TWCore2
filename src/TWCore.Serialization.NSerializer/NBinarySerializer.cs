﻿/*
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
        private static readonly string[] SExtensions = {".nbin"};
        private static readonly string[] SMimeTypes = {SerializerMimeTypes.NBinary};
        private static readonly ObjectPool<SerializersTable, SerializersTableAllocator> SerPool = new ObjectPool<SerializersTable, SerializersTableAllocator>();

        #region Allocator

        private struct SerializersTableAllocator : IPoolObjectLifecycle<SerializersTable>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;

            public SerializersTable New()
            {
                var sTable = new SerializersTable();
                sTable.Init();
                return sTable;
            }

            public void Reset(SerializersTable value)
            {
                value.Clear();
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override object OnDeserialize(Stream stream, Type itemType)
        {
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnSerialize(Stream stream, object item, Type itemType)
        {
            var ser = SerPool.New();
            ser.SetStream(stream);
            ser.WriteObjectValue(item);
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
            };
            return nSerializer;
        }
    }
}
