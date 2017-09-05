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

using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.Configuration
{
    /// <summary>
    /// Message queue server sender options
    /// </summary>
    [DataContract]
    public class MQServerSenderOptions
    {
        /// <summary>
        /// Message expiration time in seconds
        /// </summary>
        [XmlAttribute, DataMember]
        public int MessageExpirationInSec { get; set; } = 30;
        /// <summary>
        /// Message priority
        /// </summary>
        [XmlAttribute, DataMember]
        public MQMessagePriority MessagePriority { get; set; } = MQMessagePriority.Normal;
        /// <summary>
        /// Set the message as recoverable in case of server reset.
        /// </summary>
        [XmlAttribute, DataMember]
        public bool Recoverable { get; set; } = true;
        /// <summary>
        /// Message label
        /// </summary>
        [XmlAttribute, DataMember]
        public string Label { get; set; }
        /// <summary>
        /// Additional parameters
        /// </summary>
        [XmlElement("Param"), DataMember]
        public KeyValueCollection Parameters { get; set; }
    }
}

