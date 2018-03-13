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

using TWCore.Threading;
// ReSharper disable EventNeverSubscribedTo.Global

namespace TWCore.Messaging.RawServer
{
    /// <summary>
    /// General Message Queue Server Events
    /// </summary>
    public static class MQueueRawServerEvents
    {
        /// <summary>
        /// Events that fires when a request message is received
        /// </summary>
        public static AsyncEvent<RawRequestReceivedEventArgs> RequestReceived { get; set; }
        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        public static AsyncEvent<RawResponseReceivedEventArgs> ResponseReceived { get; set; }
        /// <summary>
        /// Events that fires when a response message is sent
        /// </summary>
        public static AsyncEvent<RawResponseSentEventArgs> ResponseSent { get; set; }
        /// <summary>
        /// Events that fires when a response message is about to be sent
        /// </summary>
        public static AsyncEvent<RawResponseSentEventArgs> BeforeSendResponse { get; set; }
    }
}
