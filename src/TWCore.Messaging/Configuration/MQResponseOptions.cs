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

using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TWCore.Messaging.Configuration
{
    /// <summary>
    /// Message queue response message options
    /// </summary>
    [DataContract]
    public class MQResponseOptions
    {
        /// <summary>
        /// Serializer mime type
        /// </summary>
        [XmlAttribute, DataMember]
        public string SerializerMimeType { get; set; }
        /// <summary>
        /// Compressor encoding type
        /// </summary>
        [XmlAttribute, DataMember]
        public string CompressorEncodingType { get; set; }
        /// <summary>
        /// Server sender options
        /// </summary>
        [XmlElement, DataMember]
        public MQServerSenderOptions ServerSenderOptions { get; set; } = new MQServerSenderOptions();
        /// <summary>
        /// Client receiver options
        /// </summary>
        [XmlElement, DataMember]
        public MQClientReceiverOptions ClientReceiverOptions { get; set; } = new MQClientReceiverOptions();
    }
}
