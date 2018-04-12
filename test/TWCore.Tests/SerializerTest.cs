using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using TWCore.Serialization;
using TWCore.Serialization.NSerializer;
using TWCore.Serialization.PWSerializer.Deserializer;
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

            var sTest = new STest
            {
                FirstName = "Daniel",
                LastName = "Redondo",
                Age = 33
            };

            var collection = new List<List<STest>>();
            for (var i = 0; i <= 10000; i++)
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
                    new STest2 { FirstName = "Person" , LastName = "Person" + i + "." + i+19, Age = 20, New = "This is a test" }
                };
                collection.Add(colSTest);
            }


            var lt = new List<STest>
            {
                new STest { FirstName = "Name1" , LastName = "LName1" , Age = 11 },
                new STest2 { FirstName = "Name2" , LastName = "LName2", Age = 20, New = "This is a test" }
            };

            
            var memStream = new MemoryStream();
            
            Core.Log.InfoBasic("By size:");
            Core.Log.InfoBasic("\tJson Bytes Count: {0}", collection[0].SerializeToJsonBytes().Count.ToReadableBytes().Text);
            Core.Log.InfoBasic("\tMsgPack Bytes Count: {0}", collection[0].SerializeToMsgPack().Count.ToReadableBytes().Text);
            Core.Log.InfoBasic("\tBinary Formatter Bytes Count: {0}", collection[0].SerializeToBinFormatter().Count.ToReadableBytes().Text);
            Core.Log.InfoBasic("\tWBinary Bytes Count: {0}", collection[0].SerializeToWBinary().Count.ToReadableBytes().Text);
            Core.Log.InfoBasic("\tPortable WBinary Bytes Count: {0}", collection[0].SerializeToPWBinary().Count.ToReadableBytes().Text);

            var sertable = new Serialization.NSerializer.SerializersTable();
            sertable.Init();
            var mStream = new MemoryStream();
            sertable.SetStream(mStream);
            sertable.WriteObjectValue(collection[0]);
            sertable.Clear();
            mStream.Position = 0;

            using (Watch.Create("NSerializer SERIALIZER"))
            {
                for (var i = 0; i < 100000; i++)
                {
                    sertable.WriteObjectValue(collection[i % 10000]);
                    sertable.Clear();
                    mStream.Position = 0;
                }
            }
            Core.Log.InfoBasic("\tNSerializer Bytes Count: {0}", mStream.Length.ToReadableBytes().Text);
            Core.Log.InfoDetail(mStream.TextReadToEnd());
            Console.ReadLine();

            Core.Log.InfoBasic("By time (100000 times):");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(1000);
            using (Watch.Create("JSON SERIALIZER"))
            {
                for (var i = 0; i < 100000; i++)
                {
                    collection[i % 10000].SerializeToJson(memStream);
                    memStream.Position = 0;
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(1000);
            using (Watch.Create("JSON DESERIALIZER"))
            {
                for (var i = 0; i < 100000; i++)
                {
                    memStream.DeserializeFromJson<List<STest>>();
                    memStream.Position = 0;
                }
            }


            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(1000);
            using (Watch.Create("MESSAGEPACK SERIALIZER"))
            {
                for (var i = 0; i < 100000; i++)
                {
                    collection[i % 10000].SerializeToMsgPack(memStream);
                    memStream.Position = 0;
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(1000);
            using (Watch.Create("MESSAGEPACK DESERIALIZER"))
            {
                for (var i = 0; i < 100000; i++)
                {
                    memStream.DeserializeFromMsgPack<List<STest>>();
                    memStream.Position = 0;
                }
            }


            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //Thread.Sleep(1000);
            //using (Watch.Create("BINARY FORMATTER SERIALIZER"))
            //{
            //    for (var i = 0; i < 100000; i++)
            //    {
            //        collection[i % 10000].SerializeToBinFormatter(memStream);
            //        memStream.Position = 0;
            //    }
            //}
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //Thread.Sleep(1000);
            //using (Watch.Create("BINARY FORMATTER DESERIALIZER"))
            //{
            //    for (var i = 0; i < 100000; i++)
            //    {
            //        memStream.DeserializeFromBinFormatter<List<STest>>();
            //        memStream.Position = 0;
            //    }
            //}

            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(1000);
            using (Watch.Create("WBinary SERIALIZER"))
            {
                for (var i = 0; i < 100000; i++)
                {
                    collection[i % 10000].SerializeToWBinary(memStream);
                    memStream.Position = 0;
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(1000);
            using (Watch.Create("WBinary DESERIALIZER"))
            {
                for (var i = 0; i < 100000; i++)
                {
                    memStream.DeserializeFromWBinary<List<STest>>();
                    memStream.Position = 0;
                }
            }

            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(1000);
            using (Watch.Create("Portable WBinary SERIALIZER"))
            {
                for (var i = 0; i < 100000; i++)
                {
                    collection[i % 10000].SerializeToPWBinary(memStream);
                    memStream.Position = 0;
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(1000);
            using (Watch.Create("Portable WBinary DESERIALIZER"))
            {
                for (var i = 0; i < 100000; i++)
                {
                    memStream.DeserializeFromPWBinary<List<STest>>();
                    memStream.Position = 0;
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(1000);
            using (Watch.Create("Portable WBinary DESERIALIZER (WITH NO MODEL)"))
            {
                for (var i = 0; i < 100000; i++)
                {
                    memStream.DeserializeFromPWBinary(null);
                    memStream.Position = 0;
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(1000);
            using (Watch.Create("Object Cloner"))
            {
                for (var i = 0; i < 100000; i++)
                    collection[i % 10000].DeepClone();
            }

            Console.ReadLine();
        }
    }

    [Serializable]
    public class STest //: INSerializable
    {
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
    [Serializable]
    public class STest2 : STest//, INSerializable
    {
        public string New { get; set; }

        //void INSerializable.Serialize(SerializersTable table)
        //{
        //    table.WriteValue(FirstName);
        //    table.WriteValue(LastName);
        //    table.WriteValue(Age);
        //    table.WriteGenericValue(Brother);
        //    table.WriteValue(New);
        //}
    }
}