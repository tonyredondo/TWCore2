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
