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
using System.Collections.Generic;
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
    public class RabbitMQTest : ContainerParameterServiceAsync
    {
        public RabbitMQTest() : base("rabbitmqtest", "RabbitMQ Test") { }
        protected override async Task OnHandlerAsync(ParameterHandlerInfo info)
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
                        SendQueues = new List<MQConnection> { new MQConnection("amqp://agsw:agsw@127.0.0.1:5672/", "TEST_RQ", null) },
                        RecvQueue = new MQConnection("amqp://agsw:agsw@127.0.0.1:5672/", "TEST_RS", null)
                    }
                },
                ServerQueues = new List<MQServerQueues>
                {
                    new MQServerQueues
                    {
                        EnvironmentName = "",
                        MachineName = "",
                        RecvQueues = new List<MQConnection> { new MQConnection("amqp://agsw:agsw@127.0.0.1:5672/", "TEST_RQ", null) }
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

            JsonTextSerializerExtensions.Serializer.Indent = true;

            mqConfig.SerializeToXmlFile("mqConfig.xml");
            mqConfig.SerializeToJsonFile("mqConfig.json");

            var manager = mqConfig.GetQueueManager();
            manager.CreateClientQueues();

            //Core.DebugMode = true;
            //Core.Log.MaxLogLevel = Diagnostics.Log.LogLevel.InfoDetail;

            Core.Log.Warning("Starting with Normal Listener and Client");
            await NormalTestAsync(mqConfig).ConfigureAwait(false);
            mqConfig.ResponseOptions.ClientReceiverOptions.Parameters["SingleResponseQueue"] = "true";
            Core.Log.Warning("Starting with RAW Listener and Client");
            await RawTestAsync(mqConfig).ConfigureAwait(false);
        }

        private static async Task NormalTestAsync(MQPairConfig mqConfig)
        {
            using (var mqServer = mqConfig.GetServer())
            {
                mqServer.RequestReceived += (s, e) =>
                {
                    e.SetResponseBody("Bienvenido!!!");
                    return Task.CompletedTask;
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
                            var response = await mqClient.SendAndReceiveAsync<string>("Hola mundo").ConfigureAwait(false);
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
                        await Task.WhenAll(
                            Enumerable.Range(0, totalQ).Select((i, mc) => (Task)mc.SendAndReceiveAsync<string>("Hola mundo"), mqClient).ToArray()
                        ).ConfigureAwait(false);

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
                    var totalQ = 1000;

                    #region Sync Mode
                    Core.Log.Warning("Sync Mode Test, using Multiple Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Sync Mode for {totalQ} times"))
                    {
                        for (var i = 0; i < totalQ; i++)
                        {
                            var response = await mqClient.SendAndReceiveAsync<string>("Hola mundo").ConfigureAwait(false);
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
                        await Task.WhenAll(
                            Enumerable.Range(0, totalQ).Select((i, mc) => (Task)mc.SendAndReceiveAsync<string>("Hola mundo"), mqClient).ToArray()
                        ).ConfigureAwait(false);
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

        private static async Task RawTestAsync(MQPairConfig mqConfig)
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
                            var response = await mqClient.SendAndReceiveAsync(byteRequest).ConfigureAwait(false);
                        }
                        Core.Log.InfoBasic("Total time: {0}", TimeSpan.FromMilliseconds(w.GlobalElapsedMilliseconds));
                        Core.Log.InfoBasic("Average time in ms: {0}. Press ENTER To Continue.", (w.GlobalElapsedMilliseconds / totalQ));
                    }
                    Console.ReadLine();
                    #endregion

                    #region Parallel Mode
                    Core.Log.Warning("RAW Parallel Mode Test, using Unique Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Parallel Mode for {totalQ} times"))
                    {
                        await Task.WhenAll(
                            Enumerable.Range(0, totalQ).Select((i, vTuple) => (Task)vTuple.mqClient.SendAndReceiveAsync(vTuple.byteRequest), (mqClient, byteRequest)).ToArray()
                        ).ConfigureAwait(false);
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

                mqConfig.ResponseOptions.ClientReceiverOptions.Parameters["SingleResponseQueue"] = "false";
                using (var mqClient = mqConfig.GetRawClient())
                {
                    var totalQ = 1000;

                    #region Sync Mode
                    Core.Log.Warning("RAW Sync Mode Test, using Multiple Response Queue");
                    using (var w = Watch.Create($"Hello World Example in Sync Mode for {totalQ} times"))
                    {
                        for (var i = 0; i < totalQ; i++)
                        {
                            var response = await mqClient.SendAndReceiveAsync(byteRequest).ConfigureAwait(false);
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
                        await Task.WhenAll(
                            Enumerable.Range(0, totalQ).Select((i, vTuple) => (Task)vTuple.mqClient.SendAndReceiveAsync(vTuple.byteRequest), (mqClient, byteRequest)).ToArray()
                        ).ConfigureAwait(false);
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