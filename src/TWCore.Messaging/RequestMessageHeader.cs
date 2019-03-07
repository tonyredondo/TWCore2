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
using TWCore.Messaging.Configuration;

namespace TWCore.Messaging
{
    /// <inheritdoc />
    /// <summary>
    /// Message Request Header
    /// </summary>
    [DataContract, Serializable]
    public sealed class RequestMessageHeader : MessageHeader
    {
        /// <summary>
        /// Request identifier
        /// </summary>
        [XmlAttribute, DataMember]
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Client name
        /// </summary>
        [XmlAttribute, DataMember]
        public string ClientName { get; set; }
        /// <summary>
        /// Context Group Name
        /// </summary>
        [XmlAttribute, DataMember]
        public string ContextGroupName { get; set; }
        /// <summary>
        /// Response queue
        /// </summary>
        [XmlElement, DataMember]
        public MQConnection ResponseQueue { get; set; }
        /// <summary>
        /// Gets of sets if a response message is expected for this request
        /// </summary>
        [XmlAttribute, DataMember]
        public bool ResponseExpected { get; set; } = true;
        /// <summary>
        /// Gets of sets the timeout in seconds of the sender to wait for the response
        /// </summary>
        [XmlAttribute, DataMember]
        public int ResponseTimeoutInSeconds { get; set; } = -1;
    }
}
