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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TWCore.Compression;
using TWCore.IO;
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable AccessToModifiedClosure

namespace TWCore.Serialization
{
    /// <inheritdoc cref="ITextSerializer" />
    /// <summary>
    /// Text serializer base class
    /// </summary>
    public abstract class TextSerializer : ITextSerializer, ICoreStart
    {
        private static readonly InstanceLocker<string> FilePathLocker = new InstanceLocker<string>();

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Serializer Type
        /// </summary>
        public SerializerType SerializerType { get; } = SerializerType.Text;
        /// <inheritdoc />
        /// <summary>
        /// Supported mime types
        /// </summary>
        public abstract string[] MimeTypes { get; }
        /// <inheritdoc />
        /// <summary>
        /// Supported file extensions
        /// </summary>
        public abstract string[] Extensions { get; }
        /// <summary>
        /// Encoding to use.
        /// </summary>
        public virtual Encoding Encoding { get; set; } = new UTF8Encoding(false);
        /// <inheritdoc />
        /// <summary>
        /// Known types to add to the serializer
        /// </summary>
        public virtual HashSet<Type> KnownTypes { get; } = new HashSet<Type>();
        /// <inheritdoc />
        /// <summary>
        /// Compresor used on Serialization and Deserialization of byte arrays, streams and files
        /// </summary>
        /// <remarks>This compressor is ignored on TextSerializer</remarks>
        public ICompressor Compressor { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// true if the serializer extension and/or compressor extension would be appended to the filePath; otherwise, false.
        /// </summary>
        public bool UseFileExtensions { get; set; } = true;
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Gets the object instance deserialized from a stream
        /// </summary>
        /// <param name="stream">Deserialized stream value</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract object OnDeserialize(Stream stream, Type itemType);
        /// <summary>
        /// Serialize an object and writes it to the stream
        /// </summary>
        /// <param name="stream">Destination stream</param>
        /// <param name="item">Object instance to serialize</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Deserialized byte array value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void OnSerialize(Stream stream, object item, Type itemType);
        #endregion

        #region ISerializer
        /// <inheritdoc />
        /// <summary>
        /// Serialize an object to a stream
        /// </summary>
        /// <param name="item">Object to serialize</param>
        /// <param name="itemType">Object type</param>
        /// <param name="stream">Stream data destination</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(object item, Type itemType, Stream stream)
        {
            if (Compressor is null)
            {
                OnSerialize(stream, item, itemType);
                return;
            }
            using (var ms = new RecycleMemoryStream())
            {
                OnSerialize(ms, item, itemType);
                ms.Position = 0;
                Compressor.Compress(ms, stream);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Deserialize a stream content to a object
        /// </summary>
        /// <param name="stream">Stream data source</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Deserialize(Stream stream, Type itemType)
        {
            if (Compressor is null)
            {
                return OnDeserialize(stream, itemType);
            }
            using (var ms = new RecycleMemoryStream())
            {
                Compressor.Decompress(stream, ms);
                ms.Position = 0;
                return OnDeserialize(ms, itemType);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="item">Object to serialize</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Serialized byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray<byte> Serialize(object item, Type itemType)
        {
            using (var stream = new RecycleMemoryStream())
            {
                if (Compressor is null)
                {
                    OnSerialize(stream, item, itemType);
                    return stream.GetMultiArray();
                }
                using (var ms = new RecycleMemoryStream())
                {
                    OnSerialize(ms, item, itemType);
                    ms.Position = 0;
                    Compressor.Compress(ms, stream);
                }
                return stream.GetMultiArray();
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Deserialize a byte array value to an item type
        /// </summary>
        /// <param name="value">Value to deserialize</param>
        /// <param name="valueType">Value type</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Deserialize(MultiArray<byte> value, Type valueType)
        {
            using (var stream = value.AsReadOnlyStream())
            {
                if (Compressor is null)
                {
                    return OnDeserialize(stream, valueType);
                }
                using (var ms = new RecycleMemoryStream())
                {
                    Compressor.Decompress(stream, ms);
                    ms.Position = 0;
                    return OnDeserialize(ms, valueType);
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Serialize an object to a string
        /// </summary>
        /// <param name="item">Object to serialize</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Serialized string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string SerializeToString(object item, Type itemType)
        {
            using (var ms = new RecycleMemoryStream())
            {
                OnSerialize(ms, item, itemType);
                return Encoding.GetString(ms.ToArray());
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Deserialize a string to an object value
        /// </summary>
        /// <param name="value">String to deserialize</param>
        /// <param name="itemType">Object type</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object DeserializeFromString(string value, Type itemType)
        {
            using (var ms = new MemoryStream(Encoding.GetBytes(value)))
                return OnDeserialize(ms, itemType);
        }


        /// <inheritdoc />
        /// <summary>
        /// Serialize an object to a filepath
        /// </summary>
        /// <param name="item">Object to serialize</param>
        /// <param name="itemType">Object type</param>
        /// <param name="filePath">File path to write the results of the serialization</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SerializeToFile(object item, Type itemType, string filePath)
        {
            filePath = Factory.GetAbsolutePath(filePath);
            var resPath = filePath;
            if (UseFileExtensions)
            {
                if (Compressor != null)
                {
                    var compExt = Compressor.FileExtension;
                    if (!Extensions.Any((ext, vTuple) => vTuple.resPath.EndsWith(ext + vTuple.compExt, StringComparison.OrdinalIgnoreCase), (resPath, compExt)))
                    {
                        if (!Extensions.Any((ext, rPath) => rPath.EndsWith(ext, StringComparison.OrdinalIgnoreCase), resPath))
                            resPath = filePath + Extensions.FirstOrDefault() + compExt;
                        else
                            resPath = filePath + compExt;
                    }
                }
                else if (!Extensions.Any((ext, rPath) => rPath.EndsWith(ext, StringComparison.OrdinalIgnoreCase), resPath))
                {
                    resPath = filePath + Extensions.FirstOrDefault();
                }
            }
            if (!string.Equals(resPath, filePath, StringComparison.OrdinalIgnoreCase) && !SerializerManager.SupressFileExtensionWarning)
                Core.Log.Warning("The {0} is using the UseFileExtensions flag, so the file: {1} was changed to: {2}", GetType().Name, filePath, resPath);
            lock (FilePathLocker.GetLock(resPath))
            {
                using (var stream = File.Open(resPath, FileMode.Create, FileAccess.Write))
                    Serialize(item, itemType, stream);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Serialize an object to a filepath
        /// </summary>
        /// <param name="item">Object to serialize</param>
        /// <param name="itemType">Object type</param>
        /// <param name="filePath">File path to write the results of the serialization</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual async Task SerializeToFileAsync(object item, Type itemType, string filePath)
        {
            filePath = Factory.GetAbsolutePath(filePath);
            var resPath = filePath;
            if (UseFileExtensions)
            {
                if (Compressor != null)
                {
                    var compExt = Compressor.FileExtension;
                    if (!Extensions.Any((ext, vTuple) => vTuple.resPath.EndsWith(ext + vTuple.compExt, StringComparison.OrdinalIgnoreCase), (resPath, compExt)))
                    {
                        if (!Extensions.Any((ext, rPath) => rPath.EndsWith(ext, StringComparison.OrdinalIgnoreCase), resPath))
                            resPath = filePath + Extensions.FirstOrDefault() + compExt;
                        else
                            resPath = filePath + compExt;
                    }
                }
                else if (!Extensions.Any((ext, rPath) => rPath.EndsWith(ext, StringComparison.OrdinalIgnoreCase), resPath))
                {
                    resPath = filePath + Extensions.FirstOrDefault();
                }
            }
            if (!string.Equals(resPath, filePath, StringComparison.OrdinalIgnoreCase) && !SerializerManager.SupressFileExtensionWarning)
                Core.Log.Warning("The {0} is using the UseFileExtensions flag, so the file: {1} was changed to: {2}", GetType().Name, filePath, resPath);
            using (var stream = new RecycleMemoryStream())
            {
                Serialize(item, itemType, stream);
                stream.Position = 0;
                using (var fstream = File.Open(resPath, FileMode.Create, FileAccess.Write))
                    await stream.CopyToAsync(fstream).ConfigureAwait(false);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Deserialize a file to an object instance
        /// </summary>
        /// <param name="itemType">Object type</param>
        /// <param name="filePath">File path to read the content to deserialize</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual object DeserializeFromFile(Type itemType, string filePath)
        {
            filePath = Factory.GetAbsolutePath(filePath);
            var fPath = filePath;
            if (UseFileExtensions && !File.Exists(fPath))
            {
                fPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath));
                var exist = false;
                foreach (var ext in Extensions)
                {
                    var tPath = fPath + ext + Compressor?.FileExtension;
                    if (!File.Exists(tPath)) continue;
                    exist = true;
                    fPath = tPath;
                    break;
                }
                if (!exist && Compressor != null)
                {
                    foreach (var ext in Extensions)
                    {
                        var tPath = fPath + ext;
                        if (!File.Exists(tPath)) continue;
                        exist = true;
                        fPath = tPath;
                        break;
                    }
                }
                if (!exist)
                    fPath = filePath;
            }
            if (!string.Equals(fPath, filePath, StringComparison.OrdinalIgnoreCase) && !SerializerManager.SupressFileExtensionWarning)
                Core.Log.Warning("The {0} is using the UseFileExtensions flag, so the file: {1} was changed to: {2}", GetType().Name, filePath, fPath);
            lock (FilePathLocker.GetLock(fPath))
            {
                object obj;
                using (var stream = File.Open(fPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    obj = Deserialize(stream, itemType);
                return obj;
            }
        }
        #endregion

        #region ISerializer<T>
        /// <inheritdoc />
        /// <summary>
        /// Serialize an object to a stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object to serialize</param>
        /// <param name="stream">Stream data destination</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(T item, Stream stream) => Serialize(item, item?.GetType() ?? typeof(T), stream);
        /// <inheritdoc />
        /// <summary>
        /// Deserialize a stream content to a object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream data source</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize<T>(Stream stream) => (T)Deserialize(stream, typeof(T));

        /// <inheritdoc />
        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object to serialize</param>
        /// <returns>Serialized byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MultiArray<byte> Serialize<T>(T item) => ((ISerializer)this).Serialize(item, item?.GetType() ?? typeof(T));
        /// <inheritdoc />
        /// <summary>
        /// Deserialize a byte array value to an item type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Byte array to deserialize</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize<T>(MultiArray<byte> value) => (T)((ISerializer)this).Deserialize(value, typeof(T));

        /// <inheritdoc />
        /// <summary>
        /// Serialize an object to a string
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object to serialize</param>
        /// <returns>Serialized string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string SerializeToString<T>(T item) => SerializeToString(item, item?.GetType() ?? typeof(T));
        /// <inheritdoc />
        /// <summary>
        /// Deserialize a string to an object value
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">String to deserialize</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T DeserializeFromString<T>(string value) => (T)DeserializeFromString(value, typeof(T));

        /// <inheritdoc />
        /// <summary>
        /// Serialize an object to a filepath
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object to serialize</param>
        /// <param name="filePath">File path to write the results of the serialization</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeToFile<T>(T item, string filePath) => SerializeToFile(item, item?.GetType() ?? typeof(T), filePath);
        /// <inheritdoc />
        /// <summary>
        /// Serialize an object to a filepath
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="item">Object to serialize</param>
        /// <param name="filePath">File path to write the results of the serialization</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task SerializeToFileAsync<T>(T item, string filePath) => SerializeToFileAsync(item, item?.GetType() ?? typeof(T), filePath);
        /// <inheritdoc />
        /// <summary>
        /// Deserialize a file to an object instance
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="filePath">File path to read the content to deserialize</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T DeserializeFromFile<T>(string filePath) => (T)DeserializeFromFile(typeof(T), filePath);
        #endregion

        #region IDeepCloneable
        /// <inheritdoc />
        /// <summary>
        /// Make a deep clone of the object
        /// </summary>
        /// <returns>A brand new object</returns>
        public abstract ISerializer DeepClone();
        #endregion

        #region GetSerializedObject
        /// <summary>
        /// Get Serialized Object
        /// </summary>
        /// <param name="item">Item to serialize</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedObject GetSerializedObject(object item) => item is SerializedObject serObj ? serObj : new SerializedObject(item, this);
        /// <summary>
        /// Get Serialized Object
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="item">Item to serialize</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SerializedObject GetSerializedObject<T>(T item) => item is SerializedObject serObj ? serObj : new SerializedObject(item, this);
        #endregion

        #region CoreStart
        void ICoreStart.BeforeInit() { }
        void ICoreStart.AfterFactoryInit(Factories factories)
            => SerializerManager.Register(this);
        void ICoreStart.FinalizingInit(Factories factories) { }
        #endregion
    }
}
