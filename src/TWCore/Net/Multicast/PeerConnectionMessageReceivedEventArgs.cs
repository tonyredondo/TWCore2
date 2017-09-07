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
using System.Net;

namespace TWCore.Net.Multicast
{
    /// <inheritdoc />
    /// <summary>
    /// PeerConnection Message Received EventArgs
    /// </summary>
    public class PeerConnectionMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Sender IpAddress
        /// </summary>
        public IPAddress Address { get; set; }
        /// <summary>
        /// Data byte array
        /// </summary>
        public byte[] Data { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// PeerConnection Message Received EventArgs
        /// </summary>
        public PeerConnectionMessageReceivedEventArgs(IPAddress address, byte[] data)
        {
            Address = address;
            Data = data;
        }
    }
}
