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
    /// Message queue body null exception
    /// </summary>
    public class MessageQueueBodyNullException : MessageQueueException
    {
        /// <inheritdoc />
        /// <summary>
        /// Message queue body null exception
        /// </summary>
        /// <param name="innerException">Inner exception</param>
        public MessageQueueBodyNullException(Exception innerException = null) : base("Message has a null body.", innerException) { }
        /// <inheritdoc />
        /// <summary>
        /// Message queue body null exception
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="innerException">Inner exception</param>
        public MessageQueueBodyNullException(string message, Exception innerException = null) : base(message, innerException) { }
    }
}
