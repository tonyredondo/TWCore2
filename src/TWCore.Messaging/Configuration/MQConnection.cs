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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.Configuration
{
    /// <summary>
    /// Defines a connection configuration
    /// </summary>
    [DataContract, Serializable]
    public class MQConnection
    {
        private string _key;
        private string _route;
        /// <summary>
        /// Message queue route (path or url)
        /// </summary>
        [XmlAttribute, DataMember]
        public string Route
        {
            get => _route;
            set
            {
                _route = Core.ReplaceEncriptionTemplate(value);
            }
        }
        /// <summary>
        /// Message queue name
        /// </summary>
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Message queue connection parameters
        /// </summary>
        [XmlElement("Param"), DataMember]
        public KeyValueCollection Parameters { get; set; } = new KeyValueCollection();

        /// <summary>
        /// Defines a connection configuration
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MQConnection() { }
        /// <summary>
        /// Defines a connection configuration
        /// </summary>
        /// <param name="route">Message queue route (path or url)</param>
        /// <param name="name">Message queue name</param>
        /// <param name="parameters">Message queue connection parameters</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MQConnection(string route, string name, params KeyValue<string, string>[] parameters)
        {
            Route = route;
            Name = name;
            Parameters = new KeyValueCollection(parameters);
        }

        /// <summary>
        /// Get the Precalculate Key
        /// </summary>
        /// <returns>Key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetKey()
        {
            return _key ?? (_key = Route + Name + Parameters?.Select(i => i.Key + i.Value));
        }

        /// <summary>
        /// Is Skipping Route Value
        /// </summary>
        /// <returns>True if the route value is skip; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSkippingRoute()
        {
            return Route == null || Route == Factory.SkipInstanceValue;
        }
    }
}
