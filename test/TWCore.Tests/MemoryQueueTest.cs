using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Messaging;
using TWCore.Messaging.Configuration;
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
    public class MemoryQueueTest : ContainerParameterService
    {
        public MemoryQueueTest() : base("memoryqueuetest", "MemoryQueue Test") { }

        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting MemoryQueue Test");

            #region Set Config
            var mqConfig = new MQPairConfig
            {
                Name = "QueueTest",
                Types = new MQObjectTypes { ClientType = typeof(MemoryQueueClient), ServerType = typeof(MemoryQueueServer) },
                ClientQueues = new List<MQClientQueues>
                {
                    new MQClientQueues
                    {
                        EnvironmentName = "",
                        MachineName = "",
                        SendQueues = new List<MQConnection> { new MQConnection("amqp://cdr:cdr@127.0.0.1:5672/", "TEST_RQ", null) },
                        RecvQueue = new MQConnection("amqp://cdr:cdr@127.0.0.1:5672/", "TEST_RS", null)
                    }
                },
                ServerQueues = new List<MQServerQueues>
                {
                    new MQServerQueues
                    {
                        EnvironmentName = "",
                        MachineName = "",
                        RecvQueues = new List<MQConnection> { new MQConnection("amqp://cdr:cdr@127.0.0.1:5672/", "TEST_RQ", null) }
                    }
                },
                RequestOptions = new MQRequestOptions
                {
                    SerializerMimeType = SerializerManager.DefaultBinarySerializer.MimeTypes[0],
                    CompressorEncodingType = "gzip",
                    ClientSenderOptions = new MQClientSenderOptions
                    {
                        Label = "TEST REQUEST",
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
                },
                ResponseOptions = new MQResponseOptions
                {
                    SerializerMimeType = SerializerManager.DefaultBinarySerializer.MimeTypes[0],
                    CompressorEncodingType = "gzip",
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

            mqConfig.SerializeToXmlFile("memoryqConfig.xml");
            mqConfig.SerializeToJsonFile("memoryqConfig.json");

            var manager = mqConfig.GetQueueManager();
            manager.CreateClientQueues();

            //Core.DebugMode = true;
            //Core.Log.MaxLogLevel = Diagnostics.Log.LogLevel.InfoDetail;

            Core.Log.Warning("Starting with Normal Listener and Client");
            NormalTest(mqConfig);
        }

        private static void NormalTest(MQPairConfig mqConfig)
        {
            using (var mqServer = mqConfig.GetServer())
            {
                mqServer.RequestReceived += (s, e) =>
                {
                    e.Response.Body = "Bienvenido!!!";
                    return Task.CompletedTask;
                };
                mqServer.StartListeners();

                using (var mqClient = mqConfig.GetClient())
                {
                    var totalQ = 200000;

                    #region Sync Mode
                    Core.Log.Warning("Sync Mode Test, using Unique Response Queue");
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
                    Core.Log.Warning("Parallel Mode Test, using Unique Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Parallel Mode for {totalQ} times"))
                    {
                        Task.WaitAll(
                            Enumerable.Range(0, totalQ).Select(_ => (Task)mqClient.SendAndReceiveAsync<string>("Hola mundo")).ToArray()
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
    }
}