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
        private readonly ConcurrentDictionary<Guid, (AsyncManualResetEvent Event, RPCResponseMessage Message)> _messageResponsesHandlers =
            new ConcurrentDictionary<Guid, (AsyncManualResetEvent Event, RPCResponseMessage Message)>();

        private readonly int _maxIndex = 3;
        private int _currentIndex = -1;
        private RpcClient[] _clients;


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
                _clients[0] = client;
            }
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
