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

using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;

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
        /// Settings overwrites defined by environments
        /// </summary>
        [XmlElement("EnvironmentOverwrite"), DataMember]
        public NameCollection<EnvironmentOverwrite> Overwrites { get; set; } = new NameCollection<EnvironmentOverwrite>();

        /// <summary>
        /// Get all items by environment
        /// </summary>
        /// <param name="environmentName">Environment name</param>
        /// <returns>KeyValueCollection with all items</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValueCollection GetItems(string environmentName)
        {
            var result = new KeyValueCollection(Overwrites.TryGetByPartialKey(environmentName, out var partial) ? partial.Items : null, false);
            Items?.Each(i => result.Add(i));
            return result;
        }
    }
}
