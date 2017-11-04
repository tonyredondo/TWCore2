using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Diagnostics.Log;

namespace TWCore.Diagnostics.Api.Models.Log
{
    [DataContract]
    public class NodeLogItem
    {
        [XmlAttribute, DataMember]
        public int Id { get; set; }
        [XmlAttribute, DataMember]
        public int NodeInfoId { get; set; }
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
        [XmlAttribute, DataMember]
        public DateTime Timestamp { get; set; }
    }
}