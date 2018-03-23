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
using System.Runtime.Serialization;
using System.Text;
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
        /// <summary>
        /// Get SubArray representation of the SerializedObject instance
        /// </summary>
        /// <returns>SubArray instance</returns>
        public SubArray<byte> ToSubArray()
        {
            var dataTypeByte = Encoding.UTF8.GetBytes(DataType ?? string.Empty);
            var serializerMimeTypeByte = Encoding.UTF8.GetBytes(SerializerMimeType ?? string.Empty);
            var dataByte = Data ?? new byte[0];
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write(dataTypeByte.Length);
            bw.Write(dataTypeByte);
            bw.Write(serializerMimeTypeByte.Length);
            bw.Write(serializerMimeTypeByte);
            bw.Write(dataByte.Length);
            bw.Write(dataByte);
            return ms.ToSubArray();
        }
        /// <summary>
        /// Get SerializedObject instance from the SubArray representation.
        /// </summary>
        /// <param name="byteArray">SubArray instance</param>
        /// <returns>SerializedObject instance</returns>
        public static SerializedObject FromSubArray(SubArray<byte> byteArray)
        {
            var ms = byteArray.ToMemoryStream();
            var br = new BinaryReader(ms);
            var rByte = byteArray.Count;

            var dataTypeByteLength = br.ReadInt32();
            if (dataTypeByteLength < 0) return null;
            if (dataTypeByteLength > byteArray.Count) return null;
            var dataTypeByte = br.ReadBytes(dataTypeByteLength);
            rByte -= dataTypeByteLength;

            var serializerMimeTypeByteLength = br.ReadInt32();
            if (serializerMimeTypeByteLength < 0) return null;
            if (serializerMimeTypeByteLength > rByte) return null;
            var serializerMimeTypeByte = br.ReadBytes(serializerMimeTypeByteLength);
            rByte -= serializerMimeTypeByteLength;

            var dataByteLength = br.ReadInt32();
            if (dataByteLength < 0) return null;
            if (dataByteLength > rByte) return null;
            var dataByte = br.ReadBytes(dataByteLength);

            return new SerializedObject
            {
                DataType = Encoding.UTF8.GetString(dataTypeByte),
                SerializerMimeType = Encoding.UTF8.GetString(serializerMimeTypeByte),
                Data = dataByte
            };
        }
        #endregion
    }
}
