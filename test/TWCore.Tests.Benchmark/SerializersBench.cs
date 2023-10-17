using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TWCore.IO;
using TWCore.Serialization;
using TWCore.Serialization.NSerializer;
using TWCore.Serialization.RawSerializer;
using TWCore.Serialization.Utf8Json;
using TWCore.Serialization.MsgPack;

namespace TWCore.Tests.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net70)]
    [RPlotExporter, RankColumn, MinColumn, MaxColumn, MemoryDiagnoser]
    public class SerializersBench
    {
        private List<STest> _data;
        private MultiArray<byte> _binData;
        //
        private JsonTextSerializer _jsonSerializer;
        private XmlTextSerializer _xmlSerializer;
        private BinaryFormatterSerializer _binSerializer;
        private Utf8JsonTextSerializer _utf8jsonSerializer;
        private MsgPackSerializer _msgPackSerializer;
        private NBinarySerializer _nBinary;
        private RawBinarySerializer _rawBinary;
        //
        private MemoryStream _jsonSerializerStream;
        private MemoryStream _xmlSerializerStream;
        private MemoryStream _binSerializerStream;
        private MemoryStream _utf8jsonSerializerStream;
        private MemoryStream _msgPackSerializerStream;
        private MemoryStream _nBinaryStream;
        private MemoryStream _rawBinaryStream;

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

            _jsonSerializer = new JsonTextSerializer();
            _binData = _jsonSerializer.Serialize(_data);
            _jsonSerializerStream = new MemoryStream();
            _jsonSerializer.Serialize(_data, _jsonSerializerStream);

            _xmlSerializer = new XmlTextSerializer();
            _binData = _xmlSerializer.Serialize(_data);
            _xmlSerializerStream = new MemoryStream();
            _xmlSerializer.Serialize(_data, _xmlSerializerStream);

            _binSerializer = new BinaryFormatterSerializer();
            _binData = _binSerializer.Serialize(_data);
            _binSerializerStream = new MemoryStream();
            _binSerializer.Serialize(_data, _binSerializerStream);

            _utf8jsonSerializer = new Utf8JsonTextSerializer();
            _binData = _utf8jsonSerializer.Serialize(_data);
            _utf8jsonSerializerStream = new MemoryStream();
            _utf8jsonSerializer.Serialize(_data, _utf8jsonSerializerStream);

            _msgPackSerializer = new MsgPackSerializer();
            _binData = _msgPackSerializer.Serialize(_data);
            _msgPackSerializerStream = new MemoryStream();
            _msgPackSerializer.Serialize(_data, _msgPackSerializerStream);

            _nBinary = new NBinarySerializer();
            _binData = _nBinary.Serialize(_data);
            _nBinaryStream = new MemoryStream();
            _nBinary.Serialize(_data, _nBinaryStream);

            _rawBinary = new RawBinarySerializer();
            _binData = _rawBinary.Serialize(_data);
            _rawBinaryStream = new MemoryStream();
            _rawBinary.Serialize(_data, _rawBinaryStream);
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
        public List<STest> NewtonsoftJsonDeserializerFromStream()
        {
            List<STest> data = null;
            for (var i = 0; i < N; i++)
            {
                _jsonSerializerStream.Position = 0;
                data = _jsonSerializer.Deserialize<List<STest>>(_jsonSerializerStream);
            }
            return data;
        }
        
        //
        [Benchmark]
        public void XmlSerializerToStream()
        {
            for (var i = 0; i < N; i++)
            {
                _xmlSerializer.Serialize(_data, _xmlSerializerStream);
                _xmlSerializerStream.Position = 0;
            }
        }
        [Benchmark]
        public List<STest> XmlDeserializerFromStream()
        {
            List<STest> data = null;
            for (var i = 0; i < N; i++)
            {
                _xmlSerializerStream.Position = 0;
                data = _xmlSerializer.Deserialize<List<STest>>(_xmlSerializerStream);
            }
            return data;
        }

        //
        [Benchmark]
        public void BinaryFormatterSerializerToStream()
        {
            for (var i = 0; i < N; i++)
            {
                _binSerializer.Serialize(_data, _binSerializerStream);
                _binSerializerStream.Position = 0;
            }
        }
        [Benchmark]
        public List<STest> BinaryFormatterDeserializerFromStream()
        {
            List<STest> data = null;
            for (var i = 0; i < N; i++)
            {
                _binSerializerStream.Position = 0;
                data = _binSerializer.Deserialize<List<STest>>(_binSerializerStream);
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
        public List<STest> Utf8JsonDeserializerFromStream()
        {
            List<STest> data = null;
            for (var i = 0; i < N; i++)
            {
                _utf8jsonSerializerStream.Position = 0;
                data = _utf8jsonSerializer.Deserialize<List<STest>>(_utf8jsonSerializerStream);
            }
            return data;
        }

        //
        [Benchmark]
        public void MessagePackSerializerToStream()
        {
            for (var i = 0; i < N; i++)
            {
                _msgPackSerializer.Serialize(_data, _msgPackSerializerStream);
                _msgPackSerializerStream.Position = 0;
            }
        }
        [Benchmark]
        public List<STest> MessagePackDeserializerFromStream()
        {
            List<STest> data = null;
            for (var i = 0; i < N; i++)
            {
                _msgPackSerializerStream.Position = 0;
                data = _msgPackSerializer.Deserialize<List<STest>>(_msgPackSerializerStream);
            }
            return data;
        }

        //
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
        public List<STest> NDeserializerFromStream()
        {
            List<STest> data = null;
            for (var i = 0; i < N; i++)
            {
                _nBinaryStream.Position = 0;
                data = _nBinary.Deserialize<List<STest>>(_nBinaryStream);
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
        public List<STest> RawDeserializerFromStream()
        {
            List<STest> data = null;
            for (var i = 0; i < N; i++)
            {
                _rawBinaryStream.Position = 0;
                data = _rawBinary.Deserialize<List<STest>>(_rawBinaryStream);
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