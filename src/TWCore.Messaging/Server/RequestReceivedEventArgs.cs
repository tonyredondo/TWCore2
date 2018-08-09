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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using TWCore.Messaging.Configuration;
using TWCore.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Messaging.Server
{
    /// <inheritdoc />
    /// <summary>
    /// Event args for request received event
    /// </summary>
    public class RequestReceivedEventArgs : EventArgs
    {
        private ISerializer _senderSerializer;

        /// <summary>
        /// Client name
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Sender queue
        /// </summary>
        public MQConnection Sender { get; }
        /// <summary>
        /// Response queues
        /// </summary>
        public List<MQConnection> ResponseQueues { get; } = new List<MQConnection>();
        /// <summary>
        /// Request message
        /// </summary>
        public RequestMessage Request { get; }
        /// <summary>
        /// Response message
        /// </summary>
        public ResponseMessage Response { get; }
        /// <summary>
        /// Gets if the response is going to be sent.
        /// </summary>
        public bool SendResponse { get; }
        /// <summary>
        /// Gets the timeout in seconds of the sender to wait for the response
        /// </summary>
        public int ProcessResponseTimeoutInSeconds { get; }
        /// <summary>
        /// Gets the Cancellation Token when the time to process a response has been reached.
        /// </summary>
        public CancellationToken ProcessResponseTimeoutCancellationToken { get; }
        /// <summary>
        /// Listener Metadata
        /// </summary>
        public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>();
        /// <summary>
        /// Message Length
        /// </summary>
        public int MessageLength { get; }


		/// <inheritdoc />
		/// <summary>
		/// Event args for request sent event
		/// </summary>
		/// <param name="name">Client name</param>
		/// <param name="sender">Sender queue</param>
		/// <param name="request">Request message</param>
		/// <param name="messageLength">Message length</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RequestReceivedEventArgs(string name, MQConnection sender, RequestMessage request, int messageLength, ISerializer senderSerializer)
        {
            _senderSerializer = senderSerializer;
            Name = name;
            Request = request;
            Sender = sender;
	        MessageLength = messageLength;
            SendResponse = request?.Header?.ResponseExpected ?? true;
            ProcessResponseTimeoutInSeconds = request?.Header?.ResponseTimeoutInSeconds ?? -1;
            Response = new ResponseMessage(request, null);
            if (ProcessResponseTimeoutInSeconds > 0)
            {
                var cTokenSource = new CancellationTokenSource();
                ProcessResponseTimeoutCancellationToken = cTokenSource.Token;
                cTokenSource.CancelAfter(TimeSpan.FromSeconds(ProcessResponseTimeoutInSeconds));
            }
            else
                ProcessResponseTimeoutCancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// Set response body
        /// </summary>
        /// <param name="obj">Response body</param>
        public void SetResponseBody(object obj)
        {
            if (obj != null)
                Response.Body = _senderSerializer.GetSerializedObject(obj);
        }
    }
}
