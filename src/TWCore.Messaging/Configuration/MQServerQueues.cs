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

namespace TWCore.Messaging.Configuration
{
    /// <summary>
    /// Message queue server queues
    /// </summary>
    [DataContract]
    public class MQServerQueues
    {
        /// <summary>
        /// Environment name
        /// </summary>
        [XmlAttribute, DataMember]
        public string EnvironmentName { get; set; }
        /// <summary>
        /// Machine name
        /// </summary>
        [XmlAttribute, DataMember]
        public string MachineName { get; set; }
        /// <summary>
        /// Message queue connections wich the request message will be read
        /// </summary>
        [XmlElement("RecvQueue"), DataMember]
        public List<MQConnection> RecvQueues { get; set; } = new List<MQConnection>();
        /// <summary>
        /// Additionals message queue connections where the response message will be sent
        /// </summary>
        [XmlElement("AdditionalSendQueue"), DataMember]
        public List<MQConnection> AdditionalSendQueues { get; set; } = new List<MQConnection>();
    }
}
