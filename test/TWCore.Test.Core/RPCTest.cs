using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TWCore.Net.RPC;
using TWCore.Net.RPC.Attributes;
using TWCore.Net.RPC.Client;
using TWCore.Net.RPC.Client.Transports;
using TWCore.Net.RPC.Server;
using TWCore.Net.RPC.Server.Transports;
using TWCore.Serialization;
using TWCore.Services;

namespace TWCore.Tests
{
    public class RpcTest : ContainerParameterServiceAsync
    {
        public RpcTest() : base("rpctest", "RPC Test") { }
        protected override async Task OnHandlerAsync(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting RPC TEST");

            var serializer = new WBinarySerializer();
            var service = new MyService();

            Core.Log.InfoBasic("Setting RPC Server");
            var rpcServer = new RPCServer(new TWTransportServer(20050, serializer));
            rpcServer.AddService(service);
            await rpcServer.StartAsync().ConfigureAwait(false);

            Core.Log.InfoBasic("Setting RPC Client");
            var rpcClient = new RPCClient(new TWTransportClient("127.0.0.1", 20050, 3, serializer));

            //IHello test
            Core.Log.InfoBasic("IHello test");
            dynamic hClient = await rpcClient.CreateDynamicProxyAsync<IHello>().ConfigureAwait(false);
            var rtest = (string)hClient.SayHi("MyName");

            for (var i = 0; i < 1000; i++)
            {
                using (var w = Watch.Create())
                {
                    var rHClient = (string)hClient.SayHi("MyName");
                    w.Tap("Say Hi: {0}".ApplyFormat(rHClient));
                }
            };

            //IMyService test
            Core.Log.InfoBasic("IMyService test");
            dynamic dClient = await rpcClient.CreateDynamicProxyAsync<IMyService>().ConfigureAwait(false);
            for (var i = 0; i < 1000; i++)
            {
                using (var w = Watch.Create())
                {
                    var aLst = dClient.GetAll();
                    w.Tap("GetAll, Items: {0}".ApplyFormat(((List<SimplePerson>)aLst).Count));
                }
            }

            //Proxy class test
            Core.Log.InfoBasic("Proxy class test");
            var client = await rpcClient.CreateProxyAsync<MyServiceProxy>().ConfigureAwait(false);
            for (var i = 0; i < 1000; i++)
            {
                using (var w = Watch.Create())
                {
                    var resp = client.GetAll();
                    w.Tap("GetAll, Items: {0}".ApplyFormat(resp.Count));
                }
            }

            //Event test
            Core.Log.InfoBasic("Event test");
            client.OnAddSimplePersona += (s, e) => Core.Log.Warning("On Add SimplePersona was fired!!!");
            for (var i = 0; i < 1000; i++)
            {
                using (var w = Watch.Create("Add Persona"))
                {
                    client.AddSimplePersona(new SimplePerson { Lastname = "Test", Firstname = "Test" });
                }
            }

            Core.Log.InfoBasic("Test End.");
        }
    }


    #region RPC Server Test
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
    }

    public class MyService : IMyService, IHello
    {
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
            new SimplePerson { Firstname = "Daniel", Lastname = "Redondo", Enabled = true, PersonId = Guid.NewGuid() },
        };
        #endregion

        public List<SimplePerson> GetAll()
            => SimplePersonas.Concat(SimplePersonas).Concat(SimplePersonas).Concat(SimplePersonas).ToList();

        public SimplePerson GetSimplePersona(Guid simplePersonaId)
            => SimplePersonas.FirstOrDefault(p => p.PersonId == simplePersonaId);

        public SimplePerson GetSimplePersona(string name, string apellido)
            => SimplePersonas.FirstOrDefault(p => p.Firstname == name && p.Lastname == apellido);

        public string SayHi(string name)
            => $"Hi {name}!";

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
    }
    #endregion

}