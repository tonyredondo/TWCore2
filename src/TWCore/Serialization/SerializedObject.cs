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
using TWCore.Collections;
using TWCore.Compression;
using TWCore.IO;

namespace TWCore.Serialization
{
    [DataContract, Serializable]
    public sealed class SerializedObject : IEquatable<SerializedObject>, IStructuralEquatable
    {
        private static readonly ConcurrentDictionary<(string, string), ISerializer> SerializerCache = new ConcurrentDictionary<(string, string), ISerializer>();
        private static readonly TimeoutDictionary<MultiArray<byte>, object> DesCache = new TimeoutDictionary<MultiArray<byte>, object>(MultiArrayBytesComparer.Instance);
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Serialized Object File Extension
        /// </summary>
        public const string FileExtension = ".sobj";

        #region Properties
        /// <summary>
        /// Item Data
        /// </summary>
        [DataMember]
        public MultiArray<byte> Data { get; set; }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedObject()
        {
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedObject(object data) : this(data, SerializerManager.DefaultBinarySerializer)
        {
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            else if (data is MultiArray<byte> mData)
            {
                SerializerMimeType = null;
                Data = mData;
            }
            else if (data is SerializedObject serObj)
            {
                Data = serObj.Data;
                DataType = serObj.DataType;
                SerializerMimeType = serObj.SerializerMimeType;
            }
            else
            {
                var serMimeType = serializer.MimeTypes[0];
                var serCompressor = serializer.Compressor?.EncodingType;
                SerializerMimeType = serMimeType;
                if (serCompressor != null)
                    SerializerMimeType += ":" + serCompressor;
                var cSerializer = SerializerCache.GetOrAdd((serMimeType, serCompressor), vTuple => CreateSerializer(vTuple));
                Data = cSerializer.Serialize(data, type);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedObject(byte[] data, string dataType, string serializerMimeType)
        {
            Data = data;
            DataType = dataType;
            SerializerMimeType = serializerMimeType;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedObject(MultiArray<byte> data, string dataType, string serializerMimeType)
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
            if (Data == MultiArray<byte>.Empty) return null;
            var type = string.IsNullOrWhiteSpace(DataType) ? typeof(object) : Core.GetType(DataType, true);
            if (string.IsNullOrWhiteSpace(SerializerMimeType))
            {
                if (type == typeof(byte[]))
                    return Data.AsArray();
                if (type == typeof(MultiArray<byte>))
                    return Data;
                return null;
            }
            var idx = SerializerMimeType.IndexOf(':');
            var serMime = idx < 0 ? SerializerMimeType : SerializerMimeType.Substring(0, idx);
            var serComp = idx < 0 ? null : SerializerMimeType.Substring(idx + 1);
            var serializer = SerializerCache.GetOrAdd((serMime, serComp), vTuple => CreateSerializer(vTuple));
            if (DesCache.TryGetValue(Data, out var cachedValue))
                return cachedValue.DeepClone();
            var value = serializer.Deserialize(Data, type);
            DesCache.TryAdd(Data, value, Timeout);
            return value;
        }
        /// <summary>
        /// Get Byte array representation of the SerializedObject instance
        /// </summary>
        /// <returns>Byte array instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray<byte> ToMultiArray()
        {
            using (var ms = new RecycleMemoryStream())
            {
                CopyTo(ms);
                return ms.GetMultiArray();
            }
        }
        /// <summary>
        /// Get SubArray representation of the SerializedObject instance
        /// </summary>
        /// <returns>SubArray instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Stream stream)
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
            BitConverter.TryWriteBytes(intBuffer, Data.Count);
            stream.Write(intBuffer);
            Data.CopyTo(stream);
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
                CopyTo(fs);
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
                CopyTo(fs);
        }
        /// <summary>
        /// Get SerializedObject instance from the MultiArray representation.
        /// </summary>
        /// <param name="multiArrayData">MultiArray instance</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject FromMultiArray(MultiArray<byte> multiArrayData)
        {
            if (multiArrayData.IsEmpty) return null;
            return FromStream(multiArrayData.AsReadOnlyStream());
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
            MultiArray<byte> data = MultiArray<byte>.Empty;

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
                const int segmentLength = 1024;
                var rows = dataLength / segmentLength;
                var pos = dataLength % segmentLength;
                if (pos > 0)
                    rows++;

                var bytes = new byte[rows][];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = new byte[segmentLength];
                    stream.ReadExact(bytes[i], 0, i == bytes.Length - 1 ? pos : segmentLength);
                }

                data = new MultiArray<byte>(bytes, 0, dataLength);
            }

            return new SerializedObject(data, dataType, serializerMimeType);
        }
        /// <summary>
        /// Get SerializedObject instance from a stream
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<SerializedObject> FromStreamAsync(Stream stream)
        {
            var intBuffer = new byte[4];
            string dataType = null;
            string serializerMimeType = null;
            MultiArray<byte> data = MultiArray<byte>.Empty;

            stream.Fill(intBuffer);
            var dataTypeByteLength = BitConverter.ToInt32(intBuffer);
            if (dataTypeByteLength > -1)
            {
                var dBuffer = new byte[dataTypeByteLength];
                await stream.FillAsync(dBuffer).ConfigureAwait(false);
                dataType = Encoding.UTF8.GetString(dBuffer);
            }

            stream.Fill(intBuffer);
            var serializerMimeTypeByteLength = BitConverter.ToInt32(intBuffer);
            if (serializerMimeTypeByteLength > -1)
            {
                var dBuffer = new byte[serializerMimeTypeByteLength];
                await stream.FillAsync(dBuffer).ConfigureAwait(false);
                serializerMimeType = Encoding.UTF8.GetString(dBuffer);
            }

            stream.Fill(intBuffer);
            var dataLength = BitConverter.ToInt32(intBuffer);
            if (dataLength > -1)
            {
                const int segmentLength = 1024;
                var rows = dataLength / segmentLength;
                var pos = dataLength % segmentLength;
                if (pos > 0)
                    rows++;

                var bytes = new byte[rows][];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = new byte[segmentLength];
                    await stream.ReadExactAsync(bytes[i], 0, i == bytes.Length - 1 ? pos : segmentLength).ConfigureAwait(false);
                }
                data = new MultiArray<byte>(bytes, 0, dataLength);
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
                return await FromStreamAsync(fs).ConfigureAwait(false);
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
                return FromStream(fs);
        }
        #endregion
        
