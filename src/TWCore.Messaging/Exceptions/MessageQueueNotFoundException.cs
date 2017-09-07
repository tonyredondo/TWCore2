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
// ReSharper disable UnusedMember.Global

namespace TWCore.Messaging.Exceptions
{
    /// <inheritdoc />
    /// <summary>
    /// Message queue not found exception
    /// </summary>
    public class MessageQueueNotFoundException : MessageQueueException
    {
        /// <inheritdoc />
        /// <summary>
        /// Message queue not found exception
        /// </summary>
        /// <param name="innerException">Inner exception</param>
        public MessageQueueNotFoundException(Exception innerException = null) : base("Message Queue was not found.", innerException) { }
        /// <inheritdoc />
        /// <summary>
        /// Message queue not found exception
        /// </summary>
        /// <param name="queueUri">Queue Uri (QueuePath + QueueName)</param>
        /// <param name="innerException">Inner exception</param>
        public MessageQueueNotFoundException(string queueUri, Exception innerException = null) : base(string.Format("Message Queue '{0}' was not found.", queueUri), innerException) { }
    }
}
