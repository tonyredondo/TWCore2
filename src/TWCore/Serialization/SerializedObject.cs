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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Compression;

namespace TWCore.Serialization
{
    [DataContract, Serializable]
    public sealed class SerializedObject
    {
        #region Properties
        /// <summary>
        /// Item Data
        /// </summary>
        [DataMember]
        public byte[] Data { get; set; }
        /// <summary>
        /// Item Data Type
        /// </summary>
        [XmlAttribute, DataMember]
        public string DataType { get; set; }
        /// <summary>
        /// Serializer Mime Type
        /// </summary>
        [XmlAttribute, DataMember]
        public string SerializerMimeType { get; set; }
        #endregion

        #region .ctor
        public SerializedObject() { }
        public SerializedObject(object data) : this(data, SerializerManager.DefaultBinarySerializer) { }
        public SerializedObject(object data, ISerializer serializer)
        {
            if (data == null) return;
            var type = data.GetType();
            DataType = type.GetTypeName();
            SerializerMimeType = serializer.MimeTypes[0] + ((serializer.Compressor != null) ? ":" + serializer.Compressor.EncodingType : string.Empty);
            Data = (byte[])serializer.Serialize(data, type);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get Deserialized Value
        /// </summary>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetValue()
        {
            if (Data == null || Data.Length == 0) return null;
            var type = string.IsNullOrWhiteSpace(DataType) ? typeof(object) : Core.GetType(DataType, true);
            ISerializer serializer;
            if (string.IsNullOrWhiteSpace(SerializerMimeType))
                serializer = SerializerManager.DefaultBinarySerializer;
            else
            {
                var idx = SerializerMimeType.IndexOf(':');
                var serMime = idx < 0 ? SerializerMimeType : SerializerMimeType.Substring(0, idx);
                var serComp = idx < 0 ? null : SerializerMimeType.Substring(idx + 1);
                serializer = SerializerManager.GetByMimeType(serMime);
                if (!string.IsNullOrWhiteSpace(serComp))
                    serializer.Compressor = CompressorManager.GetByEncodingType(serComp);
            }
            return serializer.Deserialize(Data, type);
        }
        #endregion
    }
}
