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
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.IO;
using TWCore.Serialization;
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace TWCore.Net.RPC.Server.Transports
{
    /// <inheritdoc />
    /// <summary>
    /// Defines a TW connection to a client
    /// </summary>
    public class TWTransportServerConnection : IDisposable
    {
        #region Delegates Definitions
        /// <summary>
        /// New session request message received delegate
        /// </summary>
        /// <param name="client">TW client where the message was received</param>
        /// <param name="request">Session request message received</param>
        /// <param name="response">Session response message</param>
        internal delegate void SessionMessageReceived(TWTransportServerConnection client, RPCSessionRequestMessage request, RPCSessionResponseMessage response);
        /// <summary>
        /// Message request received delegate
        /// </summary>
        /// <param name="client">TW client where the message was received</param>
        /// <param name="request">Request message received</param>
        /// <returns>Response message to send</returns>
        internal delegate RPCResponseMessage MessageReceived(TWTransportServerConnection client, RPCRequestMessage request);
        /// <summary>
        /// Session disconnection delegate
        /// </summary>
        /// <param name="client">TW client where the connection was closed</param>
        internal delegate void SessionDisconnection(TWTransportServerConnection client);
        #endregion

        #region Fields
        private const long _socketErrorsToDisconnection = 2;
        private readonly Task _receiveTask;
        private readonly CancellationTokenSource _tokenSource;
        private readonly BufferedStream _readStream;
        private readonly BufferedStream _writeStream;
        private readonly ISerializer _serializer;
        private readonly object _readLock = new object();
        private readonly object _writeLock = new object();

        private TcpClient _client;
        private TWTransportServer _server;
        private long _socketErrors;
        private bool _disconnectionEventSent;
        #endregion

        #region Properties
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
        /// Gets true if the connection is closed.
        /// </summary>
        [StatusProperty]
        public bool Disconnected => _socketErrors >= _socketErrorsToDisconnection;
        /// <summary>
        /// Gets true if the SessionRequest was already received and accepted.
        /// </summary>
        [StatusProperty]
        public bool IsOnSession { get; private set; }
        /// <summary>
        /// Gets the datetime when the user was disconnected
        /// </summary>
        [StatusProperty]
        public DateTime? DisconnectionDateTime { get; private set; }
        /// <summary>
        /// Receive  Size
        /// </summary>
        [StatusProperty]
		public int ReceiveSize { get; set; } = 32768;
        /// <summary>
        /// Send  Size
        /// </summary>
        [StatusProperty]
		public int SendSize { get; set; } = 32768;
        #endregion

        #region Delegates
        /// <summary>
        /// Delegate when a session request is received.
        /// </summary>
        internal SessionMessageReceived OnSessionRequestReceived;
        /// <summary>
        /// Delegate when a message request is received.
        /// </summary>
        internal MessageReceived OnRequestReceived;
        /// <summary>
        /// Delegate when the client connection closes.
        /// </summary>
        internal SessionDisconnection OnSessionDisconnected;
        #endregion

        #region .ctor
        /// <summary>
        /// Defines a TW progressive connection to a client
        /// </summary>
        /// <param name="server">TwoWay transport server</param>
		/// <param name="client">Client socket</param>
        /// <param name="serializer">Stream serializer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TWTransportServerConnection(TWTransportServer server, TcpClient client, ISerializer serializer)
        {
            _server = server;
            _client = client;
            _serializer = serializer;
            _socketErrors = 0;
            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            client.ReceiveBufferSize = ReceiveSize;
            client.SendBufferSize = SendSize;
            var netStream = client.GetStream();
            var readCounterStream = new BytesCounterStream(netStream);
            var writeCounterStream = new BytesCounterStream(netStream);
            _readStream = new BufferedStream(readCounterStream, ReceiveSize);
            _writeStream = new BufferedStream(writeCounterStream, SendSize);

            _receiveTask = Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested && !Disconnected)
                {
                    var (msgType, message) = GetRPCMessage();
                    switch (msgType)
                    {
                        case RPCMessageType.Unknown:
                            Interlocked.Increment(ref _socketErrors);
                            break;
                        case RPCMessageType.RequestMessage:
                            ThreadPool.QueueUserWorkItem(ProcessRequestMessage, message);
                            break;
                        case RPCMessageType.SessionRequest:
                            ThreadPool.QueueUserWorkItem(ProcessSessionRequestMessage, message);
                            break;
                    }
                    _server.Counters.IncrementBytesReceived(readCounterStream.BytesRead);
                    readCounterStream.ClearBytesCounters();
                    _server.Counters.IncrementBytesSent(writeCounterStream.BytesWrite);
                    writeCounterStream.ClearBytesCounters();
                }
                if (!Disconnected) return;
                Core.Log.Warning("Session {0} had disconnected.", SessionId);
                DisconnectionDateTime = DateTime.UtcNow;
                try
                {
                    _client.Dispose();
                }
                catch
                {
                    // ignored
                }
                if (_disconnectionEventSent) return;
                OnSessionDisconnected?.Invoke(this);
                _disconnectionEventSent = true;
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessSessionRequestMessage(object message)
        {
            if (!(message is RPCSessionRequestMessage sessionRequest)) return;
            using (var watch = Watch.Create())
            {
                var sessionId = sessionRequest.SessionId;
                if (sessionId == Guid.Empty)
                    sessionId = Guid.NewGuid();
                var sessionResponse = new RPCSessionResponseMessage { SessionId = sessionId, RequestMessageId = sessionRequest.MessageId, Succeed = true };
                OnSessionRequestReceived?.Invoke(this, sessionRequest, sessionResponse);
                if (sessionResponse.Succeed)
                {
                    SessionId = sessionResponse.SessionId;
                    Hub = sessionRequest.Hub;
                    IsOnSession = true;
                    WriteRPCMessageData(sessionResponse, RPCMessageType.SessionResponse);
                    watch.Tap($"Session created with Id: {SessionId}, Hub: {Hub}");
                }
                else
                {
                    WriteRPCMessageData(sessionResponse, RPCMessageType.SessionResponse);
                    watch.Tap("Session wasn't created.");
                    _client.Dispose();
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessRequestMessage(object message)
        {
            if (!(message is RPCRequestMessage messageRequest)) return;
            using (Watch.Create("Processing Request Message"))
            {
                if (!IsOnSession)
                {
                    Core.Log.Warning("RPC message received without a session, the message is not going to be processed.");
                    return;
                }
                var messageResponse = OnRequestReceived.Invoke(this, messageRequest);
                WriteRPCMessageData(messageResponse, RPCMessageType.ResponseMessage);
            }
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (RPCMessageType, RPCMessage) GetRPCMessage()
        {
            lock (_readLock)
            {
                #region LoadType
                int mTypeByte;
                try
                {
                    mTypeByte = _readStream.ReadByte();
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _socketErrors);
                    Core.Log.Warning("Error reading bytes ({0})", ex.Message);
                    return (RPCMessageType.Unknown, null);
                }
                var mTypeEnum = (RPCMessageType)mTypeByte;
                #endregion

                #region Check if is a valid type
                if (mTypeByte < 1 || mTypeByte > 3)
                {
                    Core.Log.Warning("Invalid Message Type ({0})", mTypeEnum);
                    return (RPCMessageType.Unknown, null);
                }
                #endregion

                #region Check if is a Ping
                if (mTypeEnum == RPCMessageType.Ping)
                {
                    Core.Log.LibVerbose("Ping message received, sending pong.");
                    if (!Disconnected)
                    {
                        lock (_writeStream)
                        {
                            _writeStream.WriteByte((byte)RPCMessageType.Pong);
                            _writeStream.Flush();
                            return (RPCMessageType.Ping, null);
                        }
                    }
                }
                #endregion  

                RPCMessage message = null;
                try
                {
					switch(mTypeEnum)
					{
						case RPCMessageType.SessionRequest:
							message = (RPCMessage)_serializer.Deserialize(_readStream, typeof(RPCSessionRequestMessage));
							break;
						case RPCMessageType.RequestMessage:
							message = (RPCMessage)_serializer.Deserialize(_readStream, typeof(RPCRequestMessage));
							break;
					}
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                    Interlocked.Increment(ref _socketErrors);
                    if (!Disconnected || _disconnectionEventSent) 
                        return (mTypeEnum, message);
                    OnSessionDisconnected?.Invoke(this);
                    _disconnectionEventSent = true;
                }
                return (mTypeEnum, message);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteRPCMessageData(RPCMessage message, RPCMessageType messageType)
        {
            lock (_writeLock)
            {
                if (Disconnected)
                    Factory.Thread.SleepUntil(() => !Disconnected);

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
                    Interlocked.Exchange(ref _socketErrors, 0);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _socketErrors);
                    Core.Log.Write(ex);
                    if (Disconnected && !_disconnectionEventSent)
                    {
                        OnSessionDisconnected?.Invoke(this);
                        _disconnectionEventSent = true;
                    }
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Send push message
        /// </summary>
        /// <param name="message">Message to push on the client</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendPushMessage(RPCPushMessage message) 
            => WriteRPCMessageData(message, RPCMessageType.PushMessage);
        /// <summary>
        /// Send a event message
        /// </summary>
        /// <param name="message">Event trigger message to send to the client</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendEventMessage(RPCEventMessage message)
            => WriteRPCMessageData(message, RPCMessageType.EventMessage);
        /// <inheritdoc />
        /// <summary>
        /// Dispose all the object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _tokenSource.Cancel();
            try
            {
                _receiveTask.Wait(1000);
            }
            catch
            {
                // ignored
            }
            _server = null;
            try
            {
                _client?.Dispose();
            }
            catch
            {
                // ignored
            }
            _client = null;
        }
        #endregion       
    }
}
