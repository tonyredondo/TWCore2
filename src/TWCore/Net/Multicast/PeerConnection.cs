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
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Collections;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable NotAccessedField.Local
// ReSharper disable MethodSupportsCancellation

namespace TWCore.Net.Multicast
{
    /// <summary>
    /// Peer Connection
    /// </summary>
    public class PeerConnection
    {
        private const int PacketSize = 512;
        private static readonly ObjectPool<byte[], ByteArrayAllocator> DatagramPool = new ObjectPool<byte[], ByteArrayAllocator>();
        private readonly TimeoutDictionary<(Guid, ushort), ReceivedDatagrams> _receivedMessagesDatagram = new TimeoutDictionary<(Guid, ushort), ReceivedDatagrams>();
        private readonly List<UdpClient> _clients = new List<UdpClient>();
        private readonly List<UdpClient> _sendClients = new List<UdpClient>();
        private readonly List<Task> _clientsReceiveTasks = new List<Task>();
        private readonly HashSet<EndPoint> _endpointErrors = new HashSet<EndPoint>();
        private readonly TimeSpan _timeoutTime = TimeSpan.FromSeconds(5);

        private IPAddress _multicastIp;
        private IPEndPoint _sendEndpoint;
        private IPEndPoint _receiveEndpoint;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private Task _cancellationTokenTask;
        private bool _connected;

        /// <summary>
        /// On receive message event
        /// </summary>
        public event EventHandler<PeerConnectionMessageReceivedEventArgs> OnReceive;

