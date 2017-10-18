using System;
using System.Threading.Tasks;
using TWCore.Cache.Client;
using TWCore.Net.RPC.Client.Transports.TW;
using TWCore.Serialization;
using TWCore.Serialization.WSerializer;
using TWCore.Services;
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    public class CacheClientTestAsync : ContainerParameterServiceAsync
    {
        private static ISerializer GlobalSerializer = new WBinarySerializer();


        public CacheClientTestAsync() : base("cacheclienttest", "Cache Client test")
        {
        }

        protected override async Task OnHandlerAsync(ParameterHandlerInfo info)
        {
            for (var i = 0; i < 100; i++)
            {
                Core.Log.Error("NUMBER: {0}", i);
                await Task.WhenAll(
                    CacheSingleTest(),
                    CacheSingleTest()
                ).ConfigureAwait(false);
                await Task.Delay(2000).ConfigureAwait(false);
            }
        }

        private static async Task CacheSingleTest()
        {
            using (var cachePool = new CacheClientPoolAsync { Serializer = GlobalSerializer })
            {
                var cacheClient = await CacheClientProxy
                    .GetClientAsync(new TWTransportClient("127.0.0.1", 20051, 1, GlobalSerializer)).ConfigureAwait(false);
                cachePool.Add("localhost:20051", (IStorageAsync)cacheClient, StorageItemMode.ReadAndWrite);

                try
                {
                    for (var i = 0; i < 15; i++)
                        await cachePool.GetKeysAsync().ConfigureAwait(false);

                    for (var i = 0; i < 100; i++)
                    {
                        var key = "test-" + i;
                        await cachePool.GetAsync(key).ConfigureAwait(false);
                        await cachePool.SetAsync(key, "bla bla bla bla bla bla").ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
        }
    }
}