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
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TWCore.Diagnostics.Trace
{
    /// <summary>
    /// Trace item
    /// </summary>
    [IgnoreStackFrameLog]
    [DataContract]
    public class TraceItem
    {
        /// <summary>
        /// Instance Identifier
        /// </summary>
        [XmlAttribute, DataMember]
        public Guid InstanceId { get; set; }
        /// <summary>
        /// Item unique identifier
        /// </summary>
        [XmlAttribute, DataMember]
        public Guid Id { get; set; }
        /// <summary>
        /// Ids Tags
        /// </summary>
        [XmlElement, DataMember]
        public Guid[] IdsTags { get; set; }
        /// <summary>
        /// Trace group name
        /// </summary>
        [XmlAttribute, DataMember]
        public string GroupName { get; set; }
        /// <summary>
        /// Trace Name
        /// </summary>
        [XmlAttribute, DataMember]
        public string TraceName { get; set; }
        /// <summary>
        /// Trace Object
        /// </summary>
        [XmlIgnore]
        [IgnoreParameter]
        public object TraceObject { get; set; }
        /// <summary>
        /// Item timestamp
        /// </summary>
        [XmlAttribute, DataMember]
        public DateTime Timestamp { get; set; }
    }
}
