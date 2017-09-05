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
}
