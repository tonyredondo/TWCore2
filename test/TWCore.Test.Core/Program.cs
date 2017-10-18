using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using TWCore.Collections;
using TWCore.Diagnostics.Status.Transports;
using TWCore.Injector;
using TWCore.Net.Multicast;
using TWCore.Net.RPC.Client.Transports.Default;
using TWCore.Net.RPC.Server.Transports.Default;
using TWCore.Serialization;
using TWCore.Serialization.WSerializer;
using TWCore.Services;
using TWCore.Threading;
using AsyncManualResetEvent = Nito.AsyncEx.AsyncManualResetEvent;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedVariable

namespace TWCore.Test.Core
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("MAIN");
            TWCore.Core.DebugMode = true;
            TWCore.Core.RunOnInit(async () =>
            {
                TWCore.Core.Status.Transports.Add(new HttpStatusTransport(8089));
                TWCore.Core.Log.AddSimpleFileStorage("testlog.txt");
                TWCore.Core.Log.AddHtmlFileStorage("testlog.htm");
                DiscoveryService.OnNewServiceReceived += DiscoveryService_OnServiceReceived;
                DiscoveryService.OnServiceExpired += DiscoveryService_OnServiceExpired;
                //DiscoveryService.OnServiceReceived += DiscoveryService_OnServiceReceived;

                /*
                var cSource = new CancellationTokenSource();
                var lstServerClients = new List<RpcServerClient>();
                Task.Run(async () =>
                {
                    var listener = new TcpListener(IPAddress.Any, 8081);
                    listener.Server.NoDelay = true;
                    listener.Server.ReceiveBufferSize = 32768;
                    listener.Server.SendBufferSize = 32768;
                    Factory.SetSocketLoopbackFastPath(listener.Server);
                    listener.Start();


                    while (!cSource.IsCancellationRequested)
                    {
                        var tcpClient = await listener.AcceptTcpClientAsync().ConfigureAwait(false);

                        ThreadPool.QueueUserWorkItem(objClient =>
                        {
                            var server = new RpcServerClient((TcpClient)objClient, new WBinarySerializer());
                            server.OnMessageReceived += (sender, message) =>
                            {
                                Console.WriteLine("Server message received: {0}", message);
                                
                            };

                            lstServerClients.Add(server);

                        }, tcpClient);
                    }
                    listener.Stop();

                }, cSource.Token);

                var client = new RpcClient("127.0.0.1", 8081, new WBinarySerializer());
                client.OnConnect += (sender, eventArgs) =>
                {
                    Console.WriteLine("Socket On Session");
                };
                client.OnDisconnect += (sender, eventArgs) =>
                {
                    Console.WriteLine("Socket Disconnected");

                };
                client.OnMessageReceived += (sender, message) =>
                {
                    Console.WriteLine("Message Received: {0}", message);
                };
                await client.ConnectAsync().ConfigureAwait(false);
                Console.ReadLine();
                await client.DisconnectAsync().ConfigureAwait(false);
                Console.ReadLine();
                await client.ConnectAsync().ConfigureAwait(false);
                Console.ReadLine();

                cSource.Cancel();
                */
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
                TWCore.Core.Injector.Settings.Interfaces.Add(new NonInstantiable
                {
                    Type = "TWCore.ICoreStart, TWCore",
                    ClassDefinitions = new NameCollection<Instantiable>
                    {
                        new Instantiable { Name = "C1", Type = "TWCore.CoreStart, TWCore", Singleton = true }
                    }
                });
                var value = TWCore.Core.Injector.New<ICoreStart>("C1");

                var cd = new ConcurrentDictionary<string, string>();
                cd.GetOrAdd(string.Empty, k =>
                {
                    TWCore.Core.Log.Warning("HOLA MUNDO!!!");
                    return string.Empty;
                });
                //**
                await Task.Delay(10000, token);
                TWCore.Core.Log.InfoBasic("FINALIZING TEST SERVICE");
            }
        }
    }
}