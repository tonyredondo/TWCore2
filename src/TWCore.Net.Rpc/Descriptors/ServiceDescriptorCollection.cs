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
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TWCore.Net.RPC.Descriptors
{
    /// <summary>
    /// Defines a RPC Service
    /// </summary>
    [DataContract]
    public class ServiceDescriptorCollection
    {
        /// <summary>
        /// Service descriptors list
        /// </summary>
        [XmlElement("Descriptor"), DataMember]
        public ServiceDescriptorKeyedCollection Items { get; set; } = new ServiceDescriptorKeyedCollection();
    }
    /// <summary>
    /// Service descriptor keyed collection byte name
    /// </summary>
    [DataContract]
    public class ServiceDescriptorKeyedCollection : KeyedCollection<string, ServiceDescriptor>
    {
        /// <summary>
        /// Service descriptor keyed collection byte name
        /// </summary>
        public ServiceDescriptorKeyedCollection() : base(StringComparer.Ordinal) { }
        /// <summary>
        /// Gets the Service Descriptor Key
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Key</returns>
        protected override string GetKeyForItem(ServiceDescriptor item) => item.Name;
    }
}
