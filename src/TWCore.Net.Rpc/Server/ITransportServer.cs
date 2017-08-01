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
using System.Threading.Tasks;
using TWCore.Net.RPC.Attributes;
using TWCore.Serialization;

namespace TWCore.Net.RPC.Server
{
    /// <summary>
    /// Define a RPC Transport Server
    /// </summary>
    public interface ITransportServer
    {
        /// <summary>
        /// true if the transport server can send the service descriptor; otherwise, false
        /// </summary>
        bool EnableGetDescriptors { get; set; }
        /// <summary>
        /// Serializer to encode and decode the incoming and outgoing data
        /// </summary>
        ISerializer Serializer { get; set; }
        /// <summary>
        /// Transport Counters
        /// </summary>
        RPCTransportCounters Counters { get; }

        /// <summary>
        /// Event that fires when a Descriptor request is received.
        /// </summary>
        event EventHandler<ServerDescriptorsEventArgs> OnGetDescriptorsRequest;
        /// <summary>
        /// Event that fires when a Method call is received
        /// </summary>
        event EventHandler<MethodEventArgs> OnMethodCall;
        /// <summary>
        /// Event that fires when a client connects.
        /// </summary>
        event EventHandler<ClientConnectEventArgs> OnClientConnect;

        /// <summary>
        /// Send a fire event trigger to a RPC client.
        /// </summary>
        /// <param name="eventAttribute">Event attribute</param>
        /// <param name="clientId">Client identifier</param>
        /// <param name="serviceName">Service name of the event</param>
        /// <param name="eventName">Event name</param>
        /// <param name="sender">Sender information</param>
        /// <param name="e">Event args</param>
        void FireEvent(RPCEventAttribute eventAttribute, Guid clientId, string serviceName, string eventName, object sender, EventArgs e);

        /// <summary>
        /// Starts the server listener
        /// </summary>
        /// <returns>Task as result of the startup process</returns>
        Task StartListenerAsync();
        /// <summary>
        /// Stops the server listener
        /// </summary>
        /// <returns>Task as result of the stop process</returns>
        Task StopListenerAsync();
    }
}
