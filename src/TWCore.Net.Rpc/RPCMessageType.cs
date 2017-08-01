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
        Unknown = 0x00,
        /// <summary>
        /// Ping message
        /// </summary>
        Ping = 0x01,
        /// <summary>
        /// Pong message
        /// </summary>
        Pong = 0x02,
        /// <summary>
        /// Session request message
        /// </summary>
        SessionRequest = 0x03,
        /// <summary>
        /// Session response message
        /// </summary>
        SessionResponse = 0x04,
        /// <summary>
        /// Request message
        /// </summary>
        RequestMessage = 0x05,
        /// <summary>
        /// Response message
        /// </summary>
        ResponseMessage = 0x06,
        /// <summary>
        /// Event message
        /// </summary>
        EventMessage = 0x07,
        /// <summary>
        /// Push message
        /// </summary>
        PushMessage
    }
}
