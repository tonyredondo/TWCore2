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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Serialization;
using TWCore.Threading;
using Thread = System.Threading.Thread;
// ReSharper disable MethodSupportsCancellation

namespace TWCore.Net.RPC.Server.Transports.Default
{
    /// <summary>
    /// Rpc server client event
    /// </summary>
    /// <param name="rpcServerClient">RpcServerClient instance</param>
    /// <param name="e">Event args</param>
    public delegate void RpcServerClientEvent(RpcServerClient rpcServerClient, EventArgs e);
    /// <summary>
    /// Rpc server client session event
    /// </summary>
    /// <param name="rpcServerClient">RpcServerClient instance</param>
    /// <param name="sessionMessage">Session request message</param>
    /// <returns>True if the session is accepted; otherwise, false.</returns>
    public delegate ValueTask<bool> RpcServerClientSessionEvent(RpcServerClient rpcServerClient, RPCSessionRequestMessage sessionMessage);
    /// <summary>
    /// Rpc server client session event
    /// </summary>
    /// <typeparam name="TEventArgs">Event args object type</typeparam>
    /// <param name="rpcServerClient">RpcServerClient instance</param>
    /// <param name="e">Event args</param>
    public delegate Task RpcServerClientEvent<in TEventArgs>(RpcServerClient rpcServerClient, TEventArgs e);

    /// <inheritdoc />
    /// <summary>
    /// Rpc server client
    /// </summary>
    public class RpcServerClient : IDisposable
    {
        #region Private fields
        private readonly AsyncLock _sendLocker = new AsyncLock();
        private readonly BinarySerializer _serializer;
        private readonly Func<object, Task> _messageReceivedHandlerDelegate;

        private TcpClient _client;
        private Stream _networkStream;
        private BufferedStream _readStream;
        private BufferedStream _writeStream;
        private Task _receiveTask;
        private bool _onSession;
        private string _hub;
        private Guid _sessionId;
        private CancellationTokenSource _tokenSource;
        private string _clientIp;
        #endregion

        #region Properties
        /// <summary>
        /// Get if the RPCClient is on session
        /// </summary>
        public bool OnSession => _onSession;
        /// <summary>
        /// Session hub name
        /// </summary>
        public string Hub => _hub;
        /// <summary>
        /// Session Id
        /// </summary>
        public Guid SessionId => _sessionId;
        /// <summary>
        /// Connection cancellation token
        /// </summary>
        public CancellationToken ConnectionCancellationToken => _tokenSource.Token;
        #endregion

        #region Events
        /// <summary>
        /// Event when the RpcServerClient has been disconnected
        /// </summary>
        public event RpcServerClientEvent OnDisconnect;
        /// <summary>
        /// Event when the RpcServerClient has been connected
        /// </summary>
        public event RpcServerClientEvent OnConnect;
        /// <summary>
        /// Event when a message has been received
        /// </summary>
        public event RpcServerClientEvent<RPCMessage> OnMessageReceivedAsync;
        /// <summary>
        /// Event when the session request message has been received
        /// </summary>
        public event RpcServerClientSessionEvent OnSessionMessageReceivedAsync;
        #endregion

        #region .ctor
        /// <summary>
        /// Rpc server client
        /// </summary>
        /// <param name="client">TcpClient instance</param>
        /// <param name="serializer">BinarySerializer instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RpcServerClient(TcpClient client, BinarySerializer serializer)
        {
            _messageReceivedHandlerDelegate = MessageReceivedHandler;
            _client = client;
            _serializer = serializer;
            _networkStream = _client.GetStream();
            _readStream = new BufferedStream(_networkStream);
            _writeStream = new BufferedStream(_networkStream);
            _tokenSource = new CancellationTokenSource();
            BindBackgroundTasks();
            try
            {
                if (client.Client.RemoteEndPoint is IPEndPoint remoteIp)
                    _clientIp = remoteIp.Address.ToString();
            }
            catch
            {
                //
            }
            Core.Log.InfoBasic("RPC connection started with: {0}", _clientIp);
        }
        #endregion

