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
#pragma warning disable IDE1006 // Estilos de nombres

using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Serialization;

namespace TWCore.Messaging
{
    /// <summary>
    /// Defines a new Response message for messaging
    /// </summary>
    [DataContract]
    public sealed class ResponseMessage : IMessage
    {
        /// <summary>
        /// Define a no response message
        /// </summary>
        public static object NoResponse { get; } = "TWCore.Messaging.ResponseMessage.NoResponse";

        /// <summary>
        /// Request identifier
        /// </summary>
        [XmlAttribute, DataMember]
        public Guid CorrelationId { get { return Header.Request.CorrelationId; } set { if (Header.Request != null) { Header.Request.CorrelationId = value; } } }
        /// <summary>
        /// Response headers
        /// </summary>
        [XmlElement, DataMember]
        public ResponseMessageHeader Header { get; set; }
        /// <summary>
        /// Total Request+Response Network time (Time between queue sent date and queue arrived date)
        /// </summary>
        [NonSerialize]
        public TimeSpan? _Total_NetworkTime { get { return Header.Response.QueueArrivedDate.HasValue && Header.Request.Header.QueueSentDate.HasValue ? Header.Response.QueueArrivedDate - Header.Request.Header.QueueSentDate : null; } }
        /// <summary>
        /// Message Request+Response Total Time
        /// </summary>
        [NonSerialize]
        public TimeSpan _Total_Time { get { return Header.Response.ApplicationReceivedTime - Header.Request.Header.ApplicationSentDate; } }
        /// <summary>
        /// Total Request+Response Network time (Time between queue sent date and queue arrived date)
        /// </summary>
        [XmlAttribute, NonSerialize]
        public string _Total_NetworkTimeString { get { return _Total_NetworkTime?.ToString(); } set { } }
        /// <summary>
        /// Message Request+Response Total Time
        /// </summary>
        [XmlAttribute, NonSerialize]
        public string _Total_TimeString { get { return _Total_Time.ToString(); } set { } }
        /// <summary>
        /// Response body
        /// </summary>
        [XmlElement, DataMember]
        public object Body { get; set; }

        #region .ctor
        /// <summary>
        /// Defines a new Response message for messaging
        /// </summary>
        public ResponseMessage() : this(null, null) { }
        /// <summary>
        /// Defines a new Response message for messaging
        /// </summary>
        /// <param name="request">Request message</param>
        /// <param name="body">Response body</param>
        public ResponseMessage(RequestMessage request, object body)
        {
            Header = new ResponseMessageHeader()
            {
                Request = request
            };
            Body = body;
        }
        #endregion
    }
}
#pragma warning restore IDE1006 // Estilos de nombres
