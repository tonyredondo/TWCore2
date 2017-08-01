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

namespace TWCore.Messaging
{
    /// <summary>
    /// Interface for Message Queue system administration
    /// </summary>
    public interface IMQueueAdmin
    {
        /// <summary>
        /// Check if the message queue exists
        /// </summary>
        /// <param name="queue">Message Queue connection instance</param>
        /// <returns>true if the message queue exists; otherwise, false.</returns>
        bool Exist(MQConnection queue);
        /// <summary>
        /// Create a new message queue
        /// </summary>
        /// <param name="queue">Message Queue connection instance</param>
        /// <returns>true if the message queue was created; otherwise, false.</returns>
        bool Create(MQConnection queue);
        /// <summary>
        /// Delete a message queue
        /// </summary>
        /// <param name="queue">Message Queue connection instance</param>
        /// <returns>true if the message queue was deleted; otherwise, false.</returns>
        bool Delete(MQConnection queue);
        /// <summary>
        /// Purge all messages from a message queue
        /// </summary>
        /// <param name="queue">Message Queue connection instance</param>
        void Purge(MQConnection queue);
        /// <summary>
        /// Set permission for a user in a queue
        /// </summary>
        /// <param name="queue">Message Queue connection instance</param>
        /// <param name="user">User to set the permissions</param>
        /// <param name="permission">Permission to set</param>
        void SetPermission(MQConnection queue, string user, MQAccessRights permission);
    }
}
