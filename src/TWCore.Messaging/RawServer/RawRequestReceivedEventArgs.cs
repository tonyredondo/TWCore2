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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TWCore.Collections;
using TWCore.Messaging.Configuration;

namespace TWCore.Messaging.RawServer
{
    /// <summary>
    /// Event args for raw request received event
    /// </summary>
    public class RawRequestReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Client name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Sender queue
        /// </summary>
        public MQConnection Sender { get; private set; }
        /// <summary>
        /// Response queues
        /// </summary>
        public List<MQConnection> ResponseQueues { get; private set; } = new List<MQConnection>();
        /// <summary>
        /// Correlation Id
        /// </summary>
        public Guid CorrelationId { get; private set; }
        /// <summary>
        /// Request message
        /// </summary>
        public byte[] Request { get; private set; }
        /// <summary>
        /// Response message
        /// </summary>
        public byte[] Response { get; set; }
        /// <summary>
        /// Gets if the response is going to be sent.
        /// </summary>
        public bool SendResponse { get; private set; } = true;
        /// <summary>
        /// Listener Metadata
        /// </summary>
        public KeyValueCollection Metadata { get; private set; } = new KeyValueCollection();

		/// <summary>
		/// Event args for request sent event
		/// </summary>
		/// <param name="name">Client name</param>
		/// <param name="sender">Sender queue</param>
		/// <param name="request">Request message</param>
		/// <param name="correlationId">Correlation Id</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RawRequestReceivedEventArgs(string name, MQConnection sender, byte[] request, Guid correlationId)
        {
            Name = name;
            Request = request;
            CorrelationId = correlationId;
            Sender = sender;
        }
    }
}
