using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Diagnostics.Status.Transports;
using TWCore.Net.Multicast;
using TWCore.Services;

namespace TWCore.Test.Core
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("MAIN");
            TWCore.Core.DebugMode = true;
            TWCore.Core.RunOnInit(() => {
                TWCore.Core.Status.Transports.Add(new HttpStatusTransport(8089));
                TWCore.Core.Log.AddSimpleFileStorage("testlog.txt");
                TWCore.Core.Log.AddHtmlFileStorage("testlog.htm");
                DiscoveryService.OnNewServiceReceived += DiscoveryService_OnServiceReceived;
                DiscoveryService.OnServiceExpired += DiscoveryService_OnServiceExpired;
            });
            TWCore.Core.RunService<TestService>(args);
        }

        private static void DiscoveryService_OnServiceExpired(object sender, EventArgs<DiscoveryService.ReceivedService> e)
        {
            TWCore.Core.Log.InfoBasic("Core Service Discovery Remove: {0}, {1}, {2}, {3}, {4}, {5}", e.Item1.Category, e.Item1.Name, e.Item1.Description, e.Item1.ApplicationName, e.Item1.MachineName, e.Item1.Address);
        }

        private static void DiscoveryService_OnServiceReceived(object sender, EventArgs<DiscoveryService.ReceivedService> e)
        {
            TWCore.Core.Log.InfoBasic("Core Service Discovery Add: {0}, {1}, {2}, {3}, {4}, {5}", e.Item1.Category, e.Item1.Name, e.Item1.Description, e.Item1.ApplicationName, e.Item1.MachineName, e.Item1.Address);
            if (e.Item1.Data.GetValue() is Dictionary<string, object> value)
            {
                foreach (var item in value)
                    TWCore.Core.Log.InfoDetail("Param {0} = {1}", item.Key, item.Value);
            }
        }


        private class TestService : SimpleServiceAsync
        {
            protected override async Task OnActionAsync(CancellationToken token)
            {
                TWCore.Core.Log.InfoBasic("STARTING TEST SERVICE");
                await Task.Delay(10000, token);
                TWCore.Core.Log.InfoBasic("FINALIZING TEST SERVICE");
            }
        }
    }
}