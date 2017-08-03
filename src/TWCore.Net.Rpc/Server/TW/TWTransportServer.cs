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

namespace TWCore.Net.RPC.Server.Transports
{
    /// <summary>
    /// TW RPC Transport server
    /// </summary>
    public class TWTransportServer : ITransportServer
    {
        const string DescriptorMethodId = "{DESCRIPTOR}";
        readonly ConcurrentDictionary<Guid, TWTransportServerConnection> sessions = new ConcurrentDictionary<Guid, TWTransportServerConnection>();
        TcpListener _listener;
        CancellationTokenSource tokenSource;
        CancellationToken token;
        Task tskListener;
        Timer disconnectionTimer;

        #region Properties
        /// <summary>
        /// true if the transport server can send the services descriptors; otherwise, false
        /// </summary>
        [StatusProperty]
        public bool EnableGetDescriptors { get { return false; } set { } }
        /// <summary>
        /// Serializer to encode and decode the incoming and outgoing data
        /// </summary>
        [StatusProperty, StatusReference]
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
                collection.Add("Sessions Count", sessions.Count);
                Core.Status.AttachChild(_listener, this);
                foreach (var ses in sessions)
                    Core.Status.AttachChild(ses.Value, this);
            });
        }
        /// <summary>
        /// TW RPC Transport server
        /// </summary>
        /// <param name="port">Server port</param>
        /// <param name="serializer">Serializer for data transfer, is is null then is XmlTextSerializer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TWTransportServer(int port, ISerializer serializer = null) : this()
        {
            Port = port;
            Serializer = serializer ?? Serializer;
        }
        #endregion

        #region Public Methods
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
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Server.NoDelay = true;
            Factory.SetSocketLoopbackFastPath(_listener.Server);
            _listener.Start();
            tskListener = Task.Factory.StartNew(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    ThreadPool.QueueUserWorkItem(ConnectionReceived, tcpClient);
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            disconnectionTimer = new Timer(state =>
            {
                var sessionsTimeout = sessions.Values.ToList().Where(v => v.DisconnectionDateTime.HasValue && (DateTime.UtcNow - v.DisconnectionDateTime.Value).TotalMinutes > DisconnectedSessionTimeoutInMinutes);
                foreach (var sTimeout in sessionsTimeout)
                {
                    Core.Log.LibVerbose("Removing SessionId={0}", sTimeout.SessionId);
                    sessions.TryRemove(sTimeout.SessionId, out var twc);
                }
            }, this, 10000, 10000);
            Core.Log.LibVerbose("Transport Listener Started");
            return Task.CompletedTask;
        }
        /// <summary>
        /// Stops the server listener
        /// </summary>
        /// <returns>Task as result of the stop process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task StopListenerAsync()
        {
            Core.Log.LibVerbose("Stopping Transport Listener");
            disconnectionTimer?.Dispose();
            disconnectionTimer = null;
            try
            {
                tokenSource.Cancel();
                await tskListener.ConfigureAwait(false);
                _listener.Stop();
                _listener = null;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
            sessions.Values.ToList().Each(session => Try.Do(() => session.Dispose()));
            sessions.Clear();
            Core.Log.LibVerbose("Transport Listener Stopped");
        }
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
                    if (sessions.TryGetValue(clientId, out var client))
                    {
                        client.SendEventMessage(eventMessage);
                        Core.Log.LibVerbose($"Sending event trigger to SessionId='{clientId}' on event '{eventName}'");
                    }
                    break;
                case RPCMessageScope.Hub:
                    var hubName = eventAttribute.HubName;
                    try
                    {
                        var hSessions = sessions.Values.Where(v => v.Hub == hubName && !v.Disconnected).ToList();
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
                        var s = sessions.Values.Where(v => !v.Disconnected).ToList();
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
        void ConnectionReceived(object objTcpClient)
        {
            var client = (TcpClient)objTcpClient;
            var sessionItem = new TWTransportServerConnection(this, client, Serializer)
            {
                OnSessionRequestReceived = SessionItem_OnSessionRequestReceived,
                OnSessionDisconnected = SessionItem_OnSessionDisconnected,
                OnRequestReceived = Session_OnRequestReceived
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SessionItem_OnSessionRequestReceived(TWTransportServerConnection client, RPCSessionRequestMessage request, RPCSessionResponseMessage response)
        {
            OnClientConnect?.Invoke(this, new ClientConnectEventArgs(response.SessionId));
            if (response.Succeed)
                sessions.AddOrUpdate(response.SessionId, client, (k, a) => client);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        RPCResponseMessage Session_OnRequestReceived(TWTransportServerConnection client, RPCRequestMessage request)
        {
            if (request.MethodId == DescriptorMethodId)
            {
                var dEventArgs = new ServerDescriptorsEventArgs();
                OnGetDescriptorsRequest?.Invoke(this, dEventArgs);
                var response = new RPCResponseMessage(request)
                {
                    ReturnValue = dEventArgs.Descriptors,
                    Succeed = true
                };
                return response;
            }
            var mEventArgs = new MethodEventArgs(client.SessionId, request);
            OnMethodCall?.Invoke(this, mEventArgs);
            return mEventArgs.Response;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SessionItem_OnSessionDisconnected(TWTransportServerConnection client)
        {
            client.OnSessionRequestReceived = null;
            client.OnRequestReceived = null;
            client.OnSessionDisconnected = null;
            sessions.TryRemove(client.SessionId, out client);
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
            var lstClients = sessions.Values.Where(c => c.Hub == hub && !c.Disconnected).ToArray();
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
            var lstClients = sessions.Values.Where(c => c.Hub == hub && c.SessionId != exceptSessionId && !c.Disconnected).ToArray();
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
            var lstClients = sessions.Values.Where(c => !c.Disconnected).ToArray();
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
            var lstClients = sessions.Values.Where(c => c.SessionId != exceptSessionId && !c.Disconnected).ToArray();
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
            if (sessions.TryGetValue(sessionId, out var client))
                client.SendPushMessage(msg);
        }
        #endregion
    }
}
