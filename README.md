# TWCore 2
A multipurpose framework library for netstandard 2, used to create microservices solutions on multiple platforms (runs on windows, linux, linux-arm [raspberry pi], and osx).

# Advantages

- Easy development of microservices: With fewer code you can subscribe to a message broker and start to process messages as pub/sub or RPC architecture.
- Completely agnostic and loosely coupled: All subsystems of the framework are registered with a dependency injector engine, so using only the configurations files you can switch between Messages brokers, Serialization formats, Rpc transports, Stream compressors, and many more.
- Integrated Log, Trace, and Status engines: you can log on different storage (currently: Console, File, HtmlFile), on different trace storages and you have an integrated status library to check your service health.
- A bot subsystem to create bot services on Telegram and Slack
- Extensions methods for objects, strings, and IEnumerables.
- Aspnet Core object viewer and compiler developer tool.
- Fast and fewer allocations.



Created by Daniel Redondo
