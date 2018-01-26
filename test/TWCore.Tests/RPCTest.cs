using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TWCore.Net.RPC;
using TWCore.Net.RPC.Attributes;
using TWCore.Net.RPC.Client;
using TWCore.Net.RPC.Client.Transports.Default;
using TWCore.Net.RPC.Server;
using TWCore.Net.RPC.Server.Transports.Default;
using TWCore.Serialization.PWSerializer;
using TWCore.Serialization.WSerializer;
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
    /// <inheritdoc />
    public class RpcTest : ContainerParameterServiceAsync
    {
        public RpcTest() : base("rpctest", "RPC Test") { }
        protected override async Task OnHandlerAsync(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting RPC TEST");

			var serializer = new WBinarySerializer();
            var service = new MyService();

            Core.Log.InfoBasic("Setting RPC Server");
            var rpcServer = new RPCServer(new DefaultTransportServer(20050, serializer));
            rpcServer.AddService(service);
            await rpcServer.StartAsync().ConfigureAwait(false);

            Core.Log.InfoBasic("Setting RPC Client");
            var rpcClient = new RPCClient(new DefaultTransportClient("127.0.0.1", 20050, 3, serializer));

            //IHello test
            Core.Log.InfoBasic("IHello test");
            dynamic hClient = await rpcClient.CreateDynamicProxyAsync<IHello>().ConfigureAwait(false);
            var rtest = (string)hClient.SayHi("MyName");

            for (var i = 0; i < 5000; i++)
            {
                var rHClient = (string)hClient.SayHi("MyName");
            }

            for (var i = 0; i < 5000; i++)
            {
                var rHClient = await ((Task<object>)hClient.TestAsync()).ConfigureAwait(false);
            }


            //IMyService test
            Core.Log.InfoBasic("IMyService test");
            dynamic dClient = await rpcClient.CreateDynamicProxyAsync<IMyService>().ConfigureAwait(false);
            for (var i = 0; i < 5000; i++)
            {
                var aLst = await ((Task<object>)dClient.GetAllAsync()).ConfigureAwait(false);
            }

            //Proxy class test
            Core.Log.InfoBasic("Proxy class test");
            var client = await rpcClient.CreateProxyAsync<MyServiceProxy>().ConfigureAwait(false);
            for (var i = 0; i < 5000; i++)
            {
                var resp = await client.GetAllAsync().ConfigureAwait(false);
            }

            await Task.WhenAll(Enumerable.Range(0, 100).Select(i => client.GetAllAsync()).ToArray()).ConfigureAwait(false);

            //Event test
            Core.Log.InfoBasic("Event test");
            client.OnAddSimplePersona += (s, e) => Core.Log.Warning("On Add SimplePersona was fired!!!");
            for (var i = 0; i < 5000; i++)
            {
                client.AddSimplePersona(new SimplePerson { Lastname = "Test", Firstname = "Test" });
            }

            Core.Log.InfoBasic("Test End.");

            Console.ReadLine();
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
    public interface IMyService
    {
        event EventHandler OnAddSimplePersona;
        SimplePerson GetSimplePersona(string name, string apellido);
        SimplePerson GetSimplePersona(Guid simplePersonaId);
        List<SimplePerson> GetAll();
        bool AddSimplePersona(SimplePerson simplePersona);
    }

    public interface IHello
    {
        string SayHi(string name);
        string Test();
    }

    public class MyService : IMyService, IHello
    {
        List<SimplePerson> _tmpSPerson = null;

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
            => SimplePersonas.FirstOrDefault(p => p.PersonId == simplePersonaId);

        public SimplePerson GetSimplePersona(string name, string apellido)
            => SimplePersonas.FirstOrDefault(p => p.Firstname == name && p.Lastname == apellido);

        public string SayHi(string name)
            => $"Hi {name}!";

        public string Test() => string.Empty;

        public bool AddSimplePersona(SimplePerson simplePersona)
        {
            SimplePersonas.Add(simplePersona);
            OnAddSimplePersona?.Invoke(this, new EventArgs<SimplePerson>(simplePersona));
            return true;
        }
    }
#pragma warning disable 67
    public class MyServiceProxy : RPCProxy, IMyService
    {
        public event EventHandler OnAddSimplePersona;
        public bool AddSimplePersona(SimplePerson simplePersona) => Invoke<bool>(simplePersona);
        public List<SimplePerson> GetAll() => Invoke<List<SimplePerson>>();
        public SimplePerson GetSimplePersona(Guid simplePersonaId) => Invoke<SimplePerson>(simplePersonaId);
        public SimplePerson GetSimplePersona(string name, string apellido) => Invoke<SimplePerson>(name, (object)apellido);

        public Task<List<SimplePerson>> GetAllAsync() => InvokeAsync<List<SimplePerson>>();
    }
    #endregion

}