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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Net.RPC.Attributes;
using TWCore.Serialization;
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable MethodSupportsCancellation
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore.Net.RPC.Server.Transports.TW
{
    /// <inheritdoc />
    /// <summary>
    /// TW RPC Transport server
    /// </summary>
    public class TWTransportServer : ITransportServer
    {
        private readonly ConcurrentDictionary<TWTransportServerConnection, object> _pendingConnections = new ConcurrentDictionary<TWTransportServerConnection, object>();
        private readonly ConcurrentDictionary<Guid, TWTransportServerConnection> _sessions = new ConcurrentDictionary<Guid, TWTransportServerConnection>();
        private TcpListener _listener;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private Task _tskListener;
        private Timer _disconnectionTimer;

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Transport name, should be the same name for Server and Client
        /// </summary>
        [StatusProperty]
        public string Name => "TWTransport";
        /// <inheritdoc />
        /// <summary>
        /// true if the transport server can send the services descriptors; otherwise, false
        /// </summary>
        [StatusProperty]
        public bool EnableGetDescriptors { get { return false; } set { } }
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
        /// <summary>
        /// Timeout to clean up the disconnected sessions
        /// </summary>
        [StatusProperty]
        public int DisconnectedSessionTimeoutInMinutes { get; set; } = 10;
        /// <inheritdoc />
        /// <summary>
        /// Transport Counters
        /// </summary>
        [StatusReference]
        public RPCTransportCounters Counters { get; } = new RPCTransportCounters();
        #endregion

        #region Events
        /// <summary>
        /// Event that fires when a Descriptors request is received.
        /// </summary>
        public event EventHandler<ServerDescriptorsEventArgs> OnGetDescriptorsRequest;
        /// <summary>
        /// Event that fires when a Method call is received
        /// </summary>
        public event EventHandler<MethodEventArgs> OnMethodCall;
        /// <summary>
        /// Event that fires when a client connects.
        /// </summary>
        public event EventHandler<ClientConnectEventArgs> OnClientConnect;
        #endregion

        #region .ctor
        /// <summary>
        /// TW RPC Transport server
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TWTransportServer()
        {
            Serializer = Serializer ?? SerializerManager.DefaultBinarySerializer.DeepClone();
            Core.Status.Attach(collection =>
            {
                collection.Add("Sessions Count", _sessions.Count, true);
                foreach (var ses in _sessions)
                    Core.Status.AttachChild(ses.Value, this);
            });
        }
        /// <inheritdoc />
        /// <summary>
        /// TW RPC Transport server
        /// </summary>
        /// <param name="port">Server port</param>
        /// <param name="serializer">Serializer for data transfer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TWTransportServer(int port, ISerializer serializer = null) : this()
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
                if (!Serializer.KnownTypes.Contains(typeof(RPCEventMessage)))
                    Serializer.KnownTypes.Add(typeof(RPCEventMessage));
                if (!Serializer.KnownTypes.Contains(typeof(RPCPushMessage)))
                    Serializer.KnownTypes.Add(typeof(RPCPushMessage));
                if (!Serializer.KnownTypes.Contains(typeof(RPCRequestMessage)))
                    Serializer.KnownTypes.Add(typeof(RPCRequestMessage));
                if (!Serializer.KnownTypes.Contains(typeof(RPCResponseMessage)))
                    Serializer.KnownTypes.Add(typeof(RPCResponseMessage));
                if (!Serializer.KnownTypes.Contains(typeof(RPCSessionRequestMessage)))
                    Serializer.KnownTypes.Add(typeof(RPCSessionRequestMessage));
                if (!Serializer.KnownTypes.Contains(typeof(RPCSessionResponseMessage)))
                    Serializer.KnownTypes.Add(typeof(RPCSessionResponseMessage));
            }
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Server.NoDelay = true;
            Factory.SetSocketLoopbackFastPath(_listener.Server);
            _listener.Start();
            _tskListener = Task.Factory.StartNew(async () =>
            {
                while (!_token.IsCancellationRequested)
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync().HandleCancellationAsync(_token).ConfigureAwait(false);
                    ThreadPool.QueueUserWorkItem(ConnectionReceived, tcpClient);
                }
            }, _token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _disconnectionTimer = new Timer(state =>
            {
                var disconnectedSessions = _sessions.Values.ToList().Where(v => v.Disconnected);
                foreach (var session in disconnectedSessions)
                {
                    Core.Log.LibVerbose("Removing SessionId={0}", session.SessionId);
                    session.Dispose();
                    _sessions.TryRemove(session.SessionId, out var _);
                }

            }, this, 10000, 10000);
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
            _disconnectionTimer?.Dispose();
            _disconnectionTimer = null;
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
            _sessions.Values.ToList().Each(session => Try.Do(session.Dispose));
            _sessions.Clear();
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
        public void FireEvent(RPCEventAttribute eventAttribute, Guid clientId, string serviceName, string eventName, object sender, EventArgs e)
        {
            var eventMessage = new RPCEventMessage { ServiceName = serviceName, EventName = eventName, EventArgs = e };

            switch (eventAttribute.Scope)
            {
                case RPCMessageScope.Session:
                    if (_sessions.TryGetValue(clientId, out var client))
                    {
                        client.SendEventMessage(eventMessage);
                        Core.Log.LibVerbose($"Sending event trigger to SessionId='{clientId}' on event '{eventName}'");
                    }
                    break;
                case RPCMessageScope.Hub:
                    var hubName = eventAttribute.HubName;
                    try
                    {
                        var hSessions = _sessions.Values.Where(v => v.Hub == hubName && !v.Disconnected).ToList();
                        foreach (var cItem in hSessions)
                            cItem.SendEventMessage(eventMessage);
                        Core.Log.LibVerbose($"Sending event trigger to Hub='{hubName}' on event '{eventName}'");
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                    break;
                case RPCMessageScope.Global:
                    try
                    {
                        var s = _sessions.Values.Where(v => !v.Disconnected).ToList();
                        foreach (var sItem in s)
                            sItem.SendEventMessage(eventMessage);
                        Core.Log.LibVerbose($"Sending event trigger to all sessions on event '{eventName}'");
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                    break;
            }
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ConnectionReceived(object objTcpClient)
        {
            var client = (TcpClient)objTcpClient;
            var sessionItem = new TWTransportServerConnection(this, client, Serializer)
            {
                OnSessionRequestReceived = SessionItem_OnSessionRequestReceived,
                OnSessionDisconnected = SessionItem_OnSessionDisconnected,
                OnRequestReceived = Session_OnRequestReceived
            };
            _pendingConnections.TryAdd(sessionItem, null);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SessionItem_OnSessionRequestReceived(TWTransportServerConnection client, RPCSessionRequestMessage request, RPCSessionResponseMessage response)
        {
            OnClientConnect?.Invoke(this, new ClientConnectEventArgs(response.SessionId));
            if (!response.Succeed) return;
            _sessions.AddOrUpdate(response.SessionId, client, (k, a) => client);
            _pendingConnections.TryRemove(client, out var _);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private RPCResponseMessage Session_OnRequestReceived(TWTransportServerConnection client, RPCRequestMessage request)
        {
            if (request.MethodId == Guid.Empty)
            {
                var dEventArgs = new ServerDescriptorsEventArgs();
                OnGetDescriptorsRequest?.Invoke(this, dEventArgs);
                var response = new RPCResponseMessage(request)
                {
                    ReturnValue = dEventArgs.Descriptors
                };
                return response;
            }
            var mEventArgs = new MethodEventArgs(client.SessionId, request);
            OnMethodCall?.Invoke(this, mEventArgs);
            return mEventArgs.Response;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SessionItem_OnSessionDisconnected(TWTransportServerConnection client)
        {
            client.OnSessionRequestReceived = null;
            client.OnRequestReceived = null;
            client.OnSessionDisconnected = null;
            _sessions.TryRemove(client.SessionId, out var _);
            Core.Status.DeAttachObject(client);
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
            var lstClients = _sessions.Values.Where(c => c.Hub == hub && !c.Disconnected).ToArray();
            Parallel.ForEach(lstClients, client => client.SendPushMessage(msg));
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
            var lstClients = _sessions.Values.Where(c => c.Hub == hub && c.SessionId != exceptSessionId && !c.Disconnected).ToArray();
            Parallel.ForEach(lstClients, client => client.SendPushMessage(msg));
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
            var lstClients = _sessions.Values.Where(c => !c.Disconnected).ToArray();
            Parallel.ForEach(lstClients, client => client.SendPushMessage(msg));
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
            var lstClients = _sessions.Values.Where(c => c.SessionId != exceptSessionId && !c.Disconnected).ToArray();
            Parallel.ForEach(lstClients, client => client.SendPushMessage(msg));
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
            if (_sessions.TryGetValue(sessionId, out var client))
                client.SendPushMessage(msg);
        }
        #endregion
    }
}
