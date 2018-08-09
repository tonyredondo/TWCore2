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
using System.Threading.Tasks;
using System.Xml.Serialization;
using TWCore.Serialization;
// ReSharper disable ValueParameterNotUsed
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Messaging
{
    /// <inheritdoc />
    /// <summary>
    /// Defines a new Response message for messaging
    /// </summary>
    [DataContract, Serializable]
    public sealed class ResponseMessage : IMessage
    {
        /// <summary>
        /// Define a no response message
        /// </summary>
        [NonSerialize]
        public static object NoResponse { get; } = "Message[NoResponse]";
        /// <summary>
        /// Define a no response message task
        /// </summary>
        [NonSerialize]
        public static Task<object> NoResponseTask { get; } = Task.FromResult(NoResponse);
        /// <summary>
        /// Define a no response message in serialized object format
        /// </summary>
        [NonSerialize]
        public static SerializedObject NoResponseSerialized { get; } = new SerializedObject(NoResponse);

        /// <inheritdoc />
        /// <summary>
        /// Request identifier
        /// </summary>
        [XmlAttribute, DataMember]
        public Guid CorrelationId
        {
            get => Header.Request.CorrelationId;
            set { if (Header.Request != null) { Header.Request.CorrelationId = value; } }
        }
        /// <summary>
        /// Response headers
        /// </summary>
        [XmlElement, DataMember]
        public ResponseMessageHeader Header { get; set; }
        /// <summary>
        /// Message Request+Response Total Time
        /// </summary>
        [NonSerialize]
        public TimeSpan TotalTime => Header.Response.ApplicationReceivedTime - Header.Request.Header.ApplicationSentDate;
        /// <summary>
        /// Message Request+Response Total Time
        /// </summary>
        [XmlAttribute, NonSerialize]
        public string TotalTimeString { get { return TotalTime.ToString(); } set { } }
        /// <inheritdoc />
        /// <summary>
        /// Response body
        /// </summary>
        [XmlElement, DataMember]
        public SerializedObject Body { get; set; }

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Defines a new Response message for messaging
        /// </summary>
        public ResponseMessage() : this(null, null) { }
        /// <summary>
        /// Defines a new Response message for messaging
        /// </summary>
        /// <param name="request">Request message</param>
        /// <param name="body">Response body</param>
        public ResponseMessage(RequestMessage request, SerializedObject body)
        {
            Header = new ResponseMessageHeader
            {
                Request = request
            };
            Body = body;
        }
        #endregion
    }
}
