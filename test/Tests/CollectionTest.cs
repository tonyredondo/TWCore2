using System;
using System.Collections.Generic;
using System.Linq;
using TWCore.Collections;
using TWCore.Services;

namespace TWCore.Test.Tests
{
    public class CollectionTest : ContainerParameterService
    {
        public CollectionTest() : base("collectionTest", "Collections Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting COLLECTIONS TEST");
            Run(10, 3500, 8000, 3000);
        }

        public static void Run(int min, int max, int numberElements, int capacity)
        {
            var results = PreProcessRandomNumberCountThatShouldTakeTime(min, max);

            var randNum = new Random();
            var randomNumbers = Enumerable.Repeat(0, numberElements).Select(i => randNum.Next(min, max)).ToArray();

            var lru = new LRUCollection<int, long>(capacity);
            var lfu = new LFUCollection<int, long>(capacity);
            var lru2Q = new LRU2QCollection<int, long>((int)(capacity));
            var lru2QSimple = new LRU2QSimpleCollection<int, long>(capacity);

            using (Watch.Create("LFU Start", "LFU End"))
            {
                var hit = 0;
                foreach (var item in randomNumbers)
                {
                    if (!lfu.TryGetValue(item, out var value))
                    {
                        lfu.GetOrAdd(item, RandomNumberCountThatShouldTakeTime(item));
                    }
                    else
                    {
                        if (value != results[item])
                            Core.Log.Warning("Bad value: " + value + " " + results[item]);
                        hit++;
                    }
                }
                Core.Log.Debug("LFU Hits : " + hit + "");
            }

            using (Watch.Create("LRU Start", "LRU End"))
            {
                var hit = 0;
                foreach (var item in randomNumbers)
                {
                    if (!lru.TryGetValue(item, out var value))
                    {
                        lru.GetOrAdd(item, RandomNumberCountThatShouldTakeTime(item));
                    }
                    else
                    {
                        if (value != results[item])
                            Core.Log.Warning("Bad value: " + value + " " + results[item]);
                        hit++;
                    }
                }
                Core.Log.Debug("LRU Hits : " + hit + "");
            }

            using (Watch.Create("LRU2QSimple Start", "LRU2QSimple End"))
            {
                var hit = 0;
                foreach (var item in randomNumbers)
                {
                    if (!lru2QSimple.TryGetValue(item, out var value))
                    {
                        lru2QSimple.GetOrAdd(item, RandomNumberCountThatShouldTakeTime(item));
                    }
                    else
                    {
                        if (value != results[item])
                            Core.Log.Warning("Bad value");
                        hit++;
                    }
                }
                Core.Log.Debug("LRU2QSimple Hits : " + hit + "");
            }

            using (Watch.Create("LRU2Q Start", "LRU2Q End"))
            {
                var hit = 0;
                foreach (var item in randomNumbers)
                {
                    if (!lru2Q.TryGetValue(item, out var value))
                    {
                        lru2Q.GetOrAdd(item, RandomNumberCountThatShouldTakeTime(item));
                    }
                    else
                    {
                        if (value != results[item]) Core.Log.Warning("Bad value");
                        hit++;
                    }
                }
                Core.Log.Debug("LRU2Q hits : " + hit + "");
            }
        }

        private static long RandomNumberCountThatShouldTakeTime(int n)
        {
            long result = 0;
            for (var i = 1; i <= n; i++)
            {
                result++;
            }
            for (var i = 1; i <= n / 2; i++)
            {
                result = result + i;
            }
            for (var i = 0; i < 3000; i++) { }
            return result;
        }
        private static Dictionary<int, long> PreProcessRandomNumberCountThatShouldTakeTime(int min, int max)
        {
            var list = new Dictionary<int, long>();
            for (var i = min; i <= max; i++)
            {
                long result = 0;
                for (var j = 1; j <= i; j++)
                {
                    result++;
                }
                for (var j = 1; j <= i / 2; j++)
                {
                    result = result + j;
                }
                list.Add(i, result);
            }
            return list;
        }
    }
}