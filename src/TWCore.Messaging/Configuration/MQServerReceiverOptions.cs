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

using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.Configuration
{
    /// <summary>
    /// Message queue server receiver options
    /// </summary>
    [DataContract]
    public class MQServerReceiverOptions
    {
        /// <summary>
        /// Waiting time on processing task before finalize in seconds
        /// </summary>
        [XmlAttribute, DataMember]
        public int ProcessingWaitOnFinalizeInSec { get; set; } = 5;
        /// <summary>
        /// Maximum number of message to be processed simultaneously
        /// </summary>
        [XmlAttribute, DataMember]
        public int MaxSimultaneousMessagesPerQueue { get; set; } = 50;
        /// <summary>
        /// Sleep time when an exceptions occurs
        /// </summary>
        [XmlAttribute, DataMember]
        public int SleepOnExceptionInSec { get; set; } = 5;
        /// <summary>
        /// Additional parameters
        /// </summary>
        [XmlElement("Param"), DataMember]
        public KeyValueCollection Parameters { get; set; }
    }
}
