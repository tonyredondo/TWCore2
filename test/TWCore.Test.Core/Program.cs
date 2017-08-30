using System;
using TWCore.Diagnostics.Status.Transports;
using TWCore.Net.Multicast;

namespace TWCore.Test.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            TWCore.Core.DebugMode = true;
            TWCore.Core.RunOnInit(() => {
                TWCore.Core.Status.Transports.Add(new HttpStatusTransport(8089));


                DiscoveryService.RegisterService("CACHE", "CACHENAME", "DESCRIPTION", null);
                DiscoveryService.RegisterService("CACHE", "CACHENAME2", "DESCRIPTION", null);
                DiscoveryService.OnNewServiceReceived += DiscoveryService_OnServiceReceived;
                DiscoveryService.OnServiceExpired += DiscoveryService_OnServiceExpired;
                DiscoveryService.Connect("230.0.0.1", 28999);
                DiscoveryService.RegisterService("CACHE", "CACHENAME3", "DESCRIPTION", null);
                DiscoveryService.RegisterService("CACHE", "CACHENAME4", "DESCRIPTION", null);
            });
            TWCore.Core.StartContainer(args);
            Console.ReadLine();
        }

        private static void DiscoveryService_OnServiceExpired(object sender, EventArgs<DiscoveryService.ReceivedService> e)
        {
            Console.WriteLine("Service expired");
        }

        private static void DiscoveryService_OnServiceReceived(object sender, EventArgs<DiscoveryService.ReceivedService> e)
        {
            Console.WriteLine("Service received");
        }
        
    }
}