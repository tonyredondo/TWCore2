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
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TWCore.Cache.Storages.IO
{
    /// <summary>
    /// File Storage Item Meta Log
    /// </summary>
    [DataContract, Serializable]
    public class FileStorageMetaLog
    {
        /// <summary>
        /// Transaction type
        /// </summary>
        [XmlAttribute, DataMember]
        public TransactionType Type { get; set; }
        /// <summary>
        /// Storage Item Meta data
        /// </summary>
        [DataMember]
        public StorageItemMeta Meta { get; set; }

        /// <summary>
        /// Log Transaction type
        /// </summary>
		[Serializable]
        public enum TransactionType
        {
            /// <summary>
            /// Add transaction
            /// </summary>
            Add,
            /// <summary>
            /// Remove transaction
            /// </summary>
            Remove
        }
    }
}
