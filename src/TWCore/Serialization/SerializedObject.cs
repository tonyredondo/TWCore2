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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TWCore.Compression;
using TWCore.IO;

namespace TWCore.Serialization
{
    /// <summary>
    /// Serialized Object
    /// </summary>
    [DataContract, Serializable]
    public sealed class SerializedObject : IEquatable<SerializedObject>, IStructuralEquatable, IDisposable
    {
        private static readonly ConcurrentDictionary<string, ISerializer> SerializerCache = new ConcurrentDictionary<string, ISerializer>();
        private static readonly InstanceLockerAsync<string> FileLockerAsync = new InstanceLockerAsync<string>(2048);
        private bool _canCollect;
        private MultiArray<byte> _data;
        private string _dataType;
        private string _serializerMimeType;

        /// <summary>
        /// Serialized Object File Extension
        /// </summary>
        public const string FileExtension = ".sobj";

        #region Properties
        /// <summary>
        /// Bytes count
        /// </summary>
        [XmlAttribute, DataMember]
        public int Count => _data.Count;
        /// <summary>
        /// Item Data Type
        /// </summary>
        [XmlAttribute, DataMember]
        public string DataType => _dataType;
        /// <summary>
        /// Serializer Mime Type
        /// </summary>
        [XmlAttribute, DataMember]
        public string SerializerMimeType => _serializerMimeType;
        #endregion

        #region Throw Helper
        private static void ThrowSerializerNotFound(string serMime) => throw new FormatException($"The serializer with MimeType = {serMime} wasn't found.");
        private static void ThrowDisposedValue() => throw new ObjectDisposedException(nameof(SerializedObject));
        #endregion

