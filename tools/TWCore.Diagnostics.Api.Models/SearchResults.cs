using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Diagnostics.Api.Models.Log;
using TWCore.Diagnostics.Api.Models.Trace;

namespace TWCore.Diagnostics.Api.Models
{
    [DataContract]
    public class SearchResults
    {
        [XmlElement, DataMember]
        public List<NodeInfo> Data { get; set; }
    }
}