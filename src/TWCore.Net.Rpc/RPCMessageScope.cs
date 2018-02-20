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

// ReSharper disable InconsistentNaming
namespace TWCore.Net.RPC
{
    /// <summary>
    /// RPC Push Message Scope
    /// </summary>
    public enum RPCMessageScope
    {
        /// <summary>
        /// Push message only to a client session
        /// </summary>
        Session,
        /// <summary>
        /// Push message to a clients into a hub
        /// </summary>
        Hub,
        /// <summary>
        /// Push message to all connected clients
        /// </summary>
        Global
    }
}
