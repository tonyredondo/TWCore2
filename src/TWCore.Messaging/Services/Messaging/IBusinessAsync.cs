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
// ReSharper disable CheckNamespace

namespace TWCore.Services.Messaging
{
    /// <inheritdoc />
    /// <summary>
    /// Defines a business instance for message processing
    /// </summary>
    public interface IBusinessAsync : IDisposable
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
        Task<object> ProcessAsync(object message, CancellationToken cancellationToken);
    }

    /// <inheritdoc />
    /// <summary>
    /// Defines a business instance for message processing
    /// </summary>
    /// <typeparam name="T">Message Type</typeparam>
    public interface IBusinessAsync<in T> : IBusinessAsync
    {
        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T message, CancellationToken cancellationToken);
    }

    /// <inheritdoc />
    /// <summary>
    /// Defines a business instance for message processing
    /// </summary>
    /// <typeparam name="T1">Message Type 1</typeparam>
    /// <typeparam name="T2">Message Type 2</typeparam>
    public interface IBusinessAsync<in T1, in T2> : IBusinessAsync
    {
        /// <summary>
        /// Process T1 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T1 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T2 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T2 message, CancellationToken cancellationToken);
    }

    /// <inheritdoc />
    /// <summary>
    /// Defines a business instance for message processing
    /// </summary>
    /// <typeparam name="T1">Message Type 1</typeparam>
    /// <typeparam name="T2">Message Type 2</typeparam>
    /// <typeparam name="T3">Message Type 3</typeparam>
    public interface IBusinessAsync<in T1, in T2, in T3> : IBusinessAsync
    {
        /// <summary>
        /// Process T1 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T1 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T2 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T2 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T3 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T3 message, CancellationToken cancellationToken);
    }

    /// <inheritdoc />
    /// <summary>
    /// Defines a business instance for message processing
    /// </summary>
    /// <typeparam name="T1">Message Type 1</typeparam>
    /// <typeparam name="T2">Message Type 2</typeparam>
    /// <typeparam name="T3">Message Type 3</typeparam>
    /// <typeparam name="T4">Message Type 4</typeparam>
    public interface IBusinessAsync<in T1, in T2, in T3, in T4> : IBusinessAsync
    {
        /// <summary>
        /// Process T1 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T1 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T2 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T2 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T3 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T3 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T4 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T4 message, CancellationToken cancellationToken);
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
    public interface IBusinessAsync<in T1, in T2, in T3, in T4, in T5> : IBusinessAsync
    {
        /// <summary>
        /// Process T1 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T1 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T2 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T2 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T3 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T3 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T4 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T4 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T5 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T5 message, CancellationToken cancellationToken);
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
    /// <typeparam name="T6">Message Type 6</typeparam>
    public interface IBusinessAsync<in T1, in T2, in T3, in T4, in T5, in T6> : IBusinessAsync
    {
        /// <summary>
        /// Process T1 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T1 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T2 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T2 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T3 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T3 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T4 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T4 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T5 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T5 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T6 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T6 message, CancellationToken cancellationToken);
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
    /// <typeparam name="T6">Message Type 6</typeparam>
    /// <typeparam name="T7">Message Type 7</typeparam>
    public interface IBusinessAsync<in T1, in T2, in T3, in T4, in T5, in T6, in T7> : IBusinessAsync
    {
        /// <summary>
        /// Process T1 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T1 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T2 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T2 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T3 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T3 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T4 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T4 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T5 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T5 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T6 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T6 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T7 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T7 message, CancellationToken cancellationToken);
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
    /// <typeparam name="T6">Message Type 6</typeparam>
    /// <typeparam name="T7">Message Type 7</typeparam>
    /// <typeparam name="T8">Message Type 8</typeparam>
    public interface IBusinessAsync<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8> : IBusinessAsync
    {
        /// <summary>
        /// Process T1 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T1 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T2 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T2 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T3 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T3 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T4 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T4 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T5 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T5 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T6 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T6 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T7 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T7 message, CancellationToken cancellationToken);
        /// <summary>
        /// Process T8 message
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="cancellationToken">Cancellation token for process message timeout</param>
        /// <returns>Process result</returns>
        Task<object> ProcessAsync(T8 message, CancellationToken cancellationToken);
    }

}
