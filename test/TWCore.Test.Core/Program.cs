using TWCore.Diagnostics.Status.Transports;

namespace TWCore.Test.Core
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            TWCore.Core.DebugMode = true;
            TWCore.Core.RunOnInit(() => {
                TWCore.Core.Status.Transports.Add(new HttpStatusTransport(8089));
            });
            TWCore.Core.StartContainer(args);
        }
    }
}