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

using RabbitMQ.Client;
using System;
using TWCore.Messaging.Configuration;

namespace TWCore.Messaging.RabbitMQ
{
    /// <summary>
    /// RabbitMQ Queue
    /// </summary>
    internal class RabbitQueue : MQConnection
    {
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
        #endregion

        #region .ctor
        /// <summary>
        /// RabbitMQ Queue
        /// </summary>
        /// <param name="queue">MQConnection instance</param>
        public RabbitQueue(MQConnection queue)
        {
            Route = queue.Route;
            Name = queue.Name;
            Parameters = queue.Parameters;
            Factory = new ConnectionFactory { Uri = new Uri(Route) };
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Ensure Connection
        /// </summary>
        public bool EnsureConnection(bool declareQueue)
        {
            try
            {
                if (Connection == null)
                {
                    Connection = Factory.CreateConnection();
                    Channel = Connection.CreateModel();
                }
                if (Channel == null)
                    Channel = Connection.CreateModel();
                if (declareQueue)
                    Channel.QueueDeclare(Name, true, false, false, null);
                return true;
            }
            catch (Exception ex)
            {
                Core.Log.Write(ex);
                return false;
            }
        }
        /// <summary>
        /// Close Connection
        /// </summary>
        public void Close()
        {
            if (Channel?.IsOpen == true)
                Channel.Close();
            if (Connection?.IsOpen == true)
                Connection.Close();
            Connection = null;
            Channel = null;
        }
        #endregion
    }
}
