using System;
using System.Collections.Generic;
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

                //DiscoveryService.RegisterService("CACHE", "CACHENAME", "DESCRIPTION", null);
                //DiscoveryService.RegisterService("CACHE", "CACHENAME2", "DESCRIPTION", null);
                DiscoveryService.OnNewServiceReceived += DiscoveryService_OnServiceReceived;
                DiscoveryService.OnServiceExpired += DiscoveryService_OnServiceExpired;
                //DiscoveryService.Connect("230.0.0.1", 28999);
                //DiscoveryService.RegisterService("CACHE", "CACHENAME3", "DESCRIPTION", null);
                //DiscoveryService.RegisterService("CACHE", "CACHENAME4", "DESCRIPTION", null);
            });
            TWCore.Core.StartContainer(args);
            Console.ReadLine();
        }

        private static void DiscoveryService_OnServiceExpired(object sender, EventArgs<DiscoveryService.ReceivedService> e)
        {
            TWCore.Core.Log.InfoBasic("Core Service Discovery Remove: {0}, {1}, {2}, {3}, {4}, {5}", e.Item1.Category, e.Item1.Name, e.Item1.Description, e.Item1.ApplicationName, e.Item1.MachineName, e.Item1.Address);
        }

        private static void DiscoveryService_OnServiceReceived(object sender, EventArgs<DiscoveryService.ReceivedService> e)
        {
            TWCore.Core.Log.InfoBasic("Core Service Discovery Add: {0}, {1}, {2}, {3}, {4}, {5}", e.Item1.Category, e.Item1.Name, e.Item1.Description, e.Item1.ApplicationName, e.Item1.MachineName, e.Item1.Address);
            var value = e.Item1.Data.GetValue() as Dictionary<string, object>;
            if (value != null)
            {
                foreach (var item in value)
                    TWCore.Core.Log.InfoDetail("Param {0} = {1}", item.Key, item.Value);
            }
        }
        
    }
}