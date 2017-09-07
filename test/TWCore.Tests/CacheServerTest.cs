using TWCore.Cache;
using TWCore.Cache.Storages;
using TWCore.Cache.Storages.IO;
using TWCore.Net.RPC.Server;
using TWCore.Net.RPC.Server.Transports;
using TWCore.Serialization;
using TWCore.Serialization.WSerializer;
using TWCore.Services;
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace TWCore.Tests
{

    public class CacheServerTest : ContainerParameterService
    {
        static ISerializer GlobalSerializer = new WBinarySerializer();

        public CacheServerTest() : base("cacheservertest", "Cache server test")
        {
        }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            info.Service = new TestCacheService();
            info.ShouldEndExecution = false;
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