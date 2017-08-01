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

namespace TWCore.Net.RPC
{
    /// <summary>
    /// RPC Transport Counters
    /// </summary>
    public class RPCTransportCounters
    {
        static object _locker = new object();

        #region Properties
        /// <summary>
        /// Bytes sent
        /// </summary>
        public long BytesSent { get; private set; }
        /// <summary>
        /// Bytes received
        /// </summary>
        public long BytesReceived { get; private set; }
        #endregion

        #region .ctor
        /// <summary>
        /// RPC Transport Counters
        /// </summary>
        public RPCTransportCounters()
        {
            Core.Status.Attach(collection =>
            {
                collection.Add("Bytes Sent", BytesSent.ToReadeableBytes());
                collection.Add("Bytes Received", BytesReceived.ToReadeableBytes());
            });
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Increment Bytes Sent
        /// </summary>
        /// <param name="bytes">Bytes sent</param>
        public void IncrementBytesSent(long bytes)
        {
            lock (_locker)
            {
                if (long.MaxValue - bytes < BytesSent)
                    BytesSent = 0;
                BytesSent += bytes;
            }
        }
        /// <summary>
        /// Increment Bytes Received
        /// </summary>
        /// <param name="bytes">Bytes received</param>
        public void IncrementBytesReceived(long bytes)
        {
            lock (_locker)
            {
                if (long.MaxValue - bytes < BytesReceived)
                    BytesReceived = 0;
                BytesReceived += bytes;
            }
        }
        #endregion
    }
}
