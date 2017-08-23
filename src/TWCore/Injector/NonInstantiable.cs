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

using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;

namespace TWCore.Injector
{
    /// <summary>
    /// Definition for non instantiable objects (Interfaces and Abstracts)
    /// </summary>
    [DataContract]
    public class NonInstantiable
    {
        /// <summary>
        /// Class object type
        /// </summary>
        [XmlAttribute, DataMember]
        public string Type { get; set; }
        /// <summary>
        /// Default class name implementation to load
        /// </summary>
        [XmlAttribute]
        public string DefaultClassName { get; set; }
        /// <summary>
        /// Implementations of the non instantiable object
        /// </summary>
        [XmlElement("Class")]
        public NameCollection<Instantiable> ClassDefinitions { get; set; } = new NameCollection<Instantiable>();
    }
}
