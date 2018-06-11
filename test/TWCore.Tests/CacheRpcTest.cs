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
using System.Threading.Tasks;
using TWCore.Cache;
using TWCore.Cache.Client;
using TWCore.Cache.Storages;
using TWCore.Cache.Storages.IO;
using TWCore.Net.RPC.Client.Transports.Default;
using TWCore.Net.RPC.Server.Transports;
using TWCore.Net.RPC.Server.Transports.Default;
using TWCore.Serialization;
using TWCore.Serialization.NSerializer;
using TWCore.Serialization.WSerializer;
using TWCore.Services;
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable

namespace TWCore.Tests
{
    /// <inheritdoc />
    public class CacheRpcTest : ContainerParameterServiceAsync
    {
        private static ISerializer GlobalSerializer = new NBinarySerializer();

        public CacheRpcTest() : base("cacherpcTest", "Cache Test") { }
        protected override async Task OnHandlerAsync(ParameterHandlerInfo info)
        {
	        Core.DebugMode = true;
            Core.Log.Warning("Starting CACHE TEST");

            var cacheService = new TestCacheService();
            cacheService.OnStart(null);

			using (var cachePool = new CacheClientPoolAsync("Pool Test") { Serializer = GlobalSerializer })
            {
				var cacheClient = await CacheClientProxy.GetClientAsync(new DefaultTransportClient("127.0.0.1", 20051, 3, GlobalSerializer)).ConfigureAwait(false);
                cachePool.Add("localhost:20051", cacheClient, StorageItemMode.ReadAndWrite);

				await cachePool.SetAsync("expTest", "testData", TimeSpan.FromSeconds(20), new[] { "test" }).ConfigureAwait(false);

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

                Console.ReadLine();

                var tagValue1 = await cachePool.GetByTagAsync(new [] {"test"}).ConfigureAwait(false);
                await Task.Delay(600).ConfigureAwait(false);
                var tagValue2 = await cachePool.GetByTagAsync(new [] {"test"}).ConfigureAwait(false);
                await Task.Delay(600).ConfigureAwait(false);
                await cachePool.SetAsync("expTest2", "testData2", TimeSpan.FromMinutes(1), new[] { "test" }).ConfigureAwait(false);
                await Task.Delay(600).ConfigureAwait(false);
                var tagValue3 = await cachePool.GetByTagAsync(new[] { "test" }).ConfigureAwait(false);

                await Task.Delay(600).ConfigureAwait(false);
                var response = await cachePool.ExecuteExtensionAsync("SampleExtension", "Echo", new object[] { "value1", "value2" });
                Core.Log.Warning("Extension Response: {0}", response);
                Console.ReadLine();

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