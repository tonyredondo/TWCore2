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
using System.Diagnostics;
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
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _isArray;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _isList;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _isDictionary;

        #region Properties
        /// <summary>
        /// Type name
        /// </summary>
        [XmlAttribute, DataMember, Newtonsoft.Json.JsonProperty(
        	NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
			TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None,
			ItemTypeNameHandling = Newtonsoft.Json.TypeNameHandling.None)]
        public string Type { get; set; }
        /// <summary>
        /// Is Array Object
        /// </summary>
        [XmlIgnore]
        public bool IsArray
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Array != null || _isArray;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _isArray = value;
        }
        /// <summary>
        /// Is List Object
        /// </summary>
        [XmlIgnore]
        public bool IsList
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => List != null || _isList;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _isList = value;
        }
        /// <summary>
        /// Is Dictionary Object
        /// </summary>
        [XmlIgnore]
        public bool IsDictionary
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Dictionary != null || _isDictionary;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _isDictionary = value;
        }
        /// <summary>
        /// Properties
        /// </summary>
        [DataMember, Newtonsoft.Json.JsonProperty(
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore, 
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None, 
            ItemTypeNameHandling = Newtonsoft.Json.TypeNameHandling.None)]
        public Dictionary<string, GenericObject> Properties { get; set; }
        /// <summary>
        /// Dictionary Data
        /// </summary>
        [DataMember, Newtonsoft.Json.JsonProperty(
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None,
            ItemTypeNameHandling = Newtonsoft.Json.TypeNameHandling.None)]
        public Dictionary<object, GenericObject> Dictionary { get; set; }
        /// <summary>
        /// List Data
        /// </summary>
        [DataMember, Newtonsoft.Json.JsonProperty(
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None,
            ItemTypeNameHandling = Newtonsoft.Json.TypeNameHandling.None)]
        public List<GenericObject> List { get; set; }
        /// <summary>
        /// Array Data
        /// </summary>
        [DataMember, Newtonsoft.Json.JsonProperty(
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None,
            ItemTypeNameHandling = Newtonsoft.Json.TypeNameHandling.None)]
        public GenericObject[] Array { get; set; }
		/// <summary>
		/// Value Data
		/// </summary>
		[DataMember, Newtonsoft.Json.JsonProperty(
			NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
			TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None,
			ItemTypeNameHandling = Newtonsoft.Json.TypeNameHandling.None)]
		public object Value { get; set; }
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
                    Properties = new Dictionary<string, GenericObject>();
                    foreach (var property in metaData.Properties)
                        Properties[property] = null;
                }
                if (IsList)
                {
                    List = new List<GenericObject>();
                }
                if (IsDictionary)
                {
                    Dictionary = new Dictionary<object, GenericObject>();
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
        public GenericObject this[string key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsDictionary && Dictionary.TryGetValue(key, out var dictioValue))
                    return dictioValue;
                if (Properties.TryGetValue(key, out var value))
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
        public GenericObject this[object key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsDictionary && Dictionary.TryGetValue(key, out var dictioValue))
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
        public GenericObject this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsArray)
                {
                    if (Array != null)
                        return Array[index];
                    else
                        throw new Exception("The ArrayData has not been initialized");
                }
                if (IsList)
                    return List[index];
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
			if (value is GenericObject gObject)
				Properties[key] = gObject;
			else
				Properties[key] = new GenericObject { Value = value };
        }
        /// <summary>
        /// Init array value
        /// </summary>
        /// <param name="capacity">Array capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InitArray(int capacity)
        {
            Array = new GenericObject[capacity];
        }
        /// <summary>
        /// Set an array value
        /// </summary>
        /// <param name="index">Index of the array</param>
        /// <param name="value">Object value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetArrayValue(int index, object value)
        {
			if (value is GenericObject gObject)
				Array[index] = gObject;
			else
				Array[index] = new GenericObject { Value = value };
		}
        /// <summary>
        /// Set a list value
        /// </summary>
        /// <param name="index">Index of the list</param>
        /// <param name="value">Object value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetListValue(int index, object value)
        {
			if (value is GenericObject gObject)
				List[index] = gObject;
			else
				List[index] = new GenericObject { Value = value };
        }
        /// <summary>
        /// Add a list value
        /// </summary>
        /// <param name="value">Object value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddListValue(object value)
        {
			if (value is GenericObject gObject)
				List.Add(gObject);
			else
				List.Add(new GenericObject { Value = value });
        }
        /// <summary>
        /// Set a dictionary key/value
        /// </summary>
        /// <param name="key">Dictionary key</param>
        /// <param name="value">Dictionary value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetDictionaryValue(object key, object value)
        {
			if (value is GenericObject gObject)
				Dictionary[key] = gObject;
			else
				Dictionary[key] = new GenericObject { Value = value };
        }
		#endregion
	}
}