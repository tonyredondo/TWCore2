/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Diagnostics.Log;
using TWCore.Diagnostics.Status;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace TWCore.Serialization
{
    /// <summary>
    /// Global Serializer Manager
    /// </summary>
    public static class SerializerManager
    {
        private static ISerializer _defaultBinarySerializer;
        private static ITextSerializer _defaultTextSerializer;


        /// <summary>
        /// Default Binary Serializer
        /// </summary>
        public static ISerializer DefaultBinarySerializer
        {
            get
            {
                if (_defaultBinarySerializer != null)
                    return _defaultBinarySerializer;
                foreach(var mime in DefaultBinarySerializerMimeTypes)
                {
                    var serializer = GetByMimeType(mime);
                    if (serializer == null) continue;
                    if (mime == SerializerMimeTypes.BinaryFormatter && DefaultBinarySerializerMimeTypes.Last() == mime)
                        Core.Log.Warning("The BinaryFormatter serializer was loaded and this probably is not what you want. Please check the nuget package references for other serializer. If this is intented, ignore the message.");
                    return serializer;
                }
                return Serializers.FirstOrDefault(s => s.SerializerType == SerializerType.Binary);
            }
            set => _defaultBinarySerializer = value;
        }
        /// <summary>
        /// Default Text Serializer
        /// </summary>
        public static ITextSerializer DefaultTextSerializer
        {
            get
            {
                if (_defaultTextSerializer != null)
                    return _defaultTextSerializer;
                foreach (var mime in DefaultTextSerializerMimeTypes)
                {
                    var serializer = GetByMimeType(mime);
                    if (serializer is ITextSerializer tser)
                        return tser;
                }
                return (ITextSerializer)Serializers.FirstOrDefault(s => s.SerializerType == SerializerType.Text);
            }
            set => _defaultTextSerializer = value;
        }
        /// <summary>
        /// All registered serializers
        /// </summary>
        public static readonly KeyDelegatedCollection<Type, ISerializer> Serializers = new KeyDelegatedCollection<Type, ISerializer>(i => i.GetType());
        /// <summary>
        /// Default value to indicate the known types
        /// </summary>
        public static readonly HashSet<Type> DefaultKnownTypes = new HashSet<Type>();
        /// <summary>
        /// Supress the file extension warning from the log
        /// </summary>
        public static bool SupressFileExtensionWarning = false;
        /// <summary>
        /// Default Binary Serializer MimeTypes Load array
        /// </summary>
        public static string[] DefaultBinarySerializerMimeTypes = { SerializerMimeTypes.WBinary, SerializerMimeTypes.PWBinary, SerializerMimeTypes.BinaryFormatter };
        /// <summary>
        /// Default Text Serializer MimeTypes Load array
        /// </summary>
        public static string[] DefaultTextSerializerMimeTypes = { SerializerMimeTypes.Json, SerializerMimeTypes.Xml };


        #region .ctor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static SerializerManager()
        {
            Core.RunOnInit(() =>
            {
                if (Factory.GetAllAssemblies != null)
                {
                    var assemblies = Factory.GetAllAssemblies();
                    foreach (var assembly in assemblies)
                    {
                        try
                        {
                            foreach (var type in assembly.DefinedTypes)
                            {
                                if (type.IsInterface || type.IsAbstract || type.ImplementedInterfaces.All(i => i != typeof(ISerializer))) continue;
                                var serType = type.AsType();
                                try
                                {
                                    Register((ISerializer)Activator.CreateInstance(serType));
                                }
                                catch (Exception ex)
                                {
                                    Core.Log.Write(LogLevel.Warning, $"Error registering the '{serType.FullName}' serializer, the type was ignored.", ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(LogLevel.Warning, $"An error occurs when loading the types for '{assembly.FullName}' assembly, the assembly was ignored.", ex);
                        }
                    }
                }

                Core.Status.Attach(() =>
                {
                    var item = new StatusItem
                    {
                        Name = "Application Information\\Serializer Manager"
                    };
                    var collection = item.Values;
                    collection.Add(nameof(DefaultBinarySerializer), DefaultBinarySerializer);
                    collection.Add(nameof(DefaultTextSerializer), DefaultTextSerializer);
                    collection.Add("DefaultKnownTypes Count", DefaultKnownTypes.Count);
                    var serType = Serializers.GroupBy(s => s.SerializerType).ToArray();
                    var serValues = serType.Select(s => new StatusItemValueItem(s.Key.ToString(), s.Join("\r\n"))).ToArray();
                    collection.Add("Serializers", serValues);
                    return item;
                });
            });
        }
        #endregion


        #region Registration
        /// <summary>
        /// Register a new serializer
        /// </summary>
        /// <param name="serializer">Serializer to register</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Register(ISerializer serializer)
        {
            if (serializer != null && !Serializers.Contains(serializer.GetType()))
                Serializers.Add(serializer);
        }
        /// <summary>
        /// Deregister a serializer
        /// </summary>
        /// <param name="serializer">Serializer to deregister</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deregister(ISerializer serializer)
        {
            if (serializer == null) return;
            
            var key = serializer.GetType();
            if (Serializers.Contains(key))
                Serializers.Remove(key);

            if (_defaultBinarySerializer == serializer)
                _defaultBinarySerializer = null;

            if (_defaultTextSerializer == serializer)
                _defaultTextSerializer = null;
        }
        #endregion

        #region GetByMimeType
        /// <summary>
        /// Get a serializer by mime type
        /// </summary>
        /// <param name="mimeType">Mime Type</param>
        /// <returns>Serializer to handle the mime type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ISerializer GetByMimeType(string mimeType)
        {
            if (mimeType == null)
                return null;
            mimeType = mimeType.ToLowerInvariant();
            var ser = Serializers.FirstOrDefault(i => i.MimeTypes.Any(s => string.Equals(s, mimeType)));
            return ser?.DeepClone();
        }
        /// <summary>
        /// Get a serializer by mime type
        /// </summary>
        /// <param name="mimeType">Mime Type</param>
        /// <typeparam name="T">Serializer type</typeparam>
        /// <returns>Serializer to handle the mime type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetByMimeType<T>(string mimeType) where T : ISerializer => (T)GetByMimeType(mimeType);
        #endregion

        #region GetByFileExtension
        /// <summary>
        /// Get a serializer by file extension
        /// </summary>
        /// <param name="fileExtension">File extension</param>
        /// <returns>Serializer to handle the file extension</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ISerializer GetByFileExtension(string fileExtension)
        {
            fileExtension = fileExtension.ToLowerInvariant();
            var ser = Serializers.FirstOrDefault(i => i.Extensions.Any(s => string.Equals(s, fileExtension, StringComparison.OrdinalIgnoreCase)));
            return ser?.DeepClone();
        }
        /// <summary>
        /// Get a serializer by file extension
        /// </summary>
        /// <param name="fileExtension">File extension</param>
        /// <typeparam name="T">Serializer type</typeparam>
        /// <returns>Serializer to handle the file extension</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetByFileExtension<T>(string fileExtension) where T : ISerializer => (T)GetByFileExtension(fileExtension);
        #endregion

        #region GetByType
        /// <summary>
        /// Get a registered serializer by type
        /// </summary>
        /// <typeparam name="T">Serializer type</typeparam>
        /// <returns>Serializer instance of that type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetByType<T>() where T : class, ISerializer => (T)Serializers.FirstOrDefault(s => s.GetType() == typeof(T)) ?? Activator.CreateInstance<T>();
        #endregion

        #region GetByFileName
        /// <summary>
        /// Get a serializer by file name extensions
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Serializer to handle the file extension</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ISerializer GetByFileName(string fileName)
        {
            fileName = fileName.ToLowerInvariant();
            var extensions = fileName.SplitAndTrim('.').Skip(1).Reverse().ToList();
            ICompressor compressor = null;
            ISerializer serializer = null;
            foreach (var ext in extensions)
            {
                serializer = Serializers.FirstOrDefault(i => i.Extensions.Any(s => string.Equals(s, "." + ext, StringComparison.OrdinalIgnoreCase)));
                if (serializer != null)
                {
                    serializer = serializer.DeepClone();
                    serializer.Compressor = compressor;
                    break;
                }
                compressor = CompressorManager.GetByFileExtension("." + ext);
            }
            return serializer;
        }
        /// <summary>
        /// Get a serializer by file name extensions
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <typeparam name="T">Serializer type</typeparam>
        /// <returns>Serializer to handle the file extension</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetByFileName<T>(string fileName) where T : ISerializer => (T)GetByFileName(fileName);
        #endregion

        #region GetExtraTypes
        /// <summary>
        /// Fills the hashset with the type not defined on compiler time but in runtime (object properties, interfaces, etc)
        /// </summary>
        /// <param name="extraTypes">Extra types hashset</param>
        /// <param name="item">Serializable object value</param>
        /// <param name="itemType">Object type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillExtraTypes(HashSet<Type> extraTypes, object item, Type itemType)
        {
            var props = itemType.GetRuntimeProperties().Where(p => p.PropertyType == typeof(object) && p.Name != "System.Collections.IList.Item" && p.CanRead);
            var fields = itemType.GetRuntimeFields().Where(f => f.FieldType == typeof(object));
            props.Each(p =>
            {
                try
                {
                    var ov = p.GetValue(item);
                    if (ov == null) return;
                    var ovType = ov.GetType();
                    if (ovType.IsConstructedGenericType)
                        ovType.GenericTypeArguments.Each(t => extraTypes.Add(t));
                    if (ovType == typeof(object))
                        FillExtraTypes(extraTypes, ov, p.PropertyType);
                    else
                        extraTypes.Add(ov.GetType());
                }
                catch
                {
                    // ignored
                }
            });
            fields.Each(f =>
            {
                try
                {
                    var ov = f.GetValue(item);
                    if (ov == null) return;
                    var ovType = ov.GetType();
                    if (ovType.IsConstructedGenericType)
                        ovType.GenericTypeArguments.Each(t => extraTypes.Add(t));
                    extraTypes.Add(ov.GetType());
                }
                catch
                {
                    // ignored
                }
            });
        }
        #endregion

        #region GetBinarySerializers
        /// <summary>
        /// Gets the binary serializers
        /// </summary>
        /// <returns>ISerializer array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ISerializer[] GetBinarySerializers()
            => Serializers.Where(s => s.SerializerType == SerializerType.Binary).ToArray();
        /// <summary>
        /// Gets the texts serializers
        /// </summary>
        /// <returns>ISerializer array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ISerializer[] GetTextSerializers()
            => Serializers.Where(s => s.SerializerType == SerializerType.Text).ToArray();
        #endregion

        #region Serialize/Deserialize
        /// <summary>
        /// Serialize object
        /// </summary>
        /// <param name="item">Object instance</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject Serialize(object item) => new SerializedObject(item);
        /// <summary>
        /// Serialize object
        /// </summary>
        /// <param name="item">Object instance</param>
        /// <returns>SerializedObject instance</returns>
        /// <param name="serializer">Serializer object instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject Serialize(object item, ISerializer serializer) => new SerializedObject(item, serializer);
        /// <summary>
        /// Serialize object
        /// </summary>
        /// <param name="item">Object instance</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject Serialize<T>(T item) => new SerializedObject(item);
        /// <summary>
        /// Serialize object
        /// </summary>
        /// <param name="item">Object instance</param>
        /// <param name="serializer">Serializer object instance</param>
        /// <returns>SerializedObject instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedObject Serialize<T>(T item, ISerializer serializer) => new SerializedObject(item, serializer);
        /// <summary>
        /// Deserialize object
        /// </summary>
        /// <param name="item">SerializedObject instance</param>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Deserialize(SerializedObject item) => item?.GetValue();
        /// <summary>
        /// Deserialize object
        /// </summary>
        /// <param name="item">SerializedObject instance</param>
        /// <returns>Object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Deserialize<T>(SerializedObject item) => (T)item?.GetValue();
        #endregion
    }
}
