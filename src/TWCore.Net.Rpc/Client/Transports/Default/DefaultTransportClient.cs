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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Diagnostics.Status;
using TWCore.Net.RPC.Descriptors;
using TWCore.Serialization;
using TWCore.Threading;
// ReSharper disable MethodSupportsCancellation

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Net.RPC.Client.Transports.Default
{
    /// <inheritdoc />
    /// <summary>
    /// Default RPC Transport client
    /// </summary>
    [StatusName("Transport")]
    public class DefaultTransportClient : ITransportClient
    {
        private const int ResetIndex = 500000;
        private readonly ConcurrentDictionary<Guid, RpcMessageHandler> _messageResponsesHandlers = new ConcurrentDictionary<Guid, RpcMessageHandler>();
        private readonly LRU2QCollection<Guid, object> _previousMessages = new LRU2QCollection<Guid, object>(100);
        private readonly ReferencePool<RpcMessageHandler> _messageHandlerPool = new ReferencePool<RpcMessageHandler>(0, item =>
        {
            item.Message = null;
            item.Event.Reset();
        });
        private readonly AsyncLock _connectionLocker = new AsyncLock();
        private CancellationTokenSource _connectionCancellationTokenSource;
        private CancellationToken _connectionCancellationToken;
        private bool _shouldBeConnected;
        private readonly int _socketsPerClient = 4;
        private int _currentIndex = -1;
        private RpcClient[] _clients;

        #region Nested Types
        private sealed class RpcMessageHandler
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
        public string Host { get; }
        /// <summary>
        /// RPC Transport Server port
        /// </summary>
        [StatusProperty]
        public int Port { get; }
        /// <summary>
        /// Invoke Method Timeout
        /// </summary>
        [StatusProperty]
        public int InvokeMethodTimeout { get; set; } = 45000;
        #endregion

        #region Events
        /// <summary>
        /// Events received from the RPC transport server
        /// </summary>
        public event EventHandler<EventDataEventArgs> OnEventReceived;
        /// <summary>
        /// Event when a push message has been received from server
        /// </summary>
        public event EventHandler<EventArgs<RPCPushMessage>> OnPushMessageReceived;
        #endregion

        #region .ctor
        /// <summary>
        /// Default RPC Transport client
        /// </summary>
        public DefaultTransportClient()
        {
            Serializer = SerializerManager.DefaultBinarySerializer.DeepClone();
            if (Serializer != null)
            {
                Serializer.KnownTypes.Add(typeof(RPCError));
                Serializer.KnownTypes.Add(typeof(RPCEventMessage));
                Serializer.KnownTypes.Add(typeof(RPCPushMessage));
                Serializer.KnownTypes.Add(typeof(RPCRequestMessage));
                Serializer.KnownTypes.Add(typeof(RPCResponseMessage));
                Serializer.KnownTypes.Add(typeof(RPCSessionRequestMessage));
                Serializer.KnownTypes.Add(typeof(RPCSessionResponseMessage));
            }
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
                _socketsPerClient = socketsPerClient;
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
            Core.Status.Attach(collection =>
            {
                collection.Add("Messages Waiting Response Count", _messageResponsesHandlers.Count, true);
                collection.Add("Connections count", _clients?.Length, true);
            });
        }
        #endregion

        #region Connection
        /// <summary>
        /// Connect to the transport server
        /// </summary>
        /// <returns>Task as result of the connection process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task ConnectAsync()
        {
            if (_shouldBeConnected) return;
            using (await _connectionLocker.LockAsync().ConfigureAwait(false))
            {
                _shouldBeConnected = true;
                try
                {
                    _connectionCancellationTokenSource = new CancellationTokenSource();
                    _connectionCancellationToken = _connectionCancellationTokenSource.Token;
                    if (_clients is null)
                    {
                        _clients = new RpcClient[_socketsPerClient];
                        for (var i = 0; i < _clients.Length; i++)
                        {
                            var client = new RpcClient(Host, Port, (BinarySerializer)Serializer);
                            client.OnMessageReceived += ClientOnOnMessageReceived;
                            _clients[i] = client;
                        }
                    }
                    await _clients.Select(c => c.ConnectAsync()).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    _shouldBeConnected = false;
                    throw;
                }
            }
        }
        /// <summary>
        /// Disconnect from the transport server
        /// </summary>
        /// <returns>Task as result of the disconnection process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task DisconnectAsync()
        {
            if (!_shouldBeConnected) return;
            using (await _connectionLocker.LockAsync().ConfigureAwait(false))
            {
                _shouldBeConnected = false;
                _connectionCancellationTokenSource.Cancel();
                await _clients.Select(c => c.DisconnectAsync()).ConfigureAwait(false);
            }
        }
        #endregion

        #region Interface Methods

        /// <inheritdoc />
        /// <summary>
        /// Initialize the Transport client
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task InitAsync()
        {
            try
            {
                await ConnectAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Core.Log.Error(ex.Message);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the descriptors for the RPC service
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<ServiceDescriptorCollection> GetDescriptorsAsync()
        {
            var request = RPCRequestMessage.Retrieve(Guid.Empty, null, false);
            var response = await InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            return (ServiceDescriptorCollection)response.ReturnValue;
        }
        /// <inheritdoc />
        /// <summary>
        /// Invokes a RPC method on the RPC server and gets the results
        /// </summary>
        /// <param name="messageRq">RPC request message to send to the server</param>
        /// <returns>RPC response message from the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RPCResponseMessage> InvokeMethodAsync(RPCRequestMessage messageRq)
        {
            if (!_shouldBeConnected)
                await ConnectAsync().ConfigureAwait(false);
            if (_connectionCancellationToken.IsCancellationRequested) return null;
            var handler = _messageHandlerPool.New();
            while (!_messageResponsesHandlers.TryAdd(messageRq.MessageId, handler))
                await Task.Yield();
            if (_currentIndex > ResetIndex) _currentIndex = -1;
            bool sent;
            do
            {
                var client = _clients[Interlocked.Increment(ref _currentIndex) % _socketsPerClient];
                sent = await client.SendRpcMessageAsync(messageRq).ConfigureAwait(false);
            } while (!sent);
            await handler.Event.WaitAsync(InvokeMethodTimeout, _connectionCancellationToken).ConfigureAwait(false);
            if (handler.Event.IsSet)
            {
                var msg = handler.Message;
                _messageHandlerPool.Store(handler);
                return msg;
            }
            _messageHandlerPool.Store(handler);
            _connectionCancellationToken.ThrowIfCancellationRequested();
            throw new TimeoutException($"Timeout of {InvokeMethodTimeout / 1000} seconds has been reached waiting the response from the server with Id={messageRq.MessageId}.");
        }
        /// <inheritdoc />
        /// <summary>
        /// Invokes a RPC method on the RPC server and gets the results
        /// </summary>
        /// <param name="messageRq">RPC request message to send to the server</param>
        /// <param name="cancellationToken">Cancellation Token instance</param>
        /// <returns>RPC response message from the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RPCResponseMessage> InvokeMethodAsync(RPCRequestMessage messageRq, CancellationToken cancellationToken)
        {
            if (!_shouldBeConnected)
                await ConnectAsync().ConfigureAwait(false);
            if (_connectionCancellationToken.IsCancellationRequested) return null;
            var handler = _messageHandlerPool.New();
            while (!_messageResponsesHandlers.TryAdd(messageRq.MessageId, handler))
                await Task.Yield();
            if (_currentIndex > ResetIndex) _currentIndex = -1;
            bool sent;
            RpcClient client;
            using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_connectionCancellationToken, cancellationToken))
            {
                do
                {
                    client = _clients[Interlocked.Increment(ref _currentIndex) % _socketsPerClient];
                    sent = await client.SendRpcMessageAsync(messageRq).ConfigureAwait(false);
                } while (!sent);
                await handler.Event.WaitAsync(InvokeMethodTimeout, linkedTokenSource.Token).ConfigureAwait(false);
            }
            if (handler.Event.IsSet)
            {
                var msg = handler.Message;
                _messageHandlerPool.Store(handler);
                return msg;
            }
            _messageHandlerPool.Store(handler);
            _connectionCancellationToken.ThrowIfCancellationRequested();
            if (cancellationToken.IsCancellationRequested)
            {
                await client.SendRpcMessageAsync(new RPCCancelMessage { MessageId = messageRq.MessageId }).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            }
            throw new TimeoutException($"Timeout of {InvokeMethodTimeout / 1000} seconds has been reached waiting the response from the server with Id={messageRq.MessageId}.");
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            DisconnectAsync().WaitAsync();
        }

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClientOnOnMessageReceived(RpcClient rpcClient1, RPCMessage rpcMessage)
        {
            switch (rpcMessage)
            {
                case RPCResponseMessage responseMessage:
                    if (!_messageResponsesHandlers.TryRemove(responseMessage.RequestMessageId, out var value))
                        return;
                    value.Message = responseMessage;
                    value.Event.Set();
                    break;
                case RPCEventMessage eventMessage:
                    if (!_previousMessages.TryGetValue(eventMessage.MessageId, out _))
                    {
                        _previousMessages.TryAdd(eventMessage.MessageId, null);
                        var edea = EventDataEventArgs.Retrieve(eventMessage.ServiceName, eventMessage.EventName, eventMessage.EventArgs);
                        OnEventReceived?.Invoke(this, edea);
                        EventDataEventArgs.Store(edea);
                    }
                    break;
                case RPCPushMessage pushMessage:
                    if (!_previousMessages.TryGetValue(pushMessage.MessageId, out _))
                    {
                        _previousMessages.TryAdd(pushMessage.MessageId, null);
                        var evArgs = ReferencePool<EventArgs<RPCPushMessage>>.Shared.New();
                        evArgs.Item1 = pushMessage;
                        OnPushMessageReceived?.Invoke(this, evArgs);
                        evArgs.Item1 = null;
                        ReferencePool<EventArgs<RPCPushMessage>>.Shared.Store(evArgs);
                    }
                    break;
                case RPCError errorMessage:
                    var respMsg = RPCResponseMessage.Retrieve(errorMessage);
                    foreach (var mHandler in _messageResponsesHandlers.ToArray())
                    {
                        mHandler.Value.Message = respMsg;
                        mHandler.Value.Event.Set();
                        _messageResponsesHandlers.TryRemove(mHandler.Key, out _);
                    }
                    break;
            }
        }
        #endregion
    }
}
