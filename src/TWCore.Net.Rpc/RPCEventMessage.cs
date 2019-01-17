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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
// ReSharper disable InconsistentNaming

namespace TWCore.Net.RPC
{
    /// <inheritdoc />
    /// <summary>
    /// RPC Event message sent by the server
    /// </summary>
    [Serializable, DataContract]
    public sealed class RPCEventMessage : RPCMessage
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

        #region Statics
        /// <summary>
        ///  Retrieves an Event Message from the Pool
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="eventName">Event name</param>
        /// <param name="eventArgs">Event args</param>
        /// <returns>Event message value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RPCEventMessage Retrieve(string serviceName, string eventName, EventArgs eventArgs)
        {
            var value = ReferencePool<RPCEventMessage>.Shared.New();
            value.ServiceName = serviceName;
            value.EventName = eventName;
            value.EventArgs = eventArgs;
            return value;
        }
        /// <summary>
        /// Stores an Event Message in the Pool.
        /// </summary>
        /// <param name="value">Event message value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(RPCEventMessage value)
        {
            if (value is null) return;
            value.ServiceName = null;
            value.EventName = null;
            value.EventArgs = null;
            ReferencePool<RPCEventMessage>.Shared.Store(value);
        }
        #endregion
    }
}
