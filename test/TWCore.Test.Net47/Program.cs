using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TWCore;
using TWCore.Diagnostics.Status.Transports;

namespace TWCore.Test.Net47
{
    class Program
    {
        static void Main(string[] args)
        {
            Core.DebugMode = true;
            Core.RunOnInit(() => {
                Core.Status.Transports.Add(new HttpStatusTransport(8089));
            });
            Core.StartContainer(args);
        }
    }
}
