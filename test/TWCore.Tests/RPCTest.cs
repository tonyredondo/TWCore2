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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TWCore.Net.RPC;
using TWCore.Net.RPC.Attributes;
using TWCore.Net.RPC.Client;
using TWCore.Net.RPC.Client.Transports.Default;
using TWCore.Net.RPC.Server;
using TWCore.Net.RPC.Server.Transports.Default;
using TWCore.Serialization.NSerializer;
using TWCore.Serialization.PWSerializer;
using TWCore.Services;
using TWCore.Threading;
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
    /// <inheritdoc />
    public class RpcTest : ContainerParameterServiceAsync
    {
        public RpcTest() : base("rpctest", "RPC Test") { }
        protected override async Task OnHandlerAsync(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting RPC TEST");

            var serializer = new NBinarySerializer();
            var service = new MyService();

            Core.Log.InfoBasic("Setting RPC Server");
            var rpcServer = new RPCServer(new DefaultTransportServer(20050, serializer));
            rpcServer.AddService(service);
            await rpcServer.StartAsync().ConfigureAwait(false);

            Core.Log.InfoBasic("Setting RPC Client");
            var rpcClient = new RPCClient(new DefaultTransportClient("127.0.0.1", 20050, 1, serializer));

            var sw = Stopwatch.StartNew();

            //IHello test
            Core.Log.InfoBasic("IHello test");
            dynamic hClient = await rpcClient.CreateDynamicProxyAsync<IHello>().ConfigureAwait(false);
            var rtest = (string)hClient.SayHi("MyName");
            using (var watch = Watch.Create("IHello Time - SayHi | TestAsync"))
            {
                for (var i = 0; i < 10000; i++)
                {
                    var rHClient = (string) hClient.SayHi("MyName");
                }

                for (var i = 0; i < 10000; i++)
                {
                    var rHClient = await ((Task<object>) hClient.TestAsync()).ConfigureAwait(false);
                }

                Core.Log.InfoBasic("Per Item: {0}", watch.GlobalElapsedMilliseconds / 10000);
            }


            //IMyService test
            Core.Log.InfoBasic("IMyService test");
            dynamic dClient = await rpcClient.CreateDynamicProxyAsync<IMyService>().ConfigureAwait(false);
            using (var watch = Watch.Create("IMyService Time - GetAllAsync"))
            {
                for (var i = 0; i < 10000; i++)
                {
                    var aLst = await ((Task<object>) dClient.GetAllAsync()).ConfigureAwait(false);
                }
                Core.Log.InfoBasic("Per Item: {0}", watch.GlobalElapsedMilliseconds / 5000);
            }

            //Proxy class test
            Core.Log.InfoBasic("Proxy class test");
            var client = await rpcClient.CreateProxyAsync<MyServiceProxy>().ConfigureAwait(false);

            await client.NDate(Core.Now).ConfigureAwait(false);

            var enumResult = await client.TestEnum("Valor", Option.Option1).ConfigureAwait(false);

            using (var watch = Watch.Create("Proxy class Time - GetAllAsync"))
            {
                for (var i = 0; i < 10000; i++)
                {
                    var resp = await client.GetAllAsync().ConfigureAwait(false);
                }
                Core.Log.InfoBasic("Per Item: {0}", watch.GlobalElapsedMilliseconds / 5000);
            }

            using (var watch = Watch.Create("Parallel GetAllAsync Time"))
            {
                var rAwait = await Enumerable.Range(0, 100)
                    .Select(i => client.GetAllAsync())
                    .ConfigureAwait(false);
                Core.Log.InfoBasic("Per Item: {0}", watch.GlobalElapsedMilliseconds / 100);
            }

            //Event test
            Core.Log.InfoBasic("Event test");
            using (var watch = Watch.Create("Event Test - AddSimplePersona"))
            {
                client.OnAddSimplePersona += (s, e) =>
                {
                    //Core.Log.Warning("On Add SimplePersona was fired!!!");
                };
                for (var i = 0; i < 10000; i++)
                {
                    client.AddSimplePersona(new SimplePerson {Lastname = "Test", Firstname = "Test"});
                }
                Core.Log.InfoBasic("Per Item: {0}", watch.GlobalElapsedMilliseconds / 5000);
            }

            var sTime = sw.Elapsed;
            Console.ReadLine();
            Core.Log.Warning("All Rpc Requests on: {0}", sTime);
            rpcClient.Dispose();
            await rpcServer.StopAsync().ConfigureAwait(false);
            Core.Log.InfoBasic("Test End.");
        }
    }


    #region RPC Server Test
    [Serializable]
    public class SimplePerson
    {
        public Guid PersonId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public bool Enabled { get; set; }
    }
    public enum Option
    {
        Option1,
        Option2
    }
    public interface IMyService
    {
        event EventHandler OnAddSimplePersona;
        SimplePerson GetSimplePersona(string name, string apellido);
        SimplePerson GetSimplePersona(Guid simplePersonaId);
        List<SimplePerson> GetAll();
        bool AddSimplePersona(SimplePerson simplePersona);
        Task<bool> IsEnabled();
        Task<Option> TestEnum(string name, Option option);
        Task<bool> NDate(DateTime? value);
    }

    public interface IHello
    {
        string SayHi(string name);
        string Test();
    }

    public class MyService : IMyService, IHello
    {
        List<SimplePerson> _tmpSPerson;

        [RPCEvent(RPCMessageScope.Global)]
        public event EventHandler OnAddSimplePersona;

        #region SimplePersons
        List<SimplePerson> SimplePersonas = new List<SimplePerson>
        {
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() }
        };
        #endregion

        public List<SimplePerson> GetAll()
            => _tmpSPerson ?? (_tmpSPerson = SimplePersonas.Concat(SimplePersonas).Concat(SimplePersonas).Concat(SimplePersonas).ToList());

        public SimplePerson GetSimplePersona(Guid simplePersonaId)
            => SimplePersonas.FirstOrDefault((p, pId) => p.PersonId == pId, simplePersonaId);

        public SimplePerson GetSimplePersona(string name, string apellido)
            => SimplePersonas.FirstOrDefault((p, mName, mApellido) => p.Firstname == mName && p.Lastname == mApellido, name, apellido);

        public string SayHi(string name)
            => $"Hi {name}!";

        public string Test() => string.Empty;

        public bool AddSimplePersona(SimplePerson simplePersona)
        {
            SimplePersonas.Add(simplePersona);
            OnAddSimplePersona?.Invoke(this, new EventArgs<SimplePerson>(simplePersona));
            return true;
        }
        public Task<bool> IsEnabled()
        {
            return TaskHelper.CompleteTrue;
        }
        public Task<Option> TestEnum(string name, Option option)
        {
            if (option == Option.Option1)
                return Task.FromResult(Option.Option2);
            return Task.FromResult(Option.Option1);
        }
        public Task<bool> NDate(DateTime? value)
        {
            return Task.FromResult(true);
        }
    }
#pragma warning disable 67
    public class MyServiceProxy : RPCProxy, IMyService
    {
        public event EventHandler OnAddSimplePersona;
        public bool AddSimplePersona(SimplePerson simplePersona) => Invoke<SimplePerson, bool>(simplePersona);
        public List<SimplePerson> GetAll() => Invoke<List<SimplePerson>>();
        public SimplePerson GetSimplePersona(Guid simplePersonaId) => Invoke<Guid, SimplePerson>(simplePersonaId);
        public SimplePerson GetSimplePersona(string name, string apellido) => Invoke<string, string, SimplePerson>(name, apellido);

        public Task<List<SimplePerson>> GetAllAsync() => InvokeAsAsync<List<SimplePerson>>();
        public Task<bool> IsEnabled() => InvokeAsync<bool>();
        public Task<Option> TestEnum(string name, Option option) => InvokeAsync<string, Option, Option>(name, option);
        public Task<bool> NDate(DateTime? value) => InvokeAsync<DateTime?, bool>(value);
    }

#endregion
}