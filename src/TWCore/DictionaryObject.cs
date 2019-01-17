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

using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
// ReSharper disable UnusedMember.Global

namespace TWCore
{
    /// <inheritdoc />
    /// <summary>
    /// Create a dynamic object and stores data on a dictionary
    /// </summary>
    public sealed class DictionaryObject : DynamicObject
    {
        #region Properties
        /// <summary>
        /// Object Dictionary
        /// </summary>
        private IDictionary<string, object> BaseDictionary { get; }

        /// <summary>
        /// Properties count
        /// </summary>
        public int PropertyCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => BaseDictionary.Count;
        }
        #endregion

        #region Indexer
        /// <summary>
        /// Get or Set a value for a property
        /// </summary>
        /// <param name="key">Property name</param>
        /// <returns>Value of the property</returns>
        public object this[string key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => BaseDictionary[key];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => BaseDictionary[key] = value;
        }
        #endregion

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Create a dynamic object and stores data on a dictionary
        /// </summary>
        /// <param name="dictionary">IDictionary source of data</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DictionaryObject(IDictionary<string, object> dictionary)
        {
            BaseDictionary = new Dictionary<string, object>(dictionary);
        }
        #endregion

        #region From XML
        /// <summary>
        /// Create a Dictionary Object from a Xml source
        /// </summary>
        /// <param name="xml">Xml content to parse</param>
        /// <returns>Dictionary object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DictionaryObject CreateFromXml(string xml)
        {
            var dictionary = new Dictionary<string, object>();
            var sr = new StringReader(xml);
            var doc = XDocument.Load(sr);
            Extract(doc.Root, dictionary);
            return new DictionaryObject(dictionary);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Extract(XElement element, Dictionary<string, object> currentDictionary)
        {
            if (element.HasAttributes)
            {
                foreach (var attr in element.Attributes())
                    currentDictionary[attr.Name.LocalName] = attr.Value;
            }
            if (element.HasElements)
            {
                foreach (var elm in element.Elements())
                {
                    if (!elm.HasElements && !elm.HasAttributes)
                    {
                        currentDictionary[elm.Name.LocalName] = elm.Value;
                    }
                    else
                    {
                        if (!currentDictionary.ContainsKey(elm.Name.LocalName))
                            currentDictionary[elm.Name.LocalName] = new List<Dictionary<string, object>>();

                        var lst = (List<Dictionary<string, object>>)currentDictionary[elm.Name.LocalName];
                        var nD = new Dictionary<string, object>();
                        lst.Add(nD);
                        Extract(elm, nD);
                    }
                };
                var keys = currentDictionary.Keys.ToList();
                foreach (var key in keys)
                {
                    if (!(currentDictionary[key] is List<Dictionary<string, object>> lst)) continue;
                    switch (lst.Count)
                    {
                        case 0:
                            currentDictionary[key] = null;
                            break;
                        case 1:
                            currentDictionary[key] = lst[0];
                            break;
                    }
                }

            }
        }
        #endregion

        #region Overrides
        /// <inheritdoc />
        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>A sequence that contains dynamic member names.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return BaseDictionary.Keys.ToList();
        }
        /// <inheritdoc />
        /// <summary>
        /// Provides the implementation for operations that get member values.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
        /// <param name="result">The result of the get operation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!BaseDictionary.TryGetValue(binder.Name, out result)) return false;

            if (result is Dictionary<string, object> dct)
            {
                result = new DictionaryObject(dct);
            }
            else
            {
                if (result is List<Dictionary<string, object>> list)
                {
                    result = list.Select(i => new DictionaryObject(i)).ToList();
                }
            }
            return true;
        }
        /// <inheritdoc />
        /// <summary>
        /// Provides the implementation for operations that set member values.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
        /// <param name="value">The value to set to the member.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var name = binder.Name;
            BaseDictionary[name] = value;
            return true;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the internal dictionary with all the properties names and values
        /// </summary>
        /// <returns>Internal dictionary with names and values</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDictionary<string, object> ToDictionary()
        {
            return BaseDictionary;
        }
        #endregion
    }
}
