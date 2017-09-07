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

using NsqSharp.Api;
using System;
using TWCore.Messaging.Configuration;
// ReSharper disable InconsistentNaming

namespace TWCore.Messaging.NSQ
{
    /// <inheritdoc />
    /// <summary>
    /// NSQ administration class
    /// </summary>
    public class NSQueueAdmin : IMQueueAdmin
    {
        /// <inheritdoc />
        /// <summary>
        /// Create a new message queue
        /// </summary>
        /// <param name="queue">Message Queue connection instance</param>
        /// <returns>true if the message queue was created; otherwise, false.</returns>
        public bool Create(MQConnection queue)
        {
            if (Exist(queue)) return false;
            var client = new NsqdHttpClient(queue.Route.Replace(":4150", ":4151"), TimeSpan.FromSeconds(60));
            client.CreateTopic(queue.Name);
            client.CreateChannel(queue.Name, queue.Name);
            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Delete a message queue
        /// </summary>
        /// <param name="queue">Message Queue connection instance</param>
        /// <returns>true if the message queue was deleted; otherwise, false.</returns>
        public bool Delete(MQConnection queue)
        {
            var client = new NsqdHttpClient(queue.Route.Replace(":4150", ":4151"), TimeSpan.FromSeconds(60));
            client.DeleteChannel(queue.Name, queue.Name);
            client.DeleteTopic(queue.Name);
            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Check if the message queue exists
        /// </summary>
        /// <param name="queue">Message Queue connection instance</param>
        /// <returns>true if the message queue exists; otherwise, false.</returns>
        public bool Exist(MQConnection queue)
        {
            return false;
        }

        /// <inheritdoc />
        /// <summary>
        /// Purge all messages from a message queue
        /// </summary>
        /// <param name="queue">Message Queue connection instance</param>
        public void Purge(MQConnection queue)
        {
            if (!Exist(queue)) return;
            var client = new NsqdHttpClient(queue.Route.Replace(":4150", ":4151"), TimeSpan.FromSeconds(60));
            client.EmptyChannel(queue.Name, queue.Name);
            client.EmptyTopic(queue.Name);
        }
        /// <inheritdoc />
        /// <summary>
        /// Set permission for a user in a queue
        /// </summary>
        /// <param name="queue">Message Queue connection instance</param>
        /// <param name="user">User to set the permissions</param>
        /// <param name="permission">Permission to set</param>
        public void SetPermission(MQConnection queue, string user, MQAccessRights permission)
        {
        }

    }
}
