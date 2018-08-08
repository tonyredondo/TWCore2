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
using TWCore.Serialization;

namespace TWCore.Messaging
{
    /// <inheritdoc />
    /// <summary>
    /// Defines a new Request Message for messaging
    /// </summary>
    [DataContract, Serializable]
    public sealed class RequestMessage : IMessage
    {
        /// <inheritdoc />
        /// <summary>
        /// Request Id
        /// </summary>
        [XmlAttribute, DataMember]
        public Guid CorrelationId
        {
            get => Header.CorrelationId;
            set => Header.CorrelationId = value;
        }
        /// <summary>
        /// Request message header
        /// </summary>
        [XmlElement, DataMember]
        public RequestMessageHeader Header { get; set; } = new RequestMessageHeader();
        /// <inheritdoc />
        /// <summary>
        /// Request body
        /// </summary>
        [XmlElement, DataMember]
        public SerializedObject Body { get; set; }

        #region .ctor
        /// <summary>
        /// Defines a new Request Message for messaging
        /// </summary>
        public RequestMessage() { }
        /// <summary>
        /// Defines a new Request Message for messaging
        /// </summary>
        /// <param name="body">Request body</param>
        public RequestMessage(object body)
        {
            Body = new SerializedObject(body);
        }
        #endregion
    }
}
