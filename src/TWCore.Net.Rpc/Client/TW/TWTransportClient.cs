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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Diagnostics.Status;
using TWCore.IO;
using TWCore.Net.RPC.Descriptors;
using TWCore.Serialization;

namespace TWCore.Net.RPC.Client.Transports
{
    /// <summary>
    /// TW RPC Transport client
    /// </summary>
    public class TWTransportClient : ITransportClient
    {
        const string DescriptorMethodId = "{DESCRIPTOR}";
        readonly List<SocketConnection> availableConnections = new List<SocketConnection>();
        readonly List<SocketConnection> connectedSockets = new List<SocketConnection>();
        readonly ConcurrentDictionary<Guid, RPCResponseHandler> MessageResponsesHandlers = new ConcurrentDictionary<Guid, RPCResponseHandler>();
        readonly HashSet<Guid> MessageRetries = new HashSet<Guid>();
        volatile int connectedSocketsCount = 0;
        int socketTurn = 0;
        byte SocketsPerClient = 2;
        CancellationTokenSource tokenSource;
        CancellationToken token;
        ManualResetEventSlim connectionResetEvent = new ManualResetEventSlim(false);

        #region Nested Class
        class RPCResponseHandler
        {
            public ManualResetEventSlim Event = new ManualResetEventSlim(false);
            public RPCResponseMessage Message;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public RPCResponseHandler() { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static RPCResponseHandler GetFromPool()
                => ReferencePool<RPCResponseHandler>.Shared.New();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void StoreToPool(RPCResponseHandler handler)
            {
                handler.Event.Reset();
                handler.Message = null;
                ReferencePool<RPCResponseHandler>.Shared.Store(handler);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Serializer to encode and decode the incoming and outgoing data
        /// </summary>
        [StatusReference]
        public ISerializer Serializer { get; set; }
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
        /// <summary>
        /// Transport Counters
        /// </summary>
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
        public int ReceiveBufferSize { get; set; } = 65536;
        /// <summary>
        /// Send Buffer Size
        /// </summary>
        [StatusProperty]
        public int SendBufferSize { get; set; } = 65536;
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

            for (var i = 0; i < SocketsPerClient; i++)
                availableConnections.Add(CreateSocketConnection());

            Core.Status.Attach(collection =>
            {
                collection.Add("Messages Waiting Response Count", MessageResponsesHandlers.Count);
                foreach (var cnn in availableConnections)
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
                SocketsPerClient = socketsPerClient;

            for (var i = 0; i < SocketsPerClient; i++)
                availableConnections.Add(CreateSocketConnection());

            Core.Status.Attach(collection =>
            {
                collection.Add("Messages Waiting Response Count", MessageResponsesHandlers.Count);
                foreach (var cnn in availableConnections)
                    Core.Status.AttachChild(cnn, this);
            });
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize the Transport client
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task InitAsync() => Task.CompletedTask;
        /// <summary>
        /// Initialize the Transport client
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init() { }
        /// <summary>
        /// Gets the descriptors for the RPC service
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<ServiceDescriptorCollection> GetDescriptorsAsync()
        {
            var request = new RPCRequestMessage { MethodId = DescriptorMethodId };
            var response = await InvokeMethodAsync(request).ConfigureAwait(false);
            return (ServiceDescriptorCollection)response.ReturnValue;
        }
        /// <summary>
        /// Gets the descriptors for the RPC service
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceDescriptorCollection GetDescriptors()
        {
            var request = new RPCRequestMessage { MethodId = DescriptorMethodId };
            var response = InvokeMethod(request);
            return (ServiceDescriptorCollection)response.ReturnValue;
        }
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
            if (token.IsCancellationRequested) return null;
            var messageId = messageRQ.MessageId;
            var wh = MessageResponsesHandlers.GetOrAdd(messageId, id => RPCResponseHandler.GetFromPool());
            //Core.Log.LibVerbose("Getting Connection for message Id={0}", messageId);
            var socketConnection = GetSocketConnection();
            //Core.Log.LibVerbose("Connection selected, sending message Id={0} [SessionId={1}]", messageId, socketConnection.SessionId);
            await socketConnection.SendRequestMessageAsync(messageRQ).ConfigureAwait(false);
            //Core.Log.LibVerbose("Waiting response message for Id: {0} [SessionId={1}]", messageId, socketConnection.SessionId);
            if (wh.Event.Wait(InvokeMethodTimeout, socketConnection.OnlineToken))
            {
                var message = wh.Message;
                RPCResponseHandler.StoreToPool(wh);
                return message;
            }
            else if (MessageRetries.Contains(messageId))
            {
                RPCResponseHandler.StoreToPool(wh);
                MessageRetries.Remove(messageId);
                throw new TimeoutException("Timeout of {0} seconds has been reached waiting the response from the server with Id={1}.".ApplyFormat(InvokeMethodTimeout / 1000, messageId));
            }
            else
            {
                if (socketConnection.OnlineToken.IsCancellationRequested)
                    Core.Log.Warning("Connection was closed Retrying send message with Id={0}.", messageId);
                else
                    Core.Log.Warning("Timeout of {0} seconds has been reached waiting the response from the server with Id={1}, Retrying one more time...", InvokeMethodTimeout / 1000, messageRQ.MessageId);

                socketConnection.ResetConnection();
                MessageRetries.Add(messageId);
                MessageResponsesHandlers.TryRemove(messageId, out var old);
                RPCResponseHandler.StoreToPool(wh);
                return await InvokeMethodAsync(messageRQ).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// Invokes a RPC method on the RPC server and gets the results
        /// </summary>
        /// <param name="messageRQ">RPC request message to send to the server</param>
        /// <returns>RPC response message from the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RPCResponseMessage InvokeMethod(RPCRequestMessage messageRQ)
        {
            if (Status != ConnectionStatus.Connected)
            {
                ConnectAsync().WaitAsync();
                if (Status != ConnectionStatus.Connected)
                    throw new Exception("Couldn't connect to the remote server {0}:{1} [Status: {2}, Target: {3}]".ApplyFormat(Host, Port, Status, TargetStatus));
            }
            if (token.IsCancellationRequested) return null;
            var messageId = messageRQ.MessageId;
            var wh = MessageResponsesHandlers.GetOrAdd(messageId, id => RPCResponseHandler.GetFromPool());
            var socketConnection = GetSocketConnection();
            socketConnection.SendRequestMessage(messageRQ);
            //Core.Log.LibVerbose("Waiting response message for Id: {0} [SessionId={1}]", messageId, socketConnection.SessionId);
            if (wh.Event.Wait(InvokeMethodTimeout, token))
            {
                var message = wh.Message;
                RPCResponseHandler.StoreToPool(wh);
                return message;
            }
            else if (MessageRetries.Contains(messageId))
            {
                RPCResponseHandler.StoreToPool(wh);
                MessageRetries.Remove(messageId);
                throw new TimeoutException("Timeout of {0} seconds has been reached waiting the response from the server with Id={1}.".ApplyFormat(InvokeMethodTimeout / 1000, messageId));
            }
            else
            {
                Core.Log.Warning("Timeout of {0} seconds has been reached waiting the response from the server with Id={1}, Retrying one more time...".ApplyFormat(InvokeMethodTimeout / 1000, messageId));
                socketConnection.ResetConnection();
                MessageRetries.Add(messageId);
                MessageResponsesHandlers.TryRemove(messageId, out var old);
                RPCResponseHandler.StoreToPool(wh);
                return InvokeMethod(messageRQ);
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
                tokenSource?.Cancel();
                connectionResetEvent.Reset();
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;
                OnConnecting?.Invoke(this, new EventArgs());

                try
                {
                    //Core.Log.LibVerbose("Setting available connections tasks...");
                    var tasks = new Task[availableConnections.Count];
                    for(var i = 0; i < availableConnections.Count; i++)
                    {
                        var cnn = availableConnections[i];
                        cnn._host = Host;
                        cnn._port = Port;
                        cnn._hub = Hub;
                        cnn._receiveBufferSize = ReceiveBufferSize;
                        cnn._sendBufferSize = SendBufferSize;
                        Core.Log.LibVerbose("Connecting task");
                        tasks[i] = cnn.ConnectAsync();
                    }
                    if (Task.WaitAll(tasks, 10000, token))
                    {
                        OnConnected?.Invoke(this, new EventArgs());
                        Status = ConnectionStatus.Connected;
                        Core.Log.InfoDetail("Connected to: {0}:{1}", Host, Port);
                        connectionResetEvent.Set();
                        return Task.CompletedTask;
                    }
                    else
                    {
                        Core.Log.LibVerbose("Disconnected by connection tasks timeout.");
                        Status = ConnectionStatus.Disconnected;
                        connectionResetEvent.Set();
                        return Task.CompletedTask;
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Error(ex, "Disconnected by connection tasks errors.");
                    Status = ConnectionStatus.Disconnected;
                    connectionResetEvent.Set();
                    return Task.CompletedTask;
                }
            }
            else
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
            if (Status == ConnectionStatus.Connected)
            {
                Status = ConnectionStatus.Disconnecting;
                OnDisconnecting?.Invoke(this, new EventArgs());
                Core.Log.LibVerbose("Cancelling tasks...");
                tokenSource?.Cancel();
                tokenSource = null;
                Task.WaitAll(availableConnections.Select(a => a.DisconnectAsync()).ToArray(), 11000, token);
                Status = ConnectionStatus.Disconnected;
                Core.Log.LibVerbose("Disconnected");
                OnDisconnected?.Invoke(this, new EventArgs());
            }
            return Task.CompletedTask;
        }
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
        SocketConnection CreateSocketConnection()
        {
            var sConn = new SocketConnection(Host, Port, Serializer.DeepClone(), ReceiveBufferSize, SendBufferSize);
            sConn.OnConnected += OnConnectedHandler;
            sConn.OnConnecting += OnConnecting;
            sConn.OnDisconnected += OnDisconnectedHandler;
            sConn.OnDisconnecting += OnDisconnecting;
            sConn.OnMessageReceiveHandler = OnMessageReceivedHandler;
            return sConn;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnConnectedHandler(object sender, EventArgs e)
        {
            lock (connectedSockets)
            {
                connectedSockets.Add((SocketConnection)sender);
                Interlocked.Increment(ref connectedSocketsCount);
            }
            connectionResetEvent.Set();
            OnConnected?.Invoke(this, e);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnDisconnectedHandler(object sender, EventArgs e)
        {
            var acnn = 0;
            lock (connectedSockets)
            {
                if (connectedSockets.Remove((SocketConnection)sender))
                    acnn = Interlocked.Decrement(ref connectedSocketsCount);
            }
            if (acnn > 0)
                connectionResetEvent.Set();
            else
                connectionResetEvent.Reset();
            OnDisconnected?.Invoke(this, e);
        }

        LRU2QCollection<Guid, object> previousMessages = new LRU2QCollection<Guid, object>(4096);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnMessageReceivedHandler(RPCMessageType msgType, RPCMessage message)
        {
            switch (msgType)
            {
                case RPCMessageType.ResponseMessage:
                    var messageResponse = (RPCResponseMessage)message;
                    if (MessageResponsesHandlers.TryGetValue(messageResponse.RequestMessageId, out var wh))
                    {
                        wh.Message = messageResponse;
                        wh.Event.Set();
                        MessageResponsesHandlers.TryRemove(messageResponse.RequestMessageId, out var whOld);
                        //Core.Log.LibVerbose("Message response received for Id: {0}", messageResponse.RequestMessageId);
                    }
                    break;
                case RPCMessageType.EventMessage:
                    lock (previousMessages)
                    {
                        previousMessages.GetOrAdd(message.MessageId, _id =>
                        {
                            var eventTrigger = (RPCEventMessage)message;
                            OnEventReceived?.InvokeAsync(this, new EventDataEventArgs(eventTrigger.ServiceName, eventTrigger.EventName, eventTrigger.EventArgs));
                            return null;
                        });
                    }
                    break;
                case RPCMessageType.PushMessage:
                    lock (previousMessages)
                    {
                        previousMessages.GetOrAdd(message.MessageId, _id =>
                        {
                            OnPushMessageReceived?.InvokeAsync(this, new EventArgs<RPCPushMessage>((RPCPushMessage)message));
                            return null;
                        });
                    }
                    break;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        SocketConnection GetSocketConnection()
        {
            if (connectedSocketsCount < 1)
                connectionResetEvent.Wait(5000, token);
            if (connectedSocketsCount < 1)
            {
                Status = ConnectionStatus.Disconnected;
                ConnectAsync();
                connectionResetEvent.Wait(5000, token);
            }
            lock (connectedSockets)
            {
                if (socketTurn >= connectedSocketsCount) socketTurn = 0;
                if (connectedSocketsCount > 0)
                    return connectedSockets[socketTurn++];
            }
            throw new Exception($"No connection to server. [connectedSocketsCount={connectedSocketsCount}, socketTurn={socketTurn}, availableConnections={availableConnections.Count}]");
        }
        #endregion

        #region Nested Classes
        private class SocketConnection : IDisposable
        {
            object writeLock = new object();
            object readLock = new object();

            TcpClient client;
            ISerializer Serializer;
            DateTime lastWriteDate;
            ConnectionStatus connectionStatus = ConnectionStatus.Disconnected;
            TargetConnectionStatus targetStatus = TargetConnectionStatus.Disconnected;
            SocketStatus socketStatus = SocketStatus.Disconnected;

            public bool IsConnected => connectionStatus == ConnectionStatus.Connected;

            CancellationTokenSource connectionTokenSource;
            CancellationToken connectionToken;

            CancellationTokenSource OnlineTokenSource;
            public CancellationToken OnlineToken => OnlineTokenSource.Token;

            Task ConnectionTask;
            Task ReceiveTask;
            Timer PingTimer;

            Stream WriteStream;
            Stream ReadStream;
            BinaryReader Reader;
            IO.BytesCounterStream readCounterStream;
            IO.BytesCounterStream writeCounterStream;
            ManualResetEventSlim connectionEvent = new ManualResetEventSlim();
            ManualResetEventSlim sessionMessageEvent = new ManualResetEventSlim(false);
            ManualResetEventSlim receiveMessageEvent = new ManualResetEventSlim(false);

            public string _host;
            public int _port;
            public string _hub;
            public int _receiveBufferSize = 65536;
            public int _sendBufferSize = 65536;
            public RPCTransportCounters _counters;
            public Guid SessionId { get; private set; }
            public bool IsOnSession { get; private set; } = false;
            public Action<RPCMessageType, RPCMessage> OnMessageReceiveHandler;

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
                client = new TcpClient();
                _host = host;
                _port = port;
                _receiveBufferSize = receiveBufferSize;
                _sendBufferSize = sendbufferSize;
                _counters = new RPCTransportCounters();
                client.NoDelay = true;
                Factory.SetSocketLoopbackFastPath(client.Client);
                Serializer = serializer;
                Serializer.KnownTypes.Add(typeof(RPCEventMessage));
                Serializer.KnownTypes.Add(typeof(RPCPushMessage));
                Serializer.KnownTypes.Add(typeof(RPCRequestMessage));
                Serializer.KnownTypes.Add(typeof(RPCResponseMessage));
                Serializer.KnownTypes.Add(typeof(RPCSessionRequestMessage));
                Serializer.KnownTypes.Add(typeof(RPCSessionResponseMessage));
                Core.Status.Attach(collection =>
                {
                    collection.Add(nameof(lastWriteDate), lastWriteDate);
                    collection.Add(nameof(connectionStatus), connectionStatus);
                    collection.Add(nameof(targetStatus), targetStatus);
                    collection.Add(nameof(socketStatus), socketStatus);
                    Core.Status.AttachChild(client, this);
                });
            }
            #endregion

            #region Public Method
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async Task ConnectAsync()
            {
                Core.Log.LibVerbose("SocketConnection: ConnectAsync. ConnectionStatus={0}, TargetStatus={1}", connectionStatus, targetStatus);
                targetStatus = TargetConnectionStatus.Connected;
                if (connectionStatus == ConnectionStatus.Disconnected)
                {
                    Core.Log.LibVerbose("Connecting...");
                    connectionStatus = ConnectionStatus.Connecting;
                    connectionEvent.Reset();
                    receiveMessageEvent.Reset();
                    if (connectionTokenSource == null)
                    {
                        connectionTokenSource = new CancellationTokenSource();
                        connectionToken = connectionTokenSource.Token;
                    }
                    OnConnecting?.Invoke(this, new EventArgs());

                    BindBackgroundTasks();

                    #region Socket Connection and Stream Configuration
                    socketStatus = SocketStatus.Connecting;
                    if (client == null)
                    {
                        Core.Log.LibVerbose("Setting new TcpSocketClient.");
                        client = new TcpClient()
                        {
                            NoDelay = true,
                            SendBufferSize = _sendBufferSize,
                            ReceiveBufferSize = _receiveBufferSize
                        };
                        Factory.SetSocketLoopbackFastPath(client.Client);
                    }
                    try
                    {
                        Core.Log.LibVerbose("TcpSocketClient connecting...");
                        await client.ConnectHostAsync(_host, _port).ConfigureAwait(false);
                        Core.Log.LibVerbose("TcpSocketClient connected.");
                    }
                    catch
                    {
                        Core.Log.LibVerbose("Disconnected by connection error.");
                        socketStatus = SocketStatus.Disconnected;
                        connectionStatus = ConnectionStatus.Disconnected;
                        connectionEvent.Set();
                        throw;
                    }
                    client.NoDelay = true;
                    client.ReceiveBufferSize = _receiveBufferSize;
                    client.SendBufferSize = _sendBufferSize;
                    var netStream = client.GetStream();
                    readCounterStream = new BytesCounterStream(netStream);
                    readCounterStream.OnBytesRead += (s, e) => _counters.IncrementBytesReceived(e);
                    writeCounterStream = new BytesCounterStream(netStream);
                    writeCounterStream.OnBytesWrite += (s, e) => _counters.IncrementBytesSent(e);
                    ReadStream = new BufferedStream(readCounterStream, _receiveBufferSize);
                    WriteStream = new BufferedStream(writeCounterStream, _sendBufferSize);
                    Reader = new BinaryReader(ReadStream, Encoding.UTF8, true);
                    lastWriteDate = DateTime.UtcNow;
                    receiveMessageEvent.Set();
                    socketStatus = SocketStatus.Connected;
                    #endregion

                    #region Create Session
                    sessionMessageEvent.Reset();
                    Core.Log.LibVerbose("Sending session request...");
                    SendSessionRequest();
                    if (!sessionMessageEvent.Wait(10000, connectionToken))
                    {
                        connectionStatus = ConnectionStatus.Disconnected;
                        client.Dispose();
                        throw new Exception("Timeout exception, session response no received.");
                    }
                    #endregion

                    Core.Log.LibVerbose("Session response received.");
                    connectionStatus = ConnectionStatus.Connected;
                    Core.Log.InfoDetail("Connected to: {0}:{1} with SessionId={2}", _host, _port, SessionId);
                    connectionEvent.Set();
                    OnlineTokenSource = new CancellationTokenSource();
                    OnConnected?.Invoke(this, new EventArgs());
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Task DisconnectAsync()
            {
                Core.Log.LibVerbose("SocketConnection: DisconnectAsync. ConnectionStatus={0}, TargetStatus={1}", connectionStatus, targetStatus);
                targetStatus = TargetConnectionStatus.Disconnected;
                if (connectionStatus != ConnectionStatus.Disconnected)
                {
                    Core.Log.LibVerbose("Starting disconnection...");
                    connectionStatus = ConnectionStatus.Disconnecting;
                    OnlineTokenSource.Cancel();
                    OnDisconnecting?.Invoke(this, new EventArgs());
                    Core.Log.LibVerbose("Cancelling tasks...");
                    connectionTokenSource.Cancel();
                    try
                    {
                        ConnectionTask.Wait(1000);
                    }
                    catch { }
                    ConnectionTask = null;
                    PingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    PingTimer = null;
                    try
                    {
                        ReceiveTask.Wait(1000);
                    }
                    catch { }
                    ReceiveTask = null;
                    connectionTokenSource = null;

                    Core.Log.LibVerbose("Disconnecting from server...");
                    if (socketStatus != SocketStatus.Disconnected)
                        socketStatus = SocketStatus.Disconnecting;
                    socketStatus = SocketStatus.Disconnected;
                    client.Dispose();
                    client = null;
                    connectionStatus = ConnectionStatus.Disconnected;

                    Core.Log.LibVerbose("Disconnected");
                    OnDisconnected?.Invoke(this, new EventArgs());
                    return Task.CompletedTask;
                }
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
                if (connectionToken.IsCancellationRequested) return;
                if (connectionStatus != ConnectionStatus.Connected)
                {
                    if (targetStatus == TargetConnectionStatus.Connected)
                    {
                        try
                        {
                            connectionEvent.Wait(5000, connectionToken);
                        }
                        catch { }
                    }
                    else
                        await ConnectAsync().ConfigureAwait(false);

                    if (connectionStatus != ConnectionStatus.Connected)
                        throw new Exception("Couldn't connect to the remote server {0}:{1} [Status: {2}, Target: {3}]".ApplyFormat(_host, _port, connectionStatus, targetStatus));
                }
                if (connectionToken.IsCancellationRequested) return;
                WriteRPCMessageData(messageRQ, RPCMessageType.RequestMessage);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SendRequestMessage(RPCRequestMessage messageRQ)
            {
                if (connectionToken.IsCancellationRequested) return;
                if (connectionStatus != ConnectionStatus.Connected)
                {
                    if (targetStatus == TargetConnectionStatus.Connected)
                    {
                        try
                        {
                            connectionEvent.Wait(5000, connectionToken);
                        }
                        catch { }
                    }
                    else
                        ConnectAsync().WaitAsync();

                    if (connectionStatus != ConnectionStatus.Connected)
                        throw new Exception("Couldn't connect to the remote server {0}:{1} [Status: {2}, Target: {3}]".ApplyFormat(_host, _port, connectionStatus, targetStatus));
                }
                if (connectionToken.IsCancellationRequested) return;
                WriteRPCMessageData(messageRQ, RPCMessageType.RequestMessage);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ResetConnection()
            {
                connectionStatus = ConnectionStatus.Disconnected;
                OnlineTokenSource.Cancel();
            }
            #endregion

            #region Internal Write/Read Data
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            (RPCMessageType, RPCMessage) GetRPCMessage(BinaryReader reader)
            {
                RPCMessage message = null;
                RPCMessageType mTypeEnum;
                byte mTypeByte;
                lock (readLock)
                {
                    try
                    {
                        mTypeByte = reader.ReadByte();
                    }
                    catch
                    {
                        return (RPCMessageType.Unknown, null);
                    }
                    mTypeEnum = (RPCMessageType)mTypeByte;

                    #region Check if is a valid type
                    if (mTypeEnum != RPCMessageType.Pong &&
                        mTypeEnum != RPCMessageType.SessionResponse &&
                        mTypeEnum != RPCMessageType.ResponseMessage &&
                        mTypeEnum != RPCMessageType.EventMessage &&
                        mTypeEnum != RPCMessageType.PushMessage)
                        return (RPCMessageType.Unknown, message);
                    #endregion

                    if (mTypeEnum != RPCMessageType.Pong)
                    {
                        #region Get Type
                        Type type = typeof(RPCMessage);
                        switch (mTypeEnum)
                        {
                            case RPCMessageType.ResponseMessage:
                                type = typeof(RPCResponseMessage);
                                break;
                            case RPCMessageType.SessionResponse:
                                type = typeof(RPCSessionResponseMessage);
                                break;
                            case RPCMessageType.EventMessage:
                                type = typeof(RPCEventMessage);
                                break;
                            case RPCMessageType.PushMessage:
                                type = typeof(RPCPushMessage);
                                break;
                        }
                        #endregion
                        try
                        {
                            message = (RPCMessage)Serializer.Deserialize(reader.BaseStream, type);
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                    }
                    else
                        Core.Log.LibVerbose("Pong message received.");
                }
                return (mTypeEnum, message);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void WriteRPCMessageData(RPCMessage message, RPCMessageType messageType)
            {
                lock (writeLock)
                {
                    if (socketStatus == SocketStatus.Connected)
                    {
                        try
                        {
                            WriteStream.WriteByte((byte)messageType);
                            try
                            {
                                Serializer.Serialize(message, WriteStream);
                            }
                            catch (Exception ex)
                            {
                                Core.Log.Write(ex);
                            }
                            WriteStream.Flush();
                            //Core.Log.LibVerbose("Message with Id: {0} was sent. Total Bytes sent on the stream = {1}", message.MessageId, writeCounterStream.BytesWrite);
                        }
                        catch (Exception ex)
                        {
                            connectionStatus = ConnectionStatus.Disconnected;
                            OnlineTokenSource.Cancel();
                            Core.Log.Write(ex);
                        }
                    }
                    else
                    {
                        Core.Log.LibVerbose("Message with Id:{0} was skipped due the socket is not connected.", message.MessageId);
                    }
                }
            }
            #endregion

            #region Private Tasks Handling
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void BindBackgroundTasks()
            {
                Core.Log.LibVerbose("Binding Background Tasks...");
                if (ConnectionTask == null)
                {
                    ConnectionTask = Task.Factory.StartNew(ConnectionTaskHandlerAsync, connectionToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    Core.Log.LibVerbose("Connection task created");
                }
                if (ReceiveTask == null)
                {
                    ReceiveTask = Task.Factory.StartNew(ReceiveTaskHandler, connectionToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    Core.Log.LibVerbose("Receive task created");
                }
                if (PingTimer == null)
                {
                    PingTimer = new Timer(PingTimerHandler, this, TimeSpan.FromSeconds(45), TimeSpan.FromSeconds(45));
                    Core.Log.LibVerbose("Ping timer created");
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void SendSessionRequest()
            {
                Core.Log.LibVerbose("Sending session request to server.");
                WriteRPCMessageData(new RPCSessionRequestMessage { SessionId = SessionId, Hub = _hub }, RPCMessageType.SessionRequest);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            async Task ConnectionTaskHandlerAsync()
            {
                Core.Log.LibVerbose("ConnectionTaskHandler started.");
                while (!connectionToken.IsCancellationRequested)
                {
                    if (connectionStatus == ConnectionStatus.Disconnected && targetStatus == TargetConnectionStatus.Connected)
                    {
                        try
                        {
                            #region Disconnection
                            connectionStatus = ConnectionStatus.Disconnecting;
                            socketStatus = SocketStatus.Disconnecting;
                            OnlineTokenSource?.Cancel();
                            OnDisconnecting?.Invoke(this, new EventArgs());
                            Core.Log.InfoDetail("Disconnecting from server...");
                            try
                            {
                                if (client != null)
                                {
                                    client.Dispose();
                                    client = null;
                                }
                            }
                            catch
                            {
                                client = null;
                            }
                            socketStatus = SocketStatus.Disconnected;
                            connectionStatus = ConnectionStatus.Disconnected;
                            Core.Log.InfoDetail("Disconnected");
                            OnDisconnected?.Invoke(this, new EventArgs());
                            #endregion

                            #region Reconnection
                            while (!connectionToken.IsCancellationRequested && connectionStatus == ConnectionStatus.Disconnected)
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
                                    await Task.Delay(5000, connectionToken).ConfigureAwait(false);
                                }
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                    }
                    await Task.Delay(2000, connectionToken).ConfigureAwait(false);
                }
                ConnectionTask = null;
                Core.Log.LibVerbose("ConnectionTaskHandler ended.");
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void ReceiveTaskHandler()
            {
                Core.Log.LibVerbose("ReceiveTaskHandler started.");
                while (!connectionToken.IsCancellationRequested)
                {
                    try
                    {
                        if (!receiveMessageEvent.Wait(10000, connectionToken))
                            break;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                    if (!connectionToken.IsCancellationRequested)
                    {
                        try
                        {
                            (RPCMessageType msgType, RPCMessage message) = GetRPCMessage(Reader);
                            if (msgType == RPCMessageType.Unknown)
                                continue;
                            if (msgType == RPCMessageType.SessionResponse)
                            {
                                var sessionResponse = message as RPCSessionResponseMessage;
                                if (sessionResponse?.Succeed == true)
                                {
                                    SessionId = sessionResponse.SessionId;
                                    IsOnSession = true;
                                    sessionMessageEvent.Set();
                                }
                                else
                                    Core.Log.LibVerbose("ReadWorker. Session wasn't created.");
                            }
                            else
                            {
                                OnMessageReceiveHandler(msgType, message);
                                lastWriteDate = DateTime.UtcNow;
                            }
                        }
                        catch (Exception ex)
                        {
                            connectionStatus = ConnectionStatus.Disconnected;
                            OnlineTokenSource.Cancel();
                            Core.Log.Write(ex);
                            break;
                        }
                    }
                }
                ReceiveTask = null;
                Core.Log.LibVerbose("ReceiveTaskHandler ended.");
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void PingTimerHandler(object obj)
            {
                try
                {
                    var cn = (SocketConnection)obj;
                    if (cn != null)
                    {
                        if (!cn.connectionToken.IsCancellationRequested)
                        {
                            if (cn.connectionStatus == ConnectionStatus.Connected && cn.socketStatus == SocketStatus.Connected)
                            {
                                try
                                {
                                    if ((DateTime.UtcNow - cn.lastWriteDate).TotalSeconds >= 40)
                                    {
                                        lock (cn.WriteStream)
                                        {
                                            cn.WriteStream.Write(new byte[] { (byte)RPCMessageType.Ping }, 0, 1);
                                            cn.WriteStream.Flush();
                                        }
                                    }
                                }
                                catch
                                {
                                    cn.connectionStatus = ConnectionStatus.Disconnected;
                                }
                            }
                        }
                        else
                        {
                            cn.PingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                            cn.PingTimer = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
            #endregion

            #region Private Enums
            enum ConnectionStatus : byte
            {
                Disconnected = 0x00,
                Disconnecting = 0x10,
                Connecting = 0x20,
                Connected = 0x30,
            }
            enum TargetConnectionStatus : byte
            {
                Disconnected = 0x00,
                Connected = 0x01
            }
            enum SocketStatus : byte
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
