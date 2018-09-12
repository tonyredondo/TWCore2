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

#pragma warning disable 0067
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Messaging.Client;
using TWCore.Net.RPC.Descriptors;
using TWCore.Serialization;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore.Net.RPC.Client.Transports
{
    /// <inheritdoc />
    /// <summary>
    /// Messaging RPC Transport client
    /// </summary>
    public class MessagingTransportClient : ITransportClient
    {
        private readonly IMQueueClient _queueClient;
        private readonly Dictionary<Guid, ServiceDescriptor> _methods = new Dictionary<Guid, ServiceDescriptor>(100);
        private ServiceDescriptorCollection _descriptors;
        
        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Transport name, should be the same name for Server and Client
        /// </summary>
        [StatusProperty]
        public string Name => "MessagingTransport";
        /// <inheritdoc />
        /// <summary>
        /// Serializer to encode and decode the incoming and outgoing data
        /// </summary>
        [StatusProperty]
        public ISerializer Serializer
        {
            get => _queueClient?.SenderSerializer;
            set
            {
                if (_queueClient is null) return;
                _queueClient.SenderSerializer = value;
                _queueClient.ReceiverSerializer = value;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Services descriptors to use on RPC Request messages
        /// </summary>
        public ServiceDescriptorCollection Descriptors
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _descriptors;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _descriptors = value;
                if (_descriptors is null) return;
                _methods.Clear();
                foreach (var descriptor in _descriptors.Items.Values)
                {
                    foreach (var mtd in descriptor.Methods)
                        _methods.Add(mtd.Key, descriptor);
                }
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Transport Counters
        /// </summary>
        public RPCTransportCounters Counters { get; } = new RPCTransportCounters();
        #endregion

        #region Events
        /// <summary>
        /// Events received from the RPC transport server
        /// </summary>
        public event EventHandler<EventDataEventArgs> OnEventReceived;
        #endregion

        #region .ctor
        /// <summary>
        /// Messaging RPC Transport client
        /// </summary>
        /// <param name="queueClient">QueueClient instance</param>
        public MessagingTransportClient(IMQueueClient queueClient)
        {
            _queueClient = queueClient;
            Core.Status.Attach(collection =>
            {
                Core.Status.AttachChild(Serializer, this);
                Core.Status.AttachChild(_queueClient, this);
            }, this);
        }
        /// <summary>
        /// Messaging RPC Transport client
        /// </summary>
        /// <param name="queueClient">QueueClient instance</param>
        /// <param name="serializer">Serializer instance</param>
        public MessagingTransportClient(IMQueueClient queueClient, ISerializer serializer)
        {
            _queueClient = queueClient;
            Serializer = serializer ?? Serializer ?? SerializerManager.DefaultBinarySerializer;
            Core.Status.Attach(collection =>
            {
                Core.Status.AttachChild(Serializer, this);
                Core.Status.AttachChild(_queueClient, this);
            }, this);
        }
        #endregion
        
        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Initialize the Transport client
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task InitAsync() => Task.CompletedTask;
        /// <inheritdoc />
        /// <summary>
        /// Gets the descriptor for the RPC service
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<ServiceDescriptorCollection> GetDescriptorsAsync()
            => _queueClient.SendAndReceiveAsync<ServiceDescriptorCollection>("GetDescription");
        /// <inheritdoc />
        /// <summary>
        /// Invokes a RPC method on the RPC server and gets the results
        /// </summary>
        /// <param name="messageRQ">RPC request message to send to the server</param>
        /// <returns>RPC response message from the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RPCResponseMessage> InvokeMethodAsync(RPCRequestMessage messageRQ)
            => _queueClient.SendAndReceiveAsync<RPCResponseMessage, RPCRequestMessage>(messageRQ);
        /// <inheritdoc />
        /// <summary>
        /// Invokes a RPC method on the RPC server and gets the results
        /// </summary>
        /// <param name="messageRQ">RPC request message to send to the server</param>
        /// <param name="cancellationToken">Cancellation Token instance</param>
        /// <returns>RPC response message from the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<RPCResponseMessage> InvokeMethodAsync(RPCRequestMessage messageRQ, CancellationToken cancellationToken)
            => _queueClient.SendAndReceiveAsync<RPCResponseMessage, RPCRequestMessage>(messageRQ, cancellationToken);
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
            => _queueClient.Dispose();
        #endregion
    }
}
