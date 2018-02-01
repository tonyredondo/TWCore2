﻿/*
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
using System.Threading;

namespace TWCore.Net.RPC.Server
{
    /// <inheritdoc />
    /// <summary>
    /// Method event args. To use when the server received a RPC method call request.
    /// </summary>
    public class MethodEventArgs : EventArgs
    {
        /// <summary>
        /// Client identifier
        /// </summary>
        public Guid ClientId { get; private set; }
        /// <summary>
        /// RPC Request message
        /// </summary>
        public RPCRequestMessage Request { get; private set; }
        /// <summary>
        /// RPC Response message
        /// </summary>
        public RPCResponseMessage Response { get; set; }
        /// <summary>
        /// Connection CancellationToken
        /// </summary>
        public CancellationToken CancellationToken { get; private set; }

        /// <summary>
        /// Retrieve a MethodEventArgs from the Pool
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="request">RPC Request message</param>
        /// <param name="cancellationToken">Connection CancellationToken</param>
        /// <returns>Method event args. To use when the server received a RPC method call request.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodEventArgs Retrieve(Guid clientId, RPCRequestMessage request, CancellationToken cancellationToken)
        {
            var mEvent = ReferencePool<MethodEventArgs>.Shared.New();
            mEvent.ClientId = clientId;
            mEvent.Request = request;
            mEvent.Response = null;
            mEvent.CancellationToken = cancellationToken;
            return mEvent;
        }
        /// <summary>
        /// Stores the Method event args to the pool
        /// </summary>
        /// <param name="value">Method event args. To use when the server received a RPC method call request.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(MethodEventArgs value)
        {
            value.ClientId = Guid.Empty;
            value.Request = null;
            value.Response = null;
            value.CancellationToken = CancellationToken.None;
            ReferencePool<MethodEventArgs>.Shared.Store(value);
        }
    }
}
