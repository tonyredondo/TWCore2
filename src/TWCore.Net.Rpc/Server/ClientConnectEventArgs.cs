﻿/*
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
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace TWCore.Net.RPC.Server
{
    /// <summary>
    /// Client connection event args, used on the event when a new client is connected to the service
    /// </summary>
    public class ClientConnectEventArgs
    {
        /// <summary>
        /// Client identifier
        /// </summary>
        public Guid ClientId { get; private set; }


        #region Statics
        /// <summary>
        ///  Client connection event args, for the event when the server receive a server descriptor request.
        /// </summary>
        /// <returns>ServerDescriptorsEventArgs instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ClientConnectEventArgs Retrieve(Guid clientId)
        {
            var value = ReferencePool<ClientConnectEventArgs>.Shared.New();
            value.ClientId = clientId;
            return value;
        }
        /// <summary>
        /// Stores a client connection args.
        /// </summary>
        /// <param name="value">Method event args value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(ClientConnectEventArgs value)
        {
            if (value is null) return;
            value.ClientId = Guid.Empty;
            ReferencePool<ClientConnectEventArgs>.Shared.Store(value);
        }
        #endregion
    }
}
