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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Net.RPC.Descriptors;

namespace TWCore.Net.RPC.Client
{
    /// <summary>
    /// RPC Proxy base class
    /// </summary>
    public abstract class RPCProxy
    {
        /// <summary>
        /// Gets the current RPC Client object
        /// </summary>
        RPCClient _client;
        ServiceDescriptor _descriptor;

        /// <summary>
        /// Sets the RPC client to the proxy
        /// </summary>
        /// <param name="client">RPCClient object instance</param>
        /// <param name="descriptor">Service descriptor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetClient(RPCClient client, ServiceDescriptor descriptor)
        {
            Ensure.ArgumentNotNull(client, "RPC Client can't be null.");
            Ensure.ArgumentNotNull(descriptor, "ServiceDescriptor can't be null.");
            _descriptor = descriptor;
            _client = client;
            _client.OnEventReceived += Client_OnEventReceived;

            Core.Status.Attach(collection =>
            {
                collection.Add("Client", _client);
                Core.Status.AttachChild(_client, this);
            });
        }
        /// <summary>
        /// Handles the OnEventReceived event and fires a local event on the proxy
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Object event args</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Client_OnEventReceived(object sender, EventDataEventArgs e)
        {
            if (e.ServiceName == _descriptor.Name)
            {
                if (this.GetType().GetRuntimeFields().FirstOrDefault(f => f.Name == e.EventName)?.GetValue(this) is MulticastDelegate evHandler)
                    foreach (var handler in evHandler.GetInvocationList())
                        handler.DynamicInvoke(this, e.EventArgs);
            }
        }

        #region ServerInvoke
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="method">Server method name</param>
        /// <param name="args">Server method arguments</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> ServerInvokeAsync<T>(string method, params object[] args) => _client.ServerInvokeAsync<T>(_descriptor.Name, method, args);
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="method">Server method name</param>
        /// <param name="args">Server method arguments</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> ServerInvokeAsync(string method, params object[] args) => _client.ServerInvokeAsync(_descriptor.Name, method, args);
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="method">Server method name</param>
        /// <param name="args">Server method arguments</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T ServerInvoke<T>(string method, params object[] args) => _client.ServerInvoke<T>(_descriptor.Name, method, args);
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="method">Server method name</param>
        /// <param name="args">Server method arguments</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected object ServerInvoke(string method, params object[] args) => _client.ServerInvoke(_descriptor.Name, method, args);
        #endregion

