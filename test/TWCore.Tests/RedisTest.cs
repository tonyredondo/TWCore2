using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.Redis;
using TWCore.Serialization;
using TWCore.Services;
// ReSharper disable ConvertToConstant.Local
// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedVariable
// ReSharper disable AccessToDisposedClosure

namespace TWCore.Tests
{
    /// <inheritdoc />
    public class RedisTest : ContainerParameterService
    {
        public RedisTest() : base("redistest", "Redis Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting Redis Test");

            #region Set Config
            var mqConfig = new MQPairConfig
            {
                Name = "QueueTest",
                Types = new MQObjectTypes { ClientType = typeof(RedisQueueClient), ServerType = typeof(RedisQueueServer) },
                RawTypes = new MQObjectTypes { ClientType = typeof(RedisQueueRawClient), ServerType = typeof(RedisQueueRawServer) },
                ClientQueues = new List<MQClientQueues>
                {
                    new MQClientQueues
                    {
                        EnvironmentName = "",
                        MachineName = "",
						SendQueues = new List<MQConnection> { new MQConnection("localhost", "TEST_RQ", null) },
						RecvQueue = new MQConnection("localhost", "TEST_RS", null)
                    }
                },
                ServerQueues = new List<MQServerQueues>
                {
                    new MQServerQueues
                    {
                        EnvironmentName = "",
                        MachineName = "",
						RecvQueues = new List<MQConnection> { new MQConnection("localhost", "TEST_RQ", null) }
                    }
                },
                RequestOptions = new MQRequestOptions
                {
                    SerializerMimeType = SerializerManager.DefaultBinarySerializer.MimeTypes[0],
                    //CompressorEncodingType = "gzip",
                    ClientSenderOptions = new MQClientSenderOptions
                    {
                        Label = "TEST REQUEST",
                        MessageExpirationInSec = 30,
                        MessagePriority = MQMessagePriority.Normal,
                        Recoverable = false
                    },
                    ServerReceiverOptions = new MQServerReceiverOptions
                    {
                        MaxSimultaneousMessagesPerQueue = 20000,
                        ProcessingWaitOnFinalizeInSec = 10,
                        SleepOnExceptionInSec = 1000
                    }
                },
                ResponseOptions = new MQResponseOptions
                {
                    SerializerMimeType = SerializerManager.DefaultBinarySerializer.MimeTypes[0],
                    //CompressorEncodingType = "gzip",
                    ClientReceiverOptions = new MQClientReceiverOptions(60,
                        new KeyValue<string, string>("SingleResponseQueue", "true")
                    ),
                    ServerSenderOptions = new MQServerSenderOptions
                    {
                        Label = "TEST RESPONSE",
                        MessageExpirationInSec = 30,
                        MessagePriority = MQMessagePriority.Normal,
                        Recoverable = false
                    }
                }
            };
            #endregion

            JsonTextSerializerExtensions.Serializer.Indent = true;

            mqConfig.SerializeToXmlFile("redisConfig.xml");
            mqConfig.SerializeToJsonFile("redisConfig.json");

            var manager = mqConfig.GetQueueManager();
            manager.CreateClientQueues();

            //Core.DebugMode = true;
            //Core.Log.MaxLogLevel = Diagnostics.Log.LogLevel.InfoDetail;

            Core.Log.Warning("Starting with Normal Listener and Client");
            NormalTest(mqConfig);
            mqConfig.ResponseOptions.ClientReceiverOptions.Parameters["SingleResponseQueue"] = "true";
            Core.Log.Warning("Starting with RAW Listener and Client");
            RawTest(mqConfig);
        }

