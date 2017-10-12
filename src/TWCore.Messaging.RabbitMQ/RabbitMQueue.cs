/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

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
using System.Runtime.CompilerServices;
using RabbitMQ.Client;
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
        private readonly Action _autoCloseAction;

        #region Properties
        /// <summary>
        /// Connection Factory
        /// </summary>
        public ConnectionFactory Factory { get; set; }
        /// <summary>
        /// Connection
        /// </summary>
        public IConnection Connection { get; private set; }
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
            _autoCloseAction = ActionDelegate.Create(Close).CreateBufferedAction(3000);
        }
        ~RabbitMQueue()
        {
            Close();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Ensure Connection
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EnsureConnection()
        {
            try
            {
                lock (this)
                {
                    Core.Log.LibVerbose("Creating channel for: {0}", Name);
                    if (Channel != null) return true;
                    Factory = new ConnectionFactory { Uri = new Uri(Route) };
                    Connection = Factory.CreateConnection();
                    Channel = Connection.CreateModel();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                return false;
            }
        }
        /// <summary>
        /// Ensure Queue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureQueue()
        {
            if (!string.IsNullOrEmpty(Name))
                Channel.QueueDeclare(Name, true, false, false, null);
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
                if (Channel == null) return;
                if (Channel.IsOpen)
                    Channel.Close();
                if (Connection.IsOpen)
                    Connection.Close();
                Connection = null;
                Channel = null;
                Factory = null;
                Core.Log.LibVerbose("Closing channel for: {0}", Name);
            }
        }
        /// <summary>
        /// Autoclose connection when inactive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AutoClose()
        {
            _autoCloseAction();
        }
        public void Dispose()
        {
            Close();
        }
        #endregion
    }
}
