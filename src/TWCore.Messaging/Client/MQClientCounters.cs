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
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.Client
{
    /// <summary>
    /// Message queue client counters
    /// </summary>
    public class MQClientCounters
    {
        private readonly object _locker = new object();
        private Timer _timer;

        #region Properties
        /// <summary>
        /// Number of messages Sent
        /// </summary>
        [StatusProperty("Number of messages sent", true)]
        public long MessagesSent { get; private set; }
        /// <summary>
        /// Number of messages sent in the last minute
        /// </summary>
        [StatusProperty("Number of messages sent in the last minute", true)]
        public long LastMinuteMessagesSent { get; private set; }

        /// <summary>
        /// Number of messages received
        /// </summary>
        [StatusProperty("Number of messages received", true)]
        public long MessagesReceived { get; private set; }
        /// <summary>
        /// Number of messages received in the last minute
        /// </summary>
        [StatusProperty("Number of messages received in the last minute", true)]
        public long LastMinuteMessagesReceived { get; private set; }
        
        /// <summary>
        /// Total network time
        /// </summary>
        public double TotalNetworkTime { get; private set; }
        /// <summary>
        /// Total receiving time
        /// </summary>
        public double TotalReceivingTime { get; private set; }
		#endregion

		#region .ctor
		/// <summary>
		/// Message queue server counters
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MQClientCounters()
        {
            _timer = new Timer(state =>
            {
                lock(_locker)
                {
                    LastMinuteMessagesSent = MessagesSent;
                    LastMinuteMessagesReceived = MessagesReceived;
                }
            }, this, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            Core.Status.Attach(collection =>
            {
                collection.Add("Total network time", TimeSpan.FromMilliseconds(TotalNetworkTime));
                collection.Add("Total receiving time", TimeSpan.FromMilliseconds(TotalReceivingTime));
            });
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Increments the total network time
        /// </summary>
        /// <param name="increment">Increment value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementTotalNetworkTime(TimeSpan increment)
        {
            lock (_locker)
                TotalNetworkTime += increment.TotalMilliseconds;
        }
        /// <summary>
        /// Increments the receiving time
        /// </summary>
        /// <param name="increment">Increment value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementReceivingTime(TimeSpan increment)
        {
            lock (_locker)
                TotalReceivingTime += increment.TotalMilliseconds;
        }
        /// <summary>
        /// Increments the messages sent
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementMessagesSent()
        {
            lock (_locker)
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
            lock (_locker)
            {
                MessagesReceived++;
                LastMinuteMessagesReceived++;
            }
        }
        #endregion
    }
}
