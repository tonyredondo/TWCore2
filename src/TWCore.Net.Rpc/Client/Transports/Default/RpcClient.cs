﻿/*
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
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Serialization;
using TWCore.Threading;
using Thread = System.Threading.Thread;
// ReSharper disable MethodSupportsCancellation
#pragma warning disable 4014

namespace TWCore.Net.RPC.Client.Transports.Default
{
    /// <summary>
    /// Rpc client event
    /// </summary>
    /// <param name="rpcClient">RpcClient instance</param>
    /// <param name="e">Event args</param>
    public delegate void RpcClientEvent(RpcClient rpcClient, EventArgs e);
    /// <summary>
    /// Rpc client event
    /// </summary>
    /// <typeparam name="TEventArgs">Event args object type</typeparam>
    /// <param name="rpcClient">RpcClient instance</param>
    /// <param name="e">Event args</param>
    public delegate void RpcClientEvent<in TEventArgs>(RpcClient rpcClient, TEventArgs e);

    /// <inheritdoc />
    /// <summary>
    /// Rpc client
    /// </summary>
    public class RpcClient : IDisposable
    {
        #region Private fields
        private readonly AsyncLock _connectionLocker = new AsyncLock();
        private readonly AsyncLock _sendLocker = new AsyncLock();
        private readonly AsyncManualResetEvent _sessionEvent = new AsyncManualResetEvent();
        private readonly string _host;
        private readonly int _port;
        private readonly BinarySerializer _serializer;
        private readonly Func<object, Task> _messageReceivedHandlerDelegate;

        private TcpClient _client;
        private Stream _networkStream;
        private BufferedStream _writeStream;
        private BufferedStream _readStream;
        private bool _shouldBeConnected;
        private CancellationTokenSource _connectionCancellationTokenSource;
        private CancellationToken _connectionCancellationToken;
        private Task _receiveTask;
        private bool _onSession;
        private string _hub;
        private Guid _sessionId;

        #endregion

        #region Properties
        /// <summary>
        /// Get if the Rpc client is on Session
        /// </summary>
        public bool OnSession => _onSession;
        /// <summary>
        /// Session hub name
        /// </summary>
        public string Hub
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _hub;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_client.Connected) return;
                _hub = value;
            }
        }
        /// <summary>
        /// Session Id
        /// </summary>
        public Guid SessionId => _sessionId;
        #endregion

        #region Events
        /// <summary>
        /// Event when the RpcClient is connected with a session
        /// </summary>
        public event RpcClientEvent OnConnect;
        /// <summary>
        /// Event when the RpcClient has been disconnected
        /// </summary>
        public event RpcClientEvent OnDisconnect;
        /// <summary>
        /// Event when a message has been received
        /// </summary>
        public event RpcClientEvent<RPCMessage> OnMessageReceived;
        #endregion

        #region .ctor
        /// <summary>
        /// Rpc Client
        /// </summary>
        /// <param name="host">Hostname</param>
        /// <param name="port">Tcp port</param>
        /// <param name="serializer">Binary serializer instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RpcClient(string host, int port, BinarySerializer serializer)
        {
            _serializer = serializer;
            _host = host;
            _port = port;
            _hub = "Default";
            _messageReceivedHandlerDelegate = new Func<object, Task>(MessageReceivedHandler);
        }
        #endregion

        #region Connection Methods
        /// <summary>
        /// Connects to the Rpc Server
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> ConnectAsync()
        {
            if (_client?.Connected == true) return false;
            using (await _connectionLocker.LockAsync().ConfigureAwait(false))
            {
                if (_client?.Connected == true) return false;
                _client?.Close();
                _shouldBeConnected = true;
                _onSession = false;
                _sessionEvent.Reset();
                _client = new TcpClient
                {
                    NoDelay = true
                };
                Factory.SetSocketLoopbackFastPath(_client.Client);
                await _client.ConnectAsync(_host, _port).ConfigureAwait(false);
                _networkStream = _client.GetStream();
                _readStream = new BufferedStream(_networkStream);
                _writeStream = new BufferedStream(_networkStream);
                if (_connectionCancellationTokenSource is null || _connectionCancellationTokenSource.IsCancellationRequested)
                {
                    _connectionCancellationTokenSource = new CancellationTokenSource();
                    _connectionCancellationToken = _connectionCancellationTokenSource.Token;
                }
                Core.Log.InfoBasic("Connected to server: {0}:{1}", _host, _port);
                BindBackgroundTasks();
                _serializer.Serialize(new RPCSessionRequestMessage { Hub = _hub, SessionId = _sessionId }, _writeStream);
                await _writeStream.FlushAsync(_connectionCancellationToken).ConfigureAwait(false);
                await _sessionEvent.WaitAsync(_connectionCancellationToken).ConfigureAwait(false);
                OnConnect?.Invoke(this, EventArgs.Empty);
                Core.Log.InfoBasic("RPC connection started with: {0}:{1}", _host, _port);
                return true;
            }
        }
        /// <summary>
        /// Disconnect from the Rpc Server
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> DisconnectAsync()
        {
            if (_client is null || !_client.Connected) return false;
            using (await _connectionLocker.LockAsync().ConfigureAwait(false))
            {
                if (_client is null || !_client.Connected) return false;
                _shouldBeConnected = false;
                _connectionCancellationTokenSource?.Cancel();
                _onSession = false;
                Core.Log.InfoBasic("RPC connection stopped with: {0}:{1}", _host, _port);

                _client.Close();
                _client = null;
                _readStream?.Dispose();
                _writeStream?.Dispose();
                _networkStream = null;
                _readStream = null;
                _writeStream = null;
                return true;
            }
        }
        #endregion

        #region Send Rpc Message
        /// <summary>
        /// Send a RpcMessage
        /// </summary>
        /// <param name="message">RpcMessage instance</param>
        /// <returns>Completation task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> SendRpcMessageAsync(RPCMessage message)
        {
            if (_connectionCancellationToken.IsCancellationRequested)
                return false;
            using (await _sendLocker.LockAsync().ConfigureAwait(false))
            {
                if (_connectionCancellationToken.IsCancellationRequested)
                    return false;
                if (_client is null || !_client.Connected)
                {
                    OnDisconnect?.Invoke(this, EventArgs.Empty);
                    await ConnectAsync().ConfigureAwait(false);
                }
                if (!_onSession) return false;
                _serializer.Serialize(message, _writeStream);
                await _writeStream.FlushAsync(_connectionCancellationToken).ConfigureAwait(false);
                return true;
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
        private async Task ReceiveThread()
        {
            Thread.CurrentThread.Name = "RPC.DefaultTransportClient.ReceiveThread";
            while (_shouldBeConnected && !_connectionCancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_client is null || !_client.Connected)
                    {
                        OnDisconnect?.Invoke(this, EventArgs.Empty);
                        await ReconnectAsync().ConfigureAwait(false);
                        Core.Log.InfoBasic("Reconnection succeded!");
                    }
                    try
                    {
                        var message = _serializer.Deserialize<RPCMessage>(_readStream);
                        Task.Factory.StartNew(_messageReceivedHandlerDelegate, message, _connectionCancellationToken);
                    }
                    catch (DeserializerException dEx)
                    {
                        var innerEx = dEx.InnerException;
                        if (innerEx is IOException)
                        {
                            OnMessageReceived?.Invoke(this, new RPCError() { Exception = new SerializableException(innerEx) });
                            break;
                        }
                        else
                        {
                            if (!(innerEx is FormatException))
                                Core.Log.Write(innerEx);
                            _client?.Close();
                        }
                    }
                    catch (IOException ex)
                    {
                        OnMessageReceived?.Invoke(this, new RPCError() { Exception = new SerializableException(ex) });
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is FormatException))
                            Core.Log.Write(ex);
                        _client?.Close();
                    }
                }
                catch (SocketException ex)
                {
                    Core.Log.Error(ex.Message);
                    OnMessageReceived?.Invoke(this, new RPCError() { Exception = new SerializableException(ex) });
                    break;
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                    OnMessageReceived?.Invoke(this, new RPCError() { Exception = new SerializableException(ex) });
                    break;
                }
            }
            Dispose();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task ReconnectAsync()
        {
            using (await _connectionLocker.LockAsync().ConfigureAwait(false))
            {
                if (_client?.Connected != true)
                {
                    _onSession = false;
                    _sessionEvent.Reset();
                    _client = new TcpClient
                    {
                        NoDelay = true
                    };
                    Factory.SetSocketLoopbackFastPath(_client.Client);
                    await _client.ConnectAsync(_host, _port).ConfigureAwait(false);
                    _networkStream = _client.GetStream();
                    _readStream = new BufferedStream(_networkStream);
                    _writeStream = new BufferedStream(_networkStream);
                    if (_connectionCancellationTokenSource is null || _connectionCancellationTokenSource.IsCancellationRequested)
                    {
                        _connectionCancellationTokenSource = new CancellationTokenSource();
                        _connectionCancellationToken = _connectionCancellationTokenSource.Token;
                    }
                    Core.Log.InfoBasic("Reconnected to server: {0}:{1}", _host, _port);
                    _serializer.Serialize(new RPCSessionRequestMessage { Hub = _hub, SessionId = _sessionId }, _writeStream);
                    await _writeStream.FlushAsync(_connectionCancellationToken).ConfigureAwait(false);
                    _ = CompleteReconnectAsync();
                }
            }

            async Task CompleteReconnectAsync()
            {
                await _sessionEvent.WaitAsync(_connectionCancellationToken).ConfigureAwait(false);
                OnConnect?.Invoke(this, EventArgs.Empty);
                Core.Log.InfoBasic("RPC reconnection started with: {0}:{1}", _host, _port);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task MessageReceivedHandler(object rawMessage)
        {
            try
            {
                switch (rawMessage)
                {
                    case RPCSessionResponseMessage sessionMessage:
                        if (sessionMessage.Succeed)
                        {
                            _sessionId = sessionMessage.SessionId;
                            _onSession = true;
                            _sessionEvent.Set();
                        }
                        else
                            await DisconnectAsync().ConfigureAwait(false);
                        break;
                    case RPCMessage message:
                        OnMessageReceived?.Invoke(this, message);
                        break;
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
            }
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Disposes the RpcClient instance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _shouldBeConnected = false;
            _client?.Dispose();
            _readStream?.Dispose();
            _writeStream?.Dispose();
            _networkStream?.Dispose();
            _connectionCancellationTokenSource?.Dispose();
            _sessionEvent.Reset();
            _client = null;
            _readStream = null;
            _writeStream = null;
            _networkStream = null;
            _onSession = false;
        }
    }
}
