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
using System.Threading;
using TWCore.Diagnostics.Status;
// ReSharper disable NotAccessedField.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Messaging.RawServer
{
	/// <summary>
	/// Message queue raw server counters
	/// </summary>
	[StatusName("Counters")]
	public class MQRawServerCounters
	{
		private Timer _timerThirtyMinutes;
        private long _currentMessages;
        private long _peakCurrentMessages;
        private long _lastThirtyMinutesMessages;
        private long _peakLastThirtyMinutesMessages;
        private long _totalMessagesReceived;
        private long _totalMessagesProccesed;
        private long _totalExceptions;
        private long _totalReceivingBytes;

        #region Messages On Process
        /// <summary>
        /// Number of Messages on process
        /// </summary>
        public long CurrentMessages => _currentMessages;
        /// <summary>
        /// Peak value of number of messages on process
        /// </summary>
        public long PeakCurrentMessages => _peakCurrentMessages;
		/// <summary>
		/// Date and time of the peak value of number of message on process
		/// </summary>
		public DateTime PeakCurrentMessagesLastDate { get; private set; }

        /// <summary>
        /// Number of messages processed on the last thirty minutes
        /// </summary>
        public long LastThirtyMinutesMessages => _lastThirtyMinutesMessages;
        /// <summary>
        /// Peak value of number of message processed on the last thirty minutes
        /// </summary>
        public long PeakLastThirtyMinutesMessages => _peakLastThirtyMinutesMessages;
        /// <summary>
        /// Date and time of the peak value of number of message processed on the last thirty minutes
        /// </summary>
        public DateTime PeakLastThirtyMinutesMessagesLastDate { get; private set; }
        #endregion

		#region Properties
		/// <summary>
		/// Date and time of the last received message
		/// </summary>
		public DateTime LastMessageDateTime { get; private set; }
		/// <summary>
		/// Date and time of the last process of a message
		/// </summary>
		public DateTime LastProcessingDateTime { get; private set; }

        /// <summary>
        /// Number of received messages
        /// </summary>
        public long TotalMessagesReceived => _totalMessagesReceived;
        /// <summary>
        /// Number of processed messages
        /// </summary>
        public long TotalMessagesProccesed => _totalMessagesProccesed;
        /// <summary>
        /// Number of exceptions
        /// </summary>
        public long TotalExceptions => _totalExceptions;
        /// <summary>
        /// Total receiving time
        /// </summary>
        public long TotalReceivingBytes => _totalReceivingBytes;
        #endregion

        #region .ctor
        /// <summary>
        /// Message queue server counters
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MQRawServerCounters()
		{
			_timerThirtyMinutes = new Timer(state =>
			{
                Interlocked.Exchange(ref _lastThirtyMinutesMessages, Interlocked.Read(ref _currentMessages));
                Interlocked.Exchange(ref _peakLastThirtyMinutesMessages, Interlocked.Read(ref _currentMessages));
				PeakLastThirtyMinutesMessagesLastDate = LastMessageDateTime;
			}, this, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));

			Core.Status.Attach(collection =>
			{
				collection.SortValues = false;

				#region Messages On Process
				collection.Add("Current messages on process",
					new StatusItemValueItem("Quantity", CurrentMessages, true),
					new StatusItemValueItem("Peak Quantity", PeakCurrentMessages, true),
					new StatusItemValueItem("Peak DateTime", PeakCurrentMessagesLastDate));

				collection.Add("Last thirty minutes processed messages",
					new StatusItemValueItem("Quantity", LastThirtyMinutesMessages, true),
					new StatusItemValueItem("Peak Quantity", PeakLastThirtyMinutesMessages, true),
					new StatusItemValueItem("Peak DateTime", PeakLastThirtyMinutesMessagesLastDate));
				#endregion

				collection.Add("Last DateTime",
					new StatusItemValueItem("Message Received", LastMessageDateTime),
					new StatusItemValueItem("Message Processed", LastProcessingDateTime));

				collection.Add("Totals",
					new StatusItemValueItem("Message Received", TotalMessagesReceived, true),
					new StatusItemValueItem("Message Processed", TotalMessagesProccesed, true),
					new StatusItemValueItem("Exceptions", TotalExceptions, true),
					new StatusItemValueItem("Receiving Bytes (MB)", TotalReceivingBytes.ToMegabytes(), true));
			});
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Increments the total receiving time
		/// </summary>
		/// <param name="increment">Increment value</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long IncrementTotalReceivingBytes(long increment)
            => Interlocked.Add(ref _totalReceivingBytes, increment);
        /// <summary>
        /// Increments the total exceptions number
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long IncrementTotalExceptions()
            => Interlocked.Increment(ref _totalExceptions);
        /// <summary>
        /// Increments the total exceptions number
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long IncrementTotalMessagesProccesed()
            => Interlocked.Increment(ref _totalMessagesProccesed);
		/// <summary>
		/// Increments the messages
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long IncrementMessages()
		{
            var cM = Interlocked.Increment(ref _currentMessages);
            var l30MM = Interlocked.Increment(ref _lastThirtyMinutesMessages);
		    Interlocked.Increment(ref _totalMessagesReceived);
            LastMessageDateTime = Core.Now;
			if (cM >= Interlocked.Read(ref _peakCurrentMessages))
			{
                Interlocked.Exchange(ref _peakCurrentMessages, cM);
				PeakCurrentMessagesLastDate = LastMessageDateTime;
			}
			if (l30MM >= Interlocked.Read(ref _peakLastThirtyMinutesMessages))
			{
                Interlocked.Exchange(ref _peakLastThirtyMinutesMessages, l30MM);
				PeakLastThirtyMinutesMessagesLastDate = LastMessageDateTime;
			}
            return cM;
		}
        /// <summary>
        /// Decrement the current messages
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long DecrementMessages()
            => Interlocked.Decrement(ref _currentMessages);
		#endregion
	}
}
