﻿/*
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
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;
using TWCore.Net.RPC.Descriptors;

namespace TWCore.Net.RPC.Client
{
    /// <inheritdoc />
    /// <summary>
    /// Dynamic RPC client proxy object
    /// </summary>
    public class DynamicProxy : DynamicObject
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly RPCClient _client;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly ServiceDescriptor _descriptor;

        /// <inheritdoc />
        /// <summary>
        /// Dynamic RPC client proxy object
        /// </summary>
        /// <param name="client">RPC client object</param>
        /// <param name="descriptor">Service descriptor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal DynamicProxy(RPCClient client, ServiceDescriptor descriptor)
        {
            _client = client;
            _descriptor = descriptor;
        }

        /// <inheritdoc />
        /// <summary>
        /// Provides the implementation for operations that invoke a member.
        /// </summary>
        /// <param name="binder">Provides information about the dynamic operation.</param>
        /// <param name="args">The arguments that are passed to the object member during the invoke operation.</param>
        /// <param name="result">The result of the member invocation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var binderName = binder.Name;
            if (binderName.Length > 5 && binderName.EndsWith("Async", StringComparison.Ordinal))
            {
                var name = binderName.Substring(0, binderName.Length - 5);
                result = _client.ServerInvokeAsync(_descriptor.Name, name, args);
            }
            else
                result = _client.ServerInvoke(_descriptor.Name, binderName, args);
            return true;
        }
    }
}
