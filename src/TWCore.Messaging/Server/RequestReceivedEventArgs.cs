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
using System.Threading;
using TWCore.Collections;
using TWCore.Messaging.Configuration;

namespace TWCore.Messaging.Server
{
    /// <inheritdoc />
    /// <summary>
    /// Event args for request received event
    /// </summary>
    public class RequestReceivedEventArgs : EventArgs
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
        /// Request message
        /// </summary>
        public RequestMessage Request { get; private set; }
        /// <summary>
        /// Response message
        /// </summary>
        public ResponseMessage Response { get; private set; }
        /// <summary>
        /// Gets if the response is going to be sent.
        /// </summary>
        public bool SendResponse { get; private set; }
        /// <summary>
        /// Gets the timeout in seconds of the sender to wait for the response
        /// </summary>
        public int ProcessResponseTimeoutInSeconds { get; private set; }
        /// <summary>
        /// Gets the Cancellation Token when the time to process a response has been reached.
        /// </summary>
        public CancellationToken ProcessResponseTimeoutCancellationToken { get; private set; }
        /// <summary>
        /// Listener Metadata
        /// </summary>
        public KeyValueCollection Metadata { get; private set; } = new KeyValueCollection();


		/// <inheritdoc />
		/// <summary>
		/// Event args for request sent event
		/// </summary>
		/// <param name="name">Client name</param>
		/// <param name="sender">Sender queue</param>
		/// <param name="request">Request message</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RequestReceivedEventArgs(string name, MQConnection sender, RequestMessage request)
        {
            Name = name;
            Request = request;
            Sender = sender;
            SendResponse = request?.Header?.ResponseExpected ?? true;
            ProcessResponseTimeoutInSeconds = request?.Header?.ResponseTimeoutInSeconds ?? -1;
            Response = new ResponseMessage(request, null);
            var cTokenSource = new CancellationTokenSource();
            ProcessResponseTimeoutCancellationToken = cTokenSource.Token;
            if (ProcessResponseTimeoutInSeconds > 0)
                cTokenSource.CancelAfter(TimeSpan.FromSeconds(ProcessResponseTimeoutInSeconds));
        }
    }
}
