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

namespace TWCore.Serialization
{
    /// <summary>
    /// Global Serializer Manager
    /// </summary>
    public static class SerializerManager
    {
        /// <summary>
        /// Default Binary Serializer
        /// </summary>
        public static ISerializer DefaultBinarySerializer = null;
        /// <summary>
        /// Default Text Serializer
        /// </summary>
        public static ITextSerializer DefaultTextSerializer = null;
        /// <summary>
        /// All registered serializers
        /// </summary>
        public static readonly KeyDelegatedCollection<Type, ISerializer> Serializers = new KeyDelegatedCollection<Type, ISerializer>(i => i.GetType());
        /// <summary>
        /// Default value to indicate the known types
        /// </summary>
        public static List<Type> DefaultKnownTypes = new List<Type>();
        /// <summary>
        /// Supress the file extension warning from the log
        /// </summary>
        public static bool SupressFileExtensionWarning = false;

        #region .ctor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static SerializerManager()
        {
            Core.RunOnInit(() =>
            {
                if (Factory.GetAllAssemblies == null)
                    Core.Log.Warning("The PlatformEngine is null.");
                else
                {
                    var assemblies = Factory.GetAllAssemblies();
                    var allSerializers = new List<Type>();
                    foreach (var assembly in assemblies)
                    {
                        try
                        {
                            foreach (var type in assembly.DefinedTypes)
                            {
                                if (!type.IsInterface && !type.IsAbstract && type.ImplementedInterfaces.Any(i => i == typeof(ISerializer)))
                                    allSerializers.Add(type.AsType());
                            }
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(LogLevel.Warning, $"An error occurs when loading the types for '{assembly.FullName}' assembly, the assembly was ignored.", ex);
                        }
                    }
                    foreach (var serType in allSerializers.OrderByDescending(t => t.Name))
                    {
                        try
                        {
                            Register((ISerializer)Activator.CreateInstance(serType));
                        }
                        catch(Exception ex)
                        {
                            Core.Log.Write(LogLevel.Warning, $"Error registering the '{serType.FullName}' serializer, the type was ignored.", ex);
                        }
                    }
                }

                Core.Status.Attach(() =>
                {
                    var item = new StatusItem
                    {
                        Name = "Serializer Manager"
                    };
                    var collection = item.Values;
                    collection.Add(nameof(DefaultBinarySerializer), DefaultBinarySerializer);
                    collection.Add(nameof(DefaultTextSerializer), DefaultTextSerializer);
                    collection.Add("DefaultKnownTypes Count", DefaultKnownTypes.Count);
                    collection.Add("Serializers Count", Serializers.Count);
                    collection.Add("Serializers", Serializers.Join(", "));
                    foreach (var ser in Serializers)
                        Core.Status.AttachChild(ser, item);
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
            if (serializer != null)
            {
                if (!Serializers.Contains(serializer.GetType()))
                    Serializers.Add(serializer);

                if (DefaultBinarySerializer == null && serializer.SerializerType == SerializerType.Binary)
                    DefaultBinarySerializer = serializer;

                if (DefaultTextSerializer == null && serializer.SerializerType == SerializerType.Text)
                    DefaultTextSerializer = (ITextSerializer)serializer;
            }
        }
        /// <summary>
        /// Deregister a serializer
        /// </summary>
        /// <param name="serializer">Serializer to deregister</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deregister(ISerializer serializer)
        {
            if (serializer != null)
            {
                var key = serializer.GetType();
                if (Serializers.Contains(key))
                    Serializers.Remove(key);

                if (DefaultBinarySerializer == serializer)
                    DefaultBinarySerializer = Serializers.FirstOrDefault(i => i.SerializerType == SerializerType.Binary);

                if (DefaultTextSerializer == serializer)
                    DefaultTextSerializer = (ITextSerializer)Serializers.FirstOrDefault(i => i.SerializerType == SerializerType.Text);
            }
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
            var ser = Serializers.FirstOrDefault(i => i.MimeTypes.Any(s => s.ToLowerInvariant() == mimeType));
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
            var ser = Serializers.FirstOrDefault(i => i.Extensions.Any(s => s.ToLowerInvariant() == fileExtension));
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
                serializer = Serializers.FirstOrDefault(i => i.Extensions.Any(s => s.ToLowerInvariant() == "." + ext));
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
                    if (ov != null)
                    {
                        var ovType = ov.GetType();
                        if (ovType.IsConstructedGenericType)
                            ovType.GenericTypeArguments.Each(t => extraTypes.Add(t));
                        if (ovType == typeof(object))
                            FillExtraTypes(extraTypes, ov, p.PropertyType);
                        else
                            extraTypes.Add(ov.GetType());
                    }
                }
                catch { }
            });
            fields.Each(f =>
            {
                try
                {
                    var ov = f.GetValue(item);
                    if (ov != null)
                    {
                        var ovType = ov.GetType();
                        if (ovType.IsConstructedGenericType)
                            ovType.GenericTypeArguments.Each(t => extraTypes.Add(t));
                        extraTypes.Add(ov.GetType());
                    }
                }
                catch { }
            });
        }
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
