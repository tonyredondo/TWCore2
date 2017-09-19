using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TWCore.Services;

namespace TWCore.Object.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Core.InitAspNet();
            Core.RunService(() => new WebService(BuildWebHost), args);
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
