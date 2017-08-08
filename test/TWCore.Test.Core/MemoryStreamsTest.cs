using System;
using System.Threading;
using System.Threading.Tasks;
using TWCore.IO;
using TWCore.Security;
using TWCore.Services;

namespace TWCore.Tests
{
    public class MemoryStreamsTest : ContainerParameterService
    {
        public MemoryStreamsTest() : base("memtest", "Memory Streams test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting MEMORY STREAMS TEST");

            Core.Log.WriteEmptyLine();
            Core.Log.InfoBasic("Press Enter to Start CircularBufferStream Test. Press Enter Again to finish the test.");
            Console.ReadLine();
            using (var cbs = new CircularBufferStream(50))
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                Task.Run(async () =>
                {
                    var i = 0;
                    while (!cts.Token.IsCancellationRequested)
                    {
                        i++;
                        cbs.WriteBytes(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });
                        if (i % 50 == 0)
                            Core.Log.InfoMedium("Write {0}", i);
                        await Task.Delay(1, cts.Token).ConfigureAwait(false);
                    }
                });
                Task.Run(async () =>
                {
                    var i = 0;
                    var buffer = new byte[15];
                    while (!cts.Token.IsCancellationRequested)
                    {
                        i++;
                        cbs.Read(buffer, 0, 7);
                        if (i % 50 == 0)
                            Core.Log.InfoMedium("Read {0}", i);
                        await Task.Delay(1, cts.Token).ConfigureAwait(false);
                    }
                });
                Console.ReadLine();
                cts.Cancel();
            }


            Core.Log.WriteEmptyLine();
            Core.Log.InfoBasic("Press Enter to Start SharedMemoryStream Test. Press Enter Again to finish the test.");
            Console.ReadLine();
            using (var sharedms = new SharedMemoryStream("TEST", 2000))
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                Task.Run(async () =>
                {
                    var i = 0;
                    while (!cts.Token.IsCancellationRequested)
                        {
                        i++;
                        sharedms.WriteBytes(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 });
                        if (i % 50 == 0)
                            Core.Log.InfoMedium("Write {0}", i);
                        await Task.Delay(1, cts.Token).ConfigureAwait(false);
                    }
                });
                Task.Run(async () =>
                {
                    var i = 0;
                    var buffer = new byte[15];
                    while (!cts.Token.IsCancellationRequested)
                        {
                        i++;
                        sharedms.Read(buffer, 0, 7);
                        if (i % 50 == 0)
                            Core.Log.InfoMedium("Read {0}", i);
                        await Task.Delay(1, cts.Token).ConfigureAwait(false);
                    }
                });
                Console.ReadLine();
            }
        }
    }
}