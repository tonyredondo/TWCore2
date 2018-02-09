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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
// ReSharper disable InconsistentNaming

namespace TWCore.Net.RPC
{
    /// <inheritdoc />
    /// <summary>
    /// RPC Request Message
    /// </summary>
    [Serializable, DataContract]
    public class RPCRequestMessage : RPCMessage
    {
        /// <summary>
        /// Method identifier
        /// </summary>
        [DataMember]
        public Guid MethodId { get; set; }
        /// <summary>
        /// Method parameters
        /// </summary>
        [DataMember]
		public object[] Parameters { get; set; }
        /// <summary>
        /// The client has a cancellation token
        /// </summary>
        [DataMember]
        public bool CancellationToken { get; set; }

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// RPC Request Message
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RPCRequestMessage()
        {
        }
        /// <inheritdoc />
        /// <summary>
        /// RPC Request Message
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RPCRequestMessage(Guid methodId, object[] parameters, bool cancellationToken)
        {
            MethodId = methodId;
            Parameters = parameters;
            CancellationToken = cancellationToken;
        }
        #endregion
    }
}