        private static void NormalTest(MQPairConfig mqConfig)
        {
            using (var mqServer = mqConfig.GetServer())
            {
                mqServer.RequestReceived += async (s, e) =>
                {
                    //Core.Log.Warning("Waiting...");
                    //try
                    //{
                    //    await Task.Delay(int.MaxValue, e.ProcessResponseTimeoutCancellationToken);
                    //}
                    //catch { }
                    //Core.Log.Warning("Cancelled.");
                    //Core.Log.InfoBasic("Received");
                    e.Response.Body = new SerializedObject("Bienvenido!!!");
                };
                mqServer.StartListeners();

                using (var mqClient = mqConfig.GetClient())
                {
                    var totalQ = 50000;

                    #region Sync Mode
                    Core.Log.Warning("Sync Mode Test, using Unique Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Sync Mode for {totalQ} times"))
                    {
                        for (var i = 0; i < totalQ; i++)
                        {
                            var response = mqClient.SendAndReceiveAsync<string>("Hola mundo").WaitAsync();
                        }
                        Core.Log.InfoBasic("Total time: {0}", TimeSpan.FromMilliseconds(w.GlobalElapsedMilliseconds));
                        Core.Log.InfoBasic("Average time in ms: {0}. Press ENTER To Continue.", (w.GlobalElapsedMilliseconds / totalQ));
                    }
                    Console.ReadLine();
                    #endregion

                    totalQ = 50000;

                    #region Parallel Mode
                    Core.Log.Warning("Parallel Mode Test, using Unique Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Parallel Mode for {totalQ} times"))
                    {
                        var oldContext = Core.ContextGroupName;
                        Task.WaitAll(
                            Enumerable.Range(0, totalQ).Select((i, mc) => 
                            {
                                Core.ContextGroupName = "Message: " + i;
                                return (Task)mc.SendAndReceiveAsync<string>("Hola mundo");
                            }
                        , mqClient).ToArray());
                        Core.ContextGroupName = oldContext;

                        //Parallel.For(0, totalQ, i =>
                        //{
                        //    var response = mqClient.SendAndReceiveAsync<string>("Hola mundo").WaitAndResults();
                        //});
                        Core.Log.InfoBasic("Total time: {0}", TimeSpan.FromMilliseconds(w.GlobalElapsedMilliseconds));
                        Core.Log.InfoBasic("Average time in ms: {0}. Press ENTER To Continue.", (w.GlobalElapsedMilliseconds / totalQ));
                    }
                    Console.ReadLine();
                    #endregion
                }

                mqConfig.ResponseOptions.ClientReceiverOptions.Parameters["SingleResponseQueue"] = "false";
                using (var mqClient = mqConfig.GetClient())
                {
                    var totalQ = 50000;

                    #region Sync Mode
                    Core.Log.Warning("Sync Mode Test, using Multiple Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Sync Mode for {totalQ} times"))
                    {
                        for (var i = 0; i < totalQ; i++)
                        {
                            var response = mqClient.SendAndReceiveAsync<string>("Hola mundo").WaitAndResults();
                        }
                        Core.Log.InfoBasic("Total time: {0}", TimeSpan.FromMilliseconds(w.GlobalElapsedMilliseconds));
                        Core.Log.InfoBasic("Average time in ms: {0}. Press ENTER To Continue.", (w.GlobalElapsedMilliseconds / totalQ));
                    }
                    Console.ReadLine();
                    #endregion

