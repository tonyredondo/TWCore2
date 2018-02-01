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
using System.Threading;
using System.Threading.Tasks;
using TWCore.Net.RPC.Descriptors;
using TWCore.Serialization;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Net.RPC.Client.Transports
{
    /// <inheritdoc />
    /// <summary>
    /// Define a RPC client transport
    /// </summary>
    public interface ITransportClient : IDisposable
    {
        /// <summary>
        /// Transport name, should be the same name for Server and Client
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Events received from the RPC transport server
        /// </summary>
        event EventHandler<EventDataEventArgs> OnEventReceived;
        /// <summary>
        /// Serializer to encode and decode the incoming and outgoing data
        /// </summary>
        ISerializer Serializer { get; set; }
        /// <summary>
        /// Services descriptors to use on RPC Request messages
        /// </summary>
        ServiceDescriptorCollection Descriptors { get; set; }
        /// <summary>
        /// Transport Counters
        /// </summary>
        RPCTransportCounters Counters { get; }

        /// <summary>
        /// Gets the descriptors for the RPC server
        /// </summary>
        /// <returns>Task of the method execution</returns>
        Task<ServiceDescriptorCollection> GetDescriptorsAsync();
        /// <summary>
        /// Invokes a RPC method on the RPC server and gets the results
        /// </summary>
        /// <param name="messageRQ">RPC request message to send to the server</param>
        /// <returns>RPC response message from the server</returns>
        Task<RPCResponseMessage> InvokeMethodAsync(RPCRequestMessage messageRQ);
        /// <summary>
        /// Invokes a RPC method on the RPC server and gets the results
        /// </summary>
        /// <param name="messageRQ">RPC request message to send to the server</param>
        /// <param name="cancellationToken">Cancellation token instance</param>
        /// <returns>RPC response message from the server</returns>
        Task<RPCResponseMessage> InvokeMethodAsync(RPCRequestMessage messageRQ, CancellationToken cancellationToken);
        /// <summary>
        /// Initialize the Transport client
        /// </summary>
        /// <returns>Task of the method execution</returns>
        Task InitAsync();
    }
}
