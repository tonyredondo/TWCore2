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
using TWCore.Diagnostics.Counters;
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
        const string Category = "Queue Raw Client";
        private readonly IntegerCounter _messagesSentCount = null;
        private readonly IntegerCounter _messagesReceivedCount = null;
        private readonly IntegerCounter _bytesSent = null;
        private readonly IntegerCounter _bytesReceived = null;

        #region .ctor
        /// <summary>
        /// Message queue server counters
        /// </summary>
        /// <param name="name">Client name</param>
        /// <param name="ignoreCounters">Ignore counters</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MQRawClientCounters(string name, bool ignoreCounters)
		{
            if (ignoreCounters) return;
            _messagesSentCount = Core.Counters.GetIntegerCounter(Category, name + @"\Messages Sent", CounterType.Cumulative, CounterLevel.Framework, CounterKind.Messaging, CounterUnit.Messages);
            _messagesReceivedCount = Core.Counters.GetIntegerCounter(Category, name + @"\Messages Received", CounterType.Cumulative, CounterLevel.Framework, CounterKind.Messaging, CounterUnit.Messages);
            _bytesSent = Core.Counters.GetIntegerCounter(Category, name + @"\Bytes Sent", CounterType.Cumulative, CounterLevel.Framework, CounterKind.Messaging, CounterUnit.Bytes);
            _bytesReceived = Core.Counters.GetIntegerCounter(Category, name + @"\Bytes Received", CounterType.Cumulative, CounterLevel.Framework, CounterKind.Messaging, CounterUnit.Bytes);
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Increments the total bytes sent
		/// </summary>
		/// <param name="increment">Increment value</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementTotalBytesSent(int increment)
		{
            _bytesSent?.Add(increment);
		}
		/// <summary>
		/// Increments the total bytes received
		/// </summary>
		/// <param name="increment">Increment value</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementTotalBytesReceived(int increment)
		{
            _bytesReceived?.Add(increment);
		}
		/// <summary>
		/// Increments the messages sent
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementMessagesSent()
		{
            _messagesSentCount?.Increment();
		}
		/// <summary>
		/// Increment the message received
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementMessagesReceived()
		{
            _messagesReceivedCount?.Increment();
		}
		#endregion
	}
}
