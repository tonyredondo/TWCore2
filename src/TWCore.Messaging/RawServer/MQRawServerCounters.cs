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
        const string Category = "Queue Raw Server";
        private readonly IntegerCounter _currentMessages = null;
        private readonly IntegerCounter _totalMessagesReceived = null;
        private readonly IntegerCounter _totalMessagesProcessed = null;
        private readonly IntegerCounter _totalExceptions = null;
        private readonly IntegerCounter _totalReceivingBytes = null;

        /// <summary>
        /// Current Messages
        /// </summary>
        public int CurrentMessages;

        #region .ctor
        /// <summary>
        /// Message queue server counters
        /// </summary>
        /// <param name="name">Client name</param>
        /// <param name="ignoreCounters">Ignore counters</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MQRawServerCounters(string name, bool ignoreCounters)
		{
            if (ignoreCounters) return;
            _currentMessages = Core.Counters.GetIntegerCounter(Category, name + @"\Current Messages", CounterType.Current, CounterLevel.Framework, CounterKind.Messaging);
            _totalMessagesReceived = Core.Counters.GetIntegerCounter(Category, name + @"\Messages Received", CounterType.Cumulative, CounterLevel.Framework, CounterKind.Messaging);
            _totalMessagesProcessed = Core.Counters.GetIntegerCounter(Category, name + @"\Messages Processed", CounterType.Cumulative, CounterLevel.Framework, CounterKind.Messaging);
            _totalExceptions = Core.Counters.GetIntegerCounter(Category, name + @"\Exceptions", CounterType.Cumulative, CounterLevel.Framework, CounterKind.Messaging);
            _totalReceivingBytes = Core.Counters.GetIntegerCounter(Category, name + @"\Receiving Bytes", CounterType.Cumulative, CounterLevel.Framework, CounterKind.Messaging);
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Increments the total receiving time
		/// </summary>
		/// <param name="increment">Increment value</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void IncrementTotalReceivingBytes(int increment)
        {
            _totalReceivingBytes?.Add(increment);
        }
        /// <summary>
        /// Increments the total exceptions number
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementTotalExceptions()
        {
            _totalExceptions?.Increment();
        }
        /// <summary>
        /// Increments the total exceptions number
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementTotalMessagesProccesed()
        {
            _totalMessagesProcessed?.Increment();
        }
		/// <summary>
		/// Increments the messages
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IncrementMessages()
		{
            _currentMessages?.Increment();
            _totalMessagesReceived?.Increment();
            return Interlocked.Increment(ref CurrentMessages);
		}
        /// <summary>
        /// Decrement the current messages
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DecrementMessages()
        {
            _currentMessages?.Decrement();
            return Interlocked.Decrement(ref CurrentMessages);
        }
        #endregion
    }
}
