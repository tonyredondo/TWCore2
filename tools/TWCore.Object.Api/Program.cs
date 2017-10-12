using TWCore.Services;
// ReSharper disable ClassNeverInstantiated.Global

namespace TWCore.Object.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Core.InitAspNet();
            Core.RunService(() => WebService.Create<Startup>(), args);
        }
    }
}
