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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using TWCore.Diagnostics.Status;
using TWCore.Net.RPC.Descriptors;
using TWCore.Serialization;

namespace TWCore.Net.RPC.Client.Transports.Default
{
    /// <inheritdoc />
    /// <summary>
    /// Default RPC Transport client
    /// </summary>
    public class DefaultTransportClient : ITransportClient
    {
        private readonly ConcurrentDictionary<Guid, RPCMessageHandler> _messageResponsesHandlers = new ConcurrentDictionary<Guid, RPCMessageHandler>();

        private readonly int _maxIndex = 3;
        private int _currentIndex = -1;
        private RpcClient[] _clients;

        #region Nested Types
        private sealed class RPCMessageHandler
        {
            public readonly AsyncManualResetEvent Event = new AsyncManualResetEvent();
            public RPCResponseMessage Message;
        }
        #endregion

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Transport name, should be the same name for Server and Client
        /// </summary>
        [StatusProperty]
        public string Name => "DefaultTransport";
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
        /// <inheritdoc />
        /// <summary>
        /// Transport Counters
        /// </summary>
        [StatusProperty]
        public RPCTransportCounters Counters { get; }
        #endregion

        #region Events
        /// <summary>
        /// Events received from the RPC transport server
        /// </summary>
        public event EventHandler<EventDataEventArgs> OnEventReceived;
        #endregion

        #region .ctor
        /// <summary>
        /// Default RPC Transport client
        /// </summary>
        public DefaultTransportClient()
        {
            Serializer = SerializerManager.DefaultBinarySerializer.DeepClone();
            Core.Status.Attach(collection =>
            {
                collection.Add("Messages Waiting Response Count", _messageResponsesHandlers.Count, true);
                collection.Add("Connections count", _clients?.Length, true);
            });
        }
        /// <summary>
        /// Default RPC Transport client
        /// </summary>
        /// <param name="host">RPC Transport Server Host</param>
        /// <param name="port">RPC Transport Server Port</param>
        /// <param name="socketsPerClient">Sockets per transport client</param>
        /// <param name="serializer">Serializer for data transfer, is is null then is XmlTextSerializer</param>
        public DefaultTransportClient(string host, int port, byte socketsPerClient, ISerializer serializer = null)
        {
            Host = host;
            Port = port;
            Serializer = serializer ?? SerializerManager.DefaultBinarySerializer.DeepClone();
            if (socketsPerClient > 0)
                _maxIndex = socketsPerClient - 1;

            Core.Status.Attach(collection =>
            {
                collection.Add("Messages Waiting Response Count", _messageResponsesHandlers.Count, true);
                collection.Add("Connections count", _clients?.Length, true);
            });
        }
        #endregion


        #region Init
        /// <inheritdoc />
        /// <summary>
        /// Initialize the Transport client
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task InitAsync()
        {
            Init();
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        /// <summary>
        /// Initialize the Transport client
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            _clients = new RpcClient[_maxIndex + 1];
            for (var i = 0; i <= _maxIndex; i++)
            {
                var client = new RpcClient(Host, Port, (BinarySerializer)Serializer);
                client.OnMessageReceived += (rpcClient, message) =>
                {
                    if (message is RPCResponseMessage responseMessage)
                    {
                        if (!_messageResponsesHandlers.TryGetValue(responseMessage.RequestMessageId, out var value))
                            return;

                        value.Message = responseMessage;
                        value.Event.Set();
                    }
                };
                _clients[0] = client;
            }
        }
        #endregion

        #region Connection
        /// <summary>
        /// Connect to the transport server
        /// </summary>
        /// <returns>Task as result of the connection process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task ConnectAsync()
        {
            //TargetStatus = ConnectionStatus.Connected;
            //if (Status != ConnectionStatus.Disconnected)
            //    return Task.CompletedTask;

            //Core.Log.LibVerbose("Calling Socket Connects...");
            //Status = ConnectionStatus.Connecting;
            //_tokenSource?.Cancel();
            //_connectionResetEvent.Reset();
            //_tokenSource = new CancellationTokenSource();
            //_token = _tokenSource.Token;
            //OnConnecting?.Invoke(this, new EventArgs());

            //try
            //{
            //    //Core.Log.LibVerbose("Setting available connections tasks...");
            //    var tasks = new Task[_availableConnections.Count];
            //    for (var i = 0; i < _availableConnections.Count; i++)
            //    {
            //        var cnn = _availableConnections[i];
            //        cnn.Host = Host;
            //        cnn.Port = Port;
            //        cnn.Hub = Hub;
            //        cnn.ReceiveBufferSize = ReceiveBufferSize;
            //        cnn.SendBufferSize = SendBufferSize;
            //        Core.Log.LibVerbose("Connecting task");
            //        tasks[i] = cnn.ConnectAsync();
            //    }
            //    if (Task.WaitAll(tasks, 10000, _token))
            //    {
            //        OnConnected?.Invoke(this, new EventArgs());
            //        Status = ConnectionStatus.Connected;
            //        Core.Log.InfoDetail("Connected to: {0}:{1}", Host, Port);
            //        _connectionResetEvent.Set();
            //        return Task.CompletedTask;
            //    }
            //    Core.Log.LibVerbose("Disconnected by connection tasks timeout.");
            //    Status = ConnectionStatus.Disconnected;
            //    _connectionResetEvent.Set();
            //    return Task.CompletedTask;
            //}
            //catch (Exception ex)
            //{
            //    Core.Log.Error(ex, "Disconnected by connection tasks errors.");
            //    Status = ConnectionStatus.Disconnected;
            //    _connectionResetEvent.Set();
            //    return Task.CompletedTask;
            //}
            return Task.CompletedTask;
        }
        /// <summary>
        /// Disconnect from the transport server
        /// </summary>
        /// <returns>Task as result of the disconnection process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task DisconnectAsync()
        {
            //TargetStatus = ConnectionStatus.Disconnected;
            //if (Status != ConnectionStatus.Connected)
            //    return Task.CompletedTask;
            //Status = ConnectionStatus.Disconnecting;
            //OnDisconnecting?.Invoke(this, new EventArgs());
            //Core.Log.LibVerbose("Cancelling tasks...");
            //_tokenSource?.Cancel();
            //_tokenSource = null;
            //Task.WaitAll(_availableConnections.Select(a => a.DisconnectAsync()).ToArray(), 11000, _token);
            //Status = ConnectionStatus.Disconnected;
            //Core.Log.LibVerbose("Disconnected");
            //OnDisconnected?.Invoke(this, new EventArgs());
            return Task.CompletedTask;
        }

        #endregion

        #region GetDescriptors
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
        #endregion

        #region InvokeMethods
        /// <inheritdoc />
        /// <summary>
        /// Invokes a RPC method on the RPC server and gets the results
        /// </summary>
        /// <param name="messageRQ">RPC request message to send to the server</param>
        /// <returns>RPC response message from the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RPCResponseMessage> InvokeMethodAsync(RPCRequestMessage messageRQ)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
        #endregion


        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetCurrentIndex()
        {
            Interlocked.CompareExchange(ref _currentIndex, -1, _maxIndex);
            return Interlocked.Increment(ref _currentIndex);
        }
        #endregion
    }
}
