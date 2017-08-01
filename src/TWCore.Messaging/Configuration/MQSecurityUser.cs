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

using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace TWCore.Messaging.Configuration
{
    /// <summary>
    /// Message queue security user configuration
    /// </summary>
    [DataContract]
    public class MQSecurityUser
    {
        /// <summary>
        /// User name
        /// </summary>
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        /// <summary>
        /// User permissions
        /// </summary>
        [XmlAttribute, DataMember]
        public string Permissions { get; set; }

        /// <summary>
        /// Get access rights
        /// </summary>
        /// <returns>MQAccessRights instance</returns>
        public MQAccessRights GetAccessRights()
        {
            if (Permissions.IsNotNullOrEmpty())
            {
                var permissions = Permissions.SplitAndTrim(",");
                MQAccessRights? rights = null;
                foreach (var permission in permissions)
                {
                    var r = (MQAccessRights)Enum.Parse(typeof(MQAccessRights), permission);
                    if (rights.HasValue)
                        rights = rights | r;
                    else
                        rights = r;
                }
                return rights ?? 0;
            }
            return 0;
        }
    }
}
