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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Net.RPC.Descriptors;

namespace TWCore.Net.RPC.Client
{
    /// <summary>
    /// RPC standard client
    /// </summary>
    public class RPCClient : IRPCClient, IRPCClientAsync, IDisposable
    {
        ITransportClient transport = null;
        ServiceDescriptorCollection serverDescriptors = null;
        ServiceDescriptorCollection descriptors = null;

        readonly ConcurrentDictionary<(string, string, string), MethodDescriptor> MethodDescriptorCache = new ConcurrentDictionary<(string, string, string), MethodDescriptor>();
        bool transportInit = false;

        #region Properties
        /// <summary>
        /// Service descriptor collection
        /// </summary>
        public ServiceDescriptorCollection Descriptors => GetDescriptorsAsync().WaitAndResults();
        /// <summary>
        /// Transport client object
        /// </summary>
        [StatusProperty]
        public ITransportClient Transport
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return transport; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (transport != null)
                    transport.OnEventReceived -= Transport_OnEventReceived;
                transport = value;
                if (transport != null)
                {
                    transport.OnEventReceived += Transport_OnEventReceived;
                    InitTransportAsync().WaitAsync();
                }
            }
        }
        /// <summary>
        /// Fires when an event from the RPC server fires
        /// </summary>
        public event EventHandler<EventDataEventArgs> OnEventReceived;
        /// <summary>
        /// Gets or Sets send request using the index in the descriptor (reduce size) or the method id
        /// </summary>
        [StatusProperty]
        public bool RequestByDescriptorIndex { get; set; } = true;
        /// <summary>
        /// Gets or Sets if the RPC client needs to get the server description for checks
        /// </summary>
        [StatusProperty]
        public bool UseServerDescriptor { get; set; } = false;
        #endregion

        #region .ctor
        /// <summary>
        /// RPC standard client
        /// </summary>
        /// <param name="transport">Client transport</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RPCClient(ITransportClient transport = null)
        {
            Transport = transport;
            Core.Status.AttachChild(transport, this);
        }
        #endregion

        #region Create Proxy Async
        /// <summary>
        /// Creates a dynamic object to act as client proxy
        /// </summary>
        /// <typeparam name="T">Type of the service to create the proxy</typeparam>
        /// <returns>Dynamic proxy instance, use dynamic keyword to dynamically invoke methods to the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<DynamicProxy> CreateDynamicProxyAsync<T>() => CreateDynamicProxyAsync(typeof(T));
        /// <summary>
        /// Creates a dynamic object to act as client proxy
        /// </summary>
        /// <param name="interfaceType">Type of the service to create the proxy</param>
        /// <returns>Dynamic proxy instance, use dynamic keyword to dynamically invoke methods to the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<DynamicProxy> CreateDynamicProxyAsync(Type interfaceType)
        {
            if (!interfaceType.GetTypeInfo().IsInterface)
                throw new ArgumentException("The type of the dynamic proxy should be an interface.");
            await InitTransportAsync().ConfigureAwait(false);
            ServiceDescriptor descriptor;
            if (!Descriptors.Items.Contains(interfaceType.FullName))
            {
                if (UseServerDescriptor)
                    throw new NotImplementedException("The server doesn't have an implementation for this Type.");
                else
                {
                    descriptor = ServiceDescriptor.GetDescriptor(interfaceType);
                    Descriptors.Items.Add(descriptor);
                }
            }
            else
                descriptor = Descriptors.Items[interfaceType.FullName];
            return new DynamicProxy(this, descriptor);
        }
        /// <summary>
        /// Creates a client proxy using this client.
        /// </summary>
        /// <typeparam name="T">Type of proxy</typeparam>
        /// <returns>Client proxy object instance to invoke methods to the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> CreateProxyAsync<T>() where T : RPCProxy
        {
            var typeInterfaces = typeof(T).GetInterfaces();
            var typeInterface = typeInterfaces.FirstOrDefault(i => i != typeof(IDisposable) && i != typeof(IEnumerable<>) && i != typeof(IDictionary<,>));
            if (typeInterface == null)
                throw new ArgumentException("The type of the proxy should implement a service interface.");
            var rpcClient = Activator.CreateInstance<T>();
            await InitTransportAsync().ConfigureAwait(false);
            ServiceDescriptor descriptor;
            if (!Descriptors.Items.Contains(typeInterface.FullName))
            {
                if (UseServerDescriptor)
                    throw new NotImplementedException("The server doesn't have an implementation for this Type.");
                else
                {
                    descriptor = ServiceDescriptor.GetDescriptor(typeInterface);
                    Descriptors.Items.Add(descriptor);
                }
            }
            else
                descriptor = Descriptors.Items[typeInterface.FullName];
            rpcClient.SetClient(this, descriptor);
            return rpcClient;
        }
        #endregion

        #region Create Proxy
        /// <summary>
        /// Creates a dynamic object to act as client proxy
        /// </summary>
        /// <typeparam name="T">Type of the service to create the proxy</typeparam>
        /// <returns>Dynamic proxy instance, use dynamic keyword to dynamically invoke methods to the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DynamicProxy CreateDynamicProxy<T>()
            => CreateDynamicProxy(typeof(T));
        /// <summary>
        /// Creates a dynamic object to act as client proxy
        /// </summary>
        /// <param name="interfaceType">Type of the service to create the proxy</param>
        /// <returns>Dynamic proxy instance, use dynamic keyword to dynamically invoke methods to the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DynamicProxy CreateDynamicProxy(Type interfaceType)
        {
            if (!interfaceType.GetTypeInfo().IsInterface)
                throw new ArgumentException("The type of the dynamic proxy should be an interface.");
            InitTransport();
            ServiceDescriptor descriptor;
            if (!Descriptors.Items.Contains(interfaceType.FullName))
            {
                if (UseServerDescriptor)
                    throw new NotImplementedException("The server doesn't have an implementation for this Type.");
                else
                {
                    descriptor = ServiceDescriptor.GetDescriptor(interfaceType);
                    Descriptors.Items.Add(descriptor);
                }
            }
            else
                descriptor = Descriptors.Items[interfaceType.FullName];
            return new DynamicProxy(this, descriptor);
        }
        /// <summary>
        /// Creates a client proxy using this client.
        /// </summary>
        /// <typeparam name="T">Type of proxy</typeparam>
        /// <returns>Client proxy object instance to invoke methods to the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CreateProxy<T>() where T : RPCProxy
        {
            var typeInterfaces = typeof(T).GetInterfaces();
            var typeInterface = typeInterfaces.FirstOrDefault(i => i != typeof(IDisposable) && i != typeof(IEnumerable<>) && i != typeof(IDictionary<,>));
            if (typeInterface == null)
                throw new ArgumentException("The type of the proxy should implement a service interface.");
            var rpcClient = Activator.CreateInstance<T>();
            InitTransport();
            ServiceDescriptor descriptor;
            if (!Descriptors.Items.Contains(typeInterface.FullName))
            {
                if (UseServerDescriptor)
                    throw new NotImplementedException("The server doesn't have an implementation for this Type.");
                else
                {
                    descriptor = ServiceDescriptor.GetDescriptor(typeInterface);
                    Descriptors.Items.Add(descriptor);
                }
            }
            else
                descriptor = Descriptors.Items[typeInterface.FullName];
            rpcClient.SetClient(this, descriptor);
            return rpcClient;
        }
        #endregion

        #region ServerInvoke Async
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Server method name</param>
        /// <param name="args">Server method arguments</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> ServerInvokeAsync<T>(string serviceName, string method, params object[] args)
        {
            return (T)await ServerInvokeAsync(serviceName, method, args).ConfigureAwait(false);
        }
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Server method name</param>
        /// <param name="args">Server method arguments</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<object> ServerInvokeAsync(string serviceName, string method, params object[] args)
        {
            using (Watch.Create(null, $"RPC Invoke: {serviceName}.{method}"))
            {
                var request = CreateRequest(serviceName, method, args);
                if (Transport.Descriptors == null)
                    Transport.Descriptors = await GetDescriptorsAsync().ConfigureAwait(false);
                var response = await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
                ReferencePool<RPCRequestMessage>.Shared.Store(request);
                if (response == null)
                    throw new Exception("RPC Response is null.");
                if (response.Succeed)
                    return response.ReturnValue;
                else
                    throw response.Exception.GetException();
            }
        }
        #endregion

        #region ServerInvoke
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Server method name</param>
        /// <param name="args">Server method arguments</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ServerInvoke<T>(string serviceName, string method, params object[] args)
        {
            return (T)ServerInvoke(serviceName, method, args);
        }
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Server method name</param>
        /// <param name="args">Server method arguments</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object ServerInvoke(string serviceName, string method, params object[] args)
        {
            using (Watch.Create(null, $"RPC Invoke: {serviceName}.{method}"))
            {
                var request = CreateRequest(serviceName, method, args);
                if (Transport.Descriptors == null)
                    Transport.Descriptors = GetDescriptors();
                var response = Transport.InvokeMethod(request);
                ReferencePool<RPCRequestMessage>.Shared.Store(request);
                if (response == null)
                    throw new Exception("RPC Response is null.");
                if (response.Succeed)
                    return response.ReturnValue;
                else
                    throw response.Exception.GetException();
            }
        }
        #endregion


        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        async Task InitTransportAsync()
        {
            if (!transportInit)
            {
                await Transport.InitAsync().ConfigureAwait(false);
                transportInit = true;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InitTransport()
        {
            if (!transportInit)
            {
                Transport.Init();
                transportInit = true;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        async Task<ServiceDescriptorCollection> GetDescriptorsAsync()
        {
            if (UseServerDescriptor && serverDescriptors == null)
                serverDescriptors = await Transport.GetDescriptorsAsync().ConfigureAwait(false);
            lock (this)
            {
                if (descriptors == null)
                {
                    descriptors = new ServiceDescriptorCollection();
                    if (UseServerDescriptor)
                        descriptors.Items.AddRange(serverDescriptors.Items);
                }
            }
            return descriptors;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ServiceDescriptorCollection GetDescriptors()
        {
            if (UseServerDescriptor && serverDescriptors == null)
                serverDescriptors = Transport.GetDescriptors();
            lock (this)
            {
                if (descriptors == null)
                {
                    descriptors = new ServiceDescriptorCollection();
                    if (UseServerDescriptor)
                        descriptors.Items.AddRange(serverDescriptors.Items);
                }
            }
            return descriptors;
        }
        /// <summary>
        /// Handles OnEventReceived event of the transport
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event args</param>
        void Transport_OnEventReceived(object sender, EventDataEventArgs e)
        {
            if (Descriptors.Items.Contains(e.ServiceName))
            {
                var descriptor = Descriptors.Items[e.ServiceName];
                if (descriptor.Events?.Any(ev => ev.Name == e.EventName) == true)
                    OnEventReceived?.Invoke(sender, e);
            }
        }
        /// <summary>
        /// Creates a new RPC Request message
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Method name to call</param>
        /// <param name="args">Method arguments</param>
        /// <returns>RPC Request message object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        RPCRequestMessage CreateRequest(string serviceName, string method, params object[] args)
        {
            var mDesc = MethodDescriptorCache.GetOrAdd((serviceName, method , args?.Select(a => a?.GetType() ?? Type.Missing.GetType()).Join(",")), kTuple =>
             {
                 if (Descriptors.Items.Contains(kTuple.Item1))
                 {
                     var descriptor = Descriptors.Items[kTuple.Item1];
                     var methods = descriptor.Methods.Where(m => m.Name == kTuple.Item2 && m.Parameters?.Count == args.Length).ToList();
                     if (methods.Count == 1)
                         return methods[0];
                     else
                     {
                         return methods.FirstOrDefault(m =>
                         {
                             for (var i = 0; i < m.Parameters.Count; i++)
                             {
                                 var p1 = m.Parameters[i].Parameter;
                                 var v2 = args[i];

                                 if (v2 != null && !p1.ParameterType.GetTypeInfo().IsAssignableFrom(v2.GetType().GetTypeInfo()))
                                     return false;
                                 if (v2 == null && p1.ParameterType.GetTypeInfo().IsValueType)
                                     if (!p1.ParameterType.GetTypeInfo().IsGenericType || p1.ParameterType.GetGenericTypeDefinition() != typeof(Nullable<>))
                                         return false;
                             }
                             return true;
                         });
                     }
                 }
                 return null;
             });
            if (mDesc == null)
                throw new MissingMemberException($"The method '{method}' with {args?.Length} arguments on service {serviceName} can't be found in the service description.");

            var parameters = new List<object>(args);
            if (args.Length == 0 && mDesc.Parameters?.Count == 1)
                parameters.Add(null);

            RPCRequestMessage rqMessage = ReferencePool<RPCRequestMessage>.Shared.New();
            rqMessage.MessageId = Factory.NewGuid();
            rqMessage.ServiceName = serviceName;
            rqMessage.Parameters = parameters;
            if (RequestByDescriptorIndex)
            {
                rqMessage.MethodId = null;
                rqMessage.MethodIndex = mDesc.Index;
                return rqMessage;
            }
            else
            {
                rqMessage.MethodId = mDesc.Id;
                rqMessage.MethodIndex = 0;
                return rqMessage;
            }
        }
        #endregion

        /// <summary>
        /// Dispose all resource
        /// </summary>
        public void Dispose()
        {
            transport?.Dispose();
            transport = null;
            descriptors = null;
            serverDescriptors = null;
            transportInit = false;
        }
    }
}
