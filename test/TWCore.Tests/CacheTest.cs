using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Cache;
using TWCore.Cache.Storages;
using TWCore.Cache.Storages.IO;
using TWCore.Serialization;
using TWCore.Services;

namespace TWCore.Tests
{
    public class CacheTest : ContainerParameterService
    {
        public CacheTest() : base("cachetest", "Cache Test") { }

        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Lru2QStorageTest();
            Core.Log.InfoBasic("Press ENTER to continue.");
            Console.ReadLine();
            FileStorageTest();
            Core.Log.InfoBasic("Press ENTER to continue.");
            Console.ReadLine();
            CombinedTest();
            Core.Log.InfoBasic("Press ENTER to exit.");
            Console.ReadLine();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void FileStorageTest()
        {
            Core.Log.Warning("FileStorage Test");
            using (var fSto = new FileStorage("./data"))
            {
                fSto.Init();
                Core.Log.InfoBasic("Press ENTER to start the test.");
                Console.ReadLine();
                ApplyTest(fSto);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Lru2QStorageTest()
        {
            Core.Log.Warning("LRU2QStorage Test");
            using (var sto = new LRU2QStorage(20000))
            {
                sto.Init();
                Core.Log.InfoBasic("Press ENTER to start the test.");
                Console.ReadLine();
                ApplyTest(sto);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CombinedTest()
        {
            Core.Log.Warning("Combined Test");
            using (var sto = new LRU2QStorage(20000))
            {
                sto.Init();
                using (var fSto = new FileStorage("./data2"))
                {
                    fSto.Init();
                    var stoManager = new StorageManager();
                    stoManager.Push(fSto);
                    stoManager.Push(sto);
                    Core.Log.InfoBasic("Press ENTER to start the test.");
                    Console.ReadLine();
                    ApplyTest(stoManager);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ApplyTest(IStorage fSto)
        {
            var sobj = new SerializedObject("Esto es un ejemplo del hola mundo");

            using (var w = Watch.Create("Cache GET"))
            {
                Parallel.For(0, 25000, i => fSto.Get("Tony - " + i));
                Core.Log.InfoBasic("Time per item: {0}ms", w.GlobalElapsedMilliseconds / 10000);
            }
            using (var w = Watch.Create("Cache SET"))
            {
                Parallel.For(0, 25000, i =>
                {
                    fSto.Set("Tony - " + i, sobj);
                });
                Core.Log.InfoBasic("Time per item: {0}ms", w.GlobalElapsedMilliseconds / 10000);
            }
            using (var w = Watch.Create("Cache GET"))
            {
                Parallel.For(0, 25000, i => fSto.Get("Tony - " + i));
                Core.Log.InfoBasic("Time per item: {0}ms", w.GlobalElapsedMilliseconds / 10000);
            }
        }
    }
}
