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
	public class MQRawServerCounters
	{
		private Timer _timerTenMinutes;
		private Timer _timerTwentyMinutes;
		private Timer _timerThirtyMinutes;

		#region Messages On Process
		/// <summary>
		/// Number of Messages on process
		/// </summary>
		public long CurrentMessages { get; private set; }
		/// <summary>
		/// Peak value of number of messages on process
		/// </summary>
		public long PeakCurrentMessages { get; private set; }
		/// <summary>
		/// Date and time of the peak value of number of message on process
		/// </summary>
		public DateTime PeakCurrentMessagesLastDate { get; private set; }

		/// <summary>
		/// Number of messages processed on the last ten minutes
		/// </summary>
		public long LastTenMinutesMessages { get; private set; }
		/// <summary>
		/// Peak value of number of message processed on the last ten minutes
		/// </summary>
		public long PeakLastTenMinutesMessages { get; private set; }
		/// <summary>
		/// Date and time of the peak value of number of message processed on the last ten minutes
		/// </summary>
		public DateTime PeakLastTenMinutesMessagesLastDate { get; private set; }

		/// <summary>
		/// Number of messages processed on the last twenty minutes
		/// </summary>
		public long LastTwentyMinutesMessages { get; private set; }
		/// <summary>
		/// Peak value of number of message processed on the last twenty minutes
		/// </summary>
		public long PeakLastTwentyMinutesMessages { get; private set; }
		/// <summary>
		/// Date and time of the peak value of number of message processed on the last twenty minutes
		/// </summary>
		public DateTime PeakLastTwentyMinutesMessagesLastDate { get; private set; }

		/// <summary>
		/// Number of messages processed on the last thirty minutes
		/// </summary>
		public long LastThirtyMinutesMessages { get; private set; }
		/// <summary>
		/// Peak value of number of message processed on the last thirty minutes
		/// </summary>
		public long PeakLastThirtyMinutesMessages { get; private set; }
		/// <summary>
		/// Date and time of the peak value of number of message processed on the last thirty minutes
		/// </summary>
		public DateTime PeakLastThirtyMinutesMessagesLastDate { get; private set; }
		#endregion

		#region Processing Threads
		/// <summary>
		/// Number of current active processing threads
		/// </summary>
		public long CurrentProcessingThreads { get; private set; }
		/// <summary>
		/// Peak value of number of active processing threads
		/// </summary>
		public long PeakCurrentProcessingThreads { get; private set; }
		/// <summary>
		/// Date and time of the peak value of number of active processing threads
		/// </summary>
		public DateTime PeakCurrentProcessingThreadsLastDate { get; private set; }

		/// <summary>
		/// Number of active processing threads on the last ten minutes
		/// </summary>
		public long LastTenMinutesProcessingThreads { get; private set; }
		/// <summary>
		/// Peak value of the number of active processing threads on the last ten minutes
		/// </summary>
		public long PeakLastTenMinutesProcessingThreads { get; private set; }
		/// <summary>
		/// Date and time of the peak value of number of active processing threads on the last ten minutes
		/// </summary>
		public DateTime PeakLastTenMinutesProcessingThreadsLastDate { get; private set; }

		/// <summary>
		/// Number of active processing threads on the last twenty minutes
		/// </summary>
		public long LastTwentyMinutesProcessingThreads { get; private set; }
		/// <summary>
		/// Peak value of the number of active processing threads on the last twenty minutes
		/// </summary>
		public long PeakLastTwentyMinutesProcessingThreads { get; private set; }
		/// <summary>
		/// Date and time of the peak value of number of active processing threads on the last twenty minutes
		/// </summary>
		public DateTime PeakLastTwentyMinutesProcessingThreadsLastDate { get; private set; }

		/// <summary>
		/// Number of active processing threads on the last thirty minutes
		/// </summary>
		public long LastThirtyMinutesProcessingThreads { get; private set; }
		/// <summary>
		/// Peak value of the number of active processing threads on the last thirty minutes
		/// </summary>
		public long PeakLastThirtyMinutesProcessingThreads { get; private set; }
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
		public long TotalMessagesReceived { get; private set; }
		/// <summary>
		/// Number of processed messages
		/// </summary>
		public long TotalMessagesProccesed { get; private set; }
		/// <summary>
		/// Number of exceptions
		/// </summary>
		public long TotalExceptions { get; private set; }
		/// <summary>
		/// Total network time
		/// </summary>
		public double TotalNetworkTime { get; private set; }
		/// <summary>
		/// Total receiving time
		/// </summary>
		public double TotalReceivingBytes { get; private set; }
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
				LastTenMinutesMessages = CurrentMessages;
				PeakLastTenMinutesMessages = CurrentMessages;
				PeakLastTenMinutesMessagesLastDate = LastMessageDateTime;

				LastTenMinutesProcessingThreads = CurrentProcessingThreads;
				PeakLastTenMinutesProcessingThreads = CurrentProcessingThreads;
				PeakLastTenMinutesProcessingThreadsLastDate = LastProcessingDateTime;
			}, this, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

			_timerTwentyMinutes = new Timer(state =>
			{
				LastTwentyMinutesMessages = CurrentMessages;
				PeakLastTwentyMinutesMessages = CurrentMessages;
				PeakLastTwentyMinutesMessagesLastDate = LastMessageDateTime;

				LastTwentyMinutesProcessingThreads = CurrentProcessingThreads;
				PeakLastTwentyMinutesProcessingThreads = CurrentProcessingThreads;
				PeakLastTwentyMinutesProcessingThreadsLastDate = LastProcessingDateTime;
			}, this, TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(20));

			_timerThirtyMinutes = new Timer(state =>
			{
				LastThirtyMinutesMessages = CurrentMessages;
				PeakLastThirtyMinutesMessages = CurrentMessages;
				PeakLastThirtyMinutesMessagesLastDate = LastMessageDateTime;

				LastThirtyMinutesProcessingThreads = CurrentProcessingThreads;
				PeakLastThirtyMinutesProcessingThreads = CurrentProcessingThreads;
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
					new StatusItemValueItem("Network Time (ms)", TotalNetworkTime, true),
					new StatusItemValueItem("Receiving Bytes (MB)", TotalReceivingBytes.ToMegabytes(), true));
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
			TotalNetworkTime += increment.TotalMilliseconds;
		}
		/// <summary>
		/// Increments the total receiving time
		/// </summary>
		/// <param name="increment">Increment value</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementTotalReceivingBytes(double increment)
		{
			TotalReceivingBytes += increment;
		}
		/// <summary>
		/// Increments the total exceptions number
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementTotalExceptions()
		{
			TotalExceptions++;
		}
		/// <summary>
		/// Increments the total exceptions number
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementTotalMessagesProccesed()
		{
			TotalMessagesProccesed++;
		}
		/// <summary>
		/// Increments the messages
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementMessages()
		{
			CurrentMessages++;
			LastTenMinutesMessages++;
			LastTwentyMinutesMessages++;
			LastThirtyMinutesMessages++;
			TotalMessagesReceived++;
			LastMessageDateTime = Core.Now;
			if (CurrentMessages >= PeakCurrentMessages)
			{
				PeakCurrentMessages = CurrentMessages;
				PeakCurrentMessagesLastDate = LastMessageDateTime;
			}
			if (LastTenMinutesMessages >= PeakLastTenMinutesMessages)
			{
				PeakLastTenMinutesMessages = LastTenMinutesMessages;
				PeakLastTenMinutesMessagesLastDate = LastMessageDateTime;
			}
			if (LastTwentyMinutesMessages >= PeakLastTwentyMinutesMessages)
			{
				PeakLastTwentyMinutesMessages = LastTwentyMinutesMessages;
				PeakLastTwentyMinutesMessagesLastDate = LastMessageDateTime;
			}
			if (LastThirtyMinutesMessages >= PeakLastThirtyMinutesMessages)
			{
				PeakLastThirtyMinutesMessages = LastThirtyMinutesMessages;
				PeakLastThirtyMinutesMessagesLastDate = LastMessageDateTime;
			}
		}
		/// <summary>
		/// Decrement the current messages
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DecrementMessages()
		{
			CurrentMessages--;
		}
		/// <summary>
		/// Increment the processing threads
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementProcessingThreads()
		{
			CurrentProcessingThreads++;
			LastTenMinutesProcessingThreads++;
			LastTwentyMinutesProcessingThreads++;
			LastThirtyMinutesProcessingThreads++;
			LastProcessingDateTime = Core.Now;
			if (CurrentProcessingThreads >= PeakCurrentProcessingThreads)
			{
				PeakCurrentProcessingThreads = CurrentProcessingThreads;
				PeakCurrentProcessingThreadsLastDate = LastProcessingDateTime;
			}
			if (LastTenMinutesProcessingThreads >= PeakLastTenMinutesProcessingThreads)
			{
				PeakLastTenMinutesProcessingThreads = LastTenMinutesProcessingThreads;
				PeakLastTenMinutesProcessingThreadsLastDate = LastMessageDateTime;
			}
			if (LastTwentyMinutesProcessingThreads >= PeakLastTwentyMinutesProcessingThreads)
			{
				PeakLastTwentyMinutesProcessingThreads = LastTwentyMinutesProcessingThreads;
				PeakLastTwentyMinutesProcessingThreadsLastDate = LastMessageDateTime;
			}
			if (LastThirtyMinutesProcessingThreads >= PeakLastThirtyMinutesProcessingThreads)
			{
				PeakLastThirtyMinutesProcessingThreads = LastThirtyMinutesProcessingThreads;
				PeakLastThirtyMinutesProcessingThreadsLastDate = LastMessageDateTime;
			}
		}
		/// <summary>
		/// Decrement the current processing threads
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DecrementProcessingThreads()
		{
			CurrentProcessingThreads--;
		}
		#endregion
	}
}
