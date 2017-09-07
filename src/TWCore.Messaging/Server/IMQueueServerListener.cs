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
using System.Threading;
using System.Threading.Tasks;
using TWCore.Messaging.Configuration;
using TWCore.Serialization;
// ReSharper disable UnusedMemberInSuper.Global

namespace TWCore.Messaging.Server
{
    /// <inheritdoc />
    /// <summary>
    /// Message Queue server listener definition
    /// </summary>
    public interface IMQueueServerListener : IDisposable
    {
        #region Properties
        /// <summary>
        /// Message queue listener server counters
        /// </summary>
        MQServerCounters Counters { get; }
        /// <summary>
        /// Message queue connection
        /// </summary>
        MQConnection Connection { get; }
        /// <summary>
        /// Gets the current configuration
        /// </summary>
        MQPairConfig Config { get; }
        /// <summary>
        /// Gets or sets the receiver serializer
        /// </summary>
        ISerializer ReceiverSerializer { get; set; }
        /// <summary>
        /// Gets if the server is configured as response server
        /// </summary>
        bool ResponseServer { get; }
        #endregion

        #region Events
        /// <summary>
        /// Events that fires when a request message is received
        /// </summary>
        event EventHandler<RequestReceivedEventArgs> RequestReceived;
        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        event EventHandler<ResponseReceivedEventArgs> ResponseReceived;
        #endregion

        #region Methods
        /// <summary>
        /// Start the queue listener for request messages
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Task of the method execution</returns>
        Task TaskStartAsync(CancellationToken token);
        #endregion
    }
}
