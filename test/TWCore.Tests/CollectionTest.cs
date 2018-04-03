using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Services;
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    /// <inheritdoc />
    public class CollectionTest : ContainerParameterService
    {
        public CollectionTest() : base("collectionTest", "Collections Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
			Thread.Sleep(1000);

			Core.Log.Warning("Starting HIT TEST");
            Run(0, 50_000, 10_000_000, 50_000);

			Thread.Sleep(2000);
			Core.Log.WriteEmptyLine();
			Core.Log.WriteEmptyLine();

			Core.Log.Warning("Starting INSERTION/DELETE TEST");
			Run(0, 200_000_000, 10_000_000, 50_000);
        }

        public static void Run(int min, int max, int numberElements, int capacity)
        {
            var randNum = new Random();
            var randomNumbers = Enumerable.Repeat(0, numberElements).Select((i, vTuple) => vTuple.randNum.Next(vTuple.min, vTuple.max), (randNum, min, max)).ToArray();
            var lru = new LRUCollection<int, long>(capacity);
            var lfu = new LFUCollection<int, long>(capacity);
            var lru2Q = new LRU2QCollection<int, long>(capacity);
            var lru2QSimple = new LRU2QSimpleCollection<int, long>(capacity);

            Core.Log.InfoBasic("Processing: {0} elements", numberElements);
            Core.Log.InfoBasic("Collections Capacity: {0} elements", capacity);
            Core.Log.InfoBasic("Random numbers from {0} to {1}", min, max);
			Core.Log.WriteEmptyLine();

			using (var w = Watch.Create("LFU Start", "LFU End"))
            {
                Parallel.ForEach(randomNumbers, item =>
                {
                    var value = lfu.GetOrAdd(item, item);
                    if (value != item)
                        Core.Log.Warning("Bad value: " + value + " " + item);
                });
                var ms = w.GlobalElapsedMilliseconds / numberElements;
                Core.Log.InfoDetail("LFU Hits: {0}", lfu.Hits);
                Core.Log.InfoDetail("LFU Deletes: {0}", lfu.Deletes);
                Core.Log.InfoDetail("LFU Inserts: {0}", lfu.Inserts);
                Core.Log.InfoDetail("LFU ms per item: {0}ms", ms);
                Core.Log.InfoDetail("LFU ns per item: {0}ns", ms * 1_000_000);
            }
			Thread.Sleep(1000);
			Core.Log.WriteEmptyLine();


			using (var w = Watch.Create("LRU Start", "LRU End"))
            {
                Parallel.ForEach(randomNumbers, item =>
                {
                    var value = lru.GetOrAdd(item, item);
                    if (value != item)
                        Core.Log.Warning("Bad value: " + value + " " + item);
                });
                var ms = w.GlobalElapsedMilliseconds / numberElements;
                Core.Log.InfoDetail("LRU Hits: {0}", lru.Hits);
                Core.Log.InfoDetail("LRU Deletes: {0}", lru.Deletes);
                Core.Log.InfoDetail("LRU Inserts: {0}", lru.Inserts);
                Core.Log.InfoDetail("LRU ms per item: {0}ms", ms);
                Core.Log.InfoDetail("LRU ns per item: {0}ns", ms * 1_000_000);
            }
			Thread.Sleep(1000);
			Core.Log.WriteEmptyLine();


			using (var w = Watch.Create("LRU2QSimple Start", "LRU2QSimple End"))
            {
                Parallel.ForEach(randomNumbers, item =>
                {
                    var value = lru2QSimple.GetOrAdd(item, item);
                    if (value != item)
                        Core.Log.Warning("Bad value: " + value + " " + item);
                });
                var ms = w.GlobalElapsedMilliseconds / numberElements;
                Core.Log.InfoDetail("LRU2QSimple Hits: {0}", lru2QSimple.Hits);
                Core.Log.InfoDetail("LRU2QSimple Deletes: {0}", lru2QSimple.Deletes);
                Core.Log.InfoDetail("LRU2QSimple Inserts: {0}", lru2QSimple.Inserts);
                Core.Log.InfoDetail("LRU2QSimple ms per item: {0}ms", ms);
                Core.Log.InfoDetail("LRU2QSimple ns per item: {0}ns", ms * 1_000_000);
            }
			Thread.Sleep(1000);
			Core.Log.WriteEmptyLine();


			using (var w = Watch.Create("LRU2Q Start", "LRU2Q End"))
            {
                Parallel.ForEach(randomNumbers, item =>
                {
                    var value = lru2Q.GetOrAdd(item, item);
                    if (value != item)
                        Core.Log.Warning("Bad value: " + value + " " + item);
                });
                var ms = w.GlobalElapsedMilliseconds / numberElements;
                Core.Log.InfoDetail("LRU2Q Hits: {0}", lru2Q.Hits);
                Core.Log.InfoDetail("LRU2Q Deletes: {0}", lru2Q.Deletes);
                Core.Log.InfoDetail("LRU2Q Inserts: {0}", lru2Q.Inserts);
                Core.Log.InfoDetail("LRU2Q ms per item: {0}ms", ms);
                Core.Log.InfoDetail("LRU2Q ns per item: {0}ns", ms * 1_000_000);
            }
			Thread.Sleep(1000);
			Core.Log.WriteEmptyLine();

		}
    }
}