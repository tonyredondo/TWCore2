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
using System.Runtime.CompilerServices;
using TWCore.Collections;

namespace TWCore.Messaging.RawServer
{
    /// <summary>
    /// Event args for raw response received event
    /// </summary>
    public class RawResponseReceivedEventArgs : EventArgs
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
        public SubArray<byte> Message { get; set; }
        /// <summary>
        /// Listener Metadata
        /// </summary>
        public KeyValueCollection Metadata { get; private set; } = new KeyValueCollection();


		/// <summary>
		/// Event args for response received event
		/// </summary>
		/// <param name="name">Client name</param>
		/// <param name="message">Response message received</param>
		/// <param name="correlationId">Correlation Id</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RawResponseReceivedEventArgs(string name, SubArray<byte> message, Guid correlationId)
        {
            Name = name;
            Message = message;
            CorrelationId = correlationId;
        }
    }
}
