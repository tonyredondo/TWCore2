using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TWCore.Diagnostics.Api.Models
{
    [DataContract]
    public class BasicInfo
    {
        [XmlAttribute, DataMember]
        public string Environment { get; set; }
        [XmlAttribute, DataMember]
        public string Machine { get; set; }
        [XmlAttribute, DataMember]
        public string Application { get; set; }
    }
}