/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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

namespace TWCore.Data.Schema
{
    /// <summary>
    /// Table Column Schema
    /// </summary>
    [DataContract]
    public class TableColumnSchema
    {
        /// <summary>
        /// Column Name
        /// </summary>
        [DataMember, XmlAttribute]
        public string Name { get; set; }
        /// <summary>
        /// Position
        /// </summary>
        [DataMember, XmlAttribute]
        public int Position { get; set; }
        /// <summary>
        /// Data Type
        /// </summary>
        [DataMember, XmlAttribute]
        public string DataType { get; set; }
        /// <summary>
        /// Is Nullable
        /// </summary>
        [DataMember, XmlAttribute]
        public bool IsNullable { get; set; }
        /// <summary>
        /// Maximum length
        /// </summary>
        [DataMember]
        public int? MaxLength { get; set; }
        /// <summary>
        /// Numeric Precision
        /// </summary>
        [DataMember]
        public int? NumericPrecision { get; set; }
        /// <summary>
        /// Numeric Precision Radix
        /// </summary>
        [DataMember]
        public int? NumericPrecisionRadix { get; set; }
        /// <summary>
        /// Indexes Names
        /// </summary>
        [DataMember, XmlArray("Indexes"), XmlArrayItem("Name")]
        public List<string> IndexesName { get; set; } = new List<string>();
    }
}
