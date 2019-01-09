﻿/*
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Counters;
using TWCore.Diagnostics.Status;
using TWCore.Net.RPC.Attributes;
using TWCore.Net.RPC.Descriptors;
using TWCore.Net.RPC.Server.Transports;
using TWCore.Reflection;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Local

namespace TWCore.Net.RPC.Server
{
    /// <inheritdoc />
    /// <summary>
    /// RPC Standard server
    /// </summary>
    [StatusName("RPC Server")]
    public class RPCServer : IRPCServer
    {
        private readonly List<ServiceItem> _serviceInstances = new List<ServiceItem>();
        private readonly Dictionary<Guid, (ServiceItem Service, MethodDescriptor Method)> _methods = new Dictionary<Guid, (ServiceItem Service, MethodDescriptor Method)>(100);
        private string _counterCategory = "RPC Server";
        private CounterLevel _counterLevel = CounterLevel.Framework;
        private CounterKind _counterKind = CounterKind.RPC;
        private ITransportServer _transport;

        /// <summary>
        /// Connection cancellation token
        /// </summary>
        [ThreadStatic]
        public static CancellationToken ConnectionCancellationToken;

        #region Properties
        /// <inheritdoc />
        /// <summary>
        /// Service descriptor collection
        /// </summary>
        public ServiceDescriptorCollection Descriptors { get; private set; }
        /// <inheritdoc />
        /// <summary>
        /// Server transport
        /// </summary>
        public ITransportServer Transport
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _transport; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (!Running)
                    _transport = value;
                else
                    throw new Exception("The RPC server is started, you can't modify the transport, please Stop first");
            }
        }
        /// <inheritdoc />
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
                collection.Add(nameof(Running), Running, Running ? StatusItemValueStatus.Ok : StatusItemValueStatus.Error);
                collection.Add("Instances Count", _serviceInstances.Count);
                foreach (var sItem in _serviceInstances)
                    Core.Status.AttachChild(sItem, this);
                Core.Status.AttachChild(Transport, this);
            });
        }
        /// <inheritdoc />
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
        /// <inheritdoc />
        /// <summary>
        /// Starts the RPC server listener
        /// </summary>
        /// <returns>Task with the result of the start</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task StartAsync()
        {
            Core.Log.LibVerbose("Starting RPC Server for the following types: {0}", _serviceInstances.Select(i => i.Descriptor.Name).Join(","));
            Transport.OnMethodCallAsync += OnMethodCallAsync;
            Transport.OnResponseSent += OnResponseSent;
            Transport.OnGetDescriptorsRequest += OnGetDescriptorsRequest;
            Transport.OnClientConnect += OnClientConnect;
            Descriptors = new ServiceDescriptorCollection();
            _serviceInstances.Each(v =>
            {
                Descriptors.Add(v.Descriptor);
                v.BindToServiceType();
            });
            await Transport.StartListenerAsync().ConfigureAwait(false);
            Running = true;
            Core.Log.LibVerbose("RPC Server started.");
        }
        /// <inheritdoc />
        /// <summary>
        /// Stops the RPC server listener
        /// </summary>
        /// <returns>Task with the result of the stop</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task StopAsync()
        {
            Core.Log.LibVerbose("Stopping RPC Server for the following types: {0}", _serviceInstances.Select(i => i.Descriptor.Name).Join(","));
            await Transport.StopListenerAsync().ConfigureAwait(false);
            _serviceInstances.Each(v => v.UnbindToServiceType());
            Transport.OnMethodCallAsync -= OnMethodCallAsync;
            Transport.OnResponseSent -= OnResponseSent;
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
            if (!serviceInterfaceType.IsInterface)
                throw new ArgumentException("The type of the service should be an interface", "serviceInterfaceType");
            Core.Log.LibVerbose("Adding service.");
            var sItem = new ServiceItem(this, serviceInterfaceType, serviceInstance);
            _serviceInstances.Add(sItem);
			foreach(var mDesc in sItem.Descriptor.Methods)
				_methods.Add(mDesc.Key, (sItem, mDesc.Value));
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
            if (interfaces is null) return;
            foreach (var iface in interfaces)
                AddService(iface, serviceInstance);
        }
        /// <summary>
        /// Sets the Counter info
        /// </summary>
        /// <param name="category">Counter category</param>
        /// <param name="level">Counter level</param>
        /// <param name="kind">Counter kind</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCounterInfo(string category, CounterLevel level, CounterKind kind)
        {
            _counterCategory = category;
            _counterLevel = level;
            _counterKind = kind;
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task OnMethodCallAsync(object sender, MethodEventArgs e)
        {
			if (_methods.TryGetValue(e.Request.MethodId, out var desc))
            {
                e.Response = await desc.Service.ProcessRequestAsync(e.Request, e.ClientId, desc.Method, e.CancellationToken).ConfigureAwait(false);
                return;
            }
            var responseMessage = RPCResponseMessage.Retrieve(e.Request);
            responseMessage.Exception = new SerializableException(new NotImplementedException("The MethodId = {0} was not found on the service.".ApplyFormat(e.Request.MethodId)));
            e.Response = responseMessage;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnResponseSent(object sender, RPCResponseMessage responseMessage)
        {
            if (!(responseMessage is null))
                RPCResponseMessage.Store(responseMessage);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnGetDescriptorsRequest(object sender, ServerDescriptorsEventArgs e)
        {
            e.Descriptors = Descriptors;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnClientConnect(object sender, ClientConnectEventArgs e)
        {
            foreach(var item in _serviceInstances)
                item.FireClientConnection(e.ClientId);
        }
        #endregion

        #region Nested ServiceItem Class
        [StatusName("Service Instance")]
        private class ServiceItem
        {
            public static readonly ConcurrentDictionary<Type, FastPropertyInfo> TaskResultsProperties = new ConcurrentDictionary<Type, FastPropertyInfo>();
            private readonly RPCServer _server;
            private readonly Dictionary<int, Guid> _threadClientId = new Dictionary<int, Guid>();

            [StatusProperty("Type")]
            public Type ServiceType { get; }
            [StatusProperty("Instance"), StatusReference]
            public object ServiceInstance { get; }
            public ServiceDescriptor Descriptor { get; }
            public readonly Dictionary<EventInfo, EventHandler> ServiceEventHandlers = new Dictionary<EventInfo, EventHandler>();
            public MethodInfo OnClientConnectMethod;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ServiceItem(RPCServer server, Type serviceType, object serviceInstance)
            {
                _server = server;
                ServiceInstance = serviceInstance;
                ServiceType = serviceType;
                Descriptor = ServiceDescriptor.GetDescriptor(ServiceType);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void BindToServiceType()
            {
                #region OnClientConnect Method
                OnClientConnectMethod = ServiceType.GetRuntimeMethods().FirstOrDefault(m => m.GetCustomAttribute(typeof(RPCOnClientConnectAttribute)) != null);
                if (OnClientConnectMethod != null)
                    Core.Log.LibDebug("Binding OnClientConnectMethod in the service {0}", Descriptor.Name);
                #endregion

                #region Events
                if (Descriptor?.Events?.Any() != true) return;
                
                foreach (var evDesc in Descriptor.Events.Values)
                {
                    if (evDesc.Event is null) continue;
                        
                    var eventAttribute = (RPCEventAttribute)ServiceInstance?.GetType().GetRuntimeEvent(evDesc.Name)?.GetCustomAttribute(typeof(RPCEventAttribute)) ??
                                         new RPCEventAttribute(RPCMessageScope.Session);
                    var evHandler = new EventHandler((s, e) =>
                    {
                        if (_server.Transport is null) return;
                        Guid clientId;
                        lock (_threadClientId) _threadClientId.TryGetValue(Environment.CurrentManagedThreadId, out clientId);
                        _server.Transport.FireEventAsync(eventAttribute, clientId, Descriptor.Name, evDesc.Event.Name, s, e).WaitAsync();
                    });
                    evDesc.Event.AddEventHandler(ServiceInstance, evHandler);
                    ServiceEventHandlers[evDesc.Event] = evHandler;
                    Core.Log.LibDebug("Binding Event: {0} on service {1}", evDesc.Name, Descriptor.Name);
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
                if (ServiceEventHandlers?.Any() != true) return;
                foreach (var ev in ServiceEventHandlers)
                {
                    Core.Log.LibDebug("Unbinding Event: {0} on service {1}", ev.Key.Name, Descriptor.Name);
                    ev.Key.RemoveEventHandler(ServiceInstance, ev.Value);
                }
                ServiceEventHandlers.Clear();
                #endregion
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void FireClientConnection(Guid clientId)
            {
                try
                {
                    if (OnClientConnectMethod is null) return;
                    var tId = Environment.CurrentManagedThreadId;
                    lock (_threadClientId) _threadClientId[tId] = clientId;
                    var pTers = OnClientConnectMethod.GetParameters();
                    switch (pTers.Length)
                    {
                        case 0:
                            OnClientConnectMethod?.Invoke(ServiceInstance, null);
                            break;
                        case 1:
                            OnClientConnectMethod?.Invoke(ServiceInstance, new object[] { clientId });
                            break;
                    }
                    lock (_threadClientId) _threadClientId.Remove(tId);
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
			public async ValueTask<RPCResponseMessage> ProcessRequestAsync(RPCRequestMessage request, Guid clientId, MethodDescriptor mDesc, CancellationToken cancellationToken)
            {
                var response = RPCResponseMessage.Retrieve(request);
                try
                {
					lock (_threadClientId) _threadClientId[Environment.CurrentManagedThreadId] = clientId;
                    ConnectionCancellationToken = cancellationToken;
                    var initTime = Stopwatch.GetTimestamp();
                    var results = mDesc.Method(ServiceInstance, request.Parameters);
                    if (mDesc.ReturnIsTask)
                    {
                        var resultTask = (Task)results;
                        await resultTask.ConfigureAwait(false);
                        if (mDesc.ReturnTaskResult != null)
                            response.ReturnValue = mDesc.ReturnTaskResult.GetValue(resultTask);
                        else
                            response.ReturnValue = null;
                    }
                    else
                        response.ReturnValue = results;
                    var execTime = (Stopwatch.GetTimestamp() - initTime) * 1000d / Stopwatch.Frequency;
                    var counter = Core.Counters.GetDoubleCounter(_server._counterCategory, Descriptor.Name + "\\" + mDesc.Name, CounterType.Average, _server._counterLevel, _server._counterKind, CounterUnit.Milliseconds);
                    counter.Add(execTime);
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

