using TWCore.Diagnostics.Status.Transports;

namespace TWCore.Test
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