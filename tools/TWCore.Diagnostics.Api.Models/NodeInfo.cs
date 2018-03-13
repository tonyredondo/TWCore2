﻿/*
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

namespace TWCore.Diagnostics.Api.Models
{
    [DataContract]
    public class NodeInfo
    {
        [XmlAttribute, DataMember]
        public string Id { get; set; }
        [XmlAttribute, DataMember]
        public string Machine { get; set; }
        [XmlAttribute, DataMember]
        public string Environment { get; set; }
        [XmlAttribute, DataMember]
        public string Application { get; set; }
        [XmlAttribute, DataMember]
        public DateTime Date { get; set; } 
    }
}