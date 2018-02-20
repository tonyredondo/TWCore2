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

using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
// ReSharper disable CollectionNeverUpdated.Global

namespace TWCore.Injector
{
    /// <inheritdoc />
    /// <summary>
    /// Instantiable object definition
    /// </summary>
    [DataContract]
    public class Instantiable : INameItem
    {
        /// <inheritdoc />
        /// <summary>
        /// Name of the definition
        /// </summary>
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Instantiable object type
        /// </summary>
        [XmlAttribute, DataMember]
        public string Type { get; set; }
        /// <summary>
        /// Defines if the class should be loaded as a singleton
        /// </summary>
        [XmlAttribute]
        public bool Singleton { get; set; }
        /// <summary>
        /// Constructor parameters
        /// </summary>
        [XmlElement("Parameter"), DataMember]
        public NameCollection<Parameter> Parameters { get; set; } = new NameCollection<Parameter>();
        /// <summary>
        /// After create instance values on the properties
        /// </summary>
        [XmlElement("PropertySet"), DataMember]
        public NameCollection<PropertySet> PropertiesSets { get; set; } = new NameCollection<PropertySet>();
    }
}
