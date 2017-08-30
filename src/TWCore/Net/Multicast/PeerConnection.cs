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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using TWCore.Collections;

namespace TWCore.Net.Multicast
{
    /// <summary>
    /// Peer Connection
    /// </summary>
    public class PeerConnection
    {
        readonly TimeoutDictionary<Guid, byte[][]> ReceivedMessagesDatagram = new TimeoutDictionary<Guid, byte[][]>();
        readonly int packetSize = 512;
        UdpClient _client;
        IPAddress _multicastIp;
        IPEndPoint _sendEndpoint;
        IPEndPoint _receiveEndpoint;
        Thread _receiveThread;
        CancellationTokenSource _tokenSource;
        CancellationToken _token;
        bool _connected;
        TimeSpan _messageTimeout = TimeSpan.FromSeconds(30);

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

            _client = new UdpClient();
            _client.ExclusiveAddressUse = false;
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.ExclusiveAddressUse = false;
            if (EnableReceive)
                _client.Client.Bind(_receiveEndpoint);
            _client.MulticastLoopback = true;
            _client.JoinMulticastGroup(_multicastIp);
            if (EnableReceive)
            {
                _receiveThread = new Thread(ReceiveThread)
                {
                    Name = "PeerConnectionReceiveThread",
                    IsBackground = true
                };
                _receiveThread.Start();
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
            _client.DropMulticastGroup(_multicastIp);
            _client.Client.Close();
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
            var dtsize = packetSize - 16 - 2 - 2 - 2;
            var numMsgs = (int)Math.Ceiling((double)count / dtsize);
            if (numMsgs > ushort.MaxValue) throw new ArgumentOutOfRangeException($"The buffer must be less than {ushort.MaxValue} bytes");
            var guidBytes = Guid.NewGuid().ToByteArray();
            var numMsgsBytes = BitConverter.GetBytes((ushort)numMsgs);
            var remain = count;
            for (var i = 0; i < numMsgs; i++)
            {
                var datagram = new byte[packetSize];
                var csize = remain >= dtsize ? dtsize : remain;
                Buffer.BlockCopy(guidBytes, 0, datagram, 0, 16);
                Buffer.BlockCopy(numMsgsBytes, 0, datagram, 16, 2);
                Buffer.BlockCopy(BitConverter.GetBytes((ushort)i), 0, datagram, 18, 2);
                Buffer.BlockCopy(BitConverter.GetBytes((ushort)csize), 0, datagram, 20, 2);
                Buffer.BlockCopy(buffer, offset, datagram, 22, csize);
                var sentBytes = _client.Send(datagram, packetSize, _sendEndpoint);
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
        void ReceiveThread()
        {
            var guidBytes = new byte[16];

            while (!_token.IsCancellationRequested)
            {
                var rcvEndpoint = new IPEndPoint(IPAddress.Any, Port);
                var datagram = _client.Receive(ref rcvEndpoint);
                Buffer.BlockCopy(datagram, 0, guidBytes, 0, 16);
                var guid = new Guid(guidBytes);
                var numMsgs = BitConverter.ToUInt16(datagram, 16);
                var currentMsg = BitConverter.ToUInt16(datagram, 18);
                var dataSize = BitConverter.ToUInt16(datagram, 20);
                var buffer = new byte[dataSize];
                Buffer.BlockCopy(datagram, 22, buffer, 0, dataSize);
                var datagrams = ReceivedMessagesDatagram.GetOrAdd(guid, _guid => (new byte[numMsgs][], _messageTimeout));
                datagrams[currentMsg] = buffer;
                if (datagrams.All(i => i != null))
                {
                    ReceivedMessagesDatagram.TryRemove(guid, out var @out);
                    buffer = datagrams.SelectMany(i => i).ToArray();
                    ThreadPool.QueueUserWorkItem(obj =>
                    {
                        try
                        {
                            var objArray = (object[])obj;
                            var pcnn = (PeerConnection)objArray[0];
                            var pcbf = (byte[])objArray[1];
                            var pcip = (IPEndPoint)objArray[2];
                            pcnn.OnReceive?.Invoke(pcnn, new PeerConnectionMessageReceivedEventArgs(pcip.Address, pcbf));
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                    }, new object[] { this, buffer, rcvEndpoint });
                }
            }
        }
        #endregion
    }
}
