using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TWCore.Compression;
using TWCore.IO;
using TWCore.Messaging;
using TWCore.Reflection;
using TWCore.Serialization;
using TWCore.Serialization.MsgPack;
using TWCore.Serialization.NSerializer;
using TWCore.Serialization.PWSerializer;
using TWCore.Serialization.PWSerializer.Deserializer;
using TWCore.Serialization.RawSerializer;
using TWCore.Serialization.Utf8Json;
using TWCore.Serialization.WSerializer;
using TWCore.Services;
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedVariable

namespace TWCore.Tests
{
    /// <inheritdoc />
    public class SerializerTest : ContainerParameterService
    {
        public SerializerTest() : base("serializertest", "Serializer Test") { }
        protected override void OnHandler(ParameterHandlerInfo info)
        {
            Core.Log.Warning("Starting Serializer TEST");

            if (info.Arguments?.Contains("/complex") == true)
            {
                var file = "c:\\temp\\complexObject.nbin.deflate";
                var serializer = SerializerManager.GetByFileName(file);

                AssemblyResolverManager.RegisterDomain(new[] { @"C:\AGSW_GIT\Travel\src\Flights\Engines\Services\Agsw.Travel.Flights.Engines.Service\bin\Release\netcoreapp2.1" });
                //AssemblyResolverManager.RegisterDomain(new[] { @"C:\Repo\AgswGit\Travel\src\Flights\Engines\Services\Agsw.Travel.Flights.Engines.Service\bin\Release\netcoreapp2.1" });

                object value = null;
                try
                {
                    var rMsg = serializer.DeserializeFromFile<ResponseMessage>(file);
                    value = rMsg.Body.GetValue();
                }
                catch(DeserializerException exGO)
                {
                    var jsonSerializer = new JsonTextSerializer { Indent = true };
                    jsonSerializer.SerializeToFile(exGO.Value, "c:\\temp\\complexObject-GenericObject.json");

                    var val = exGO.Value["Products"][5];
                }
                

                RunTestEx(value, 200, null);
                GC.Collect();
                GC.WaitForFullGCComplete();
                RunTestEx(value, 200, new GZipCompressor());
                GC.Collect();
                GC.WaitForFullGCComplete();
                RunTestEx(value, 200, new DeflateCompressor());

                return;
            }

            var sTest = new STest
            {
                FirstName = "Daniel",
                LastName = "Redondo",
                Age = 33,
                value = 166
            };

            var collection = new List<List<STest>>();
            for (var i = 0; i <= 10; i++)
            {
                var colSTest = new List<STest>
                {
                    sTest,sTest,sTest,sTest,sTest,sTest,
                    sTest,sTest,sTest,sTest,sTest,sTest,
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+0, Age = 1 , Brother = sTest },
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+1, Age =2 , Brother = sTest },
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+2, Age = 3 , Brother = sTest },
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+3, Age = 4 , Brother = sTest },
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+4, Age = 5  , Brother = sTest},
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+5, Age = 6  , Brother = sTest},
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+6, Age = 7  , Brother = sTest},
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+7, Age = 8  , Brother = sTest},
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+8, Age = 9  , Brother = sTest},
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+9, Age = 10 , Brother = sTest },
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+10, Age = 11 },
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+11, Age = 12 },
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+12, Age = 13 },
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+13, Age = 14 },
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+14, Age = 15 },
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+15, Age = 16 },
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+16, Age = 17 },
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+17, Age = 18 },
                    new STest { FirstName = "Person" , LastName = "Person" + i + "." + i+18, Age = 19 },
                    //new STest2 { FirstName = "Person" , LastName = "Person" + i + "." + i+19, Age = 20, New = "This is a test" }
                };
                collection.Add(colSTest);
            }

            var lt = new List<STest>
            {
                new STest { FirstName = "Name1" , LastName = "LName1" , Age = 11 },
                //new STest2 { FirstName = "Name2" , LastName = "LName2", Age = 20, New = "This is a test" }
            };

            var lt2 = new List<Test3>
            {
                new Test3 { Values = new List<int> {2, 3, 4, 5} },
                new Test3 { Values = new List<int> {10, 11, 12, 13} }
            };

            var dct = new Dictionary<string, int>
            {
                ["Value1"] = 1,
                ["Value2"] = 2,
                ["Value3"] = 3,
            };

            var colClone = collection[0].DeepClone();
            var clone = collection.DeepClone();



            if (info.Arguments?.Contains("/parallel") == true)
            {
                RunSingleTest(collection[0], 2, false);
                Core.Log.InfoBasic("Press ENTER to Start:");
                Console.ReadLine();
                Task.WaitAll(
                    Enumerable.Range(0, 8).Select(i => Task.Run(() => RunSingleTest(collection[0], 200_000, false))).ToArray()
                );
                Console.ReadLine();
                return;
            }

            RunTest(collection[0], 200_000, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RunTest(object value, int times, bool useGZip)
        {
            var vType = value?.GetType() ?? typeof(object);
            var compressor = useGZip ? CompressorManager.GetByEncodingType("gzip") : null;
            var memStream = new RecycleMemoryStream();
            var jsonSerializer = new JsonTextSerializer { Compressor = compressor };
            var xmlSerializer = new XmlTextSerializer { Compressor = compressor };
            var binarySerializer = new BinaryFormatterSerializer { Compressor = compressor };
            var ut8JsonSerializer = new Utf8JsonTextSerializer { Compressor = compressor };
            var msgPackSerializer = new MsgPackSerializer { Compressor = compressor };
            var nBinarySerializer = new NBinarySerializer { Compressor = compressor };
            var rawBinarySerializer = new RawBinarySerializer { Compressor = compressor };
            var wBinarySerializer = new WBinarySerializer { Compressor = compressor };
            var pwBinarySerializer = new PWBinarySerializer { Compressor = compressor };

            Core.Log.Warning("Running Serializer Test. Use GZIP = {0}", useGZip);
            Core.Log.WriteEmptyLine();
            Core.Log.InfoBasic("By size:");
            Core.Log.InfoBasic("\tJson Bytes Count: {0}", SerializerSizeProcess(value, vType, jsonSerializer));
            Core.Log.InfoBasic("\tXml Bytes Count: {0}", SerializerSizeProcess(value, vType, xmlSerializer));
            Core.Log.InfoBasic("\tBinaryFormatter Bytes Count: {0}", SerializerSizeProcess(value, vType, binarySerializer));
            Core.Log.InfoBasic("\tUtf8Json Bytes Count: {0}", SerializerSizeProcess(value, vType, ut8JsonSerializer));
            Core.Log.InfoBasic("\tMessagePack Bytes Count: {0}", SerializerSizeProcess(value, vType, msgPackSerializer));
            Core.Log.InfoBasic("\tNBinary Bytes Count: {0}", SerializerSizeProcess(value, vType, nBinarySerializer));
            Core.Log.InfoBasic("\tRawBinary Bytes Count: {0}", SerializerSizeProcess(value, vType, rawBinarySerializer));
            Core.Log.InfoBasic("\tWBinary Bytes Count: {0}", SerializerSizeProcess(value, vType, wBinarySerializer));
            Core.Log.InfoBasic("\tPortable WBinary Bytes Count: {0}", SerializerSizeProcess(value, vType, pwBinarySerializer));
            Core.Log.WriteEmptyLine();
            Core.Log.InfoBasic("By Times: {0}", times);
            SerializerProcess("Json", value, vType, times, jsonSerializer, memStream);
            SerializerProcess("Utf8Json", value, vType, times, ut8JsonSerializer, memStream);
            SerializerProcess("NBinary", value, vType, times, nBinarySerializer, memStream);
            SerializerProcess("RawBinary", value, vType, times, rawBinarySerializer, memStream);
            SerializerProcess("WBinary", value, vType, times, wBinarySerializer, memStream);
            SerializerProcess("PWBinary", value, vType, times, pwBinarySerializer, memStream);
            Console.ReadLine();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RunTestEx(object value, int times, ICompressor compressor)
        {
            var vType = value?.GetType() ?? typeof(object);
            //var memStream = new MemoryStream();
            var memStream = new RecycleMemoryStream();
            var nBinarySerializer = new NBinarySerializer { Compressor = compressor };
            var rawBinarySerializer = new RawBinarySerializer { Compressor = compressor };
            var wBinarySerializer = new WBinarySerializer { Compressor = compressor };

            Core.Log.Warning("Running Serializer Test. Use Compressor = {0}", compressor?.EncodingType ?? "(no)");
            Core.Log.WriteEmptyLine();
            Core.Log.InfoBasic("By size:");
            Core.Log.InfoBasic("\tNBinary Bytes Count: {0}", SerializerSizeProcess(value, vType, nBinarySerializer));
            Core.Log.InfoBasic("\tRawBinary Bytes Count: {0}", SerializerSizeProcess(value, vType, rawBinarySerializer));
            Core.Log.InfoBasic("\tWBinary Bytes Count: {0}", SerializerSizeProcess(value, vType, wBinarySerializer));
            Core.Log.WriteEmptyLine();
            Console.ReadLine();
            Core.Log.InfoBasic("By Times: {0}", times);
            SerializerProcess("NBinary", value, vType, times, nBinarySerializer, memStream);
            SerializerProcess("RawBinary", value, vType, times, rawBinarySerializer, memStream);
            SerializerProcess("WBinary", value, vType, times, wBinarySerializer, memStream);
            Console.ReadLine();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RunSingleTest(object value, int times, bool useGZip)
        {
            var vType = value?.GetType() ?? typeof(object);
            var memStream = new RecycleMemoryStream();
            var compressor = useGZip ? CompressorManager.GetByEncodingType("gzip") : null;
            var nBinarySerializer = new NBinarySerializer { Compressor = compressor };
            Core.Log.Warning("Running Serializer Test. Use GZIP = {0}, Times = {1}", useGZip, times);
            SerializerProcess("NBinary", value, vType, times, nBinarySerializer, memStream, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string SerializerSizeProcess(object value, Type valueType, ISerializer serializer)
        {
            var memStream = new MemoryStream();
            serializer.Serialize(value, valueType, memStream);
            memStream.Position = 0;
            var obj = serializer.Deserialize(memStream, valueType);
            return memStream.Length.ToString();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SerializerProcess(string name, object value, Type valueType, int times, ISerializer serializer, Stream memStream, bool sleep = true)
        {
            double totalValue;
            memStream.Position = 0;
            if (sleep)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(1500);
            }
            using (var w = Watch.Create(name + " SERIALIZER"))
            {
                for (var i = 0; i < times; i++)
                {
                    serializer.Serialize(value, valueType, memStream);
                    memStream.Position = 0;
                }
                totalValue = w.GlobalElapsedMilliseconds;
            }
            Core.Log.InfoBasic("\t" + name + " SERIALIZER - Average Time: {0}ms", totalValue / times);
            if (sleep)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(1500);
            }
            using (var w = Watch.Create(name + " DESERIALIZER"))
            {
                for (var i = 0; i < times; i++)
                {
                    serializer.Deserialize(memStream, valueType);
                    memStream.Position = 0;
                }
                totalValue = w.GlobalElapsedMilliseconds;
            }
            Core.Log.InfoBasic("\t"+ name + " DESERIALIZER - Average Time: {0}ms", totalValue / times);
            if (sleep)
                Thread.Sleep(1000);
            Core.Log.WriteEmptyLine();
        }
    }

    [Serializable]
    public sealed class STest //: INSerializable
    {
        public int value;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public STest Brother { get; set; }

        //void INSerializable.Serialize(SerializersTable table)
        //{
        //    table.WriteValue(FirstName);
        //    table.WriteValue(LastName);
        //    table.WriteValue(Age);
        //    table.WriteGenericValue(Brother);
        //}
    }
    //[Serializable]
    //public class STest2 : STest//, INSerializable
    //{
    //    public string New { get; set; }

    //    //void INSerializable.Serialize(SerializersTable table)
    //    //{
    //    //    table.WriteValue(FirstName);
    //    //    table.WriteValue(LastName);
    //    //    table.WriteValue(Age);
    //    //    table.WriteGenericValue(Brother);
    //    //    table.WriteValue(New);
    //    //}
    //}

    public sealed class Test3
    {
        public List<int> Values { get; set; }
    }
}
