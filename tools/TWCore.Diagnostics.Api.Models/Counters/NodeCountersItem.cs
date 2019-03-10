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
using TWCore.Diagnostics.Counters;

namespace TWCore.Diagnostics.Api.Models.Counters
{
    [DataContract]
    public class NodeCountersItem
    {
        [XmlAttribute, DataMember]
        public string Id { get; set; }
        [XmlAttribute, DataMember]
        public string Environment { get; set; }
        [XmlAttribute, DataMember]
        public string Application { get; set; }
        [XmlAttribute, DataMember]
        public Guid CountersId { get; set; }
        [XmlAttribute, DataMember]
        public string Category { get; set; }
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        [XmlAttribute, DataMember]
        public CounterType Type { get; set; }
        [XmlAttribute, DataMember]
        public CounterLevel Level { get; set; }
        [XmlAttribute, DataMember]
        public CounterKind Kind { get; set; }
        [XmlAttribute, DataMember]
        public CounterUnit Unit { get; set; }
        [XmlAttribute, DataMember]
        public string TypeOfValue { get; set; }
    }

    [DataContract]
    public class NodeCountersValue
    {
        [XmlAttribute, DataMember]
        public string Id { get; set; }
        [XmlAttribute, DataMember]
        public Guid CountersId { get; set; }
        [XmlAttribute, DataMember]
        public DateTime Timestamp { get; set; }
        [XmlAttribute, DataMember]
        public object Value { get; set; }

    }


    [DataContract]
    public class NodeCountersQueryItem
    {
        [XmlAttribute, DataMember]
        public string Application { get; set; }
        [XmlAttribute, DataMember]
        public Guid CountersId { get; set; }
        [XmlAttribute, DataMember]
        public string Category { get; set; }
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        [XmlAttribute, DataMember]
        public CounterType Type { get; set; }
        [XmlAttribute, DataMember]
        public CounterLevel Level { get; set; }
        [XmlAttribute, DataMember]
        public CounterKind Kind { get; set; }
        [XmlAttribute, DataMember]
        public CounterUnit Unit { get; set; }
        [XmlAttribute, DataMember]
        public string TypeOfValue { get; set; }
    }
    [DataContract]
    public class NodeCountersQueryValue
    {
        [XmlAttribute, DataMember]
        public string Id { get; set; }
        [XmlAttribute, DataMember]
        public DateTime Timestamp { get; set; }
        [XmlAttribute, DataMember]
        public object Value { get; set; }
    }


    [DataContract]
    public class NodeLastCountersValue
    {
        [XmlAttribute, DataMember]
        public DateTime Timestamp { get; set; }
        [XmlAttribute, DataMember]
        public object Value { get; set; }
    }


    public enum CounterValuesDivision
    {
        QuarterDay,
        HalfDay,
        Day,
        Week,
        Month,
        TwoMonths,
        QuarterYear,
        HalfYear,
        Year
    }
}
