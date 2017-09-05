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
using TWCore.Cache.Configuration;
using TWCore.Configuration;
// ReSharper disable CheckNamespace

namespace TWCore.Services.Configuration
{
    /// <summary>
    /// Server Options
    /// </summary>
    [DataContract]
    public class ServerOptions
    {
        /// <summary>
        /// Environment Name
        /// </summary>
        [XmlAttribute, DataMember]
        public string EnvironmentName { get; set; }
        /// <summary>
        /// Machine name
        /// </summary>
        [XmlAttribute, DataMember]
        public string MachineName { get; set; }
        /// <summary>
        /// Storage stack
        /// </summary>
        [XmlElement, DataMember]
        public StorageManagerConfig StorageStack { get; set; }
        /// <summary>
        /// Server transport
        /// </summary>
        [XmlElement("Transport"), DataMember]
        public List<BasicConfigurationItem> Transports { get; set; } = new List<BasicConfigurationItem>();
    }
}
