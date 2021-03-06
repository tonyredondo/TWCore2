﻿/*
Copyright 2015-2018 Daniel Adrian Redondo Suarez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

using System;
using System.Threading.Tasks;
using TWCore.Cache.Client;
using TWCore.Cache.Client.Redis;
using TWCore.Net.RPC.Client.Transports.Default;
using TWCore.Serialization;
using TWCore.Serialization.NSerializer;
using TWCore.Services;
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    public class RedisCacheClientTestAsync : ContainerParameterServiceAsync
    {
        private static ISerializer GlobalSerializer = new NBinarySerializer();


        public RedisCacheClientTestAsync() : base("rediscacheclienttest", "Redis Cache Client test")
        {
        }

        protected override async Task OnHandlerAsync(ParameterHandlerInfo info)
        {
            for (var i = 0; i < 100; i++)
            {
                Core.Log.Error("NUMBER: {0}", i);
                await Task.WhenAll(
                    CacheSingleTest(),
                    CacheSingleTest()
                ).ConfigureAwait(false);
                await Task.Delay(2000).ConfigureAwait(false);
            }
        }

        private static async Task CacheSingleTest()
        {
            using (var cachePool = new CacheClientPoolAsync("Pool Test") { Serializer = GlobalSerializer })
            {
                try
                {
                    var cacheClient = new RedisCacheClient("localhost", "test") { DisableNotSupportedExceptions = true };
                    cachePool.Add("test", cacheClient, StorageItemMode.ReadAndWrite);

                    for (var i = 0; i < 15; i++)
                        await cachePool.GetKeysAsync().ConfigureAwait(false);

                    for (var i = 0; i < 5000; i++)
                    {
                        var key = "test-" + i;
                        var kv = await cachePool.GetAsync(key).ConfigureAwait(false);
                        var kv2 = await cachePool.SetAsync(key, "bla bla bla bla bla bla").ConfigureAwait(false);
                    }
                    Core.Log.InfoBasic("Done.");
                }
                catch (Exception ex)
                {
                    Core.Log.Write(ex);
                }
            }
        }
    }
}