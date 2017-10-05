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
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TWCore.Net.RPC.Attributes;
using TWCore.Serialization;
// ReSharper disable RedundantAssignment
// ReSharper disable CheckNamespace
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore.Net.RPC.Server.Transports
{
    public class MessagingTransportServer : ITransportServer
    {
        public string Name => throw new NotImplementedException();
        public bool EnableGetDescriptors { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISerializer Serializer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public RPCTransportCounters Counters => throw new NotImplementedException();

        public event EventHandler<ServerDescriptorsEventArgs> OnGetDescriptorsRequest;
        public event EventHandler<MethodEventArgs> OnMethodCall;
        public event EventHandler<ClientConnectEventArgs> OnClientConnect;


        public void FireEvent(RPCEventAttribute eventAttribute, Guid clientId, string serviceName, string eventName, object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public Task StartListenerAsync()
        {
            throw new NotImplementedException();
        }

        public Task StopListenerAsync()
        {
            throw new NotImplementedException();
        }
    }
}
