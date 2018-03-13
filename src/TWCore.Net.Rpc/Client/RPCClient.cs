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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status;
using TWCore.Net.RPC.Client.Transports;
using TWCore.Net.RPC.Descriptors;
// ReSharper disable InconsistentNaming
// ReSharper disable InconsistentlySynchronizedField
// ReSharper disable InheritdocConsiderUsage
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Net.RPC.Client
{
    /// <inheritdoc cref="IRPCClient" />
    /// <summary>
    /// RPC standard client
    /// </summary>
    public class RPCClient : IRPCClient, IDisposable
    {
        private static readonly object[] _emptyArgs = new object[0];
        private static readonly object[] _nullItemArgs = { null };
        private readonly NonBlocking.ConcurrentDictionary<(string ServiceName, string Method, Type[] Types), MethodDescriptor> _methodDescriptorCache = new NonBlocking.ConcurrentDictionary<(string, string, Type[]), MethodDescriptor>(new MethodDescriptionEqualityComparer());
        private ITransportClient _transport;
        private bool _transportInit;
        private ServiceDescriptorCollection _serverDescriptors;
        private ServiceDescriptorCollection _descriptors;

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
            get { return _transport; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_transport != null)
                    _transport.OnEventReceived -= Transport_OnEventReceived;
                _transport = value;
                if (_transport == null) return;
                _transport.OnEventReceived += Transport_OnEventReceived;
                InitTransportAsync().WaitAsync();
            }
        }
        /// <summary>
        /// Fires when an event from the RPC server fires
        /// </summary>
        public event EventHandler<EventDataEventArgs> OnEventReceived;
        /// <summary>
        /// Gets or Sets if the RPC client needs to get the server description for checks
        /// </summary>
        [StatusProperty]
        public bool UseServerDescriptor { get; set; } = false;
        #endregion

        #region Nested Types
        class MethodDescriptionEqualityComparer : IEqualityComparer<(string ServiceName, string Method, Type[] Types)>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals((string ServiceName, string Method, Type[] Types) x, (string ServiceName, string Method, Type[] Types) y)
            {
                if (x.ServiceName != y.ServiceName) return false;
                if (x.Method != y.Method) return false;
                if (x.Types == null && y.Types == null) return true;
                if (x.Types == null) return false;
                if (y.Types == null) return false;
                if (x.Types.Length != y.Types.Length) return false;
                for(var i = 0; i < x.Types.Length; i++)
                    if (x.Types[i] != y.Types[i]) return false;
                return true;
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetHashCode((string ServiceName, string Method, Type[] Types) obj)
                => (obj.ServiceName?.GetHashCode() ?? 11) + (obj.Method?.GetHashCode() ?? 17) + obj.Types?.Length * 19 ?? 19;
        }
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

        #region Create Proxy
        /// <summary>
        /// Creates a dynamic object to act as client proxy
        /// </summary>
        /// <typeparam name="T">Type of the service to create the proxy</typeparam>
        /// <returns>Dynamic proxy instance, use dynamic keyword to dynamically invoke methods to the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<DynamicProxy> CreateDynamicProxyAsync<T>()
            => CreateDynamicProxyAsync(typeof(T));
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
            if (Descriptors.Items.TryGetValue(interfaceType.FullName, out var descriptor))
                return new DynamicProxy(this, descriptor);
            if (!UseServerDescriptor)
            {
                descriptor = ServiceDescriptor.GetDescriptor(interfaceType);
                Descriptors.Add(descriptor);
            }
            else
                throw new NotImplementedException("The server doesn't have an implementation for this Type.");
            return new DynamicProxy(this, descriptor);
        }
        /// <summary>
        /// Creates a client proxy using this client.
        /// </summary>
        /// <typeparam name="T">Type of proxy</typeparam>
        /// <returns>Client proxy object instance to invoke methods to the server</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> CreateProxyAsync<T>() where T : RPCProxy, new()
        {
            var typeInterfaces = typeof(T).GetInterfaces();
            var typeInterface = typeInterfaces.FirstOrDefault(i => i != typeof(IDisposable) && i != typeof(IEnumerable<>) && i != typeof(IDictionary<,>));
            if (typeInterface == null)
                throw new ArgumentException("The type of the proxy should implement a service interface.");
            var rpcClient = new T();
            await InitTransportAsync().ConfigureAwait(false);
            if (!Descriptors.Items.TryGetValue(typeInterface.FullName, out var descriptor))
            {
                if (!UseServerDescriptor)
                {
                    descriptor = ServiceDescriptor.GetDescriptor(typeInterface);
                    Descriptors.Add(descriptor);
                }
                else
                    throw new NotImplementedException("The server doesn't have an implementation for this Type.");
            }
            rpcClient.SetClient(this, descriptor.Name);
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
        public Task<T> ServerInvokeAsync<T>(string serviceName, string method, params object[] args)
            => ServerInvokeAsync(serviceName, method, args).ContinueWith(tsk => (T)tsk.Result,
                CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
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
            var request = CreateRequest(serviceName, method, args, null, false);
            if (Transport.Descriptors == null)
                Transport.Descriptors = await GetDescriptorsAsync().ConfigureAwait(false);
            var response = await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            if (response == null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return response.ReturnValue;
        }

        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Server method name</param>
        /// <param name="args">Server method arguments</param>
        /// <param name="cancellationToken">Cancellation Token instance</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> ServerInvokeAsync<T>(string serviceName, string method, object[] args, CancellationToken cancellationToken)
            => ServerInvokeAsync(serviceName, method, args, cancellationToken).ContinueWith(tsk => (T)tsk.Result,
                CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Server method name</param>
        /// <param name="args">Server method arguments</param>
        /// <param name="cancellationToken">Cancellation Token instance</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<object> ServerInvokeAsync(string serviceName, string method, object[] args, CancellationToken cancellationToken)
        {
            var request = CreateRequest(serviceName, method, args, null, true);
            if (Transport.Descriptors == null)
                Transport.Descriptors = await GetDescriptorsAsync().ConfigureAwait(false);
            var response = await Transport.InvokeMethodAsync(request, cancellationToken).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            if (response == null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return response.ReturnValue;
        }
        #endregion

        #region Alternative ServerInvoke Async
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TReturn> ServerInvokeNoArgumentsAsync<TReturn>(string serviceName, string method, CancellationToken? cancellationToken = null)
        {
            var request = CreateRequest(serviceName, method, null, null, cancellationToken != null);
            if (Transport.Descriptors == null)
                Transport.Descriptors = await GetDescriptorsAsync().ConfigureAwait(false);
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            if (response == null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }

        private static readonly ObjectPool<(object[], Type[]), Args1Allocator> ServiceInvokeArgs1Pool = new ObjectPool<(object[], Type[]), Args1Allocator>();
        private static readonly ObjectPool<(object[], Type[]), Args2Allocator> ServiceInvokeArgs2Pool = new ObjectPool<(object[], Type[]), Args2Allocator>();
        private static readonly ObjectPool<(object[], Type[]), Args3Allocator> ServiceInvokeArgs3Pool = new ObjectPool<(object[], Type[]), Args3Allocator>();
        private static readonly ObjectPool<(object[], Type[]), Args4Allocator> ServiceInvokeArgs4Pool = new ObjectPool<(object[], Type[]), Args4Allocator>();
        
        private struct Args1Allocator : IPoolObjectLifecycle<(object[], Type[])>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public (object[], Type[]) New() => (new object[1], new Type[1]);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset((object[], Type[]) value)
            {
            }
        }
        private struct Args2Allocator : IPoolObjectLifecycle<(object[], Type[])>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public (object[], Type[]) New() => (new object[2], new Type[2]);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset((object[], Type[]) value)
            {
            }
        }
        private struct Args3Allocator : IPoolObjectLifecycle<(object[], Type[])>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public (object[], Type[]) New() => (new object[3], new Type[3]);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset((object[], Type[]) value)
            {
            }
        }
        private struct Args4Allocator : IPoolObjectLifecycle<(object[], Type[])>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public (object[], Type[]) New() => (new object[4], new Type[4]);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset((object[], Type[]) value)
            {
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TReturn> ServerInvokeAsync<TArg1, TReturn>(string serviceName, string method, TArg1 arg1, CancellationToken? cancellationToken = null)
        {
            var arrayArgs = ServiceInvokeArgs1Pool.New();
            arrayArgs.Item1[0] = arg1;
            arrayArgs.Item2[0] = typeof(TArg1);
            var request = CreateRequest(serviceName, method, arrayArgs.Item1, arrayArgs.Item2, cancellationToken != null);
            if (Transport.Descriptors == null)
                Transport.Descriptors = await GetDescriptorsAsync().ConfigureAwait(false);
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            arrayArgs.Item1[0] = null;
            ServiceInvokeArgs1Pool.Store(arrayArgs);
            if (response == null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TReturn> ServerInvokeAsync<TArg1, TArg2, TReturn>(string serviceName, string method, TArg1 arg1, TArg2 arg2, CancellationToken? cancellationToken = null)
        {
            var arrayArgs = ServiceInvokeArgs2Pool.New();
            arrayArgs.Item1[0] = arg1;
            arrayArgs.Item1[1] = arg2;
            arrayArgs.Item2[0] = typeof(TArg1);
            arrayArgs.Item2[1] = typeof(TArg2);
            var request = CreateRequest(serviceName, method, arrayArgs.Item1, arrayArgs.Item2, cancellationToken != null);
            if (Transport.Descriptors == null)
                Transport.Descriptors = await GetDescriptorsAsync().ConfigureAwait(false);
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            arrayArgs.Item1[0] = null;
            arrayArgs.Item1[1] = null;
            ServiceInvokeArgs2Pool.Store(arrayArgs);
            if (response == null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TReturn> ServerInvokeAsync<TArg1, TArg2, TArg3, TReturn>(string serviceName, string method, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken? cancellationToken = null)
        {
            var arrayArgs = ServiceInvokeArgs3Pool.New();
            arrayArgs.Item1[0] = arg1;
            arrayArgs.Item1[1] = arg2;
            arrayArgs.Item1[2] = arg3;
            arrayArgs.Item2[0] = typeof(TArg1);
            arrayArgs.Item2[1] = typeof(TArg2);
            arrayArgs.Item2[2] = typeof(TArg3);
            var request = CreateRequest(serviceName, method, arrayArgs.Item1, arrayArgs.Item2, cancellationToken != null);
            if (Transport.Descriptors == null)
                Transport.Descriptors = await GetDescriptorsAsync().ConfigureAwait(false);
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            arrayArgs.Item1[0] = null;
            arrayArgs.Item1[1] = null;
            arrayArgs.Item1[2] = null;
            ServiceInvokeArgs3Pool.Store(arrayArgs);
            if (response == null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TReturn> ServerInvokeAsync<TArg1, TArg2, TArg3, TArg4, TReturn>(string serviceName, string method, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, CancellationToken? cancellationToken = null)
        {
            var arrayArgs = ServiceInvokeArgs4Pool.New();
            arrayArgs.Item1[0] = arg1;
            arrayArgs.Item1[1] = arg2;
            arrayArgs.Item1[2] = arg3;
            arrayArgs.Item1[3] = arg4;
            arrayArgs.Item2[0] = typeof(TArg1);
            arrayArgs.Item2[1] = typeof(TArg2);
            arrayArgs.Item2[2] = typeof(TArg3);
            arrayArgs.Item2[3] = typeof(TArg4);
            var request = CreateRequest(serviceName, method, arrayArgs.Item1, arrayArgs.Item2, cancellationToken != null);
            if (Transport.Descriptors == null)
                Transport.Descriptors = await GetDescriptorsAsync().ConfigureAwait(false);
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            arrayArgs.Item1[0] = null;
            arrayArgs.Item1[1] = null;
            arrayArgs.Item1[2] = null;
            arrayArgs.Item1[3] = null;
            ServiceInvokeArgs4Pool.Store(arrayArgs);
            if (response == null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task InitTransportAsync()
        {
            if (_transportInit) return;
            await Transport.InitAsync().ConfigureAwait(false);
            _transportInit = true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<ServiceDescriptorCollection> GetDescriptorsAsync()
        {
            if (_descriptors != null) return _descriptors;
            if (_serverDescriptors == null && UseServerDescriptor)
                _serverDescriptors = await Transport.GetDescriptorsAsync().ConfigureAwait(false);
            lock (this)
            {
                _descriptors = new ServiceDescriptorCollection();
                if (UseServerDescriptor)
                    _descriptors.Combine(_serverDescriptors);
                return _descriptors;
            }
        }
        /// <summary>
        /// Handles OnEventReceived event of the transport
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event args</param>
        private void Transport_OnEventReceived(object sender, EventDataEventArgs e)
        {
            if (!Descriptors.Items.TryGetValue(e.ServiceName, out var descriptor)) return;
            if (descriptor.Events.ContainsKey(e.EventName))
                OnEventReceived?.Invoke(sender, e);
        }
        /// <summary>
        /// Creates a new RPC Request message
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Method name to call</param>
        /// <param name="args">Method arguments</param>
        /// <param name="types">Types array</param>
        /// <param name="useCancellationToken">Use cancellation token</param>
        /// <returns>RPC Request message object instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private RPCRequestMessage CreateRequest(string serviceName, string method, object[] args, Type[] types, bool useCancellationToken)
        {
            if (types == null && args != null)
            {
                types = new Type[args.Length];
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i] != null)
                        types[i] = args[i].GetType();
                }
            }
            var mDesc = _methodDescriptorCache.GetOrAdd((serviceName, method, types), key =>
            {
                if (!Descriptors.Items.TryGetValue(key.ServiceName, out var descriptor)) return null;
                var iArgs = args ?? _emptyArgs;

                var methods = descriptor.Methods.Values.Where(m => m.Name == key.Method && (m.Parameters?.Length ?? 0) == iArgs.Length).ToArray();
                switch (methods.Length)
                {
                    case 0:
                        return null;
                    case 1:
                        return methods[0];
                    default:
                        return methods.FirstOrDefault(m =>
                        {
                            for (var i = 0; i < m.Parameters.Length; i++)
                            {
                                var p1 = m.Parameters[i].Parameter;
                                var v2 = iArgs[i];
                                var p1TypeInfo = p1.ParameterType.GetTypeInfo();
                                if (v2 != null && !p1TypeInfo.IsAssignableFrom(v2.GetType().GetTypeInfo()))
                                    return false;
                                if (v2 != null || !p1TypeInfo.IsValueType) continue;
                                if (!p1TypeInfo.IsGenericType || p1.ParameterType.GetGenericTypeDefinition() != typeof(Nullable<>))
                                    return false;
                            }
                            return true;
                        });
                }
            });
            if (mDesc == null)
                throw new MissingMemberException($"The method '{method}' with {args?.Length} arguments on service {serviceName} can't be found in the service description.");

            if (mDesc.Parameters?.Length == 1 && (args == null || args.Length == 0))
                args = _nullItemArgs;

            return RPCRequestMessage.Retrieve(mDesc.Id, args, useCancellationToken);
        }
        #endregion

        /// <summary>
        /// Dispose all resource
        /// </summary>
        public void Dispose()
        {
            _transport?.Dispose();
            _transport = null;
            _descriptors = null;
            _serverDescriptors = null;
            _transportInit = false;
        }
    }
}
