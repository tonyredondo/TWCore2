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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    [StatusName("RPC Client")]
    public class RPCClient : IRPCClient, IDisposable
    {
        private static readonly object[] _nullItemArgs = { null };
        private readonly ConcurrentDictionary<(string ServiceName, string Method, Type[] Types), MethodDescriptor> _methodDescriptorCache = new ConcurrentDictionary<(string, string, Type[]), MethodDescriptor>(new MethodDescriptionEqualityComparer());
        private ITransportClient _transport;
        private bool _transportInit;
        private ServiceDescriptorCollection _serverDescriptors;
        private ServiceDescriptorCollection _descriptors;
        private Func<(string, string, Type[]), MethodDescriptor> _getMethodDescriptorDelegate;

        #region Properties
        /// <summary>
        /// Service descriptor collection
        /// </summary>
        public ServiceDescriptorCollection Descriptors => GetDescriptorsAsync().WaitAndResults();
        /// <summary>
        /// Transport client object
        /// </summary>
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
                if (_transport is null) return;
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
        public bool UseServerDescriptor { get; set; } = false;
        #endregion

        #region Nested Types
        private sealed class MethodDescriptionEqualityComparer : IEqualityComparer<(string ServiceName, string Method, Type[] Types)>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals((string ServiceName, string Method, Type[] Types) x, (string ServiceName, string Method, Type[] Types) y)
            {
                if (x.ServiceName != y.ServiceName) return false;
                if (x.Method != y.Method) return false;
                if (x.Types is null && y.Types is null) return true;
                if (x.Types is null) return false;
                if (y.Types is null) return false;
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
            _getMethodDescriptorDelegate = GetMethodDescriptor;
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
            if (!interfaceType.IsInterface)
                throw new ArgumentException("The type of the dynamic proxy should be an interface.");
            await InitTransportAsync().ConfigureAwait(false);
            Transport.Descriptors = Descriptors;
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
            if (typeInterface is null)
                throw new ArgumentException("The type of the proxy should implement a service interface.");
            var rpcClient = new T();
            await InitTransportAsync().ConfigureAwait(false);
            Transport.Descriptors = Descriptors;
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
        public async Task<T> ServerInvokeAsync<T>(string serviceName, string method, params object[] args)
        {
            var response = await ServerInvokeAsync(serviceName, method, args).ConfigureAwait(false);
            return (T)response;
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
            var request = CreateRequest(serviceName, method, args, null, false);
            var response = await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            if (response is null)
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
        public async Task<T> ServerInvokeAsync<T>(string serviceName, string method, object[] args, CancellationToken cancellationToken)
        {
            var response = await ServerInvokeAsync(serviceName, method, args, cancellationToken).ConfigureAwait(false);
            return (T)response;
        }

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
            var response = await Transport.InvokeMethodAsync(request, cancellationToken).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            if (response is null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return response.ReturnValue;
        }
        #endregion

        #region Alternative ServerInvoke Async
        private static readonly ObjectPool<Tuple<object[], Type[]>, Args1Allocator> ServiceInvokeArgs1Pool = new ObjectPool<Tuple<object[], Type[]>, Args1Allocator>();
        private static readonly ObjectPool<Tuple<object[], Type[]>, Args2Allocator> ServiceInvokeArgs2Pool = new ObjectPool<Tuple<object[], Type[]>, Args2Allocator>();
        private static readonly ObjectPool<Tuple<object[], Type[]>, Args3Allocator> ServiceInvokeArgs3Pool = new ObjectPool<Tuple<object[], Type[]>, Args3Allocator>();
        private static readonly ObjectPool<Tuple<object[], Type[]>, Args4Allocator> ServiceInvokeArgs4Pool = new ObjectPool<Tuple<object[], Type[]>, Args4Allocator>();
        private static readonly ObjectPool<Tuple<object[], Type[]>, Args5Allocator> ServiceInvokeArgs5Pool = new ObjectPool<Tuple<object[], Type[]>, Args5Allocator>();
        
        private readonly struct Args1Allocator : IPoolObjectLifecycle<Tuple<object[], Type[]>>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Tuple<object[], Type[]> New() => Tuple.Create(new object[1], new Type[1]);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(Tuple<object[], Type[]> value)
            {
                value.Item1[0] = null;
            }
            public int DropTimeFrequencyInSeconds => 120;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void DropAction(Tuple<object[], Type[]> value)
            {
            }
            public int DropMaxSizeThreshold => 10;
        }
        private readonly struct Args2Allocator : IPoolObjectLifecycle<Tuple<object[], Type[]>>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Tuple<object[], Type[]> New() => Tuple.Create(new object[2], new Type[2]);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(Tuple<object[], Type[]> value)
            {
                value.Item1[0] = null;
                value.Item1[1] = null;
            }
            public int DropTimeFrequencyInSeconds => 120;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void DropAction(Tuple<object[], Type[]> value)
            {
            }
            public int DropMaxSizeThreshold => 10;
        }
        private readonly struct Args3Allocator : IPoolObjectLifecycle<Tuple<object[], Type[]>>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Tuple<object[], Type[]> New() => Tuple.Create(new object[3], new Type[3]);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(Tuple<object[], Type[]> value)
            {
                value.Item1[0] = null;
                value.Item1[1] = null;
                value.Item1[2] = null;
            }
            public int DropTimeFrequencyInSeconds => 120;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void DropAction(Tuple<object[], Type[]> value)
            {
            }
            public int DropMaxSizeThreshold => 10;
        }
        private readonly struct Args4Allocator : IPoolObjectLifecycle<Tuple<object[], Type[]>>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Tuple<object[], Type[]> New() => Tuple.Create(new object[4], new Type[4]);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(Tuple<object[], Type[]> value)
            {
                value.Item1[0] = null;
                value.Item1[1] = null;
                value.Item1[2] = null;
                value.Item1[3] = null;
            }
            public int DropTimeFrequencyInSeconds => 120;
            public void DropAction(Tuple<object[], Type[]> value)
            {
            }
            public int DropMaxSizeThreshold => 10;
        }
        private readonly struct Args5Allocator : IPoolObjectLifecycle<Tuple<object[], Type[]>>
        {
            public int InitialSize => 1;
            public PoolResetMode ResetMode => PoolResetMode.AfterUse;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Tuple<object[], Type[]> New() => Tuple.Create(new object[5], new Type[5]);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(Tuple<object[], Type[]> value)
            {
                value.Item1[0] = null;
                value.Item1[1] = null;
                value.Item1[2] = null;
                value.Item1[3] = null;
                value.Item1[4] = null;
            }
            public int DropTimeFrequencyInSeconds => 120;
            public void DropAction(Tuple<object[], Type[]> value)
            {
            }
            public int DropMaxSizeThreshold => 10;
        }

        #region Generic
        /// <summary>
        /// Invoke a Server RPC method with 1 argument 
        /// </summary>
        /// <typeparam name="TArg1">Argument 1 type</typeparam>
        /// <typeparam name="TReturn">Return type</typeparam>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Method name</param>
        /// <param name="arg1">Argument 1</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TReturn> ServerInvokeAsync<TArg1, TReturn>(string serviceName, string method, TArg1 arg1, CancellationToken? cancellationToken = null)
        {
            var arrayArgs = ServiceInvokeArgs1Pool.New();
            arrayArgs.Item1[0] = arg1;
            arrayArgs.Item2[0] = typeof(TArg1);
            var request = CreateRequest(serviceName, method, arrayArgs.Item1, arrayArgs.Item2, cancellationToken != null);
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            ServiceInvokeArgs1Pool.Store(arrayArgs);
            if (response is null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }
        /// <summary>
        /// Invoke a Server RPC method with 2 argument 
        /// </summary>
        /// <typeparam name="TArg1">Argument 1 type</typeparam>
        /// <typeparam name="TArg2">Argument 2 type</typeparam>
        /// <typeparam name="TReturn">Return type</typeparam>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Method name</param>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TReturn> ServerInvokeAsync<TArg1, TArg2, TReturn>(string serviceName, string method, TArg1 arg1, TArg2 arg2, CancellationToken? cancellationToken = null)
        {
            var arrayArgs = ServiceInvokeArgs2Pool.New();
            arrayArgs.Item1[0] = arg1;
            arrayArgs.Item1[1] = arg2;
            arrayArgs.Item2[0] = typeof(TArg1);
            arrayArgs.Item2[1] = typeof(TArg2);
            var request = CreateRequest(serviceName, method, arrayArgs.Item1, arrayArgs.Item2, cancellationToken != null);
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            ServiceInvokeArgs2Pool.Store(arrayArgs);
            if (response is null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }
        /// <summary>
        /// Invoke a Server RPC method with 3 argument 
        /// </summary>
        /// <typeparam name="TArg1">Argument 1 type</typeparam>
        /// <typeparam name="TArg2">Argument 2 type</typeparam>
        /// <typeparam name="TArg3">Argument 3 type</typeparam>
        /// <typeparam name="TReturn">Return type</typeparam>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Method name</param>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
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
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            ServiceInvokeArgs3Pool.Store(arrayArgs);
            if (response is null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }
        /// <summary>
        /// Invoke a Server RPC method with 4 argument 
        /// </summary>
        /// <typeparam name="TArg1">Argument 1 type</typeparam>
        /// <typeparam name="TArg2">Argument 2 type</typeparam>
        /// <typeparam name="TArg3">Argument 3 type</typeparam>
        /// <typeparam name="TArg4">Argument 4 type</typeparam>
        /// <typeparam name="TReturn">Return type</typeparam>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Method name</param>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
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
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            ServiceInvokeArgs4Pool.Store(arrayArgs);
            if (response is null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }
        /// <summary>
        /// Invoke a Server RPC method with 5 argument 
        /// </summary>
        /// <typeparam name="TArg1">Argument 1 type</typeparam>
        /// <typeparam name="TArg2">Argument 2 type</typeparam>
        /// <typeparam name="TArg3">Argument 3 type</typeparam>
        /// <typeparam name="TArg4">Argument 4 type</typeparam>
        /// <typeparam name="TArg5">Argument 5 type</typeparam>
        /// <typeparam name="TReturn">Return type</typeparam>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Method name</param>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="arg5">Argument 5</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TReturn> ServerInvokeAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TReturn>(string serviceName, string method, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, CancellationToken? cancellationToken = null)
        {
            var arrayArgs = ServiceInvokeArgs5Pool.New();
            arrayArgs.Item1[0] = arg1;
            arrayArgs.Item1[1] = arg2;
            arrayArgs.Item1[2] = arg3;
            arrayArgs.Item1[3] = arg4;
            arrayArgs.Item1[4] = arg5;
            arrayArgs.Item2[0] = typeof(TArg1);
            arrayArgs.Item2[1] = typeof(TArg2);
            arrayArgs.Item2[2] = typeof(TArg3);
            arrayArgs.Item2[3] = typeof(TArg4);
            arrayArgs.Item2[4] = typeof(TArg5);
            var request = CreateRequest(serviceName, method, arrayArgs.Item1, arrayArgs.Item2, cancellationToken != null);
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            ServiceInvokeArgs5Pool.Store(arrayArgs);
            if (response is null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }

        #endregion

        #region Object
        /// <summary>
        /// Invokes a Server RPC method with no arguments
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Server method name</param>
        /// <param name="cancellationToken">Cancellation Token instance</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TReturn> ServerInvokeObjectAsync<TReturn>(string serviceName, string method, CancellationToken? cancellationToken = null)
        {
            var request = CreateRequest(serviceName, method, null, null, cancellationToken != null);
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            if (response is null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }
        /// <summary>
        /// Invoke a Server RPC method with 1 argument 
        /// </summary>
        /// <typeparam name="TReturn">Return type</typeparam>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Method name</param>
        /// <param name="arg1">Argument 1</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TReturn> ServerInvokeObjectAsync<TReturn>(string serviceName, string method, object arg1, CancellationToken? cancellationToken = null)
        {
            var arrayArgs = ServiceInvokeArgs1Pool.New();
            arrayArgs.Item1[0] = arg1;
            var request = CreateRequest(serviceName, method, arrayArgs.Item1, null, cancellationToken != null);
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            ServiceInvokeArgs1Pool.Store(arrayArgs);
            if (response is null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }
        /// <summary>
        /// Invoke a Server RPC method with 2 argument 
        /// </summary>
        /// <typeparam name="TReturn">Return type</typeparam>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Method name</param>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TReturn> ServerInvokeObjectAsync<TReturn>(string serviceName, string method, object arg1, object arg2, CancellationToken? cancellationToken = null)
        {
            var arrayArgs = ServiceInvokeArgs2Pool.New();
            arrayArgs.Item1[0] = arg1;
            arrayArgs.Item1[1] = arg2;
            var request = CreateRequest(serviceName, method, arrayArgs.Item1, null, cancellationToken != null);
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            ServiceInvokeArgs2Pool.Store(arrayArgs);
            if (response is null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }
        /// <summary>
        /// Invoke a Server RPC method with 3 argument 
        /// </summary>
        /// <typeparam name="TReturn">Return type</typeparam>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Method name</param>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TReturn> ServerInvokeObjectAsync<TReturn>(string serviceName, string method, object arg1, object arg2, object arg3, CancellationToken? cancellationToken = null)
        {
            var arrayArgs = ServiceInvokeArgs3Pool.New();
            arrayArgs.Item1[0] = arg1;
            arrayArgs.Item1[1] = arg2;
            arrayArgs.Item1[2] = arg3;
            var request = CreateRequest(serviceName, method, arrayArgs.Item1, null, cancellationToken != null);
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            ServiceInvokeArgs3Pool.Store(arrayArgs);
            if (response is null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }
        /// <summary>
        /// Invoke a Server RPC method with 4 argument 
        /// </summary>
        /// <typeparam name="TReturn">Return type</typeparam>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Method name</param>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TReturn> ServerInvokeObjectAsync<TReturn>(string serviceName, string method, object arg1, object arg2, object arg3, object arg4, CancellationToken? cancellationToken = null)
        {
            var arrayArgs = ServiceInvokeArgs4Pool.New();
            arrayArgs.Item1[0] = arg1;
            arrayArgs.Item1[1] = arg2;
            arrayArgs.Item1[2] = arg3;
            arrayArgs.Item1[3] = arg4;
            var request = CreateRequest(serviceName, method, arrayArgs.Item1, null, cancellationToken != null);
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            ServiceInvokeArgs4Pool.Store(arrayArgs);
            if (response is null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }
        /// <summary>
        /// Invoke a Server RPC method with 5 argument 
        /// </summary>
        /// <typeparam name="TReturn">Return type</typeparam>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Method name</param>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="arg5">Argument 5</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TReturn> ServerInvokeObjectAsync<TReturn>(string serviceName, string method, object arg1, object arg2, object arg3, object arg4, object arg5, CancellationToken? cancellationToken = null)
        {
            var arrayArgs = ServiceInvokeArgs5Pool.New();
            arrayArgs.Item1[0] = arg1;
            arrayArgs.Item1[1] = arg2;
            arrayArgs.Item1[2] = arg3;
            arrayArgs.Item1[3] = arg4;
            arrayArgs.Item1[4] = arg5;
            var request = CreateRequest(serviceName, method, arrayArgs.Item1, null, cancellationToken != null);
            var response = cancellationToken.HasValue ?
                await Transport.InvokeMethodAsync(request, cancellationToken.Value).ConfigureAwait(false) :
                await Transport.InvokeMethodAsync(request).ConfigureAwait(false);
            RPCRequestMessage.Store(request);
            ServiceInvokeArgs5Pool.Store(arrayArgs);
            if (response is null)
                throw new Exception("RPC Response is null.");
            if (response.Exception != null)
                throw response.Exception.GetException();
            return (TReturn)response.ReturnValue;
        }
        #endregion

        /// <summary>
        /// Get Method Descriptor
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Server method name</param>
        /// <param name="args">Server method arguments</param>
        /// <returns>Method descriptor</returns>
        public MethodDescriptor GetMethodDescriptor(string serviceName, string method, object[] args)
        {
            Type[] types = null;
            if (args != null)
            {
                types = new Type[args.Length];
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i] != null)
                        types[i] = args[i].GetType();
                }
            }
            return _methodDescriptorCache.GetOrAdd((serviceName, method, types), _getMethodDescriptorDelegate);
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
        private async ValueTask<ServiceDescriptorCollection> GetDescriptorsAsync()
        {
            if (_descriptors != null) return _descriptors;
            if (_serverDescriptors is null && UseServerDescriptor)
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
            if (types is null && args != null)
            {
                types = new Type[args.Length];
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i] != null)
                        types[i] = args[i].GetType();
                }
            }
            var mDesc = _methodDescriptorCache.GetOrAdd((serviceName, method, types), _getMethodDescriptorDelegate);
            if (mDesc is null)
                throw new MissingMemberException($"The method '{method}' with {args?.Length} arguments on service {serviceName} can't be found in the service description.");

            if (mDesc.Parameters?.Length == 1 && (args is null || args.Length == 0))
                args = _nullItemArgs;

            return RPCRequestMessage.Retrieve(mDesc.Id, args, useCancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MethodDescriptor GetMethodDescriptor((string ServiceName, string Method, Type[] Types) key)
        {
            if (!Descriptors.Items.TryGetValue(key.ServiceName, out var descriptor)) return null;
            var lstMethods = new List<MethodDescriptor>();
            var types = key.Types ?? Array.Empty<Type>();
            foreach (var method in descriptor.Methods.Values)
            {
                if (method.Name == key.Method && (method.Parameters?.Length ?? 0) == types.Length)
                    lstMethods.Add(method);
            }
            var mCount = lstMethods.Count;
            if (mCount == 0)
                return null;
            else if (mCount == 1)
                return lstMethods[0];
            else
            {
                foreach (var method in lstMethods)
                {
                    var found = true;
                    for (var i = 0; i < method.Parameters.Length; i++)
                    {
                        var p1 = method.Parameters[i].Parameter;
                        var t2 = types[i];
                        if (t2 != null && !p1.ParameterType.IsAssignableFrom(t2))
                        {
                            found = false;
                            break;
                        }
                        if (t2 != null || !p1.ParameterType.IsValueType) continue;
                        if (!p1.ParameterType.IsGenericType || p1.ParameterType.GetGenericTypeDefinition() != typeof(Nullable<>))
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found)
                        return method;
                }
                return null;
            }
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
