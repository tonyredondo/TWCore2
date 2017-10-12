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
using TWCore.Net.RPC.Descriptors;
// ReSharper disable InconsistentNaming
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace TWCore.Net.RPC.Client
{
    /// <summary>
    /// Define a RPC Client
    /// </summary>
    public interface IRPCClient
    {
        /// <summary>
        /// Service descriptor collection
        /// </summary>
        ServiceDescriptorCollection Descriptors { get; }
        /// <summary>
        /// Transport client object
        /// </summary>
        ITransportClient Transport { get; set; }
        /// <summary>
        /// Fires when an event from the RPC server fires
        /// </summary>
        event EventHandler<EventDataEventArgs> OnEventReceived;
        /// <summary>
        /// Creates a dynamic object to act as client proxy
        /// </summary>
        /// <typeparam name="T">Type of the service to create the proxy</typeparam>
        /// <returns>Dynamic proxy instance, use dynamic keyword to dynamically invoke methods to the server</returns>
        DynamicProxy CreateDynamicProxy<T>();
        /// <summary>
        /// Creates a dynamic object to act as client proxy
        /// </summary>
        /// <param name="interfaceType">Type of the service to create the proxy</param>
        /// <returns>Dynamic proxy instance, use dynamic keyword to dynamically invoke methods to the server</returns>
        DynamicProxy CreateDynamicProxy(Type interfaceType);
        /// <summary>
        /// Creates a client proxy using this client.
        /// </summary>
        /// <typeparam name="T">Type of proxy</typeparam>
        /// <returns>Client proxy object instance to invoke methods to the server</returns>
        T CreateProxy<T>() where T : RPCProxy;
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <typeparam name="T">Response object type</typeparam>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Server method name</param>
        /// <param name="args">Server method arguments</param>
        /// <returns>Server method return value</returns>
        T ServerInvoke<T>(string serviceName, string method, params object[] args);
        /// <summary>
        /// Invokes a Server RPC method
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="method">Server method name</param>
        /// <param name="args">Server method arguments</param>
        /// <returns>Server method return value</returns>
        object ServerInvoke(string serviceName, string method, params object[] args);
    }
}
