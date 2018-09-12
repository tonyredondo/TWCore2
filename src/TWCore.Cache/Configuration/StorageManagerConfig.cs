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
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Configuration;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace TWCore.Cache.Configuration
{
    /// <summary>
    /// Storage manager configuration
    /// </summary>
    [DataContract]
    public class StorageManagerConfig
    {
        /// <summary>
        /// Expiration check time in minutes
        /// </summary>
        [XmlAttribute, DataMember]
        public string ExpirationCheckTimeInMinutes { get; set; }
        /// <summary>
        /// Maximum duration in minutes by each item
        /// </summary>
        [XmlAttribute, DataMember]
        public string MaxItemDurationInMinutes { get; set; }
        /// <summary>
        /// Overwrite of the Item expiration in Date format
        /// </summary>
        [XmlAttribute, DataMember]
        public string ItemsExpirationAbsoluteDateOverwrite { get; set; }
        /// <summary>
        /// Item expiration date overwrite in minutes
        /// </summary>
        [XmlAttribute, DataMember]
        public string ItemsExpirationDateOverwriteInMinutes { get; set; }
        /// <summary>
        /// Storages configuration
        /// </summary>
        [XmlElement("Storage"), DataMember]
        public List<BasicConfigurationItem> Storages { get; set; } = new List<BasicConfigurationItem>();

        /// <summary>
        /// Get the storage manager from the configuration
        /// </summary>
        /// <returns>Storage manager instance</returns>
        public StorageManager GetStorageManager()
        {
            var stoMng = new StorageManager();

            var expirationCheckTimeInMinutes = ExpirationCheckTimeInMinutes.ParseTo(-1);
            var maxItemDurationInMinutes = MaxItemDurationInMinutes.ParseTo(-1);
            var itemsExpirationDateOverwriteInMinutes = ItemsExpirationDateOverwriteInMinutes.ParseTo(-1);
            var itemsExpirationAbsoluteDateOverwrite = ItemsExpirationAbsoluteDateOverwrite.ParseTo<DateTime?>(null, "dd/MM/YYYY");

            if (expirationCheckTimeInMinutes > 0)
                stoMng.ExpirationCheckTimeInMinutes = expirationCheckTimeInMinutes;
            if (maxItemDurationInMinutes > -1)
                stoMng.MaximumItemDuration = TimeSpan.FromMinutes(maxItemDurationInMinutes);
            if (itemsExpirationDateOverwriteInMinutes > 0)
                stoMng.ItemsExpirationDateOverwrite = TimeSpan.FromMinutes(itemsExpirationDateOverwriteInMinutes);
            if (itemsExpirationAbsoluteDateOverwrite.HasValue)
                stoMng.ItemsExpirationAbsoluteDateOverwrite = itemsExpirationAbsoluteDateOverwrite;
            
            if (Storages?.Any() != true) return stoMng;
            foreach(var stoconfig in Storages)
            {
                var sto = stoconfig.CreateInstance<StorageBase>();
                if (sto is null) continue;
                sto.ExpirationCheckTimeInMinutes = stoMng.ExpirationCheckTimeInMinutes;
                sto.MaximumItemDuration = stoMng.MaximumItemDuration;
                sto.ItemsExpirationDateOverwrite = stoMng.ItemsExpirationDateOverwrite;
                sto.ItemsExpirationAbsoluteDateOverwrite = stoMng.ItemsExpirationAbsoluteDateOverwrite;
                sto.Init();
                stoMng.Push(sto);
            }
            return stoMng;
        }
    }
}
