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

using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TWCore.Diagnostics.Log
{
    /// <summary>
    /// Log item
    /// </summary>
    [IgnoreStackFrameLog]
    [DataContract]
    public class LogItem : ILogItem
    {
        /// <summary>
        /// Item unique identifier
        /// </summary>
        [XmlAttribute, DataMember]
        public Guid Id { get; set; }
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
        /// Application name
        /// </summary>
        [XmlAttribute, DataMember]
        public string ApplicationName { get; set; }
        /// <summary>
        /// Process name
        /// </summary>
        [XmlAttribute, DataMember]
        public string ProcessName { get; set; }
        /// <summary>
        /// Process Id
        /// </summary>
        [XmlAttribute, DataMember]
        public int ProcessId { get; set; } = -1;
        /// <summary>
        /// Thread Id
        /// </summary>
        [XmlAttribute, DataMember]
        public int ThreadId { get; set; } = -1;
        /// <summary>
        /// Assembly name
        /// </summary>
        [XmlAttribute, DataMember]
        public string AssemblyName { get; set; }
        /// <summary>
        /// Type name
        /// </summary>
        [XmlAttribute, DataMember]
        public string TypeName { get; set; }
        /// <summary>
        /// Line number
        /// </summary>
        [XmlAttribute, DataMember]
        public int LineNumber { get; set; } = -1;
        /// <summary>
        /// Nivel de log
        /// </summary>
        [XmlAttribute, DataMember]
        public LogLevel Level { get; set; }
        /// <summary>
        /// Message code
        /// </summary>
        [XmlAttribute, DataMember]
        public string Code { get; set; }
        /// <summary>
        /// Message
        /// </summary>
        [XmlAttribute, DataMember]
        public string Message { get; set; }
        /// <summary>
        /// Item timestamp
        /// </summary>
        [XmlAttribute, DataMember]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Message group name
        /// </summary>
        [XmlAttribute, DataMember]
        public string GroupName { get; set; }
        /// <summary>
        /// If is an error log item, the exception object instance
        /// </summary>
        [XmlElement, DataMember]
        public SerializableException Exception { get; set; }
    }


    /// <summary>
    /// NewLine Log item
    /// </summary>
    [IgnoreStackFrameLog]
    [DataContract]
    public class NewLineLogItem : LogItem { }
}
