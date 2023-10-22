# <img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/icon.png" alt="Potato" width="45px" height="45px" /> TWCore 3.1
[![Build](https://github.com/tonyredondo/TWCore2/actions/workflows/build.yml/badge.svg)](https://github.com/tonyredondo/TWCore2/actions/workflows/build.yml)
[![Nuget](https://img.shields.io/nuget/vpre/TWCore.svg)](https://www.nuget.org/packages?q=Tags%3A"TWCore")

A multipurpose framework library used to create microservices solutions on multiple platforms (runs on windows, linux, linux-arm [raspberry pi], and osx).

### Compatible with
- .NET Framework 4.7.2
- netstandard2.0
- netcoreapp2.1
- netcoreapp2.2
- netcoreapp3.1
- net5.0
- net6.0
- net7.0
- net8.0

## Advantages

- Easy development of microservices: With fewer code you can subscribe to a message broker and start to process messages as pub/sub or RPC architecture.
- Completely agnostic and loosely coupled: All subsystems of the framework are registered with a dependency injector engine, so using only the configurations files you can switch between Messages brokers, Serialization formats, Rpc transports, Stream compressors, and many more.
- Integrated Log, Trace, and Status engines: you can log on different storage, also use differents trace storages and you have an integrated status library to check your service health.
- A bot subsystem to create bot services on Telegram and Slack
- Extensions methods for objects, strings, and IEnumerables.
- Aspnet Core object viewer and compiler developer tool.

#### Supported log storages
- Console
- File
- Html File
- MessageBroker
- ElasticSearch
- Mail

#### Supported trace storages
- File
- MessageBroker

#### Supported compressors
- Gzip
- Deflate
- Brotli

#### Supported messages brokers

- RabbitMQ
- NATS
- NSQ
- Kafka
- Redis (Pub/Sub)

#### Supported key/value cache storages

- Custom TWCore
- Redis

#### Supported bot connectors

- Slack
- Telegram

#### Supported serializers

- Newtonsoft JsonNet
- XmlSerializer
- BinaryFormatter
- Utf8Json
- MsgPack
- **NSerializer** (Custom binary serializer with object reference graph support and cyclic reference)
- **RawSerializer** (Like NSerializer but without internal caching, faster, but serialized objects are bigger in bytes size.)

---

### Simple Object Serializer Benchmark
```
BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.523 (1803/April2018Update/Redstone4)\
Intel Core i7-4770R CPU 3.20GHz (Haswell), 1 CPU, 8 logical and 4 physical cores\
.NET Core SDK=2.2.101\
  [Host] : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT\
  Core   : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT\
Job=Core  Runtime=Core
```
|                                Method | N |      Mean |     Error |    StdDev |       Min |       Max | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|-------------------------------------- |-- |----------:|----------:|----------:|----------:|----------:|------------:|------------:|------------:|--------------------:|
|      NewtonsoftJsonSerializerToStream | 1 | 42.935 us | 0.4296 us | 0.4019 us | 42.123 us | 43.608 us |      3.6621 |      0.1831 |           - |             23296 B |
|  NewtonsoftJsonDeserializerFromStream | 1 | 78.432 us | 1.5565 us | 1.4560 us | 76.154 us | 80.546 us |      3.5400 |      0.8545 |           - |             22960 B |
|                 XmlSerializerToStream | 1 | 45.512 us | 0.3721 us | 0.3299 us | 44.858 us | 46.154 us |      1.8921 |           - |           - |             12224 B |
|             XmlDeserializerFromStream | 1 | 91.809 us | 1.4537 us | 1.3598 us | 89.874 us | 94.327 us |      4.5166 |      0.3662 |           - |             28608 B |
|     BinaryFormatterSerializerToStream | 1 | 53.381 us | 0.5491 us | 0.5137 us | 52.418 us | 54.022 us |      2.5635 |           - |           - |             16504 B |
| BinaryFormatterDeserializerFromStream | 1 | 44.802 us | 0.5174 us | 0.4840 us | 43.914 us | 45.613 us |      3.6011 |      0.4883 |           - |             22720 B |
|            Utf8JsonSerializerToStream | 1 |  7.609 us | 0.1364 us | 0.1276 us |  7.429 us |  7.832 us |           - |           - |           - |                   - |
|        Utf8JsonDeserializerFromStream | 1 | 20.249 us | 0.2015 us | 0.1885 us | 20.009 us | 20.641 us |      0.9460 |      0.1831 |           - |              6016 B |
|         MessagePackSerializerToStream | 1 |  7.777 us | 0.0682 us | 0.0638 us |  7.658 us |  7.900 us |           - |           - |           - |                96 B |
|     MessagePackDeserializerFromStream | 1 | 11.087 us | 0.1592 us | 0.1489 us | 10.795 us | 11.419 us |      1.3123 |      0.3204 |           - |              8272 B |
|                   NSerializerToStream | 1 |  7.823 us | 0.1440 us | 0.1347 us |  7.607 us |  8.051 us |           - |           - |           - |                   - |
|               NDeserializerFromStream | 1 |  7.301 us | 0.0702 us | 0.0657 us |  7.106 us |  7.405 us |      0.4425 |      0.0458 |           - |              2800 B |
|                 RawSerializerToStream | 1 |  6.574 us | 0.1298 us | 0.1275 us |  6.375 us |  6.781 us |           - |           - |           - |                   - |
|             RawDeserializerFromStream | 1 |  7.837 us | 0.0655 us | 0.0613 us |  7.748 us |  7.937 us |      0.5569 |      0.0763 |           - |              3520 B |

![Serializer Graph](https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/serializer-deserializer1.png)

**Created by Daniel Redondo**


## Powered By
<img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/rider.jpg" alt="Rider" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotTrace.png" alt="dotTrace" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotMemory.png" alt="dotMemory" width="50px" height="50px" />

Thanks to @jetbrains for helping on this development with the licenses for Rider, dotTrace and dotMemory
