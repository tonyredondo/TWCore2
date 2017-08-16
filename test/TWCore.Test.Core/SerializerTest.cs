using TWCore.Services;
using TWCore.Serialization;
using System.Text;
using System.Collections.Generic;
using TWCore.Serialization.PWSerializer.Deserializer;

namespace TWCore.Tests
{
    public class SerializerTest : ContainerParameterService
    {
        public SerializerTest() : base("serializertest", "PWSerializer Test") { }
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
                var colSTest = new List<STest>()
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
                    new STest2 { FirstName = "Person" , LastName = "Person" + i + "." + i+19, Age = 20, New = "This is a test" },
                };
                collection.Add(colSTest);
            }

            string jsonText = null;
            SubArray<byte> jsonBytes = null, pwBytes = null, wBytes = null;

            var lt = new List<STest>
            {
                new STest { FirstName = "Name1" , LastName = "LName1" , Age = 11 },
                new STest2 { FirstName = "Name2" , LastName = "LName2", Age = 20, New = "This is a test" },
            };

            var fByte = lt.SerializeToPWBinary();
            var obj1 = (DynamicDeserializedType)fByte.DeserializeFromPWBinary(null);

            pwBytes = collection[0].SerializeToPWBinary();
            var obj = (DynamicDeserializedType)pwBytes.DeserializeFromPWBinary(null);
            var lst = obj.GetObject<List<STest>>();
            var obj2 = pwBytes.DeserializeFromPWBinary<List<STest>>();

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            Factory.Thread.Sleep(1000);
            using (Watch.Create("JSON SERIALIZER"))
                for (var i = 0; i < 100000; i++)
                    jsonText = collection[i % 10000].SerializeToJson();
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            Factory.Thread.Sleep(1000);
            using (Watch.Create("JSON DESERIALIZER"))
                for (var i = 0; i < 100000; i++)
                    jsonText.DeserializeFromJson<List<STest>>();

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            Factory.Thread.Sleep(1000);
            using (Watch.Create("PWBinary SERIALIZER"))
                for (var i = 0; i < 100000; i++)
                    pwBytes = collection[i % 10000].SerializeToPWBinary();
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            Factory.Thread.Sleep(1000);
            using (Watch.Create("PWBinary DESERIALIZER"))
                for (var i = 0; i < 100000; i++)
                    pwBytes.DeserializeFromPWBinary<List<STest>>();
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            Factory.Thread.Sleep(1000);
            using (Watch.Create("PWBinary DESERIALIZER GENERIC"))
                for (var i = 0; i < 100000; i++)
                    pwBytes.DeserializeFromPWBinary(null);

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            Factory.Thread.Sleep(1000);
            using (Watch.Create("WBinary SERIALIZER"))
                for (var i = 0; i < 100000; i++)
                    wBytes = collection[i % 10000].SerializeToWBinary();
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            Factory.Thread.Sleep(1000);
            using (Watch.Create("WBinary DESERIALIZER"))
                for (var i = 0; i < 100000; i++)
                    wBytes.DeserializeFromWBinary<List<STest>>();

            jsonBytes = Encoding.UTF8.GetBytes(jsonText);

            Core.Log.InfoBasic("Json length: {0}", jsonBytes.Count);
            Core.Log.InfoBasic("Json: {0}", Encoding.UTF8.GetString(jsonBytes));
            Core.Log.InfoBasic("PWBytes: {0}", pwBytes.Count);
            Core.Log.InfoBasic("PW: {0}", Encoding.UTF8.GetString(pwBytes));
            Core.Log.InfoBasic("WBytes: {0}", wBytes.Count);
            Core.Log.InfoBasic("W: {0}", Encoding.UTF8.GetString(wBytes));
        }
    }

    public class STest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public STest Brother { get; set; }
    }
    public class STest2 : STest
    {
        public string New { get; set; }
    }
}