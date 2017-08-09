using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TWCore;
using TWCore.Diagnostics.Status.Transports;
using TWCore.IO;
using System.Threading;

namespace TWCore.Test.Net47
{
    class Program
    {
        static void Main(string[] args)
        {
            Core.DebugMode = true;
            Core.RunOnInit(() =>
            {
                Core.Status.Transports.Add(new HttpStatusTransport(8089));

                var buffer = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

                for (var m = 0; m < 200000; m++)
                {
                    //Console.WriteLine("TEST " + m);
                    using (var rms = new RecycleMemoryStream())
                    //using (var rms = new MemoryStream())
                    {
                        for (var x = 0; x < 1; x++)
                        {
                            for (int i = 0; i < 1000; i++)
                                rms.Write(buffer, 0, buffer.Length);
                        }
                        rms.Position = 0;
                        for (var x = 0; x < 10; x++)
                        {
                            //Console.WriteLine();
                            for (byte i = 0; i < 200; i++)
                            {
                                var bt = rms.ReadByte();
                                //Console.Write(bt + ", ");
                            }
                            //Console.WriteLine();
                        }
                    }
                }

                Console.ReadLine();
            });
            Core.StartContainer(args);
        }
    }
}
