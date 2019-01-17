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
    /// RPC Session Request Message
    /// </summary>
    [Serializable, DataContract]
    public sealed class RPCSessionRequestMessage : RPCMessage
    {
        /// <summary>
        /// Client session identifier
        /// </summary>
        [DataMember]
        public Guid SessionId { get; set; }
        /// <summary>
        /// Hub name
        /// </summary>
        [DataMember]
        public string Hub { get; set; }



        #region Static Methods
        /// <summary>
        /// Retrieve a Session Request Message from the pool
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RPCSessionRequestMessage Retrieve(Guid sessionId, string hub)
        {
            var message = ReferencePool<RPCSessionRequestMessage>.Shared.New();
            message.MessageId = Guid.NewGuid();
            message.SessionId = sessionId;
            message.Hub = hub;
            return message;
        }
        /// <summary>
        /// Store the Session Request Message to the pool
        /// </summary>
        /// <param name="message">RPCSessionRequestMessage value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(RPCSessionRequestMessage message)
        {
            message.MessageId = Guid.Empty;
            message.SessionId = Guid.Empty;
            message.Hub = null;
            ReferencePool<RPCSessionRequestMessage>.Shared.Store(message);
        }
        #endregion
    }
}
