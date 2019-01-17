﻿/*
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

namespace TWCore.Net.RPC.Client
{
    /// <inheritdoc />
    /// <summary>
    /// Event args for the received event data from the server
    /// </summary>
    public class EventDataEventArgs : EventArgs
    {
        /// <summary>
        /// Service Name
        /// </summary>
        public string ServiceName { get; private set; }
        /// <summary>
        /// Event name that got fired
        /// </summary>
        public string EventName { get; private set; }
        /// <summary>
        /// Event arguments
        /// </summary>
        public EventArgs EventArgs { get;  private set;}

        /// <inheritdoc />
        /// <summary>
        /// Event args for the received event data from the server
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventDataEventArgs()
        {
        }


        #region Statics
        /// <summary>
        ///  Retrieves an Event data event args from the Pool
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="eventName">Event name that got fired</param>
        /// <param name="eventArgs">Event arguments</param>
        /// <returns>Event data event args value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EventDataEventArgs Retrieve(string serviceName, string eventName, EventArgs eventArgs)
        {
            var value = ReferencePool<EventDataEventArgs>.Shared.New();
            value.ServiceName = serviceName;
            value.EventName = eventName;
            value.EventArgs = eventArgs;
            return value;
        }
        /// <summary>
        /// Stores an Event data event args in the Pool.
        /// </summary>
        /// <param name="value">Event data event args value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(EventDataEventArgs value)
        {
            if (value is null) return;
            value.ServiceName = null;
            value.EventName = null;
            value.EventArgs = null;
            ReferencePool<EventDataEventArgs>.Shared.Store(value);
        }
        #endregion
    }
}
