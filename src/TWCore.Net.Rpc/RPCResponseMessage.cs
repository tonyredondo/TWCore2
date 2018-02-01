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
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore.Net.RPC
{
    /// <inheritdoc />
    /// <summary>
    /// RPC Response Message
    /// </summary>
    [Serializable, DataContract]
    public class RPCResponseMessage : RPCMessage
    {
        /// <summary>
        /// Request Message identifier for this response
        /// </summary>
        [DataMember]
        public Guid RequestMessageId { get; set; }
        /// <summary>
        /// Object returned from the execution of the remote procedure, if the method return void, then is null.
        /// </summary>
        [DataMember]
        public object ReturnValue { get; set; }
        /// <summary>
        /// Contains the exception on the remote procedure call if wasn't successful.
        /// </summary>
        [DataMember]
        public SerializableException Exception { get; set; }

        #region Static Methods
        /// <summary>
        /// Retrieve a Response Message instance
        /// </summary>
        /// <param name="request">Request Message instance</param>
        /// <returns>Response Message instance</returns>
        public static RPCResponseMessage Retrieve(RPCRequestMessage request)
        {
            var response = ReferencePool<RPCResponseMessage>.Shared.New();
            response.MessageId = Guid.NewGuid();
            response.RequestMessageId = request.MessageId;
            return response;
        }
        /// <summary>
        /// Store a Response Message instance
        /// </summary>
        /// <param name="response">Response Message instance</param>
        public static void Store(RPCResponseMessage response)
        {
            response.MessageId = Guid.Empty;
            response.RequestMessageId = Guid.Empty;
            response.ReturnValue = null;
            response.Exception = null;
            ReferencePool<RPCResponseMessage>.Shared.Store(response);
        }
        #endregion
    }
}
