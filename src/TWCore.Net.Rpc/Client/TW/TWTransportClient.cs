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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Diagnostics.Status;
using TWCore.IO;
using TWCore.Net.RPC.Descriptors;
using TWCore.Serialization;
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable SuggestBaseTypeForParameter

namespace TWCore.Net.RPC.Client.Transports
{
    /// <inheritdoc />
    /// <summary>
    /// TW RPC Transport client
    /// </summary>
    public class TWTransportClient : ITransportClient
    {
        private readonly ManualResetEventSlim _connectionResetEvent = new ManualResetEventSlim(false);
        private readonly List<SocketConnection> _availableConnections = new List<SocketConnection>();
        private readonly List<SocketConnection> _connectedSockets = new List<SocketConnection>();
        private readonly ConcurrentDictionary<Guid, RPCResponseHandler> _messageResponsesHandlers = new ConcurrentDictionary<Guid, RPCResponseHandler>();
        private readonly HashSet<Guid> _messageRetries = new HashSet<Guid>();
        private readonly LRU2QCollection<Guid, object> _previousMessages = new LRU2QCollection<Guid, object>(2048);
        private int _connectedSocketsCount;
        private int _socketTurn;
        private readonly byte _socketsPerClient = 2;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;


        #region Nested Class
        private sealed class RPCResponseHandler
        {
            public readonly ManualResetEventSlim Event = new ManualResetEventSlim(false);
            public RPCResponseMessage Message;
        }
        #endregion

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Transport name, should be the same name for Server and Client
        /// </summary>
        [StatusProperty]
        public string Name => "TWTransport";
        /// <inheritdoc />
        /// <summary>
        /// Serializer to encode and decode the incoming and outgoing data
        /// </summary>
        [StatusProperty]
        public ISerializer Serializer { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Services descriptors to use on RPC Request messages
        /// </summary>
        public ServiceDescriptorCollection Descriptors { get; set; }
        /// <summary>
        /// RPC Transport Server host
        /// </summary>
        [StatusProperty]
        public string Host { get; private set; }
        /// <summary>
        /// RPC Transport Server port
        /// </summary>
        [StatusProperty]
        public int Port { get; private set; }
        /// <summary>
        /// Client session identifier
        /// </summary>
        [StatusProperty]
        public Guid SessionId { get; private set; }
        /// <summary>
        /// Hub name
        /// </summary>
        [StatusProperty]
        public string Hub { get; private set; }
        /// <summary>
        /// Gets true if the SessionResponse was already received and accepted.
        /// </summary>
        [StatusProperty]
        public bool IsOnSession { get; private set; } = false;
        /// <summary>
        /// Gets if the client is connected to a server
        /// </summary>
        [StatusProperty]
        public bool Connected => Status == ConnectionStatus.Connected;
        /// <summary>
        /// Gets or Sets the ping time period in seconds
        /// </summary>
        [StatusProperty]
        public int PingTimeInSeconds { get; set; } = 30;
        /// <inheritdoc />
        /// <summary>
        /// Transport Counters
        /// </summary>
        [StatusProperty]
        public RPCTransportCounters Counters { get; } = new RPCTransportCounters();
        /// <summary>
        /// Connection status
        /// </summary>
        [StatusProperty]
        public ConnectionStatus Status { get; set; } = ConnectionStatus.Disconnected;
        /// <summary>
        /// Receive Buffer Size
        /// </summary>
        [StatusProperty]
        public int ReceiveBufferSize { get; set; } = 32768;
        /// <summary>
        /// Send Buffer Size
        /// </summary>
        [StatusProperty]
		public int SendBufferSize { get; set; } = 32768;
        /// <summary>
        /// Invoke Method Timeout
        /// </summary>
        [StatusProperty]
        public int InvokeMethodTimeout { get; set; } = 45000;
        /// <summary>
        /// Target Status
        /// </summary>
        public ConnectionStatus TargetStatus { get; set; } = ConnectionStatus.Disconnected;
        #endregion

        #region Events
        /// <summary>
        /// Events received from the RPC transport server
        /// </summary>
        public event EventHandler<EventDataEventArgs> OnEventReceived;
        /// <summary>
        /// On connecting event
        /// </summary>
        public event EventHandler OnConnecting;
        /// <summary>
        /// On connected event
        /// </summary>
        public event EventHandler OnConnected;
        /// <summary>
        /// On disconnected event
        /// </summary>
        public event EventHandler OnDisconnected;
        /// <summary>
        /// On disconnecting event
        /// </summary>
        public event EventHandler OnDisconnecting;
        /// <summary>
        /// Event when a push message has been received from server
        /// </summary>
        public event EventHandler<EventArgs<RPCPushMessage>> OnPushMessageReceived;
        #endregion

        #region .ctor
        /// <summary>
        /// TwoWay RPC Transport Progressive client
        /// </summary>
        public TWTransportClient()
        {
            Serializer = SerializerManager.DefaultBinarySerializer.DeepClone();

            for (var i = 0; i < _socketsPerClient; i++)
                _availableConnections.Add(CreateSocketConnection());

            Core.Status.Attach(collection =>
            {
                collection.Add("Messages Waiting Response Count", _messageResponsesHandlers.Count, true);
                foreach (var cnn in _availableConnections)
                    Core.Status.AttachChild(cnn, this);
            });
        }
        /// <summary>
        /// TwoWay RPC Transport Progressive client
        /// </summary>
        /// <param name="host">RPC Transport Server Host</param>
        /// <param name="port">RPC Transport Server Port</param>
        /// <param name="socketsPerClient">Sockets per transport client</param>
        /// <param name="serializer">Serializer for data transfer, is is null then is XmlTextSerializer</param>
        public TWTransportClient(string host, int port, byte socketsPerClient, ISerializer serializer = null)
        {
            Host = host;
            Port = port;
            Serializer = serializer ?? SerializerManager.DefaultBinarySerializer.DeepClone();
            if (socketsPerClient > 0)
                _socketsPerClient = socketsPerClient;

            for (var i = 0; i < _socketsPerClient; i++)
                _availableConnections.Add(CreateSocketConnection());

            Core.Status.Attach(collection =>
            {
                collection.Add("Messages Waiting Response Count", _messageResponsesHandlers.Count, true);
                foreach (var cnn in _availableConnections)
                    Core.Status.AttachChild(cnn, this);
            });
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Initialize the Transport client
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task InitAsync() => Task.CompletedTask;
        /// <inheritdoc />
        /// <summary>
        /// Initialize the Transport client
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init() { }
        /// <inheritdoc />
        /// <summary>
        /// Gets the descriptors for the RPC service
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<ServiceDescriptorCollection> GetDescriptorsAsync()
        {
            var request = new RPCRequestMessage { MethodId = Guid.Empty };
            var response = await InvokeMethodAsync(request).ConfigureAwait(false);
            return (ServiceDescriptorCollection)response.ReturnValue;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the descriptors for the RPC service
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceDescriptorCollection GetDescriptors()
        {
			var request = new RPCRequestMessage { MethodId = Guid.Empty };
            var response = InvokeMethod(request);
            return (ServiceDescriptorCollection)response.ReturnValue;
        }
        /// <inheritdoc />
        /// <summary>
        /// Invokes a RPC method on the RPC server and gets the results
        /// </summary>
        /// <param name="messageRQ">RPC request message to send to the server</param>
        /// <returns>RPC response message from the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RPCResponseMessage> InvokeMethodAsync(RPCRequestMessage messageRQ)
        {
            if (Status != ConnectionStatus.Connected)
            {
                await ConnectAsync().ConfigureAwait(false);
                if (Status != ConnectionStatus.Connected)
                    throw new Exception("Couldn't connect to the remote server {0}:{1} [Status: {2}, Target: {3}]".ApplyFormat(Host, Port, Status, TargetStatus));
            }
            if (_token.IsCancellationRequested) return null;
            var messageId = messageRQ.MessageId;
            var wh = _messageResponsesHandlers.GetOrAdd(messageId, id => new RPCResponseHandler());
            //Core.Log.LibVerbose("Getting Connection for message Id={0}", messageId);
            var socketConnection = GetSocketConnection();
            //Core.Log.LibVerbose("Connection selected, sending message Id={0} [SessionId={1}]", messageId, socketConnection.SessionId);
            await socketConnection.SendRequestMessageAsync(messageRQ).ConfigureAwait(false);
            //Core.Log.LibVerbose("Waiting response message for Id: {0} [SessionId={1}]", messageId, socketConnection.SessionId);
            if (wh.Event.Wait(InvokeMethodTimeout, socketConnection.OnlineToken))
            {
                return wh.Message;
            }
            if (_messageRetries.Contains(messageId))
            {
                _messageRetries.Remove(messageId);
                throw new TimeoutException("Timeout of {0} seconds has been reached waiting the response from the server with Id={1}.".ApplyFormat(InvokeMethodTimeout / 1000, messageId));
            }
            
            if (socketConnection.OnlineToken.IsCancellationRequested)
                Core.Log.Warning("Connection was closed Retrying send message with Id={0}.", messageId);
            else
                Core.Log.Warning("Timeout of {0} seconds has been reached waiting the response from the server with Id={1}, Retrying one more time...", InvokeMethodTimeout / 1000, messageRQ.MessageId);

            socketConnection.ResetConnection();
            _messageRetries.Add(messageId);
            _messageResponsesHandlers.TryRemove(messageId, out var _);
            return await InvokeMethodAsync(messageRQ).ConfigureAwait(false);
        }

        /// <inheritdoc />
        /// <summary>
        /// Invokes a RPC method on the RPC server and gets the results
        /// </summary>
        /// <param name="messageRQ">RPC request message to send to the server</param>
        /// <returns>RPC response message from the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RPCResponseMessage InvokeMethod(RPCRequestMessage messageRQ)
        {
            while (true)
            {
                if (Status != ConnectionStatus.Connected)
                {
                    ConnectAsync().WaitAsync();
                    if (Status != ConnectionStatus.Connected)
                        throw new Exception("Couldn't connect to the remote server {0}:{1} [Status: {2}, Target: {3}]".ApplyFormat(Host, Port, Status, TargetStatus));
                }
                if (_token.IsCancellationRequested) return null;
                var messageId = messageRQ.MessageId;
                var wh = _messageResponsesHandlers.GetOrAdd(messageId, id => new RPCResponseHandler());
                var socketConnection = GetSocketConnection();
                socketConnection.SendRequestMessage(messageRQ);
                //Core.Log.LibVerbose("Waiting response message for Id: {0} [SessionId={1}]", messageId, socketConnection.SessionId);
                if (wh.Event.Wait(InvokeMethodTimeout, _token))
                {
                    return wh.Message;
                }
                if (_messageRetries.Contains(messageId))
                {
                    _messageRetries.Remove(messageId);
                    throw new TimeoutException("Timeout of {0} seconds has been reached waiting the response from the server with Id={1}.".ApplyFormat(InvokeMethodTimeout / 1000, messageId));
                }

                Core.Log.Warning("Timeout of {0} seconds has been reached waiting the response from the server with Id={1}, Retrying one more time...".ApplyFormat(InvokeMethodTimeout / 1000, messageId));
                socketConnection.ResetConnection();
                _messageRetries.Add(messageId);
                _messageResponsesHandlers.TryRemove(messageId, out var _);
            }
        }

        /// <summary>
        /// Connect to the transport server
        /// </summary>
        /// <returns>Task as result of the connection process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task ConnectAsync()
        {
            TargetStatus = ConnectionStatus.Connected;
            if (Status == ConnectionStatus.Disconnected)
            {
                Core.Log.LibVerbose("Calling Socket Connects...");
                Status = ConnectionStatus.Connecting;
                _tokenSource?.Cancel();
                _connectionResetEvent.Reset();
                _tokenSource = new CancellationTokenSource();
                _token = _tokenSource.Token;
                OnConnecting?.Invoke(this, new EventArgs());

                try
                {
                    //Core.Log.LibVerbose("Setting available connections tasks...");
                    var tasks = new Task[_availableConnections.Count];
                    for (var i = 0; i < _availableConnections.Count; i++)
                    {
                        var cnn = _availableConnections[i];
                        cnn.Host = Host;
                        cnn.Port = Port;
                        cnn.Hub = Hub;
                        cnn.ReceiveBufferSize = ReceiveBufferSize;
                        cnn.SendBufferSize = SendBufferSize;
                        Core.Log.LibVerbose("Connecting task");
                        tasks[i] = cnn.ConnectAsync();
                    }
                    if (Task.WaitAll(tasks, 10000, _token))
                    {
                        OnConnected?.Invoke(this, new EventArgs());
                        Status = ConnectionStatus.Connected;
                        Core.Log.InfoDetail("Connected to: {0}:{1}", Host, Port);
                        _connectionResetEvent.Set();
                        return Task.CompletedTask;
                    }
                    Core.Log.LibVerbose("Disconnected by connection tasks timeout.");
                    Status = ConnectionStatus.Disconnected;
                    _connectionResetEvent.Set();
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    Core.Log.Error(ex, "Disconnected by connection tasks errors.");
                    Status = ConnectionStatus.Disconnected;
                    _connectionResetEvent.Set();
                    return Task.CompletedTask;
                }
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// Disconnect from the transport server
        /// </summary>
        /// <returns>Task as result of the disconnection process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task DisconnectAsync()
        {
            TargetStatus = ConnectionStatus.Disconnected;
            if (Status != ConnectionStatus.Connected) 
                return Task.CompletedTask;
            Status = ConnectionStatus.Disconnecting;
            OnDisconnecting?.Invoke(this, new EventArgs());
            Core.Log.LibVerbose("Cancelling tasks...");
            _tokenSource?.Cancel();
            _tokenSource = null;
            Task.WaitAll(_availableConnections.Select(a => a.DisconnectAsync()).ToArray(), 11000, _token);
            Status = ConnectionStatus.Disconnected;
            Core.Log.LibVerbose("Disconnected");
            OnDisconnected?.Invoke(this, new EventArgs());
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            DisconnectAsync().WaitAsync();
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SocketConnection CreateSocketConnection()
        {
            var sConn = new SocketConnection(Host, Port, Serializer.DeepClone(), ReceiveBufferSize, SendBufferSize);
            sConn.OnConnected += OnConnectedHandler;
            sConn.OnConnecting += OnConnecting;
            sConn.OnDisconnected += OnDisconnectedHandler;
            sConn.OnDisconnecting += OnDisconnecting;
            sConn.OnResponseMessageReceivedHandler = OnResponseMessageReceivedHandler;
            sConn.OnEventMessageReceivedHandler = OnEventMessageReceivedHandler;
            sConn.OnPushMessageReceivedHandler = OnPushMessageReceivedHandler;
            return sConn;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnConnectedHandler(object sender, EventArgs e)
        {
            lock (_connectedSockets)
            {
                _connectedSockets.Add((SocketConnection)sender);
                Interlocked.Increment(ref _connectedSocketsCount);
            }
            _connectionResetEvent.Set();
            OnConnected?.Invoke(this, e);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnDisconnectedHandler(object sender, EventArgs e)
        {
            var acnn = 0;
            lock (_connectedSockets)
            {
                if (_connectedSockets.Remove((SocketConnection)sender))
                    acnn = Interlocked.Decrement(ref _connectedSocketsCount);
            }
            if (acnn > 0)
                _connectionResetEvent.Set();
            else
                _connectionResetEvent.Reset();
            OnDisconnected?.Invoke(this, e);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnResponseMessageReceivedHandler(RPCResponseMessage message)
        {
            if (!_messageResponsesHandlers.TryGetValue(message.RequestMessageId, out var wh)) 
                return;
            wh.Message = message;
            wh.Event.Set();
            _messageResponsesHandlers.TryRemove(message.RequestMessageId, out var _);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnEventMessageReceivedHandler(RPCEventMessage message)
        {
            _previousMessages.GetOrAdd(message.MessageId, mId =>
            {
                OnEventReceived?.InvokeAsync(this, new EventDataEventArgs(message.ServiceName, message.EventName, message.EventArgs));
                return null;
            });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnPushMessageReceivedHandler(RPCPushMessage message)
        {
            _previousMessages.GetOrAdd(message.MessageId, mId =>
            {
                OnPushMessageReceived?.InvokeAsync(this, new EventArgs<RPCPushMessage>(message));
                return null;
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SocketConnection GetSocketConnection()
        {
            if (_connectedSocketsCount < 1)
                _connectionResetEvent.Wait(5000, _token);
            if (_connectedSocketsCount < 1)
            {
                Status = ConnectionStatus.Disconnected;
                ConnectAsync();
                _connectionResetEvent.Wait(5000, _token);
            }
            lock (_connectedSockets)
            {
                if (_socketTurn >= _connectedSocketsCount) _socketTurn = 0;
                if (_connectedSocketsCount > 0)
                    return _connectedSockets[_socketTurn++];
            }
            throw new Exception($"No connection to server. [connectedSocketsCount={_connectedSocketsCount}, socketTurn={_socketTurn}, availableConnections={_availableConnections.Count}]");
        }
        #endregion

        #region Nested Classes
        private class SocketConnection : IDisposable
        {
            private readonly object _writeLock = new object();
            private readonly object _readLock = new object();
            private readonly ISerializer _serializer;
            private TcpClient _client;
            private DateTime _lastWriteDate;
            private ConnectionStatus _connectionStatus = ConnectionStatus.Disconnected;
            private TargetConnectionStatus _targetStatus = TargetConnectionStatus.Disconnected;
            private SocketStatus _socketStatus = SocketStatus.Disconnected;
            private CancellationTokenSource _connectionTokenSource;
            private CancellationToken _connectionToken;
            private CancellationTokenSource _onlineTokenSource;
            
            //public bool IsConnected => connectionStatus == ConnectionStatus.Connected;
            public CancellationToken OnlineToken => _onlineTokenSource.Token;

            private Task _connectionTask;
            private Task _receiveTask;
            private Timer _pingTimer;
            private BufferedStream _writeStream;
            private BufferedStream _readStream;
            private BytesCounterStream _readCounterStream;
            private BytesCounterStream _writeCounterStream;
            private readonly ManualResetEventSlim _connectionEvent = new ManualResetEventSlim();
            private readonly ManualResetEventSlim _sessionMessageEvent = new ManualResetEventSlim(false);
            private readonly ManualResetEventSlim _receiveMessageEvent = new ManualResetEventSlim(false);
            private readonly RPCTransportCounters _counters;

            public string Host;
            public int Port;
            public string Hub;
            public int ReceiveBufferSize;
            public int SendBufferSize;
            public Guid SessionId { get; private set; }
            public bool IsOnSession { get; private set; }
            public Action<RPCResponseMessage> OnResponseMessageReceivedHandler;
            public Action<RPCEventMessage> OnEventMessageReceivedHandler;
            public Action<RPCPushMessage> OnPushMessageReceivedHandler;


            #region Events
            public event EventHandler OnConnecting;
            public event EventHandler OnConnected;
            public event EventHandler OnDisconnected;
            public event EventHandler OnDisconnecting;
            #endregion

            #region .ctor
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SocketConnection(string host, int port, ISerializer serializer, int receiveBufferSize = 65536, int sendbufferSize = 65536)
            {
                _client = new TcpClient();
                Host = host;
                Port = port;
                ReceiveBufferSize = receiveBufferSize;
                SendBufferSize = sendbufferSize;
                _counters = new RPCTransportCounters();
                _client.NoDelay = true;
                Factory.SetSocketLoopbackFastPath(_client.Client);
                _serializer = serializer;
                _serializer.KnownTypes.Add(typeof(RPCEventMessage));
                _serializer.KnownTypes.Add(typeof(RPCPushMessage));
                _serializer.KnownTypes.Add(typeof(RPCRequestMessage));
                _serializer.KnownTypes.Add(typeof(RPCResponseMessage));
                _serializer.KnownTypes.Add(typeof(RPCSessionRequestMessage));
                _serializer.KnownTypes.Add(typeof(RPCSessionResponseMessage));
                Core.Status.Attach(collection =>
                {
                    collection.Add(nameof(_lastWriteDate), _lastWriteDate);
                    collection.Add(nameof(_connectionStatus), _connectionStatus);
                    collection.Add(nameof(_targetStatus), _targetStatus);
                    collection.Add(nameof(_socketStatus), _socketStatus);
                    Core.Status.AttachChild(_client, this);
                });
            }
            #endregion

            #region Public Method
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async Task ConnectAsync()
            {
                Core.Log.LibVerbose("SocketConnection: ConnectAsync. ConnectionStatus={0}, TargetStatus={1}", _connectionStatus, _targetStatus);
                _targetStatus = TargetConnectionStatus.Connected;
                if (_connectionStatus == ConnectionStatus.Disconnected)
                {
                    Core.Log.LibVerbose("Connecting...");
                    _connectionStatus = ConnectionStatus.Connecting;
                    _connectionEvent.Reset();
                    _receiveMessageEvent.Reset();
                    if (_connectionTokenSource == null)
                    {
                        _connectionTokenSource = new CancellationTokenSource();
                        _connectionToken = _connectionTokenSource.Token;
                    }
                    OnConnecting?.Invoke(this, new EventArgs());

                    BindBackgroundTasks();

                    #region Socket Connection and Stream Configuration
                    _socketStatus = SocketStatus.Connecting;
                    if (_client == null)
                    {
                        Core.Log.LibVerbose("Setting new TcpSocketClient.");
                        _client = new TcpClient()
                        {
                            NoDelay = true,
                        };
                        Factory.SetSocketLoopbackFastPath(_client.Client);
                    }
                    try
                    {
                        Core.Log.LibVerbose("TcpSocketClient connecting...");
                        await _client.ConnectHostAsync(Host, Port).ConfigureAwait(false);
                        Core.Log.LibVerbose("TcpSocketClient connected.");
                    }
                    catch
                    {
                        Core.Log.LibVerbose("Disconnected by connection error.");
                        _socketStatus = SocketStatus.Disconnected;
                        _connectionStatus = ConnectionStatus.Disconnected;
                        _connectionEvent.Set();
                        throw;
                    }
                    _client.NoDelay = true;
                    var netStream = _client.GetStream();
                    _readCounterStream = new BytesCounterStream(netStream);
                    _writeCounterStream = new BytesCounterStream(netStream);
                    _readStream = new BufferedStream(_readCounterStream, ReceiveBufferSize);
                    _writeStream = new BufferedStream(_writeCounterStream, SendBufferSize);
                    _lastWriteDate = DateTime.UtcNow;
                    _receiveMessageEvent.Set();
                    _socketStatus = SocketStatus.Connected;
                    #endregion

                    #region Create Session
                    _sessionMessageEvent.Reset();
                    Core.Log.LibVerbose("Sending session request...");
                    SendSessionRequest();
                    if (!_sessionMessageEvent.Wait(10000, _connectionToken))
                    {
                        _connectionStatus = ConnectionStatus.Disconnected;
                        _client.Dispose();
                        throw new Exception("Timeout exception, session response no received.");
                    }
                    #endregion

                    Core.Log.LibVerbose("Session response received.");
                    _connectionStatus = ConnectionStatus.Connected;
                    Core.Log.InfoDetail("Connected to: {0}:{1} with SessionId={2}", Host, Port, SessionId);
                    _connectionEvent.Set();
                    _onlineTokenSource = new CancellationTokenSource();
                    OnConnected?.Invoke(this, new EventArgs());
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Task DisconnectAsync()
            {
                Core.Log.LibVerbose("SocketConnection: DisconnectAsync. ConnectionStatus={0}, TargetStatus={1}", _connectionStatus, _targetStatus);
                _targetStatus = TargetConnectionStatus.Disconnected;
                if (_connectionStatus == ConnectionStatus.Disconnected) 
                    return Task.CompletedTask;
                Core.Log.LibVerbose("Starting disconnection...");
                _connectionStatus = ConnectionStatus.Disconnecting;
                _onlineTokenSource.Cancel();
                OnDisconnecting?.Invoke(this, new EventArgs());
                Core.Log.LibVerbose("Cancelling tasks...");
                _connectionTokenSource.Cancel();
                try
                {
                    _connectionTask.Wait(1000);
                }
                catch
                {
                    // ignored
                }
                _connectionTask = null;
                _pingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                _pingTimer = null;
                try
                {
                    _receiveTask.Wait(1000);
                }
                catch
                {
                    // ignored
                }
                _receiveTask = null;
                _connectionTokenSource = null;

                Core.Log.LibVerbose("Disconnecting from server...");
                if (_socketStatus != SocketStatus.Disconnected)
                    _socketStatus = SocketStatus.Disconnecting;
                _socketStatus = SocketStatus.Disconnected;
                _client.Dispose();
                _client = null;
                _connectionStatus = ConnectionStatus.Disconnected;

                Core.Log.LibVerbose("Disconnected");
                OnDisconnected?.Invoke(this, new EventArgs());
                return Task.CompletedTask;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                DisconnectAsync().WaitAsync();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async Task SendRequestMessageAsync(RPCRequestMessage messageRQ)
            {
                if (_connectionToken.IsCancellationRequested) return;
                if (_connectionStatus != ConnectionStatus.Connected)
                {
                    if (_targetStatus == TargetConnectionStatus.Connected)
                    {
                        try
                        {
                            _connectionEvent.Wait(5000, _connectionToken);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    else
                        await ConnectAsync().ConfigureAwait(false);

                    if (_connectionStatus != ConnectionStatus.Connected)
                        throw new Exception("Couldn't connect to the remote server {0}:{1} [Status: {2}, Target: {3}]".ApplyFormat(Host, Port, _connectionStatus, _targetStatus));
                }
                if (_connectionToken.IsCancellationRequested) return;
                WriteRPCMessageData(messageRQ, RPCMessageType.RequestMessage);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SendRequestMessage(RPCRequestMessage messageRQ)
            {
                if (_connectionStatus != ConnectionStatus.Connected)
                {
                    if (_targetStatus == TargetConnectionStatus.Connected)
                    {
                        try
                        {
                            _connectionEvent.Wait(5000, _connectionToken);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    else
                        ConnectAsync().WaitAsync();

                    if (_connectionStatus != ConnectionStatus.Connected)
                        throw new Exception("Couldn't connect to the remote server {0}:{1} [Status: {2}, Target: {3}]".ApplyFormat(Host, Port, _connectionStatus, _targetStatus));
                }
                if (_connectionToken.IsCancellationRequested) return;
                WriteRPCMessageData(messageRQ, RPCMessageType.RequestMessage);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ResetConnection()
            {
                _connectionStatus = ConnectionStatus.Disconnected;
                _onlineTokenSource.Cancel();
            }
            #endregion

            #region Internal Write/Read Data
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private (RPCMessageType, RPCMessage) GetRPCMessage()
            {
                lock (_readLock)
                {
                    int mTypeByte;
                    try
                    {
                        mTypeByte = _readStream.ReadByte();
                    }
                    catch
                    {
                        return (RPCMessageType.Unknown, null);
                    }

                    #region Check if is a valid type
                    if (mTypeByte < 11 || mTypeByte > 15)
                        return (RPCMessageType.Unknown, null);
                    #endregion

                    var mTypeEnum = (RPCMessageType)mTypeByte;
                    RPCMessage message = null;

                    #region Check if is Pong
                    if (mTypeEnum == RPCMessageType.Pong)
                    {
                        Core.Log.LibVerbose("Pong message received.");
                        return (mTypeEnum, null);
                    }
                    #endregion

                    try
                    {
						switch(mTypeEnum)
						{
							case RPCMessageType.SessionResponse:
								message = (RPCMessage)_serializer.Deserialize(_readStream, typeof(RPCSessionResponseMessage));
								break;
							case RPCMessageType.ResponseMessage:
								message = (RPCMessage)_serializer.Deserialize(_readStream, typeof(RPCResponseMessage));
								break;
							case RPCMessageType.EventMessage:
								message = (RPCMessage)_serializer.Deserialize(_readStream, typeof(RPCEventMessage));
								break;
							case RPCMessageType.PushMessage:
								message = (RPCMessage)_serializer.Deserialize(_readStream, typeof(RPCPushMessage));
								break;
						}
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                    if (_readCounterStream != null)
                        _counters.SetBytesReceived(_readCounterStream.BytesRead);
                    return (mTypeEnum, message);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteRPCMessageData(RPCMessage message, RPCMessageType messageType)
            {
                lock (_writeLock)
                {
                    if (_socketStatus != SocketStatus.Connected)
                    {
                        Core.Log.Warning("Message with Id:{0} was skipped due the socket is not connected.", message.MessageId);
                        return;
                    }
                    try
                    {
                        _writeStream.WriteByte((byte)messageType);
                        try
                        {
                            _serializer.Serialize(message, _writeStream);
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                        _writeStream.Flush();
                        if (_writeCounterStream != null)
                            _counters.SetBytesSent(_writeCounterStream.BytesWrite);
                    }
                    catch (Exception ex)
                    {
                        _connectionStatus = ConnectionStatus.Disconnected;
                        _onlineTokenSource.Cancel();
                        Core.Log.Write(ex);
                    }
                }
            }
            #endregion

            #region Private Tasks Handling
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void BindBackgroundTasks()
            {
                Core.Log.LibVerbose("Binding Background Tasks...");
                if (_connectionTask == null)
                {
                    _connectionTask = Task.Factory.StartNew(ConnectionTaskHandlerAsync, _connectionToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    Core.Log.LibVerbose("Connection task created");
                }
                if (_receiveTask == null)
                {
                    _receiveTask = Task.Factory.StartNew(ReceiveTaskHandler, _connectionToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    Core.Log.LibVerbose("Receive task created");
                }
                if (_pingTimer == null)
                {
                    _pingTimer = new Timer(PingTimerHandler, this, TimeSpan.FromSeconds(45), TimeSpan.FromSeconds(45));
                    Core.Log.LibVerbose("Ping timer created");
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void SendSessionRequest()
            {
                Core.Log.LibVerbose("Sending session request to server.");
                WriteRPCMessageData(new RPCSessionRequestMessage { SessionId = SessionId, Hub = Hub }, RPCMessageType.SessionRequest);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private async Task ConnectionTaskHandlerAsync()
            {
                Core.Log.LibVerbose("ConnectionTaskHandler started.");
                while (!_connectionToken.IsCancellationRequested)
                {
                    if (_connectionStatus == ConnectionStatus.Disconnected && _targetStatus == TargetConnectionStatus.Connected)
                    {
                        try
                        {
                            #region Disconnection
                            _connectionStatus = ConnectionStatus.Disconnecting;
                            _socketStatus = SocketStatus.Disconnecting;
                            _onlineTokenSource?.Cancel();
                            OnDisconnecting?.Invoke(this, new EventArgs());
                            Core.Log.InfoDetail("Disconnecting from server...");
                            try
                            {
                                if (_client != null)
                                {
                                    _client.Dispose();
                                    _client = null;
                                }
                            }
                            catch
                            {
                                _client = null;
                            }
                            _socketStatus = SocketStatus.Disconnected;
                            _connectionStatus = ConnectionStatus.Disconnected;
                            Core.Log.InfoDetail("Disconnected");
                            OnDisconnected?.Invoke(this, new EventArgs());
                            #endregion

                            #region Reconnection
                            while (!_connectionToken.IsCancellationRequested && _connectionStatus == ConnectionStatus.Disconnected)
                            {
                                try
                                {
                                    Core.Log.InfoDetail("Connecting...");
                                    await ConnectAsync().ConfigureAwait(false);
                                    Core.Log.InfoDetail("Reconected.");
                                }
                                catch (Exception ex)
                                {
                                    Core.Log.Error(ex, "Error trying to reconnect, waiting 10sec to try again...");
                                    await Task.Delay(5000, _connectionToken).ConfigureAwait(false);
                                }
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                    }
                    await Task.Delay(2000, _connectionToken).ConfigureAwait(false);
                }
                _connectionTask = null;
                Core.Log.LibVerbose("ConnectionTaskHandler ended.");
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ReceiveTaskHandler()
            {
                Core.Log.LibVerbose("ReceiveTaskHandler started.");
                while (!_connectionToken.IsCancellationRequested)
                {
                    if (!_receiveMessageEvent.IsSet)
                    {
                        try
                        {
                            if (!_receiveMessageEvent.Wait(10000, _connectionToken))
                                break;
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                    if (_connectionToken.IsCancellationRequested) break;

                    RPCMessageType msgType;
                    RPCMessage message;
                    try
                    {
                        var tpl = GetRPCMessage();
                        msgType = tpl.Item1;
                        message = tpl.Item2;
                    }
                    catch (Exception ex)
                    {
                        _connectionStatus = ConnectionStatus.Disconnected;
                        _onlineTokenSource.Cancel();
                        Core.Log.Write(ex);
                        break;
                    }

                    try
                    {
                        switch (msgType)
                        {
                            case RPCMessageType.Unknown:
                                continue;
                            case RPCMessageType.SessionResponse:
                                var sessionResponse = message as RPCSessionResponseMessage;
                                if (sessionResponse?.Succeed == true)
                                {
                                    SessionId = sessionResponse.SessionId;
                                    IsOnSession = true;
                                    _sessionMessageEvent.Set();
                                }
                                else
                                    Core.Log.LibVerbose("ReadWorker. Session wasn't created.");
                                break;
                            case RPCMessageType.ResponseMessage:
                                OnResponseMessageReceivedHandler((RPCResponseMessage)message);
                                _lastWriteDate = DateTime.UtcNow;
                                break;
                            case RPCMessageType.EventMessage:
                                OnEventMessageReceivedHandler((RPCEventMessage)message);
                                _lastWriteDate = DateTime.UtcNow;
                                break;
                            case RPCMessageType.PushMessage:
                                OnPushMessageReceivedHandler((RPCPushMessage)message);
                                _lastWriteDate = DateTime.UtcNow;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                }
                _receiveTask = null;
                Core.Log.LibVerbose("ReceiveTaskHandler ended.");
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void PingTimerHandler(object obj)
            {
                try
                {
                    var cn = (SocketConnection)obj;
                    if (cn == null) return;
                    if (!cn._connectionToken.IsCancellationRequested)
                    {
                        if (cn._connectionStatus != ConnectionStatus.Connected ||
                            cn._socketStatus != SocketStatus.Connected) return;
                        
                        try
                        {
                            if (!((DateTime.UtcNow - cn._lastWriteDate).TotalSeconds >= 40)) 
                                return
                                    ;
                            lock (cn._writeStream)
                            {
                                cn._writeStream.Write(new[] { (byte)RPCMessageType.Ping }, 0, 1);
                                cn._writeStream.Flush();
                            }
                        }
                        catch
                        {
                            cn._connectionStatus = ConnectionStatus.Disconnected;
                        }
                    }
                    else
                    {
                        cn._pingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                        cn._pingTimer = null;
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
            #endregion

            #region Private Enums
            private enum TargetConnectionStatus : byte
            {
                Disconnected = 0x00,
                Connected = 0x01
            }
            private enum SocketStatus : byte
            {
                Disconnected = 0x00,
                Disconnecting = 0x01,
                Connecting = 0x10,
                Connected = 0x11
            }
            #endregion
        }

        /// <summary>
        /// Connection status
        /// </summary>
        public enum ConnectionStatus
        {
            /// <summary>
            /// The transport is disconnected
            /// </summary>
            Disconnected,
            /// <summary>
            /// The transport is disconnecting
            /// </summary>
            Disconnecting,
            /// <summary>
            /// The transport is connecting
            /// </summary>
            Connecting,
            /// <summary>
            /// The transport is connected
            /// </summary>
            Connected
        }
        #endregion
    }
}
