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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TWCore.Diagnostics.Status
{
    /// <summary>
    /// Status item collection
    /// </summary>
    [XmlRoot("StatusItems"), DataContract]
    public class StatusItemCollection
    {
        /// <summary>
        /// Environment Name
        /// </summary>
        [XmlAttribute, DataMember]
        public string EnvironmentName { get; set; }
        /// <summary>
        /// Machine Name
        /// </summary>
        [XmlAttribute, DataMember]
        public string MachineName { get; set; }
        /// <summary>
        /// Application Display Name
        /// </summary>
        [XmlAttribute, DataMember]
        public string ApplicationDisplayName { get; set; }
        /// <summary>
        /// Application Name
        /// </summary>
        [XmlAttribute, DataMember]
        public string ApplicationName { get; set; }
        /// <summary>
        /// Status item collection datetime
        /// </summary>
        [XmlAttribute, DataMember]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Milliseconds to process all the status result
        /// </summary>
        [XmlAttribute, DataMember]
        public double ElapsedMilliseconds { get; set; }
        /// <summary>
        /// App start time
        /// </summary>
        [XmlAttribute, DataMember]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// Status items
        /// </summary>
        [XmlElement("StatusItem"), DataMember]
        public List<StatusItem> Items { get; set; } = new List<StatusItem>();
    }
}
