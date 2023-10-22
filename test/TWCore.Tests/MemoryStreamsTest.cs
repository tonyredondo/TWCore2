/*
Copyright 2015-2018 Daniel Adrian Redondo Suarez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TWCore.IO;
using TWCore.Services;
// ReSharper disable UnusedVariable
// ReSharper disable AccessToDisposedClosure
// ReSharper disable MethodSupportsCancellation
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    /// <inheritdoc />
#if NET5_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    public class MemoryStreamsTest : ContainerParameterService
    {
        public MemoryStreamsTest() : base("memtest", "Memory Streams test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting MEMORY STREAMS TEST");


            Core.Log.WriteEmptyLine();
            Core.Log.InfoBasic("Press Enter to Start RecycleMemoryStream Test.");
            Console.ReadLine();
            var xbuffer = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            using (Watch.Create("RecycleMemoryStream"))
            {
                for (var m = 0; m < 50000; m++)
                {
                    using (var rms = new RecycleMemoryStream())
                    {
                        for (var x = 0; x < 1; x++)
                        {
                            for (int i = 0; i < 10000; i++)
                                rms.Write(xbuffer, 0, xbuffer.Length);
                        }
                        rms.Position = 0;
                        for (var x = 0; x < 10; x++)
                        {
                            for (int i = 0; i < 2000; i++)
                            {
                                var bt = rms.ReadByte();
                            }
                        }
                    }
                }
            }
            Core.Log.WriteEmptyLine();
            Core.Log.InfoBasic("Press Enter to Start MemoryStream Test.");
            Console.ReadLine();
            using (Watch.Create("MemoryStream"))
            {
                for (var m = 0; m < 50000; m++)
                {
                    using (var rms = new MemoryStream())
                    {
                        for (var x = 0; x < 1; x++)
                        {
                            for (var i = 0; i < 10000; i++)
                                rms.Write(xbuffer, 0, xbuffer.Length);
                        }
                        rms.Position = 0;
                        for (var x = 0; x < 10; x++)
                        {
                            for (var i = 0; i < 2000; i++)
                            {
                                var bt = rms.ReadByte();
                            }
                        }
                    }
                }
            }



            Core.Log.WriteEmptyLine();
            Core.Log.InfoBasic("Press Enter to Start CircularBufferStream Test. Press Enter Again to finish the test.");
            Console.ReadLine();
            using (var cbs = new CircularBufferStream(50))
            {
                var cts = new CancellationTokenSource();
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
            using (var sharedms = new SharedMemoryStream("test", 2000))
            {
                var cts = new CancellationTokenSource();
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