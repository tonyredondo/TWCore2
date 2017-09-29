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
// ReSharper disable CheckNamespace

namespace TWCore.Services.Messaging
{
    /// <inheritdoc />
    /// <summary>
    /// Defines a business instance for message processing
    /// </summary>
    public interface IBusiness : IDisposable
    {
        /// <summary>
        /// Initialize business instance
        /// </summary>
        void Init();
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(object message, CancellationToken cancellationToken);
    }

    /// <inheritdoc />
    /// <summary>
    /// Defines a business instance for message processing
    /// </summary>
    /// <typeparam name="T">Message Type</typeparam>
    public interface IBusiness<in T> : IBusiness
    {
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T message, CancellationToken cancellationToken);
    }

    /// <inheritdoc />
    /// <summary>
    /// Defines a business instance for message processing
    /// </summary>
    /// <typeparam name="T1">Message Type 1</typeparam>
    /// <typeparam name="T2">Message Type 2</typeparam>
    public interface IBusiness<in T1, in T2> : IBusiness
    {
        /// <summary>
        /// Process T1 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T1 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T2 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T2 message, CancellationToken cancellationToken);
    }

    /// <inheritdoc />
    /// <summary>
    /// Defines a business instance for message processing
    /// </summary>
    /// <typeparam name="T1">Message Type 1</typeparam>
    /// <typeparam name="T2">Message Type 2</typeparam>
    /// <typeparam name="T3">Message Type 3</typeparam>
    public interface IBusiness<in T1, in T2, in T3> : IBusiness
    {
        /// <summary>
        /// Process T1 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T1 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T2 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T2 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T3 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T3 message, CancellationToken cancellationToken);
    }

    /// <inheritdoc />
    /// <summary>
    /// Defines a business instance for message processing
    /// </summary>
    /// <typeparam name="T1">Message Type 1</typeparam>
    /// <typeparam name="T2">Message Type 2</typeparam>
    /// <typeparam name="T3">Message Type 3</typeparam>
    /// <typeparam name="T4">Message Type 4</typeparam>
    public interface IBusiness<in T1, in T2, in T3, in T4> : IBusiness
    {
        /// <summary>
        /// Process T1 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T1 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T2 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T2 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T3 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T3 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T4 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T4 message, CancellationToken cancellationToken);
    }

    /// <inheritdoc />
    /// <summary>
    /// Defines a business instance for message processing
    /// </summary>
    /// <typeparam name="T1">Message Type 1</typeparam>
    /// <typeparam name="T2">Message Type 2</typeparam>
    /// <typeparam name="T3">Message Type 3</typeparam>
    /// <typeparam name="T4">Message Type 4</typeparam>
    /// <typeparam name="T5">Message Type 5</typeparam>
    public interface IBusiness<in T1, in T2, in T3, in T4, in T5> : IBusiness
    {
        /// <summary>
        /// Process T1 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T1 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T2 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T2 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T3 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T3 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T4 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T4 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T5 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        object Process(T5 message, CancellationToken cancellationToken);
    }
}
