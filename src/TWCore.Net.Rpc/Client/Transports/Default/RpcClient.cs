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
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using TWCore.Serialization;

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
        private TcpClient _client;
        private Stream _networkStream;
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
            get => _hub;
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
        public RpcClient(string host, int port, BinarySerializer serializer)
        {
            _serializer = serializer;
            _host = host;
            _port = port;
            _hub = "Default";
        }
        #endregion

        #region Connection Methods
        /// <summary>
        /// Connects to the Rpc Server
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            if (_client?.Connected == true) return;
            using (await _connectionLocker.LockAsync().ConfigureAwait(false))
            {
                if (_client?.Connected == true) return;
                _client?.Close();
                _shouldBeConnected = true;
                _onSession = false;
                _client = new TcpClient
                {
                    NoDelay = true,
                    ReceiveBufferSize = 32768,
                    SendBufferSize = 32768
                };
                Factory.SetSocketLoopbackFastPath(_client.Client);
                await _client.ConnectAsync(_host, _port).ConfigureAwait(false);
                _networkStream = _client.GetStream();
                if (_connectionCancellationTokenSource == null || _connectionCancellationTokenSource.IsCancellationRequested)
                {
                    _connectionCancellationTokenSource = new CancellationTokenSource();
                    _connectionCancellationToken = _connectionCancellationTokenSource.Token;
                    BindBackgroundTasks();
                }
                _serializer.Serialize(new RPCSessionRequestMessage { Hub = _hub, SessionId = _sessionId }, _networkStream);
                await _sessionEvent.WaitAsync(_connectionCancellationToken).ConfigureAwait(false);
                OnConnect?.Invoke(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// Disconnect from the Rpc Server
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            if (_client == null || !_client.Connected) return;
            using (await _connectionLocker.LockAsync().ConfigureAwait(false))
            {
                if (_client == null || !_client.Connected) return;
                _shouldBeConnected = false;
                _connectionCancellationTokenSource?.Cancel();
                _onSession = false;
                _client.Close();
                _client = null;
                _networkStream = null;
            }
        }
        #endregion

        #region Send Rpc Message
        /// <summary>
        /// Send a RpcMessage
        /// </summary>
        /// <param name="message">RpcMessage instance</param>
        /// <returns>Completation task</returns>
        public async Task SendRpcMessageAsync(RPCMessage message)
        {
            using (await _sendLocker.LockAsync().ConfigureAwait(false))
            {
                if (_connectionCancellationToken.IsCancellationRequested)
                    return;
                if (!_shouldBeConnected)
                    throw new Exception("The client is not connected.");
                if (_client == null || !_client.Connected)
                {
                    OnDisconnect?.Invoke(this, EventArgs.Empty);
                    await ConnectAsync().ConfigureAwait(false);
                }
                if (!_onSession) return;
                _serializer.Serialize(message, _networkStream);
            }
        }
        #endregion

        #region Receive Background Thread
        private void BindBackgroundTasks()
        {
            if (_receiveTask == null || _receiveTask.IsCompleted)
                _receiveTask = Task.Factory.StartNew(ReceiveThread, TaskCreationOptions.LongRunning);
        }
        private void ReceiveThread()
        {
            Thread.CurrentThread.Name = "RPC.DefaultTransportClient.ReceiveThread";
            while (_shouldBeConnected && !_connectionCancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_client == null || !_client.Connected)
                    {
                        OnDisconnect?.Invoke(this, EventArgs.Empty);
                        ConnectAsync().WaitAsync();
                    }
                    var message = _serializer.Deserialize<RPCMessage>(_networkStream);
                    ThreadPool.QueueUserWorkItem(MessageReceivedHandler, message);
                }
                catch (IOException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    TWCore.Core.Log.Write(ex);
                }
            }
            Dispose();
        }
        private void MessageReceivedHandler(object rawMessage)
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
                        DisconnectAsync().WaitAsync();
                    break;
                case RPCMessage message:
                    OnMessageReceived?.Invoke(this, message);
                    break;
            }
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Disposes the RpcClient instance
        /// </summary>
        public void Dispose()
        {
            _shouldBeConnected = false;
            _client?.Dispose();
            _networkStream?.Dispose();
            _connectionCancellationTokenSource?.Dispose();

            _client = null;
            _networkStream = null;
            _onSession = false;
        }
    }
}
