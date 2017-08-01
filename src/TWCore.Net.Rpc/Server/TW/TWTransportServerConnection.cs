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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.IO;
using TWCore.Serialization;

namespace TWCore.Net.RPC.Server.Transports
{
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
        TcpClient Client;
        TWTransportServer Server;
        readonly Task ReceiveTask;
        readonly long socketErrorsToDisconnection = 2;
        long socketErrors;
        bool disconnectionEventSent = false;
        CancellationTokenSource tokenSource;
        CancellationToken token;
        Stream ReadStream;
        Stream WriteStream;
        BinaryReader Reader;
        ISerializer Serializer;
        BytesCounterStream readCounterStream;
        BytesCounterStream writeCounterStream;

        object readLock = new object();
        object writeLock = new object();
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
        public bool Disconnected => socketErrors >= socketErrorsToDisconnection;
        /// <summary>
        /// Gets true if the SessionRequest was already received and accepted.
        /// </summary>
        [StatusProperty]
        public bool IsOnSession { get; private set; } = false;
        /// <summary>
        /// Gets the datetime when the user was disconnected
        /// </summary>
        [StatusProperty]
        public DateTime? DisconnectionDateTime { get; private set; }
        /// <summary>
        /// Receive  Size
        /// </summary>
        [StatusProperty]
        public int ReceiveSize { get; set; } = 65536;
        /// <summary>
        /// Send  Size
        /// </summary>
        [StatusProperty]
        public int SendSize { get; set; } = 65536;
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
        /// <param name="socket">Client socket</param>
        /// <param name="serializer">Stream serializer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TWTransportServerConnection(TWTransportServer server, TcpClient client, ISerializer serializer)
        {
            Server = server;
            Client = client;
            socketErrors = 0;
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            client.ReceiveBufferSize = ReceiveSize;
            client.SendBufferSize = SendSize;
            var netStream = client.GetStream();
            Serializer = serializer;
            readCounterStream = new BytesCounterStream(netStream);
            readCounterStream.OnBytesRead += (s, e) => Server.Counters.IncrementBytesReceived(e);
            writeCounterStream = new BytesCounterStream(netStream);
            writeCounterStream.OnBytesWrite += (s, e) => Server.Counters.IncrementBytesSent(e);
            ReadStream = new BufferedStream(readCounterStream, ReceiveSize);
            WriteStream = new BufferedStream(writeCounterStream, SendSize);
            Reader = new BinaryReader(ReadStream, Encoding.UTF8, true);

            ReceiveTask = Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested && !Disconnected)
                {
                    var tuple = GetRPCMessage(Reader);
                    var msgType = tuple.Item1;
                    var message = tuple.Item2;
#pragma warning disable 4014
                    switch (msgType)
                    {
                        case RPCMessageType.Unknown:
                            Interlocked.Increment(ref socketErrors);
                            break;
                        case RPCMessageType.RequestMessage:
                            Task.Factory.StartNew(ProcessRequestMessage, message, token);
                            break;
                        case RPCMessageType.SessionRequest:
                            ProcessSessionRequestMessage((RPCSessionRequestMessage)message);
                            break;
                    }
#pragma warning restore 4014
                }
                if (Disconnected)
                {
                    Core.Log.Warning("Session {0} had disconnected.", SessionId);
                    DisconnectionDateTime = DateTime.UtcNow;
                    try
                    {
                        Client.Dispose();
                    }
                    catch { }
                    if (!disconnectionEventSent)
                    {
                        OnSessionDisconnected?.Invoke(this);
                        disconnectionEventSent = true;
                    }
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ProcessSessionRequestMessage(RPCSessionRequestMessage sessionRequest)
        {
            if (sessionRequest != null)
            {
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
                        Client.Dispose();
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ProcessRequestMessage(object message)
        {
            using (var watch = Watch.Create())
            {
                var messageRequest = message as RPCRequestMessage;
                if (messageRequest == null) return;
                if (IsOnSession)
                {
                    var messageResponse = OnRequestReceived?.Invoke(this, messageRequest);
                    WriteRPCMessageData(messageResponse, RPCMessageType.ResponseMessage);
                    watch.Tap($"Response message for Id: {messageResponse.RequestMessageId} sent.");
                }
                else
                    Core.Log.Warning("RPC message received without a session, the message is not going to be processed.");
            }
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Tuple<RPCMessageType, RPCMessage> GetRPCMessage(BinaryReader reader)
        {
            lock (readLock)
            {
                RPCMessage message = null;
                RPCMessageType mTypeEnum = RPCMessageType.Unknown;
                //Load Type
                byte mTypeByte = 255;
                try
                {
                    mTypeByte = reader.ReadByte();
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref socketErrors);
                    Core.Log.Warning("Error reading bytes ({0})", ex.Message);
                    return Tuple.Create(RPCMessageType.Unknown, message);
                }
                mTypeEnum = (RPCMessageType)mTypeByte;

                #region Check if is a valid type
                if (mTypeEnum != RPCMessageType.Ping &&
                    mTypeEnum != RPCMessageType.SessionRequest &&
                    mTypeEnum != RPCMessageType.RequestMessage)
                {
                    Core.Log.Warning("Invalid Message Type ({0})", mTypeEnum);
                    return Tuple.Create(RPCMessageType.Unknown, message);
                }
                #endregion

                if (mTypeEnum == RPCMessageType.Ping)
                {
                    Core.Log.LibVerbose("Ping message received, sending pong.");
                    if (!Disconnected)
                    {
                        lock (WriteStream)
                        {
                            WriteStream.Write(new byte[] { (byte)RPCMessageType.Pong }, 0, 1);
                            WriteStream.Flush();
                        }
                    }
                }
                else
                {
                    Type type = typeof(RPCMessage);
                    switch (mTypeEnum)
                    {
                        case RPCMessageType.RequestMessage:
                            type = typeof(RPCRequestMessage);
                            break;
                        case RPCMessageType.SessionRequest:
                            type = typeof(RPCSessionRequestMessage);
                            break;
                    }
                    try
                    {
                        message = (RPCMessage)Serializer.Deserialize(reader.BaseStream, type);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                        Interlocked.Increment(ref socketErrors);
                        if (Disconnected && !disconnectionEventSent)
                        {
                            OnSessionDisconnected?.Invoke(this);
                            disconnectionEventSent = true;
                        }
                    }
                }
                return Tuple.Create(mTypeEnum, message);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WriteRPCMessageData(RPCMessage message, RPCMessageType messageType = RPCMessageType.Unknown)
        {
            lock (writeLock)
            {
                RPCMessageType mTypeEnum = messageType;
                Type mType = message?.GetType() ?? typeof(object);
                if (mTypeEnum == RPCMessageType.Unknown)
                {
                    if (message as RPCSessionResponseMessage != null) mTypeEnum = RPCMessageType.SessionResponse;
                    else if (message as RPCResponseMessage != null) mTypeEnum = RPCMessageType.ResponseMessage;
                    else if (message as RPCEventMessage != null) mTypeEnum = RPCMessageType.EventMessage;
                    else if (message as RPCPushMessage != null) mTypeEnum = RPCMessageType.PushMessage;
                }
                if (Disconnected)
                    Factory.Thread.SleepUntil(() => !Disconnected);
                try
                {
                    WriteStream.WriteByte((byte)mTypeEnum);
                    try
                    {
                        Serializer.Serialize(message, mType, WriteStream);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                    WriteStream.Flush();
                    Interlocked.Exchange(ref socketErrors, 0);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref socketErrors);
                    Core.Log.Write(ex);
                    if (Disconnected && !disconnectionEventSent)
                    {
                        OnSessionDisconnected?.Invoke(this);
                        disconnectionEventSent = true;
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
        /// <summary>
        /// Dispose all the object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            tokenSource.Cancel();
            try
            {
                ReceiveTask.Wait(1000);
            }
            catch { }
            Server = null;
            try
            {
                Client?.Dispose();
            }
            catch { }
            Client = null;
        }
        #endregion       
    }
}
