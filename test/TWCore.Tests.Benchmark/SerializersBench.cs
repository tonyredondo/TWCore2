using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using TWCore.IO;
using TWCore.Serialization;
using TWCore.Serialization.NSerializer;
using TWCore.Serialization.RawSerializer;
using TWCore.Serialization.Utf8Json;

namespace TWCore.Tests.Benchmark
{
    [CoreJob(baseline: true)]
    [RPlotExporter, RankColumn, MinColumn, MaxColumn, MemoryDiagnoser]
    public class SerializersBench
    {
        private List<STest> _data;
        private MultiArray<byte> _binData;
        //
        private NBinarySerializer _nBinary;
        private RawBinarySerializer _rawBinary;
        private JsonTextSerializer _jsonSerializer;
        private Utf8JsonTextSerializer _utf8jsonSerializer;
        //
        private MemoryStream _nBinaryStream;
        private MemoryStream _rawBinaryStream;
        private MemoryStream _jsonSerializerStream;
        private MemoryStream _utf8jsonSerializerStream;

        [Params(1)]
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
            _rawBinary = new RawBinarySerializer();
            _jsonSerializer = new JsonTextSerializer();
            _utf8jsonSerializer = new Utf8JsonTextSerializer();

            _binData = _nBinary.Serialize(_data);
            _binData = _rawBinary.Serialize(_data);
            _binData = _jsonSerializer.Serialize(_data);
            _binData = _utf8jsonSerializer.Serialize(_data);

            _nBinaryStream = new MemoryStream();
            _rawBinaryStream = new MemoryStream();
            _jsonSerializerStream = new MemoryStream();
            _utf8jsonSerializerStream = new MemoryStream();

            _nBinary.Serialize(_data, _nBinaryStream);
            _rawBinary.Serialize(_data, _rawBinaryStream);
            _jsonSerializer.Serialize(_data, _jsonSerializerStream);
            _utf8jsonSerializer.Serialize(_data, _utf8jsonSerializerStream);
        }

        [Benchmark]
        public void NSerializerToStream()
        {
            for (var i = 0; i < N; i++)
            {
                _nBinary.Serialize(_data, _nBinaryStream);
                _nBinaryStream.Position = 0;
            }
        }
        [Benchmark]
        public object NDeserializerFromStream()
        {
            object data = null;
            for (var i = 0; i < N; i++)
            {
                _nBinaryStream.Position = 0;
                data = _nBinary.Deserialize<object>(_nBinaryStream);
            }
            return data;
        }
        //
        [Benchmark]
        public void RawSerializerToStream()
        {
            for (var i = 0; i < N; i++)
            {
                _rawBinary.Serialize(_data, _rawBinaryStream);
                _rawBinaryStream.Position = 0;
            }
        }
        [Benchmark]
        public object RawDeserializerFromStream()
        {
            object data = null;
            for (var i = 0; i < N; i++)
            {
                _rawBinaryStream.Position = 0;
                data = _rawBinary.Deserialize<object>(_rawBinaryStream);
            }
            return data;
        }

        //
        [Benchmark]
        public void NewtonsoftJsonSerializerToStream()
        {
            for (var i = 0; i < N; i++)
            {
                _jsonSerializer.Serialize(_data, _jsonSerializerStream);
                _jsonSerializerStream.Position = 0;
            }
        }
        [Benchmark]
        public object NewtonsoftJsonRawDeserializerFromStream()
        {
            object data = null;
            for (var i = 0; i < N; i++)
            {
                _jsonSerializerStream.Position = 0;
                data = _jsonSerializer.Deserialize<object>(_jsonSerializerStream);
            }
            return data;
        }
        //
        [Benchmark]
        public void Utf8JsonSerializerToStream()
        {
            for (var i = 0; i < N; i++)
            {
                _utf8jsonSerializer.Serialize(_data, _utf8jsonSerializerStream);
                _utf8jsonSerializerStream.Position = 0;
            }
        }
        [Benchmark]
        public object Utf8JsonRawDeserializerFromStream()
        {
            object data = null;
            for (var i = 0; i < N; i++)
            {
                _utf8jsonSerializerStream.Position = 0;
                data = _utf8jsonSerializer.Deserialize<object>(_utf8jsonSerializerStream);
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