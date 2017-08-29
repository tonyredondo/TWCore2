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
using System.Net.Sockets;

namespace TWCore.Net.Discovery
{
    /// <summary>
    /// Peer Connection
    /// </summary>
    public class PeerConnection
    {
        UdpClient _client;
        IPAddress _multicastIp;
        IPEndPoint _sendEndpoint;
        IPEndPoint _receiveEndpoint;

        #region Properties
        /// <summary>
        /// Port number
        /// </summary>
        public int Port { get; set; } = 23128;
        /// <summary>
        /// Multicast Ip Address
        /// </summary>
        public string MulticastIp { get; set; } = "230.023.012.083";
        #endregion

        #region Methods
        /// <summary>
        /// Connect and join the peer group
        /// </summary>
        public void Connect()
        {
            _multicastIp = IPAddress.Parse(MulticastIp);
            _sendEndpoint = new IPEndPoint(_multicastIp, Port);
            _receiveEndpoint = new IPEndPoint(IPAddress.Any, Port);

            _client = new UdpClient();
            _client.ExclusiveAddressUse = false;
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.ExclusiveAddressUse = false;
            _client.Client.Bind(_receiveEndpoint);
            _client.MulticastLoopback = false;
            _client.JoinMulticastGroup(_multicastIp);
        }
        /// <summary>
        /// Disconnect and leave the peer group
        /// </summary>
        public void Disconnect()
        {
            _client.DropMulticastGroup(_multicastIp);
            _client.Client.Close();
        }
        #endregion
    }
}
