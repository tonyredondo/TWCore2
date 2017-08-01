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


using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;

namespace TWCore.Cache.Client.Configuration
{
    /// <summary>
    /// Cache configuration (client part)
    /// </summary>
    [DataContract]
    public class CacheConfiguration : INameItem
    {
        /// <summary>
        /// Cache name
        /// </summary>
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Use Shared Memory on local
        /// </summary>
        [XmlAttribute, DataMember]
        public bool UseSharedMemoryOnLocal { get; set; }
        /// <summary>
        /// Client options list by environment name
        /// </summary>
        [XmlElement("ClientOptions"), DataMember]
        public List<ClientOptions> ClientOptionsList { get; set; } = new List<ClientOptions>();
    }
}
