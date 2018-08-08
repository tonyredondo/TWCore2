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
using System.Text;
using Utf8Json;
using Utf8Json.Resolvers;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace TWCore.Serialization.Utf8Json
{
    /// <inheritdoc />
    /// <summary>
    /// Utf8Json Serializer
    /// </summary>
    public sealed class Utf8JsonTextSerializer : TextSerializer
    {
        private static readonly string[] SExtensions = { ".json" };
        private static readonly string[] SMimeTypes = { SerializerMimeTypes.Json, "text/json" };
        private IJsonFormatterResolver _resolver;
        private bool _indent;
        private bool _useCamelCase;
        private bool _ignoreNullValues;

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
        /// <summary>
        /// Gets or sets if the serialized json result should be indented
        /// </summary>
        public bool Indent
        {
            get => _indent;
            set
            {
                _indent = value;
                UpdateResolver();
            }
        }
        /// <summary>
        /// Gets or sets if the properties should be serialized in CammelCase, false if is PascalCase
        /// </summary>
        public bool UseCamelCase
        {
            get => _useCamelCase;
            set
            {
                _useCamelCase = value;
                UpdateResolver();
            }
        }
        /// <summary>
        /// Ignore Null Values
        /// </summary>
        public bool IgnoreNullValues
        {
            get => _ignoreNullValues;
            set
            {
                _ignoreNullValues = value;
                UpdateResolver();
            }
        }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Json Serializer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8JsonTextSerializer()
        {
            UpdateResolver();
        }
        #endregion
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateResolver()
        {
            if (UseCamelCase && IgnoreNullValues)
            {
                _resolver = StandardResolver.ExcludeNullCamelCase;
                return;
            }
            if (UseCamelCase)
            {
                _resolver = StandardResolver.CamelCase;
                return;
            }
            if(IgnoreNullValues)
            {
                _resolver = StandardResolver.ExcludeNull;
                return;
            }
            _resolver = StandardResolver.Default;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the object instance deserialized from a stream
        /// </summary>
        /// <param name="stream">Deserialized stream value</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override object OnDeserialize(Stream stream, Type itemType)
            => JsonSerializer.NonGeneric.Deserialize(itemType, stream, _resolver);

        /// <inheritdoc />
        /// <summary>
        /// Serialize an object and writes it to the stream
        /// </summary>
        /// <param name="stream">Destination stream</param>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Deserialized byte array value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnSerialize(Stream stream, object item, Type itemType)
            => JsonSerializer.NonGeneric.Serialize(itemType, stream, item, _resolver);

        /// <inheritdoc />
        /// <summary>
        /// Make a deep clone of the object
        /// </summary>
        /// <returns>A brand new object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ISerializer DeepClone()
        {
            var nSerializer = new JsonTextSerializer
            {
                Compressor = Compressor,
                UseFileExtensions = UseFileExtensions,
                Encoding = Encoding,
                Indent = Indent,
                UseCamelCase = UseCamelCase,
                EnableCache = EnableCache,
                CacheTimeout = CacheTimeout
            };
            nSerializer.KnownTypes.UnionWith(KnownTypes);
            return nSerializer;
        }
    }
}