        #region Overrides

        public override int GetHashCode()
        {
            var hash = SerializerMimeType?.GetHashCode() ?? 0;
            hash += (DataType?.GetHashCode() ?? 0) ^ 31;
            hash += Data.GetHashCode() ^ 31;
            return hash;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as SerializedObject);
        }
        
        public bool Equals(SerializedObject other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.DataType != DataType) return false;
            if (other.SerializerMimeType != SerializerMimeType) return false;
            return MultiArrayBytesComparer.Instance.Equals(Data, other.Data);
        }
        
        public static bool operator ==(SerializedObject a, SerializedObject b)
        {
            if (ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return false;
            if (!ReferenceEquals(a, null) && ReferenceEquals(b, null)) return false;
            if (ReferenceEquals(a, null)) return true;
            if (ReferenceEquals(a, b)) return true;
            if (a.DataType != b.DataType) return false;
            if (a.SerializerMimeType != b.SerializerMimeType) return false;
            return MultiArrayBytesComparer.Instance.Equals(a.Data, b.Data);
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
            return MultiArrayBytesComparer.Instance.Equals(Data, bSer.Data);
        }
        public int GetHashCode(IEqualityComparer comparer)
        {
            var hash = SerializerMimeType != null ? comparer.GetHashCode(SerializerMimeType) : 0;
            hash += (DataType != null ? comparer.GetHashCode(DataType) : 0) ^ 31;
            hash += MultiArrayBytesComparer.Instance.GetHashCode(Data) ^ 31;
            return hash;
        }
        #endregion

    }
}