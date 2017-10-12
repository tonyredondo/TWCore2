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
// ReSharper disable EventNeverSubscribedTo.Global

namespace TWCore.Messaging.Server
{
    /// <summary>
    /// General Message Queue Server Events
    /// </summary>
    public static class MQueueServerEvents
    {
        /// <summary>
        /// Events that fires when a request message is received
        /// </summary>
        public static event EventHandler<RequestReceivedEventArgs> RequestReceived;
        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        public static event EventHandler<ResponseReceivedEventArgs> ResponseReceived;
        /// <summary>
        /// Events that fires when a response message is sent
        /// </summary>
        public static event EventHandler<ResponseSentEventArgs> ResponseSent;
        /// <summary>
        /// Events that fires when a response message is about to be sent
        /// </summary>
        public static event EventHandler<ResponseSentEventArgs> BeforeSendResponse;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void FireRequestReceived(object sender, RequestReceivedEventArgs e) 
            => RequestReceived?.Invoke(sender, e);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void FireResponseReceived(object sender, ResponseReceivedEventArgs e)
            => ResponseReceived?.Invoke(sender, e);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void FireResponseSent(object sender, ResponseSentEventArgs e)
            => ResponseSent?.Invoke(sender, e);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void FireBeforeSendResponse(object sender, ResponseSentEventArgs e)
            => BeforeSendResponse?.Invoke(sender, e);
    }
}
