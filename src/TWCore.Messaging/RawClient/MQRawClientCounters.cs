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
using System.Threading;
using TWCore.Diagnostics.Status;
// ReSharper disable NotAccessedField.Local

namespace TWCore.Messaging.RawClient
{
    /// <summary>
    /// Message queue client counters
    /// </summary>
    public class MQRawClientCounters
    {
        readonly object locker = new object();
        Timer timer;

        #region Properties
        /// <summary>
        /// Number of messages Sent
        /// </summary>
        [StatusProperty("Number of messages sent")]
        public long MessagesSent { get; private set; }
        /// <summary>
        /// Number of messages sent in the last minute
        /// </summary>
        [StatusProperty("Number of messages sent in the last minute")]
        public long LastMinuteMessagesSent { get; private set; }

        /// <summary>
        /// Number of messages received
        /// </summary>
        [StatusProperty("Number of messages received")]
        public long MessagesReceived { get; private set; }
        /// <summary>
        /// Number of messages received in the last minute
        /// </summary>
        [StatusProperty("Number of messages received in the last minute")]
        public long LastMinuteMessagesReceived { get; private set; }
        
        /// <summary>
        /// Total bytes sent
        /// </summary>
        [StatusProperty("Total bytes sent")]
        public double TotalBytesSent { get; private set; }
        /// <summary>
        /// Total bytes received
        /// </summary>
        [StatusProperty("Total bytes received")]
        public double TotalBytesReceived { get; private set; }
		#endregion

		#region .ctor
		/// <summary>
		/// Message queue server counters
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MQRawClientCounters()
        {
            timer = new Timer(state =>
            {
                lock(locker)
                {
                    LastMinuteMessagesSent = MessagesSent;
                    LastMinuteMessagesReceived = MessagesReceived;
                }
            }, this, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Increments the total bytes sent
        /// </summary>
        /// <param name="increment">Increment value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementTotalBytesSent(double increment)
        {
            lock (locker)
                TotalBytesSent += increment;
        }
        /// <summary>
        /// Increments the total bytes received
        /// </summary>
        /// <param name="increment">Increment value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementTotalBytesReceived(double increment)
        {
            lock (locker)
                TotalBytesReceived += increment;
        }
        /// <summary>
        /// Increments the messages sent
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementMessagesSent()
        {
            lock (locker)
            {
                MessagesSent++;
                LastMinuteMessagesSent++;
            }
        }
        /// <summary>
        /// Increment the message received
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementMessagesReceived()
        {
            lock (locker)
            {
                MessagesReceived++;
                LastMinuteMessagesReceived++;
            }
        }
        #endregion
    }
}
