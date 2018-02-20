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
// ReSharper disable UnusedMember.Global

namespace TWCore.Messaging.Exceptions
{
    /// <inheritdoc />
    /// <summary>
    /// Message queue timeout exception
    /// </summary>
    public class MessageQueueTimeoutException : MessageQueueException
    {
        /// <inheritdoc />
        /// <summary>
        /// Message queue timeout exception
        /// </summary>
        /// <param name="innerException">Inner exception</param>
        public MessageQueueTimeoutException(Exception innerException = null) : base("Timeout for response on Message Queue was reached.", innerException) { }
        /// <inheritdoc />
        /// <summary>
        /// Message queue timeout exception
        /// </summary>
        /// <param name="time">Time waiting for the message</param>
        /// <param name="innerException">Inner exception</param>
        public MessageQueueTimeoutException(TimeSpan time, Exception innerException = null) : base(string.Format("Timeout of {0}sec for response on Message Queue was reached.", time.TotalSeconds), innerException) { }
        /// <inheritdoc />
        /// <summary>
        /// Message queue timeout exception
        /// </summary>
        /// <param name="time">Time waiting for the message</param>
        /// <param name="correlationId">Message correlation id</param>
        /// <param name="innerException">Inner exception</param>
        public MessageQueueTimeoutException(TimeSpan time, string correlationId, Exception innerException = null) : base(string.Format("Timeout of {0}sec for response with CorrelationId '{1}' on Message Queue was reached.", time.TotalSeconds, correlationId), innerException) { }
        /// <inheritdoc />
        /// <summary>
        /// Message queue timeout exception
        /// </summary>
        /// <param name="correlationId">Message correlation id</param>
        /// <param name="innerException">Inner exception</param>
        public MessageQueueTimeoutException(string correlationId, Exception innerException = null) : base(string.Format("Timeout for response with CorrelationId '{0}' on Message Queue was reached.", correlationId), innerException) { }
    }
}