                    #region Parallel Mode
                    Core.Log.Warning("Parallel Mode Test, using Multiple Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Parallel Mode for {totalQ} times"))
                    {
                        Task.WaitAll(
                            Enumerable.Range(0, totalQ).Select((i, mc) => (Task)mc.SendAndReceiveAsync<string>("Hola mundo"), mqClient).ToArray()
                        );
                        //Parallel.For(0, totalQ, i =>
                        //{
                        //    var response = mqClient.SendAndReceiveAsync<string>("Hola mundo").WaitAndResults();
                        //});
                        Core.Log.InfoBasic("Total time: {0}", TimeSpan.FromMilliseconds(w.GlobalElapsedMilliseconds));
                        Core.Log.InfoBasic("Average time in ms: {0}. Press ENTER To Continue.", (w.GlobalElapsedMilliseconds / totalQ));
                    }
                    Console.ReadLine();
                    #endregion
                }
            }
        }

        private static void RawTest(MQPairConfig mqConfig)
        {
            using (var mqServer = mqConfig.GetRawServer())
            {
                var byteRequest = new byte[] { 0x21, 0x22, 0x23, 0x24, 0x25, 0x30, 0x31, 0x32, 0x33, 0x34 };
                var byteResponse = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x10, 0x11, 0x12, 0x13, 0x14 };
                mqServer.RequestReceived += (s, e) =>
                {
                    e.Response = byteResponse;
                    return Task.CompletedTask;
                };
                mqServer.StartListeners();

                using (var mqClient = mqConfig.GetRawClient())
                {
                    var totalQ = 50000;

                    #region Sync Mode
                    Core.Log.Warning("RAW Sync Mode Test, using Unique Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Sync Mode for {totalQ} times"))
                    {
                        for (var i = 0; i < totalQ; i++)
                        {
                            var response = mqClient.SendAndReceiveAsync(byteRequest).WaitAsync();
                        }
                        Core.Log.InfoBasic("Total time: {0}", TimeSpan.FromMilliseconds(w.GlobalElapsedMilliseconds));
                        Core.Log.InfoBasic("Average time in ms: {0}. Press ENTER To Continue.", (w.GlobalElapsedMilliseconds / totalQ));
                    }
                    Console.ReadLine();
                    #endregion

                    totalQ = 50000;

                    #region Parallel Mode
                    Core.Log.Warning("RAW Parallel Mode Test, using Unique Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Parallel Mode for {totalQ} times"))
                    {
                        Task.WaitAll(
                            Enumerable.Range(0, totalQ).Select((i, vTuple) => (Task)vTuple.mqClient.SendAndReceiveAsync(vTuple.byteRequest), (mqClient, byteRequest)).ToArray()
                        );
                        //Parallel.For(0, totalQ, i =>
                        //{
                        //    var response = mqClient.SendAndReceiveAsync(byteRequest).WaitAndResults();
                        //});
                        Core.Log.InfoBasic("Total time: {0}", TimeSpan.FromMilliseconds(w.GlobalElapsedMilliseconds));
                        Core.Log.InfoBasic("Average time in ms: {0}. Press ENTER To Continue.", (w.GlobalElapsedMilliseconds / totalQ));
                    }
                    Console.ReadLine();
                    #endregion
                }


                mqConfig.ResponseOptions.ClientReceiverOptions.Parameters["SingleResponseQueue"] = "false";
                using (var mqClient = mqConfig.GetRawClient())
                {
                    var totalQ = 50000;

                    #region Sync Mode
                    Core.Log.Warning("RAW Sync Mode Test, using Multiple Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Sync Mode for {totalQ} times"))
                    {
                        for (var i = 0; i < totalQ; i++)
                        {
                            var response = mqClient.SendAndReceiveAsync(byteRequest).WaitAndResults();
                        }
                        Core.Log.InfoBasic("Total time: {0}", TimeSpan.FromMilliseconds(w.GlobalElapsedMilliseconds));
                        Core.Log.InfoBasic("Average time in ms: {0}. Press ENTER To Continue.", (w.GlobalElapsedMilliseconds / totalQ));
                    }
                    Console.ReadLine();
                    #endregion

                    #region Parallel Mode
                    Core.Log.Warning("RAW Parallel Mode Test, using Multiple Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Parallel Mode for {totalQ} times"))
                    {
                        Task.WaitAll(
                            Enumerable.Range(0, totalQ).Select((i, vTuple) => (Task)vTuple.mqClient.SendAndReceiveAsync(vTuple.byteRequest), (mqClient, byteRequest)).ToArray()
                        );
                        //Parallel.For(0, totalQ, i =>
                        //{
                        //    var response = mqClient.SendAndReceiveAsync(byteRequest).WaitAndResults();
                        //});
                        Core.Log.InfoBasic("Total time: {0}", TimeSpan.FromMilliseconds(w.GlobalElapsedMilliseconds));
                        Core.Log.InfoBasic("Average time in ms: {0}. Press ENTER To Continue.", (w.GlobalElapsedMilliseconds / totalQ));
                    }
                    Console.ReadLine();
                    #endregion
                }
            }
        }

    }
}