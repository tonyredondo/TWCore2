using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TWCore.Cache;
using TWCore.Cache.Client;
using TWCore.Cache.Storages;
using TWCore.Cache.Storages.IO;
using TWCore.Net.RPC.Client.Transports.Default;
using TWCore.Net.RPC.Server.Transports;
using TWCore.Net.RPC.Server.Transports.Default;
using TWCore.Serialization;
using TWCore.Serialization.WSerializer;
using TWCore.Services;
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    /// <inheritdoc />
    public class CacheRpcTest : ContainerParameterServiceAsync
    {
        private static ISerializer GlobalSerializer = new WBinarySerializer();

        public CacheRpcTest() : base("cacherpcTest", "Cache Test") { }
        protected override async Task OnHandlerAsync(ParameterHandlerInfo info)
        {
	        Core.DebugMode = true;
            Core.Log.Warning("Starting CACHE TEST");

            var cacheService = new TestCacheService();
            cacheService.OnStart(null);

			using (var cachePool = new CacheClientPoolAsync { Serializer = GlobalSerializer })
            {
				var cacheClient = await CacheClientProxy.GetClientAsync(new DefaultTransportClient("127.0.0.1", 20051, 3, GlobalSerializer)).ConfigureAwait(false);
                cachePool.Add("localhost:20051", cacheClient, StorageItemMode.ReadAndWrite);

				await cachePool.SetAsync("expTest", "testData", TimeSpan.FromSeconds(20)).ConfigureAwait(false);

				Console.ReadLine();

	            var asto = await cachePool.GetAsync("test").ConfigureAwait(false);
	            if (asto == null)
	            {
		            await cachePool.SetAsync("test", "value").ConfigureAwait(false);
		            asto = await cachePool.GetAsync("test").ConfigureAwait(false);
	            }
	            var guid = Guid.NewGuid();
	            await cachePool.CopyAsync("test", guid.ToString("N")).ConfigureAwait(false);
	            var asto2 = await cachePool.GetAsync(guid.ToString("N")).ConfigureAwait(false);

	            var guid2 = Guid.NewGuid();
	            await cachePool.CopyAsync("test", guid2.ToString("N")).ConfigureAwait(false);
	            var asto3 = await cachePool.GetAsync(guid2.ToString("N")).ConfigureAwait(false);

	            var guid3 = Guid.NewGuid();
	            await cachePool.CopyAsync("test", guid3.ToString("N")).ConfigureAwait(false);
	            var asto4 = await cachePool.GetAsync(guid3.ToString("N")).ConfigureAwait(false);

	            try
	            {
	                for (var i = 0; i < 15; i++)
			            await cachePool.GetKeysAsync().ConfigureAwait(false);

		            Console.ReadLine();

	                for (var i = 0; i < 100; i++)
		            {
			            var key = "test-" + i;
			            await cachePool.GetAsync(key).ConfigureAwait(false);
			            await cachePool.SetAsync(key, "bla bla bla bla bla").ConfigureAwait(false);
		            }
		            Console.ReadLine();
		            for (var i = 0; i < 100; i++)
		            {
			            var key = "test-" + i;
			            await cachePool.GetAsync(key).ConfigureAwait(false);
			            await cachePool.SetAsync(key, "bla bla bla bla bla").ConfigureAwait(false);
		            }
	            }
	            catch (Exception ex)
	            {
		            Core.Log.Write(ex);
	            }
            }

            cacheService.OnStop();
        }

        /// <inheritdoc />
        private class TestCacheService : CacheService
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
				return new ITransportServer[] { new DefaultTransportServer(20051, GlobalSerializer) };
            }
        }
    }
}