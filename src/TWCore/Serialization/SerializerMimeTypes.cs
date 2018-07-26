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


// ReSharper disable InconsistentNaming
namespace TWCore.Serialization
{
    /// <summary>
    /// Serializer mime types
    /// </summary>
    public static class SerializerMimeTypes
    {
        /// <summary>
        /// Xml mime type
        /// </summary>
        public const string Xml = "application/xml";
        /// <summary>
        /// .NET Binary format mime type
        /// </summary>
        public const string BinaryFormatter = "application/binary-formatter";
        /// <summary>
        /// WSerializer binary mime type
        /// </summary>
        public const string WBinary = "application/w-binary";
        /// <summary>
        /// WSerializer binary mime type
        /// </summary>
        public const string PWBinary = "application/pw-binary";
        /// <summary>
        /// Json mime type
        /// </summary>
        public const string Json = "application/json";
        /// <summary>
        /// NSerializer binary mime type
        /// </summary>
        public const string NBinary = "application/n-binary";
        /// <summary>
        /// RawSerializer binary mime type
        /// </summary>
        public const string RawBinary = "application/raw-binary";
    }
}
