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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Threading;

// ReSharper disable EventNeverSubscribedTo.Global

namespace TWCore.Messaging.RawClient
{
    /// <summary>
    /// General Message Queue Raw Client Events
    /// </summary>
    public static class MQueueRawClientEvents
    {
        /// <summary>
        /// Events that fires when a request message is sent
        /// </summary>
        //public static event AsyncEventHandler<RawMessageEventArgs> OnRequestSent;
        public static AsyncEvent<RawMessageEventArgs> OnRequestSent { get; set; }
        
        /// <summary>
        /// Events that fires when a request message is about to be sent
        /// </summary>
        //public static event AsyncEventHandler<RawMessageEventArgs> OnBeforeSendRequest;
        public static AsyncEvent<RawMessageEventArgs> OnBeforeSendRequest { get; set; }

        /// <summary>
        /// Events that fires when a response message is received
        /// </summary>
        //public static event AsyncEventHandler<RawMessageEventArgs> OnResponseReceived;
        public static AsyncEvent<RawMessageEventArgs> OnResponseReceived { get; set; }

        //
		/*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Task FireOnRequestSentAsync(object sender, RawMessageEventArgs e)
            => OnRequestSent?.InvokeAsync(sender, e) ?? Task.CompletedTask;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Task FireOnBeforeSendRequestAsync(object sender, RawMessageEventArgs e)
            => OnBeforeSendRequest?.InvokeAsync(sender, e) ?? Task.CompletedTask;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Task FireOnResponseReceivedAsync(object sender, RawMessageEventArgs e)
            => OnResponseReceived?.InvokeAsync(sender, e) ?? Task.CompletedTask;
        */
    }
}
