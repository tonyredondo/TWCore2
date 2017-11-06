using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace TWCore.Diagnostics.Api.Models.Status
{
    [DataContract]
    public class NodeTraceItem
    {
        [XmlAttribute, DataMember]
        public string Id { get; set; }
        [XmlAttribute, DataMember]
        public string NodeInfoId { get; set; }
        [XmlAttribute, DataMember]
        public Guid TraceId { get; set; }
        [XmlAttribute, DataMember]
        public string Group { get; set; }
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        [XmlIgnore]
        [IgnoreParameter]
        public object TraceObject { get; set; }
        [XmlAttribute, DataMember]
        public DateTime Timestamp { get; set; }
    }
}
