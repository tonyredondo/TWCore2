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
using System.Runtime.Serialization;
using System.Xml.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TWCore.Data.Schema
{
    /// <summary>
    /// Foreign Key Schema
    /// </summary>
    [DataContract]
    public class ForeignKeySchema
    {
        /// <summary>
        /// Constraint Name
        /// </summary>
        [DataMember, XmlAttribute]
        public string ConstraintName { get; set; }
        /// <summary>
        /// Foreign Table
        /// </summary>
        [DataMember, XmlAttribute]
        public string ForeignTable { get; set; }
    }
}
