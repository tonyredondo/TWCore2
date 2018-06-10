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

namespace TWCore.Messaging.RawClient
{
	/// <summary>
	/// Message queue client counters
	/// </summary>
	[StatusName("Counters")]
	public class MQRawClientCounters
	{
		private Timer _timerTen;
		private Timer _timerThirty;

		#region Properties
		/// <summary>
		/// Number of messages Sent
		/// </summary>
		public long MessagesSent { get; private set; }
		/// <summary>
		/// Number of messages sent in the last ten minutes
		/// </summary>
		public long LastTenMinutesMessagesSent { get; private set; }
		/// <summary>
		/// Number of messages sent in the last thirty minutes
		/// </summary>
		public long LastThirtyMinutesMessagesSent { get; private set; }

		/// <summary>
		/// Number of messages received
		/// </summary>
		public long MessagesReceived { get; private set; }
		/// <summary>
		/// Number of messages received in the last ten minutes
		/// </summary>
		public long LastTenMinutesMessagesReceived { get; private set; }
		/// <summary>
		/// Number of messages received in the last thirty minutes
		/// </summary>
		public long LastThirtyMinutesMessagesReceived { get; private set; }

		/// <summary>
		/// Total bytes sent
		/// </summary>
		public double TotalBytesSent { get; private set; }
		/// <summary>
		/// Total bytes received
		/// </summary>
		public double TotalBytesReceived { get; private set; }
		#endregion

		#region .ctor
		/// <summary>
		/// Message queue server counters
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MQRawClientCounters()
		{
			_timerTen = new Timer(state =>
			{
				LastTenMinutesMessagesSent = 0;
				LastTenMinutesMessagesReceived = 0;
			}, this, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

			_timerThirty = new Timer(state =>
			{
				LastThirtyMinutesMessagesSent = 0;
				LastThirtyMinutesMessagesReceived = 0;
			}, this, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));

			Core.Status.Attach(collection =>
			{
				collection.Add("Number of messages sent",
					new StatusItemValueItem("Last 10 Minutes", LastTenMinutesMessagesSent, true),
					new StatusItemValueItem("Last 30 Minutes", LastThirtyMinutesMessagesSent, true),
					new StatusItemValueItem("Total", MessagesSent, true));

				collection.Add("Number of messages received",
					new StatusItemValueItem("Last 10 Minutes", LastTenMinutesMessagesReceived, true),
					new StatusItemValueItem("Last 30 Minutes", LastThirtyMinutesMessagesReceived, true),
					new StatusItemValueItem("Total", MessagesReceived, true));

				collection.Add("Total Bytes",
					new StatusItemValueItem("Sent bytes (MB)", TotalBytesSent.ToMegabytes(), true),
					new StatusItemValueItem("Received bytes (MB)", TotalBytesReceived.ToMegabytes(), true));
			});
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
			TotalBytesSent += increment;
		}
		/// <summary>
		/// Increments the total bytes received
		/// </summary>
		/// <param name="increment">Increment value</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementTotalBytesReceived(double increment)
		{
			TotalBytesReceived += increment;
		}
		/// <summary>
		/// Increments the messages sent
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementMessagesSent()
		{
			MessagesSent++;
			LastTenMinutesMessagesSent++;
			LastThirtyMinutesMessagesSent++;
		}
		/// <summary>
		/// Increment the message received
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementMessagesReceived()
		{
			MessagesReceived++;
			LastTenMinutesMessagesReceived++;
			LastThirtyMinutesMessagesReceived++;
		}
		#endregion
	}
}
