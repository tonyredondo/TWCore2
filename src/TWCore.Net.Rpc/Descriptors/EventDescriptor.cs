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
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TWCore.Net.RPC.Descriptors
{
    /// <summary>
    /// Describe a event definition of an object
    /// </summary>
    [Serializable, DataContract]
    public class EventDescriptor
    {
        /// <summary>
        /// Property name
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Property type
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Reflected event info object for direct access.
        /// </summary>
        [XmlIgnore, NonSerialize]
        public EventInfo Event { get; internal set; }
    }
}
