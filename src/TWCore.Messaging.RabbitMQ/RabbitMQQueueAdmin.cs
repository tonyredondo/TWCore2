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

using TWCore.Messaging.Configuration;

namespace TWCore.Messaging.RabbitMQ
{
    /// <summary>
    /// RabbitMQ administration class
    /// </summary>
    public class RabbitMQQueueAdmin : IMQueueAdmin
    {
        /// <summary>
        /// Create a new message queue
        /// </summary>
        /// <param name="queue">Message Queue connection instance</param>
        /// <returns>true if the message queue was created; otherwise, false.</returns>
        public bool Create(MQConnection queue)
        {
            if (!Exist(queue))
            {
                var rabbitQueue = new RabbitQueue(queue);
                return rabbitQueue.EnsureConnection(true);
            }
            return false;
        }

        /// <summary>
        /// Delete a message queue
        /// </summary>
        /// <param name="queue">Message Queue connection instance</param>
        /// <returns>true if the message queue was deleted; otherwise, false.</returns>
        public bool Delete(MQConnection queue)
        {
            var rabbitQueue = new RabbitQueue(queue);
            rabbitQueue.EnsureConnection(false);
            rabbitQueue.Channel.QueueDelete(queue.Name, false, false);
            return true;
        }

        /// <summary>
        /// Check if the message queue exists
        /// </summary>
        /// <param name="queue">Message Queue connection instance</param>
        /// <returns>true if the message queue exists; otherwise, false.</returns>
        public bool Exist(MQConnection queue)
        {
            var rabbitQueue = new RabbitQueue(queue);
            rabbitQueue.EnsureConnection(false);
            try
            {
                var result = rabbitQueue.Channel.QueueDeclarePassive(queue.Name);
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Purge all messages from a message queue
        /// </summary>
        /// <param name="queue">Message Queue connection instance</param>
        public void Purge(MQConnection queue)
        {
            if (Exist(queue))
            {
                var rabbitQueue = new RabbitQueue(queue);
                rabbitQueue.EnsureConnection(false);
                rabbitQueue.Channel.QueuePurge(queue.Name);
            }
        }
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
