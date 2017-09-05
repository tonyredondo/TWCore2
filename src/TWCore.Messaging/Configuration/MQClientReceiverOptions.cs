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
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore.Messaging.Configuration
{
    /// <summary>
    /// Message queue client receiver options
    /// </summary>
    [DataContract]
    public class MQClientReceiverOptions
    {
        /// <summary>
        /// Receive timeout in seconds
        /// </summary>
        [XmlAttribute, DataMember]
        public int TimeoutInSec { get; set; } = 60;
        /// <summary>
        /// Additional parameters
        /// </summary>
        [XmlElement("Param"), DataMember]
        public KeyValueCollection Parameters { get; set; }


        /// <summary>
        /// Message queue client receiver options
        /// </summary>
        public MQClientReceiverOptions() { }
        /// <summary>
        /// Message queue client receiver options
        /// </summary>
        /// <param name="timeoutInSec">Receive timeout in seconds</param>
        /// <param name="parameters">Additional parameters</param>
        public MQClientReceiverOptions(int timeoutInSec, params KeyValue<string, string>[] parameters)
        {
            TimeoutInSec = timeoutInSec;
            Parameters = new KeyValueCollection(parameters);
        }

    }
}
