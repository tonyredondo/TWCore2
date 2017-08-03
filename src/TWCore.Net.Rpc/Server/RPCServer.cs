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
using TWCore.Net.RPC.Attributes;
using TWCore.Net.RPC.Descriptors;

namespace TWCore.Net.RPC.Server
{
    /// <summary>
    /// RPC Standard server
    /// </summary>
    public class RPCServer : IRPCServer
    {
        readonly Dictionary<string, ServiceItem> ServicesInstances = new Dictionary<string, ServiceItem>(100);
        ServiceItem _singleServiceInstance = null;
        ITransportServer transport = null;

        #region Properties
        /// <summary>
        /// Service descriptor collection
        /// </summary>
        public ServiceDescriptorCollection Descriptors { get; private set; }
        /// <summary>
        /// Server transport
        /// </summary>
        public ITransportServer Transport
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return transport; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (!Running)
                    transport = value;
                else
                    throw new Exception("The RPC server is started, you can't modify the transport, please Stop first");
            }
        }
        /// <summary>
        /// true if the RPC server is running; otherwise, false.
        /// </summary>
        public bool Running { get; private set; }
        #endregion

        #region .ctor
        /// <summary>
        /// RPC Standard server
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RPCServer()
        {
            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(Running), Running, Running ? StatusItemValueStatus.Green : StatusItemValueStatus.Red);
                collection.Add("Service Instances Count", ServicesInstances.Count);
                foreach (var sItem in ServicesInstances)
                    Core.Status.AttachChild(sItem.Value, this);
                Core.Status.AttachChild(Transport, this);
            });
        }
        /// <summary>
        /// RPC Standard server
        /// </summary>
        /// <param name="transport">Server transport</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RPCServer(ITransportServer transport) : this()
        {
            Transport = transport;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts the RPC server listener
        /// </summary>
        /// <returns>Task with the result of the start</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task StartAsync()
        {
            Core.Log.LibVerbose("Starting RPC Server for the following types: {0}", ServicesInstances.Select(i => i.Value.Descriptor.Name).Join(","));
            Transport.OnMethodCall += OnMethodCall;
            Transport.OnGetDescriptorsRequest += OnGetDescriptorsRequest;
            Transport.OnClientConnect += OnClientConnect;
            Descriptors = new ServiceDescriptorCollection();
            ServicesInstances.Each(v =>
            {
                Descriptors.Add(v.Value.Descriptor);
                v.Value.BindToServiceType();
            });
            if (ServicesInstances.Count == 1)
                _singleServiceInstance = ServicesInstances.First().Value;
            await Transport.StartListenerAsync().ConfigureAwait(false);
            Running = true;
            Core.Log.LibVerbose("RPC Server started.");
        }
        /// <summary>
        /// Stops the RPC server listener
        /// </summary>
        /// <returns>Task with the result of the stop</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task StopAsync()
        {
            Core.Log.LibVerbose("Stopping RPC Server for the following types: {0}", ServicesInstances.Select(i => i.Value.Descriptor.Name).Join(","));
            await Transport.StopListenerAsync().ConfigureAwait(false);
            ServicesInstances.Each(v => v.Value.UnbindToServiceType());
            Transport.OnMethodCall -= OnMethodCall;
            Transport.OnGetDescriptorsRequest -= OnGetDescriptorsRequest;
            Transport.OnClientConnect -= OnClientConnect;
            Running = false;
            Core.Log.LibVerbose("RPC Server Stopped.");
        }
        /// <summary>
        /// Adds a service to the RPC Server
        /// </summary>
        /// <param name="serviceInterfaceType">Service interface type</param>
        /// <param name="serviceInstance">Service instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddService(Type serviceInterfaceType, object serviceInstance)
        {
            if (!serviceInterfaceType.GetTypeInfo().IsInterface)
                throw new ArgumentException("The type of the service should be an interface", "serviceInterfaceType");
            Core.Log.LibVerbose("Adding service.");
            var sItem = new ServiceItem(this, serviceInterfaceType, serviceInstance);
            ServicesInstances.Add(sItem.Descriptor.Name, sItem);
            Core.Log.LibVerbose("Service added.");
        }
        /// <summary>
        /// Adds a service to the RPC Server
        /// </summary>
        /// <param name="serviceInstance">Service instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddService(object serviceInstance)
        {
            var interfaces = serviceInstance?.GetType()?.GetInterfaces().Where(i => i != typeof(IDisposable) && i != typeof(IEnumerable<>) && i != typeof(IDictionary<,>));
            foreach (var iface in interfaces)
                AddService(iface, serviceInstance);
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnMethodCall(object sender, MethodEventArgs e)
        {
            if (ServicesInstances.TryGetValue(e.Request.ServiceName, out var sItem))
                e.Response = sItem.ProcessRequest(e.Request, e.ClientId);
            else
            {
                e.Response = new RPCResponseMessage(e.Request)
                {
                    Succeed = false,
                    Exception = new SerializableException(new NotImplementedException("An instance of ServiceName = {0} was not found on the service.".ApplyFormat(e.Request.ServiceName)))
                };
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnGetDescriptorsRequest(object sender, ServerDescriptorsEventArgs e)
        {
            e.Descriptors = Descriptors;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnClientConnect(object sender, ClientConnectEventArgs e)
        {
            ServicesInstances.Each(item => item.Value.FireClientConnection(e.ClientId));
        }
        #endregion

        #region Nested ServiceItem Class
        class ServiceItem
        {
            RPCServer Server;

            [StatusProperty]
            public Type ServiceType { get; set; }
            [StatusProperty, StatusReference]
            public object ServiceInstance { get; set; }
            public ServiceDescriptor Descriptor { get; set; }
            public readonly Dictionary<EventInfo, EventHandler> ServiceEventHandlers = new Dictionary<EventInfo, EventHandler>();
            readonly ConcurrentDictionary<int, Guid> ThreadClientId = new ConcurrentDictionary<int, Guid>();
            public MethodInfo OnClientConnectMethod;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ServiceItem(RPCServer server, Type serviceType, object serviceInstance)
            {
                Server = server;
                ServiceInstance = serviceInstance;
                ServiceType = serviceType;
                Descriptor = ServiceDescriptor.GetDescriptor(ServiceType);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void BindToServiceType()
            {
                #region OnClientConnect Method
                OnClientConnectMethod = ServiceType.GetRuntimeMethods().FirstOrDefault(m => m.GetCustomAttribute(typeof(RPCOnClientConnectAttribute)) != null); ;
                if (OnClientConnectMethod != null)
                    Core.Log.LibDebug("Binding OnClientConnectMethod in the service {0}", Descriptor.Name);
                #endregion

                #region Events
                if (Descriptor?.Events?.Any() == true)
                {
                    foreach (var evDesc in Descriptor.Events.Values)
                    {
                        if (evDesc.Event != null)
                        {
                            var eventAttribute = (RPCEventAttribute)ServiceInstance?.GetType().GetRuntimeEvent(evDesc.Name)?.GetCustomAttribute(typeof(RPCEventAttribute));
                            if (eventAttribute == null)
                                eventAttribute = new RPCEventAttribute(RPCMessageScope.Session);
                            var evHandler = new EventHandler((s, e) =>
                            {
                                if (Server.Transport != null)
                                {
                                    ThreadClientId.TryGetValue(Environment.CurrentManagedThreadId, out Guid clientId);
                                    Server.Transport.FireEvent(eventAttribute, clientId, Descriptor.Name, evDesc.Event.Name, s, e);
                                }
                            });
                            evDesc.Event.AddEventHandler(ServiceInstance, evHandler);
                            ServiceEventHandlers[evDesc.Event] = evHandler;
                            Core.Log.LibDebug("Binding Event: {0} on service {1}", evDesc.Name, Descriptor.Name);
                        }
                    }
                }
                #endregion
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UnbindToServiceType()
            {
                #region OnClientConnect Method
                if (OnClientConnectMethod != null)
                    Core.Log.LibDebug("Unbinding OnClientConnectMethod in the service {0}", Descriptor.Name);
                OnClientConnectMethod = null;
                #endregion

                #region Events
                if (ServiceEventHandlers?.Any() == true)
                {
                    foreach (var ev in ServiceEventHandlers)
                    {
                        Core.Log.LibDebug("Unbinding Event: {0} on service {1}", ev.Key.Name, Descriptor.Name);
                        ev.Key.RemoveEventHandler(ServiceInstance, ev.Value);
                    }
                }
                #endregion
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void FireClientConnection(Guid clientId)
            {
                Try.Do(() =>
                {
                    if (OnClientConnectMethod != null)
                    {
                        ThreadClientId.AddOrUpdate(Environment.CurrentManagedThreadId, clientId, (x, i) => clientId);
                        var pTers = OnClientConnectMethod.GetParameters();
                        if (pTers.Length == 0)
                            OnClientConnectMethod?.Invoke(ServiceInstance, null);
                        if (pTers.Length == 1)
                            OnClientConnectMethod?.Invoke(ServiceInstance, new object[] { clientId });
                        ThreadClientId.TryRemove(Environment.CurrentManagedThreadId, out var clientOldId);
                    }
                });
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public RPCResponseMessage ProcessRequest(RPCRequestMessage request, Guid clientId)
            {
                var response = new RPCResponseMessage(request);
                MethodDescriptor mDesc = null;
                try
                {
                    if (request.MethodId != null && Descriptor.Methods.TryGetValue(request.MethodId, out mDesc))
                    {
                        var tId = Environment.CurrentManagedThreadId;
                        ThreadClientId.AddOrUpdate(tId, clientId, (x, i) => clientId);
                        response.ReturnValue = mDesc.Method(ServiceInstance, request.Parameters?.ToArray());
                        ThreadClientId.TryRemove(tId, out var clientOldId);
                        response.Succeed = true;
                    }
                }
                catch (TargetInvocationException ex)
                {
                    Core.Log.Error("Error calling the Method: {0} with Parameters: [{1}], on the Service: {2}", mDesc?.Name, mDesc?.Parameters?.Select(p => p.Name + "(" + p.Type + ")").Join(","), Descriptor?.Name);
                    Core.Log.Error("Trying to call with the following parameters: {0}", request?.Parameters?.Select(s => s?.GetType().FullName ?? "(null)").Join(","));
                    Core.Log.Write(ex);
                    response.Exception = new SerializableException(ex.InnerException);
                }
                catch (Exception ex)
                {
                    Core.Log.Error("Error calling the Method: {0} with Parameters: [{1}], on the Service: {2}", mDesc?.Name, mDesc?.Parameters?.Select(p => p.Name + "(" + p.Type + ")").Join(","), Descriptor?.Name);
                    Core.Log.Error("Trying to call with the following parameters: {0}", request?.Parameters?.Select(s => s?.GetType().FullName ?? "(null)").Join(","));
                    Core.Log.Write(ex);
                    response.Exception = new SerializableException(ex);
                }
                return response;
            }
        }
        #endregion
    }
}

