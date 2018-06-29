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
using System.Buffers;
using System.Buffers.Text;
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
            if (serializer == null)
                throw new FormatException($"The serializer with MimeType = {serMime} wasn't found.");
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
            var hasDataType = !string.IsNullOrEmpty(DataType);
            var hasMimeType = !string.IsNullOrEmpty(SerializerMimeType);

            var dataTypeLength = hasDataType ? Encoding.UTF8.GetByteCount(DataType) : 0;
            var serializerMimeTypeLength = hasMimeType ? Encoding.UTF8.GetByteCount(SerializerMimeType) : 0;
            var totalBytes = 4 + dataTypeLength + 4 + serializerMimeTypeLength + 4 + (Data?.Length ?? 0);
            var bytes = new byte[totalBytes];
            var span = new Span<byte>(bytes);

            BitConverter.TryWriteBytes(span, hasDataType ? dataTypeLength : -1);
            span = span.Slice(4);
            if (hasDataType)
            {
                Encoding.UTF8.GetBytes(DataType, span);
                span = span.Slice(dataTypeLength);
            }

            BitConverter.TryWriteBytes(span, hasMimeType ? serializerMimeTypeLength : -1);
            span = span.Slice(4);
            if (hasMimeType)
            {
                Encoding.UTF8.GetBytes(SerializerMimeType, span);
                span = span.Slice(serializerMimeTypeLength);
            }

            BitConverter.TryWriteBytes(span, Data?.Length ?? -1);
            span = span.Slice(4);
            if (Data != null)
            {
                Data.CopyTo(span.Slice(0, Data.Length));
            }

            return new SubArray<byte>(bytes);
        }
        /// <summary>
        /// Get SubArray representation of the SerializedObject instance
        /// </summary>
        /// <returns>SubArray instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<byte> span)
        {
            var hasDataType = !string.IsNullOrEmpty(DataType);
            var hasMimeType = !string.IsNullOrEmpty(SerializerMimeType);

            var dataTypeLength = hasDataType ? Encoding.UTF8.GetByteCount(DataType) : 0;
            var serializerMimeTypeLength = hasMimeType ? Encoding.UTF8.GetByteCount(SerializerMimeType) : 0;
            var totalBytes = 4 + dataTypeLength + 4 + serializerMimeTypeLength + 4 + (Data?.Length ?? 0);
            if (span.Length < totalBytes)
                throw new ArgumentOutOfRangeException(nameof(span), "The span length is lower than the required length");

            BitConverter.TryWriteBytes(span, hasDataType ? dataTypeLength : -1);
            span = span.Slice(4);
            if (hasDataType)
            {
                Encoding.UTF8.GetBytes(DataType, span);
                span = span.Slice(dataTypeLength);
            }

            BitConverter.TryWriteBytes(span, hasMimeType ? serializerMimeTypeLength : -1);
            span = span.Slice(4);
            if (hasMimeType)
            {
                Encoding.UTF8.GetBytes(SerializerMimeType, span);
                span = span.Slice(serializerMimeTypeLength);
            }

            BitConverter.TryWriteBytes(span, Data?.Length ?? -1);
            span = span.Slice(4);
            if (Data != null)
            {
                Data.CopyTo(span.Slice(0, Data.Length));
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
            if (byteArray.Count == 0) return null;
            return FromSpan(byteArray.AsReadOnlySpan());
        }
        /// <summary>
        /// Get SerializedObject instance from the Memory representation.
        /// </summary>
        /// <param name="byteArray">Readonly Memory instance</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject FromMemory(ReadOnlyMemory<byte> memoryData)
        {
            if (memoryData.IsEmpty) return null;
            return FromSpan(memoryData.Span);
        }
        /// <summary>
        /// Get SerializedObject instance from the Span representation.
        /// </summary>
        /// <param name="byteArray">Readonly Span instance</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject FromSpan(ReadOnlySpan<byte> spanData)
        {
            if (spanData.IsEmpty) return null;
            var length = spanData.Length;

            var dtLength = BitConverter.ToInt32(spanData.Slice(0, 4));
            if (dtLength < -1 || dtLength > length) return null;
            var dataTypeByte = ReadOnlySpan<byte>.Empty;
            if (dtLength != -1)
            {
                dataTypeByte = spanData.Slice(4, dtLength);
                length -= dtLength;
                spanData = spanData.Slice(4 + dtLength);
            }

            var smtLength = BitConverter.ToInt32(spanData.Slice(0, 4));
            if (smtLength < -1 || smtLength > length) return null;
            var serializerMimeTypeByte = ReadOnlySpan<byte>.Empty;
            if (smtLength != -1)
            {
                serializerMimeTypeByte = spanData.Slice(4, smtLength);
                length -= smtLength;
                spanData = spanData.Slice(4 + smtLength);
            }

            var dataLength = BitConverter.ToInt32(spanData.Slice(0, 4));
            if (dataLength < -1 || dataLength > length) return null;
            var data = ReadOnlySpan<byte>.Empty;
            if (dataLength != -1)
            {
                data = spanData.Slice(4, dataLength);
            }

            return new SerializedObject(data.ToArray(),
                !dataTypeByte.IsEmpty ? Encoding.UTF8.GetString(dataTypeByte) : null,
                !serializerMimeTypeByte.IsEmpty ? Encoding.UTF8.GetString(serializerMimeTypeByte) : null);
        }
        private static MemoryPool<byte> pool = MemoryPool<byte>.Shared;
        /// <summary>
        /// Get SerializedObject instance from the ReadonlySequence representation.
        /// </summary>
        /// <param name="byteArray">Readonly Span instance</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject FromReadOnlySequence(ReadOnlySequence<byte> sequence)
        {
            if (sequence.IsEmpty) return null;
            var length = sequence.Length;
            using (var intBuffer = pool.Rent(minBufferSize: 4))
            {
                var lengthSpan = intBuffer.Memory.Span;
                string dataType = null;
                string mimeType = null;

                sequence.Slice(0, 4).CopyTo(lengthSpan);
                var dtLength = BitConverter.ToInt32(lengthSpan);
                if (dtLength < -1 || dtLength > length) return null;
                if (dtLength != -1)
                {
                    using (var buffer = pool.Rent(minBufferSize: dtLength))
                    {
                        sequence.Slice(4, dtLength).CopyTo(buffer.Memory.Span);
                        dataType = Encoding.UTF8.GetString(buffer.Memory.Span.Slice(0, dtLength));
                        length -= dtLength;
                        sequence = sequence.Slice(4 + dtLength);
                    }
                }

                sequence.Slice(0, 4).CopyTo(lengthSpan);
                var smtLength = BitConverter.ToInt32(lengthSpan);
                if (smtLength < -1 || smtLength > length) return null;
                if (smtLength != -1)
                {
                    using (var buffer = pool.Rent(minBufferSize: smtLength))
                    {
                        sequence.Slice(4, smtLength).CopyTo(buffer.Memory.Span);
                        mimeType = Encoding.UTF8.GetString(buffer.Memory.Span.Slice(0, smtLength));
                        length -= smtLength;
                        sequence = sequence.Slice(4 + smtLength);
                    }
                }

                sequence.Slice(0, 4).CopyTo(lengthSpan);
                var dataLength = BitConverter.ToInt32(lengthSpan);
                if (dataLength < -1 || dataLength > length) return null;
                var data = ReadOnlySpan<byte>.Empty;
                if (dataLength != -1)
                {
                    var dSpan = new Span<byte>(new byte[dataLength]);
                    sequence.Slice(4, dataLength).CopyTo(dSpan);
                    data = dSpan;
                }

                return new SerializedObject(data.ToArray(), dataType, mimeType);

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
                return FromMemory(await fs.ReadAllBytesAsMemoryAsync().ConfigureAwait(false));
        }
        #endregion
    }
}