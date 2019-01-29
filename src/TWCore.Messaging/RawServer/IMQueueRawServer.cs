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

using System.Collections.Generic;
using TWCore.Messaging.Configuration;
using TWCore.Threading;
// ReSharper disable UnusedMemberInSuper.Global

namespace TWCore.Messaging.RawServer
{
    /// <inheritdoc />
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
        /// <summary>
        /// Message queue listener server counters
        /// </summary>
        MQRawServerCounters Counters { get; }
        #endregion

        #region Events
        /// <summary>
        /// Events that fires when a request message is received
        /// </summary>
        //event AsyncEventHandler<RawRequestReceivedEventArgs> RequestReceived;
        AsyncEvent<RawRequestReceivedEventArgs> RequestReceived { get; set; }
        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        //event AsyncEventHandler<RawResponseReceivedEventArgs> ResponseReceived;
        AsyncEvent<RawResponseReceivedEventArgs> ResponseReceived { get; set; }
        /// <summary>
        /// Events that fires when a response message is sent
        /// </summary>
        //event AsyncEventHandler<RawResponseSentEventArgs> ResponseSent;
        AsyncEvent<RawResponseSentEventArgs> ResponseSent { get; set; }
        /// <summary>
        /// Events that fires when a response message is about to be sent
        /// </summary>
        //event AsyncEventHandler<RawResponseSentEventArgs> BeforeSendResponse;
        AsyncEvent<RawResponseSentEventArgs> BeforeSendResponse { get; set; }
        #endregion

        #region Init Methods
        /// <summary>
        /// Initialize client with the configuration
        /// </summary>
        /// <param name="config">Message queue client configuration</param>
        void Init(MQPairConfig config);
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