        #region Send Rpc Message
        /// <summary>
        /// Send a RpcMessage
        /// </summary>
        /// <param name="message">RpcMessage instance</param>
        /// <returns>Completation task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task SendRpcMessageAsync(RPCMessage message)
        {
            using (await _sendLocker.LockAsync().ConfigureAwait(false))
            {
                if (_client is null || !_client.Connected || _isDisposing) return;
                _serializer.Serialize(message, _writeStream);
                try
                {
                    await _writeStream.FlushAsync().ConfigureAwait(false);
                }
                catch
                {
                    //
                }
            }
        }
        #endregion

        #region Receive Background Thread
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BindBackgroundTasks()
        {
            if (_receiveTask is null || _receiveTask.IsCompleted)
                _receiveTask = Task.Factory.StartNew(ReceiveThread, TaskCreationOptions.LongRunning);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReceiveThread()
        {
            Thread.CurrentThread.Name = "RPC.DefaultTransportServer.ReceiveThread";
            while (_client != null && _client.Connected)
            {
                try
                {
                    var message = _serializer.Deserialize<RPCMessage>(_readStream);
                    Task.Factory.StartNew(_messageReceivedHandlerDelegate, message, _tokenSource.Token);
                }
                catch (DeserializerException dEx)
                {
                    var innerEx = dEx.InnerException;
                    if (innerEx is IOException || innerEx is FormatException)
                        break;
                    if (_tokenSource?.IsCancellationRequested != true && _client.Connected && !_isDisposing)
                        Core.Log.Write(innerEx);
                    break;
                }
                catch (IOException)
                {
                    break;
                }
                catch (FormatException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (_tokenSource?.IsCancellationRequested != true && _client.Connected && !_isDisposing)
                        Core.Log.Write(ex);
                    break;
                }
            }
            Dispose();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task MessageReceivedHandler(object rawMessage)
        {
            try
            {
                switch (rawMessage)
                {
                    case RPCSessionRequestMessage sessionMessage:
                        var sessionOk = true;
                        if (!(OnSessionMessageReceivedAsync is null))
                            sessionOk = await OnSessionMessageReceivedAsync(this, sessionMessage).ConfigureAwait(false);
                        if (sessionOk)
                        {
                            _hub = sessionMessage.Hub;
                            _sessionId = sessionMessage.SessionId == Guid.Empty
                                ? Guid.NewGuid()
                                : sessionMessage.SessionId;
                            _onSession = true;
                            var msg = RPCSessionResponseMessage.Retrieve(sessionMessage.MessageId, true, _sessionId);
                            await SendRpcMessageAsync(msg).ConfigureAwait(false);
                            OnConnect?.Invoke(this, EventArgs.Empty);
                            RPCSessionResponseMessage.Store(msg);
                        }
                        else
                            Dispose();

                        break;
                    case RPCMessage message:
                        if (!_onSession) return;
                        if (!(OnMessageReceivedAsync is null))
                            await OnMessageReceivedAsync(this, message).ConfigureAwait(false);
                        break;
                }
            }
            catch(Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        #endregion

        private bool _isDisposing;
        /// <inheritdoc />
        /// <summary>
        /// Disposes the RpcClient instance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            try
            {
                Core.Log.InfoBasic("RPC connection stopped with: {0}", _clientIp);
                _isDisposing = true;
                _client?.Dispose();
                _readStream?.Dispose();
                _writeStream?.Dispose();
                _networkStream?.Dispose();
                _tokenSource.Cancel();

                _client = null;
                _readStream = null;
                _writeStream = null;
                _networkStream = null;
                _tokenSource = null;
            }
            catch
            {
                //
            }
            OnDisconnect?.Invoke(this, EventArgs.Empty);
        }
    }
}
