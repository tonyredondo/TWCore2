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
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore.Diagnostics.Log
{
    /// <inheritdoc />
    /// <summary>
    /// Log item
    /// </summary>
    [IgnoreStackFrameLog]
    [DataContract]
    public class LogItem : ILogItem
    {
        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Instance identifier
        /// </summary>
        [XmlAttribute, DataMember]
        public Guid InstanceId { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Item unique identifier
        /// </summary>
        [XmlAttribute, DataMember]
        public Guid Id { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Environment name
        /// </summary>
        [XmlAttribute, DataMember]
        public string EnvironmentName { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Machine name
        /// </summary>
        [XmlAttribute, DataMember]
        public string MachineName { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Application name
        /// </summary>
        [XmlAttribute, DataMember]
        public string ApplicationName { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Process name
        /// </summary>
        [XmlAttribute, DataMember]
        public string ProcessName { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Process Id
        /// </summary>
        [XmlAttribute, DataMember]
        public int ProcessId { get; set; } = -1;
        /// <inheritdoc />
        /// <summary>
        /// Assembly name
        /// </summary>
        [XmlAttribute, DataMember]
        public string AssemblyName { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Type name
        /// </summary>
        [XmlAttribute, DataMember]
        public string TypeName { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Nivel de log
        /// </summary>
        [XmlAttribute, DataMember]
        public LogLevel Level { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Message code
        /// </summary>
        [XmlAttribute, DataMember]
        public string Code { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Message
        /// </summary>
        [XmlAttribute, DataMember]
        public string Message { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Item timestamp
        /// </summary>
        [XmlAttribute, DataMember]
        public DateTime Timestamp { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Message group name
        /// </summary>
        [XmlAttribute, DataMember]
        public string GroupName { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// If is an error log item, the exception object instance
        /// </summary>
        [XmlElement, DataMember]
        public SerializableException Exception { get; set; }
        #endregion

        #region Static Methods
        /// <summary>
        /// Retrieve LogItem instance
        /// </summary>
        /// <returns>LogItem instance</returns>
        public static LogItem Retrieve()
            => ReferencePool<LogItem>.Shared.New();
        /// <summary>
        /// Store LogItem instance
        /// </summary>
        /// <param name="value">LogItem instance</param>
        public static void Store(LogItem value)
            => ReferencePool<LogItem>.Shared.Store(value);
        #endregion
    }

    /// <inheritdoc />
    /// <summary>
    /// NewLine Log item
    /// </summary>
    [IgnoreStackFrameLog]
    [DataContract]
    public class NewLineLogItem : LogItem { }
}
