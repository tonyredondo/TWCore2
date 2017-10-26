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

using System.Runtime.CompilerServices;
using System.Threading;
// ReSharper disable InconsistentNaming

namespace TWCore.Net.RPC
{
    /// <summary>
    /// RPC Transport Counters
    /// </summary>
    public class RPCTransportCounters
    {
        private long _bytesSent;
        private long _bytesReceived;

        #region Properties
        /// <summary>
        /// Bytes sent
        /// </summary>
        public long BytesSent => _bytesSent;
        /// <summary>
        /// Bytes received
        /// </summary>
        public long BytesReceived => _bytesReceived;
        #endregion

        #region .ctor
        /// <summary>
        /// RPC Transport Counters
        /// </summary>
        public RPCTransportCounters()
        {
            Core.Status.Attach(collection =>
            {
                collection.Add("MegaBytes Sent", BytesSent.ToMegabytes(), true);
                collection.Add("MegaBytes Received", BytesReceived.ToMegabytes(), true);
            });
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Set Bytes Sent
        /// </summary>
        /// <param name="bytes">Bytes sent</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBytesSent(long bytes)
            => Interlocked.Exchange(ref _bytesSent, bytes);
        /// <summary>
        /// Set Bytes Received
        /// </summary>
        /// <param name="bytes">Bytes received</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBytesReceived(long bytes)
            => Interlocked.Exchange(ref _bytesReceived, bytes);
        /// <summary>
        /// Increment Bytes Sent
        /// </summary>
        /// <param name="bytes">Bytes sent</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementBytesSent(long bytes)
            => Interlocked.Add(ref _bytesSent, bytes);
        /// <summary>
        /// Increment Bytes Received
        /// </summary>
        /// <param name="bytes">Bytes received</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementBytesReceived(long bytes)
            => Interlocked.Add(ref _bytesReceived, bytes);
        #endregion
    }
}
