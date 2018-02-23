using System;
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
using TWCore.Serialization.PWSerializer;
using TWCore.Services;
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    /// <inheritdoc />
    public class CacheAsyncRpcTest : ContainerParameterServiceAsync
    {
        private static ISerializer GlobalSerializer = new PWBinarySerializer();

		public CacheAsyncRpcTest() : base("cacheasyncrpcTest", "Cache Async Test") { }
        protected override async Task OnHandlerAsync(ParameterHandlerInfo info)
        {
            Core.DebugMode = false;
            Core.Log.Warning("Starting CACHE Async TEST");

            var cacheService = new TestCacheService();
            cacheService.OnStart(null);

			using (var cachePool = new CacheClientPoolAsync { Serializer = GlobalSerializer, ForceAtLeastOneNetworkItemEnabled = false, WriteNetworkItemsToMemoryOnGet = true })
            {
				var cacheClient = await CacheClientProxy.GetClientAsync(new DefaultTransportClient("127.0.0.1", 20051, 3, GlobalSerializer)).ConfigureAwait(false);
				cachePool.Add("localhost:20051", cacheClient, StorageItemMode.ReadAndWrite);
                //cachePool.Add("memory", new LRU2QStorage(2000), StorageItemMode.ReadAndWrite);
                await cachePool.GetKeysAsync().ConfigureAwait(false);

                using (var watch = Watch.Create("GetKeysAsync"))
                {
                    for (var i = 0; i < 1000; i++)
                        await cachePool.GetKeysAsync().ConfigureAwait(false);
                    Core.Log.InfoBasic("Time Per Item: {0}ms", watch.GlobalElapsedMilliseconds / 1000);
                }

                Console.ReadLine();

                using (var watch = Watch.Create("Get And Sets"))
                {
                    for (var i = 0; i < 5000; i++)
                    {
                        var key = "test-" + i;
                        await cachePool.GetAsync(key).ConfigureAwait(false);
                        await cachePool.SetAsync(key, "bla bla bla bla bla").ConfigureAwait(false);
                    }
                    Core.Log.InfoBasic("Time Per Item: {0}ms", watch.GlobalElapsedMilliseconds / 5000);
                }
            }

            cacheService.OnStop();
            Console.ReadLine();
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