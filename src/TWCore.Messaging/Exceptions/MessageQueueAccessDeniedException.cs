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
    /// Message queue access denied exception
    /// </summary>
    public class MessageQueueAccessDeniedException : MessageQueueException
    {
        /// <inheritdoc />
        /// <summary>
        /// Message queue access denied exception
        /// </summary>
        /// <param name="innerException">Inner exception</param>
        public MessageQueueAccessDeniedException(Exception innerException = null) : base("Access Denied on Message Queue", innerException) { }
        /// <inheritdoc />
        /// <summary>
        /// Message queue access denied exception
        /// </summary>
        /// <param name="queueUri">Queue Uri (QueuePath + QueueName)</param>
        /// <param name="innerException">Inner exception</param>
        public MessageQueueAccessDeniedException(string queueUri, Exception innerException = null) : base(string.Format("Access Denied on Message Queue '{0}'", queueUri), innerException) { }
    }
}
