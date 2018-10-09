# TWCore 2.1
A multipurpose framework library for dotnet core 2.1, dotnet standard 2.0, net461 and net462 used to create microservices solutions on multiple platforms (runs on windows, linux, linux-arm [raspberry pi], and osx).

## Advantages

- Easy development of microservices: With fewer code you can subscribe to a message broker and start to process messages as pub/sub or RPC architecture.
- Completely agnostic and loosely coupled: All subsystems of the framework are registered with a dependency injector engine, so using only the configurations files you can switch between Messages brokers, Serialization formats, Rpc transports, Stream compressors, and many more.
- Integrated Log, Trace, and Status engines: you can log on different storage (currently: Console, File, HtmlFile), on different trace storages and you have an integrated status library to check your service health.
- A bot subsystem to create bot services on Telegram and Slack
- Extensions methods for objects, strings, and IEnumerables.
- Aspnet Core object viewer and compiler developer tool.
- Fast and fewer allocations.

## Supported messages brokers

- RabbitMQ
- NATS
- NSQ
- Kafka
- Redis (Pub/Sub)

## Supported key/value cache storages

- Custom TWCore
- Redis

## Supported bot connectors

- Slack
- Telegram

## Supported serializers

- Newtonsoft JsonNet
- XmlSerializer
- BinaryFormatter
- Utf8Json
- MsgPack
- NSerializer (Custom and faster serializer)
- RawSerializer (Like NSerializer without caching)


Created by Daniel Redondo



Thanks to @jetbrains for helping on this development with the licenses for Rider, dotTrace and dotMemory
