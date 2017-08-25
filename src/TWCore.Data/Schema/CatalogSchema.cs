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
using System.Xml.Serialization;

namespace TWCore.Data.Schema
{
    /// <summary>
    /// Catalog Schema
    /// </summary>
    [DataContract]
    public class CatalogSchema
    {
        /// <summary>
        /// Catalog Name
        /// </summary>
        [DataMember, XmlAttribute]
        public string Name { get; set; }
        /// <summary>
        /// Provider Name
        /// </summary>
        [DataMember, XmlAttribute]
        public string Provider { get; set; }
        /// <summary>
        /// Assembly Name
        /// </summary>
        [DataMember, XmlAttribute]
        public string Assembly { get; set; }
        /// <summary>
        /// AssemblyQualified Name
        /// </summary>
        [DataMember, XmlAttribute]
        public string AssemblyQualifiedName { get; set; }
        /// <summary>
        /// Connection String
        /// </summary>
        [DataMember]
        public string ConnectionString { get; set; }
        /// <summary>
        /// Tables Schemas
        /// </summary>
        [DataMember, XmlArray("Tables"), XmlArrayItem("Table")]
        public List<TableSchema> Tables { get; set; } = new List<TableSchema>();
    }
}
