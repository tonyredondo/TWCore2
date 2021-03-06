﻿/*
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
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Compression;
using TWCore.Diagnostics.Status.Transports;
using TWCore.Messaging;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.RabbitMQ;
using TWCore.Net.Multicast;
using TWCore.Reflection;
using TWCore.Serialization;
using TWCore.Serialization.NSerializer;
using TWCore.Serialization.PWSerializer;
using TWCore.Serialization.WSerializer;
using TWCore.Services;
using TWCore.Tests;
using TWCore.Threading;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedVariable

namespace TWCore.Test.Core
{    
    internal class Program
    {
        private static readonly ICompressor Compressor = new GZipCompressor();
        private static readonly NBinarySerializer NBinarySerializer = new NBinarySerializer
        {
            Compressor = Compressor
        };

        public enum VarEnum
        {
            Value1,
            Value2
        }

        public sealed class TestClass
        {
            public bool? Enabled { get; set; }
            public int[] Values { get; set; }
            public decimal DecimalValue { get; set; }
        }
        public class ProblematicClass
        {
            public string Name { get; set; }
            public VarEnum? EnumValue { get; set; }
            public VarEnum? Value2 { get; set; }
            public VarEnum? Value3 { get; set; }
            public int? Number1 { get; set; }
            public int? Number2 { get; set; }
        }

        public sealed class ProviderCacheClone<T1, T2, T3, T4, T5>
        {
            public T1 Item1 { get; set; }
            public T2 Item2 { get; set; }
            public T3 Item3 { get; set; }
            public T4 Item4 { get; set; }
            public T5 Item5 { get; set; }
        }

        
        public class AItem
        {
            public string Name { get; set; }
        }
        public class BItem
        {
            public string Name { get; set; }
        }

        private static AsyncLocal<string> _asyncLocal = new AsyncLocal<string>();

        private static void Main(string[] args)
        {
            Console.WriteLine("MAIN");
            //
            //            var awaitCount = 0;
            //            var awaitableSample = new AwaitableManualEvent();
            //            _ = Task.Run(async () =>
            //            {
            //                while (true)
            //                {
            //                    Console.WriteLine("FIRE!!! - Count: " + awaitCount);
            //                    awaitCount = 0;
            //                    awaitableSample.Fire();
            //                    await Task.Delay(1000).ConfigureAwait(false);
            //                    awaitableSample.Reset();
            //                    Console.WriteLine("RESET!!! - Count: " + awaitCount);
            //                    awaitCount = 0;
            //                    await Task.Delay(2000).ConfigureAwait(false);
            //                }
            //            });
            //
            //            while (true)
            //            {
            //                var task = awaitableSample.WaitAsync();
            //                await task.ConfigureAwait(false);
            //                awaitCount++;
            //            }

            //_ = Task.Run(async () =>
            //{
            //    int i = 0;
            //    while(true)
            //    {
            //        i++;
            //        _asyncLocal.Value = "Task 1 - " + i;
            //        await Task.Delay(500).ConfigureAwait(false);
            //        Console.WriteLine(_asyncLocal.Value);
            //    }
            //});
            //_ = Task.Run(async () =>
            //{
            //    int i = 0;
            //    while (true)
            //    {
            //        i++;
            //        _asyncLocal.Value = "Task 2 - " + i;
            //        await Task.Delay(500).ConfigureAwait(false);
            //        Console.WriteLine(_asyncLocal.Value);
            //    }
            //});
            //Console.ReadLine();

            var aValue = new AItem { Name = "Test" };
            var bValue = aValue.MapTo(i => new BItem { Name = i.Name });

            var kv1 = new KeyValue<string, DateTime?>("Hola", TWCore.Core.Now);
			var kv2 = new KeyValue<string, DateTime?>("Hola", null);
			var kv3 = new KeyValue<string, string>("Hola", null);
			var kv4 = new KeyValue<string, string>("Hola", "");
			var xmlKv1 = kv1.SerializeToXml();
			var xmlKv2 = kv2.SerializeToXml();
			var xmlKv3 = kv3.SerializeToXml();
			var xmlKv4 = kv4.SerializeToXml();

            var kv5List = new KeyValue<string, string>[]
            {
                new KeyValue<string, string>("Key1", null),
                new KeyValue<string, string>("Key2", string.Empty),
                new KeyValue<string, string>("Key3", "Value 3"),
            };
            var xmlkv5List = kv5List.SerializeToXml();
            var kv5List2 = xmlkv5List.DeserializeFromXml<KeyValue<string, string>[]>();

            var kv1s = xmlKv1.DeserializeFromXml<KeyValue<string, DateTime?>>();
            var kv2s = xmlKv2.DeserializeFromXml<KeyValue<string, DateTime?>>();
            var kv3s = xmlKv3.DeserializeFromXml<KeyValue<string, string>>();
            var kv4s = xmlKv4.DeserializeFromXml<KeyValue<string, string>>();

            TWCore.Core.DebugMode = true;
            TWCore.Core.RunOnInit(() =>
            {
                TWCore.Core.SetContextGroupNameIfIsNullOrEmpty("TEST");
                TWCore.Core.Status.Transports.Add(new HttpStatusTransport(8089));
                TWCore.Core.Log.AddSimpleFileStorage("./log/testlog.txt");
                TWCore.Core.Log.AddHtmlFileStorage("./log/testlog.htm");
                TWCore.Core.Trace.AddSimpleFileStorage("./traces");

                // TWCore.Core.Log.AddElasticSearchStorage("http://10.10.1.52:9200", "TestIndex{0:yyyy.MM}");
                //TWCore.Core.Counters.Storages.Add(new TWCore.Diagnostics.Counters.Storages.ConsoleCountersStorage());
                //var queue = GetConfig().GetRawClient();
                //TWCore.Core.Counters.Storages.Add(new TWCore.Diagnostics.Counters.Storages.MessagingCountersStorage(queue));

                var path = Factory.ResolveLowLowFilePath("<</temp/copyright.txt");
                var folder = Factory.ResolveLowLowPath("<</temp/copyright.txt");
                var folder2 = Factory.ResolveLowLowPath("<<(Github)/logs");

                TWCore.Core.Log.AddGroupMetadata(("Key", "Value"), ("Key2", "Value"));
                TWCore.Core.Log.AddGroupMetadata(("Key3", "Value"));

                var matchTest = "value value value {Env:CONFIG_CACHESERVERIP} value value \r\n{Env:CONFIG_CACHESERVERIP} values";
                matchTest = TWCore.Core.ReplaceEnvironmentTemplate(matchTest);

                var testValue = new TestClass { Enabled = true, Values = new[] { 1, 2, 3, 4 }, DecimalValue = -13213.432M };
                
                var request = new TWCore.Net.RPC.RPCRequestMessage
                {
                    MessageId = Guid.NewGuid(),
                    MethodId = Guid.NewGuid(),
                    Parameters = new object[]
                    {
                        new string[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() },
                        new ProviderCacheClone<TestClass, TestClass, TestClass, TestClass, TestClass> { Item1 = testValue, Item2 = testValue, Item3 = testValue, Item4 = testValue, Item5 = testValue },
                        TimeSpan.FromMinutes(5)
                    }
                };

                var pcc = new ProviderCacheClone<TestClass, TestClass, TestClass, TestClass, TestClass> { Item1 = testValue, Item2 = testValue, Item3 = testValue, Item4 = testValue, Item5 = testValue };
                var pccBytes = pcc.SerializeToNBinary();

                var rqSer = request.SerializeToNBinary();

                var rqDes = rqSer.DeserializeFromNBinary<TWCore.Net.RPC.RPCMessage>();

                var testValueSer = new SerializedObject(testValue);

                testValueSer.SerializeToNBinary();

                var desTestValueSer = (TestClass)testValueSer.GetValue();

                new int[] { 1, 2, 3 }.SerializeToNBinary();

                testValue.SerializeToNBinary();

                var probValue = new ProblematicClass
                {
                    Name = "Hola",
                    EnumValue = VarEnum.Value2,
                    Value2 = null,
                    Value3 = VarEnum.Value1,
                    Number1 = 143243,
                    Number2 = null
                };

                var probValueSer = probValue.SerializeToNBinary();
                var probValue2 = probValueSer.DeserializeFromNBinary<ProblematicClass>();


                //                var testBytes = new byte[1024];
                //                var testBytes2 = new byte[1024];
                //                new Random().NextBytes(testBytes);
                //                new Random().NextBytes(testBytes2);
                //
                //                var multiTest1 = new MultiArray<byte>(new[] { testBytes, testBytes2, testBytes2, testBytes2 }).Slice(666, 3100);
                //                var multiTest2 = new MultiArray<byte>(new[] { testBytes, testBytes2, testBytes2, testBytes2 }).Slice(666, 3100);
                //
                //                for (var i = 0; i < 100000; i++)
                //                {
                //                    MultiArrayBytesComparer.Instance.GetHashCode(multiTest1);
                //                }
                //                
                //                using (Watch.Create("MultiArray Hash"))
                //                {
                //                    for (var i = 0; i < 100000; i++)
                //                    {
                //                        MultiArrayBytesComparer.Instance.GetHashCode(multiTest1);
                //                    }
                //                }


                //AssemblyResolverManager.RegisterDomain(new[] { "C:\\AGSW_GIT\\dlls" });

                //var serObj = NBinarySerializer.DeserializeFromFile<ResponseMessage>("C:\\Temp\\trace.data");

                //Task.Run(async () =>
                //{
                //    var value = serObj;
                //    NBinarySerializerExtensions.Serializer.Compressor = CompressorManager.GetByEncodingType("deflate");
                //    var valueByte = value.SerializeToNBinary();

                //    TWCore.Core.Log.InfoBasic("Testing object trace - {0}", valueByte.Count);
                //    GC.Collect();
                //    await Task.Delay(2000).ConfigureAwait(false);
                //    using (Watch.Create("Serialize"))
                //    {
                //        for (var i = 0; i < 10000; i++)
                //            value.SerializeToNBinary();
                //    }

                //    GC.Collect();
                //    await Task.Delay(2000).ConfigureAwait(false);
                //    using (Watch.Create("Deserialize"))
                //    {
                //        for (var i = 0; i < 10000; i++)
                //            valueByte.DeserializeFromNBinary<object>();
                //    }

                //});


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

        public static MQPairConfig GetConfig()
        {
            return new MQPairConfig
            {
                Name = "TWCore.Diagnostics.Api",
                RawTypes = new MQObjectTypes { ClientType = typeof(RabbitMQueueRawClient), ServerType = typeof(RabbitMQueueRawServer) },
                ClientQueues = new List<MQClientQueues>
                {
                    new MQClientQueues
                    {
                        EnvironmentName = "",
                        MachineName = "",
                        SendQueues = new List<MQConnection> { new MQConnection("amqp://test:test@127.0.0.1:5672/", "DIAGNOSTICS_RQ", null) },
                    }
                },
                RequestOptions = new MQRequestOptions
                {
                    SerializerMimeType = SerializerManager.DefaultBinarySerializer.MimeTypes[0],
                    CompressorEncodingType = "deflate",
                    ClientSenderOptions = new MQClientSenderOptions
                    {
                        MessageExpirationInSec = 30,
                        MessagePriority = MQMessagePriority.Normal,
                        Recoverable = false
                    },
                    ServerReceiverOptions = new MQServerReceiverOptions
                    {
                        MaxSimultaneousMessagesPerQueue = 2000,
                        ProcessingWaitOnFinalizeInSec = 10,
                        SleepOnExceptionInSec = 1000
                    }
                }
            };
        }
    }
}