        #region Allocators
        private readonly struct ByteArrayAllocator : IPoolObjectLifecycle<byte[]>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public byte[] New() => new byte[PacketSize];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(byte[] value) {}
            public int DropTimeFrequencyInSeconds => 60;
            public void DropAction(byte[] value)
            {
            }
        }
        #endregion

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
            _cancellationTokenTask = _token.WhenCanceledAsync();
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

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) continue;
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;
                var nicProp = nic.GetIPProperties();
                var nicPropv4 = nicProp?.GetIPv4Properties();
                if (nicPropv4 is null) continue;
                var addresses = nicProp.UnicastAddresses
                    .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.Address)
                    .Distinct();

                foreach (var ipAddress in addresses)
                {
                    var client = new UdpClient();
                    client.Client.ReceiveBufferSize = 4096;
                    client.Client.SendBufferSize = 4096;
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 50);
                    client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, IPAddress.HostToNetworkOrder(nicPropv4.Index));
                    client.Client.Bind(new IPEndPoint(ipAddress, Port));
                    client.MulticastLoopback = true;
                    client.JoinMulticastGroup(_multicastIp, ipAddress);
                    if (EnableReceive)
                        _clientsReceiveTasks.Add(ReceiveSocketThreadAsync(client));
                    _clients.Add(client);
                    _sendClients.Add(client);
                }
            }
            if (EnableReceive)
            {
                try
                {
                    var basicReceiver = new UdpClient();
                    basicReceiver.Client.ReceiveBufferSize = 4096;
                    basicReceiver.Client.SendBufferSize = 4096;
                    basicReceiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    basicReceiver.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 50);
                    basicReceiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 4096);
                    basicReceiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 4096);
                    basicReceiver.Client.Bind(new IPEndPoint(IPAddress.Any, Port));
                    basicReceiver.JoinMulticastGroup(_multicastIp);
                    _clientsReceiveTasks.Add(ReceiveSocketThreadAsync(basicReceiver));
                    _clients.Add(basicReceiver);
                }
                catch (Exception ex)
                {
                    Core.Log.Warning(ex.Message);
                }
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
            Task.WaitAll(_clientsReceiveTasks.ToArray());
            foreach (var client in _clients)
            {
                try
                {
                    client.DropMulticastGroup(_multicastIp);
                }
                catch
                {
                    //
                }
                try
                {
                    client.Client.Close();
                }
                catch
                {
                    //
                }
            }
            _clients.Clear();
            _sendClients.Clear();
            _clientsReceiveTasks.Clear();
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            _cancellationTokenTask = _token.WhenCanceledAsync();
            _connected = false;
        }
        /// <summary>
        /// Send byte array
        /// </summary>
        /// <param name="buffer">Buffer byte array</param>
        /// <param name="offset">Buffer offset</param>
        /// <param name="count">Buffer count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task SendAsync(byte[] buffer, int offset, int count)
        {
            const int dtsize = PacketSize - 16 - 2 - 2 - 2;
            var numMsgs = (int)Math.Ceiling((double)count / dtsize);
            if (numMsgs > ushort.MaxValue) throw new ArgumentOutOfRangeException($"The buffer must be less than {ushort.MaxValue} bytes");
            var guid = Guid.NewGuid();
            var remain = count;
            var datagram = DatagramPool.New();

#if COMPATIBILITY
            var guidBytes = guid.ToByteArray();
            var numMsgsBytes = BitConverter.GetBytes((ushort)numMsgs);
            for (var i = 0; i < numMsgs; i++)
            {
                var csize = remain >= dtsize ? dtsize : remain;
                var dGram = new Memory<byte>(datagram);
                guidBytes.CopyTo(datagram, 0);
                numMsgsBytes.CopyTo(datagram, 16);
                var ui = (ushort)i;
                var ucsize = (ushort)csize;
                MemoryMarshal.Write(datagram.AsSpan(18), ref ui);
                MemoryMarshal.Write(datagram.AsSpan(20), ref ucsize);
                buffer.AsSpan(offset, csize).CopyTo(dGram.Span.Slice(22));
                dGram.Span.Slice(csize + 22).Clear();

                foreach (var c in _sendClients)
                {
                    if (_token.IsCancellationRequested) break;
                    if (_endpointErrors.Contains(c.Client.LocalEndPoint)) continue;
                    try
                    {
                        await c.SendAsync(datagram, PacketSize, _sendEndpoint).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        if (_endpointErrors.Add(c.Client.LocalEndPoint))
                            Core.Log.Error("Error sending datagram to the multicast group on: {0}", c.Client.LocalEndPoint);
                    }
                }

                remain -= dtsize;
                offset += csize;
            }
#else
            for (var i = 0; i < numMsgs; i++)
            {
                var csize = remain >= dtsize ? dtsize : remain;
                var dGram = new Memory<byte>(datagram);
                guid.TryWriteBytes(dGram.Span.Slice(0, 16));
                BitConverter.TryWriteBytes(dGram.Span.Slice(16, 2), (ushort)numMsgs);
                BitConverter.TryWriteBytes(dGram.Span.Slice(18, 2), (ushort)i);
                BitConverter.TryWriteBytes(dGram.Span.Slice(20, 2), (ushort)csize);
                buffer.AsSpan(offset, csize).CopyTo(dGram.Span.Slice(22));
                dGram.Span.Slice(csize + 22).Clear();

                foreach (var c in _sendClients)
                {
                    if (_token.IsCancellationRequested) break;
                    if (_endpointErrors.Contains(c.Client.LocalEndPoint)) continue;
                    try
                    {
                        await c.SendAsync(datagram, PacketSize, _sendEndpoint).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        if (_endpointErrors.Add(c.Client.LocalEndPoint))
                            Core.Log.Error("Error sending datagram to the multicast group on: {0}", c.Client.LocalEndPoint);
                    }
                }

                remain -= dtsize;
                offset += csize;
            }
#endif

            DatagramPool.Store(datagram);
        }
        /// <summary>
        /// Send buffer
        /// </summary>
        /// <param name="buffer">MultiArray Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task SendAsync(MultiArray<byte> buffer)
        {
            const int dtsize = PacketSize - 16 - 2 - 2 - 2;
            var numMsgs = (int)Math.Ceiling((double)buffer.Count / dtsize);
            if (numMsgs > ushort.MaxValue) throw new ArgumentOutOfRangeException($"The buffer must be less than {ushort.MaxValue} bytes");
            var guid = Guid.NewGuid();
            var offset = buffer.Offset;
            var remain = buffer.Count;
            var datagram = DatagramPool.New();

#if COMPATIBILITY
            var guidBytes = guid.ToByteArray();
            var numMsgsBytes = BitConverter.GetBytes((ushort)numMsgs);
            for (var i = 0; i < numMsgs; i++)
            {
                var csize = remain >= dtsize ? dtsize : remain;
                var dGram = new Memory<byte>(datagram);
                guidBytes.CopyTo(datagram, 0);
                numMsgsBytes.CopyTo(datagram, 16);
                var ui = (ushort)i;
                var ucsize = (ushort)csize;
                MemoryMarshal.Write(datagram.AsSpan(18), ref ui);
                MemoryMarshal.Write(datagram.AsSpan(20), ref ucsize);
                buffer.Slice(offset, csize).CopyTo(dGram.Span.Slice(22));
                dGram.Span.Slice(csize + 22).Clear();

                foreach (var c in _sendClients)
                {
                    if (_token.IsCancellationRequested) break;
                    if (_endpointErrors.Contains(c.Client.LocalEndPoint)) continue;
                    try
                    {
                        await c.SendAsync(datagram, PacketSize, _sendEndpoint).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        if (_endpointErrors.Add(c.Client.LocalEndPoint))
                            Core.Log.Error("Error sending datagram to the multicast group on: {0}", c.Client.LocalEndPoint);
                    }
                }

                remain -= dtsize;
                offset += csize;
            }
#else
            for (var i = 0; i < numMsgs; i++)
            {
                var csize = remain >= dtsize ? dtsize : remain;
                var dGram = new Memory<byte>(datagram);
                guid.TryWriteBytes(dGram.Span.Slice(0, 16));
                BitConverter.TryWriteBytes(dGram.Span.Slice(16, 2), (ushort)numMsgs);
                BitConverter.TryWriteBytes(dGram.Span.Slice(18, 2), (ushort)i);
                BitConverter.TryWriteBytes(dGram.Span.Slice(20, 2), (ushort)csize);
                buffer.Slice(offset, csize).CopyTo(dGram.Span.Slice(22));
                dGram.Span.Slice(csize + 22).Clear();

                foreach (var c in _sendClients)
                {
                    if (_token.IsCancellationRequested) break;
                    if (_endpointErrors.Contains(c.Client.LocalEndPoint)) continue;
                    try
                    {
                        await c.SendAsync(datagram, PacketSize, _sendEndpoint).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        if (_endpointErrors.Add(c.Client.LocalEndPoint))
                            Core.Log.Error("Error sending datagram to the multicast group on: {0}", c.Client.LocalEndPoint);
                    }
                }

                remain -= dtsize;
                offset += csize;
            }
#endif

            DatagramPool.Store(datagram);
        }
        /// <summary>
        /// Send buffer
        /// </summary>
        /// <param name="buffer">Buffer subarray</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task SendAsync(byte[] buffer)
            => SendAsync(buffer, 0, buffer.Length);

        #endregion

        #region Private Methods

        private async Task ReceiveSocketThreadAsync(UdpClient client)
        {
            while (!_token.IsCancellationRequested)
            {
                try
                {
                    var clientTask = client.ReceiveAsync();
                    await Task.WhenAny(clientTask, _cancellationTokenTask).ConfigureAwait(false);
                    if (_token.IsCancellationRequested)
                        return;
                    var udpReceiveResult = clientTask.Result;
                    var rcvEndpoint = udpReceiveResult.RemoteEndPoint;
                    if (rcvEndpoint is null)
                        continue;
                    var datagram = udpReceiveResult.Buffer;
                    if (datagram.Length < 22)
                        continue;

#if COMPATIBILITY
                    var guid = new Guid(datagram.AsSpan(0, 16).ToArray());
#else
                    var guid = new Guid(datagram.AsSpan(0, 16));
#endif

                    var numMsgs = BitConverter.ToUInt16(datagram, 16);
                    var currentMsg = BitConverter.ToUInt16(datagram, 18);
                    var dataSize = BitConverter.ToUInt16(datagram, 20);
                    var buffer = datagram.AsMemory(22, dataSize);
                    var key = (guid, numMsgs);

                    var receivedDatagrams = _receivedMessagesDatagram.GetOrAdd(key, tuple => (new ReceivedDatagrams(tuple.Item2), _timeoutTime));
                    receivedDatagrams.Datagrams[currentMsg] = buffer;
                    if (!receivedDatagrams.Complete)
                        continue;

                    if (OnReceive != null)
                    {
                        try
                        {
                            var bufferMessage = receivedDatagrams.GetMessage();
                            OnReceive.Invoke(this, new PeerConnectionMessageReceivedEventArgs(rcvEndpoint.Address, bufferMessage));
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                    }

                    _receivedMessagesDatagram.TryRemove(key, out _);
                }
                catch (InvalidCastException)
                {
                    //
                }
                catch (ObjectDisposedException)
                {
                    //
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }

        }
        #endregion

        #region Nested Types
        private readonly struct ReceivedDatagrams
        {
            public readonly Memory<byte>[] Datagrams;
            public bool Complete => !Datagrams?.Any(i => i.IsEmpty) ?? false;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySequence<byte> GetMessage()
            {
                if (Datagrams is null) return ReadOnlySequence<byte>.Empty;
                if (Datagrams.Length == 0) return ReadOnlySequence<byte>.Empty;
                ReadOnlySequence<byte> sequence;
                if (Datagrams.Length == 1)
                {
                    sequence = new ReadOnlySequence<byte>(Datagrams[0]);
                }
                else
                {
                    var firstSegment = new Segment<byte>(Datagrams[0]);
                    var lastSegment = firstSegment;
                    for (var i = 1; i < Datagrams.Length; i++)
                        lastSegment = lastSegment.Add(Datagrams[i]);
                    sequence = new ReadOnlySequence<byte>(firstSegment, 0, lastSegment, lastSegment.Memory.Length);
                }
                return sequence;
            }

            public ReceivedDatagrams(int numMessages)
            {
                Datagrams = new Memory<byte>[numMessages];
            }
        }

        private class Segment<T> : ReadOnlySequenceSegment<T>
        {
            public Segment(ReadOnlyMemory<T> memory)
                => Memory = memory;

            public Segment<T> Add(ReadOnlyMemory<T> mem)
            {
                var segment = new Segment<T>(mem)
                {
                    RunningIndex = RunningIndex + Memory.Length
                };
                Next = segment;
                return segment;
            }
        }
        #endregion
    }
}
