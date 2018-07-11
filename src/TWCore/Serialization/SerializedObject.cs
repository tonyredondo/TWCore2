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
                Data = (byte[])serializer.Serialize(data, type);
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
        /// Get Byte array representation of the SerializedObject instance
        /// </summary>
        /// <returns>Byte array instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ToArray()
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

            return bytes;
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
            var hasDataType = !string.IsNullOrEmpty(DataType);
            var hasMimeType = !string.IsNullOrEmpty(SerializerMimeType);

            var dataTypeLength = hasDataType ? Encoding.UTF8.GetByteCount(DataType) : 0;
            var serializerMimeTypeLength = hasMimeType ? Encoding.UTF8.GetByteCount(SerializerMimeType) : 0;

            Span<byte> intBuffer = stackalloc byte[4];

            //DataType
            BitConverter.TryWriteBytes(intBuffer, hasDataType ? dataTypeLength : -1);
            stream.Write(intBuffer);
            if (hasDataType)
            {
                using (var memBuffer = MemoryPool<byte>.Shared.Rent(minBufferSize: dataTypeLength))
                {
                    var dataTypeBuffer = memBuffer.Memory.Span.Slice(0, dataTypeLength);
                    Encoding.UTF8.GetBytes(DataType, dataTypeBuffer);
                    stream.Write(dataTypeBuffer);
                }
            }

            //MimeType
            BitConverter.TryWriteBytes(intBuffer, hasMimeType ? serializerMimeTypeLength : -1);
            stream.Write(intBuffer);
            if (hasMimeType)
            {
                Span<byte> mimeTypeBuffer = stackalloc byte[serializerMimeTypeLength];
                Encoding.UTF8.GetBytes(SerializerMimeType, mimeTypeBuffer);
                stream.Write(mimeTypeBuffer);
            }

            //Data
            BitConverter.TryWriteBytes(intBuffer, Data?.Length ?? -1);
            stream.Write(intBuffer);
            if (Data != null)
            {
                stream.Write(Data, 0, Data.Length);
            }
        }
        /// <summary>
        /// Write the SerializedObject to a file
        /// </summary>
        /// <param name="filepath">Filepath to save the serialized object instance</param>
        /// <returns>Awaitable task instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task ToFileAsync(string filepath)
        {
            if (!filepath.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
                filepath += FileExtension;
            using (var fs = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                WriteTo(fs);
                await fs.FlushAsync().ConfigureAwait(false);
            }
        }


        /// <summary>
        /// Get SerializedObject instance from the SubArray representation.
        /// </summary>
        /// <param name="byteArray">SubArray instance</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject FromArray(byte[] byteArray)
        {
            if (byteArray.Length == 0) return null;
            return FromSpan(byteArray.AsSpan());
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
        /// <summary>
        /// Get SerializedObject instance from the ReadonlySequence representation.
        /// </summary>
        /// <param name="sequence">Readonly sequence</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject FromReadOnlySequence(ReadOnlySequence<byte> sequence)
        {
            if (sequence.IsEmpty) return null;
            var length = sequence.Length;
            Span<byte> lengthSpan = stackalloc byte[4];
            string dataType = null;
            string mimeType = null;

            sequence.Slice(0, 4).CopyTo(lengthSpan);
            var dtLength = BitConverter.ToInt32(lengthSpan);
            if (dtLength < -1 || dtLength > length) return null;
            if (dtLength != -1)
            {
                using (var memBuffer = MemoryPool<byte>.Shared.Rent(minBufferSize: dtLength))
                {
                    Span<byte> buffer = memBuffer.Memory.Span.Slice(0, dtLength);
                    sequence.Slice(4, dtLength).CopyTo(buffer);
                    dataType = Encoding.UTF8.GetString(buffer);
                    length -= dtLength;
                    sequence = sequence.Slice(4 + dtLength);
                }
            }

            sequence.Slice(0, 4).CopyTo(lengthSpan);
            var smtLength = BitConverter.ToInt32(lengthSpan);
            if (smtLength < -1 || smtLength > length) return null;
            if (smtLength != -1)
            {
                Span<byte> buffer = stackalloc byte[smtLength];
                sequence.Slice(4, smtLength).CopyTo(buffer);
                mimeType = Encoding.UTF8.GetString(buffer);
                length -= smtLength;
                sequence = sequence.Slice(4 + smtLength);
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
        /// <summary>
        /// Get SerializedObject instance from a stream
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject FromStream(Stream stream)
        {
            var intBuffer = ArrayPool<byte>.Shared.Rent(4);
            string dataType = null;
            string serializerMimeType = null;
            byte[] data = null;

            stream.Read(intBuffer, 0, 4);
            var dataTypeByteLength = BitConverter.ToInt32(intBuffer);
            if (dataTypeByteLength > -1)
            {
                var buffer = ArrayPool<byte>.Shared.Rent(dataTypeByteLength);
                try
                {
                    stream.ReadExact(buffer, 0, dataTypeByteLength);
                    dataType = Encoding.UTF8.GetString(buffer, 0, dataTypeByteLength);
                }
                catch
                {
                    ArrayPool<byte>.Shared.Return(intBuffer);
                    throw;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            stream.Read(intBuffer, 0, 4);
            var serializerMimeTypeByteLength = BitConverter.ToInt32(intBuffer);
            if (serializerMimeTypeByteLength > -1)
            {
                var buffer = ArrayPool<byte>.Shared.Rent(serializerMimeTypeByteLength);
                try
                {
                    stream.ReadExact(buffer, 0, serializerMimeTypeByteLength);
                    serializerMimeType = Encoding.UTF8.GetString(buffer, 0, serializerMimeTypeByteLength);
                }
                catch
                {
                    ArrayPool<byte>.Shared.Return(intBuffer);
                    throw;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            stream.Read(intBuffer, 0, 4);
            var dataLength = BitConverter.ToInt32(intBuffer);
            if (dataLength > -1)
            {
                data = new byte[dataLength];
                stream.ReadExact(data, 0, dataLength);
            }

            ArrayPool<byte>.Shared.Return(intBuffer);


            return new SerializedObject(data, dataType, serializerMimeType);
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