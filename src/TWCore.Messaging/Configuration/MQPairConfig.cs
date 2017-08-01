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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
using TWCore.Messaging.Client;
using TWCore.Messaging.RawClient;
using TWCore.Messaging.RawServer;
using TWCore.Messaging.Server;

namespace TWCore.Messaging.Configuration
{
    /// <summary>
    /// Message queue send/recv configuration
    /// </summary>
    [XmlRoot("QueueConfig"), DataContract]
    public class MQPairConfig : INameItem
    {
        /// <summary>
        /// Configuration name
        /// </summary>
        [XmlAttribute, DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Message queue client/server types
        /// </summary>
        [XmlElement, DataMember]
        public MQObjectTypes Types { get; set; } = new MQObjectTypes();
        /// <summary>
        /// Message queue raw client/server types
        /// </summary>
        [XmlElement, DataMember]
        public MQObjectTypes RawTypes { get; set; } = new MQObjectTypes();
        /// <summary>
        /// Message queue client queues
        /// </summary>
        [XmlElement, DataMember]
        public List<MQClientQueues> ClientQueues { get; set; } = new List<MQClientQueues>();
        /// <summary>
        /// Message queue server queues
        /// </summary>
        [XmlElement, DataMember]
        public List<MQServerQueues> ServerQueues { get; set; } = new List<MQServerQueues>();
        /// <summary>
        /// Request message options
        /// </summary>
        [XmlElement, DataMember]
        public MQRequestOptions RequestOptions { get; set; } = new MQRequestOptions();
        /// <summary>
        /// Response message options
        /// </summary>
        [XmlElement, DataMember]
        public MQResponseOptions ResponseOptions { get; set; } = new MQResponseOptions();
        /// <summary>
        /// Message queue security settings
        /// </summary>
        [XmlElement, DataMember]
        public List<MQSecurity> Security { get; set; } = new List<MQSecurity>();

        #region Get Methods
        /// <summary>
        /// Gets the Queue Client from the configuration
        /// </summary>
        /// <returns>IMQueueClient instance</returns>
        public IMQueueClient GetClient()
        {
            var type = Types?.ClientType;
            if (type != null)
            {
                var client = (IMQueueClient)Activator.CreateInstance(type);
                client.Init(this);
                return client;
            }
            else
                Core.Log.Warning("The type '{0}' couldn't be found in the assembly folder.", Types?.Client);
            return null;
        }
        /// <summary>
        /// Gets the Queue Raw Client from the configuration
        /// </summary>
        /// <returns>IMQueueClient instance</returns>
        public IMQueueRawClient GetRawClient()
        {
            var type = RawTypes?.ClientType;
            if (type != null)
            {
                var client = (IMQueueRawClient)Activator.CreateInstance(type);
                client.Init(this);
                return client;
            }
            else
                Core.Log.Warning("The type '{0}' couldn't be found in the assembly folder.", RawTypes?.Client);
            return null;
        }
        /// <summary>
        /// Gets the Queue Server from the configuration
        /// </summary>
        /// <param name="responseServer">true if the server needs to listen for response messages at the client side</param>
        /// <returns>IMQueueServer instance</returns>
        public IMQueueServer GetServer(bool responseServer = false)
        {
            var type = Types?.ServerType;
            if (type != null)
            {
                var server = (IMQueueServer)Activator.CreateInstance(type);
                server.Init(this);
                server.ResponseServer = responseServer;
                return server;
            }
            else
                Core.Log.Warning("The type '{0}' couldn't be found in the assembly folder.", Types?.Server);
            return null;
        }
        /// <summary>
        /// Gets the Queue RawServer from the configuration
        /// </summary>
        /// <param name="responseServer">true if the server needs to listen for response messages at the client side</param>
        /// <returns>IMQueueServer instance</returns>
        public IMQueueRawServer GetRawServer(bool responseServer = false)
        {
            var type = RawTypes?.ServerType;
            if (type != null)
            {
                var server = (IMQueueRawServer)Activator.CreateInstance(type);
                server.Init(this);
                server.ResponseServer = responseServer;
                return server;
            }
            else
                Core.Log.Warning("The type '{0}' couldn't be found in the assembly folder.", RawTypes?.Server);
            return null;
        }
        /// <summary>
        /// Gets the Queue Admin from the configuration
        /// </summary>
        /// <returns>IMQueueAdmin instance</returns>
        public IMQueueAdmin GetAdmin()
        {
            var type = Types?.AdminType ?? RawTypes?.AdminType;
            if (type != null)
            {
                var admin = (IMQueueAdmin)Activator.CreateInstance(type);
                return admin;
            }
            else
                Core.Log.Warning("The type '{0}' couldn't be found in the assembly folder.", Types?.Admin ?? RawTypes?.Admin);
            return null;
        }
        /// <summary>
        /// Gets the Queue manager for this queue configuration
        /// </summary>
        /// <returns>Queue manager instance</returns>
        public QueueManager GetQueueManager() => new QueueManager(this);
        #endregion
    }
}