        #region Invoke Generic
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T Invoke<T>([CallerMemberName]string memberName = "") => ServerInvoke<T>(memberName);
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="arg1">Server method arguments</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T Invoke<T>(object arg1, [CallerMemberName]string memberName = "") => ServerInvoke<T>(memberName, new object[] { arg1 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T Invoke<T>(object arg1, object arg2, [CallerMemberName]string memberName = "") => ServerInvoke<T>(memberName, new object[] { arg1, arg2 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="arg3">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T Invoke<T>(object arg1, object arg2, object arg3, [CallerMemberName]string memberName = "") => ServerInvoke<T>(memberName, new object[] { arg1, arg2, arg3 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="arg3">Server method argument</param>
        /// <param name="arg4">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T Invoke<T>(object arg1, object arg2, object arg3, object arg4, [CallerMemberName]string memberName = "") => ServerInvoke<T>(memberName, new object[] { arg1, arg2, arg3, arg4 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="arg3">Server method argument</param>
        /// <param name="arg4">Server method argument</param>
        /// <param name="arg5">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T Invoke<T>(object arg1, object arg2, object arg3, object arg4, object arg5, [CallerMemberName]string memberName = "") => ServerInvoke<T>(memberName, new object[] { arg1, arg2, arg3, arg4, arg5 });
        #endregion

        #region Invoke 
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected object Invoke ([CallerMemberName]string memberName = "") => ServerInvoke(memberName);
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="arg1">Server method arguments</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected object Invoke(object arg1, [CallerMemberName]string memberName = "") => ServerInvoke(memberName, new object[] { arg1 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected object Invoke(object arg1, object arg2, [CallerMemberName]string memberName = "") => ServerInvoke(memberName, new object[] { arg1, arg2 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="arg3">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected object Invoke(object arg1, object arg2, object arg3, [CallerMemberName]string memberName = "") => ServerInvoke(memberName, new object[] { arg1, arg2, arg3 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="arg3">Server method argument</param>
        /// <param name="arg4">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected object Invoke(object arg1, object arg2, object arg3, object arg4, [CallerMemberName]string memberName = "") => ServerInvoke(memberName, new object[] { arg1, arg2, arg3, arg4 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="arg3">Server method argument</param>
        /// <param name="arg4">Server method argument</param>
        /// <param name="arg5">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5, [CallerMemberName]string memberName = "") => ServerInvoke(memberName, new object[] { arg1, arg2, arg3, arg4, arg5 });
        #endregion

        ConcurrentDictionary<string, string> _memberNames = new ConcurrentDictionary<string, string>();
        string GetMemberName(string memberName) => _memberNames.GetOrAdd(memberName, key => key?.EndsWith("Async") == true && key?.Length > 5 ? key.Substring(0, key.Length - 5) : key);

        #region InvokeAsync Generic
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsync<T>([CallerMemberName]string memberName = "") => ServerInvokeAsync<T>(GetMemberName(memberName));
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="arg1">Server method arguments</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsync<T>(object arg1, [CallerMemberName]string memberName = "") => ServerInvokeAsync<T>(GetMemberName(memberName), new object[] { arg1 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsync<T>(object arg1, object arg2, [CallerMemberName]string memberName = "") => ServerInvokeAsync<T>(GetMemberName(memberName), new object[] { arg1, arg2 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="arg3">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsync<T>(object arg1, object arg2, object arg3, [CallerMemberName]string memberName = "") => ServerInvokeAsync<T>(GetMemberName(memberName), new object[] { arg1, arg2, arg3 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="arg3">Server method argument</param>
        /// <param name="arg4">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsync<T>(object arg1, object arg2, object arg3, object arg4, [CallerMemberName]string memberName = "") => ServerInvokeAsync<T>(GetMemberName(memberName), new object[] { arg1, arg2, arg3, arg4 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="arg3">Server method argument</param>
        /// <param name="arg4">Server method argument</param>
        /// <param name="arg5">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<T> InvokeAsync<T>(object arg1, object arg2, object arg3, object arg4, object arg5, [CallerMemberName]string memberName = "") => ServerInvokeAsync<T>(GetMemberName(memberName), new object[] { arg1, arg2, arg3, arg4, arg5 });
        #endregion

        #region InvokeAsync 
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsync([CallerMemberName]string memberName = "") => ServerInvokeAsync(GetMemberName(memberName));
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="arg1">Server method arguments</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsync(object arg1, [CallerMemberName]string memberName = "") => ServerInvokeAsync(GetMemberName(memberName), new object[] { arg1 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsync(object arg1, object arg2, [CallerMemberName]string memberName = "") => ServerInvokeAsync(GetMemberName(memberName), new object[] { arg1, arg2 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="arg3">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsync(object arg1, object arg2, object arg3, [CallerMemberName]string memberName = "") => ServerInvokeAsync(GetMemberName(memberName), new object[] { arg1, arg2, arg3 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="arg3">Server method argument</param>
        /// <param name="arg4">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsync(object arg1, object arg2, object arg3, object arg4, [CallerMemberName]string memberName = "") => ServerInvokeAsync(GetMemberName(memberName), new object[] { arg1, arg2, arg3, arg4 });
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="arg1">Server method argument</param>
        /// <param name="arg2">Server method argument</param>
        /// <param name="arg3">Server method argument</param>
        /// <param name="arg4">Server method argument</param>
        /// <param name="arg5">Server method argument</param>
        /// <param name="memberName">Compiler caller member name</param>
        /// <returns>Server method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<object> InvokeAsync(object arg1, object arg2, object arg3, object arg4, object arg5, [CallerMemberName]string memberName = "") => ServerInvokeAsync(GetMemberName(memberName), new object[] { arg1, arg2, arg3, arg4, arg5 });
        #endregion

        /// <summary>
        /// Dispose all resource
        /// </summary>
        public void Dispose()
        {
            _descriptor = null;
            try {
                _client?.Dispose();
            }
            catch {}
            _client = null;
            Core.Status.DeAttachObject(this);
        }
    }
}
