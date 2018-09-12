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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace TWCore.Net.RPC.Client
{
    /// <summary>
    /// RPC Proxy base class
    /// </summary>
    public abstract class RPCProxy
    {
	    private RPCClient _client;
	    private string _serviceName;
	    private readonly Dictionary<string, FieldInfo> _events;
        private readonly ConcurrentDictionary<string, string> _memberNames = new ConcurrentDictionary<string, string>();

        #region .ctor
        /// <summary>
        /// RPC Proxy base class
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected RPCProxy()
        {
            _events = GetType().GetRuntimeFields().ToDictionary(k => k.Name, v => v);
        }
        #endregion

	    /// <summary>
	    /// Sets the RPC client to the proxy
	    /// </summary>
	    /// <param name="client">RPCClient object instance</param>
	    /// <param name="serviceName">Service name</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetClient(RPCClient client, string serviceName)
        {
            Ensure.ArgumentNotNull(client, "RPC Client can't be null.");
            Ensure.ArgumentNotNull(serviceName, "ServiceName can't be null.");
            _serviceName = serviceName;
            _client = client;
            _client.OnEventReceived += Client_OnEventReceived;
            Core.Status.AttachChild(_client, this);
        }

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Client_OnEventReceived(object sender, EventDataEventArgs e)
        {
	        if (e.ServiceName != _serviceName || !_events.TryGetValue(e.EventName, out var value) ||
	            !(value.GetValue(this) is MulticastDelegate evHandler)) return;
	        foreach (var handler in evHandler.GetInvocationList())
		        handler.DynamicInvoke(this, e.EventArgs);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetMemberName(string memberName)
            => _memberNames.GetOrAdd(memberName, key => key?.EndsWith("Async") == true && key.Length > 5 ? key.Substring(0, key.Length - 5) : key);
        #endregion


        #region Invoke Generic
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected T Invoke<T>([CallerMemberName]string memberName = "") 
            => _client.ServerInvokeAsync<T>(_serviceName, memberName).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected T Invoke<T>(object arg1, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync<T>(_serviceName, memberName, arg1).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected T Invoke<T>(object arg1, object arg2, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync<T>(_serviceName, memberName, arg1, arg2).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected T Invoke<T>(object arg1, object arg2, object arg3, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync<T>(_serviceName, memberName, arg1, arg2, arg3).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected T Invoke<T>(object arg1, object arg2, object arg3, object arg4, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync<T>(_serviceName, memberName, arg1, arg2, arg3, arg4).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="arg5">Argument 5</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected T Invoke<T>(object arg1, object arg2, object arg3, object arg4, object arg5, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync<T>(_serviceName, memberName, arg1, arg2, arg3, arg4, arg5).WaitAndResults();
        #endregion

        #region Invoke 
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected object Invoke([CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync(_serviceName, memberName).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected object Invoke(object arg1, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync(_serviceName, memberName, arg1).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected object Invoke(object arg1, object arg2, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync(_serviceName, memberName, arg1, arg2).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected object Invoke(object arg1, object arg2, object arg3, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync(_serviceName, memberName, arg1, arg2, arg3).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected object Invoke(object arg1, object arg2, object arg3, object arg4, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync(_serviceName, memberName, arg1, arg2, arg3, arg4).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="arg5">Argument 5</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync(_serviceName, memberName, arg1, arg2, arg3, arg4, arg5).WaitAndResults();
        #endregion



        #region InvokeAsync Generic
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsync<T>([CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync<T>(_serviceName, memberName);
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsync<T>(object arg1, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync<T>(_serviceName, memberName, arg1);
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsync<T>(object arg1, object arg2, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync<T>(_serviceName, memberName, arg1, arg2);
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsync<T>(object arg1, object arg2, object arg3, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync<T>(_serviceName, memberName, arg1, arg2, arg3);
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsync<T>(object arg1, object arg2, object arg3, object arg4, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync<T>(_serviceName, memberName, arg1, arg2, arg3, arg4);
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="arg5">Argument 5</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsync<T>(object arg1, object arg2, object arg3, object arg4, object arg5, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync<T>(_serviceName, memberName, arg1, arg2, arg3, arg4, arg5);
        #endregion

        #region InvokeAsync 
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsync([CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync(_serviceName, memberName);
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsync(object arg1, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync(_serviceName, memberName, arg1);
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsync(object arg1, object arg2, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync(_serviceName, memberName, arg1, arg2);
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsync(object arg1, object arg2, object arg3, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync(_serviceName, memberName, arg1, arg2, arg3);
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsync(object arg1, object arg2, object arg3, object arg4, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync(_serviceName, memberName, arg1, arg2, arg3, arg4);
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="arg5">Argument 5</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsync(object arg1, object arg2, object arg3, object arg4, object arg5, [CallerMemberName]string memberName = "") 
			=> _client.ServerInvokeAsync(_serviceName, memberName, arg1, arg2, arg3, arg4, arg5);
        #endregion

        #region Alternative Invokes
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TResult InvokeArgs<TResult>([CallerMemberName]string memberName = "")
            => _client.ServerInvokeNoArgumentsAsync<TResult>(_serviceName, memberName).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> InvokeArgsAsync<TResult>([CallerMemberName]string memberName = "")
            => _client.ServerInvokeNoArgumentsAsync<TResult>(_serviceName, memberName);


        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <typeparam name="TArg1">Argument 1 type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TResult InvokeArgs<TArg1, TResult>(TArg1 arg1, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<TArg1, TResult>(_serviceName, memberName, arg1).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <typeparam name="TArg1">Argument 1 type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> InvokeArgsAsync<TArg1, TResult>(TArg1 arg1, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<TArg1,TResult>(_serviceName, memberName, arg1);


        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <typeparam name="TArg1">Argument 1 type</typeparam>
        /// <typeparam name="TArg2">Argument 2 type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TResult InvokeArgs<TArg1, TArg2, TResult>(TArg1 arg1, TArg2 arg2, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<TArg1, TArg2, TResult>(_serviceName, memberName, arg1, arg2).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <typeparam name="TArg1">Argument 1 type</typeparam>
        /// <typeparam name="TArg2">Argument 2 type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> InvokeArgsAsync<TArg1, TArg2, TResult>(TArg1 arg1, TArg2 arg2, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<TArg1, TArg2, TResult>(_serviceName, memberName, arg1, arg2);


        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <typeparam name="TArg1">Argument 1 type</typeparam>
        /// <typeparam name="TArg2">Argument 2 type</typeparam>
        /// <typeparam name="TArg3">Argument 3 type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TResult InvokeArgs<TArg1, TArg2, TArg3, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<TArg1, TArg2, TArg3, TResult>(_serviceName, memberName, arg1, arg2, arg3).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <typeparam name="TArg1">Argument 1 type</typeparam>
        /// <typeparam name="TArg2">Argument 2 type</typeparam>
        /// <typeparam name="TArg3">Argument 3 type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> InvokeArgsAsync<TArg1, TArg2, TArg3, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<TArg1, TArg2, TArg3, TResult>(_serviceName, memberName, arg1, arg2, arg3);


        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <typeparam name="TArg1">Argument 1 type</typeparam>
        /// <typeparam name="TArg2">Argument 2 type</typeparam>
        /// <typeparam name="TArg3">Argument 3 type</typeparam>
        /// <typeparam name="TArg4">Argument 4 type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TResult InvokeArgs<TArg1, TArg2, TArg3, TArg4, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<TArg1, TArg2, TArg3, TArg4, TResult>(_serviceName, memberName, arg1, arg2, arg3, arg4).WaitAndResults();
        /// <summary>
        /// Proxy an invocation to a rpc method
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <typeparam name="TArg1">Argument 1 type</typeparam>
        /// <typeparam name="TArg2">Argument 2 type</typeparam>
        /// <typeparam name="TArg3">Argument 3 type</typeparam>
        /// <typeparam name="TArg4">Argument 4 type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> InvokeArgsAsync<TArg1, TArg2, TArg3, TArg4, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<TArg1, TArg2, TArg3, TArg4, TResult>(_serviceName, memberName, arg1, arg2, arg3, arg4);
        #endregion



        #region InvokeAsAsync Generic
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsAsync<T>([CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<T>(_serviceName, GetMemberName(memberName));
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsAsync<T>(object arg1, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<T>(_serviceName, GetMemberName(memberName), arg1);
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsAsync<T>(object arg1, object arg2, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<T>(_serviceName, GetMemberName(memberName), arg1, arg2);
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsAsync<T>(object arg1, object arg2, object arg3, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<T>(_serviceName, GetMemberName(memberName), arg1, arg2, arg3);
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsAsync<T>(object arg1, object arg2, object arg3, object arg4, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<T>(_serviceName, GetMemberName(memberName), arg1, arg2, arg3, arg4);
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="arg5">Argument 5</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsAsync<T>(object arg1, object arg2, object arg3, object arg4, object arg5, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<T>(_serviceName, GetMemberName(memberName), arg1, arg2, arg3, arg4, arg5);
        #endregion

        #region InvokeAsAsync 
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsAsync([CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync(_serviceName, GetMemberName(memberName));
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsAsync(object arg1, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync(_serviceName, GetMemberName(memberName), arg1);
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsAsync(object arg1, object arg2, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync(_serviceName, GetMemberName(memberName), arg1, arg2);
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsAsync(object arg1, object arg2, object arg3, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync(_serviceName, GetMemberName(memberName), arg1, arg2, arg3);
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsAsync(object arg1, object arg2, object arg3, object arg4, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync(_serviceName, GetMemberName(memberName), arg1, arg2, arg3, arg4);
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="arg5">Argument 5</param>
        /// <param name="memberName">Method name to call</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsAsync(object arg1, object arg2, object arg3, object arg4, object arg5, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync(_serviceName, GetMemberName(memberName), arg1, arg2, arg3, arg4, arg5);
        #endregion

        #region Alternative InvokesWithAsync
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> InvokeArgsAsAsync<TResult>([CallerMemberName]string memberName = "")
            => _client.ServerInvokeNoArgumentsAsync<TResult>(_serviceName, GetMemberName(memberName));
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <typeparam name="TArg1">Type of Argument 1</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> InvokeArgsAsAsync<TArg1, TResult>(TArg1 arg1, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<TArg1, TResult>(_serviceName, GetMemberName(memberName), arg1);
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <typeparam name="TArg1">Type of Argument 1</typeparam>
        /// <typeparam name="TArg2">Type of Argument 2</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> InvokeArgsAsAsync<TArg1, TArg2, TResult>(TArg1 arg1, TArg2 arg2, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<TArg1, TArg2, TResult>(_serviceName, GetMemberName(memberName), arg1, arg2);
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <typeparam name="TArg1">Type of Argument 1</typeparam>
        /// <typeparam name="TArg2">Type of Argument 2</typeparam>
        /// <typeparam name="TArg3">Type of Argument 3</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> InvokeArgsAsAsync<TArg1, TArg2, TArg3, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<TArg1, TArg2, TArg3, TResult>(_serviceName, GetMemberName(memberName), arg1, arg2, arg3);
        /// <summary>
        /// Proxy an invocation to a rpc method as async
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <typeparam name="TArg1">Type of Argument 1</typeparam>
        /// <typeparam name="TArg2">Type of Argument 2</typeparam>
        /// <typeparam name="TArg3">Type of Argument 3</typeparam>
        /// <typeparam name="TArg4">Type of Argument 4</typeparam>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <param name="arg3">Argument 3</param>
        /// <param name="arg4">Argument 4</param>
        /// <param name="memberName">Method name</param>
        /// <returns>Return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> InvokeArgsAsAsync<TArg1, TArg2, TArg3, TArg4, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, [CallerMemberName]string memberName = "")
            => _client.ServerInvokeAsync<TArg1, TArg2, TArg3, TArg4, TResult>(_serviceName, GetMemberName(memberName), arg1, arg2, arg3, arg4);
        #endregion



        /// <summary>
        /// Dispose all resource
        /// </summary>
        public void Dispose()
        {
            try
            {
                _client?.Dispose();
            }
	        catch
	        {
		        // ignored
	        }
	        _client = null;
            Core.Status.DeAttachObject(this);
        }
    }
}
