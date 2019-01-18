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

using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TWCore.Collections;
using TWCore.Messaging.Configuration;
// ReSharper disable InheritdocConsiderUsage
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TWCore.Messaging.RabbitMQ
{
    /// <summary>
    /// RabbitMQ Queue
    /// </summary>
    internal class RabbitMQueue : MQConnection, IDisposable
    {
        private static readonly ConcurrentDictionary<string, IConnection> CurrentConnections = new ConcurrentDictionary<string, IConnection>();
        private readonly Action _internalConnection;

        #region Properties
        /// <summary>
        /// Channel
        /// </summary>
        public IModel Channel { get; private set; }
        /// <summary>
        /// Exchange Name
        /// </summary>
        public string ExchangeName { get; }
        /// <summary>
        /// Exchange Type
        /// </summary>
        public string ExchangeType { get; }
        /// <summary>
        /// Durable queue
        /// </summary>
        public bool Durable { get; }
        #endregion

        #region .ctor
        /// <summary>
        /// RabbitMQ Queue
        /// </summary>
        /// <param name="queue">MQConnection instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RabbitMQueue(MQConnection queue)
        {
            Route = queue.Route;
            Name = queue.Name;
            Parameters = queue.Parameters ?? new KeyValueCollection();
            ExchangeName = Parameters[nameof(ExchangeName)];
            ExchangeType = Parameters[nameof(ExchangeType)];
            Durable = Parameters[nameof(Durable)].ParseTo(true);
            _internalConnection = InternalConnection;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~RabbitMQueue()
        {
            Close();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Ensure Connection
        /// </summary>
        /// <param name="retryInterval">Retry interval</param>
        /// <param name="retryCount">Retry count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<bool> EnsureConnectionAsync(int retryInterval, int retryCount)
        {
            try
            {
                if (Channel != null) return true;
                if (string.IsNullOrEmpty(Route))
                    throw new UriFormatException($"The route for the connection to {Name} is null.");
                await _internalConnection.InvokeWithRetry(retryInterval, retryCount, "Trying to connect to " + Route).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                return false;
            }
        }

        private void InternalConnection()
        {
            lock (this)
            {
                if (Channel != null) return;
                if (CurrentConnections.TryGetValue(Route, out var connection))
                {
                    if (string.IsNullOrEmpty(Name))
                        Core.Log.InfoDetail("Using existing connection, creating channel for: Route={0}", Route);
                    else
                        Core.Log.InfoDetail("Using existing connection, creating channel for: Route={0}, Name={1}", Route, Name);
                    Channel = connection.CreateModel();
                }
                else
                {
                    Core.Log.InfoDetail("Creating connection for: Route={0}", Route);
                    var factory = new ConnectionFactory
                    {
                        Uri = new Uri(Route),
                        UseBackgroundThreadsForIO = true,
                        AutomaticRecoveryEnabled = true,
                    };
                    connection = factory.CreateConnection();
                    if (string.IsNullOrEmpty(Name))
                        Core.Log.InfoDetail("Creating channel for: Route={0}", Route);
                    else
                        Core.Log.InfoDetail("Creating channel for: Route={0}, Name={1}", Route, Name);
                    Channel = connection.CreateModel();
                    CurrentConnections.TryAdd(Route, connection);
                }
            }
        }

        /// <summary>
        /// Ensure Queue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureQueue()
        {
            if (!string.IsNullOrEmpty(Name))
                Channel.QueueDeclare(Name, Durable, false, false, null);
        }
        /// <summary>
        /// Ensure Queue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureExchange()
        {
            if (!string.IsNullOrEmpty(ExchangeName))
                Channel.ExchangeDeclare(ExchangeName, ExchangeType ?? global::RabbitMQ.Client.ExchangeType.Direct, true, false, null);
        }
        /// <summary>
        /// Close Connection
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Close()
        {
            lock (this)
            {
                if (Channel is null) return;
                if (Channel.IsOpen)
                    Channel.Close();
                Channel = null;
                Core.Log.LibVerbose("Closing channel for: Route={0}, Name={1}", Route, Name);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Close();
        }
        #endregion
    }
}
