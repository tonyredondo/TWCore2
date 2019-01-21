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

using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using TWCore.Messaging.Configuration;
using TWCore.Threading;

// ReSharper disable NotAccessedField.Local
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.Redis
{
    /// <summary>
    /// Redis queue connection
    /// </summary>
    public class RedisMQConnection : MQConnection
    {
        private static readonly AsyncLock _creationLock = new AsyncLock();
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> Multiplexers = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        private ISubscriber _subscriber;

        /// <summary>
        /// Redis queue connection
        /// </summary>
        /// <param name="connection">Base connection</param>
        public RedisMQConnection(MQConnection connection)
        {
            if (string.IsNullOrEmpty(connection.Route))
                throw new UriFormatException($"The route for the connection to {connection.Name} is null.");
            Name = connection.Name;
            Parameters = connection.Parameters;
            Route = connection.Route;
        }

        /// <summary>
        /// Get Subscriber
        /// </summary>
        /// <returns>Subscriber instance</returns>
        public async Task<ISubscriber> GetSubscriberAsync()
        {
            if (!Multiplexers.TryGetValue(Route, out var connection))
            {
                using (await _creationLock.LockAsync().ConfigureAwait(false))
                {
                    if (!Multiplexers.TryGetValue(Route, out connection))
                    {
                        connection = await Extensions.InvokeWithRetry(() => ConnectionMultiplexer.ConnectAsync(Route), conn => !conn.IsConnected, 5000, int.MaxValue).ConfigureAwait(false);
                        Multiplexers.TryAdd(Route, connection);
                    }
                }
            }
            return connection.GetSubscriber();
        }

        /// <summary>
        /// Subscribe to the queue channel
        /// </summary>
        /// <param name="handler">Handler delegate to process messages</param>
        /// <returns>Subscribe task</returns>
        public async Task SubscribeAsync(Action<RedisChannel, RedisValue> handler)
        {
            if (!(_subscriber is null))
            {
                await _subscriber.UnsubscribeAllAsync().ConfigureAwait(false);
                _subscriber = null;
            }
            _subscriber = await GetSubscriberAsync().ConfigureAwait(false);
            await _subscriber.SubscribeAsync(Name, handler).ConfigureAwait(false);
        }
        /// <summary>
        /// Unsubscribe all handlers
        /// </summary>
        /// <returns>Unsubscribe task</returns>
        public async Task UnsubscribeAsync()
        {
            if (!(_subscriber is null))
            {
                await _subscriber.UnsubscribeAsync(Name).ConfigureAwait(false);
                _subscriber = null;
            }
        }
    }
}
