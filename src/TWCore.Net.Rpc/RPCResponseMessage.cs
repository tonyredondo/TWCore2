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

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// RPC Response Message
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RPCResponseMessage()
        {

        }
        /// <inheritdoc />
        /// <summary>
        /// RPC Response Message
        /// </summary>
        /// <param name="request">Request Message instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RPCResponseMessage(RPCRequestMessage request)
        {
            RequestMessageId = request.MessageId;
        }
        #endregion
        
        #region Static Methods
        /// <summary>
        /// Retrieve a Response Message from the pool
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RPCResponseMessage Retrieve(RPCRequestMessage request)
        {
            var message = ReferencePool<RPCResponseMessage>.Shared.New();
            message.MessageId = Guid.NewGuid();
            message.RequestMessageId = request.MessageId;
            return message;
        }
        /// <summary>
        /// Retrieve a Response Message from the pool
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RPCResponseMessage Retrieve(RPCError errorMessage)
        {
            var message = ReferencePool<RPCResponseMessage>.Shared.New();
            message.MessageId = errorMessage.MessageId;
            message.Exception = errorMessage.Exception;
            return message;
        }
        /// <summary>
        /// Store the RPCResponseMessage to the pool
        /// </summary>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(RPCResponseMessage message)
        {
            message.MessageId = Guid.Empty;
            message.RequestMessageId = Guid.Empty;
            message.ReturnValue = null;
            message.Exception = null;
            ReferencePool<RPCResponseMessage>.Shared.Store(message);
        }
        #endregion
    }
}
