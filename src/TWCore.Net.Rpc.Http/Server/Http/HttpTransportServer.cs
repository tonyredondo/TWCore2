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
using System.Threading.Tasks;
using TWCore.Net.HttpServer;
using TWCore.Net.RPC.Attributes;
using TWCore.Serialization;
// ReSharper disable RedundantAssignment
// ReSharper disable CheckNamespace

namespace TWCore.Net.RPC.Server.Transports
{
    /// <inheritdoc />
    /// <summary>
    /// Http RPC Transport server
    /// </summary>
    public class HttpTransportServer : ITransportServer
    {
        private readonly SimpleHttpServer _httpServer;

        #region Properties
        /// <summary>
        /// true if the transport server can send the services descriptors; otherwise, false
        /// </summary>
        public bool EnableGetDescriptors { get; set; } = true;
        /// <summary>
        /// Serializer to encode and decode the incoming and outgoing data
        /// </summary>
        public ISerializer Serializer { get; set; }
        /// <summary>
        /// Http server port
        /// </summary>
        public int Port { get; set; }
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
        /// Http RPC Transport server
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HttpTransportServer()
        {
            _httpServer = new SimpleHttpServer();
            _httpServer.OnBeginRequest += HttpServer_OnBeginRequest;
			Serializer = new JsonTextSerializer();
            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(Port), Port);
                collection.Add(nameof(EnableGetDescriptors), EnableGetDescriptors);
                collection.Add("Bytes Sent", Counters.BytesSent);
                collection.Add("Bytes Received", Counters.BytesReceived);
                Core.Status.AttachChild(_httpServer, this);
                Core.Status.AttachChild(Serializer, this);
            });
        }
        /// <summary>
        /// Http RPC Transport server
        /// </summary>
        /// <param name="port">Http server port</param>
        /// <param name="serializer">Serializer for data transfer, is is null then is XmlTextSerializer</param>
        /// <param name="enableGetDescriptors">true if the transport server can send the service descriptor; otherwise, false</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HttpTransportServer(int port, ISerializer serializer = null, bool enableGetDescriptors = true) : this()
        {
            Port = port;
            Serializer = serializer ?? Serializer;
            EnableGetDescriptors = enableGetDescriptors;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts the server listener
        /// </summary>
        /// <returns>Task as result of the startup process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task StartListenerAsync()
        {
            Core.Log.LibVerbose("Starting Transport Listener");
            await _httpServer.StartAsync(Port).ConfigureAwait(false);
            Core.Log.LibVerbose("Transport Listener Started");
        }
        /// <summary>
        /// Stops the server listener
        /// </summary>
        /// <returns>Task as result of the stop process</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task StopListenerAsync()
        {
            Core.Log.LibVerbose("Stopping Transport Listener");
            await _httpServer.StopAsync().ConfigureAwait(false);
            Core.Log.LibVerbose("Transport Listener Stopped");
        }
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
            Core.Log.Warning("Events is not supported on HttpTransportServer");
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Handles an OnBeginRequest event
        /// </summary>
        /// <param name="context">Http context</param>
        /// <param name="handled">Get if the request was handled</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HttpServer_OnBeginRequest(HttpContext context, ref bool handled)
        {
            var request = context.Request;
            var response = context.Response;
            Core.Log.LibVerbose("Request received from {0}:{1} to {2} {3}", request.RemoteAddress, request.RemotePort, request.Method, request.RawUrl);

            var clientId = Guid.NewGuid();
            OnClientConnect?.Invoke(this, new ClientConnectEventArgs(clientId));

            context.Response.ContentType = Serializer.MimeTypes[0];
            var responseBuffer = default(SubArray<byte>);

            if (context.Request.Method == HttpMethod.GET && EnableGetDescriptors && OnGetDescriptorsRequest != null)
            {
                var eArgs = new ServerDescriptorsEventArgs();
                OnGetDescriptorsRequest(this, eArgs);
                responseBuffer = Serializer.Serialize(eArgs.Descriptors);
            }
            if (context.Request.Method == HttpMethod.POST && OnMethodCall != null)
            {
                Counters.IncrementBytesReceived(context.Request.PostData.Length);
                var messageRq = Serializer.Deserialize<RPCRequestMessage>(context.Request.PostData);
                var eArgs = new MethodEventArgs(clientId, messageRq);
                OnMethodCall(this, eArgs);
                if (eArgs.Response != null)
                    responseBuffer = Serializer.Serialize(eArgs.Response);
            }
            response.Write(responseBuffer.Array, responseBuffer.Offset, responseBuffer.Count);
            Counters.IncrementBytesSent(responseBuffer.Count);
            handled = true;
        }
        #endregion
    }
}
