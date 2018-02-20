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

using System.Xml.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Net
{
    /// <summary>
    /// Serializable http error
    /// </summary>
    [XmlRoot("Error")]
    public class SerializableHttpError
    {
        /// <summary>
        /// Exception message
        /// </summary>
        [XmlAttribute]
        public string ExceptionMessage { get; set; }
        /// <summary>
        /// Exception type
        /// </summary>
        [XmlAttribute]
        public string ExceptionType { get; set; }
        /// <summary>
        /// Message
        /// </summary>
        [XmlAttribute]
        public string Message { get; set; }
        /// <summary>
        /// Message details
        /// </summary>
        [XmlAttribute]
        public string MessageDetail { get; set; }
        /// <summary>
        /// Stacktrace of the exception
        /// </summary>
        [XmlAttribute]
        public string StackTrace { get; set; }
        /// <summary>
        /// Inner exception
        /// </summary>
        public SerializableHttpError InnerException { get; set; }
    }
}
