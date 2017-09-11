using TWCore.Cache.Client.Configuration;
using TWCore.Serialization;
using TWCore.Services;
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    public class DefaultCacheTest : ContainerParameterService
    {
        public DefaultCacheTest() : base("defaultcachetest", "Default Cache Test") { }

        protected override void OnHandler(ParameterHandlerInfo info)
        {
            var settings = Config.DeserializeFromXml<CacheSettings>();
            Core.Services.SetDefaultCacheClientSettings(settings);

            var cacheClient = Core.Services.GetCacheClient("Agsw.Services.Cache");

            var keys = cacheClient.GetKeys();

            Core.Log.InfoBasic("Keys: {0}", keys.Length);
        }


        private const string Config = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<CacheSettings>
  <Cache Name=""Agsw.Services.Cache"">
    <ClientOptions EnvironmentName="""" MachineName="""">
      <Pool SerializerMimeType=""application/w-binary"" CompressorEncoding=""gzip"" PingDelay=""30000"" PingDelayOnError=""60000"" ReadMode=""NormalRead"" WriteMode=""WritesFirstAndThenAsync"" WriteNetworkItemsToMemoryOnGet=""true"" ForceAtLeastOneNetworkItemEnabled=""true"">
        <PoolItem Mode=""ReadAndWrite"" TypeFactory=""TWCore.Net.RPC.Client.Transports.TWTransportClientFactory, TWCore.Net.Rpc"" Enabled=""true"">
          <Param Key=""Host"" Value=""10.10.1.17"" />
          <Param Key=""Port"" Value=""27903"" />
          <Param Key=""SocketsPerClient"" Value=""3"" />
          <Param Key=""SerializerMimeType"" Value=""application/w-binary"" />
        </PoolItem>
        <PoolItem Mode=""ReadAndWrite"" InMemoryStorage=""true"" TypeFactory=""TWCore.Cache.Configuration.MemoryStorageFactory, TWCore.Cache"" Enabled=""true"">
          <Param Key=""Name"" Value=""LRU2QStorage"" />
          <Param Key=""Capacity"" Value=""1024"" />
        </PoolItem>
      </Pool>
    </ClientOptions>    
  </Cache>
</CacheSettings>
";
    }
}