# <img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/icon.png" alt="Potato" width="45px" height="45px" /> TWCore 2.1
A multipurpose framework library for dotnet core 2.2, dotnet standard 2.0 and net461 used to create microservices solutions on multiple platforms (runs on windows, linux, linux-arm [raspberry pi], and osx).

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
- NSerializer (Custom and faster serializer)
- RawSerializer (Like NSerializer without caching)

#### Simple Object Serializer Benchmark

|                                  Method | N |       Mean |     Error |    StdDev |        Min |        Max | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------------- |-- |-----------:|----------:|----------:|-----------:|-----------:|------------:|------------:|------------:|--------------------:|
|                     NSerializerToStream | 1 |   8.141 us | 0.1624 us | 0.2430 us |   7.858 us |   8.749 us |           - |           - |           - |                   - |
|                 NDeserializerFromStream | 1 |   7.232 us | 0.0541 us | 0.0480 us |   7.170 us |   7.319 us |      0.4425 |      0.0458 |           - |              2800 B |
|                   RawSerializerToStream | 1 |   6.533 us | 0.0869 us | 0.0813 us |   6.445 us |   6.707 us |           - |           - |           - |                   - |
|               RawDeserializerFromStream | 1 |   7.830 us | 0.1105 us | 0.1033 us |   7.667 us |   7.992 us |      0.5493 |           - |           - |              3520 B |
|        NewtonsoftJsonSerializerToStream | 1 |  42.048 us | 0.3592 us | 0.3360 us |  41.308 us |  42.623 us |      3.6621 |      0.1831 |           - |             23296 B |
| NewtonsoftJsonRawDeserializerFromStream | 1 | 164.306 us | 1.5901 us | 1.4874 us | 162.131 us | 167.001 us |     14.8926 |      7.3242 |           - |             94864 B |
|              Utf8JsonSerializerToStream | 1 |   7.770 us | 0.1065 us | 0.0996 us |   7.675 us |   8.018 us |           - |           - |           - |                   - |
|       Utf8JsonRawDeserializerFromStream | 1 |  60.504 us | 0.4154 us | 0.3885 us |  59.908 us |  61.032 us |      5.3101 |      2.6245 |           - |             33568 B |


**Created by Daniel Redondo**


## Powered By
<img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/rider.jpg" alt="Rider" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotTrace.png" alt="dotTrace" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotMemory.png" alt="dotMemory" width="50px" height="50px" />

Thanks to @jetbrains for helping on this development with the licenses for Rider, dotTrace and dotMemory
