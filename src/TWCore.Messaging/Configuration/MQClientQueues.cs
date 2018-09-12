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
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.Configuration
{
    /// <summary>
    /// Message queue client queues
    /// </summary>
    [DataContract, Serializable]
    public class MQClientQueues
    {
        private List<MQConnection> _sendQueues;
        private bool _sendQueuesProcessed;
        private MQConnection _recvQueue;
        private bool _recvQueueProcessed;

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
        /// Message queue connections wich the request message will be sent
        /// </summary>
        [XmlElement("SendQueue"), DataMember]
        public List<MQConnection> SendQueues
        {
            get
            {
                if (_sendQueues is null) _sendQueues = new List<MQConnection>();
                if (!_sendQueuesProcessed)
                {
                    _sendQueuesProcessed = true;
                    if (_sendQueues.Any(i => i.IsSkippingRoute()))
                    {
                        var newQueues = new List<MQConnection>();
                        foreach (var queue in _sendQueues)
                        {
                            if (queue.IsSkippingRoute())
                            {
                                Core.Log.Warning("Skipping client send queue by route skip value");
                                continue;
                            }
                            newQueues.Add(queue);
                        }
                        _sendQueues = newQueues;
                    }
                }
                return _sendQueues;
            }
            set
            {
                _sendQueues = value;
                _sendQueuesProcessed = false;
            }
        }
            //{ get; set; } = new List<MQConnection>();
        /// <summary>
        /// Message queue connection were the response message will be readed.
        /// </summary>
        [XmlElement("RecvQueue"), DataMember]
        public MQConnection RecvQueue
        {
            get
            {
                if (!_recvQueueProcessed)
                {
                    _recvQueueProcessed = true;
                    if (_recvQueue is null) return null;
                    if (_recvQueue.IsSkippingRoute())
                    {
                        Core.Log.Warning("Skipping client receiving queue by route skip value");
                        _recvQueue = null;
                    }
                }
                return _recvQueue;
            }
            set
            {
                _recvQueue = value;
                _recvQueueProcessed = false;
            }
        }
    }
}