        #region .ctor
        /// <summary>
        /// Serialized Object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedObject()
        {
        }
        /// <summary>
        /// Serialized Object
        /// </summary>
        /// <param name="data">Object instance to serialize</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedObject(object data) : this(data, SerializerManager.DefaultBinarySerializer)
        {
        }
        /// <summary>
        /// Serialized Object
        /// </summary>
        /// <param name="data">Object instance to serialize</param>
        /// <param name="serializer">Serializer instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedObject(object data, ISerializer serializer)
        {
            if (data is null) return;
            var type = data.GetType();
            _dataType = string.Intern(type.GetTypeName());
            if (data is byte[] bytes)
            {
                _serializerMimeType = null;
                _data = bytes;
                _canCollect = false;
            }
            else if (data is MultiArray<byte> mData)
            {
                _serializerMimeType = null;
                _data = mData;
                _canCollect = false;
            }
            else if (data is SerializedObject serObj)
            {
                _data = serObj._data;
                _dataType = serObj._dataType;
                _serializerMimeType = serObj._serializerMimeType;
                _canCollect = false;
            }
            else
            {
                var serMimeType = serializer.MimeTypes[0];
                var serCompressor = serializer.Compressor?.EncodingType;
                if (serCompressor != null)
                    _serializerMimeType = string.Intern(serMimeType + ":" + serCompressor);
                else
                    _serializerMimeType = string.Intern(serMimeType);
                var cSerializer = SerializerCache.GetOrAdd(_serializerMimeType, smt => CreateSerializer(smt));
                _data = cSerializer.Serialize(data, type);
                _canCollect = true;
            }
        }
        /// <summary>
        /// Serialized Object
        /// </summary>
        /// <param name="data">Byte array with the serializer data</param>
        /// <param name="dataType">Data type name</param>
        /// <param name="serializerMimeType">Serializer mime type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SerializedObject(byte[] data, string dataType, string serializerMimeType)
        {
            _data = data;
            _dataType = string.Intern(dataType);
            _serializerMimeType = string.Intern(serializerMimeType);
            _canCollect = true;
        }
        /// <summary>
        /// Serialized Object
        /// </summary>
        /// <param name="data">MultiArray byte with the serializer data</param>
        /// <param name="dataType">Data type name</param>
        /// <param name="serializerMimeType">Serializer mime type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SerializedObject(MultiArray<byte> data, string dataType, string serializerMimeType)
        {
            _data = data;
            _dataType = string.Intern(dataType);
            _serializerMimeType = string.Intern(serializerMimeType);
            _canCollect = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ISerializer CreateSerializer(string serMimeType)
        {
            var idx = serMimeType.IndexOf(':');
            var serMime = idx < 0 ? serMimeType : serMimeType.Substring(0, idx);
            var serComp = idx < 0 ? null : serMimeType.Substring(idx + 1);
            var ser = SerializerManager.GetByMimeType(serMime);
            if (ser is null)
                ThrowSerializerNotFound(serMime);
            if (!string.IsNullOrWhiteSpace(serComp))
                ser.Compressor = CompressorManager.GetByEncodingType(serComp);
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
            if (disposedValue) ThrowDisposedValue();
            if (_data == MultiArray<byte>.Empty) return null;
            var type = string.IsNullOrWhiteSpace(_dataType) ? typeof(object) : Core.GetType(_dataType, false);
            if (string.IsNullOrWhiteSpace(_serializerMimeType))
            {
                if (type == typeof(byte[]))
                    return _data.AsArray();
                if (type == typeof(MultiArray<byte>))
                    return _data;
                return null;
            }
            var serializer = SerializerCache.GetOrAdd(_serializerMimeType, smt => CreateSerializer(smt));
            var value = serializer.Deserialize(_data, type ?? typeof(object));
            return value;
        }
        /// <summary>
        /// Get Deserialized Value or Generic Deserialized value
        /// </summary>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetValueOrGenericValue()
        {
            if (disposedValue) ThrowDisposedValue();
            if (_data == MultiArray<byte>.Empty) return null;
            var type = string.IsNullOrWhiteSpace(_dataType) ? typeof(object) : Core.GetType(_dataType, false);
            if (string.IsNullOrWhiteSpace(_serializerMimeType))
            {
                if (type == typeof(byte[]))
                    return _data.AsArray();
                if (type == typeof(MultiArray<byte>))
                    return _data;
                return null;
            }
            var serializer = SerializerCache.GetOrAdd(_serializerMimeType, smt => CreateSerializer(smt));
            try
            {
                return serializer.Deserialize(_data, type ?? typeof(object));
            }
            catch (DeserializerException desException)
            {
                return desException.Value;
            }
        }
        /// <summary>
        /// Get Byte array representation of the SerializedObject instance
        /// </summary>
        /// <returns>Byte array instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray<byte> ToMultiArray()
        {
            if (disposedValue) ThrowDisposedValue();
            var ms = ReferencePool<RecycleMemoryStream>.Shared.New();
            CopyTo(ms);
            var value = ms.GetMultiArray();
            ms.Reset();
            ReferencePool<RecycleMemoryStream>.Shared.Store(ms);
            return value;
        }
        /// <summary>
        /// Get SubArray representation of the SerializedObject instance
        /// </summary>
        /// <returns>SubArray instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Stream stream)
        {
            if (disposedValue) ThrowDisposedValue();
            var hasDataType = !string.IsNullOrEmpty(_dataType);
            var hasMimeType = !string.IsNullOrEmpty(_serializerMimeType);

            var dataTypeLength = hasDataType ? Encoding.UTF8.GetByteCount(_dataType) : 0;
            var serializerMimeTypeLength = hasMimeType ? Encoding.UTF8.GetByteCount(_serializerMimeType) : 0;

#if COMPATIBILITY
            byte[] buffer;

            //DataType
            buffer = BitConverter.GetBytes(hasDataType ? dataTypeLength : -1);
            stream.WriteBytes(buffer);
            if (hasDataType)
            {
                buffer = Encoding.UTF8.GetBytes(_dataType);
                stream.Write(buffer);
            }

            //MimeType
            buffer = BitConverter.GetBytes(hasMimeType ? serializerMimeTypeLength : -1);
            stream.WriteBytes(buffer);
            if (hasMimeType)
            {
                buffer = Encoding.UTF8.GetBytes(_serializerMimeType);
                stream.Write(buffer);
            }

            //Data
            buffer = BitConverter.GetBytes(_data.Count);
            stream.WriteBytes(buffer);
            _data.CopyTo(stream);
#else
            Span<byte> intBuffer = stackalloc byte[4];

            //DataType
            BitConverter.TryWriteBytes(intBuffer, hasDataType ? dataTypeLength : -1);
            stream.Write(intBuffer);
            if (hasDataType)
            {
                Span<byte> dataTypeBuffer = stackalloc byte[dataTypeLength];
                Encoding.UTF8.GetBytes(_dataType, dataTypeBuffer);
                stream.Write(dataTypeBuffer);
            }

            //MimeType
            BitConverter.TryWriteBytes(intBuffer, hasMimeType ? serializerMimeTypeLength : -1);
            stream.Write(intBuffer);
            if (hasMimeType)
            {
                Span<byte> mimeTypeBuffer = stackalloc byte[serializerMimeTypeLength];
                Encoding.UTF8.GetBytes(_serializerMimeType, mimeTypeBuffer);
                stream.Write(mimeTypeBuffer);
            }

            //Data
            BitConverter.TryWriteBytes(intBuffer, _data.Count);
            stream.Write(intBuffer);
            _data.CopyTo(stream);
#endif
        }
        /// <summary>
        /// Get SubArray representation of the SerializedObject instance
        /// </summary>
        /// <returns>SubArray instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task CopyToAsync(Stream stream)
        {
            if (disposedValue) ThrowDisposedValue();
            var hasDataType = !string.IsNullOrEmpty(_dataType);
            var hasMimeType = !string.IsNullOrEmpty(_serializerMimeType);

            var dataTypeLength = hasDataType ? Encoding.UTF8.GetByteCount(_dataType) : 0;
            var serializerMimeTypeLength = hasMimeType ? Encoding.UTF8.GetByteCount(_serializerMimeType) : 0;

#if COMPATIBILITY
            byte[] buffer;

            //DataType
            buffer = BitConverter.GetBytes(hasDataType ? dataTypeLength : -1);
            await stream.WriteBytesAsync(buffer).ConfigureAwait(false);
            if (hasDataType)
            {
                buffer = Encoding.UTF8.GetBytes(_dataType);
                await stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            }

            //MimeType
            buffer = BitConverter.GetBytes(hasMimeType ? serializerMimeTypeLength : -1);
            await stream.WriteBytesAsync(buffer).ConfigureAwait(false);
            if (hasMimeType)
            {
                buffer = Encoding.UTF8.GetBytes(_serializerMimeType);
                await stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            }

            //Data
            buffer = BitConverter.GetBytes(_data.Count);
            await stream.WriteBytesAsync(buffer).ConfigureAwait(false);
            await _data.CopyToAsync(stream).ConfigureAwait(false);
#else
            using (var intBuffer = MemoryPool<byte>.Shared.Rent(4))
            {
                var intBufferMemory = intBuffer.Memory.Slice(0, 4);

                //DataType
                BitConverter.TryWriteBytes(intBufferMemory.Span, hasDataType ? dataTypeLength : -1);
                await stream.WriteAsync(intBufferMemory).ConfigureAwait(false);
                if (hasDataType)
                {
                    using (var dataTypeBuffer = MemoryPool<byte>.Shared.Rent(dataTypeLength))
                    {
                        var dataTypeBufferMemory = dataTypeBuffer.Memory.Slice(0, dataTypeLength);
                        Encoding.UTF8.GetBytes(_dataType, dataTypeBufferMemory.Span);
                        await stream.WriteAsync(dataTypeBufferMemory).ConfigureAwait(false);
                    }
                }

                //MimeType
                BitConverter.TryWriteBytes(intBufferMemory.Span, hasMimeType ? serializerMimeTypeLength : -1);
                await stream.WriteAsync(intBufferMemory).ConfigureAwait(false);
                if (hasMimeType)
                {
                    using (var mimeTypeBuffer = MemoryPool<byte>.Shared.Rent(serializerMimeTypeLength))
                    {
                        var mimeTypeBufferMemory = mimeTypeBuffer.Memory.Slice(0, serializerMimeTypeLength);
                        Encoding.UTF8.GetBytes(_serializerMimeType, mimeTypeBufferMemory.Span);
                        await stream.WriteAsync(mimeTypeBufferMemory).ConfigureAwait(false);
                    }
                }

                //Data
                BitConverter.TryWriteBytes(intBufferMemory.Span, _data.Count);
                await stream.WriteAsync(intBufferMemory).ConfigureAwait(false);
                await _data.CopyToAsync(stream).ConfigureAwait(false);
            }
#endif
        }

        /// <summary>
        /// Write the SerializedObject to a file
        /// </summary>
        /// <param name="filepath">Filepath to save the serialized object instance</param>
        /// <returns>Awaitable task instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task ToFileAsync(string filepath)
        {
            if (disposedValue) ThrowDisposedValue();
            if (!filepath.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
                filepath += FileExtension;

            using (await FileLockerAsync.GetLockAsync(filepath).LockAsync().ConfigureAwait(false))
            {
                using (var fs = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    await CopyToAsync(fs).ConfigureAwait(false);
                    await fs.FlushAsync().ConfigureAwait(false);
                }
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
            if (disposedValue) ThrowDisposedValue();
            if (!filepath.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
                filepath += FileExtension;
            using (FileLockerAsync.GetLockAsync(filepath).GetLock())
            {
                using (var fs = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    CopyTo(fs);
            }
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
            byte[] dataArray = null;
            string dataType = null;
            string mimeType = null;

#if COMPATIBILITY
            var lengthBytes = new byte[4];

            sequence.Slice(0, 4).CopyTo(lengthBytes);
            var dtLength = BitConverter.ToInt32(lengthBytes, 0);
            if (dtLength < -1 || dtLength > length) return null;
            if (dtLength != -1)
            {
                var buffer = new byte[dtLength];
                sequence.Slice(4, dtLength).CopyTo(buffer);
                dataType = Encoding.UTF8.GetString(buffer);
                length -= dtLength;
                sequence = sequence.Slice(4 + dtLength);
            }

            sequence.Slice(0, 4).CopyTo(lengthBytes);
            var smtLength = BitConverter.ToInt32(lengthBytes, 0);
            if (smtLength < -1 || smtLength > length) return null;
            if (smtLength != -1)
            {
                var buffer = new byte[smtLength];
                sequence.Slice(4, smtLength).CopyTo(buffer);
                mimeType = Encoding.UTF8.GetString(buffer);
                length -= smtLength;
                sequence = sequence.Slice(4 + smtLength);
            }

            sequence.Slice(0, 4).CopyTo(lengthBytes);
            var dataLength = BitConverter.ToInt32(lengthBytes, 0);
            if (dataLength < -1 || dataLength > length) return null;
            if (dataLength != -1)
            {
                dataArray = new byte[dataLength];
                var dSpan = dataArray.AsSpan();
                sequence.Slice(4, dataLength).CopyTo(dSpan);
            }
#else
            Span<byte> lengthSpan = stackalloc byte[4];

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
            if (dataLength != -1)
            {
                dataArray = new byte[dataLength];
                var dSpan = dataArray.AsSpan();
                sequence.Slice(4, dataLength).CopyTo(dSpan);
            }
#endif

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
            string dataType = null;
            string serializerMimeType = null;
            MultiArray<byte> data = MultiArray<byte>.Empty;

#if COMPATIBILITY
            var intBuffer = new byte[4];

            stream.ReadExact(intBuffer, 0, 4);
            var dataTypeByteLength = BitConverter.ToInt32(intBuffer, 0);
            if (dataTypeByteLength > -1)
            {
                var bytes = new byte[dataTypeByteLength];
                stream.ReadExact(bytes, 0, dataTypeByteLength);
                dataType = Encoding.UTF8.GetString(bytes);
            }

            stream.ReadExact(intBuffer, 0, 4);
            var serializerMimeTypeByteLength = BitConverter.ToInt32(intBuffer, 0);
            if (serializerMimeTypeByteLength > -1)
            {
                var bytes = new byte[serializerMimeTypeByteLength];
                stream.ReadExact(bytes, 0, serializerMimeTypeByteLength);
                serializerMimeType = Encoding.UTF8.GetString(bytes);
            }

            stream.ReadExact(intBuffer, 0, 4);
            var dataLength = BitConverter.ToInt32(intBuffer, 0);
            if (dataLength > -1)
            {
                var segmentLength = SegmentPool.SegmentLength;
                var rows = Math.DivRem(dataLength, segmentLength, out var pos);
                if (pos > 0)
                    rows++;
                else
                    pos = segmentLength;

                var bytes = SegmentPool.RentContainer();
                for (var i = 0; i < rows; i++)
                {
                    var row = SegmentPool.Rent();
                    stream.ReadExact(row, 0, i == rows - 1 ? pos : segmentLength);
                    bytes.Add(row);
                }

                data = new MultiArray<byte>(bytes, 0, dataLength);
            }
#else
            Span<byte> intBuffer = stackalloc byte[4];

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
                var segmentLength = SegmentPool.SegmentLength;
                var rows = Math.DivRem(dataLength, segmentLength, out var pos);
                if (pos > 0)
                    rows++;
                else
                    pos = segmentLength;

                var bytes = SegmentPool.RentContainer();
                for (var i = 0; i < rows; i++)
                {
                    var row = SegmentPool.Rent();
                    stream.ReadExact(row, 0, i == rows - 1 ? pos : segmentLength);
                    bytes.Add(row);
                }

                data = new MultiArray<byte>(bytes, 0, dataLength);
            }
#endif

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
            string dataType = null;
            string serializerMimeType = null;
            MultiArray<byte> data = MultiArray<byte>.Empty;

#if COMPATIBILITY
            var intBuffer = new byte[4];

            await stream.ReadExactAsync(intBuffer, 0, 4).ConfigureAwait(false);
            var dataTypeByteLength = BitConverter.ToInt32(intBuffer, 0);
            if (dataTypeByteLength > -1)
            {
                var bytes = new byte[dataTypeByteLength];
                await stream.ReadExactAsync(bytes, 0, dataTypeByteLength).ConfigureAwait(false);
                dataType = Encoding.UTF8.GetString(bytes);
            }

            await stream.ReadExactAsync(intBuffer, 0, 4).ConfigureAwait(false);
            var serializerMimeTypeByteLength = BitConverter.ToInt32(intBuffer, 0);
            if (serializerMimeTypeByteLength > -1)
            {
                var bytes = new byte[serializerMimeTypeByteLength];
                await stream.ReadExactAsync(bytes, 0, serializerMimeTypeByteLength).ConfigureAwait(false);
                serializerMimeType = Encoding.UTF8.GetString(bytes);
            }

            await stream.ReadExactAsync(intBuffer, 0, 4).ConfigureAwait(false);
            var dataLength = BitConverter.ToInt32(intBuffer, 0);
            if (dataLength > -1)
            {
                const int segmentLength = 8192;
                var rows = dataLength / segmentLength;
                var pos = dataLength % segmentLength;
                if (pos > 0)
                    rows++;
                else
                    pos = segmentLength;

                var bytes = SegmentPool.RentContainer();
                for (var i = 0; i < rows; i++)
                {
                    var row = SegmentPool.Rent();
                    await stream.ReadExactAsync(row, 0, i == rows - 1 ? pos : segmentLength).ConfigureAwait(false);
                    bytes.Add(row);
                }
                data = new MultiArray<byte>(bytes, 0, dataLength);
            }
#else
            var intBuffer = new byte[4];

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
                var segmentLength = SegmentPool.SegmentLength;
                var rows = Math.DivRem(dataLength, segmentLength, out var pos);
                if (pos > 0)
                    rows++;
                else
                    pos = segmentLength;

                var bytes = SegmentPool.RentContainer();
                for (var i = 0; i < rows; i++)
                {
                    var row = SegmentPool.Rent();
                    await stream.ReadExactAsync(row, 0, i == rows - 1 ? pos : segmentLength).ConfigureAwait(false);
                    bytes.Add(row);
                }
                data = new MultiArray<byte>(bytes, 0, dataLength);
            }
#endif

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
            using (await FileLockerAsync.GetLockAsync(filepath).LockAsync().ConfigureAwait(false))
            {
                using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    return await FromStreamAsync(fs).ConfigureAwait(false);
            }
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

            using (FileLockerAsync.GetLockAsync(filepath).GetLock())
            {
                using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    return FromStream(fs);
            }
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Gets the SerializedObject hash code
        /// </summary>
        /// <returns>Hash code value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override int GetHashCode()
        {
            if (disposedValue) ThrowDisposedValue();
            var hash = _serializerMimeType?.GetHashCode() ?? 0;
            hash += (_dataType?.GetHashCode() ?? 0) ^ 31;
            hash += _data.GetHashCode() ^ 31;
            return hash;
        }
        /// <summary>
        /// Gets the SerializedObject hash code using a custom EqualityComparer
        /// </summary>
        /// <param name="comparer">EqualityComparer instance</param>
        /// <returns>Hash code value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(IEqualityComparer comparer)
        {
            if (disposedValue) ThrowDisposedValue();
            var hash = _serializerMimeType != null ? comparer.GetHashCode(_serializerMimeType) : 0;
            hash += (_dataType != null ? comparer.GetHashCode(_dataType) : 0) ^ 31;
            hash += MultiArrayBytesComparer.Instance.GetHashCode(_data) ^ 31;
            return hash;
        }
        /// <summary>
        /// Gets if the SerializedObject instance is equal to other object
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if both objects are equal</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => base.Equals(obj as SerializedObject);
        /// <summary>
        /// Gets if the SerializedObject instance is equal to other SerializedObject
        /// </summary>
        /// <param name="other">SerializedObject to compare</param>
        /// <returns>True if both objects are equal</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SerializedObject other)
        {
            if (disposedValue) ThrowDisposedValue();
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other._dataType != _dataType) return false;
            if (other._serializerMimeType != _serializerMimeType) return false;
            return MultiArrayBytesComparer.Instance.Equals(_data, other._data);
        }
        /// <summary>
        /// Gets if the SerializedObject instance is equal to other object
        /// </summary>
        /// <param name="other">Object to compare</param>
        /// <param name="comparer">EqualityComparer instance</param>
        /// <returns>True if both objects are equal</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(object other, IEqualityComparer comparer)
        {
            if (disposedValue) ThrowDisposedValue();
            if (ReferenceEquals(other, null)) return false;
            if (!(other is SerializedObject bSer)) return false;
            if (!comparer.Equals(_dataType, bSer._dataType)) return false;
            if (!comparer.Equals(_serializerMimeType, bSer._serializerMimeType)) return false;
            return MultiArrayBytesComparer.Instance.Equals(_data, bSer._data);
        }
        /// <summary>
        /// Gets if the SerializedObject instance is equal to other SerializedObject
        /// </summary>
        /// <param name="a">First instance</param>
        /// <param name="b">Second instance</param>
        /// <returns>True if both instances are equal</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SerializedObject a, SerializedObject b)
        {
            if (ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return false;
            if (!ReferenceEquals(a, null) && ReferenceEquals(b, null)) return false;
            if (ReferenceEquals(a, null)) return true;
            if (ReferenceEquals(a, b)) return true;
            if (a._dataType != b._dataType) return false;
            if (a._serializerMimeType != b._serializerMimeType) return false;
            return MultiArrayBytesComparer.Instance.Equals(a._data, b._data);
        }
        /// <summary>
        /// Gets if the SerializedObject instance is different to other SerializedObject
        /// </summary>
        /// <param name="a">First instance</param>
        /// <param name="b">Second instance</param>
        /// <returns>True if both instances are different</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SerializedObject a, SerializedObject b)
        {
            return !(a == b);
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _dataType = null;
                    _serializerMimeType = null;
                }
                if (_canCollect)
                {
                    //Collect arrays
                    var lstArrays = _data.ListOfArrays;
                    if (lstArrays != null)
                    {
                        foreach (var arr in lstArrays)
                            SegmentPool.Return(arr);
                        if (lstArrays is List<byte[]> lBytes)
                            SegmentPool.ReturnContainer(lBytes);
                        else
                            lstArrays.Clear();
                    }
                    _data = MultiArray<byte>.Empty;
                    _canCollect = false;
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Dispose this instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}