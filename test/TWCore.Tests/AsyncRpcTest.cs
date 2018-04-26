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

using System.Threading.Tasks;
using TWCore.Net.RPC.Client;
using TWCore.Net.RPC.Client.Transports.Default;
using TWCore.Net.RPC.Server;
using TWCore.Net.RPC.Server.Transports.Default;
using TWCore.Serialization.NSerializer;
using TWCore.Services;
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedVariable

namespace TWCore.Tests
{
    public class AsyncRpcTest : ContainerParameterServiceAsync
    {
        public AsyncRpcTest() : base("asyncrpctest", "Async RPC Test") { }
        protected override async Task OnHandlerAsync(ParameterHandlerInfo info)
        {
            var serializer = new NBinarySerializer();
            var service = new SampleProvider();
            Core.Log.InfoBasic("Setting RPC Server");
            var rpcServer = new RPCServer(new DefaultTransportServer(20050, serializer));
            rpcServer.AddService(typeof(ISampleProvider), service);
            await rpcServer.StartAsync().ConfigureAwait(false);

            Core.Log.InfoBasic("Setting RPC Client");
            var rpcClient = new RPCClient(new DefaultTransportClient("127.0.0.1", 20050, 1, serializer));

            var hClient = await rpcClient.CreateDynamicProxyAsync<ISampleProvider>().ConfigureAwait(false);
            var client = hClient.ActAs<ISampleProvider>();

            var sampleTsk = await client.GetSampleAsync().ConfigureAwait(false);
            await client.DelayTestAsync().ConfigureAwait(false);
            var sample = client.GetSample();
            var sampleSimAsync = ((dynamic) hClient).GetSample2Async().Result;

            var pClient = await rpcClient.CreateProxyAsync<SampleProxy>().ConfigureAwait(false);

            var sampleTsk2 = await pClient.GetSampleAsync().ConfigureAwait(false);
            await pClient.DelayTestAsync().ConfigureAwait(false);
            var sample2 = pClient.GetSample();

        }


        public class Sample
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public interface ISampleProvider
        {
            Task<Sample> GetSampleAsync();
            Task DelayTestAsync();
            Sample GetSample();
            Sample GetSample2();
        }

        public class SampleProxy : RPCProxy, ISampleProvider
        {
            public Task<Sample> GetSampleAsync()
            {
                return InvokeAsync<Sample>();
            }

            public Task DelayTestAsync()
            {
                return InvokeAsync();
            }

            public Sample GetSample()
            {
                return Invoke<Sample>();
            }

            public Sample GetSample2()
            {
                return Invoke<Sample>();
            }
        }

        public class SampleProvider : ISampleProvider
        {
            /// <inheritdoc />
            public async Task<Sample> GetSampleAsync()
            {
                await Task.Delay(2000).ConfigureAwait(false);
                return new Sample
                {
                    Name = "Sample Name",
                    Value = "Sample Value"
                };
            }
            public Task DelayTestAsync()
            {
                return Task.Delay(2000);
            }
            public Sample GetSample()
            {
                return new Sample
                {
                    Name = "Sample Name",
                    Value = "Sample Value"
                };
            }

            public Sample GetSample2()
            {
                return new Sample
                {
                    Name = "Sample Name",
                    Value = "Sample Value"
                };
            }
        }
    }
}