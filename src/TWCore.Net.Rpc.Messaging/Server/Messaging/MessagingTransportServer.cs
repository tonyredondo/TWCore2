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
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TWCore.Messaging.Server;
using TWCore.Net.RPC.Attributes;
using TWCore.Serialization;

// ReSharper disable RedundantAssignment
// ReSharper disable CheckNamespace
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore.Net.RPC.Server.Transports
{
    /// <inheritdoc />
    /// <summary>
    /// Messaging RPC Transport server
    /// </summary>
    public class MessagingTransportServer : ITransportServer
    {
        private readonly IMQueueServer _queueServer;
        
        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Transport name, should be the same name for Server and Client
        /// </summary>
        public string Name => "MessagingTransport";
        /// <inheritdoc />
        /// <summary>
        /// true if the transport server can send the services descriptors; otherwise, false
        /// </summary>
        public bool EnableGetDescriptors { get; set; } = true;
        /// <inheritdoc />
        /// <summary>
        /// Serializer to encode and decode the incoming and outgoing data
        /// </summary>
        public ISerializer Serializer { get; set; }
        /// <inheritdoc />
        /// <summary>
        /// Transport Counters
        /// </summary>
        public RPCTransportCounters Counters { get; } = new RPCTransportCounters();
        #endregion

        #region Events
        /// <summary>
        /// Event that fires when a Descriptors request is received.
        /// </summary>
        public event EventHandler<ServerDescriptorsEventArgs> OnGetDescriptorsRequest;
        /// <summary>
        /// Event that fires when a Method call is received
        /// </summary>
        public event EventHandler<MethodEventArgs> OnMethodCall;
        /// <summary>
        /// Event that fires when a client connects.
        /// </summary>
        public event EventHandler<ClientConnectEventArgs> OnClientConnect;
        #endregion

        #region .ctor
        /// <summary>
        /// Messaging RPC Transport server
        /// </summary>
        /// <param name="queueServer">QueueServer instance</param>
        /// <param name="serializer">Serializer instance</param>
        public MessagingTransportServer(IMQueueServer queueServer, ISerializer serializer = null)
        {
            _queueServer = queueServer;
            Serializer = serializer ?? SerializerManager.DefaultBinarySerializer;
            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(Name), Name);
                collection.Add(nameof(EnableGetDescriptors), EnableGetDescriptors);
                collection.Add("Bytes Sent", Counters.BytesSent, true);
                collection.Add("Bytes Received", Counters.BytesReceived, true);
                Core.Status.AttachChild(Serializer, this);
                Core.Status.AttachChild(_queueServer, this);
            }, this);
        }
        #endregion
        
        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Starts the server listener
        /// </summary>
        /// <returns>Task as result of the startup process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task StartListenerAsync()
        {
            Core.Log.LibVerbose("Starting Transport Listener");
            _queueServer.StartListeners();
            _queueServer.RequestReceived += QueueServerOnRequestReceived;
            Core.Log.LibVerbose("Transport Listener Started");
            return Task.CompletedTask;
        }


        /// <inheritdoc />
        /// <summary>
        /// Stops the server listener
        /// </summary>
        /// <returns>Task as result of the stop process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task StopListenerAsync()
        {
            Core.Log.LibVerbose("Stopping Transport Listener");
            _queueServer.StopListeners();
            _queueServer.RequestReceived -= QueueServerOnRequestReceived;
            Core.Log.LibVerbose("Transport Listener Stopped");
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        /// <summary>
        /// Send a fire event trigger to a RPC client.
        /// </summary>
        /// <param name="eventAttribute">Event attribute</param>
        /// <param name="clientId">Client identifier</param>
        /// <param name="serviceName">Service name</param>
        /// <param name="eventName">Event name</param>
        /// <param name="sender">Sender information</param>
        /// <param name="e">Event args</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FireEvent(RPCEventAttribute eventAttribute, Guid clientId, string serviceName, string eventName, object sender, EventArgs e)
        {
            Core.Log.Warning("Events is not supported on MessagingTransportServer");
        }
        #endregion
        
        #region Private Methods
        private void QueueServerOnRequestReceived(object sender, RequestReceivedEventArgs requestReceivedEventArgs)
        {
            var body = requestReceivedEventArgs.Request.Body;
            Counters.IncrementBytesReceived(requestReceivedEventArgs.MessageLength);
            switch (body)
            {
                case string strValue:
                    if (strValue == "GetDescription")
                    {
                        var sDesc = new ServerDescriptorsEventArgs();
                        if (OnGetDescriptorsRequest != null)
                        {
                            OnGetDescriptorsRequest(requestReceivedEventArgs.Request.CorrelationId, sDesc);
                            requestReceivedEventArgs.Response.Body = sDesc.Descriptors;
                        }
                    }
                    break;
                case RPCRequestMessage rqMessage:
                    var mEvent = new MethodEventArgs(requestReceivedEventArgs.Request.CorrelationId, rqMessage);
                    if (OnMethodCall != null)
                    {
                        OnMethodCall(this, mEvent);
                        requestReceivedEventArgs.Response.Body = mEvent.Response;
                        return;
                    }
                    break;
            }
        }
        #endregion
    }
}
