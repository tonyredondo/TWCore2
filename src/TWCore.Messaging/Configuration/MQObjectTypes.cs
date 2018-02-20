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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Serialization;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.Configuration
{
    /// <summary>
    /// Message queue object types
    /// </summary>
    [DataContract]
    public class MQObjectTypes
    {
        /// <summary>
        /// Message queue client Type
        /// </summary>
        [XmlAttribute, DataMember]
        public string Client { get; set; }
        /// <summary>
        /// Message queue server Type
        /// </summary>
        [XmlAttribute, DataMember]
        public string Server { get; set; }
        /// <summary>
        /// Message queue admin Type
        /// </summary>
        [XmlAttribute, DataMember]
        public string Admin { get; set; }

        /// <summary>
        /// Gets or Sets the Message queue client System.Type
        /// </summary>
        [XmlIgnore, NonSerialize]
        public Type ClientType
        {
            get
            {
                try
                {
                    return Client != null ? Core.GetType(Client, true) : null;
                }
                catch (Exception ex)
                {
                    Core.Log.Error(ex, "The Message Queue Client Type '{0}' couldn't be found.", Client);
                }
                return null;
            }
            set => Client = value?.GetTypeName();
        }
        /// <summary>
        /// Gets or Sets the Message queue server System.Type
        /// </summary>
        [XmlIgnore, NonSerialize]
        public Type ServerType
        {
            get
            {
                try
                {
                    return Server != null ? Core.GetType(Server, true) : null;
                }
                catch (Exception ex)
                {
                    Core.Log.Error(ex, "The Message Queue Server Type '{0}' couldn't be found.", Server);
                }
                return null;
            }
            set => Server = value?.GetTypeName();
        }
        /// <summary>
        /// Gets or Sets the Message queue admin System.Type
        /// </summary>
        [XmlIgnore, NonSerialize]
        public Type AdminType
        {
            get
            {
                try
                {
                    return Admin != null ? Core.GetType(Admin, true) : null;
                }
                catch (Exception ex)
                {
                    Core.Log.Error(ex, "The Message Queue Admin Type '{0}' couldn't be found.", Admin);
                }
                return null;
            }
            set => Admin = value?.GetTypeName();
        }
    }
}
