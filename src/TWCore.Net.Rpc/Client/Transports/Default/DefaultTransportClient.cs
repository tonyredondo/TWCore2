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
        private readonly NonBlocking.ConcurrentDictionary<Guid, RpcMessageHandler> _messageResponsesHandlers = new NonBlocking.ConcurrentDictionary<Guid, RpcMessageHandler>();
        private readonly LRU2QCollection<Guid, object> _previousMessages = new LRU2QCollection<Guid, object>(100);
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
        /// <inheritdoc />
        /// <summary>
        /// Transport Counters
        /// </summary>
        public RPCTransportCounters Counters { get; }
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
                    if (_clients == null)
                    {
                        _clients = new RpcClient[_socketsPerClient];
                        for (var i = 0; i < _clients.Length; i++)
                        {
                            var client = new RpcClient(Host, Port, (BinarySerializer)Serializer);
                            client.OnMessageReceived += ClientOnOnMessageReceived;
                            _clients[i] = client;
                        }
                    }
                    await Task.WhenAll(_clients.Select(c => c.ConnectAsync())).ConfigureAwait(false);
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
                await Task.WhenAll(_clients.Select(c => c.DisconnectAsync())).ConfigureAwait(false);
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
            var request = new RPCRequestMessage { MethodId = Guid.Empty };
            var response = await InvokeMethodAsync(request).ConfigureAwait(false);
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
            var handler = new RpcMessageHandler();
            while (!_messageResponsesHandlers.TryAdd(messageRq.MessageId, handler))
                await Task.Delay(1).ConfigureAwait(false);
            if (_currentIndex > ResetIndex) _currentIndex = -1;
            bool sent;
            do
            {
                var client = _clients[Interlocked.Increment(ref _currentIndex) % _socketsPerClient];
                sent = await client.SendRpcMessageAsync(messageRq).ConfigureAwait(false);
                if (!sent)
                {
                    try
                    {
                        await client.DisconnectAsync();
                        await client.ConnectAsync();
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Write(ex);
                    }
                }
            } while (!sent);
            await handler.Event.WaitAsync(InvokeMethodTimeout, _connectionCancellationToken).ConfigureAwait(false);
            if (handler.Event.IsSet)
                return handler.Message;
            _connectionCancellationToken.ThrowIfCancellationRequested();
            throw new TimeoutException("Timeout of {0} seconds has been reached waiting the response from the server with Id={1}.".ApplyFormat(InvokeMethodTimeout / 1000, messageRq.MessageId));
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
            var handler = new RpcMessageHandler();
            while (!_messageResponsesHandlers.TryAdd(messageRq.MessageId, handler))
                await Task.Delay(1).ConfigureAwait(false);
            if (_currentIndex > ResetIndex) _currentIndex = -1;
            bool sent;
            RpcClient client;
            using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_connectionCancellationToken, cancellationToken))
            {
                do
                {
                    client = _clients[Interlocked.Increment(ref _currentIndex) % _socketsPerClient];
                    sent = await client.SendRpcMessageAsync(messageRq).ConfigureAwait(false);
                    if (!sent)
                    {
                        try
                        {
                            await client.DisconnectAsync();
                            await client.ConnectAsync();
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Write(ex);
                        }
                    }
                } while (!sent);
                await handler.Event.WaitAsync(InvokeMethodTimeout, linkedTokenSource.Token).ConfigureAwait(false);
            }

            if (handler.Event.IsSet)
                return handler.Message;
            _connectionCancellationToken.ThrowIfCancellationRequested();
            if (cancellationToken.IsCancellationRequested)
            {
                await client.SendRpcMessageAsync(new RPCCancelMessage { MessageId = messageRq.MessageId }).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            }
            throw new TimeoutException("Timeout of {0} seconds has been reached waiting the response from the server with Id={1}.".ApplyFormat(InvokeMethodTimeout / 1000, messageRq.MessageId));
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
                    _previousMessages.GetOrAdd(eventMessage.MessageId, mId =>
                    {
                        OnEventReceived?.InvokeAsync(this, new EventDataEventArgs(eventMessage.ServiceName, eventMessage.EventName, eventMessage.EventArgs));
                        return null;
                    });
                    break;
                case RPCPushMessage pushMessage:
                    _previousMessages.GetOrAdd(pushMessage.MessageId, mId =>
                    {
                        OnPushMessageReceived?.InvokeAsync(this, new EventArgs<RPCPushMessage>(pushMessage));
                        return null;
                    });
                    break;
                case RPCError errorMessage:
                    var respMsg = new RPCResponseMessage { Exception = errorMessage.Exception };
                    foreach (var mHandler in _messageResponsesHandlers.ToArray())
                    {
                        mHandler.Value.Message = respMsg;
                        mHandler.Value.Event.Set();
                        _messageResponsesHandlers.TryRemove(mHandler.Key, out var _);
                    }
                    break;
            }
        }
        #endregion
    }
}
