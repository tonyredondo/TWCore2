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
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Serialization;

namespace TWCore.Net.RPC.Descriptors
{
    /// <summary>
    /// Describe a event definition of an object
    /// </summary>
    [DataContract]
    public class EventDescriptor
    {
        /// <summary>
        /// Property name
        /// </summary>
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Property type
        /// </summary>
        [XmlAttribute, DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Reflected event info object for direct access.
        /// </summary>
        [XmlIgnore, NonSerialize]
        public EventInfo Event { get; internal set; }
    }
    /// <summary>
    /// Event description collection
    /// </summary>
    [DataContract]
    public class EventDescriptorCollection : KeyedCollection<string, EventDescriptor>
    {
        /// <summary>
        /// Event description collection
        /// </summary>
        public EventDescriptorCollection() : base(StringComparer.Ordinal) { }
        /// <summary>
        /// Gets the key of the item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override string GetKeyForItem(EventDescriptor item) => item.Name;
    }
}
