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

namespace TWCore.Diagnostics.Api.Models
{
    [DataContract]
    public class PagedList<T>
    {
        [XmlAttribute, DataMember]
        public int PageNumber { get; set; }
        [XmlAttribute, DataMember]
        public int PageSize { get; set; }
        [XmlAttribute, DataMember]
        public int TotalResults { get; set; }
        [XmlAttribute, DataMember]
        public int TotalPages
        {
            get
            {
                if (PageSize == 0) return 0;
                return (int)Math.Ceiling((decimal)TotalResults / PageSize);
            }
            set { }
        }
        [XmlElement, DataMember]
        public List<T> Data { get; set; }
    }
}