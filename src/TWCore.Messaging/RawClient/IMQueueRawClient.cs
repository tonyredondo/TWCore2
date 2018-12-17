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

using System;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Threading;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable ReturnTypeCanBeEnumerable.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Messaging.RawClient
{
    /// <inheritdoc />
    /// <summary>
    /// Message Queue raw client definition
    /// </summary>
    public interface IMQueueRawClient : IMQueue
    {
        #region Events
        /// <summary>
        /// Events that fires when a request message is sent
        /// </summary>
        AsyncEvent<RawMessageEventArgs> OnRequestSent { get; set; }
        /// <summary>
        /// Events that fires when a request message is about to be sent
        /// </summary>
        AsyncEvent<RawMessageEventArgs> OnBeforeSendRequest { get; set; }
        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        AsyncEvent<RawMessageEventArgs> OnResponseReceived { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Message correlation Id</returns>
        Task<Guid> SendAsync(object obj);
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Message correlation Id</returns>
        Task<Guid> SendAsync(object obj, Guid correlationId);
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Message correlation Id</returns>
        Task<Guid> SendBytesAsync(byte[] obj);
        /// <summary>
        /// Sends a message and returns the correlation Id
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Message correlation Id</returns>
        Task<Guid> SendBytesAsync(byte[] obj, Guid correlationId);


        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <typeparam name="T">Type of the object to be received</typeparam>
        /// <param name="correlationId">Correlation id</param>
        /// <returns>Object instance received from the queue</returns>
        Task<T> ReceiveAsync<T>(Guid correlationId);
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <typeparam name="T">Type of the object to be received</typeparam>
        /// <param name="correlationId">Correlation id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        Task<T> ReceiveAsync<T>(Guid correlationId, CancellationToken cancellationToken);
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <param name="correlationId">Correlation id</param>
        /// <returns>Object instance received from the queue</returns>
        Task<byte[]> ReceiveBytesAsync(Guid correlationId);
        /// <summary>
        /// Receive a message from the queue
        /// </summary>
        /// <param name="correlationId">Correlation id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        Task<byte[]> ReceiveBytesAsync(Guid correlationId, CancellationToken cancellationToken);
        #endregion

        #region SendAndReceive
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Object instance received from the queue</returns>
        Task<byte[]> SendAndReceiveAsync(byte[] obj);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Object instance received from the queue</returns>
        Task<TR> SendAndReceiveAsync<TR, T>(T obj);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        Task<TR> SendAndReceiveAsync<TR, T>(T obj, CancellationToken cancellationToken);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Object instance received from the queue</returns>
        Task<TR> SendAndReceiveAsync<TR, T>(T obj, Guid correlationId);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="T">Type of the object to be sent</typeparam>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        Task<TR> SendAndReceiveAsync<TR, T>(T obj, Guid correlationId, CancellationToken cancellationToken);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <returns>Object instance received from the queue</returns>
        Task<TR> SendAndReceiveAsync<TR>(object obj);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        Task<TR> SendAndReceiveAsync<TR>(object obj, CancellationToken cancellationToken);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <returns>Object instance received from the queue</returns>
        Task<TR> SendAndReceiveAsync<TR>(object obj, Guid correlationId);
        /// <summary>
        /// Sends and waits for receive response from the queue (like RPC)
        /// </summary>
        /// <typeparam name="TR">Type of the object to be received</typeparam>
        /// <param name="obj">Object to be sent</param>
        /// <param name="correlationId">Manual defined correlationId</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object instance received from the queue</returns>
        Task<TR> SendAndReceiveAsync<TR>(object obj, Guid correlationId, CancellationToken cancellationToken);
        #endregion
    }
}
