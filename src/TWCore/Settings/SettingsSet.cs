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

using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Settings
{
    /// <summary>
    /// Application settings set
    /// </summary>
    [DataContract]
    public class SettingsSet
    {
        /// <summary>
        /// Settings items
        /// </summary>
        [XmlArray("Items"), XmlArrayItem("Item"), DataMember]
        public KeyValueCollection Items { get; set; } = new KeyValueCollection();
        /// <summary>
        /// Settings overwrites defined by environments and machine names
        /// </summary>
        [XmlElement("Overwrite"), DataMember]
        public NameCollection<Overwrite> Overwrites { get; set; } = new NameCollection<Overwrite>();

        /// <summary>
        /// Get all items by environment
        /// </summary>
        /// <param name="environmentName">Environment name</param>
        /// <param name="machineName">Machine name</param>
        /// <returns>KeyValueCollection with all items</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValueCollection GetItems(string environmentName, string machineName)
        {
            var combinedKey = environmentName + ">" + machineName;
            if (!Overwrites.TryGetByPartialKey(combinedKey, out var partial))
            {
                if (!Overwrites.TryGetByPartialKey(environmentName, out partial))
                {
                    var machineKey = ">" + machineName;
                    Overwrites.TryGetByPartialKey(machineKey, out partial);
                }
            }
            var result = new KeyValueCollection(partial?.Items, false);
            if (Items != null)
            {
                foreach (var i in Items)
                    result.Add(i);
            }
            return result;
        }
    }
}
