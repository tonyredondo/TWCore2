using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using TWCore.IO;
using TWCore.Serialization.NSerializer;

namespace TWCore.Tests.Benchmark
{
    [ClrJob(baseline: true), CoreJob, MonoJob]
    [RPlotExporter, RankColumn, MinColumn, MaxColumn, MemoryDiagnoser]
    public class SerializersBench
    {
        private List<STest> _data;
        private MultiArray<byte> _binData;
        private NBinarySerializer _nBinary;
        private SerializersTable _nBinarySerTable;
        private DeserializersTable _nBinaryDSerTable;
        private MemoryStream _memStream;
        private RecycleMemoryStream _recmemStream;

        [Params(1, 100, 1000, 10000)]
        public int N;
        
        [GlobalSetup]
        public void Setup()
        {
            Core.InitDefaults();

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
            
            _nBinarySerTable = new SerializersTable();
            _memStream = new MemoryStream();
            _nBinarySerTable.Serialize(_memStream, _data);
            _memStream.Position = 0;
            
            _nBinaryDSerTable = new DeserializersTable();
            
            _recmemStream = new RecycleMemoryStream();
            _nBinarySerTable.Serialize(_recmemStream, _data);
            _recmemStream.Position = 0;
        }

        [Benchmark]
        public MultiArray<byte> Serialize()
        {
            var data = MultiArray<byte>.Empty;
            for(var i = 0; i < N; i ++)
                data = _nBinary.Serialize(_data);
            return data;
        }

        [Benchmark]
        public object Deserialize()
        {
            object data = null;
            for(var i = 0; i < N; i++)
                data = _nBinary.Deserialize<object>(_binData);
            return data;
        }
        
        [Benchmark]
        public void SerializerTable()
        {
            for (var i = 0; i < N; i++)
            {
                _nBinarySerTable.Serialize(_memStream, _data);
                _memStream.Position = 0;
            }
        }
        [Benchmark]
        public object DeserializerTable()
        {
            object data = null;
            for (var i = 0; i < N; i++)
            {
                _memStream.Position = 0;
                data = _nBinaryDSerTable.Deserialize(_memStream);
            }
            return data;
        }
        
        
        [Benchmark]
        public void RecycleSerializerTable()
        {
            for (var i = 0; i < N; i++)
            {
                _nBinarySerTable.Serialize(_recmemStream, _data);
                _recmemStream.Position = 0;
            }
        }
        [Benchmark]
        public object RecycleDeserializerTable()
        {
            object data = null;
            for (var i = 0; i < N; i++)
            {
                _recmemStream.Position = 0;
                data = _nBinaryDSerTable.Deserialize(_recmemStream);
            }
            return data;
        }
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