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

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Diagnostics.Status;

namespace TWCore.Diagnostics.Api.Models.Trace
{
    [DataContract]
    public class NodeStatusChildValue
    {
        [XmlAttribute, DataMember]
        public string Id { get; set; }
        [XmlAttribute, DataMember]
        public string Key { get; set; }
        [XmlAttribute, DataMember]
        public string Value { get; set; }
        [DataMember]
        public List<NodeStatusChildValue> Values { get; set; }
        [XmlAttribute, DataMember]
        public StatusItemValueType Type { get; set; }
        [XmlAttribute, DataMember]
        public StatusItemValueStatus Status { get; set; }
    }
}
