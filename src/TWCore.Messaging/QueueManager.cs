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
using System.Linq;
using TWCore.Messaging.Configuration;

namespace TWCore.Messaging
{
    /// <summary>
    /// Message Queue Manager 
    /// </summary>
    public class QueueManager
    {
        IMQueueAdmin admin;
        MQClientQueues ClientQueues;
        MQServerQueues ServerQueues;

        #region Properties
        /// <summary>
        /// Gets the current configuration
        /// </summary>
        public MQPairConfig Config { get; private set; }
        /// <summary>
        /// Current Security configuration for the queue
        /// </summary>
        public MQSecurity Security { get; private set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Message Queue Manager 
        /// </summary>
        public QueueManager(MQPairConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("The configuration pair must not be null", nameof(config));

            Config = config;
            Security = Config.Security?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true)
                ?? Config.Security?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true)
                ?? Config.Security?.FirstOrDefault(c => c.EnvironmentName.IsNullOrWhitespace());
            ClientQueues = Config.ClientQueues?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true)
                    ?? Config.ClientQueues?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true)
                    ?? Config.ClientQueues?.FirstOrDefault(c => c.EnvironmentName.IsNullOrWhitespace());
            ServerQueues = Config.ServerQueues?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true && c.MachineName?.SplitAndTrim(",").Contains(Core.MachineName) == true)
                    ?? Config.ServerQueues?.FirstOrDefault(c => c.EnvironmentName?.SplitAndTrim(",").Contains(Core.EnvironmentName) == true)
                    ?? Config.ServerQueues?.FirstOrDefault(c => c.EnvironmentName.IsNullOrWhitespace());
            admin = Config.GetAdmin();
        }
        #endregion

        #region Exist Methods
        /// <summary>
        /// Check if the client queues exists
        /// </summary>
        /// <returns>True if all client queues exists, otherwise; false.</returns>
        public bool ExistClientQueues()
        {
            bool response = true;
            if (ClientQueues != null)
            {
                if (ClientQueues.SendQueues!= null)
                    foreach(var queue in ClientQueues.SendQueues)
                        response &= ExistQueue(queue);
                else
                    Core.Log.Warning("There is not client send queues configured.");

                if (ClientQueues.RecvQueue != null)
                    response &= ExistQueue(ClientQueues.RecvQueue);
                else
                    Core.Log.Warning("There is not client receive queue configured.");
            }
            else
                Core.Log.Warning("There is not client queues configured.");

            return response;
        }
        /// <summary>
        /// Check if the server queues exists
        /// </summary>
        /// <returns>True if all server queues exists, otherwise; false.</returns>
        public bool ExistServerQueues()
        {
            bool response = true;
            if (ServerQueues != null)
            {
                if (ServerQueues.RecvQueues != null)
                    foreach (var queue in ServerQueues.RecvQueues)
                        response &= ExistQueue(queue);
                else
                    Core.Log.Warning("There is not server receive queues configured.");

                if (ServerQueues.AdditionalSendQueues != null)
                    foreach (var queue in ServerQueues.AdditionalSendQueues)
                        response &= ExistQueue(queue);
                else
                    Core.Log.Warning("There is not additional send queues configured.");
            }
            else
                Core.Log.Warning("There is not client queues configured.");

            return response;
        }
        /// <summary>
        /// Check if a queue exists
        /// </summary>
        /// <param name="queue">Queue connection definition</param>
        /// <returns>True if the queues exists, otherwise; false.</returns>
        public bool ExistQueue(MQConnection queue)
        {
            var exist = admin.Exist(queue);
            if (!exist)
                Core.Log.Warning("The queue Route={0}, Name={1} doesn't exist.", queue.Route, queue.Name);
            return exist;
        }
        #endregion

        #region Create Methods
        /// <summary>
        /// Create the client queues and set the permissions, if a queue is already created then only try to set permissions.
        /// </summary>
        /// <returns>True if all queues were created, otherwise; false.</returns>
        public bool CreateClientQueues()
        {
            bool response = true;
            if (ClientQueues != null)
            {
                if (ClientQueues.SendQueues != null)
                    foreach (var queue in ClientQueues.SendQueues)
                        response &= CreateQueue(queue);
                else
                    Core.Log.Warning("There is not client send queues configured.");

                if (ClientQueues.RecvQueue != null)
                    response &= CreateQueue(ClientQueues.RecvQueue);
                else
                    Core.Log.Warning("There is not client receive queue configured.");
            }
            else
                Core.Log.Warning("There is not client queues configured.");
            return response;
        }
        /// <summary>
        /// Create the server queues and set the permissions, if a queue is already created then only try to set permissions.
        /// </summary>
        /// <returns>True if all queues were created, otherwise; false.</returns>
        public bool CreateServerQueues()
        {
            bool response = true;
            if (ServerQueues != null)
            {
                if (ServerQueues.RecvQueues != null)
                    foreach (var queue in ServerQueues.RecvQueues)
                        response &= CreateQueue(queue);
                else
                    Core.Log.Warning("There is not server receive queues configured.");

                if (ServerQueues.AdditionalSendQueues != null)
                    foreach (var queue in ServerQueues.AdditionalSendQueues)
                        response &= CreateQueue(queue);
                else
                    Core.Log.Warning("There is not additional send queues configured.");
            }
            else
                Core.Log.Warning("There is not client queues configured.");
            return response;
        }
        /// <summary>
        /// Create a queue and set the permissions, if the queue is already created then only tries to set permissions.
        /// </summary>
        /// <param name="queue">Queue connection definition</param>
        /// <returns>True if the queue was created, otherwise; false.</returns>
        public bool CreateQueue(MQConnection queue)
        {
            var exist = admin.Exist(queue);
            if (!exist)
            {
                try
                {
                    Core.Log.InfoBasic("Creating queue: Route={0}, Name={1}", queue.Route, queue.Name);
                    exist = admin.Create(queue);
                }
                catch (Exception ex)
                {
                    Core.Log.Error(ex, "Error creating the queue: Route={0}, Name={1}", queue.Route, queue.Name);
                }
            }
            if (exist && Security != null && Security.Users?.Any() == true)
            {
                foreach (var user in Security.Users)
                {
                    try
                    {
                        Core.Log.InfoBasic("Settings '{0}' permissions to user '{1}' on queue: Route={2}, Name={3}", user.Permissions, user.Name, queue.Route, queue.Name);
                        var accessRights = user.GetAccessRights();
                        admin.SetPermission(queue, user.Name, accessRights);
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Error(ex, "Error when trying to set permissions to user '{0}' on queue: Route={1}, Name={2}", user.Name, queue.Route, queue.Name);
                    }
                }
            }
            return exist;
        }
        #endregion

        #region Delete Methods
        /// <summary>
        /// Delete the client queues
        /// </summary>
        /// <returns>True if all queues were deleted, otherwise; false.</returns>
        public bool DeleteClientQueues()
        {
            bool response = true;
            if (ClientQueues != null)
            {
                if (ClientQueues.SendQueues != null)
                    foreach (var queue in ClientQueues.SendQueues)
                        response &= DeleteQueue(queue);
                else
                    Core.Log.Warning("There is not client send queues configured.");

                if (ClientQueues.RecvQueue != null)
                    response &= DeleteQueue(ClientQueues.RecvQueue);
                else
                    Core.Log.Warning("There is not client receive queue configured.");
            }
            else
                Core.Log.Warning("There is not client queues configured.");
            return response;
        }
        /// <summary>
        /// Delete the server queues
        /// </summary>
        /// <returns>True if all queues were deleted, otherwise; false.</returns>
        public bool DeleteServerQueues()
        {
            bool response = true;
            if (ServerQueues != null)
            {
                if (ServerQueues.RecvQueues != null)
                    foreach (var queue in ServerQueues.RecvQueues)
                        response &= DeleteQueue(queue);
                else
                    Core.Log.Warning("There is not server receive queues configured.");

                if (ServerQueues.AdditionalSendQueues != null)
                    foreach (var queue in ServerQueues.AdditionalSendQueues)
                        response &= DeleteQueue(queue);
                else
                    Core.Log.Warning("There is not additional send queues configured.");
            }
            else
                Core.Log.Warning("There is not client queues configured.");
            return response;
        }
        /// <summary>
        /// Delete a queue
        /// </summary>
        /// <param name="queue">Queue connection definition</param>
        /// <returns>True if the queue was deleted, otherwise; false.</returns>
        public bool DeleteQueue(MQConnection queue)
        {
            var exist = admin.Exist(queue);
            if (exist)
            {
                try
                {
                    Core.Log.InfoBasic("Deleting queue: Route={0}, Name={1}", queue.Route, queue.Name);
                    return admin.Delete(queue);
                }
                catch (Exception ex)
                {
                    Core.Log.Error(ex, "Error deleting the queue: Route={0}, Name={1}", queue.Route, queue.Name);
                    return false;
                }
            }
            else
                return true;
        }
        #endregion

        #region Purge Methods
        /// <summary>
        /// Purge the client queues
        /// </summary>
        public void PurgeClientQueues()
        {
            if (ClientQueues != null)
            {
                if (ClientQueues.SendQueues != null)
                    foreach (var queue in ClientQueues.SendQueues)
                        PurgeQueue(queue);
                else
                    Core.Log.Warning("There is not client send queues configured.");

                if (ClientQueues.RecvQueue != null)
                    PurgeQueue(ClientQueues.RecvQueue);
                else
                    Core.Log.Warning("There is not client receive queue configured.");
            }
            else
                Core.Log.Warning("There is not client queues configured.");
        }
        /// <summary>
        /// Purge the server queues
        /// </summary>
        public void PurgeServerQueues()
        {
            if (ServerQueues != null)
            {
                if (ServerQueues.RecvQueues != null)
                    foreach (var queue in ServerQueues.RecvQueues)
                        PurgeQueue(queue);
                else
                    Core.Log.Warning("There is not server receive queues configured.");

                if (ServerQueues.AdditionalSendQueues != null)
                    foreach (var queue in ServerQueues.AdditionalSendQueues)
                        PurgeQueue(queue);
                else
                    Core.Log.Warning("There is not additional send queues configured.");
            }
            else
                Core.Log.Warning("There is not client queues configured.");
        }
        /// <summary>
        /// Purge a queue
        /// </summary>
        /// <param name="queue">Queue connection definition</param>
        public void PurgeQueue(MQConnection queue)
        {
            var exist = admin.Exist(queue);
            if (exist)
            {
                try
                {
                    Core.Log.InfoBasic("Purging queue: Route={0}, Name={1}", queue.Route, queue.Name);
                    admin.Purge(queue);
                }
                catch (Exception ex)
                {
                    Core.Log.Error(ex, "Error purging the queue: Route={0}, Name={1}", queue.Route, queue.Name);
                }
            }
        }
        #endregion
    }
}
