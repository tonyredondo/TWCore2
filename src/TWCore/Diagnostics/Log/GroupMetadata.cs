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
using TWCore.Collections;

namespace TWCore.Diagnostics.Log
{
    /// <summary>
    /// Group metadata
    /// </summary>
    [XmlRoot("GroupMetadata")]
    public class GroupMetadata : IGroupMetadata
	{
        /// <summary>
        /// Instance id
        /// </summary>
        [XmlAttribute, DataMember]
        public Guid InstanceId { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Item timestamp
        /// </summary>
        [XmlAttribute, DataMember]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Gets the name of the group.
        /// </summary>
        /// <value>The name of the group.</value>
        [XmlAttribute, DataMember]
        public string GroupName { get; set; }
		/// <summary>
		/// Gets the Metadata Items
		/// </summary>
		/// <value>The metadata items</value>
        [XmlArray("Items"), XmlArrayItem("Item"), DataMember]
		public KeyValue[] Items { get; set; }
	}
}
