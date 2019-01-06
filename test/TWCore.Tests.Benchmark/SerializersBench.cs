using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using TWCore.Serialization.NSerializer;

namespace TWCore.Tests.Benchmark
{
    [ClrJob(baseline: true), CoreJob, MonoJob]
    [RPlotExporter, RankColumn]
    public class SerializersBench
    {
        private List<STest> _data;
        private MultiArray<byte> _binData;
        private NBinarySerializer _nBinary;

        [Params(1000, 10000, 100000)]
        public int N;
        
        [GlobalSetup]
        public void Setup()
        {
            var sTest = new STest
            {
                FirstName = "Daniel",
                LastName = "Redondo",
                Age = 35,
                value = 166
            };
            
            _data = new List<STest>
                {
                    sTest,sTest,sTest,sTest,sTest,sTest,
                    sTest,sTest,sTest,sTest,sTest,sTest,
                    new STest { FirstName = "Person" , LastName = "Person" + "." +0, Age = 1 , Brother = sTest },
                    new STest { FirstName = "Person" , LastName = "Person" + "." +1, Age =2 , Brother = sTest },
                    new STest { FirstName = "Person" , LastName = "Person" + "." +2, Age = 3 , Brother = sTest },
                    new STest { FirstName = "Person" , LastName = "Person" + "." +3, Age = 4 , Brother = sTest },
                    new STest { FirstName = "Person" , LastName = "Person" + "." +4, Age = 5  , Brother = sTest},
                    new STest { FirstName = "Person" , LastName = "Person" + "." +5, Age = 6  , Brother = sTest},
                    new STest { FirstName = "Person" , LastName = "Person" + "." +6, Age = 7  , Brother = sTest},
                    new STest { FirstName = "Person" , LastName = "Person" + "." +7, Age = 8  , Brother = sTest},
                    new STest { FirstName = "Person" , LastName = "Person" + "." +8, Age = 9  , Brother = sTest},
                    new STest { FirstName = "Person" , LastName = "Person" + "." +9, Age = 10 , Brother = sTest },
                    new STest { FirstName = "Person" , LastName = "Person" + "." +10, Age = 11 },
                    new STest { FirstName = "Person" , LastName = "Person" + "." +11, Age = 12 },
                    new STest { FirstName = "Person" , LastName = "Person" + "." +12, Age = 13 },
                    new STest { FirstName = "Person" , LastName = "Person" + "." +13, Age = 14 },
                    new STest { FirstName = "Person" , LastName = "Person" + "." +14, Age = 15 },
                    new STest { FirstName = "Person" , LastName = "Person" + "." +15, Age = 16 },
                    new STest { FirstName = "Person" , LastName = "Person" + "." +16, Age = 17 },
                    new STest { FirstName = "Person" , LastName = "Person" + "." +17, Age = 18 },
                    new STest { FirstName = "Person" , LastName = "Person" + "." +18, Age = 19 },
                };
            _nBinary = new NBinarySerializer();

            _binData = _nBinary.Serialize(_data);
        }

        [Benchmark]
        public MultiArray<byte> Serialize() => _nBinary.Serialize(_data);
        
        [Benchmark]
        public object Deserialize() => _nBinary.Deserialize<object>(_binData);
    }
    
    [Serializable]
    public sealed class STest
    {
        public int value;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public STest Brother { get; set; }
    }
}