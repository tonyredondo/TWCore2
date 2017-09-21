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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using TWCore.Collections;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Net.Multicast
{
    /// <summary>
    /// Peer Connection
    /// </summary>
    public class PeerConnection
    {
        private const int PacketSize = 512;
        private readonly TimeoutDictionary<Guid, ReceivedDatagrams> _receivedMessagesDatagram = new TimeoutDictionary<Guid, ReceivedDatagrams>();
        private readonly List<UdpClient> _clients = new List<UdpClient>();
        private readonly List<UdpClient> _sendClients = new List<UdpClient>();
        private readonly List<Thread> _clientsReceiveThreads = new List<Thread>();
        private readonly TimeSpan _messageTimeout = TimeSpan.FromSeconds(30);

        private IPAddress _multicastIp;
        private IPEndPoint _sendEndpoint;
        private IPEndPoint _receiveEndpoint;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private bool _connected;

        /// <summary>
        /// On receive message event
        /// </summary>
        public event EventHandler<PeerConnectionMessageReceivedEventArgs> OnReceive;

        #region Properties
        /// <summary>
        /// Port number
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// Multicast Ip Address
        /// </summary>
        public string MulticastIp { get; private set; }
        /// <summary>
        /// Enable Receive
        /// </summary>
        public bool EnableReceive { get; private set; } = true;
        #endregion

        #region .ctor
        /// <summary>
        /// Peer Connection
        /// </summary>
        public PeerConnection()
        {
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            _connected = false;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Connect and join the peer group
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Connect(string multicastIp, int port, bool enableReceive = true)
        {
            Disconnect();
            _connected = true;
            MulticastIp = multicastIp;
            Port = port;
            EnableReceive = enableReceive;
            _multicastIp = IPAddress.Parse(MulticastIp);
            _sendEndpoint = new IPEndPoint(_multicastIp, Port);
            _receiveEndpoint = new IPEndPoint(IPAddress.Any, Port);
            
            foreach(var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) continue;
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;
                var nicProp = nic.GetIPProperties();
                if (nicProp == null) continue;
                var nicPropv4 = nicProp.GetIPv4Properties();
                if (nicPropv4 == null) continue;
                var addresses = nicProp.UnicastAddresses
                    .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork).Select(a => a.Address);

                foreach (var ipAddress in addresses)
                {
                    var client = new UdpClient();
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 50);
                    client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, IPAddress.HostToNetworkOrder(nicPropv4.Index));
                    client.Client.Bind(new IPEndPoint(ipAddress, Port));
                    client.MulticastLoopback = true;
                    client.JoinMulticastGroup(_multicastIp, ipAddress);
                    if (EnableReceive)
                    {
                        var thread = new Thread(ReceiveSocketThread)
                        {
                            Name = "PeerConnectionReceiveThread:" + ipAddress,
                            IsBackground = true
                        };
                        thread.Start(client);
                        _clientsReceiveThreads.Add(thread);
                    }
                    _clients.Add(client);
                    _sendClients.Add(client);
                }
            }
            if (EnableReceive)
            {
                var basicReceiver = new UdpClient();
                basicReceiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                basicReceiver.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 50);
                basicReceiver.Client.Bind(new IPEndPoint(IPAddress.Any, Port));
                basicReceiver.JoinMulticastGroup(_multicastIp);
                var thread = new Thread(ReceiveSocketThread)
                {
                    Name = "PeerConnectionReceiveThread",
                    IsBackground = true
                };
                thread.Start(basicReceiver);
                _clientsReceiveThreads.Add(thread);
                _clients.Add(basicReceiver);
            }
        }
        /// <summary>
        /// Disconnect and leave the peer group
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Disconnect()
        {
            if (!_connected) return;
            _tokenSource?.Cancel();
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            foreach (var client in _clients)
            {
                client.DropMulticastGroup(_multicastIp);
                client.Client.Close();
            }
            _clients.Clear();
            _sendClients.Clear();
            _clientsReceiveThreads.Clear();
            _connected = false;
        }
        /// <summary>
        /// Send byte array
        /// </summary>
        /// <param name="buffer">Buffer byte array</param>
        /// <param name="offset">Buffer offset</param>
        /// <param name="count">Buffer count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(byte[] buffer, int offset, int count)
        {
            const int dtsize = PacketSize - 16 - 2 - 2 - 2;
            var numMsgs = (int)Math.Ceiling((double)count / dtsize);
            if (numMsgs > ushort.MaxValue) throw new ArgumentOutOfRangeException($"The buffer must be less than {ushort.MaxValue} bytes");
            var guidBytes = Guid.NewGuid().ToByteArray();
            var numMsgsBytes = BitConverter.GetBytes((ushort)numMsgs);
            var remain = count;
            for (var i = 0; i < numMsgs; i++)
            {
                var datagram = new byte[PacketSize];
                var csize = remain >= dtsize ? dtsize : remain;
                Buffer.BlockCopy(guidBytes, 0, datagram, 0, 16);
                Buffer.BlockCopy(numMsgsBytes, 0, datagram, 16, 2);
                Buffer.BlockCopy(BitConverter.GetBytes((ushort)i), 0, datagram, 18, 2);
                Buffer.BlockCopy(BitConverter.GetBytes((ushort)csize), 0, datagram, 20, 2);
                Buffer.BlockCopy(buffer, offset, datagram, 22, csize);

                foreach (var c in _sendClients)
                {
                    try
                    {
                        c.Send(datagram, PacketSize, _sendEndpoint);
                        //Core.Log.InfoBasic("Datagram sent to to the multicast group on: {0}", c.Client.LocalEndPoint);
                    }
                    catch (Exception)
                    {
                        Core.Log.Error("Error sending datagram to the multicast group on: {0}", c.Client.LocalEndPoint);
                    }
                }

                remain -= dtsize;
                offset += csize;
            }
        }
        /// <summary>
        /// Send buffer
        /// </summary>
        /// <param name="buffer">Buffer subarray</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(SubArray<byte> buffer)
            => Send(buffer.Array, buffer.Offset, buffer.Count);
        /// <summary>
        /// Send buffer
        /// </summary>
        /// <param name="buffer">Buffer subarray</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(byte[] buffer)
            => Send(buffer, 0, buffer.Length);

        #endregion

        #region Private Methods

        private void ReceiveSocketThread(object clientObject)
        {
            var guidBytes = new byte[16];
            var client = (UdpClient)clientObject;
            while (!_token.IsCancellationRequested)
            {
                try
                {
                    //Core.Log.InfoBasic("Waiting data on Endpoint: {0}", client.Client.LocalEndPoint);
                    var rcvEndpoint = new IPEndPoint(IPAddress.Any, Port);
                    var datagram = client.Receive(ref rcvEndpoint);
                    if (datagram.Length < 22)
                        continue;
                    Buffer.BlockCopy(datagram, 0, guidBytes, 0, 16);
                    var guid = new Guid(guidBytes);
                    var numMsgs = BitConverter.ToUInt16(datagram, 16);
                    var currentMsg = BitConverter.ToUInt16(datagram, 18);
                    var dataSize = BitConverter.ToUInt16(datagram, 20);
                    var buffer = new byte[dataSize];
                    Buffer.BlockCopy(datagram, 22, buffer, 0, dataSize);

                    //Core.Log.InfoDetail("Guid: {0}, NumMsg: {1}, CurrentMsg: {2}, DataSize: {3}", guid, numMsgs, currentMsg, dataSize);

                    var receivedDatagrams = _receivedMessagesDatagram.GetOrAdd(guid, mguid => (new ReceivedDatagrams(numMsgs, rcvEndpoint.Address), _messageTimeout));
                    if (receivedDatagrams.Address != rcvEndpoint.Address.ToString())
                        continue;
                    receivedDatagrams.Datagrams[currentMsg] = buffer;
                    if (!receivedDatagrams.Complete)
                        continue;
                    _receivedMessagesDatagram.TryRemove(guid, out var _);

                    buffer = receivedDatagrams.GetMessage();

                    ThreadPool.QueueUserWorkItem(obj =>
                    {
                        try
                        {
                            var objArray = (object[])obj;
                            var pcnn = (PeerConnection)objArray[0];
                            var pcbf = (byte[])objArray[1];
                            var pcip = (IPEndPoint)objArray[2];
                            pcnn.OnReceive?.Invoke(pcnn,
                                new PeerConnectionMessageReceivedEventArgs(pcip.Address, pcbf));
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                    }, new object[] { this, buffer, rcvEndpoint });
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }

        }
        #endregion

        #region Nested Types
        private class ReceivedDatagrams
        {
            public byte[][] Datagrams { get; }
            public string Address { get; }
            public bool Complete => Datagrams.All(i => i != null);
            public ReceivedDatagrams(int numMessages, IPAddress address)
            {
                Datagrams = new byte[numMessages][];
                Address = address.ToString();
            }

            public byte[] GetMessage()
                => Datagrams.SelectMany(i => i).ToArray();
        }
        #endregion
    }
}
