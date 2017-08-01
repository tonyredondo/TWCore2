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

namespace TWCore.Messaging.RawServer
{
    /// <summary>
    /// Event args for response sent event
    /// </summary>
    public class RawResponseSentEventArgs : EventArgs
    {
        /// <summary>
        /// Client name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Correlation Id
        /// </summary>
        public Guid CorrelationId { get; private set; }
        /// <summary>
        /// Response message received
        /// </summary>
        public byte[] Message { get; set; }

        /// <summary>
        /// Event args for response received event
        /// </summary>
        /// <param name="name">Client name</param>
        /// <param name="message">Response message received</param>
        /// <param name="correlationId">Correlation id</param>
        public RawResponseSentEventArgs(string name, byte[] message, Guid correlationId)
        {
            Name = name;
            Message = message;
            CorrelationId = correlationId;
        }
    }
}
