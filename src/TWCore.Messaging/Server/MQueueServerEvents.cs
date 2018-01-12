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
using System.Threading.Tasks;
using TWCore.Threading;
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
        //public static event EventHandler<RequestReceivedEventArgs> RequestReceived;
        public static AsyncEvent<RequestReceivedEventArgs> RequestReceived { get; set; }
        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        //public static event EventHandler<ResponseReceivedEventArgs> ResponseReceived;
        public static AsyncEvent<ResponseReceivedEventArgs> ResponseReceived { get; set; }
        /// <summary>
        /// Events that fires when a response message is sent
        /// </summary>
        //public static event EventHandler<ResponseSentEventArgs> ResponseSent;
        public static AsyncEvent<ResponseSentEventArgs> ResponseSent { get; set; }
        /// <summary>
        /// Events that fires when a response message is about to be sent
        /// </summary>
        //public static event EventHandler<ResponseSentEventArgs> BeforeSendResponse;
        public static AsyncEvent<ResponseSentEventArgs> BeforeSendResponse { get; set; }


        //
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Task FireRequestReceivedAsync(object sender, RequestReceivedEventArgs e) 
            => RequestReceived?.InvokeAsync(sender, e) ?? Task.CompletedTask;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Task FireResponseReceivedAsync(object sender, ResponseReceivedEventArgs e)
            => ResponseReceived?.InvokeAsync(sender, e) ?? Task.CompletedTask;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Task FireResponseSentAsync(object sender, ResponseSentEventArgs e)
            => ResponseSent?.InvokeAsync(sender, e) ?? Task.CompletedTask;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Task FireBeforeSendResponseAsync(object sender, ResponseSentEventArgs e)
            => BeforeSendResponse?.InvokeAsync(sender, e) ?? Task.CompletedTask;
    }
}
