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

using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Serialization;

namespace TWCore.Net.RPC.Descriptors
{
    /// <summary>
    /// Describe a parameter of a method
    /// </summary>
    [DataContract]
    public class ParameterDescriptor
    {
        /// <summary>
        /// Parameter index number
        /// </summary>
        [XmlAttribute, DataMember]
        public int Index { get; set; }
        /// <summary>
        /// Parameter name
        /// </summary>
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Parameter type
        /// </summary>
        [XmlAttribute, DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Reflected parameter info object for direct access.
        /// </summary>
        [XmlIgnore, NonSerialize]
        public ParameterInfo Parameter { get; internal set; }
    }
}
