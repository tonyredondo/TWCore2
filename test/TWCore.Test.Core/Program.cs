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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status.Transports;
using TWCore.Net.Multicast;
using TWCore.Reflection;
using TWCore.Serialization;
using TWCore.Serialization.NSerializer;
using TWCore.Serialization.PWSerializer;
using TWCore.Serialization.WSerializer;
using TWCore.Services;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedVariable

namespace TWCore.Test.Core
{
    internal class Program
    {

        public enum VarEnum
        {
            Value1,
            Value2
        }

        public class TestClass
        {
            public bool? Enabled { get; set; }
            public int[] Values { get; set; }
            public decimal DecimalValue { get; set; }
        }

        public class ProviderCacheClone<T1, T2, T3, T4, T5>
        {
            public T1 Item1 { get; set; }
            public T2 Item2 { get; set; }
            public T3 Item3 { get; set; }
            public T4 Item4 { get; set; }
            public T5 Item5 { get; set; }
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("MAIN");

            SerializerManager.DefaultBinarySerializer = new NBinarySerializer();

            TWCore.Core.DebugMode = true;
            TWCore.Core.RunOnInit(() =>
            {
                TWCore.Core.Status.Transports.Add(new HttpStatusTransport(8089));
                TWCore.Core.Log.AddSimpleFileStorage("./log/testlog.txt");
                TWCore.Core.Log.AddHtmlFileStorage("./log/testlog.htm");
                TWCore.Core.Trace.AddSimpleFileStorage("./traces");

                var path = Factory.ResolveLowLowFilePath("<</temp/copyright.txt");
                var folder = Factory.ResolveLowLowPath("<</temp/copyright.txt");
                var folder2 = Factory.ResolveLowLowPath("<<(Github)/logs");

                var matchTest = "value value value {Env:CONFIG_CACHESERVERIP} value value \r\n{Env:CONFIG_CACHESERVERIP} values";
                matchTest = TWCore.Core.ReplaceEnvironmentTemplate(matchTest);

                var sKeyProvider = new TWCore.Security.SymmetricKeyProvider();
                var guid = Guid.NewGuid().ToString();
                var value = sKeyProvider.Encrypt("Data Source=10.10.1.24;Initial Catalog=AGSW_BACKEND;User Id=sa;Password=ElPatr0n;Pooling=True", guid);

                var testValue = new TestClass { Enabled = true, Values = new[] { 1, 2, 3, 4 }, DecimalValue = -13213.432M };

                var strSplit = "fdsfdsafdsafd;FdsafdsAF;dsafD;saF;DSA;FDS;AFDsafdsa;fdssa;;;fdsaf;";
                var split1 = strSplit.Split(';', StringSplitOptions.None);
                var split2 = strSplit.Split(';', StringSplitOptions.RemoveEmptyEntries);
                var splitSpan1 = strSplit.AsSpan().SplitAsString(';');
                var splitSpan2 = strSplit.AsSpan().SplitAsString(';', StringSplitOptions.RemoveEmptyEntries);
                var splitMemory1 = strSplit.AsMemory().SplitAsString(';');
                var splitMemory2 = strSplit.AsMemory().SplitAsString(';', StringSplitOptions.RemoveEmptyEntries);

                var request = new TWCore.Net.RPC.RPCRequestMessage
                {
                    MessageId = Guid.NewGuid(),
                    MethodId = Guid.NewGuid(),
                    Parameters = new object[]
                    {
                        new string[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() },
                        new SerializedObject(new ProviderCacheClone<TestClass, TestClass, TestClass, TestClass, TestClass> { Item1 = testValue, Item2 = testValue, Item3 = testValue, Item4 = testValue, Item5 = testValue } ),
                        TimeSpan.FromMinutes(5)
                    }
                };

                var rqSer = request.SerializeToNBinary();

                var rqDes = rqSer.DeserializeFromNBinary<TWCore.Net.RPC.RPCMessage>();



                var testValueSer = new SerializedObject(testValue);

                testValueSer.SerializeToNBinary();

                var desTestValueSer = (TestClass)testValueSer.GetValue();

                new int[] { 1, 2, 3 }.SerializeToNBinary();

                testValue.SerializeToNBinary();

                //using (var wc = Watch.Create("OffersController - POST - Synchronous", Diagnostics.Log.LogLevel.Stats, "f5c77cd8-48ce-4fc7-9c41-ada953b1ebb9"))
                //{
                //    wc.Tap("Tap 1");
                //    wc.Tap("Tap 2");
                //}
                //using (var wc = Watch.Create("OffersController - POST - Synchronous", Diagnostics.Log.LogLevel.Stats, "f5c77cd8-48ce-4fc7-9c41-ada953b1ebb9"))
                //{
                //    wc.Tap("Tap 1");
                //    wc.Tap("Tap 2");
                //}
                //using (var wc = Watch.Create("OffersController - POST - Synchronous", Diagnostics.Log.LogLevel.Stats, "f5c77cd8-48ce-4fc7-9c41-ada953b1ebb9"))
                //{
                //    wc.Tap("Tap 1");
                //    wc.Tap("Tap 2");
                //}
                //using (var wc = Watch.Create("OffersController - POST - Synchronous", Diagnostics.Log.LogLevel.Stats, "f5c77cd8-48ce-4fc7-9c41-ada953b1ebb9"))
                //{
                //    wc.Tap("Tap 1");
                //    wc.Tap("Tap 2");
                //}

                //using (var wc = Watch.Create("Inicio", "Fin"))
                //{
                //    wc.Tap("Tap 1");
                //    using (var wc2 = Watch.Create("Inicio 2", "Fin 2"))
                //    {
                //        wc2.Tap("Tap 1");
                //        using (var wc3 = Watch.Create("Inicio 2", "Fin 2"))
                //        {
                //            wc3.Tap("Tap 1");
                //            wc3.Tap("Tap 2");
                //        }
                //        wc2.Tap("Tap 2");
                //    }
                //    wc.Tap("Tap 2");
                //}
                //Console.ReadLine();

                /*Task.Run(async () =>
                {
                    var rnd = new Random();
                    var pool = new ObjectPool<int>(i => rnd.Next(100), null, 0, PoolResetMode.AfterUse, 5);
                    while (true)
                    {
                        var max = rnd.Next(10);
                        TWCore.Core.Log.InfoBasic("Using pool ({0}) Current Count = {1}", max, pool.Count);
                        for (var i = 0; i < max; i++)
                            pool.New();
                        for (var i = 0; i < max; i++)
                            pool.Store(i);
                        
                        await Task.Delay(1000).ConfigureAwait(false);
                        TWCore.Core.Log.InfoBasic("Current Count = {0}", pool.Count);
                    }
                });*/

                //DiscoveryService.OnNewServiceReceived += DiscoveryService_OnServiceReceived;
                //DiscoveryService.OnServiceExpired += DiscoveryService_OnServiceExpired;
                //DiscoveryService.OnServiceReceived += DiscoveryService_OnServiceReceived;
            });
            TWCore.Core.RunService<TestService>(args);
        }

