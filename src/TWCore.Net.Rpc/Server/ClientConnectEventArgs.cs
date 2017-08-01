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
        /// <summary>
        /// Client connection event args, used on the event when a new client is connected to the service
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        public ClientConnectEventArgs(Guid clientId) => ClientId = clientId;
    }
}
