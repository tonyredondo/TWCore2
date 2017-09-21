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

#pragma warning disable 0067
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Net.RPC.Descriptors;
using TWCore.Serialization;
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore.Net.RPC.Client.Transports
{
    /// <inheritdoc />
    /// <summary>
    /// Http RPC Transport client
    /// </summary>
    public class HttpTransportClient : ITransportClient
    {
	    private readonly Dictionary<Guid, ServiceDescriptor> _methods = new Dictionary<Guid, ServiceDescriptor>(100);
	    private ServiceDescriptorCollection _descriptors;
	    private HttpClient _httpClient;

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Transport name, should be the same name for Server and Client
        /// </summary>
        [StatusProperty]
        public string Name => "HttpTransport";
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
		public ServiceDescriptorCollection Descriptors 
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _descriptors;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set 
			{
				_descriptors = value;
				if (_descriptors == null) return;
				_methods.Clear();
				foreach(var descriptor in _descriptors.Items.Values)
				{
					foreach(var mtd in descriptor.Methods)
						_methods.Add(mtd.Key, descriptor);
				}
			}
		}
        /// <summary>
        /// Http RPC Transport Server URL
        /// </summary>
        [StatusProperty]
        public string Url { get; set; }
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
        /// Http RPC Transport client
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HttpTransportClient()
        {
            var handler = new DecompressionHandler()
            {
                InnerHandler = new HttpClientHandler()
            };
            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            Serializer = new JsonTextSerializer();

            Core.Status.Attach(collection =>
            {
                collection.Add("Bytes Sent", Counters.BytesSent, true);
                collection.Add("Bytes Received", Counters.BytesReceived, true);
                Core.Status.AttachChild(_httpClient, this);
            }, this);
        }
        /// <inheritdoc />
        /// <summary>
        /// Http RPC Transport client
        /// </summary>
        /// <param name="url">Http RPC Transport Server URL</param>
        /// <param name="serializer">Serializer for data transfer, is is null then is XmlTextSerializer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HttpTransportClient(string url, ISerializer serializer = null) : this()
        {
            Url = url;
            Serializer = serializer ?? Serializer;
        }
        #endregion

        #region Public Methods
        /// <inheritdoc />
        /// <summary>
        /// Initialize the Transport client
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task InitAsync() { return Task.CompletedTask; }
        /// <inheritdoc />
        /// <summary>
        /// Initialize the Transport client
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init() {}
        /// <inheritdoc />
        /// <summary>
        /// Gets the descriptor for the RPC service
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<ServiceDescriptorCollection> GetDescriptorsAsync()
        {
            var result = await _httpClient.GetAsync(Url).ConfigureAwait(false);
            var data = await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            return Serializer.Deserialize<ServiceDescriptorCollection>(data);
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets the descriptor for the RPC service
        /// </summary>
        /// <returns>Task of the method execution</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceDescriptorCollection GetDescriptors()
        {
            var result = _httpClient.GetAsync(Url).WaitAsync();
            var data = result.Content.ReadAsByteArrayAsync().WaitAsync();
            return Serializer.Deserialize<ServiceDescriptorCollection>(data);
        }
        /// <inheritdoc />
        /// <summary>
        /// Invokes a RPC method on the RPC server and gets the results
        /// </summary>
        /// <param name="messageRQ">RPC request message to send to the server</param>
        /// <returns>RPC response message from the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<RPCResponseMessage> InvokeMethodAsync(RPCRequestMessage messageRQ)
        {
			if (_methods.TryGetValue(messageRQ.MethodId, out var descriptor))
            {
                foreach (var tDesc in descriptor.Types.Values)
                {
                    var type = Core.GetType(tDesc.FullName);
                    if (type != null)
                        Serializer.KnownTypes.Add(type);
                }
            }
            var dataRQ = Serializer.Serialize(messageRQ);
            var sContent = new StreamContent(dataRQ.ToMemoryStream());
            var postResult = await _httpClient.PostAsync(Url, sContent).ConfigureAwait(false);
			Counters.IncrementBytesSent(dataRQ.Count);
            var dataRS = await postResult.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            Counters.IncrementBytesReceived(dataRS.Length);
            var res = Serializer.Deserialize<RPCResponseMessage>(dataRS);
            return res;
        }
        /// <inheritdoc />
        /// <summary>
        /// Invokes a RPC method on the RPC server and gets the results
        /// </summary>
        /// <param name="messageRQ">RPC request message to send to the server</param>
        /// <returns>RPC response message from the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RPCResponseMessage InvokeMethod(RPCRequestMessage messageRQ)
			=> InvokeMethodAsync(messageRQ).WaitAndResults();
        /// <inheritdoc />
        /// <summary>
        /// Dispose all resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _httpClient?.Dispose();
            _httpClient = null;
        }
        #endregion
    }
}
