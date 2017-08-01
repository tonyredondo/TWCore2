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

namespace TWCore.Diagnostics.Status
{
    /// <summary>
    /// Represent a status item value
    /// </summary>
    [DataContract]
    public class StatusItemValue
    {
        /// <summary>
        /// Key of the value
        /// </summary>
        [XmlAttribute, DataMember]
        public string Key { get; set; }
        /// <summary>
        /// Value
        /// </summary>
        [XmlAttribute, DataMember]
        public string Value { get; set; }
        /// <summary>
        /// Value status
        /// </summary>
        [XmlAttribute, DataMember]
        public StatusItemValueStatus Status { get; set; }

        #region .ctor
        /// <summary>
        /// Represent a status item value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatusItemValue() { }
        /// <summary>
        /// Represent a status item value
        /// </summary>
        /// <param name="key">Key of the value</param>
        /// <param name="value">Value</param>
        /// <param name="status">Value status</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatusItemValue(string key, string value, StatusItemValueStatus status)
        {
            Key = key;
            Value = value;
            Status = status;
        }
        #endregion
    }
}
