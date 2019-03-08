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
using TWCore.Diagnostics.Log;

namespace TWCore.Diagnostics.Api.Models.Log
{
    [DataContract]
	public class NodeLogItem : NodeInfo
    {
		[XmlAttribute, DataMember]
		public Guid LogId { get; set; }
        [XmlAttribute, DataMember]
        public string Assembly { get; set; }
        [XmlAttribute, DataMember]
        public string Type { get; set; }
        [XmlAttribute, DataMember]
        public string Group { get; set; }
        [XmlAttribute, DataMember]
        public string Code { get; set; }
        [XmlAttribute, DataMember]
        public LogLevel Level { get; set; }
        [XmlAttribute, DataMember]
        public string Message { get; set; }
        [XmlElement, DataMember]
        public SerializableException Exception { get; set; }
    }
}