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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace TWCore.Data.Schema
{
    /// <summary>
    /// Table Schemas
    /// </summary>
    [DataContract]
    public class TableSchema
    {
        /// <summary>
        /// Table Name
        /// </summary>
        [DataMember, XmlAttribute]
        public string Name { get; set; }
        /// <summary>
        /// Table Schema
        /// </summary>
        [DataMember, XmlAttribute]
        public string Schema { get; set; }
        /// <summary>
        /// Table Type
        /// </summary>
        [DataMember, XmlAttribute]
        public TableType Type { get; set; }
        /// <summary>
        /// Columns Schema
        /// </summary>
        [DataMember, XmlArray("Columns"), XmlArrayItem("Column")]
        public List<TableColumnSchema> Columns { get; set; } = new List<TableColumnSchema>();
        /// <summary>
        /// Indexes Schema
        /// </summary>
        [DataMember, XmlArray("Indexes"), XmlArrayItem("Index")]
        public List<TableIndexSchema> Indexes { get; set; } = new List<TableIndexSchema>();
        /// <summary>
        /// ForeignKeys Schema
        /// </summary>
        [DataMember, XmlArray("ForeignKeys"), XmlArrayItem("ForeignKey")]
        public List<ForeignKeySchema> ForeignKeys { get; set; } = new List<ForeignKeySchema>();
    }
}
