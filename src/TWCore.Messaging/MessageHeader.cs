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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
using TWCore.Serialization;

namespace TWCore.Messaging
{
    /// <summary>
    /// Message Header Base
    /// </summary>
    [DataContract]
    public class MessageHeader
    {
        /// <summary>
        /// Message Label
        /// </summary>
        [XmlAttribute, DataMember]
        public string Label { get; set; }
        /// <summary>
        /// Machine name where the message was generated
        /// </summary>
        [XmlAttribute, DataMember]
        public string MachineName { get; set; } = Core.MachineName;
        /// <summary>
        /// Application name where the message was generated
        /// </summary>
        [XmlAttribute, DataMember]
        public string ApplicationName { get; set; } = Core.ApplicationName;
        /// <summary>
        /// Environment name where the message was generated
        /// </summary>
        [XmlAttribute, DataMember]
        public string EnvironmentName { get; set; } = Core.EnvironmentName;
        /// <summary>
        /// Application sent datetime
        /// </summary>
        [XmlAttribute, DataMember]
        public DateTime ApplicationSentDate { get; set; }
        /// <summary>
        /// Application received time
        /// </summary>
        [XmlAttribute, DataMember]
        public DateTime ApplicationReceivedTime { get; set; }
        /// <summary>
        /// Message Total Time
        /// </summary>
        [NonSerialize]
        public TimeSpan TotalTime { get { return ApplicationReceivedTime - ApplicationSentDate; } }
        /// <summary>
        /// Aditional metadata
        /// </summary>
        [XmlArray("MetaData"), XmlArrayItem("Meta"), DataMember]
        public KeyValueCollection MetaData { get; set; } = new KeyValueCollection();
        /// <summary>
        /// Message Total Time
        /// </summary>
        [XmlAttribute, NonSerialize]
        public string TotalTimeString { get { return TotalTime.ToString(); } set { } }
    }
}