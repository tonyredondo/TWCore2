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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TWCore.Collections;
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.Configuration
{
    /// <summary>
    /// Message queue server queues
    /// </summary>
    [DataContract, Serializable]
    public class MQServerQueues
    {
        private List<MQConnection> _recvQueues;
        private bool _recvQueuesProcessed;
        private List<MQConnection> _additionalSendQueues;
        private bool _additionalSendQueuesProcessed;

        /// <summary>
        /// Environment name
        /// </summary>
        [XmlAttribute, DataMember]
        public string EnvironmentName { get; set; }
        /// <summary>
        /// Machine name
        /// </summary>
        [XmlAttribute, DataMember]
        public string MachineName { get; set; }
        /// <summary>
        /// Message queue connections wich the request message will be read
        /// </summary>
        [XmlElement("RecvQueue"), DataMember]
        public List<MQConnection> RecvQueues
        {
            get
            {
                if (_recvQueues is null) _recvQueues = new List<MQConnection>();
                if (!_recvQueuesProcessed)
                {
                    _recvQueuesProcessed = true;
                    if (_recvQueues.Any(i => i.IsSkippingRoute()))
                    {
                        var newQueues = new List<MQConnection>();
                        foreach (var queue in _recvQueues)
                        {
                            if (queue.IsSkippingRoute())
                            {
                                Core.Log.Warning("Skipping server receiving queue by route skip value");
                                continue;
                            }
                            newQueues.Add(queue);
                        }
                        _recvQueues = newQueues;
                    }
                }
                return _recvQueues;
            }
            set
            {
                _recvQueues = value;
                _recvQueuesProcessed = false;
            }
        }
        /// <summary>
        /// Additionals message queue connections where the response message will be sent
        /// </summary>
        [XmlElement("AdditionalSendQueue"), DataMember]
        public List<MQConnection> AdditionalSendQueues
        {
            get
            {
                if (_additionalSendQueues is null) _additionalSendQueues = new List<MQConnection>();
                if (!_additionalSendQueuesProcessed)
                {
                    _additionalSendQueuesProcessed = true;
                    if (_additionalSendQueues.Any(i => i.IsSkippingRoute()))
                    {
                        var newQueues = new List<MQConnection>();
                        foreach (var queue in _additionalSendQueues)
                        {
                            if (queue.IsSkippingRoute())
                            {
                                Core.Log.Warning("Skipping additional send queue by route skip value");
                                continue;
                            }
                            newQueues.Add(queue);
                        }
                        _additionalSendQueues = newQueues;
                    }
                }
                return _additionalSendQueues;
            }
            set
            {
                _additionalSendQueues = value;
                _additionalSendQueuesProcessed = false;
            }
        }
        /// <summary>
        /// Client queues routes rebinding
        /// </summary>
        [XmlElement("ClientQueueRouteRebinding"), DataMember]
        public KeyValueCollection ClientQueuesRoutesRebindings { get; set; }
    }
}
