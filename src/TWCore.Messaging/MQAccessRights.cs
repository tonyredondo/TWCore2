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
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging
{
    /// <summary>
    /// Message queue access rights
    /// </summary>
    [Flags]
    public enum MQAccessRights : byte
    {
        /// <summary>
        /// Delete messages permission
        /// </summary>
        DeleteMessage = 1,
        /// <summary>
        /// Peek messages permission
        /// </summary>
        PeekMessage = 2,
        /// <summary>
        /// Receive messages permission
        /// </summary>
        ReceiveMessage = 4,
        /// <summary>
        /// Write messages permission
        /// </summary>
        WriteMessage = 8,
        /// <summary>
        /// Delete queue permission
        /// </summary>
        DeleteQueue = 16,
        /// <summary>
        /// Fullcontrol permission
        /// </summary>
        FullControl = 32
    }
}
