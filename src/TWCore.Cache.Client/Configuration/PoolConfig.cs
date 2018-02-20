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

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace TWCore.Cache.Client.Configuration
{
    /// <summary>
    /// Pool configuration
    /// </summary>
    [DataContract]
    public class PoolConfig
    {
        /// <summary>
        /// Serializer for the data object in bytes
        /// </summary>
        [XmlAttribute, DataMember]
        public string SerializerMimeType { get; set; }
        /// <summary>
        /// Serializer compressor encoding for the data object in bytes
        /// </summary>
        [XmlAttribute, DataMember]
        public string CompressorEncoding { get; set; }
        /// <summary>
        /// Delays between ping tries in milliseconds
        /// </summary>
        [XmlAttribute, DataMember]
        public string PingDelay { get; set; }
        /// <summary>
        /// Delay after a ping error for next try
        /// </summary>
        [XmlAttribute, DataMember]
        public string PingDelayOnError { get; set; }
        /// <summary>
        /// Cache pool Read Mode
        /// </summary>
        [XmlAttribute, DataMember]
        public PoolReadMode ReadMode { get; set; }
        /// <summary>
        /// Cache pool Write Mode
        /// </summary>
        [XmlAttribute, DataMember]
        public PoolWriteMode WriteMode { get; set; }
        /// <summary>
        /// Force at least one network item enabled
        /// </summary>
        [XmlAttribute, DataMember]
        public bool ForceAtLeastOneNetworkItemEnabled { get; set; } = true;
        /// <summary>
        /// Pool item selection order for Read and Write
        /// </summary>
        [XmlAttribute, DataMember]
        public PoolOrder SelectionOrder { get; set; } = PoolOrder.PingTime;
        /// <summary>
        /// Custom index order
        /// </summary>
        [XmlAttribute, DataMember]
        public string IndexOrder { get; set; } = null;
        /// <summary>
        /// Cache pool items collection
        /// </summary>
        [XmlElement("PoolItem"), DataMember]
        public List<PoolItemConfig> Items { get; set; } = new List<PoolItemConfig>();
    }
}
