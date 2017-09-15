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
using TWCore.Serialization;

namespace TWCore.Object.Descriptor
{
    /// <summary>
    /// Member definition
    /// </summary>
    [DataContract]
    public class Member
    {
        /// <summary>
        /// Member name
        /// </summary>
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Member value
        /// </summary>
        [XmlElement("Value"), DataMember]
        public Value Value { get; set; }
        /// <summary>
        /// Member type
        /// </summary>
        [XmlAttribute, DataMember]
        public MemberType Type { get; set; }
        /// <summary>
        /// Object member
        /// </summary>
        [XmlIgnore, NonSerialize]
        public object Object { get; set; }
    }
}