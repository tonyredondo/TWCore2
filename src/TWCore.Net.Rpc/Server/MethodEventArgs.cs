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
        public Guid ClientId { get; }
        /// <summary>
        /// RPC Request message
        /// </summary>
        public RPCRequestMessage Request { get; }
        /// <summary>
        /// RPC Response message
        /// </summary>
        public RPCResponseMessage Response { get; set; }
        /// <summary>
        /// Connection CancellationToken
        /// </summary>
        public CancellationToken CancellationToken { get; }

        #region .ctor
        /// <inheritdoc />
        /// <summary>
        /// Method event args. To use when the server received a RPC method call request.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MethodEventArgs()
        {
        }
        /// <inheritdoc />
        /// <summary>
        /// Method event args. To use when the server received a RPC method call request.
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="request">RPC Request message</param>
        /// <param name="cancellationToken">Connection CancellationToken</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MethodEventArgs(Guid clientId, RPCRequestMessage request, CancellationToken cancellationToken)
        {
            ClientId = clientId;
            Request = request;
            CancellationToken = cancellationToken;
        }
        #endregion
    }
}
