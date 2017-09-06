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
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TWCore.Net.RPC
{
    /// <inheritdoc />
    /// <summary>
    /// RPC Session Response Message
    /// </summary>
    [Serializable, DataContract]
    public class RPCSessionResponseMessage : RPCMessage
    {
        /// <summary>
        /// Request Message identifier for this response
        /// </summary>
        [DataMember]
        public Guid RequestMessageId { get; set; }
        /// <summary>
        /// true if the remote procedure call was executed successfully; otherwise, false.
        /// </summary>
        [DataMember]
        public bool Succeed { get; set; }
        /// <summary>
        /// Client session identifier
        /// </summary>
        [DataMember]
        public Guid SessionId { get; set; }
    }
}
