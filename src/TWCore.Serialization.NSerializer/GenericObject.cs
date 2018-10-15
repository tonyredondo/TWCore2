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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
// ReSharper disable PossibleNullReferenceException
#pragma warning disable 1591

namespace TWCore.Serialization.NSerializer
{
    /// <summary>
    /// Generic object
    /// </summary>
    [DataContract]
    public class GenericObject
    {
        #region Properties
        /// <summary>
        /// Type name
        /// </summary>
        [XmlAttribute, DataMember]
        public string Type { get; set; }
        /// <summary>
        /// Is Array Object
        /// </summary>
        [XmlAttribute, DataMember]
        public bool IsArray { get; set; }
        /// <summary>
        /// Is List Object
        /// </summary>
        [XmlAttribute, DataMember]
        public bool IsList { get; set; }
        /// <summary>
        /// Is Dictionary Object
        /// </summary>
        [XmlAttribute, DataMember]
        public bool IsDictionary { get; set; }
        /// <summary>
        /// Properties
        /// </summary>
        [DataMember]
        public KeyValueCollection<string, object> Properties { get; set; }
        /// <summary>
        /// Dictionary Data
        /// </summary>
        [DataMember]
        public KeyValueCollection<object, object> DictionaryData { get; set; }
        /// <summary>
        /// List Data
        /// </summary>
        [DataMember]
        public List<object> ListData { get; set; }
        /// <summary>
        /// Array Data
        /// </summary>
        [DataMember]
        public object[] ArrayData { get; set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Generic object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GenericObject() { }
        /// <summary>
        /// Generic object
        /// </summary>
        /// <param name="metaData">Generic deserializer metadata</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GenericObject(GenericDeserializerMetaDataOfType metaData)
        {
            if (metaData != null)
            {
                Type = metaData.Type;
                IsArray = metaData.IsArray;
                IsList = metaData.IsList;
                IsDictionary = metaData.IsDictionary;

                if (metaData.Properties != null)
                {
                    Properties = new KeyValueCollection<string, object>();
                    foreach (var property in metaData.Properties)
                        Properties[property] = null;
                }
                if (IsList)
                {
                    ListData = new List<object>();
                }
                if (IsDictionary)
                {
                    DictionaryData = new KeyValueCollection<object, object>();
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the object property value
        /// </summary>
        /// <param name="key">Object property name</param>
        /// <returns>Property value</returns>
        public object this[string key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsDictionary && DictionaryData.TryGet(key, out var dictioValue))
                    return dictioValue;
                if (Properties.TryGet(key, out var value))
                    return value;
                if (!IsDictionary)
                    throw new Exception("Property is not found in the object");
                throw new KeyNotFoundException();
            }
        }
        /// <summary>
        /// Get the object dictionary value
        /// </summary>
        /// <param name="key">Object dictionary name</param>
        /// <returns>Property value</returns>
        public object this[object key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsDictionary && DictionaryData.TryGet(key, out var dictioValue))
                    return dictioValue;
                if (!IsDictionary)
                    throw new Exception("The object is not a Dictionary");
                throw new KeyNotFoundException();
            }
        }
        /// <summary>
        /// Get the index value of an array or list
        /// </summary>
        /// <param name="index">Index number</param>
        /// <returns>Object value</returns>
        public object this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsArray)
                {
                    if (ArrayData != null)
                        return ArrayData[index];
                    else
                        throw new Exception("The ArrayData has not been initialized");
                }
                if (IsList)
                    return ListData[index];
                throw new Exception("The object is not an Array/List");
            }
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Set property value
        /// </summary>
        /// <param name="key">Object property key</param>
        /// <param name="value">Object property value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetProperty(string key, object value)
        {
            Properties[key] = value;
        }
        /// <summary>
        /// Init array value
        /// </summary>
        /// <param name="capacity">Array capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InitArray(int capacity)
        {
            ArrayData = new object[capacity];
        }
        /// <summary>
        /// Set an array value
        /// </summary>
        /// <param name="index">Index of the array</param>
        /// <param name="value">Object value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetArrayValue(int index, object value)
        {
            ArrayData[index] = value;
        }
        /// <summary>
        /// Set a list value
        /// </summary>
        /// <param name="index">Index of the list</param>
        /// <param name="value">Object value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetListValue(int index, object value)
        {
            ListData[index] = value;
        }
        /// <summary>
        /// Set a dictionary key/value
        /// </summary>
        /// <param name="key">Dictionary key</param>
        /// <param name="value">Dictionary value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetDictionaryValue(object key, object value)
        {
            DictionaryData[key] = value;
        }
        #endregion
    }
}