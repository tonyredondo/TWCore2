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

using System.Runtime.CompilerServices;
using TWCore.Diagnostics.Counters;

// ReSharper disable InconsistentNaming

namespace TWCore.Net.RPC
{
    /// <summary>
    /// RPC Transport Counters
    /// </summary>
    public class RPCTransportCounters
    {
        const string Category = "RPC Transport";

        private readonly IntegerCounter _bytesSent = null;
        private readonly IntegerCounter _bytesReceived = null;

        #region .ctor
        /// <summary>
        /// RPC Transport Counters
        /// </summary>
        public RPCTransportCounters()
        {
            _bytesSent = Core.Counters.GetIntegerCounter(Category, @"\Bytes Sent", CounterType.Cumulative, CounterLevel.Framework, CounterKind.RPC);
            _bytesReceived = Core.Counters.GetIntegerCounter(Category, @"\Bytes Received", CounterType.Cumulative, CounterLevel.Framework, CounterKind.RPC);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Increment Bytes Sent
        /// </summary>
        /// <param name="bytes">Bytes sent</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementBytesSent(int bytes)
        {
            _bytesSent.Add(bytes);
        }
        /// <summary>
        /// Increment Bytes Received
        /// </summary>
        /// <param name="bytes">Bytes received</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementBytesReceived(int bytes)
        {
            _bytesReceived.Add(bytes);
        }
        #endregion
    }
}
