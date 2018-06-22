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
using TWCore.Diagnostics.Log;

namespace TWCore.Diagnostics.Api.Models.Log
{
    [DataContract]
    public class LogSummary
    {
        [DataMember]
        public ApplicationsLevels[] Applications { get; set; }
        [DataMember]
        public LogLevelTimes[] Levels { get; set; }
    }



    [DataContract]
    public class ApplicationsLevels
    {
        [XmlAttribute, DataMember]
        public string Application { get; set; }
        [DataMember]
        public LogLevelQuantity[] Levels { get; set; }
    }
    [DataContract]
    public class LogLevelQuantity
    {
        [XmlAttribute, DataMember]
        public LogLevel Name { get; set; }
        [XmlAttribute, DataMember]
        public int Count { get; set; }
    }



    [DataContract]
    public class LogLevelTimes
    {
        [XmlAttribute, DataMember]
        public LogLevel Name { get; set; }
        [XmlAttribute, DataMember]
        public int Count { get; set; }
        [DataMember]
        public TimeCount[] Series { get; set; }
    }
    [DataContract]
    public class TimeCount
    {
        [XmlAttribute, DataMember]
        public DateTime Date { get; set; }
        [XmlAttribute, DataMember]
        public int Count { get; set; }
    }
}