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


// ReSharper disable InconsistentNaming
namespace TWCore.Net.RPC
{
    /// <summary>
    /// RPC Message Type
    /// </summary>
    public enum RPCMessageType : byte
    {
        /// <summary>
        /// Unknown message type
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Ping message
        /// </summary>
        Ping = 1,
        /// <summary>
        /// Session request message
        /// </summary>
        SessionRequest = 2,
        /// <summary>
        /// Request message
        /// </summary>
        RequestMessage = 3,

        /// <summary>
        /// Pong message
        /// </summary>
        Pong = 11,
        /// <summary>
        /// Session response message
        /// </summary>
        SessionResponse = 12,
        /// <summary>
        /// Response message
        /// </summary>
        ResponseMessage = 13,
        /// <summary>
        /// Event message
        /// </summary>
        EventMessage = 14,
        /// <summary>
        /// Push message
        /// </summary>
        PushMessage = 15
    }
}
