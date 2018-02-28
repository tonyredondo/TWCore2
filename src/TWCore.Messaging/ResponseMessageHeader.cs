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

namespace TWCore.Messaging
{
    /// <summary>
    /// Message Response Header
    /// </summary>
    [DataContract, Serializable]
    public sealed class ResponseMessageHeader
    {
        /// <summary>
        /// Response Header
        /// </summary>
        [XmlElement, DataMember]
        public MessageHeader Response { get; set; } = new MessageHeader();
        /// <summary>
        /// Request Header
        /// </summary>
        [XmlElement, DataMember]
        public RequestMessage Request { get; set; } = new RequestMessage();
    }
}
