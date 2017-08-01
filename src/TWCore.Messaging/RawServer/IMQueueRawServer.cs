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
using System.Collections.Generic;

namespace TWCore.Messaging.RawServer
{
    /// <summary>
    /// Message Queue raw server definition
    /// </summary>
    public interface IMQueueRawServer : IMQueue
    {
        #region Properties
        /// <summary>
        /// Gets the list of message queue server listeners
        /// </summary>
        List<IMQueueRawServerListener> QueueServerListeners { get; }
        /// <summary>
        /// Gets if the server is configured as response server
        /// </summary>
        bool ResponseServer { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Events that fires when a request message is received
        /// </summary>
        event EventHandler<RawRequestReceivedEventArgs> RequestReceived;
        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        event EventHandler<RawResponseReceivedEventArgs> ResponseReceived;
        /// <summary>
        /// Events that fires when a response message is sent
        /// </summary>
        event EventHandler<RawResponseSentEventArgs> ResponseSent;
        /// <summary>
        /// Events that fires when a response message is about to be sent
        /// </summary>
        event EventHandler<RawResponseSentEventArgs> BeforeSendResponse;
        #endregion

        #region Methods
        /// <summary>
        /// Start the queue listener for request messages
        /// </summary>
        void StartListeners();
        /// <summary>
        /// Stop the queue listener
        /// </summary>
        void StopListeners();
        #endregion
    }
}
