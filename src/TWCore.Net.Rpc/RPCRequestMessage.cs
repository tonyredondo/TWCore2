﻿/*
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

namespace TWCore.Net.RPC
{
    /// <summary>
    /// RPC Request Message
    /// </summary>
    [DataContract]
    public class RPCRequestMessage : RPCMessage
    {
        /// <summary>
        /// Service name
        /// </summary>
        [XmlAttribute, DataMember]
        public string ServiceName { get; set; }
        /// <summary>
        /// Method identifier
        /// </summary>
        [XmlAttribute, DataMember]
        public int MethodIndex { get; set; }
        /// <summary>
        /// Method identifier
        /// </summary>
        [XmlAttribute, DataMember]
        public string MethodId { get; set; }
        /// <summary>
        /// Method parameters
        /// </summary>
        [XmlElement("Parameter"), DataMember]
        public List<object> Parameters { get; set; }
    }
}
