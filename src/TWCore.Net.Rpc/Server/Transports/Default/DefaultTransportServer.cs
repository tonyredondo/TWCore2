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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Net.RPC.Attributes;
using TWCore.Serialization;
using TWCore.Threading;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable MethodSupportsCancellation
// ReSharper disable InconsistentNaming
#pragma warning disable 4014

namespace TWCore.Net.RPC.Server.Transports.Default
{
    /// <inheritdoc />
    /// <summary>
    /// Default RPC Transport Server
    /// </summary>
    [StatusName("Transport")]
    public class DefaultTransportServer : ITransportServer
    {
        private readonly object _locker = new object();
        private readonly List<RpcServerClient> _sessions = new List<RpcServerClient>();
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _rpcMessagesCancellations = new ConcurrentDictionary<Guid, CancellationTokenSource>();
        private TcpListener _listener;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private Task _tskListener;
        private Action<object> _connectionReceivedAction;

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Transport name, should be the same name for Server and Client
        /// </summary>
        [StatusProperty]
        public string Name => "DefaultTransport";
        /// <inheritdoc />
        /// <summary>
        /// true if the transport server can send the services descriptors; otherwise, false
        /// </summary>
        [StatusProperty("Get Descriptors Enabled")]
        public bool EnableGetDescriptors 
        { 
            get => false;
            set { } 
        }
        /// <inheritdoc />
        /// <summary>
        /// Serializer to encode and decode the incoming and outgoing data
        /// </summary>
        [StatusProperty]
        public ISerializer Serializer { get; set; }
        /// <summary>
        /// Server port
        /// </summary>
        [StatusProperty]
        public int Port { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Event that fires when a Descriptor request is received.
        /// </summary>
        public event EventHandler<ServerDescriptorsEventArgs> OnGetDescriptorsRequest;
        /// <summary>
        /// Event that fires when a Method call is received
        /// </summary>
        public AsyncEvent<MethodEventArgs> OnMethodCallAsync { get; set; }
        /// <summary>
        /// Event that fires when a Method response is sent
        /// </summary>
        public event EventHandler<RPCResponseMessage> OnResponseSent;
        /// <summary>
        /// Event that fires when a client connects.
        /// </summary>
        public event EventHandler<ClientConnectEventArgs> OnClientConnect;
        #endregion

        #region .ctor
        /// <summary>
        /// Default RPC Transport Server
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DefaultTransportServer()
        {
            Serializer = Serializer ?? SerializerManager.DefaultBinarySerializer.DeepClone();
            _connectionReceivedAction = ConnectionReceived;
            Core.Status.Attach(collection =>
            {
                lock (_locker)
                {
                    collection.Add("Sessions Count", _sessions.Count, true);
                }
            });
        }
        /// <inheritdoc />
        /// <summary>
        /// Default RPC Transport Server
        /// </summary>
        /// <param name="port">Server port</param>
        /// <param name="serializer">Serializer for data transfer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DefaultTransportServer(int port, ISerializer serializer = null) : this()
        {
            Port = port;
            Serializer = serializer ?? Serializer;
        }
        #endregion


        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Starts the server listener
        /// </summary>
        /// <returns>Task as result of the startup process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task StartListenerAsync()
        {
            Core.Log.LibVerbose("Starting Transport Listener");
            if (Serializer != null)
            {
                Serializer.KnownTypes.Add(typeof(RPCError));
                Serializer.KnownTypes.Add(typeof(RPCCancelMessage));
                Serializer.KnownTypes.Add(typeof(RPCEventMessage));
                Serializer.KnownTypes.Add(typeof(RPCPushMessage));
                Serializer.KnownTypes.Add(typeof(RPCRequestMessage));
                Serializer.KnownTypes.Add(typeof(RPCResponseMessage));
                Serializer.KnownTypes.Add(typeof(RPCSessionRequestMessage));
                Serializer.KnownTypes.Add(typeof(RPCSessionResponseMessage));
            }
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Server.NoDelay = true;
            Factory.SetSocketLoopbackFastPath(_listener.Server);
            _listener.Start();
            _tskListener = ConnectionListenerAsync();
            Core.Log.LibVerbose("Transport Listener Started");
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        /// <summary>
        /// Stops the server listener
        /// </summary>
        /// <returns>Task as result of the stop process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task StopListenerAsync()
        {
            Core.Log.LibVerbose("Stopping Transport Listener");
            try
            {
                _tokenSource.Cancel();
                await _tskListener.ConfigureAwait(false);
                _listener.Stop();
                _listener = null;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            lock (_locker)
            {
                var tmpSessions = _sessions.ToArray();
                foreach (var i in tmpSessions)
                    i.Dispose();
                _sessions.Clear();
            }
            Core.Log.LibVerbose("Transport Listener Stopped");
        }
        /// <inheritdoc />
        /// <summary>
        /// Send a fire event trigger to a RPC client.
        /// </summary>
        /// <param name="eventAttribute">Event attribute</param>
        /// <param name="clientId">Client identifier</param>
        /// <param name="serviceName">Service name</param>
        /// <param name="eventName">Event name</param>
        /// <param name="sender">Sender information</param>
        /// <param name="e">Event args</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task FireEventAsync(RPCEventAttribute eventAttribute, Guid clientId, string serviceName, string eventName, object sender, EventArgs e)
        {
            try
            {
                var eventMessage = new RPCEventMessage {ServiceName = serviceName, EventName = eventName, EventArgs = e};

                switch (eventAttribute.Scope)
                {
                    case RPCMessageScope.Session:
                        RpcServerClient client = null;
                        lock (_locker)
                        {
                            foreach (var s in _sessions)
                            {
                                if (s.OnSession && s.SessionId == clientId)
                                {
                                    client = s;
                                    break;
                                }
                            }
                        }
                        if (client != null)
                        {
                            await client.SendRpcMessageAsync(eventMessage).ConfigureAwait(false);
                            Core.Log.LibVerbose($"Sending event trigger to SessionId='{clientId}' on event '{eventName}'");
                        }
                        break;
                    case RPCMessageScope.Hub:
                        var hubName = eventAttribute.HubName;
                        var clients = new List<RpcServerClient>();
                        lock (_locker)
                        {
                            foreach (var s in _sessions)
                            {
                                if (s.OnSession && s.Hub == hubName)
                                    clients.Add(s);
                            }
                        }
                        if (clients.Count > 0)
                        {
                            await Task.WhenAll(clients.Select((s, eMessage) => s.SendRpcMessageAsync(eMessage), eventMessage)).ConfigureAwait(false);
                            Core.Log.LibVerbose($"Sending event trigger to Hub='{hubName}' on event '{eventName}'");
                        }
                        break;
                    case RPCMessageScope.Global:
                        var gClients = new List<RpcServerClient>();
                        lock (_locker)
                        {
                            foreach (var s in _sessions)
                            {
                                if (s.OnSession)
                                    gClients.Add(s);
                            }
                        }
                        if (gClients.Count > 0)
                        {
                            await Task.WhenAll(gClients.Select((s, eMessage) => s.SendRpcMessageAsync(eMessage), eventMessage)).ConfigureAwait(false);
                            Core.Log.LibVerbose($"Sending event trigger to all sessions on event '{eventName}'");
                        }
                        break;
                }
            }
            catch(Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        #endregion

        #region Private Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task ConnectionListenerAsync()
        {
            var tokenTask = _token.WhenCanceledAsync();
            var retries = 5;
            while (!_token.IsCancellationRequested)
            {
                try
                {
                    var listenerTask = _listener.AcceptTcpClientAsync();
                    var rTask = await Task.WhenAny(listenerTask, tokenTask).ConfigureAwait(false);
                    if (rTask == tokenTask) break;
                    Task.Factory.StartNew(_connectionReceivedAction, listenerTask.Result, _token);
                }
                catch(Exception ex)
                {
                    Core.Log.Write(ex);
                    if (Interlocked.Decrement(ref retries) == 0)
                    {
                        Core.Log.Error("The connection listener has been stopped. Too many exceptions.");
                        break;
                    }
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ConnectionReceived(object objTcpClient)
        {
            var client = (TcpClient)objTcpClient;
            try
            {
                if (client.Client.RemoteEndPoint is IPEndPoint remoteIp)
                    Core.Log.InfoBasic("Client Connected from: {0}", remoteIp.Address.ToString());
            }
            catch
            {
                //
            }
            var serverClient = new RpcServerClient(client, (BinarySerializer)Serializer);
            serverClient.OnSessionMessageReceivedAsync += ServerClient_OnSessionMessageReceivedAsync;
            serverClient.OnConnect += ServerClient_OnConnect;
            serverClient.OnDisconnect += ServerClient_OnDisconnect;
            serverClient.OnMessageReceivedAsync += ServerClient_OnMessageReceivedAsync;
            lock (_locker)
            {
                _sessions.Add(serverClient);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task<bool> ServerClient_OnSessionMessageReceivedAsync(RpcServerClient rpcServerClient, RPCSessionRequestMessage sessionMessage)
        {
            return TaskHelper.CompleteTrue;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ServerClient_OnConnect(RpcServerClient rpcServerClient, EventArgs e)
        {
            OnClientConnect?.Invoke(this, new ClientConnectEventArgs(rpcServerClient.SessionId));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task ServerClient_OnMessageReceivedAsync(RpcServerClient rpcServerClient, RPCMessage e)
        {
            switch (e)
            {
                case RPCCancelMessage cancelMessage:
                    if (_rpcMessagesCancellations.TryRemove(cancelMessage.MessageId, out var tSource))
                        tSource.Cancel();
                    break;
                case RPCRequestMessage request:
                    if (request.MethodId == Guid.Empty)
                    {
                        var dEventArgs = new ServerDescriptorsEventArgs();
                        OnGetDescriptorsRequest?.InvokeAsync(this, dEventArgs);
                        var response = new RPCResponseMessage(request) { ReturnValue = dEventArgs.Descriptors };
                        await rpcServerClient.SendRpcMessageAsync(response).ConfigureAwait(false);
                        OnResponseSent?.Invoke(this, response);
                        break;
                    }
                    if (request.CancellationToken)
                    {
                        using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(rpcServerClient.ConnectionCancellationToken, CancellationToken.None))
                        {
                            _rpcMessagesCancellations.TryAdd(request.MessageId, tokenSource);
                            var mEventArgs = new MethodEventArgs(rpcServerClient.SessionId, request, tokenSource.Token);
                            if (!(OnMethodCallAsync is null))
                                await OnMethodCallAsync.InvokeAsync(this, mEventArgs).ConfigureAwait(false);
                            if (!tokenSource.Token.IsCancellationRequested)
                                await rpcServerClient.SendRpcMessageAsync(mEventArgs.Response).ConfigureAwait(false);
                            _rpcMessagesCancellations.TryRemove(request.MessageId, out _);
                            OnResponseSent?.Invoke(this, mEventArgs.Response);
                        }
                        break;
                    }
                    var mEventArgs2 = new MethodEventArgs(rpcServerClient.SessionId, request, rpcServerClient.ConnectionCancellationToken);
                    if (!(OnMethodCallAsync is null))
                        await OnMethodCallAsync.InvokeAsync(this, mEventArgs2).ConfigureAwait(false);
                    if (!rpcServerClient.ConnectionCancellationToken.IsCancellationRequested)
                        await rpcServerClient.SendRpcMessageAsync(mEventArgs2.Response).ConfigureAwait(false);
                    OnResponseSent?.Invoke(this, mEventArgs2.Response);
                    break;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ServerClient_OnDisconnect(RpcServerClient rpcServerClient, EventArgs e)
        {
            rpcServerClient.OnSessionMessageReceivedAsync -= ServerClient_OnSessionMessageReceivedAsync;
            rpcServerClient.OnConnect -= ServerClient_OnConnect;
            rpcServerClient.OnDisconnect -= ServerClient_OnDisconnect;
            rpcServerClient.OnMessageReceivedAsync -= ServerClient_OnMessageReceivedAsync;
            lock (_locker)
            {
                _sessions.Remove(rpcServerClient);
            }
        }
        #endregion

        #region Push Methods
        /// <summary>
        /// Push a message to all sessions in a hub
        /// </summary>
        /// <param name="hub">Hub name</param>
        /// <param name="description">Message description</param>
        /// <param name="message">Message object data</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushMessageToAllHub(string hub, string description, object message)
        {
            var msg = new RPCPushMessage
            {
                Data = message,
                Scope = RPCMessageScope.Hub,
                Description = description
            };
            lock (_locker)
            {
                foreach (var s in _sessions.Where((s, mHub) => s.OnSession && s.Hub == mHub, hub))
                    s.SendRpcMessageAsync(msg);
            }
        }
        /// <summary>
        /// Push a message to all sessions in a hub except the specified sessionId
        /// </summary>
        /// <param name="hub">Hub name</param>
        /// <param name="description">Message description</param>
        /// <param name="message">Message object data</param>
        /// <param name="exceptSessionId">Session id to exclude</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushMessageToAllHub(string hub, string description, object message, Guid exceptSessionId)
        {
            var msg = new RPCPushMessage
            {
                Data = message,
                Scope = RPCMessageScope.Hub,
                Description = description
            };
            lock (_locker)
            {
                foreach (var s in _sessions.Where((s, iHub, iExceptSessionId) => s.OnSession && s.Hub == iHub && s.SessionId != iExceptSessionId, hub, exceptSessionId))
                    s.SendRpcMessageAsync(msg);
            }
        }
        /// <summary>
        /// Push a message to all connected sessions
        /// </summary>
        /// <param name="description">Message description</param>
        /// <param name="message">Message object data</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushMessageToAll(string description, object message)
        {
            var msg = new RPCPushMessage
            {
                Data = message,
                Scope = RPCMessageScope.Global,
                Description = description
            };
            lock (_locker)
            {
                foreach (var s in _sessions.Where(s => s.OnSession))
                    s.SendRpcMessageAsync(msg);
            }
        }
        /// <summary>
        /// Push a message to all connected sessions except the specified sessionId
        /// </summary>
        /// <param name="description">Message description</param>
        /// <param name="message">Message object data</param>
        /// <param name="exceptSessionId">Session id to exclude</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushMessageToAll(string description, object message, Guid exceptSessionId)
        {
            var msg = new RPCPushMessage
            {
                Data = message,
                Scope = RPCMessageScope.Global,
                Description = description
            };
            lock (_locker)
            {
                foreach (var s in _sessions.Where((s, mExceptSessionId) => s.OnSession && s.SessionId != mExceptSessionId, exceptSessionId))
                    s.SendRpcMessageAsync(msg);
            }
        }
        /// <summary>
        /// Push a message to a session
        /// </summary>
        /// <param name="sessionId">Session identifier to send the message</param>
        /// <param name="description">Message description</param>
        /// <param name="message">Message object data</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushMessageTo(Guid sessionId, string description, object message)
        {
            var msg = new RPCPushMessage
            {
                Data = message,
                Scope = RPCMessageScope.Session,
                Description = description
            };
            lock (_locker)
            {
                foreach (var s in _sessions.Where((s, mSessionId) => s.OnSession && s.SessionId == mSessionId, sessionId))
                    s.SendRpcMessageAsync(msg);
            }
        }
        #endregion

    }
}
