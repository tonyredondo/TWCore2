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
// ReSharper disable InconsistentNaming

namespace TWCore.Net.RPC
{
    /// <inheritdoc />
    /// <summary>
    /// RPC Event message sent by the server
    /// </summary>
    [Serializable, DataContract]
    public class RPCEventMessage : RPCMessage
    {
        /// <summary>
        /// Service name
        /// </summary>
        [DataMember]
        public string ServiceName { get; set; }
        /// <summary>
        /// Fired event name
        /// </summary>
        [DataMember]
        public string EventName { get; set; }
        /// <summary>
        /// Event arguments
        /// </summary>
        [DataMember]
        public EventArgs EventArgs { get; set; }
    }
}
