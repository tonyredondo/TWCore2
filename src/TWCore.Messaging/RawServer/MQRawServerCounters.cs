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
		private Timer _timerTenMinutes;
		private Timer _timerTwentyMinutes;
		private Timer _timerThirtyMinutes;
        private long _currentMessages;
        private long _peakCurrentMessages;
        private long _lastTenMinutesMessages;
        private long _peakLastTenMinutesMessages;
        private long _lastTwentyMinutesMessages;
        private long _peakLastTwentyMinutesMessages;
        private long _lastThirtyMinutesMessages;
        private long _peakLastThirtyMinutesMessages;
        private long _currentProcessingThreads;
        private long _peakCurrentProcessingThreads;
        private long _lastTenMinutesProcessingThreads;
        private long _peakLastTenMinutesProcessingThreads;
        private long _lastTwentyMinutesProcessingThreads;
        private long _peakLastTwentyMinutesProcessingThreads;
        private long _lastThirtyMinutesProcessingThreads;
        private long _peakLastThirtyMinutesProcessingThreads;
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
        /// Number of messages processed on the last ten minutes
        /// </summary>
        public long LastTenMinutesMessages => _lastTenMinutesMessages;
        /// <summary>
        /// Peak value of number of message processed on the last ten minutes
        /// </summary>
        public long PeakLastTenMinutesMessages => _peakLastTenMinutesMessages;
		/// <summary>
		/// Date and time of the peak value of number of message processed on the last ten minutes
		/// </summary>
		public DateTime PeakLastTenMinutesMessagesLastDate { get; private set; }

        /// <summary>
        /// Number of messages processed on the last twenty minutes
        /// </summary>
        public long LastTwentyMinutesMessages => _lastTwentyMinutesMessages;
        /// <summary>
        /// Peak value of number of message processed on the last twenty minutes
        /// </summary>
        public long PeakLastTwentyMinutesMessages => _peakLastTwentyMinutesMessages;
        /// <summary>
        /// Date and time of the peak value of number of message processed on the last twenty minutes
        /// </summary>
        public DateTime PeakLastTwentyMinutesMessagesLastDate { get; private set; }

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

        #region Processing Threads
        /// <summary>
        /// Number of current active processing threads
        /// </summary>
        public long CurrentProcessingThreads => _currentProcessingThreads;
        /// <summary>
        /// Peak value of number of active processing threads
        /// </summary>
        public long PeakCurrentProcessingThreads => _peakCurrentProcessingThreads;
        /// <summary>
        /// Date and time of the peak value of number of active processing threads
        /// </summary>
        public DateTime PeakCurrentProcessingThreadsLastDate { get; private set; }

		/// <summary>
		/// Number of active processing threads on the last ten minutes
		/// </summary>
		public long LastTenMinutesProcessingThreads => _lastTenMinutesProcessingThreads;
        /// <summary>
        /// Peak value of the number of active processing threads on the last ten minutes
        /// </summary>
        public long PeakLastTenMinutesProcessingThreads => _peakLastTenMinutesProcessingThreads;
        /// <summary>
        /// Date and time of the peak value of number of active processing threads on the last ten minutes
        /// </summary>
        public DateTime PeakLastTenMinutesProcessingThreadsLastDate { get; private set; }

        /// <summary>
        /// Number of active processing threads on the last twenty minutes
        /// </summary>
        public long LastTwentyMinutesProcessingThreads => _lastTwentyMinutesProcessingThreads;
        /// <summary>
        /// Peak value of the number of active processing threads on the last twenty minutes
        /// </summary>
        public long PeakLastTwentyMinutesProcessingThreads => _peakLastTwentyMinutesProcessingThreads;
        /// <summary>
        /// Date and time of the peak value of number of active processing threads on the last twenty minutes
        /// </summary>
        public DateTime PeakLastTwentyMinutesProcessingThreadsLastDate { get; private set; }

        /// <summary>
        /// Number of active processing threads on the last thirty minutes
        /// </summary>
        public long LastThirtyMinutesProcessingThreads => _lastThirtyMinutesProcessingThreads;
        /// <summary>
        /// Peak value of the number of active processing threads on the last thirty minutes
        /// </summary>
        public long PeakLastThirtyMinutesProcessingThreads => _peakLastThirtyMinutesProcessingThreads;
        /// <summary>
        /// Date and time of the peak value of number of active processing threads on the last thirty minutes
        /// </summary>
        public DateTime PeakLastThirtyMinutesProcessingThreadsLastDate { get; private set; }
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
			_timerTenMinutes = new Timer(state =>
			{
                Interlocked.Exchange(ref _lastTenMinutesMessages, Interlocked.Read(ref _currentMessages));
                Interlocked.Exchange(ref _peakLastTenMinutesMessages, Interlocked.Read(ref _currentMessages));
				PeakLastTenMinutesMessagesLastDate = LastMessageDateTime;

                Interlocked.Exchange(ref _lastTenMinutesProcessingThreads, Interlocked.Read(ref _currentProcessingThreads));
                Interlocked.Exchange(ref _peakLastTenMinutesProcessingThreads, Interlocked.Read(ref _currentProcessingThreads));
				PeakLastTenMinutesProcessingThreadsLastDate = LastProcessingDateTime;
			}, this, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

			_timerTwentyMinutes = new Timer(state =>
			{
                Interlocked.Exchange(ref _lastTwentyMinutesMessages, Interlocked.Read(ref _currentMessages));
                Interlocked.Exchange(ref _peakLastTwentyMinutesMessages, Interlocked.Read(ref _currentMessages));
				PeakLastTwentyMinutesMessagesLastDate = LastMessageDateTime;

                Interlocked.Exchange(ref _lastTwentyMinutesProcessingThreads, Interlocked.Read(ref _currentProcessingThreads));
                Interlocked.Exchange(ref _peakLastTwentyMinutesProcessingThreads, Interlocked.Read(ref _currentProcessingThreads));
				PeakLastTwentyMinutesProcessingThreadsLastDate = LastProcessingDateTime;
			}, this, TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(20));

			_timerThirtyMinutes = new Timer(state =>
			{
                Interlocked.Exchange(ref _lastThirtyMinutesMessages, Interlocked.Read(ref _currentMessages));
                Interlocked.Exchange(ref _peakLastThirtyMinutesMessages, Interlocked.Read(ref _currentMessages));
				PeakLastThirtyMinutesMessagesLastDate = LastMessageDateTime;

                Interlocked.Exchange(ref _lastThirtyMinutesProcessingThreads, Interlocked.Read(ref _currentProcessingThreads));
                Interlocked.Exchange(ref _peakLastThirtyMinutesProcessingThreads, Interlocked.Read(ref _currentProcessingThreads));
				PeakLastThirtyMinutesProcessingThreadsLastDate = LastProcessingDateTime;
			}, this, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));

			Core.Status.Attach(collection =>
			{
				collection.SortValues = false;

				#region Messages On Process
				collection.Add("Current messages on process",
					new StatusItemValueItem("Quantity", CurrentMessages, true),
					new StatusItemValueItem("Peak Quantity", PeakCurrentMessages, true),
					new StatusItemValueItem("Peak DateTime", PeakCurrentMessagesLastDate));

				collection.Add("Last ten minutes processed messages",
					new StatusItemValueItem("Quantity", LastTenMinutesMessages, true),
					new StatusItemValueItem("Peak Quantity", PeakLastTenMinutesMessages, true),
					new StatusItemValueItem("Peak DateTime", PeakLastTenMinutesMessagesLastDate));

				collection.Add("Last twenty minutes processed messages",
					new StatusItemValueItem("Quantity", LastTwentyMinutesMessages, true),
					new StatusItemValueItem("Peak Quantity", PeakLastTwentyMinutesMessages, true),
					new StatusItemValueItem("Peak DateTime", PeakLastTwentyMinutesMessagesLastDate));

				collection.Add("Last thirty minutes processed messages",
					new StatusItemValueItem("Quantity", LastThirtyMinutesMessages, true),
					new StatusItemValueItem("Peak Quantity", PeakLastThirtyMinutesMessages, true),
					new StatusItemValueItem("Peak DateTime", PeakLastThirtyMinutesMessagesLastDate));
				#endregion

				#region Processing Threads
				collection.Add("Current active processing threads",
					new StatusItemValueItem("Quantity", CurrentProcessingThreads, true),
					new StatusItemValueItem("Peak Quantity", PeakCurrentProcessingThreads, true),
					new StatusItemValueItem("Peak DateTime", PeakCurrentProcessingThreadsLastDate));

				collection.Add("Last ten minutes active processed threads",
					new StatusItemValueItem("Quantity", LastTenMinutesProcessingThreads, true),
					new StatusItemValueItem("Peak Quantity", PeakLastTenMinutesProcessingThreads, true),
					new StatusItemValueItem("Peak DateTime", PeakLastTenMinutesProcessingThreadsLastDate));

				collection.Add("Last twenty minutes active processed threads",
					new StatusItemValueItem("Quantity", LastTwentyMinutesProcessingThreads, true),
					new StatusItemValueItem("Peak Quantity", PeakLastTwentyMinutesProcessingThreads, true),
					new StatusItemValueItem("Peak DateTime", PeakLastTwentyMinutesProcessingThreadsLastDate));

				collection.Add("Last thirty minutes active processed threads",
					new StatusItemValueItem("Quantity", LastThirtyMinutesProcessingThreads, true),
					new StatusItemValueItem("Peak Quantity", PeakLastThirtyMinutesProcessingThreads, true),
					new StatusItemValueItem("Peak DateTime", PeakLastThirtyMinutesProcessingThreadsLastDate));
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
            var l10MM = Interlocked.Increment(ref _lastTenMinutesMessages);
            var l20MM = Interlocked.Increment(ref _lastTwentyMinutesMessages);
            var l30MM = Interlocked.Increment(ref _lastThirtyMinutesMessages);
		    Interlocked.Increment(ref _totalMessagesReceived);
            LastMessageDateTime = Core.Now;
			if (cM >= Interlocked.Read(ref _peakCurrentMessages))
			{
                Interlocked.Exchange(ref _peakCurrentMessages, cM);
				PeakCurrentMessagesLastDate = LastMessageDateTime;
			}
			if (l10MM >= Interlocked.Read(ref _peakLastTenMinutesMessages))
			{
                Interlocked.Exchange(ref _peakLastTenMinutesMessages, l10MM);
				PeakLastTenMinutesMessagesLastDate = LastMessageDateTime;
			}
			if (l20MM >= Interlocked.Read(ref _peakLastTwentyMinutesMessages))
			{
                Interlocked.Exchange(ref _peakLastTwentyMinutesMessages, l20MM);
				PeakLastTwentyMinutesMessagesLastDate = LastMessageDateTime;
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
		/// <summary>
		/// Increment the processing threads
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long IncrementProcessingThreads()
		{
            var cP = Interlocked.Increment(ref _currentProcessingThreads);
            var l10MP = Interlocked.Increment(ref _lastTenMinutesProcessingThreads);
            var l20MP = Interlocked.Increment(ref _lastTwentyMinutesProcessingThreads);
            var l30MP = Interlocked.Increment(ref _lastThirtyMinutesProcessingThreads);
			LastProcessingDateTime = Core.Now;
			if (cP >= Interlocked.Read(ref _peakCurrentProcessingThreads))
			{
                Interlocked.Exchange(ref _peakCurrentProcessingThreads, cP);
				PeakCurrentProcessingThreadsLastDate = LastProcessingDateTime;
			}
			if (l10MP >= Interlocked.Read(ref _peakLastTenMinutesProcessingThreads))
			{
                Interlocked.Exchange(ref _peakLastTenMinutesProcessingThreads, l10MP);
				PeakLastTenMinutesProcessingThreadsLastDate = LastMessageDateTime;
			}
			if (l20MP >= Interlocked.Read(ref _peakLastTwentyMinutesProcessingThreads))
			{
                Interlocked.Exchange(ref _peakLastTwentyMinutesProcessingThreads, l20MP);
				PeakLastTwentyMinutesProcessingThreadsLastDate = LastMessageDateTime;
			}
			if (l30MP >= Interlocked.Read(ref _peakLastThirtyMinutesProcessingThreads))
			{
                Interlocked.Exchange(ref _peakLastThirtyMinutesProcessingThreads, l30MP);
				PeakLastThirtyMinutesProcessingThreadsLastDate = LastMessageDateTime;
			}
            return cP;
		}
        /// <summary>
        /// Decrement the current processing threads
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long DecrementProcessingThreads()
            => Interlocked.Decrement(ref _currentProcessingThreads);
		#endregion
	}
}
