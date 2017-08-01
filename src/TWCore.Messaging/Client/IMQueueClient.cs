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
using System.Threading;

namespace TWCore.Messaging.Client
{
    /// <summary>
    /// Message Queue client definition
    /// </summary>
    public interface IMQueueClient : IMQueue
    {
        #region Properties
        /// <summary>
        /// Gets the client counters
        /// </summary>
        MQClientCounters Counters { get; }
        #endregion

        #region Events
        /// <summary>
        /// Events that fires when a request message is sent
        /// </summary>
        event EventHandler<RequestSentEventArgs> OnRequestSent;
        /// <summary>
        /// Events that fires when a request message is about to be sent
        /// </summary>
        event EventHandler<RequestSentEventArgs> OnBeforeSendRequest;
        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        event EventHandler<ResponseReceivedEventArgs> OnResponseReceived;
        #endregion

        #region Methods
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Message correlation Id</returns>
        Guid Send<T>(T obj);
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Message correlation Id</returns>
        Guid Send<T>(T obj, Guid correlationId);
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <typeparam name="T">Type of the object to be received</typeparam>
        /// <param name="correlationId">Correlation id</param>
        /// <returns>Object instance received from the queue</returns>
        T Receive<T>(Guid correlationId);
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <typeparam name="T">Type of the object to be received</typeparam>
        /// <param name="correlationId">Correlation id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        T Receive<T>(Guid correlationId, CancellationToken cancellationToken);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Object instance received from the queue</returns>
        R SendAndReceive<R, T>(T obj);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        R SendAndReceive<R, T>(T obj, CancellationToken cancellationToken);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Object instance received from the queue</returns>
        R SendAndReceive<R, T>(T obj, Guid correlationId);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        R SendAndReceive<R, T>(T obj, Guid correlationId, CancellationToken cancellationToken);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Object instance received from the queue</returns>
        R SendAndReceive<R>(object obj);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        R SendAndReceive<R>(object obj, CancellationToken cancellationToken);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Object instance received from the queue</returns>
        R SendAndReceive<R>(object obj, Guid correlationId);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="R">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        R SendAndReceive<R>(object obj, Guid correlationId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the complete response message with headers from a body
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <returns>Complete Response message instance</returns>
        ResponseMessage GetCompleteMessage(object messageBody);
        #endregion
    }
}
