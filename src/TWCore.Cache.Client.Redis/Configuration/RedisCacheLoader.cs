/*
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
using System.Linq;
using TWCore.Collections;

namespace TWCore.Cache.Client.Redis.Configuration
{
    /// <inheritdoc />
    /// <summary>
    /// IStorageAsync Loader
    /// </summary>
    public class RedisCacheLoader : IStorageAsyncLoader
    {
        /// <summary>
        /// Get Storage
        /// </summary>
        /// <param name="name">Cache Name</param>
        /// <param name="parameters">Storage Parameters</param>
        /// <returns>Storage instance</returns>
        public IStorageAsync GetStorage(string name, KeyValueCollection parameters)
        {
            var connectionString = parameters?.FirstOrDefault(p => p.Key == "ConnectionString")?.Value;
            if (connectionString == null)
                throw new ArgumentNullException("ConnectionString", "The RedisCache doesn't have the ConnectionString parameter");
            return new RedisCacheClient(connectionString, name);
        }
    }
}
