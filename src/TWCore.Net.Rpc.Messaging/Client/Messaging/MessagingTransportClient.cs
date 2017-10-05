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

#pragma warning disable 0067
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TWCore.Net.RPC.Descriptors;
using TWCore.Serialization;
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TWCore.Net.RPC.Client.Transports
{
    public class MessagingTransportClient : ITransportClient
    {
        public string Name => throw new NotImplementedException();
        public ISerializer Serializer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ServiceDescriptorCollection Descriptors { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public RPCTransportCounters Counters => throw new NotImplementedException();

        public event EventHandler<EventDataEventArgs> OnEventReceived;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ServiceDescriptorCollection GetDescriptors()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceDescriptorCollection> GetDescriptorsAsync()
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            throw new NotImplementedException();
        }

        public Task InitAsync()
        {
            throw new NotImplementedException();
        }

        public RPCResponseMessage InvokeMethod(RPCRequestMessage messageRQ)
        {
            throw new NotImplementedException();
        }

        public Task<RPCResponseMessage> InvokeMethodAsync(RPCRequestMessage messageRQ)
        {
            throw new NotImplementedException();
        }
    }
}
