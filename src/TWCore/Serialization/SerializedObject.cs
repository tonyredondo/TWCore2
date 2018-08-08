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
using System.Buffers;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using NonBlocking;
using TWCore.Compression;

namespace TWCore.Serialization
{
    [DataContract, Serializable]
    public sealed class SerializedObject : IEquatable<SerializedObject>, IStructuralEquatable
    {
        private static readonly ConcurrentDictionary<(string, string), ISerializer> SerializerWithCache = new ConcurrentDictionary<(string, string), ISerializer>();
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
                var cSerializer = SerializerWithCache.GetOrAdd((serializer.MimeTypes[0], serializer.Compressor?.EncodingType), vTuple => CreateSerializer(vTuple));
                Data = (byte[])cSerializer.Serialize(data, type);
            }
        }

        public SerializedObject(byte[] data, string dataType, string serializerMimeType)
        {
            Data = data;
            DataType = dataType;
            SerializerMimeType = serializerMimeType;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ISerializer CreateSerializer((string, string) vTuple)
        {
            var ser = SerializerManager.GetByMimeType(vTuple.Item1);
            if (ser == null)
                throw new FormatException($"The serializer with MimeType = {vTuple.Item1} wasn't found.");
            if (!string.IsNullOrWhiteSpace(vTuple.Item2))
                ser.Compressor = CompressorManager.GetByEncodingType(vTuple.Item2);
            ser.EnableCache = true;
            return ser;
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
            var type = string.IsNullOrWhiteSpace(DataType) ? typeof(object) : Core.GetType(DataType, true);
            if (string.IsNullOrWhiteSpace(SerializerMimeType))
                return type == typeof(byte[]) ? Data : null;
            var idx = SerializerMimeType.IndexOf(':');
            var serMime = idx < 0 ? SerializerMimeType : SerializerMimeType.Substring(0, idx);
            var serComp = idx < 0 ? null : SerializerMimeType.Substring(idx + 1);
            var serializer = SerializerWithCache.GetOrAdd((serMime, serComp), vTuple => CreateSerializer(vTuple));
            var value = serializer.Deserialize(Data, type);
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
                Span<byte> dataTypeBuffer = stackalloc byte[dataTypeLength];
                Encoding.UTF8.GetBytes(DataType, dataTypeBuffer);
                stream.Write(dataTypeBuffer);
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
        /// Write the SerializedObject to a file
        /// </summary>
        /// <param name="filepath">Filepath to save the serialized object instance</param>
        /// <returns>Awaitable task instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToFile(string filepath)
        {
            if (!filepath.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
                filepath += FileExtension;
            using (var fs = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Read))
                WriteTo(fs);
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
                Span<byte> buffer = stackalloc byte[dtLength];
                sequence.Slice(4, dtLength).CopyTo(buffer);
                dataType = Encoding.UTF8.GetString(buffer);
                length -= dtLength;
                sequence = sequence.Slice(4 + dtLength);
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
            byte[] dataArray = null;
            if (dataLength != -1)
            {
                dataArray = new byte[dataLength];
                var dSpan = dataArray.AsSpan();
                sequence.Slice(4, dataLength).CopyTo(dSpan);
            }

            return new SerializedObject(dataArray, dataType, mimeType);
        }
        /// <summary>
        /// Get SerializedObject instance from a stream
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject FromStream(Stream stream)
        {
            Span<byte> intBuffer = stackalloc byte[4];
            string dataType = null;
            string serializerMimeType = null;
            byte[] data = null;

            stream.Fill(intBuffer);
            var dataTypeByteLength = BitConverter.ToInt32(intBuffer);
            if (dataTypeByteLength > -1)
            {
                Span<byte> span = stackalloc byte[dataTypeByteLength];
                stream.Fill(span);
                dataType = Encoding.UTF8.GetString(span);
            }

            stream.Fill(intBuffer);
            var serializerMimeTypeByteLength = BitConverter.ToInt32(intBuffer);
            if (serializerMimeTypeByteLength > -1)
            {
                Span<byte> span = stackalloc byte[serializerMimeTypeByteLength];
                stream.Fill(span);
                serializerMimeType = Encoding.UTF8.GetString(span);
            }

            stream.Fill(intBuffer);
            var dataLength = BitConverter.ToInt32(intBuffer);
            if (dataLength > -1)
            {
                data = new byte[dataLength];
                stream.ReadExact(data, 0, dataLength);
            }

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
        /// <summary>
        /// Read the SerializedObject from a file
        /// </summary>
        /// <param name="filepath">Filepath to load the serialized object instance</param>
        /// <returns>Awaitable task instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject FromFile(string filepath)
        {
            if (!File.Exists(filepath) && File.Exists(filepath + FileExtension))
                filepath += FileExtension;
            using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                return FromMemory(fs.ReadAllBytesAsMemory());
        }
        #endregion
        
        #region Overrides

        public override int GetHashCode()
        {
            var hash = SerializerMimeType?.GetHashCode() ?? 0;
            hash += (DataType?.GetHashCode() ?? 0) ^ 31;
            hash += Data != null ? ByteArrayComparer.Instance.GetHashCode(Data) ^ 31 : 0;
            return hash;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as SerializedObject);
        }
        
        public bool Equals(SerializedObject other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (other.DataType != DataType) return false;
            if (other.SerializerMimeType != SerializerMimeType) return false;
            return ByteArrayComparer.Instance.Equals(Data, other.Data);
        }
        
        public static bool operator ==(SerializedObject a, SerializedObject b)
        {
            if (ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return false;
            if (!ReferenceEquals(a, null) && ReferenceEquals(b, null)) return false;
            if (ReferenceEquals(a, null)) return true;
            if (a.DataType != b.DataType) return false;
            if (a.SerializerMimeType != b.SerializerMimeType) return false;
            return ByteArrayComparer.Instance.Equals(a.Data, b.Data);
        }
        public static bool operator !=(SerializedObject a, SerializedObject b)
        {
            return !(a == b);
        }

        public bool Equals(object other, IEqualityComparer comparer)
        {
            if (ReferenceEquals(other, null)) return false;
            if (!(other is SerializedObject bSer)) return false;
            if (!comparer.Equals(DataType, bSer.DataType)) return false;
            if (!comparer.Equals(SerializerMimeType, bSer.SerializerMimeType)) return false;
            return ByteArrayComparer.Instance.Equals(Data, bSer.Data);
        }
        public int GetHashCode(IEqualityComparer comparer)
        {
            var hash = SerializerMimeType != null ? comparer.GetHashCode(SerializerMimeType) : 0;
            hash += (DataType != null ? comparer.GetHashCode(DataType) : 0) ^ 31;
            hash += Data != null ? ByteArrayComparer.Instance.GetHashCode(Data) ^ 31 : 0;
            return hash;
        }
        #endregion

    }
}