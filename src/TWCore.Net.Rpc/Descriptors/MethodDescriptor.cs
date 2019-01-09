﻿/*
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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Reflection;
using TWCore.Serialization;

namespace TWCore.Net.RPC.Descriptors
{
    /// <summary>
    /// Describe a Method member of an object
    /// </summary>
    [DataContract]
    public class MethodDescriptor
    {
        /// <summary>
        /// Method identifier
        /// </summary>
        [XmlAttribute, DataMember]
        public Guid Id { get; set; }
        /// <summary>
        /// Method name
        /// </summary>
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Method parameters
        /// </summary>
        [XmlElement("Parameter"), DataMember]
        public ParameterDescriptor[] Parameters { get; set; }
        /// <summary>
        /// Method return object type
        /// </summary>
        [XmlAttribute, DataMember]
        public string ReturnType { get; set; }

        /// <summary>
        /// Reflected method accessor delegate for direct access.
        /// </summary>
        [XmlIgnore, NonSerialize]
        public MethodAccessorDelegate Method { get; internal set; }
        /// <summary>
        /// Type of ReturnType
        /// </summary>
        [XmlIgnore, NonSerialize]
        public Type TypeOfReturnType { get; internal set; }

        /// <summary>
        /// Return is Task
        /// </summary>
        [XmlIgnore, NonSerialize]
        public bool ReturnIsTask { get; internal set; }
        /// <summary>
        /// Return Task Result
        /// </summary>
        [XmlIgnore, NonSerialize]
        public FastPropertyInfo ReturnTaskResult { get; internal set; }
    }
}
