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
using System.Threading.Tasks;
using System.Xml.Serialization;
using TWCore.Collections;
using TWCore.Compression;

namespace TWCore.Serialization
{
    [DataContract, Serializable]
    public sealed class SerializedObject
    {
        private static WeakDictionary<byte[], object> SerializedObjects = new WeakDictionary<byte[], object>(new ArrayEqualityComparer<byte>());

        /// <summary>
        /// Serialized Object File Extension
        /// </summary>
        public const string FileExtension = ".sobj";
        
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

        public SerializedObject()
        {
        }
        
        public SerializedObject(object data) : this(data, SerializerManager.DefaultBinarySerializer)
        {
        }

        public SerializedObject(object data, ISerializer serializer)
        {
            if (data == null) return;
            var type = data.GetType();
            DataType = type.GetTypeName();
            if (data is byte[] bytes)
            {
                SerializerMimeType = null;
                Data = bytes;
            }
            else
            {
                SerializerMimeType = serializer.MimeTypes[0];
                if (serializer.Compressor != null)
                    SerializerMimeType += ":" + serializer.Compressor.EncodingType;
                Data = (byte[]) serializer.Serialize(data, type);
            }
            SerializedObjects.TryAdd(Data, data);
        }

        public SerializedObject(byte[] data, string dataType, string serializerMimeType)
        {
            Data = data;
            DataType = dataType;
            SerializerMimeType = serializerMimeType;
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
            if (Data == null) return null;
            if (SerializedObjects.TryGetValue(Data, out var value))
                return value;
            var type = string.IsNullOrWhiteSpace(DataType) ? typeof(object) : Core.GetType(DataType, true);
            if (string.IsNullOrWhiteSpace(SerializerMimeType)) 
                return type == typeof(byte[]) ? Data : null;
            var idx = SerializerMimeType.IndexOf(':');
            var serMime = idx < 0 ? SerializerMimeType : SerializerMimeType.Substring(0, idx);
            var serComp = idx < 0 ? null : SerializerMimeType.Substring(idx + 1);
            var serializer = SerializerManager.GetByMimeType(serMime);
            if (!string.IsNullOrWhiteSpace(serComp))
                serializer.Compressor = CompressorManager.GetByEncodingType(serComp);
            value = serializer.Deserialize(Data, type);
            SerializedObjects.TryAdd(Data, value);
            return value;
        }
        /// <summary>
        /// Get SubArray representation of the SerializedObject instance
        /// </summary>
        /// <returns>SubArray instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SubArray<byte> ToSubArray()
        {
            var dataTypeByte = !string.IsNullOrEmpty(DataType) ? Encoding.UTF8.GetBytes(DataType) : null;
            var serializerMimeTypeByte = !string.IsNullOrEmpty(SerializerMimeType) ? Encoding.UTF8.GetBytes(SerializerMimeType) : null;
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(dataTypeByte?.Length ?? -1);
                if (dataTypeByte != null) bw.Write(dataTypeByte);
                bw.Write(serializerMimeTypeByte?.Length ?? -1);
                if (serializerMimeTypeByte != null) bw.Write(serializerMimeTypeByte);
                bw.Write(Data?.Length ?? -1);
                if (Data != null) bw.Write(Data);
                return ms.ToSubArray();
            }
        }
        /// <summary>
        /// Get SubArray representation of the SerializedObject instance
        /// </summary>
        /// <returns>SubArray instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteTo(Stream stream)
        {
            var dataTypeByte = !string.IsNullOrEmpty(DataType) ? Encoding.UTF8.GetBytes(DataType) : null;
            var serializerMimeTypeByte = !string.IsNullOrEmpty(SerializerMimeType) ? Encoding.UTF8.GetBytes(SerializerMimeType) : null;
            using (var bw = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                bw.Write(dataTypeByte?.Length ?? -1);
                if (dataTypeByte != null) bw.Write(dataTypeByte);
                bw.Write(serializerMimeTypeByte?.Length ?? -1);
                if (serializerMimeTypeByte != null) bw.Write(serializerMimeTypeByte);
                bw.Write(Data?.Length ?? -1);
                if (Data != null) bw.Write(Data);
            }
        }
        /// <summary>
        /// Writes the SerializedObject to a binary writer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteTo(BinaryWriter bw)
        {
            var dataTypeByte = !string.IsNullOrEmpty(DataType) ? Encoding.UTF8.GetBytes(DataType) : null;
            var serializerMimeTypeByte = !string.IsNullOrEmpty(SerializerMimeType) ? Encoding.UTF8.GetBytes(SerializerMimeType) : null;
            bw.Write(dataTypeByte?.Length ?? -1);
            if (dataTypeByte != null) bw.Write(dataTypeByte);
            bw.Write(serializerMimeTypeByte?.Length ?? -1);
            if (serializerMimeTypeByte != null) bw.Write(serializerMimeTypeByte);
            bw.Write(Data?.Length ?? -1);
            if (Data != null) bw.Write(Data);
        }
        /// <summary>
        /// Write the SerializedObject to a file
        /// </summary>
        /// <param name="filepath">Filepath to save the serialized object instance</param>
        /// <returns>Awaitable task instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task ToFileAsync(string filepath)
        {
            var dataTypeByte = !string.IsNullOrEmpty(DataType) ? Encoding.UTF8.GetBytes(DataType) : null;
            var serializerMimeTypeByte = !string.IsNullOrEmpty(SerializerMimeType) ? Encoding.UTF8.GetBytes(SerializerMimeType) : null;
            if (!filepath.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
                filepath += FileExtension;
            using (var fs = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await fs.WriteBytesAsync(BitConverter.GetBytes(dataTypeByte?.Length ?? -1), false).ConfigureAwait(false);
                if (dataTypeByte != null) 
                    await fs.WriteBytesAsync(dataTypeByte, false).ConfigureAwait(false);
                await fs.WriteBytesAsync(BitConverter.GetBytes(serializerMimeTypeByte?.Length ?? -1), false).ConfigureAwait(false);
                if (serializerMimeTypeByte != null) 
                    await fs.WriteBytesAsync(serializerMimeTypeByte, false).ConfigureAwait(false);
                await fs.WriteBytesAsync(BitConverter.GetBytes(Data?.Length ?? -1), false).ConfigureAwait(false);
                if (Data != null) 
                    await fs.WriteBytesAsync(Data).ConfigureAwait(false);
            }
        }
        
        /// <summary>
        /// Get SerializedObject instance from the SubArray representation.
        /// </summary>
        /// <param name="byteArray">SubArray instance</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject FromSubArray(SubArray<byte> byteArray)
        {
            using(var ms = byteArray.ToMemoryStream())
            using (var br = new BinaryReader(ms))
            {
                var rByte = byteArray.Count;

                var dataTypeByteLength = br.ReadInt32();
                if (dataTypeByteLength < -1 || dataTypeByteLength > rByte) return null;
                var dataTypeByte = dataTypeByteLength != -1 ? br.ReadBytes(dataTypeByteLength) : null;
                rByte -= dataTypeByteLength;

                var serializerMimeTypeByteLength = br.ReadInt32();
                if (serializerMimeTypeByteLength < -1 || serializerMimeTypeByteLength > rByte) return null;
                var serializerMimeTypeByte = serializerMimeTypeByteLength != -1 ? br.ReadBytes(serializerMimeTypeByteLength) : null;
                rByte -= serializerMimeTypeByteLength;

                var dataLength = br.ReadInt32();
                if (dataLength < -1 || dataLength > rByte) return null;
                var data = dataLength != -1 ? br.ReadBytes(dataLength) : null;

                return new SerializedObject(data,
                    dataTypeByte != null ? Encoding.UTF8.GetString(dataTypeByte) : null,
                    serializerMimeTypeByte != null ? Encoding.UTF8.GetString(serializerMimeTypeByte) : null);
            }
        }
        /// <summary>
        /// Get SerializedObject instance from a stream
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject FromStream(Stream stream)
        {
            using (var br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var dataTypeByteLength = br.ReadInt32();
                if (dataTypeByteLength < -1) return null;
                var dataTypeByte = dataTypeByteLength != -1 ? br.ReadBytes(dataTypeByteLength) : null;

                var serializerMimeTypeByteLength = br.ReadInt32();
                if (serializerMimeTypeByteLength < -1) return null;
                var serializerMimeTypeByte = serializerMimeTypeByteLength != -1 ? br.ReadBytes(serializerMimeTypeByteLength) : null;

                var dataLength = br.ReadInt32();
                if (dataLength < -1) return null;
                var data = dataLength != -1 ? br.ReadBytes(dataLength) : null;

                return new SerializedObject(data,
                    dataTypeByte != null ? Encoding.UTF8.GetString(dataTypeByte) : null,
                    serializerMimeTypeByte != null ? Encoding.UTF8.GetString(serializerMimeTypeByte) : null);
            }
        }
        /// <summary>
        /// Get SerializedObject instance from a BinaryReader
        /// </summary>
        /// <param name="br">BinaryReader instance</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject FromStream(BinaryReader br)
        {
            var dataTypeByteLength = br.ReadInt32();
            if (dataTypeByteLength < -1) return null;
            var dataTypeByte = dataTypeByteLength != -1 ? br.ReadBytes(dataTypeByteLength) : null;

            var serializerMimeTypeByteLength = br.ReadInt32();
            if (serializerMimeTypeByteLength < -1) return null;
            var serializerMimeTypeByte = serializerMimeTypeByteLength != -1 ? br.ReadBytes(serializerMimeTypeByteLength) : null;

            var dataLength = br.ReadInt32();
            if (dataLength < -1) return null;
            var data = dataLength != -1 ? br.ReadBytes(dataLength) : null;

            return new SerializedObject(data,
                dataTypeByte != null ? Encoding.UTF8.GetString(dataTypeByte) : null,
                serializerMimeTypeByte != null ? Encoding.UTF8.GetString(serializerMimeTypeByte) : null);
        }
        /// <summary>
        /// Read the SerializedObject from a file
        /// </summary>
        /// <param name="filepath">Filepath to load the serialized object instance</param>
        /// <returns>Awaitable task instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<SerializedObject> FromFileAsync(string filepath)
        {
            if (!File.Exists(filepath) && File.Exists(filepath + FileExtension))
                filepath += FileExtension;
            using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var length = fs.Length;
                var bInt = new byte[4];
                
                await fs.ReadAsync(bInt, 0, 4).ConfigureAwait(false);
                var dtLength = BitConverter.ToInt32(bInt, 0);
                if (dtLength < -1 || dtLength > length) return null;
                byte[] dataTypeByte = null;
                if (dtLength != -1)
                {
                    dataTypeByte = new byte[dtLength];
                    await fs.ReadAsync(dataTypeByte, 0, dtLength).ConfigureAwait(false);
                    length -= dtLength;
                }

                await fs.ReadAsync(bInt, 0, 4).ConfigureAwait(false);
                var smtLength = BitConverter.ToInt32(bInt, 0);
                if (smtLength < -1 || smtLength > length) return null;
                byte[] serializerMimeTypeByte = null;
                if (smtLength != -1)
                {
                    serializerMimeTypeByte = new byte[smtLength];
                    await fs.ReadAsync(serializerMimeTypeByte, 0, smtLength).ConfigureAwait(false);
                    length -= smtLength;
                }
                
                await fs.ReadAsync(bInt, 0, 4).ConfigureAwait(false);
                var dataLength = BitConverter.ToInt32(bInt, 0);
                if (dataLength < -1 || dataLength > length) return null;
                byte[] data = null;
                if (dataLength != -1)
                {
                    data = new byte[dataLength];
                    await fs.ReadAsync(data, 0, dataLength).ConfigureAwait(false);
                }
                
                return new SerializedObject(data,
                    dataTypeByte != null ? Encoding.UTF8.GetString(dataTypeByte) : null,
                    serializerMimeTypeByte != null ? Encoding.UTF8.GetString(serializerMimeTypeByte) : null);
            }
        }
        #endregion
    }
}