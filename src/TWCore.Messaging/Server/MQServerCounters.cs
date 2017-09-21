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
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Messaging.Server
{
    /// <summary>
    /// Message queue server counters
    /// </summary>
    public class MQServerCounters
    {
        private readonly object _locker = new object();
        private Timer _timer;

        #region Properties
        /// <summary>
        /// Number of Messages on process
        /// </summary>
        [StatusProperty("Number of Messages on process", true)]
        public long CurrentMessages { get; private set; }
        /// <summary>
        /// Peak value of number of messages on process
        /// </summary>
        [StatusProperty("Peak value of number of messages on process", true)]
        public long PeakCurrentMessages { get; private set; }
        /// <summary>
        /// Date and time of the peak value of number of message on process
        /// </summary>
        [StatusProperty("Date and time of the peak value of number of message on process")]
        public DateTime PeakCurrentMessagesLastDate { get; private set; }

        /// <summary>
        /// Number of messages processed on the last minute
        /// </summary>
        [StatusProperty("Number of messages processed on the last minute", true)]
        public long LastMinuteMessages { get; private set; }
        /// <summary>
        /// Peak value of number of message processed on the last minute
        /// </summary>
        [StatusProperty("Peak value of number of message processed on the last minute", true)]
        public long PeakLastMinuteMessages { get; private set; }
        /// <summary>
        /// Date and time of the peak value of number of message processed on the last minute
        /// </summary>
        [StatusProperty("Date and time of the peak value of number of message processed on the last minute")]
        public DateTime PeakLastMinuteMessagesLastDate { get; private set; }

        /// <summary>
        /// Number of current active processing threads
        /// </summary>
        [StatusProperty("Number of current active processing threads", true)]
        public long CurrentProcessingThreads { get; private set; }
        /// <summary>
        /// Peak value of number of active processing threads
        /// </summary>
        [StatusProperty("Peak value of number of active processing threads", true)]
        public long PeakCurrentProcessingThreads { get; private set; }
        /// <summary>
        /// Date and time of the peak value of number of active processing threads
        /// </summary>
        [StatusProperty("Date and time of the peak value of number of active processing threads")]
        public DateTime PeakCurrentProcessingThreadsLastDate { get; private set; }

        /// <summary>
        /// Number of active processing threads on the last minute
        /// </summary>
        [StatusProperty("Number of active processing threads on the last minute", true)]
        public long LastMinuteProcessingThreads { get; private set; }
        /// <summary>
        /// Peak value of the number of active processing threads on the last minute
        /// </summary>
        [StatusProperty("Peak value of the number of active processing threads on the last minute", true)]
        public long PeakLastMinuteProcessingThreads { get; private set; }
        /// <summary>
        /// Date and time of the peak value of number of active processing threads on the last minute
        /// </summary>
        [StatusProperty("Date and time of the peak value of number of active processing threads on the last minute")]
        public DateTime PeakLastMinuteProcessingThreadsLastDate { get; private set; }

        /// <summary>
        /// Date and time of the last received message
        /// </summary>
        [StatusProperty("Date and time of the last received message")]
        public DateTime LastMessageDateTime { get; private set; }
        /// <summary>
        /// Date and time of the last process of a message
        /// </summary>
        [StatusProperty("Date and time of the last process of a message")]
        public DateTime LastProcessingDateTime { get; private set; }

        /// <summary>
        /// Number of received messages
        /// </summary>
        [StatusProperty("Number of received messages", StatusItemValueStatus.Green, true)]
        public long TotalMessagesReceived { get; private set; }
        /// <summary>
        /// Number of processed messages
        /// </summary>
        [StatusProperty("Number of processed messages", StatusItemValueStatus.Green, true)]
        public long TotalMessagesProccesed { get; private set; }
        /// <summary>
        /// Number of exceptions
        /// </summary>
        [StatusProperty("Number of exceptions", StatusItemValueStatus.Red, true)]
        public long TotalExceptions { get; private set; }
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
		public MQServerCounters()
        {
            _timer = new Timer(state =>
            {
                lock(_locker)
                {
                    LastMinuteMessages = CurrentMessages;
                    PeakLastMinuteMessages = CurrentMessages;
                    PeakLastMinuteMessagesLastDate = LastMessageDateTime;

                    LastMinuteProcessingThreads = CurrentProcessingThreads;
                    PeakLastMinuteProcessingThreads = CurrentProcessingThreads;
                    PeakLastMinuteProcessingThreadsLastDate = LastProcessingDateTime;
                }
            }, this, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            Core.Status.Attach(collection =>
            {
                collection.Add("Total receiving time", TimeSpan.FromMilliseconds(TotalReceivingTime));
            });
        }
		#endregion

		#region Public Methods
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
		/// Increments the total exceptions number
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementTotalExceptions()
        {
            lock (_locker)
                TotalExceptions++;
        }
		/// <summary>
		/// Increments the total exceptions number
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementTotalMessagesProccesed()
        {
            lock (_locker)
                TotalMessagesProccesed++;
        }
		/// <summary>
		/// Increments the messages
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementMessages()
        {
            lock (_locker)
            {
                CurrentMessages++;
                TotalMessagesReceived++;
                LastMinuteMessages++;
                LastMessageDateTime = Core.Now;
                if (CurrentMessages >= PeakCurrentMessages)
                {
                    PeakCurrentMessages = CurrentMessages;
                    PeakCurrentMessagesLastDate = LastMessageDateTime;
                }
                if (LastMinuteMessages >= PeakLastMinuteMessages)
                {
                    PeakLastMinuteMessages = LastMinuteMessages;
                    PeakLastMinuteMessagesLastDate = LastMessageDateTime;
                }
            }
        }
		/// <summary>
		/// Decrement the current messages
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DecrementMessages()
        {
            lock(_locker)
                CurrentMessages--;
        }
		/// <summary>
		/// Increment the processing threads
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementProcessingThreads()
        {
            lock (_locker)
            {
                CurrentProcessingThreads++;
                LastMinuteProcessingThreads++;
                LastProcessingDateTime = Core.Now;
                if (CurrentProcessingThreads >= PeakCurrentProcessingThreads)
                {
                    PeakCurrentProcessingThreads = CurrentProcessingThreads;
                    PeakCurrentProcessingThreadsLastDate = LastProcessingDateTime;
                }
                if (LastMinuteProcessingThreads >= PeakLastMinuteProcessingThreads)
                {
                    PeakLastMinuteProcessingThreads = LastMinuteProcessingThreads;
                    PeakLastMinuteProcessingThreadsLastDate = LastMessageDateTime;
                }
            }
        }
		/// <summary>
		/// Decrement the current processing threads
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DecrementProcessingThreads()
        {
            lock (_locker)
                CurrentProcessingThreads--;
        }
        #endregion
    }
}