        private static void DiscoveryService_OnServiceExpired(object sender, EventArgs<DiscoveryService.ReceivedService> e)
        {
            TWCore.Core.Log.InfoBasic("Core Service Discovery Remove: {0}, {1}, {2}, {3}, {4}, {5}, {6}", e.Item1.Category, e.Item1.ApplicationName, e.Item1.Name, e.Item1.Description, e.Item1.ApplicationName, e.Item1.MachineName, string.Join(", ", e.Item1.Addresses.Select(a => a.ToString())));
        }

        private static void DiscoveryService_OnServiceReceived(object sender, EventArgs<DiscoveryService.ReceivedService> e)
        {
            TWCore.Core.Log.InfoBasic("Core Service Discovery Add: {0}, {1}, {2}, {3}, {4}, {5}, {6}", e.Item1.Category, e.Item1.ApplicationName, e.Item1.Name, e.Item1.Description, e.Item1.ApplicationName, e.Item1.MachineName, string.Join(", ", e.Item1.Addresses.Select(a => a.ToString())));
            if (e.Item1.Data.GetValue() is int iValue)
                TWCore.Core.Log.InfoDetail("Value: {0}", iValue);
            if (!(e.Item1.Data.GetValue() is Dictionary<string, object> value)) return;
            foreach (var item in value)
                TWCore.Core.Log.InfoDetail("\tParam {0} = {1}", item.Key, item.Value);
        }

        private class TestService : SimpleServiceAsync
        {
            protected override async Task OnActionAsync(CancellationToken token)
            {
                TWCore.Core.Log.InfoBasic("STARTING TEST SERVICE");
                //**
                //TWCore.Core.Injector.Settings.Interfaces.Add(new NonInstantiable
                //{
                //    Type = "TWCore.ICoreStart, TWCore",
                //    ClassDefinitions = new NameCollection<Instantiable>
                //    {
                //        new Instantiable { Name = "C1", Type = "TWCore.CoreStart, TWCore", Singleton = true }
                //    }
                //});
                //var value = TWCore.Core.Injector.New<ICoreStart>("C1");

                var cd = new ConcurrentDictionary<string, string>();
                cd.GetOrAdd(string.Empty, k =>
                {
                    TWCore.Core.Log.Warning("HOLA MUNDO!!!");
                    return string.Empty;
                });
                //**
                await Task.Delay(10000, token).ConfigureAwait(false);
                TWCore.Core.Log.InfoBasic("FINALIZING TEST SERVICE");
            }
        }
    }
}