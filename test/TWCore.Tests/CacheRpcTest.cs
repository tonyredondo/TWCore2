using System;
using System.Threading.Tasks;
using TWCore.Cache;
using TWCore.Cache.Client;
using TWCore.Cache.Storages;
using TWCore.Cache.Storages.IO;
using TWCore.Net.RPC.Client.Transports;
using TWCore.Net.RPC.Server;
using TWCore.Net.RPC.Server.Transports;
using TWCore.Serialization;
using TWCore.Serialization.WSerializer;
using TWCore.Services;

namespace TWCore.Tests
{
    public class CacheRpcTest : ContainerParameterServiceAsync
    {
		static ISerializer GlobalSerializer = new WBinarySerializer();

        public CacheRpcTest() : base("cacherpcTest", "Cache Test") { }
        protected override async Task OnHandlerAsync(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting CACHE TEST");

            var cacheService = new TestCacheService();
            cacheService.OnStart(null);

			using (var cachePool = new CacheClientPool { Serializer = GlobalSerializer })
            {
				var cacheClient = await CacheClientProxy.GetClientAsync(new TWTransportClient("127.0.0.1", 20051, 3, GlobalSerializer)).ConfigureAwait(false);
                cachePool.Add("localhost:20051", cacheClient, StorageItemMode.ReadAndWrite);

	            try
	            {
	                for (var i = 0; i < 15; i++)
			            cachePool.GetKeys();

		            Console.ReadLine();

	                for (var i = 0; i < 100; i++)
		            {
			            var key = "test-" + i;
			            cachePool.Get(key);
			            cachePool.Set(key, "bla bla bla bla bla");
		            }
		            Console.ReadLine();
		            for (var i = 0; i < 100; i++)
		            {
			            var key = "test-" + i;
			            cachePool.Get(key);
			            cachePool.Set(key, "bla bla bla bla bla");
		            }
	            }
	            catch (Exception ex)
	            {
		            Core.Log.Write(ex);
	            }
            }
        }

        class TestCacheService : CacheService
        {
            protected override StorageManager GetManager()
            {
				var fileSto = new FileStorage("./cache_data")
				{
					NumberOfSubFolders = 10
				};
				var lruSto = new LRU2QStorage(10000);
                var stoManager = new StorageManager();
                stoManager.Push(fileSto);
                stoManager.Push(lruSto);
                return stoManager;
            }
            protected override ITransportServer[] GetTransports()
            {
				return new ITransportServer[] { new TWTransportServer(20051, GlobalSerializer) };
            }
        }
    }
}