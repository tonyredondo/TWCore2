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
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TWCore.Net.RPC.Descriptors
{
    /// <summary>
    /// Describe an object type
    /// </summary>
    [DataContract]
    public class TypeDescriptor
    {
        /// <summary>
        /// Object type name
        /// </summary>
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Object type Qualified name
        /// </summary>
        [XmlAttribute, DataMember]
        public string FullName { get; set; }
        /// <summary>
        /// Object type public properties
        /// </summary>
        [XmlElement("Property"), DataMember]
        public List<PropertyDescriptor> Properties { get; set; } = new List<PropertyDescriptor>();
    }
    /// <summary>
    /// Type description collection
    /// </summary>
    [DataContract]
    public class TypeDescriptorCollection : KeyedCollection<string, TypeDescriptor>
    {
        /// <summary>
        /// Type description collection
        /// </summary>
        public TypeDescriptorCollection() : base(StringComparer.Ordinal) { }
        /// <summary>
        /// Gets the key of the item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override string GetKeyForItem(TypeDescriptor item) => item.Name;
    }
}
