using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Messaging.Configuration;
using TWCore.Messaging.RabbitMQ;
using TWCore.Security;
using TWCore.Serialization;
using TWCore.Services;

namespace TWCore.Tests
{
    public class MessageQueueTest : ContainerParameterService
    {
        public MessageQueueTest() : base("mqtest", "MessageQueue Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting Message Queue Test");

            #region Set Config
            var mqConfig = new MQPairConfig
            {
                Name = "QueueTest",
                Types = new MQObjectTypes { ClientType = typeof(RabbitMQueueClient), ServerType = typeof(RabbitMQueueServer), AdminType = typeof(RabbitMQueueAdmin) },
                ClientQueues = new List<MQClientQueues>
                {
                    new MQClientQueues
                    {
                        EnvironmentName = "",
                        MachineName = "",
                        SendQueues = new List<MQConnection> { new MQConnection("amqp://test:test@10.10.1.50:5672/", "TEST_RQ", null) },
                        RecvQueue = new MQConnection("amqp://test:test@10.10.1.50:5672/", "TEST_RS", null)
                    }
                },
                ServerQueues = new List<MQServerQueues>
                {
                    new MQServerQueues
                    {
                        EnvironmentName = "",
                        MachineName = "",
                        RecvQueues = new List<MQConnection> { new MQConnection("amqp://test:test@10.10.1.50:5672/", "TEST_RQ", null) }
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

            mqConfig.SerializeToXmlFile("mqConfig.xml");
            mqConfig.SerializeToJsonFile("mqConfig.json");

            var manager = mqConfig.GetQueueManager();
            manager.CreateClientQueues();

            using (var mqServer = mqConfig.GetServer())
            {
                mqServer.RequestReceived += (s, e) =>
                {
                    e.Response.Body = "Bienvenido!!!";
                };
                mqServer.StartListeners();

                //using (var mqClient = mqConfig.GetClient())
                //{
                //    var totalQ = 5000;

                //    Console.WriteLine("Sync Mode Test, using Unique Response Queue");
                //    using (var w = Watch.Create($"Hello World Example in Sync Mode for {totalQ} times"))
                //    {
                //        for (var i = 0; i < totalQ; i++)
                //        {
                //            var response = mqClient.SendAndReceive<string>("Hola mundo");
                //        }
                //        Core.Log.InfoBasic("Average time in ms: {0}", (w.GlobalElapsedMilliseconds / totalQ));
                //    }
                //    Console.ReadLine();


                //    Console.WriteLine("Parallel Mode Test, using Unique Response Queue");
                //    using (var w = Watch.Create($"Hello World Example in Parallel Mode for {totalQ} times"))
                //    {
                //        Parallel.For(0, totalQ, i =>
                //        {
                //            var response = mqClient.SendAndReceive<string>("Hola mundo");
                //        });
                //        Core.Log.InfoBasic("Average time in ms: {0}", (w.GlobalElapsedMilliseconds / totalQ));
                //    }
                //    Console.ReadLine();
                //}


                mqConfig.ResponseOptions.ClientReceiverOptions.Parameters["SingleResponseQueue"] = "false";
                using (var mqClient = mqConfig.GetClient())
                {
                    var totalQ = 50;

                    Console.WriteLine("Sync Mode Test, using Multiple Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Sync Mode for {totalQ} times"))
                    {
                        for (var i = 0; i < totalQ; i++)
                        {
                            var response = mqClient.SendAndReceive<string>("Hola mundo");
                        }
                        Core.Log.InfoBasic("Average time in ms: {0}", (w.GlobalElapsedMilliseconds / totalQ));
                    }
                    Console.ReadLine();


                    Console.WriteLine("Parallel Mode Test, using Multiple Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Parallel Mode for {totalQ} times"))
                    {
                        Parallel.For(0, totalQ, i =>
                        {
                            var response = mqClient.SendAndReceive<string>("Hola mundo");
                        });
                        Core.Log.InfoBasic("Average time in ms: {0}", (w.GlobalElapsedMilliseconds / totalQ));
                    }
                    Console.ReadLine();
                }
            }
        }
    }
}