using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.RabbitMQ;
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
    public class RabbitMQTest2 : ContainerParameterService
    {
        public RabbitMQTest2() : base("rabbitmqtest2", "RabbitMQ Test 2") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting RabbitMQ Test");

            #region Set Config
            var mqConfig = new MQPairConfig
            {
                Name = "QueueTest",
                Types = new MQObjectTypes { ClientType = typeof(RabbitMQueueClient), ServerType = typeof(RabbitMQueueServer), AdminType = typeof(RabbitMQueueAdmin) },
                RawTypes = new MQObjectTypes { ClientType = typeof(RabbitMQueueRawClient), ServerType = typeof(RabbitMQueueRawServer), AdminType = typeof(RabbitMQueueAdmin) },
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
                        MaxSimultaneousMessagesPerQueue = 2000,
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

            var manager = mqConfig.GetQueueManager();
            manager.CreateClientQueues();

            Core.Log.Warning("Starting with RAW Listener and Client");
            RawTest(mqConfig);
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
                    var totalQ = 150000;

                    #region Parallel Mode
                    Core.Log.Warning("RAW Parallel Mode Test, using Unique Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Parallel Mode for {totalQ} times"))
                    {
                        Task.WaitAll(
                            Enumerable.Range(0, totalQ).Select(_ => (Task)mqClient.SendAndReceiveAsync(byteRequest)).ToArray()
                        );
                        //Parallel.For(0, totalQ, i =>
                        //{
                        //    var response = mqClient.SendAndReceive(byteRequest);
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