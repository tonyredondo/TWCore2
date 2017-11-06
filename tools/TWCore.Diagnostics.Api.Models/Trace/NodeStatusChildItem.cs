using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TWCore.Diagnostics.Api.Models.Trace
{
    [DataContract]
    public class NodeStatusChildItem
    {
        [XmlAttribute, DataMember]
        public string Id { get; set; }
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        [XmlElement("Value"), DataMember]
        public List<NodeStatusChildValue> Values { get; set; } = new List<NodeStatusChildValue>();
        [XmlElement("Child"), DataMember]
        public List<NodeStatusChildItem> Children { get; set; } = new List<NodeStatusChildItem>();
    }
}
